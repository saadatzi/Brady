using Windows.Storage.Pickers;
using System.Xml.Serialization;
using EnergyReportGenerator.Models;
using EnergyReportGenerator.Services;

namespace EnergyReportGenerator.UnoApp;

public sealed partial class MainPage : Page
{
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

}