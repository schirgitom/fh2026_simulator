using System.Text.Json.Serialization;

namespace AquariumSimulator.Models;

public sealed record Cluster
{
    [JsonPropertyName("id")]
    public long Id { get; init; }
}
