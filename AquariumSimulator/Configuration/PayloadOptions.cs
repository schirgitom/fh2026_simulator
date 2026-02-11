namespace AquariumSimulator.Configuration;

public sealed record PayloadOptions
{
    public Dictionary<string, double> Values { get; init; } = new();
}
