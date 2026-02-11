using AquariumSimulator.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;

namespace AquariumSimulator.Messaging;

public sealed class MqttPublisher : IMqttPublisher, IAsyncDisposable
{
    private readonly SimulatorOptions _options;
    private readonly ILogger<MqttPublisher> _logger;
    private readonly IMqttClient _client;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private CancellationTokenSource? _reconnectCts;
    private int _started;

    public MqttPublisher(IOptions<SimulatorOptions> options, ILogger<MqttPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new MqttClientFactory().CreateMqttClient();
        _client.DisconnectedAsync += OnDisconnectedAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
        {
            return;
        }

        _reconnectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _logger.LogInformation("Starting MQTT publisher for {Host}:{Port}", _options.MqttHost, _options.MqttPort);
        await EnsureConnectedAsync(_reconnectCts.Token);
    }

    public async Task PublishAsync(string topic, string payload, CancellationToken cancellationToken)
    {
        await EnsureConnectedAsync(cancellationToken);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithPayload(payload)
            .Build();

        await _client.PublishAsync(message, cancellationToken);
        _logger.LogDebug("Published MQTT message to topic {Topic}", topic);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _started, 0) == 0)
        {
            return;
        }

        _reconnectCts?.Cancel();
        if (_client.IsConnected)
        {
            _logger.LogInformation("Disconnecting MQTT client");
            await _client.DisconnectAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _reconnectCts?.Cancel();
        if (_client.IsConnected)
        {
            _logger.LogInformation("Disposing connected MQTT client");
            await _client.DisconnectAsync();
        }

        _client.Dispose();
        _connectLock.Dispose();
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_client.IsConnected)
        {
            return;
        }

        await _connectLock.WaitAsync(cancellationToken);
        try
        {
            if (_client.IsConnected)
            {
                return;
            }

            var clientId = string.IsNullOrWhiteSpace(_options.ClientId)
                ? $"aquarium-simulator-{Guid.NewGuid():N}"
                : _options.ClientId;

            var clientOptions = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(_options.MqttHost, _options.MqttPort)
                .WithCleanSession()
                .Build();

            while (!_client.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Connecting to MQTT broker {Host}:{Port} with client id {ClientId}", _options.MqttHost, _options.MqttPort, clientId);
                    await _client.ConnectAsync(clientOptions, cancellationToken);
                    _logger.LogInformation("MQTT connection established");
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "MQTT connection failed, retrying in 5 seconds");
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        if (_reconnectCts is null || _reconnectCts.IsCancellationRequested)
        {
            return;
        }

        try
        {
            _logger.LogWarning("MQTT client disconnected, attempting reconnect");
            await Task.Delay(TimeSpan.FromSeconds(5), _reconnectCts.Token);
            await EnsureConnectedAsync(_reconnectCts.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
