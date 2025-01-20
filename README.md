
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
    *   Update `appsettings.json` with the correct paths for `InputFolder`, `OutputFolder`, `Processed` and `ReferenceDataPath`. These paths should point to the `Data/Input`, `Data/Processed`  and `Data/Output` folders and the `Data/ReferenceData.xml` file, respectively.

    ```json
    {
      "InputFolder": "/Data/Input",
      "Processed": "/Data/Processed",
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

## Observability with Serilog, Seq, and OpenTelemetry

This application utilizes Serilog for structured logging, Seq for centralized log viewing, and OpenTelemetry for distributed tracing and metrics.

### Serilog

*   **Structured Logging:**  Serilog is used throughout the application to generate well-structured log messages that are easy to read and analyze.
*   **Configuration:** Serilog is configured using the `Serilog` section in `appsettings.json`. This section allows you to control logging levels, output sinks (like console, file, and Seq), and other settings.
*   **Enrichment:** Log messages are enriched with valuable context, such as:
    *   `Timestamp`: When the event occurred.
    *   `Level`: The severity of the event (e.g., Information, Warning, Error).
    *   `Message`: The log message itself.
    *   `Exception`: Any exception details.
    *   `TraceId`, `SpanId`:  (When an OpenTelemetry activity is active) to link logs to traces.
    *   `FileName`: (In `FileProcessorService`) The file name when processing files.

### Seq

*   **Centralized Log Viewing:** Seq is used as a central log server for viewing and analyzing your application's logs and telemetry data.
*   **Installation:** To use Seq, you will need to run the Seq server and configure your application to send logs there. A convenient way to do this is using docker compose, described in the following steps.
*   **Docker/Docker-compose:** To start Seq with docker, please follow these steps:
    1.  Make sure you have `docker` installed.
    2.  run this command and replace your password with <password>

    ```bash
        PH=$(echo '<password>' | docker run --rm -i datalust/seq config hash)

        docker run \
            --name brady-seq \
            -d \
            --restart unless-stopped \
            -e ACCEPT_EULA=Y \
            -e SEQ_API_CANONICALURI=http://localhost:8080 \
            -e SEQ_FIRSTRUN_ADMINPASSWORDHASH="$PH" \
            -v brady-seq-data:/data \
            -p 8080:80 \
            -p 5341:5341 \
            datalust/seq
    ```
### OpenTelemetry

*   **Distributed Tracing:** OpenTelemetry is used to collect and export traces from your application, allowing you to visualize the flow of requests and understand performance bottlenecks.
*   **Custom Spans:** The application is instrumented using a custom `ActivitySource` and creates spans for key operations in the `FileProcessorService`, `GenerationCalculatorService`, and `XmlService`.
*   **Tags and Events:** Spans are enriched with relevant tags and events to provide context (e.g., file paths, success/failure indicators).
*   **OTLP/gRPC Export:** OpenTelemetry traces and metrics are sent to Seq using the OTLP protocol over gRPC.

### Correlating Logs and Traces

*   Serilog log messages are automatically enriched with `TraceId` and `SpanId` when an OpenTelemetry activity is active.
*   This allows you to easily jump from a log message to the specific trace that generated it in the Seq UI, providing invaluable context for debugging and monitoring.
*   The enrichment is accomplished using `Serilog.Enrichers.Span` package.

### Viewing Logs and Traces in Seq

*   Open your web browser and go to the Seq UI (typically `http://localhost:80`).
*   You can then view and search your application's log messages and traces within the Seq interface.
*   Use Seq's filtering and querying features to find specific events or traces of interest.

By using Serilog, Seq, and OpenTelemetry together, you have a comprehensive observability solution for your application, enabling you to understand its behavior, performance, and health effectively.

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