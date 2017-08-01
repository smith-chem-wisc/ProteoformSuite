namespace ProteoformSuiteInternal
{
    public class BiorepIntensity :
        IBiorepIntensity
    {
        public bool imputed { get; set; } = false;
        public string biorep { get; set; }
        public string condition { get; set; }
        public double intensity_sum { get; set; } // this should be linear intensity not log intensity

        public BiorepIntensity(bool imputed, string biorep, string condition, double intensity_sum)
        {
            this.imputed = imputed;
            this.biorep = biorep;
            this.condition = condition;
            this.intensity_sum = intensity_sum;// this should be linear intensity not log intensity
        }
    }
}
