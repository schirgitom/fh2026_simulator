using System.Text.Json.Serialization;

namespace AquariumSimulator.Models;

public sealed record GatewayInfo
{
    [JsonPropertyName("gwEui")]
    public string GwEui { get; init; } = string.Empty;

    [JsonPropertyName("rfRegion")]
    public string RfRegion { get; init; } = string.Empty;

    [JsonPropertyName("rssi")]
    public int Rssi { get; init; }

    [JsonPropertyName("snr")]
    public double Snr { get; init; }

    [JsonPropertyName("channel")]
    public int Channel { get; init; }

    [JsonPropertyName("radioId")]
    public int RadioId { get; init; }

    [JsonPropertyName("antenna")]
    public int Antenna { get; init; }
}
