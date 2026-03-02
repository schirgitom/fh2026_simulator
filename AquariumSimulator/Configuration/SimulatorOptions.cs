namespace AquariumSimulator.Configuration;

public sealed record SimulatorOptions
{
    public string MqttHost { get; init; } = "localhost";
    public int MqttPort { get; init; } = 1883;
    public string MqttTopic { get; init; } = "aquariums";
    public string ClientId { get; init; } = "aquarium-simulator";
}
