namespace EnergyReportGenerator.Services;

public interface IGenerationCalculatorService
{
    GenerationOutput Calculate(GenerationReport? report, ReferenceData? referenceData);
}
public class GenerationCalculatorService : IGenerationCalculatorService
{
    private readonly ILogger<GenerationCalculatorService> _logger;
    private readonly ActivitySource _activitySource;
    public GenerationCalculatorService(ILogger<GenerationCalculatorService> logger, ActivitySource activitySource)
    {
        _logger = logger;
        _activitySource = activitySource;
    }

    public GenerationOutput Calculate(GenerationReport report, ReferenceData referenceData)
    {
        using var activity = _activitySource.StartActivity($"{nameof(GenerationCalculatorService)}.{nameof(Calculate)}");

        if (report == null || referenceData == null)
        {
            _logger.LogError("GenerationReport or ReferenceData is null.");
            activity?.SetStatus(ActivityStatusCode.Error, "Input data is null.");
            return new GenerationOutput();
        }

        var output = new GenerationOutput
        {
            Totals = CalculateGeneratorTotals(report, referenceData),
            MaxEmissionGenerators = CalculateMaxEmissionGenerators(report, referenceData),
            ActualHeatRates = CalculateActualHeatRates(report)
        };

        _logger.LogInformation("Generation output calculation completed.");
        activity?.AddEvent(new ActivityEvent("Finished generation calculation."));
        return output;
    }

    private List<GeneratorTotal> CalculateGeneratorTotals(GenerationReport report, ReferenceData referenceData)
    {
        using var activity = _activitySource.StartActivity($"{nameof(CalculateGeneratorTotals)}");
        var totals = new List<GeneratorTotal>();

        var allGenerators = report.WindGenerators?.Cast<Generator>()
            .Concat(report.GasGenerators)
            .Concat(report.CoalGenerators)
            .ToList();

        foreach (var generator in allGenerators)
        {
            double totalGenerationValue = 0;

            foreach (var day in generator.Generation)
            {
                double valueFactor = GetValueFactor(generator, referenceData);
                totalGenerationValue += day.Energy * day.Price * valueFactor;
            }

            totals.Add(new GeneratorTotal { Name = generator.Name, Total = totalGenerationValue });
        }

        return totals;
    }

    private List<MaxEmissionDay> CalculateMaxEmissionGenerators(GenerationReport report, ReferenceData referenceData)
    {
        using var activity = _activitySource.StartActivity($"{nameof(CalculateMaxEmissionGenerators)}");
        var maxEmissionDays = new Dictionary<DateTime, MaxEmissionDay>();

        var emissionGenerators = report.GasGenerators.Cast<Generator>()
            .Concat(report.CoalGenerators)
            .ToList();

        foreach (var generator in emissionGenerators)
        {
            double emissionFactor = GetEmissionFactor(generator, referenceData);

            foreach (var day in generator.Generation)
            {
                double emissionRating = (generator is GasGenerator gasGen) ? gasGen.EmissionsRating :
                                        (generator is CoalGenerator coalGen) ? coalGen.EmissionsRating :
                                        0;

                double dailyEmission = day.Energy * emissionRating * emissionFactor;

                if (!maxEmissionDays.ContainsKey(day.Date) || dailyEmission > maxEmissionDays[day.Date].Emission)
                {
                    maxEmissionDays[day.Date] = new MaxEmissionDay
                    {
                        Name = generator.Name,
                        Date = day.Date,
                        Emission = dailyEmission
                    };
                }
            }
        }

        return maxEmissionDays.Values.ToList();
    }

    private List<ActualHeatRate> CalculateActualHeatRates(GenerationReport report)
    {
        using var activity = _activitySource.StartActivity($"{nameof(CalculateActualHeatRates)}");
        return report.CoalGenerators.Select(g => new ActualHeatRate
        {
            Name = g.Name,
            HeatRate = g.ActualNetGeneration != 0 ? g.TotalHeatInput / g.ActualNetGeneration : double.NaN
        }).ToList();
    }

    private double GetValueFactor(Generator generator, ReferenceData referenceData)
    {
        if (generator is WindGenerator windGen)
        {
            return windGen.Location == "Offshore" ? referenceData.Factors.ValueFactor.Low : referenceData.Factors.ValueFactor.High;
        }
        else if (generator is GasGenerator || generator is CoalGenerator)
        {
            return referenceData.Factors.ValueFactor.Medium;
        }

        return 0;
    }

    private double GetEmissionFactor(Generator generator, ReferenceData referenceData)
    {
        if (generator is GasGenerator)
        {
            return referenceData.Factors.EmissionsFactor.Medium;
        }
        else if (generator is CoalGenerator)
        {
            return referenceData.Factors.EmissionsFactor.High;
        }

        return 0;
    }
}