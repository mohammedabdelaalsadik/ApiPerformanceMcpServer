using System.Text.Json.Serialization;

namespace ApiPerformanceMcpServer.Models;

public sealed class ApiCompareRequest
{
    [JsonPropertyName("api1Url")]
    public required string Api1Url { get; init; }

    [JsonPropertyName("api2Url")]
    public required string Api2Url { get; init; }

    [JsonPropertyName("method")]
    public string Method { get; init; } = "GET";

    [JsonPropertyName("iterations")]
    public int Iterations { get; init; } = 5;

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }
}
