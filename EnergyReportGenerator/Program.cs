namespace EnergyReportGenerator;

public class Program
{
    internal static readonly ActivitySource ActivitySource = new ActivitySource("EnergyReportGenerator");
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting up the application");
            var host = CreateHostBuilder(args).Build();

            var configuration = host.Services.GetRequiredService<IConfiguration>();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            var fileProcessorService = host.Services.GetRequiredService<IFileProcessorService>();
            await fileProcessorService.StartAsync(host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

            await host.RunAsync();
        }
        catch (System.Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton(ActivitySource);
                services.AddSingleton<IXmlService, XmlService>();
                services.AddSingleton<IGenerationCalculatorService, GenerationCalculatorService>();
                services.AddSingleton<IFileProcessorService, FileProcessorService>();

                services.AddOpenTelemetry()
                        .WithTracing(tracerProviderBuilder =>
                        {
                            tracerProviderBuilder
                                // TODO: Add service version and Machine name(instance id) later
                                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(hostContext.HostingEnvironment.ApplicationName))
                                .AddSource(ActivitySource.Name)
                                .AddOtlpExporter(options =>
                                {
                                    options.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
                                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                                });
                        })
                        .WithMetrics(meterProviderBuilder =>
                        {
                            meterProviderBuilder
                                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(hostContext.HostingEnvironment.ApplicationName))
                                .AddRuntimeInstrumentation()
                                .AddOtlpExporter(options =>
                                {
                                    options.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/metrics");
                                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                                });
                        });

                var configuration = hostContext.Configuration;
                var referenceDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "ReferenceData.xml");
                configuration["ReferenceDataPath"] = referenceDataPath;
            })
            .UseSerilog();
}