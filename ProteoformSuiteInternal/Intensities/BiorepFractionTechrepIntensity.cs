namespace ProteoformSuiteInternal
{
    public class BiorepFractionTechrepIntensity
        : IBiorepIntensity
    {
        public string condition { get; set; }
        public string biorep { get; set; }
        public string fraction { get; set; }
        public string techrep { get; set; }
        public bool imputed { get; set; }
        public double intensity_sum { get; set; }
        public BiorepFractionTechrepIntensity(string condition, string biorep, string fraction, string techrep, bool imputed, double intensity_sum)
        {
            this.biorep = biorep;
            this.fraction = fraction;
            this.techrep = techrep;
            this.condition = condition;
            this.imputed = imputed;
            this.intensity_sum = intensity_sum;
        }
    }
}
