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
    private readonly ActivitySource _activitySource;
    private FileSystemWatcher? _watcher;

    public FileProcessorService(
        ILogger<FileProcessorService> logger,
        IConfiguration configuration,
        IXmlService xmlService,
        IGenerationCalculatorService calculatorService,
        ActivitySource activitySource)
    {
        _logger = logger;
        _configuration = configuration;
        _xmlService = xmlService;
        _calculatorService = calculatorService;
        _activitySource = activitySource;

        Directory.CreateDirectory(_configuration["InputFolder"]!);
        Directory.CreateDirectory(_configuration["OutputFolder"]!);
        Directory.CreateDirectory(_configuration["Processed"]!);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity($"{nameof(FileProcessorService)}.{nameof(StartAsync)}");

        var inputFolder = _configuration["InputFolder"]!;

        _watcher = new FileSystemWatcher(inputFolder!, "*.xml");
        _watcher.Created += OnFileCreated;
        _watcher.EnableRaisingEvents = true;

        await ProcessExistingFilesAsync(inputFolder);
        _logger.LogInformation($"Watching for XML files in: {inputFolder}");
        activity?.AddEvent(new ActivityEvent("Started watching for files."));
    }

    public async Task MultiprocessingStartAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity($"{nameof(FileProcessorService)}.{nameof(MultiprocessingStartAsync)}");
        var inputFolder = _configuration["InputFolder"]!;

        _watcher = new FileSystemWatcher(inputFolder, "*.xml");
        _watcher.Created += OnFileCreated;
        _watcher.EnableRaisingEvents = true;

        await ProcessExistingFilesParallelAsync(inputFolder, cancellationToken);
        _logger.LogInformation($"Watching for XML files in: {inputFolder} with multiprocessing");
        activity?.AddEvent(new ActivityEvent("Started watching for files with multiprocessing."));
    }

    private async Task ProcessExistingFilesParallelAsync(string inputFolder, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity($"{nameof(FileProcessorService)}.{nameof(ProcessExistingFilesParallelAsync)}");
        _logger.LogInformation($"Processing existing XML files in: {inputFolder}");
        activity?.AddEvent(new ActivityEvent("Started processing existing files in parallel."));

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
                activity?.RecordException(ex);
            }
        });

        _logger.LogInformation($"Finished processing all existing files.");
        activity?.AddEvent(new ActivityEvent("Finished processing existing files in parallel."));
    }
        
    public void Stop(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity($"{nameof(FileProcessorService)}.{nameof(Stop)}");
        if(_watcher != null)
        {
            _watcher!.EnableRaisingEvents = false;
            _watcher.Dispose();
            _logger.LogInformation("Stopped watching for XML files.");
            activity?.AddEvent(new ActivityEvent("Stopped watching for files."));
        }

        _logger.LogInformation("FileProcessorService stopped");
        activity?.AddEvent(new ActivityEvent("FileProcessorService stopped."));
    }


    private async Task ProcessExistingFilesAsync(string inputFolder)
    {
        using var activity = _activitySource.StartActivity($"{nameof(FileProcessorService)}.{nameof(ProcessExistingFilesAsync)}");
        _logger.LogInformation($"Processing existing XML files in: {inputFolder}");
        activity?.AddEvent(new ActivityEvent("Started processing existing files."));

        var tasks = Directory.GetFiles(inputFolder, "*.xml").Select(ProcessFileAsync);

        await Task.WhenAll(tasks);

        _logger.LogInformation($"Finished processing all existing files.");
        activity?.AddEvent(new ActivityEvent("Finished processing existing files."));
    }

    private async void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        using (LogContext.PushProperty("FileName", Path.GetFileName(e.FullPath)))
        {
            using var activity = _activitySource.StartActivity($"{nameof(FileProcessorService)}.{nameof(OnFileCreated)}");
            _logger.LogInformation("New file created: {FullPath}", e.FullPath);
            activity?.SetTag("file.path", e.FullPath);
            await ProcessFileAsync(e.FullPath);
        }
    }

    private async Task ProcessFileAsync(string filePath)
    {
        using (LogContext.PushProperty("FileName", Path.GetFileName(filePath)))
        {
            using var activity = _activitySource.StartActivity($"{nameof(FileProcessorService)}.{nameof(ProcessFileAsync)}");
            activity?.SetTag("file.path", filePath);
            _logger.LogInformation("Processing file: {FilePath}", filePath);
            activity?.AddEvent(new ActivityEvent("Started processing file."));
        

            try
            {
                // Deserialize the GenerationReport XML
                var generationReport = new GenerationReport();
                using (var deserializeActivity = _activitySource.StartActivity($"{nameof(IXmlService)}.{nameof(_xmlService.DeserializeGenerationReportAsync)}"))
                {
                    deserializeActivity?.SetTag("file.path", filePath);
                    generationReport = await _xmlService.DeserializeGenerationReportAsync(filePath);
                    deserializeActivity?.SetTag("success", generationReport != null);
                    if (generationReport == null)
                    {
                        deserializeActivity?.SetStatus(ActivityStatusCode.Error, "Failed to deserialize GenerationReport.");
                        return;
                    }
                }

                // Load reference data
                var referenceData = new ReferenceData();
                using (var referenceDataActivity = _activitySource.StartActivity($"{nameof(IXmlService)}.{nameof(_xmlService.DeserializeReferenceDataAsync)}"))
                {
                    var referenceDataPath = _configuration["ReferenceDataPath"]!;
                    referenceDataActivity?.SetTag("file.path", referenceDataPath);
                    referenceData = await _xmlService.DeserializeReferenceDataAsync(referenceDataPath);
                    referenceDataActivity?.SetTag("success", referenceData != null);
                    if (referenceData == null)
                    {
                        referenceDataActivity?.SetStatus(ActivityStatusCode.Error, "Failed to deserialize ReferenceData.");
                        return;
                    }
                }

                // Calculate the required values
                var generationOutput = new GenerationOutput();
                using (var calculateActivity = _activitySource.StartActivity($"{nameof(IGenerationCalculatorService)}.{nameof(_calculatorService.Calculate)}"))
                {
                    generationOutput = _calculatorService.Calculate(generationReport, referenceData);
                }

                // Serialize the GenerationOutput to XML
                string outputFilePath;
                using (var serializeActivity = _activitySource.StartActivity($"{nameof(IXmlService)}.{nameof(_xmlService.SerializeGenerationOutputAsync)}"))
                {
                    var inputFilename = Path.GetFileNameWithoutExtension(filePath);
                    outputFilePath = Path.Combine(_configuration["OutputFolder"]!, $"{inputFilename}_GenerationOutput_{DateTime.Now:yyyyMMddHHmmss}.xml");
                    serializeActivity?.SetTag("file.path", outputFilePath);
                    await _xmlService.SerializeGenerationOutputAsync(generationOutput, outputFilePath);
                }

                _logger.LogInformation("Processed and saved output to: {OutputFilePath}", outputFilePath);
                activity?.AddEvent(new ActivityEvent(
                    "Processed file and saved output.",
                    tags:new ActivityTagsCollection { { "outputFilePath", outputFilePath } }
                ));

                string processedFilePath;
                using (var moveActivity = _activitySource.StartActivity("MoveProcessedFile"))
                {
                    var inputFilename = Path.GetFileNameWithoutExtension(filePath);
                    processedFilePath = Path.Combine(_configuration["Processed"]!, inputFilename + ".xml");
                    moveActivity?.SetTag("source.path", filePath);
                    moveActivity?.SetTag("destination.path", processedFilePath);
                    File.Move(filePath, processedFilePath);
                }

                _logger.LogInformation("Processed file moved to Processed Folder : {ProcessedFilePath}", Path.GetFileName(processedFilePath));
                activity?.AddEvent(new ActivityEvent(
                    "Moved processed file.",
                    tags: new ActivityTagsCollection { { "processedFilePath", processedFilePath } }
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FilePath}", filePath);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
            }
        }
    }
}