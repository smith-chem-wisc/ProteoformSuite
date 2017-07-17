namespace ProteoformSuiteInternal
{
    public class BiorepFractionTechrepIntensity
    {
        public string condition { get; set; }
        public string biorep { get; set; }
        public string fraction { get; set; }
        public string techrep { get; set; }
        public double intensity { get; set; }
        public BiorepFractionTechrepIntensity(string condition, string biorep, string fraction, string techrep, double intensity)
        {
            this.condition = condition;
            this.biorep = biorep;
            this.fraction = fraction;
            this.techrep = techrep;
            this.intensity = intensity;
        }
    }
}
