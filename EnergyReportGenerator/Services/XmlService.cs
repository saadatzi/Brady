using System.Xml.Serialization;
using EnergyReportGenerator.Models;

namespace EnergyReportGenerator.Services
{
    public interface IXmlService
    {
        Task<GenerationReport?> DeserializeGenerationReportAsync(string filePath);
        Task<ReferenceData?> DeserializeReferenceDataAsync(string filePath);
        Task SerializeGenerationOutputAsync(GenerationOutput generationOutput, string filePath);
    }
    public class XmlService : IXmlService
    {
        public async Task<GenerationReport?> DeserializeGenerationReportAsync(string filePath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GenerationReport));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    return await Task.Run(() => (GenerationReport?)serializer.Deserialize(reader));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing GenerationReport: {ex.Message}");
                throw;
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
                Console.WriteLine($"Error deserializing ReferenceData: {ex.Message}");
                throw;
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
                Console.WriteLine($"Error serializing GenerationOutput: {ex.Message}");
                throw;
            }
        }
    }
}