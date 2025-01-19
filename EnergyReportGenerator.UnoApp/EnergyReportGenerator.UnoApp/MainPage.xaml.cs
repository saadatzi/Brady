using Windows.Storage.Pickers;
using System.Xml.Serialization;
using EnergyReportGenerator.Models;
using EnergyReportGenerator.Services;

namespace EnergyReportGenerator.UnoApp;

public sealed partial class MainPage : Page
{
    private StorageFile? _generationReport;
    private StorageFile? _referenceData;
    private IGenerationCalculatorService _calculatorService;
    private IXmlService _xmlService;

    public MainPage()
    {
        this.InitializeComponent();
        _calculatorService = App.Services.GetRequiredService<IGenerationCalculatorService>();
        _xmlService = App.Services.GetRequiredService<IXmlService>();
    }
    private async Task SelectGenerationReportButton_Click(object sender, RoutedEventArgs e)
    {
        _generationReport = await PickFileAsync();
        GenerationReportPathTextBlock.Text = Path.GetFileName(_generationReport.Path) ?? "No file selected";
    }

    private async Task SelectReferenceDataButton_Click(object sender, RoutedEventArgs e)
    {
        _referenceData = await PickFileAsync();
        ReferenceDataPathTextBlock.Text = Path.GetFileName(_referenceData.Path) ?? "No file selected";
    }

    private async Task<StorageFile> PickFileAsync()
    {
        var openPicker = new FileOpenPicker();
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        openPicker.FileTypeFilter.Add(".xml");

        var window = Window.Current;

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        StorageFile file = await openPicker.PickSingleFileAsync();
        return file;
    }

    private async Task CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_generationReport == null || _referenceData == null)
        {
            OutputTextBlock.Text = "Please select both Generation Report and Reference Data files.";
            return;
        }
        
        try
        {
            using (var generationReportStream = await _generationReport.OpenStreamForReadAsync())
            using (var referenceDataStream = await _referenceData.OpenStreamForReadAsync())
            {
                var generationReport = await _xmlService.DeserializeGenerationReportStreamAsync(generationReportStream);
                var referenceData = await _xmlService.DeserializeReferenceDataStreamAsync(referenceDataStream);

                var generationOutput = _calculatorService.Calculate(generationReport, referenceData);

                using (var outputStream = new MemoryStream())
                {
                    await _xmlService.SerializeGenerationOutputStreamAsync(generationOutput, outputStream);

                    outputStream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(outputStream))
                    {
                        var outputXml = await reader.ReadToEndAsync();
                        OutputTextBlock.Text = outputXml;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            OutputTextBlock.Text = $"Error during calculation: {ex.Message}";
        }
    }
}