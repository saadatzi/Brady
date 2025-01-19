namespace EnergyReportGenerator.Tests;

public class GenerationCalculatorServiceTests
{
    private readonly GenerationCalculatorService _calculatorService;

    public GenerationCalculatorServiceTests()
    {
        _calculatorService = new GenerationCalculatorService();
    }

    [Fact]
    public void Calculate_ValidData_ReturnsCorrectTotals()
    {
        // Arrange
        var report = new GenerationReport
        {
            WindGenerators = new List<WindGenerator>
            {
                new WindGenerator
                {
                    Name = "Wind[Offshore]",
                    Location = "Offshore",
                    Generation = new List<DayGeneration>
                    {
                        new DayGeneration { Date = DateTime.Now, Energy = 100, Price = 20 }
                    }
                }
            },
            GasGenerators = new List<GasGenerator>
            {
                new GasGenerator
                {
                    Name = "Gas[1]",
                    EmissionsRating = 0.03,
                    Generation = new List<DayGeneration>
                    {
                        new DayGeneration { Date = DateTime.Now, Energy = 200, Price = 15 }
                    }
                }
            },
            CoalGenerators = new List<CoalGenerator>
            {
                new CoalGenerator
                {
                    Name = "Coal[1]",
                    EmissionsRating = 0.5,
                    TotalHeatInput = 12,
                    ActualNetGeneration = 10,
                    Generation = new List<DayGeneration>
                    {
                        new DayGeneration { Date = DateTime.Now, Energy = 300, Price = 10 }
                    }
                }
            }
        };
        var referenceData = new ReferenceData
        {
            Factors = new Factors
            {
                ValueFactor = new ValueFactor { High = 0.9, Medium = 0.7, Low = 0.3 },
                EmissionsFactor = new EmissionsFactor { High = 0.8, Medium = 0.6, Low = 0.3 }
            }
        };

        // Act
        var result = _calculatorService.Calculate(report, referenceData);

        // Assert
        result.Totals.Should().NotBeNull();
        result.Totals.Should().HaveCount(3);
        result.Totals?.Single(t => t.Name == "Wind[Offshore]").Total.Should().BeApproximately(100 * 20 * 0.3, 0.01); // Energy * Price * ValueFactor.Low
        result.Totals?.Single(t => t.Name == "Gas[1]").Total.Should().BeApproximately(200 * 15 * 0.7, 0.01); // Energy * Price * ValueFactor.Medium
        result.Totals?.Single(t => t.Name == "Coal[1]").Total.Should().BeApproximately(300 * 10 * 0.7, 0.01); // Energy * Price * ValueFactor.Medium
    }

    [Fact]
    public void Calculate_ValidData_ReturnsCorrectMaxEmissionGenerators()
    {
        // Arrange
        var report = new GenerationReport
        {
            WindGenerators = new List<WindGenerator>(), // No emissions for wind
            GasGenerators = new List<GasGenerator>
            {
                new GasGenerator
                {
                    Name = "Gas[1]",
                    EmissionsRating = 0.03,
                    Generation = new List<DayGeneration>
                    {
                        new DayGeneration { Date = DateTime.Today, Energy = 100, Price = 15 },
                        new DayGeneration { Date = DateTime.Today.AddDays(1), Energy = 150, Price = 15 }
                    }
                }
            },
            CoalGenerators = new List<CoalGenerator>
            {
                new CoalGenerator
                {
                    Name = "Coal[1]",
                    EmissionsRating = 0.5,
                    TotalHeatInput = 12,
                    ActualNetGeneration = 10,
                    Generation = new List<DayGeneration>
                    {
                        new DayGeneration { Date = DateTime.Today, Energy = 50, Price = 10 },
                        new DayGeneration { Date = DateTime.Today.AddDays(1), Energy = 40, Price = 10 }
                    }
                }
            }
        };
        var referenceData = new ReferenceData
        {
            Factors = new Factors
            {
                ValueFactor = new ValueFactor { High = 0.9, Medium = 0.7, Low = 0.3 },
                EmissionsFactor = new EmissionsFactor { High = 0.8, Medium = 0.6, Low = 0.3 }
            }
        };

        // Act
        var result = _calculatorService.Calculate(report, referenceData);

        // Assert
        result.MaxEmissionGenerators.Should().NotBeNull();
        result.MaxEmissionGenerators.Should().HaveCount(2); // Two days with emissions
        result.MaxEmissionGenerators?.Single(m => m.Date == DateTime.Today).Name.Should().Be("Coal[1]");
        result.MaxEmissionGenerators?.Single(m => m.Date == DateTime.Today).Emission.Should().BeApproximately(50 * 0.5 * 0.8, 0.01); // Energy * EmissionRating * EmissionFactor.High
        result.MaxEmissionGenerators?.Single(m => m.Date == DateTime.Today.AddDays(1)).Name.Should().Be("Coal[1]");
        result.MaxEmissionGenerators?.Single(m => m.Date == DateTime.Today.AddDays(1)).Emission.Should().BeApproximately(40 * 0.5 * 0.8, 0.01); // Energy * EmissionRating * EmissionFactor.Medium
    }

    [Fact]
    public void Calculate_ValidData_ReturnsCorrectActualHeatRates()
    {
        // Arrange
        var report = new GenerationReport
        {
            WindGenerators = new List<WindGenerator>(),
            GasGenerators = new List<GasGenerator>(),
            CoalGenerators = new List<CoalGenerator>
            {
                new CoalGenerator
                {
                    Name = "Coal[1]",
                    EmissionsRating = 0.5,
                    TotalHeatInput = 120,
                    ActualNetGeneration = 100,
                    Generation = new List<DayGeneration>
                    {
                        new DayGeneration { Date = DateTime.Now, Energy = 300, Price = 10 }
                    }
                },
                new CoalGenerator
                {
                    Name = "Coal[2]",
                    EmissionsRating = 0.6,
                    TotalHeatInput = 100,
                    ActualNetGeneration = 0, // Test case for division by zero
                    Generation = new List<DayGeneration>
                    {
                        new DayGeneration { Date = DateTime.Now, Energy = 200, Price = 12 }
                    }
                }
            }
        };
        var referenceData = new ReferenceData
        {
            Factors = new Factors
            {
                ValueFactor = new ValueFactor { High = 0.9, Medium = 0.7, Low = 0.3 },
                EmissionsFactor = new EmissionsFactor { High = 0.8, Medium = 0.6, Low = 0.3 }
            }
        };

        // Act
        var result = _calculatorService.Calculate(report, referenceData);

        // Assert
        result.ActualHeatRates.Should().NotBeNull();
        result.ActualHeatRates.Should().HaveCount(2);
        result.ActualHeatRates?.Single(h => h.Name == "Coal[1]").HeatRate.Should().BeApproximately(1.2, 0.01); // TotalHeatInput / ActualNetGeneration
        result.ActualHeatRates?.Single(h => h.Name == "Coal[2]").HeatRate.Should().Be(double.NaN); // Handle division by zero
    }
}