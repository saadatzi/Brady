namespace EnergyReportGenerator.Models;

[XmlRoot("GenerationReport")]
public class GenerationReport
{
    [XmlArray("Wind")]
    [XmlArrayItem("WindGenerator", typeof(WindGenerator))]
    public List<WindGenerator>? WindGenerators { get; set; }

    [XmlArray("Gas")]
    [XmlArrayItem("GasGenerator", typeof(GasGenerator))]
    public List<GasGenerator>? GasGenerators { get; set; }

    [XmlArray("Coal")]
    [XmlArrayItem("CoalGenerator", typeof(CoalGenerator))]
    public List<CoalGenerator>? CoalGenerators { get; set; }
}

public class WindGenerator : Generator
{
    [XmlElement("Location")]
    public string? Location { get; set; }
}

public class GasGenerator : Generator
{
    [XmlElement("EmissionsRating")]
    public double EmissionsRating { get; set; }
}

public class CoalGenerator : Generator
{
    [XmlElement("TotalHeatInput")]
    public double TotalHeatInput { get; set; }

    [XmlElement("ActualNetGeneration")]
    public double ActualNetGeneration { get; set; }

    [XmlElement("EmissionsRating")]
    public double EmissionsRating { get; set; }
}

public class Generator
{
    [XmlElement("Name")]
    public string? Name { get; set; }

    [XmlArray("Generation")]
    [XmlArrayItem("Day", typeof(DayGeneration))]
    public List<DayGeneration>? Generation { get; set; }
}

public class DayGeneration
{
    [XmlElement("Date")]
    public DateTime Date { get; set; }

    [XmlElement("Energy")]
    public double Energy { get; set; }

    [XmlElement("Price")]
    public double Price { get; set; }
}