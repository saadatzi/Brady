
## Prerequisites

*   .NET 8 SDK
*   An IDE like Visual Studio or VS Code

## Setup

1. **Clone the Repository:**
    ```bash
    git clone https://github.com/saadatzi/Brady.git
    cd EnergyReportGenerator
    ```

2. **Configuration:**
    *   Update `appsettings.json` with the correct paths for `InputFolder`, `OutputFolder`, and `ReferenceDataPath`. These paths should point to the `Data\Input` and `Data\Output` folders and the `Data\ReferenceData.xml` file, respectively.

    ```json
    {
      "InputFolder": "/Data/Input",
      "OutputFolder": "/Data/Output",
      "ReferenceDataPath": "./Data/ReferenceData.xml"
    }
    ```

3. **Build the Solution:**

    *   **Using Visual Studio:** Open the solution in Visual Studio and build it (Build > Build Solution).
    *   **Using VS Code:** Open the project folder in VS Code. Use the .NET CLI:

        ```bash
        dotnet build
        ```

## Running the Application

1. **Place Input Files:** Ensure that `GenerationReport.xml` files are placed in the `Data\Input` folder. If you want to test with existing files, make sure they are present before starting the application.

2. **Start the Application:**

    *   **Using Visual Studio:** Press F5 or Ctrl+F5 to start the application.
    *   **Using VS Code:** Run the following command in the terminal or use c# dev kit and simply start a new instance from EnergyReportGenerator

        ```bash
        dotnet run --project EnergyReportGenerator
        ```

3. **Monitor Output:** The application will process any existing `GenerationReport.xml` files in `Data\Input` and monitor the folder for new files. Output files will be generated in the `Data\Output` folder with names like `GenerationOutput_yyyyMMddHHmmss.xml`.

## Running Unit Tests

1. **Using Visual Studio:**
    *   Open Test Explorer (Test > Windows > Test Explorer).
    *   Click "Run All Tests" to execute all tests.

2. **Using VS Code:**
    *   Open the Command Palette (Ctrl+Shift+P or Cmd+Shift+P).
    *   Type `>dotnet test` and select the "dotnet test" command.
    *   Alternatively, use the following command in the terminal:

        ```bash
        dotnet test EnergyReportGenerator.Tests
        ```

## Code Highlights

### FileProcessorService

*   The `StartAsync` method initializes the `FileSystemWatcher` and processes existing files in the input folder.
*   The `OnFileCreated` method handles the file creation event, deserializes the XML, performs calculations, and serializes the output.
*   The `IsGenerationReportFile` method is a helper to determine if a file is a `GenerationReport` based on its root element.

### GenerationCalculatorService

*   The `Calculate` method performs the core calculations to generate the `GenerationOutput` object.

### XmlService

*   Provides methods to deserialize `GenerationReport.xml` and `ReferenceData.xml` files using `XmlSerializer`.
*   Provides a method to serialize the `GenerationOutput` object to XML.

## License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.