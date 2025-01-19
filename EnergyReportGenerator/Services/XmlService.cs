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

    public XmlService(ILogger<XmlService> logger)
    {
        _logger = logger;
    }
    public async Task<GenerationReport?> DeserializeGenerationReportAsync(string filePath)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GenerationReport));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return await Task.Run(() => {
                    try
                    {
                        return (GenerationReport?)serializer.Deserialize(reader);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deserializing GenerationReport within Task.Run");
                        return null;;
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing GenerationReport");
            return null;
        }
    }

    public async Task<GenerationReport?> DeserializeGenerationReportStreamAsync(Stream stream)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var serializer = new XmlSerializer(typeof(GenerationReport));
                return (GenerationReport)serializer.Deserialize(memoryStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing GenerationReport");
            return null;
        }
    }

    public async Task<ReferenceData?> DeserializeReferenceDataAsync(string filePath)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ReferenceData));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return await Task.Run(() => (ReferenceData?)serializer.Deserialize(reader));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing ReferenceData");
            return null;
        }
    }

    public async Task<ReferenceData?> DeserializeReferenceDataStreamAsync(Stream stream)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var serializer = new XmlSerializer(typeof(ReferenceData));
                return (ReferenceData)serializer.Deserialize(memoryStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing ReferenceData");
            return null;
        }
    }

    public async Task SerializeGenerationOutputAsync(GenerationOutput generationOutput, string filePath)
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GenerationOutput));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                await Task.Run(() => serializer.Serialize(writer, generationOutput));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing GenerationOutput");
        }
    }

    public async Task SerializeGenerationOutputStreamAsync(GenerationOutput generationOutput, Stream stream)
    {
        try
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(GenerationOutput));
                serializer.Serialize(memoryStream, generationOutput);

                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(stream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing GenerationOutput");
        }
    }
}