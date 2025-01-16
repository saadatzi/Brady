using System.Xml.Serialization;

namespace EnergyReportGenerator.Models
{
    [XmlRoot("GenerationOutput")]
    public class GenerationOutput
    {
        [XmlArray("Totals")]
        [XmlArrayItem("Generator", typeof(GeneratorTotal))]
        public List<GeneratorTotal> Totals { get; set; }

        [XmlArray("MaxEmissionGenerators")]
        [XmlArrayItem("Day", typeof(MaxEmissionDay))]
        public List<MaxEmissionDay> MaxEmissionGenerators { get; set; }

        [XmlArray("ActualHeatRates")]
        [XmlArrayItem("CoalGenerator", typeof(ActualHeatRate))]
        public List<ActualHeatRate> ActualHeatRates { get; set; }
    }

    public class GeneratorTotal
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Total")]
        public double Total { get; set; }
    }

    public class MaxEmissionDay
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Date")]
        public DateTime Date { get; set; }

        [XmlElement("Emission")]
        public double Emission { get; set; }
    }

    public class ActualHeatRate
    {
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("HeatRate")]
        public double HeatRate { get; set; }
    }
}