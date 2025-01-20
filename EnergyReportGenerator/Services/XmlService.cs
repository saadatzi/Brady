namespace EnergyReportGenerator.Services;

public interface IXmlService
{
    Task<GenerationReport?> DeserializeGenerationReportAsync(string filePath);
    Task<ReferenceData?> DeserializeReferenceDataAsync(string filePath);
    Task SerializeGenerationOutputAsync(GenerationOutput generationOutput, string filePath);
    Task<GenerationReport?> DeserializeGenerationReportStreamAsync(Stream stream);
    Task<ReferenceData?> DeserializeReferenceDataStreamAsync(Stream stream);
    Task SerializeGenerationOutputStreamAsync(GenerationOutput generationOutput, Stream stream);
}
public class XmlService : IXmlService
{
    private readonly ILogger<XmlService> _logger;
    private readonly IActivitySource _activitySource;

    public XmlService(ILogger<XmlService> logger, IActivitySource activitySource)
    {
        _logger = logger;
        _activitySource = activitySource;
    }
    public async Task<GenerationReport?> DeserializeGenerationReportAsync(string filePath)
    {
        using var activity = _activitySource.StartActivity($"{nameof(XmlService)}.{nameof(DeserializeGenerationReportAsync)}");
        activity?.SetTag("file.path", filePath);
        _logger.LogInformation("Deserializing GenerationReport from: {FilePath}", filePath);

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GenerationReport));
            using (StreamReader reader = new StreamReader(filePath))
            {
                
                var report = (GenerationReport?)serializer.Deserialize(reader);
                activity?.SetTag("success", report != null);
                if (report == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Deserialized GenerationReport is null.");
                }
                return report;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing GenerationReport from: {FilePath}", filePath);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return null;
        }
    }

    public async Task<GenerationReport?> DeserializeGenerationReportStreamAsync(Stream stream)
    {
        using var activity = _activitySource.StartActivity($"{nameof(XmlService)}.{nameof(DeserializeGenerationReportStreamAsync)}");
        _logger.LogInformation("Deserializing GenerationReport from stream.");

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var serializer = new XmlSerializer(typeof(GenerationReport));
                var report = (GenerationReport)serializer.Deserialize(memoryStream);
                activity?.SetTag("success", true);
                return report;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing GenerationReport from stream.");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return null;
        }
    }

    public async Task<ReferenceData?> DeserializeReferenceDataAsync(string filePath)
    {
        using var activity = _activitySource.StartActivity($"{nameof(XmlService)}.{nameof(DeserializeReferenceDataAsync)}");
        activity?.SetTag("file.path", filePath);
        _logger.LogInformation("Deserializing ReferenceData from: {FilePath}", filePath);

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ReferenceData));
            using (StreamReader reader = new StreamReader(filePath))
            {
                var referenceData = (ReferenceData?)serializer.Deserialize(reader);
                activity?.SetTag("success", referenceData != null);
                if (referenceData == null)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Deserialized ReferenceData is null.");
                }
                return referenceData;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing ReferenceData from: {FilePath}", filePath);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return null;
        }
    }

    public async Task<ReferenceData?> DeserializeReferenceDataStreamAsync(Stream stream)
    {
        using var activity = _activitySource.StartActivity($"{nameof(XmlService)}.{nameof(DeserializeReferenceDataStreamAsync)}");
        _logger.LogInformation("Deserializing ReferenceData from stream.");

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var serializer = new XmlSerializer(typeof(ReferenceData));
                var referenceData = (ReferenceData)serializer.Deserialize(memoryStream);
                activity?.SetTag("success", true);
                return referenceData;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing ReferenceData from stream.");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return null;
        }
    }

    public async Task SerializeGenerationOutputAsync(GenerationOutput generationOutput, string filePath)
    {
        using var activity = _activitySource.StartActivity($"{nameof(XmlService)}.{nameof(SerializeGenerationOutputAsync)}");
        activity?.SetTag("file.path", filePath);
        _logger.LogInformation("Serializing GenerationOutput to: {FilePath}", filePath);
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GenerationOutput));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, generationOutput);
                activity?.SetTag("success", true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing GenerationOutput to: {FilePath}", filePath);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
        }
    }

    public async Task SerializeGenerationOutputStreamAsync(GenerationOutput generationOutput, Stream stream)
    {
        using var activity = _activitySource.StartActivity($"{nameof(XmlService)}.{nameof(SerializeGenerationOutputStreamAsync)}");
        _logger.LogInformation("Serializing GenerationOutput to stream.");

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(GenerationOutput));
                serializer.Serialize(memoryStream, generationOutput);

                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(stream);
                activity?.SetTag("success", true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing GenerationOutput to stream.");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
        }
    }
}