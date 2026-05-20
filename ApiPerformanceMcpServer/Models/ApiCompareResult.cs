using System.Text.Json.Serialization;

namespace ApiPerformanceMcpServer.Models;

public sealed class ApiCompareResult
{
    [JsonPropertyName("api1")]
    public required ApiTestResult Api1 { get; init; }

    [JsonPropertyName("api2")]
    public required ApiTestResult Api2 { get; init; }

    [JsonPropertyName("fasterApi")]
    public required string FasterApi { get; init; }

    [JsonPropertyName("differenceMs")]
    public double DifferenceMs { get; init; }

    [JsonPropertyName("recommendation")]
    public required string Recommendation { get; init; }
}
