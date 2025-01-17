
## Setup

1. **Build the Solution:**
    *   Make sure you have built the entire `EnergyReportGenerator` solution in **Release** mode. This is crucial for accurate performance measurements.

2. **Configuration (`appsettings.json`):**
    *   The `appsettings.json` file in the `EnergyReportGenerator.Benchmark` project configures the benchmark.
    *   **`InputFolder`:** Specifies the folder where input files will be generated for the benchmark.
    *   **`OutputFolder`:** Specifies the folder where output files will be generated during the benchmark.
    *   **`Processed`:** Specifies the folder where processed files will be moved during the benchmark.
    *   **`ReferenceDataPath`:** Path to the `ReferenceData.xml` file used by the application.
    *   **`SampleGenerationReportFile`:** Path to a sample `GenerationReport.xml` file. This file will be copied multiple times to create the benchmark input files.

    **Example `appsettings.json`:**

    ```json
    {
        "InputFolder": "/home/sss/Brady/BenchmarkInput",
        "OutputFolder": "/home/sss/Brady/BenchmarkOutput",
        "Processed": "/home/sss/Brady/BenchmarkProcessed",
        "ReferenceDataPath": "./Data/ReferenceData.xml",
        "SampleGenerationReportFile": "/home/sss/Brady/GenerationReport.xml"
    }
    ```

3. **Data Folder:**
    *   Ensure you have a `Data` folder in the root of the `EnergyReportGenerator.Benchmark` project.
    *   Place a sample `GenerationReport.xml` file in `SampleGenerationReportFile` path.
    *   Copy the `ReferenceData.xml` file to the `Data` folder.

## Running the Benchmarks

1. **Navigate to the Benchmark Project's Output Directory:**
    *   Open your command line or terminal.
    *   Navigate to the directory where the compiled benchmark executable is located. This is typically `EnergyReportGenerator.Benchmark\bin\Release\net8.0` (or a similar path, depending on your build configuration).

2. **Run the Benchmark using the .NET CLI:**

    ```bash
    dotnet EnergyReportGenerator.Benchmark.dll
    ```

    This will start the BenchmarkDotNet runner.

## Understanding the Output

BenchmarkDotNet will output a detailed summary table to the console after the benchmarks have finished running. This table will include metrics such as:

*   **Mean:** The average execution time of the benchmark method.
*   **Error:** The estimated error of the mean.
*   **StdDev:** The standard deviation of the measurements.
*   **Gen 0/1/2:** Garbage collection counts for different generations.
*   **Allocated:** The amount of memory allocated by the benchmark method.

You'll also find results in the `BenchmarkDotNet.Artifacts` folder, which is generated in the same location as the benchmark executable. These results include more detailed reports in various formats (HTML, CSV, etc.).

## Analyzing the Results

Compare the `Mean` execution time of the `StartAsyncBenchmark` and `MultiprocessingStartAsyncBenchmark` methods. You should observe a performance improvement with `MultiprocessingStartAsync` when processing a large number of files, especially on systems with multiple CPU cores.

**Results on my device:**
```
BenchmarkDotNet v0.14.0, Ubuntu 24.04.1 LTS (Noble Numbat)
Intel Core i7-14700HX, 1 CPU, 28 logical and 20 physical cores
.NET SDK 8.0.405
  [Host]   : .NET 8.0.12 (8.0.1224.60305), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.12 (8.0.1224.60305), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0  InvocationCount=1  
UnrollFactor=1  


| Method                             | Mean     | Error    | StdDev   | Gen0      | Gen1      | Gen2      | Allocated |
|----------------------------------- |---------:|---------:|---------:|----------:|----------:|----------:|----------:|
| StartAsyncBenchmark                | 55.52 ms | 4.786 ms | 13.73 ms | 5000.0000 | 4000.0000 | 1000.0000 |  79.12 MB |
| MultiprocessingStartAsyncBenchmark | 45.19 ms | 5.225 ms | 15.16 ms | 5000.0000 | 3000.0000 |         - |   79.3 MB |
```
---

**Note:** The performance difference will depend on factors such as:

*   The number of files being processed.
*   The size of the files.
*   The number of CPU cores on your machine.

## Important Considerations

*   **Release Mode:** Always run benchmarks in Release mode to get accurate performance measurements. Debug builds include extra overhead that can significantly skew results.
*   **File System Caching:** The operating system's file system cache can affect benchmark results. If you run the benchmark multiple times, the first run might be slower because it needs to load data from disk. Subsequent runs might be faster because the data is cached in memory. Consider running the benchmark multiple times or using a larger dataset to minimize the impact of caching.
*   **Synchronization:** The benchmark uses a simple `Task.Delay` to wait for processing to complete. For more accurate measurements, you might want to implement a synchronization mechanism (e.g., `ManualResetEventSlim`) to ensure that the benchmark only proceeds after all files have been processed.