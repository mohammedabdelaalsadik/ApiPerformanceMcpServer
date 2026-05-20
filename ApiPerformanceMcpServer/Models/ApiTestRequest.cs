using System.Text.Json.Serialization;

namespace ApiPerformanceMcpServer.Models;

public sealed class ApiTestRequest
{
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("method")]
    public string Method { get; init; } = "GET";

    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; init; }

    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("iterations")]
    public int Iterations { get; init; } = 5;
}
