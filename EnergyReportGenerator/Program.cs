namespace EnergyReportGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var fileProcessorService = host.Services.GetRequiredService<IFileProcessorService>();
        await fileProcessorService.StartAsync(host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

        await host.RunAsync();
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
                services.AddLogging(configure => configure.AddConsole());
                services.AddSingleton<IXmlService, XmlService>();
                services.AddSingleton<IGenerationCalculatorService, GenerationCalculatorService>();
                services.AddSingleton<IFileProcessorService, FileProcessorService>();

                var configuration = hostContext.Configuration;
                var referenceDataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "ReferenceData.xml");
                configuration["ReferenceDataPath"] = referenceDataPath;
            });
}