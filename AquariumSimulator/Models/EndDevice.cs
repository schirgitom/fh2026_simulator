using System.Text.Json.Serialization;

namespace AquariumSimulator.Models;

public sealed record EndDevice
{
    [JsonPropertyName("devEui")]
    public string DevEui { get; init; } = string.Empty;

    [JsonPropertyName("devAddr")]
    public string DevAddr { get; init; } = string.Empty;

    [JsonPropertyName("cluster")]
    public Cluster Cluster { get; init; } = new();
}
