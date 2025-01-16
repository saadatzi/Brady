using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnergyReportGenerator.Services;

public interface IFileProcessorService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
public class FileProcessorService : IFileProcessorService
{
    private readonly ILogger<FileProcessorService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IXmlService _xmlService;
    private readonly IGenerationCalculatorService _calculatorService;
    private FileSystemWatcher? _watcher;

    public FileProcessorService(
        ILogger<FileProcessorService> logger,
        IConfiguration configuration,
        IXmlService xmlService,
        IGenerationCalculatorService calculatorService)
    {
        _logger = logger;
        _configuration = configuration;
        _xmlService = xmlService;
        _calculatorService = calculatorService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var inputFolder = Path.Combine(baseDirectory, _configuration["InputFolder"]!);
        Directory.CreateDirectory(inputFolder);

        _watcher = new FileSystemWatcher(inputFolder!, "*.xml");
        _watcher.Created += OnFileCreated;
        _watcher.EnableRaisingEvents = true;

        _logger.LogInformation($"Watching for XML files in: {inputFolder}");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher!.EnableRaisingEvents = false;
        _watcher.Dispose();
        _logger.LogInformation("Stopped watching for XML files.");
        return Task.CompletedTask;
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"New file created: {e.FullPath}");

        try
        {
            // Deserialize the GenerationReport XML
            var generationReport = await _xmlService.DeserializeGenerationReportAsync(e.FullPath);

            // Load reference data
            var referenceData = await _xmlService.DeserializeReferenceDataAsync(_configuration["ReferenceDataPath"]!);

            // Calculate the required values
            var generationOutput = _calculatorService.Calculate(generationReport, referenceData);

            // Serialize the GenerationOutput to XML
            var outputFilePath = Path.Combine(_configuration["OutputFolder"]!, $"GenerationOutput_{DateTime.Now:yyyyMMddHHmmss}.xml");
            await _xmlService.SerializeGenerationOutputAsync(generationOutput, outputFilePath);

            _logger.LogInformation($"Processed and saved output to: {outputFilePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing file: {e.FullPath}");
        }
    }
}