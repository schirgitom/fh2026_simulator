using AquariumSimulator.Configuration;
using AquariumSimulator.Messaging;
using AquariumSimulator.Payload;
using AquariumSimulator.Simulator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        config.AddEnvironmentVariables();
    })
    .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext())
    .ConfigureServices((context, services) =>
    {
        services.Configure<SimulatorOptions>(context.Configuration.GetSection("Simulator"));
        services.Configure<AquariumOptions>(context.Configuration.GetSection("Aquariums"));
        services.Configure<PayloadOptions>(context.Configuration.GetSection("Payload"));
        services.AddSingleton<IMqttPublisher, MqttPublisher>();
        services.AddSingleton<IPayloadEncoder, HexPayloadEncoder>();
        services.AddHostedService<AquariumSimulator.Simulator.AquariumSimulator>();
    })
    .Build();

await host.RunAsync();
