# Energy Report Generator - Uno Platform App

## Overview

This project is a web application built using the Uno Platform that demonstrates how to process XML data representing energy generation reports. The app allows users to:

1. Select a `GenerationReport.xml` file.
2. Select a `ReferenceData.xml` file.
3. Perform calculations based on the data in these files.
4. Display the results as XML in the browser or offer a download option.

This application is built using Uno Platform and targets WebAssembly (WASM) for browser-based execution.

## Prerequisites

*   [.NET 8 SDK](https://dotnet.microsoft.com/download)
*   Visual Studio 2022 with the following workloads:
    *   .NET Multi-platform App UI development
    *   ASP.NET and web development
*   [Uno Platform Extension](https://marketplace.visualstudio.com/items?itemName=unoplatform.uno-platform-addin-2022) for Visual Studio (optional but recommended)
*   Install the wasm-tools workload
    ```bash
    dotnet workload install wasm-tools
    ```
*   Install the Uno.Templates
    ```bash
    dotnet new install Uno.Templates
    ```

## Project Structure

*   **`EnergyReportGenerator.UnoApp`:** Shared project containing the core application logic, UI (XAML), and services.
*   **`EnergyReportGenerator.UnoApp.Wasm`:** Project for the WebAssembly (WASM) target.
*   **`MainPage.xaml`:** The main UI of the application.
*   **`App.xaml.cs`:** Application entry point and service configuration.
*   **`Models`:** Contains the C# classes that represent the structure of the XML data (e.g., `GenerationReport`, `ReferenceData`, `GenerationOutput`).
*   **`Services`:** Contains the services for XML processing (`XmlService`) and calculations (`GenerationCalculatorService`).

## Getting Started

1. **Open the Solution:** Open the solution file (`EnergyReportGenerator.UnoApp.sln`) in Visual Studio 2022.

2. **Restore NuGet Packages:**
    *   In Visual Studio, right-click on the solution in the Solution Explorer and select "Restore NuGet Packages."
    *   Or, using the .NET CLI:

        ```bash
        dotnet restore
        ```

3. **Set Startup Project:** In the Solution Explorer, right-click on the `EnergyReportGenerator.UnoApp.Wasm` project and select "Set as Startup Project."

4. **Build and Run:** Press F5 or click the "Start" button in Visual Studio to build and run the application. The app will open in your default web browser.

## Key Features

*   **File Selection:** Uses the browser's `FileOpenPicker` to allow users to select `GenerationReport.xml` and `ReferenceData.xml` files.
*   **XML Processing:** Uses `XmlSerializer` to deserialize the XML data into C# objects and serialize the output back to XML.
*   **Dependency Injection:** Employs dependency injection to manage services like `XmlService` and `GenerationCalculatorService`.
*   **Asynchronous Operations:** Uses `async` and `await` for file I/O and potentially long-running calculations to keep the UI responsive.

## Usage

1. Launch the application in your web browser.
2. Click the "Browse" button next to "Generation Report" and select a `GenerationReport.xml` file.
3. Click the "Browse" button next to "Reference Data" and select a `ReferenceData.xml` file.
4. Click the "Calculate" button to perform the calculations.
5. The generated `GenerationOutput.xml` will be displayed in the text area below the button.

## Notes

*   **WASM Limitations:** WebAssembly has limitations regarding file system access. The `FileOpenPicker` uses the browser's file selection capabilities, and the application works with the file content as streams.
*   **Debugging:** Use your browser's developer tools (F12) to view console logs and debug JavaScript code. You can also debug C# code in Visual Studio by attaching the debugger to the browser process.
*   **Error Handling:** The application includes basic error handling, but you might want to enhance it further based on your specific requirements.

## License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.