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
        var inputFolder = _configuration["InputFolder"]!;
        Directory.CreateDirectory(inputFolder);

        ProcessExistingFiles(inputFolder);

        _watcher = new FileSystemWatcher(inputFolder!, "*.xml");
        _watcher.Created += OnFileCreated;
        _watcher.EnableRaisingEvents = true;

        _logger.LogInformation($"Watching for XML files in: {inputFolder}");
        return Task.CompletedTask;
    }

    private void ProcessExistingFiles(string inputFolder)
    {
        _logger.LogInformation($"Processing existing XML files in: {inputFolder}");

        foreach (var filePath in Directory.GetFiles(inputFolder, "*.xml"))
        {
            try
            {
                // Manually raise the event for each existing file
                OnFileCreated(_watcher, new FileSystemEventArgs(WatcherChangeTypes.Created, inputFolder, Path.GetFileName(filePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing existing file: {filePath}");
            }
        }
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