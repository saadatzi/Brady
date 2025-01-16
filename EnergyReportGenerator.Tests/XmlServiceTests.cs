using EnergyReportGenerator.Models;
using EnergyReportGenerator.Services;
using FluentAssertions;

namespace EnergyReportGenerator.Tests
{
    public class XmlServiceTests
    {
        private readonly XmlService _xmlService;

        public XmlServiceTests()
        {
            _xmlService = new XmlService();
        }

        [Fact]
        public async Task DeserializeGenerationReportAsync_ValidXml_ReturnsGenerationReport()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<GenerationReport>
    <Wind>
        <WindGenerator>
            <Name>Wind[Offshore]</Name>
            <Generation>
                <Day>
                    <Date>2023-01-01T00:00:00</Date>
                    <Energy>100</Energy>
                    <Price>20</Price>
                </Day>
            </Generation>
            <Location>Offshore</Location>
        </WindGenerator>
    </Wind>
    <Gas>
        <GasGenerator>
            <Name>Gas[1]</Name>
            <Generation>
                <Day>
                    <Date>2023-01-01T00:00:00</Date>
                    <Energy>200</Energy>
                    <Price>15</Price>
                </Day>
            </Generation>
            <EmissionsRating>0.03</EmissionsRating>
        </GasGenerator>
    </Gas>
    <Coal>
        <CoalGenerator>
            <Name>Coal[1]</Name>
            <Generation>
                <Day>
                    <Date>2023-01-01T00:00:00</Date>
                    <Energy>300</Energy>
                    <Price>10</Price>
                </Day>
            </Generation>
            <TotalHeatInput>12</TotalHeatInput>
            <ActualNetGeneration>10</ActualNetGeneration>
            <EmissionsRating>0.5</EmissionsRating>
        </CoalGenerator>
    </Coal>
</GenerationReport>";
            var filePath = CreateTestXmlFile(xmlContent, "GenerationReportTest.xml");

            // Act
            var result = await _xmlService.DeserializeGenerationReportAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.WindGenerators.Should().HaveCount(1);
            result.GasGenerators.Should().HaveCount(1);
            result.CoalGenerators.Should().HaveCount(1);

            // Clean up
            File.Delete(filePath);
        }

        [Fact]
        public async Task DeserializeReferenceDataAsync_ValidXml_ReturnsReferenceData()
        {
            // Arrange
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ReferenceData>
    <Factors>
        <ValueFactor>
            <High>0.9</High>
            <Medium>0.7</Medium>
            <Low>0.3</Low>
        </ValueFactor>
        <EmissionsFactor>
            <High>0.8</High>
            <Medium>0.6</Medium>
            <Low>0.3</Low>
        </EmissionsFactor>
    </Factors>
</ReferenceData>";
            var filePath = CreateTestXmlFile(xmlContent, "ReferenceDataTest.xml");

            // Act
            var result = await _xmlService.DeserializeReferenceDataAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Factors.Should().NotBeNull();
            result.Factors.ValueFactor.High.Should().Be(0.9);
            result.Factors.EmissionsFactor.Medium.Should().Be(0.6);

            // Clean up
            File.Delete(filePath);
        }

        [Fact]
        public async Task SerializeGenerationOutputAsync_ValidData_CreatesXmlFile()
        {
            // Arrange
            var generationOutput = new GenerationOutput
            {
                Totals = new List<GeneratorTotal>
                {
                    new GeneratorTotal { Name = "Wind[Offshore]", Total = 1000 }
                },
                MaxEmissionGenerators = new List<MaxEmissionDay>
                {
                    new MaxEmissionDay { Name = "Coal[1]", Date = DateTime.Now, Emission = 50 }
                },
                ActualHeatRates = new List<ActualHeatRate>
                {
                    new ActualHeatRate { Name = "Coal[1]", HeatRate = 1.2 }
                }
            };
            var filePath = Path.Combine(Path.GetTempPath(), "GenerationOutputTest.xml");

            // Act
            await _xmlService.SerializeGenerationOutputAsync(generationOutput, filePath);

            // Assert
            File.Exists(filePath).Should().BeTrue();

            // Clean up
            File.Delete(filePath);
        }

        private string CreateTestXmlFile(string xmlContent, string fileName)
        {
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            File.WriteAllText(filePath, xmlContent);
            return filePath;
        }
    }
}