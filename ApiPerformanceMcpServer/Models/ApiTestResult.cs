using System.Text.Json.Serialization;

namespace ApiPerformanceMcpServer.Models;

public sealed class ApiTestResult
{
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("statusCodes")]
    public IReadOnlyList<int> StatusCodes { get; init; } = [];

    [JsonPropertyName("successCount")]
    public int SuccessCount { get; init; }

    [JsonPropertyName("failureCount")]
    public int FailureCount { get; init; }

    [JsonPropertyName("averageResponseMs")]
    public double AverageResponseMs { get; init; }

    [JsonPropertyName("minResponseMs")]
    public double MinResponseMs { get; init; }

    [JsonPropertyName("maxResponseMs")]
    public double MaxResponseMs { get; init; }

    [JsonPropertyName("totalElapsedMs")]
    public double TotalElapsedMs { get; init; }

    [JsonPropertyName("errors")]
    public IReadOnlyList<string> Errors { get; init; } = [];
}
