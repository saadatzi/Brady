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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var inputFolder = _configuration["InputFolder"]!;
        Directory.CreateDirectory(inputFolder);

        _watcher = new FileSystemWatcher(inputFolder!, "*.xml");
        _watcher.Created += OnFileCreated;
        _watcher.EnableRaisingEvents = true;

        await ProcessExistingFilesAsync(inputFolder);
        _logger.LogInformation($"Watching for XML files in: {inputFolder}");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher!.EnableRaisingEvents = false;
        _watcher.Dispose();
        _logger.LogInformation("Stopped watching for XML files.");
        return Task.CompletedTask;
    }


    private async Task ProcessExistingFilesAsync(string inputFolder)
    {
        _logger.LogInformation($"Processing existing XML files in: {inputFolder}");

        var tasks = Directory.GetFiles(inputFolder, "*.xml").Select(filePath => ProcessFileAsync(filePath));

        await Task.WhenAll(tasks);

        _logger.LogInformation($"Finished processing all existing files.");
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation($"New file created: {e.FullPath}");
        await ProcessFileAsync(e.FullPath);
    }

    private async Task ProcessFileAsync(string filePath)
    {
        try
        {
            // Deserialize the GenerationReport XML
            var generationReport = await _xmlService.DeserializeGenerationReportAsync(filePath);

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
            _logger.LogError(ex, $"Error processing file: {filePath}");
        }
    }
}