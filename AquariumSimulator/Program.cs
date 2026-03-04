using AquariumSimulator.Configuration;
using AquariumSimulator.Messaging;
using AquariumSimulator.Payload;
using AquariumSimulator.Simulator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;


Console.WriteLine("Starting app");

const string ConsoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: ConsoleTemplate)
    .CreateBootstrapLogger();

Console.WriteLine("Logger loaded - starting app");


try
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            Console.WriteLine("Setting Base Path: " +AppContext.BaseDirectory);
            config.SetBasePath(AppContext.BaseDirectory);
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            config.AddEnvironmentVariables();
        })
        .UseSerilog((context, services, loggerConfiguration) =>
        {
            Console.WriteLine("Using Serilog");
            try
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            }
            catch (Exception ex)
            {
                loggerConfiguration
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: ConsoleTemplate);

                Log.Warning(ex, "Failed to initialize configured logger sinks. Falling back to console logging.");
            }
        }, preserveStaticLogger: true)
        .ConfigureServices((context, services) =>
        {
            Console.WriteLine("Configuring services");
            services.Configure<SimulatorOptions>(context.Configuration.GetSection("Simulator"));
            services.Configure<AquariumOptions>(context.Configuration.GetSection("Aquariums"));
            services.Configure<PayloadOptions>(context.Configuration.GetSection("Payload"));
            services.AddSingleton<IMqttPublisher, MqttPublisher>();
            services.AddSingleton<IPayloadEncoder, HexPayloadEncoder>();
            services.AddHostedService<AquariumSimulator.Simulator.AquariumSimulator>();
        })
        .Build();
    Console.WriteLine("Run Host");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aquarium simulator terminated unexpectedly");
    Console.WriteLine("Error: " + ex.ToString());
}
finally
{
    await Log.CloseAndFlushAsync();
}
