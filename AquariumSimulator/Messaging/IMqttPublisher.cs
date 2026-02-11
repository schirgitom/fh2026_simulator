namespace AquariumSimulator.Messaging;

public interface IMqttPublisher
{
    Task StartAsync(CancellationToken cancellationToken);
    Task PublishAsync(string topic, string payload, CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
