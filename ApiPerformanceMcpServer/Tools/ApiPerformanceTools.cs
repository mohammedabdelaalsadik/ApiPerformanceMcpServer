using ApiPerformanceMcpServer.Models;
using ApiPerformanceMcpServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ApiPerformanceMcpServer.Tools;

[McpServerToolType]
public sealed class ApiPerformanceTools(ApiPerformanceService service)
{
    [McpServerTool(Name = "test_api", Title = "Test API Performance", ReadOnly = true, Destructive = false, UseStructuredContent = true)]
    [Description("Tests one HTTP API endpoint multiple times and returns status codes and response time statistics.")]
    public Task<ApiTestResult> TestApiAsync(
        [Description("Absolute HTTP or HTTPS URL to test.")] string url,
        [Description("HTTP method to use. Supported values: GET or POST.")] string method = "GET",
        [Description("Optional request headers.")] Dictionary<string, string>? headers = null,
        [Description("Optional request body for POST requests. Treated as JSON text.")] string? body = null,
        [Description("Number of times to call the API. Defaults to 5 when omitted or less than 1.")] int iterations = 5,
        CancellationToken cancellationToken = default)
    {
        return service.TestApiAsync(new ApiTestRequest
        {
            Url = url,
            Method = method,
            Headers = headers,
            Body = body,
            Iterations = iterations
        }, cancellationToken);
    }

    [McpServerTool(Name = "compare_apis", Title = "Compare API Performance", ReadOnly = true, Destructive = false, UseStructuredContent = true)]
    [Description("Tests two HTTP API endpoints multiple times and recommends the faster API based on average response time.")]
    public Task<ApiCompareResult> CompareApisAsync(
        [Description("Absolute HTTP or HTTPS URL for the first API.")] string api1Url,
        [Description("Absolute HTTP or HTTPS URL for the second API.")] string api2Url,
        [Description("HTTP method to use. Supported values: GET or POST.")] string method = "GET",
        [Description("Number of times to call each API. Defaults to 5 when omitted or less than 1.")] int iterations = 5,
        [Description("Optional request headers sent to both APIs.")] Dictionary<string, string>? headers = null,
        [Description("Optional request body for POST requests. Sent to both APIs as JSON text.")] string? body = null,
        CancellationToken cancellationToken = default)
    {
        return service.CompareApisAsync(new ApiCompareRequest
        {
            Api1Url = api1Url,
            Api2Url = api2Url,
            Method = method,
            Iterations = iterations,
            Headers = headers,
            Body = body
        }, cancellationToken);
    }
}
