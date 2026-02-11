namespace AquariumSimulator.Configuration;

public sealed record AquariumOptions
{
    public IReadOnlyList<Guid> AquariumIds { get; init; } = Array.Empty<Guid>();
}
