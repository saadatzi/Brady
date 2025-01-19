namespace EnergyReportGenerator.Tests;

public class FileProcessorServiceTests : IDisposable
{
    private readonly Mock<ILogger<FileProcessorService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IXmlService> _mockXmlService;
    private readonly Mock<IGenerationCalculatorService> _mockCalculatorService;
    private readonly FileProcessorService _fileProcessorService;
    private readonly string _testDirectory;

    public FileProcessorServiceTests()
    {
        _mockLogger = new Mock<ILogger<FileProcessorService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockXmlService = new Mock<IXmlService>();
        _mockCalculatorService = new Mock<IGenerationCalculatorService>();

        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        Directory.CreateDirectory(Path.Combine(_testDirectory, "Input"));
        Directory.CreateDirectory(Path.Combine(_testDirectory, "Output"));
        Directory.CreateDirectory(Path.Combine(_testDirectory, "Processed"));

        _mockConfiguration.Setup(c => c["InputFolder"]).Returns(Path.Combine(_testDirectory, "Input"));
        _mockConfiguration.Setup(c => c["Processed"]).Returns(Path.Combine(_testDirectory, "Processed"));
        _mockConfiguration.Setup(c => c["OutputFolder"]).Returns(Path.Combine(_testDirectory, "Output"));
        _mockConfiguration.Setup(c => c["ReferenceDataPath"]).Returns(Path.Combine(_testDirectory, "ReferenceData.xml"));

        _fileProcessorService = new FileProcessorService(
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockXmlService.Object,
            _mockCalculatorService.Object);
    }

    [Fact]
    public async Task StartAsync_WatchesInputFolder()
    {
        // Act
        await _fileProcessorService.StartAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith("Watching for XML files in")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task OnFileCreated_ValidFile_ProcessesFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "Input", "GenerationReportTest.xml");
        await File.WriteAllTextAsync(filePath, "<GenerationReport></GenerationReport>");

        var generationReport = new GenerationReport();
        var referenceData = new ReferenceData();
        var generationOutput = new GenerationOutput();

        _mockXmlService.Setup(x => x.DeserializeGenerationReportAsync(filePath)).ReturnsAsync(generationReport);
        _mockXmlService.Setup(x => x.DeserializeReferenceDataAsync(It.IsAny<string>())).ReturnsAsync(referenceData);
        _mockCalculatorService.Setup(x => x.Calculate(generationReport, referenceData)).Returns(generationOutput);

        await _fileProcessorService.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(1000); // Allow time for the FileSystemWatcher to trigger

        // Assert
        _mockXmlService.Verify(x => x.DeserializeGenerationReportAsync(filePath), Times.Once);
        _mockXmlService.Verify(x => x.DeserializeReferenceDataAsync(It.IsAny<string>()), Times.Once);
        _mockCalculatorService.Verify(x => x.Calculate(generationReport, referenceData), Times.Once);
        _mockXmlService.Verify(x => x.SerializeGenerationOutputAsync(generationOutput, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task OnFileCreated_Exception_LogsError()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "Input", "GenerationReportTest.xml");
        File.WriteAllText(filePath, "<InvalidXml>");

        _mockXmlService.Setup(x => x.DeserializeGenerationReportAsync(filePath)).ThrowsAsync(new Exception("Deserialization error"));

        await _fileProcessorService.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(1000); // Allow time for the FileSystemWatcher to trigger

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.StartsWith("Error processing file")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    public void Dispose()
    {
        _fileProcessorService.Stop(CancellationToken.None);
        Directory.Delete(_testDirectory, true);
    }
}
