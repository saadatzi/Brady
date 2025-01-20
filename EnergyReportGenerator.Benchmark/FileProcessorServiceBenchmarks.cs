namespace EnergyReportGenerator.Benchmark;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class FileProcessorServiceBenchmarks
{
    private string? InputFolder;
    private string? OutputFolder;
    private string? ProcessedFolder;
    private string? ReferenceDataPath;
    private const string SampleFileSetting = "SampleGenerationReportFile";
    private const int FileCount = 1000;
    private string? _sampleFilePath;
    private ServiceProvider? _serviceProvider;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _sampleFilePath = configuration[SampleFileSetting];
        if (string.IsNullOrEmpty(_sampleFilePath) || !File.Exists(_sampleFilePath))
        {
            throw new FileNotFoundException("Sample generation report file not found.", _sampleFilePath);
        }

        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<IActivitySource, ActivitySourceWrapper>(sp => new ActivitySourceWrapper("EnergyReportGenerator"));
        services.AddSingleton<ILogger<FileProcessorService>>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<FileProcessorService>());
        services.AddSingleton<ILogger<XmlService>>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<XmlService>());
        services.AddSingleton<ILogger<GenerationCalculatorService>>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<GenerationCalculatorService>());
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IXmlService, XmlService>();
        services.AddSingleton<IGenerationCalculatorService, GenerationCalculatorService>();
        services.AddSingleton<IFileProcessorService, FileProcessorService>();

        _serviceProvider = services.BuildServiceProvider();

        InputFolder = configuration["InputFolder"]!;
        OutputFolder = configuration["OutputFolder"]!;
        ProcessedFolder = configuration["Processed"]!;
        ReferenceDataPath = configuration["ReferenceDataPath"]!;
        
        Directory.CreateDirectory(InputFolder);
        Directory.CreateDirectory(OutputFolder);
        Directory.CreateDirectory(ProcessedFolder);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        CleanUpBenchmarkFolders();
        CopySampleFiles(FileCount);
    }

    [Benchmark]
    public async Task StartAsyncBenchmark()
    {
        var fileProcessorService = _serviceProvider!.GetRequiredService<IFileProcessorService>();
        var cancellationTokenSource = new CancellationTokenSource();

        await fileProcessorService.StartAsync(cancellationTokenSource.Token);
        fileProcessorService.Stop(cancellationTokenSource.Token);
    }

    [Benchmark]
    public async Task MultiprocessingStartAsyncBenchmark()
    {
        var fileProcessorService = _serviceProvider!.GetRequiredService<IFileProcessorService>();
        var cancellationTokenSource = new CancellationTokenSource();

        await fileProcessorService.MultiprocessingStartAsync(cancellationTokenSource.Token);
        fileProcessorService.Stop(cancellationTokenSource.Token);
    }

    private void CopySampleFiles(int count)
    {
        for (int i = 0; i < count; i++)
        {
            string newFileName = $"GenerationReport_{i}.xml";
            string destPath = Path.Combine(InputFolder!, newFileName);
            File.Copy(_sampleFilePath!, destPath, true);
        }
    }

    private void CleanUpBenchmarkFolders()
    {
        foreach (var file in Directory.GetFiles(InputFolder!))
        {
            File.Delete(file);
        }

        foreach (var file in Directory.GetFiles(OutputFolder!))
        {
            File.Delete(file);
        }

        foreach (var file in Directory.GetFiles(ProcessedFolder!))
        {
            File.Delete(file);
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        try
        {
            if (Directory.Exists(InputFolder)) Directory.Delete(InputFolder, true);
            if (Directory.Exists(OutputFolder)) Directory.Delete(OutputFolder, true);
            if (Directory.Exists(ProcessedFolder)) Directory.Delete(ProcessedFolder, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during cleanup: {ex.Message}");
        }
    }
}