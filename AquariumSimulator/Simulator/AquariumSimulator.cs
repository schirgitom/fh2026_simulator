using AquariumSimulator.Configuration;
using AquariumSimulator.Messaging;
using AquariumSimulator.Payload;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AquariumSimulator.Simulator;

public sealed class AquariumSimulator : BackgroundService
{
    private readonly SimulatorOptions _simulatorOptions;
    private readonly AquariumOptions _aquariumOptions;
    private readonly PayloadOptions _payloadOptions;
    private readonly IMqttPublisher _publisher;
    private readonly IPayloadEncoder _encoder;
    private readonly ILogger<AquariumSimulator> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public AquariumSimulator(
        IOptions<SimulatorOptions> simulatorOptions,
        IOptions<AquariumOptions> aquariumOptions,
        IOptions<PayloadOptions> payloadOptions,
        IMqttPublisher publisher,
        IPayloadEncoder encoder,
        ILogger<AquariumSimulator> logger,
        ILoggerFactory loggerFactory)
    {
        _simulatorOptions = simulatorOptions.Value;
        _aquariumOptions = aquariumOptions.Value;
        _payloadOptions = payloadOptions.Value;
        _publisher = publisher;
        _encoder = encoder;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Aquarium simulator started");
        await _publisher.StartAsync(stoppingToken);

        if (_aquariumOptions.AquariumIds.Count == 0)
        {
            _logger.LogWarning("No aquariums configured. Simulator will idle until shutdown");
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        _logger.LogInformation("Starting workers for {AquariumCount} aquariums on topic {Topic}", _aquariumOptions.AquariumIds.Count, _simulatorOptions.MqttTopic);
        var tasks = new List<Task>(_aquariumOptions.AquariumIds.Count);
        foreach (var aquariumId in _aquariumOptions.AquariumIds)
        {
            var workerLogger = _loggerFactory.CreateLogger<AquariumWorker>();
            var topic = $"{_simulatorOptions.MqttTopic.TrimEnd('/')}/{aquariumId:D}";
            var worker = new AquariumWorker(aquariumId, topic, _publisher, _encoder, _payloadOptions, workerLogger);
            tasks.Add(worker.RunAsync(stoppingToken));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }

        _logger.LogInformation("Aquarium simulator stopped");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping aquarium simulator");
        await base.StopAsync(cancellationToken);
        await _publisher.StopAsync(cancellationToken);
    }
}
