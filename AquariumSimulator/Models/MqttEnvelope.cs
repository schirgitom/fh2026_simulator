using System.Text.Json.Serialization;

namespace AquariumSimulator.Models;

public sealed record MqttEnvelope
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("endDevice")]
    public EndDevice EndDevice { get; init; } = new();

    [JsonPropertyName("fPort")]
    public int FPort { get; init; }

    [JsonPropertyName("fCntDown")]
    public int FCntDown { get; init; }

    [JsonPropertyName("fCntUp")]
    public long FCntUp { get; init; }

    [JsonPropertyName("adr")]
    public bool Adr { get; init; }

    [JsonPropertyName("confirmed")]
    public bool Confirmed { get; init; }

    [JsonPropertyName("encrypted")]
    public bool Encrypted { get; init; }

    [JsonPropertyName("payload")]
    public string Payload { get; init; } = string.Empty;

    [JsonPropertyName("encodingType")]
    public string EncodingType { get; init; } = string.Empty;

    [JsonPropertyName("recvTime")]
    public long RecvTime { get; init; }

    [JsonPropertyName("gwRecvTime")]
    public long GwRecvTime { get; init; }

    [JsonPropertyName("classB")]
    public bool ClassB { get; init; }

    [JsonPropertyName("delayed")]
    public bool Delayed { get; init; }

    [JsonPropertyName("ulFrequency")]
    public double UlFrequency { get; init; }

    [JsonPropertyName("modulation")]
    public string Modulation { get; init; } = string.Empty;

    [JsonPropertyName("dataRate")]
    public string DataRate { get; init; } = string.Empty;

    [JsonPropertyName("codingRate")]
    public string CodingRate { get; init; } = string.Empty;

    [JsonPropertyName("gwCnt")]
    public int GwCnt { get; init; }

    [JsonPropertyName("gwInfo")]
    public IReadOnlyList<GatewayInfo> GwInfo { get; init; } = Array.Empty<GatewayInfo>();
}
