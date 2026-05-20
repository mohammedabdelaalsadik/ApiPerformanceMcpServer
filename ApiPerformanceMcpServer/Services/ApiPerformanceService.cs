using ApiPerformanceMcpServer.Models;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace ApiPerformanceMcpServer.Services;

public sealed class ApiPerformanceService(IHttpClientFactory httpClientFactory)
{
    public const string HttpClientName = "api-performance";

    private static readonly HashSet<string> SupportedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethod.Get.Method,
        HttpMethod.Post.Method
    };

    public async Task<ApiTestResult> TestApiAsync(ApiTestRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var statusCodes = new List<int>();
        var responseTimes = new List<double>();
        var totalStopwatch = Stopwatch.StartNew();

        var iterations = NormalizeIterations(request.Iterations);
        var method = NormalizeMethod(request.Method);

        if (!IsValidAbsoluteHttpUrl(request.Url, out var uri))
        {
            return CreateFailedResult(request.Url, iterations, "Invalid URL. Provide an absolute HTTP or HTTPS URL.");
        }

        if (!SupportedMethods.Contains(method))
        {
            return CreateFailedResult(request.Url, iterations, "Invalid method. Supported methods are GET and POST.");
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        var successCount = 0;
        var failureCount = 0;

        for (var iteration = 1; iteration <= iterations; iteration++)
        {
            using var httpRequest = CreateHttpRequest(uri, method, request.Headers, request.Body);
            var requestStopwatch = Stopwatch.StartNew();

            try
            {
                using var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                requestStopwatch.Stop();

                var elapsedMs = requestStopwatch.Elapsed.TotalMilliseconds;
                responseTimes.Add(elapsedMs);
                statusCodes.Add((int)response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    successCount++;
                }
                else
                {
                    failureCount++;
                    errors.Add($"Iteration {iteration} returned non-success HTTP status {(int)response.StatusCode} ({response.ReasonPhrase}).");
                }
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                requestStopwatch.Stop();
                failureCount++;
                errors.Add($"Iteration {iteration} timed out after 30 seconds. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                requestStopwatch.Stop();
                failureCount++;
                errors.Add($"Iteration {iteration} failed. Exception: {ex.Message}");
            }
        }

        totalStopwatch.Stop();

        return new ApiTestResult
        {
            Url = request.Url,
            StatusCodes = statusCodes,
            SuccessCount = successCount,
            FailureCount = failureCount,
            AverageResponseMs = Round(responseTimes.Count == 0 ? 0 : responseTimes.Average()),
            MinResponseMs = Round(responseTimes.Count == 0 ? 0 : responseTimes.Min()),
            MaxResponseMs = Round(responseTimes.Count == 0 ? 0 : responseTimes.Max()),
            TotalElapsedMs = Round(totalStopwatch.Elapsed.TotalMilliseconds),
            Errors = errors
        };
    }

    public async Task<ApiCompareResult> CompareApisAsync(ApiCompareRequest request, CancellationToken cancellationToken = default)
    {
        var api1 = await TestApiAsync(new ApiTestRequest
        {
            Url = request.Api1Url,
            Method = request.Method,
            Headers = request.Headers,
            Body = request.Body,
            Iterations = request.Iterations
        }, cancellationToken);

        var api2 = await TestApiAsync(new ApiTestRequest
        {
            Url = request.Api2Url,
            Method = request.Method,
            Headers = request.Headers,
            Body = request.Body,
            Iterations = request.Iterations
        }, cancellationToken);

        var api1Average = EffectiveAverage(api1);
        var api2Average = EffectiveAverage(api2);
        var differenceMs = Math.Abs(api1Average - api2Average);
        var fasterApi = GetFasterApi(request.Api1Url, request.Api2Url, api1Average, api2Average);

        return new ApiCompareResult
        {
            Api1 = api1,
            Api2 = api2,
            FasterApi = fasterApi,
            DifferenceMs = double.IsInfinity(differenceMs) ? 0 : Round(differenceMs),
            Recommendation = BuildRecommendation(fasterApi, differenceMs, api1, api2)
        };
    }

    private static HttpRequestMessage CreateHttpRequest(Uri uri, string method, Dictionary<string, string>? headers, string? body)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), uri);

        if (method.Equals(HttpMethod.Post.Method, StringComparison.OrdinalIgnoreCase))
        {
            request.Content = new StringContent(body ?? string.Empty, Encoding.UTF8, "application/json");
        }

        if (headers is null)
        {
            return request;
        }

        foreach (var (name, value) in headers)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(name, value))
            {
                request.Content?.Headers.TryAddWithoutValidation(name, value);
            }
        }

        return request;
    }

    private static bool IsValidAbsoluteHttpUrl(string? url, out Uri uri)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out uri!)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string NormalizeMethod(string? method)
    {
        return string.IsNullOrWhiteSpace(method)
            ? HttpMethod.Get.Method
            : method.Trim().ToUpperInvariant();
    }

    private static int NormalizeIterations(int iterations)
    {
        return iterations <= 0 ? 5 : iterations;
    }

    private static ApiTestResult CreateFailedResult(string url, int iterations, string error)
    {
        return new ApiTestResult
        {
            Url = url,
            FailureCount = iterations,
            Errors = [error]
        };
    }

    private static double EffectiveAverage(ApiTestResult result)
    {
        return result.SuccessCount > 0 ? result.AverageResponseMs : double.PositiveInfinity;
    }

    private static string GetFasterApi(string api1Url, string api2Url, double api1Average, double api2Average)
    {
        if (double.IsInfinity(api1Average) && double.IsInfinity(api2Average))
        {
            return "none";
        }

        if (Math.Abs(api1Average - api2Average) < 0.01)
        {
            return "tie";
        }

        return api1Average < api2Average ? api1Url : api2Url;
    }

    private static string BuildRecommendation(string fasterApi, double differenceMs, ApiTestResult api1, ApiTestResult api2)
    {
        if (fasterApi == "none")
        {
            return "Both APIs failed all successful measurements. Review the errors before comparing performance.";
        }

        if (fasterApi == "tie")
        {
            return "Both APIs performed similarly. Choose based on reliability, payload, and functional requirements.";
        }

        var reliabilityNote = api1.FailureCount == api2.FailureCount
            ? "Both APIs had the same failure count."
            : api1.FailureCount < api2.FailureCount
                ? "API 1 had fewer failures."
                : "API 2 had fewer failures.";

        return string.Create(
            CultureInfo.InvariantCulture,
            $"Use {fasterApi} for lower average latency. It was faster by {Round(differenceMs)} ms. {reliabilityNote}");
    }

    private static double Round(double value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
