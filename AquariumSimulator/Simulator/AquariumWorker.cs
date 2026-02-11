using System.Text.Json;
using AquariumSimulator.Configuration;
using AquariumSimulator.Messaging;
using AquariumSimulator.Models;
using AquariumSimulator.Payload;
using Microsoft.Extensions.Logging;

namespace AquariumSimulator.Simulator;

public sealed class AquariumWorker
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly Guid _aquariumId;
    private readonly string _topic;
    private readonly IMqttPublisher _publisher;
    private readonly IPayloadEncoder _encoder;
    private readonly PayloadOptions _payloadOptions;
    private readonly ILogger<AquariumWorker> _logger;
    private long _fCntUp;

    public AquariumWorker(
        Guid aquariumId,
        string topic,
        IMqttPublisher publisher,
        IPayloadEncoder encoder,
        PayloadOptions payloadOptions,
        ILogger<AquariumWorker> logger)
    {
        _aquariumId = aquariumId;
        _topic = topic;
        _publisher = publisher;
        _encoder = encoder;
        _payloadOptions = payloadOptions;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker started for aquarium {AquariumId}", _aquariumId);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            var payload = GeneratePayload(_payloadOptions.Values);
            var payloadHex = _encoder.Encode(payload);
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var envelope = new MqttEnvelope
            {
                Id = Guid.NewGuid(),
                EndDevice = new EndDevice
                {
                    DevEui = _aquariumId.ToString(),
                    DevAddr = "01611555",
                    Cluster = new Cluster { Id = 2684354603 }
                },
                FPort = 2,
                FCntDown = 0,
                FCntUp = Interlocked.Increment(ref _fCntUp),
                Adr = true,
                Confirmed = false,
                Encrypted = false,
                Payload = payloadHex,
                EncodingType = "HEXA",
                RecvTime = now,
                GwRecvTime = now,
                ClassB = false,
                Delayed = false,
                UlFrequency = 867.1,
                Modulation = "LORA",
                DataRate = "SF10BW125",
                CodingRate = "4/5",
                GwCnt = 2,
                GwInfo = new[]
                {
                    new GatewayInfo
                    {
                        GwEui = "7076FFFFFF034CEC",
                        RfRegion = "",
                        Rssi = -110,
                        Snr = 7,
                        Channel = 0,
                        RadioId = 0,
                        Antenna = 0
                    },
                    new GatewayInfo
                    {
                        GwEui = "7076FFFFFF03B15F",
                        RfRegion = "",
                        Rssi = -117,
                        Snr = -1.8,
                        Channel = 0,
                        RadioId = 0,
                        Antenna = 0
                    }
                }
            };

            var json = JsonSerializer.Serialize(envelope, JsonOptions);
            await _publisher.PublishAsync(_topic, json, cancellationToken);
            _logger.LogInformation("Published message for aquarium {AquariumId} with FCntUp={FCntUp}", _aquariumId, envelope.FCntUp);
        }
    }

    private static Dictionary<string, double> GeneratePayload(IReadOnlyDictionary<string, double> configuredPayload)
    {
        if (configuredPayload.Count > 0)
        {
            return configuredPayload.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        return new Dictionary<string, double>
        {
            ["temperature"] = 24.0 + Random.Shared.NextDouble(),
            ["mg"] = 300 + Random.Shared.NextDouble(),
            ["kh"] = 7.0 + Random.Shared.NextDouble(),
            ["ca"] = 1300.0 + Random.Shared.NextDouble(),
            ["ph"] = 7.0 + Random.Shared.NextDouble() * 0.4,
            ["oxygen"] = 8.0 + Random.Shared.NextDouble(),
            ["pump"] = 1,
        };
    }
}
