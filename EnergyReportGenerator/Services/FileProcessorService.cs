namespace EnergyReportGenerator.Services;

public interface IFileProcessorService
{
    Task StartAsync(CancellationToken cancellationToken);
    void Stop(CancellationToken cancellationToken);
    Task MultiprocessingStartAsync(CancellationToken cancellationToken);
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

        Directory.CreateDirectory(_configuration["InputFolder"]!);
        Directory.CreateDirectory(_configuration["OutputFolder"]!);
        Directory.CreateDirectory(_configuration["Processed"]!);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var inputFolder = _configuration["InputFolder"]!;

        _watcher = new FileSystemWatcher(inputFolder!, "*.xml");
        _watcher.Created += OnFileCreated;
        _watcher.EnableRaisingEvents = true;

        await ProcessExistingFilesAsync(inputFolder);
        _logger.LogInformation($"Watching for XML files in: {inputFolder}");
    }

    public async Task MultiprocessingStartAsync(CancellationToken cancellationToken)
    {
        var inputFolder = _configuration["InputFolder"]!;

        _watcher = new FileSystemWatcher(inputFolder, "*.xml");
        _watcher.Created += OnFileCreated;
        _watcher.EnableRaisingEvents = true;

        await ProcessExistingFilesParallelAsync(inputFolder, cancellationToken);
        _logger.LogInformation($"Watching for XML files in: {inputFolder} with multiprocessing");
    }

    private async Task ProcessExistingFilesParallelAsync(string inputFolder, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Processing existing XML files in: {inputFolder}");

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(Directory.GetFiles(inputFolder, "*.xml"), options, async (filePath, ct) =>
        {
            try
            {
                await ProcessFileAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing existing file: {filePath}");
            }
        });

        _logger.LogInformation($"Finished processing all existing files.");
    }
        
    public void Stop(CancellationToken cancellationToken)
    {
        if(_watcher != null)
        {
            _watcher!.EnableRaisingEvents = false;
            _watcher.Dispose();
            _logger.LogInformation("Stopped watching for XML files.");
        }

        _logger.LogInformation("FileProcessorService stopped");
    }


    private async Task ProcessExistingFilesAsync(string inputFolder)
    {
        _logger.LogInformation($"Processing existing XML files in: {inputFolder}");

        var tasks = Directory.GetFiles(inputFolder, "*.xml").Select(ProcessFileAsync);

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
            var inputFilename = Path.GetFileNameWithoutExtension(filePath);
            var outputFilePath = Path.Combine(_configuration["OutputFolder"]!, $"{inputFilename}_GenerationOutput_{DateTime.Now:yyyyMMddHHmmss}.xml");
            await _xmlService.SerializeGenerationOutputAsync(generationOutput, outputFilePath);

            _logger.LogInformation($"Processed and saved output to: {outputFilePath}");

            File.Move(filePath, Path.Combine(_configuration["Processed"]!, inputFilename + ".xml") );

            _logger.LogInformation($"Processed file moved to Processed Folder : {inputFilename}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing file: {filePath}");
        }
    }
}