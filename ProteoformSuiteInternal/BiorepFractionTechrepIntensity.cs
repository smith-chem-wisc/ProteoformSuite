namespace ProteoformSuiteInternal
{
    public class BiorepFractionTechrepIntensity
    {
        public InputFile input_file { get; set; }
        public string condition { get; set; }
        public bool imputed { get; set; }
        public double intensity_sum { get; set; }
        public BiorepFractionTechrepIntensity(InputFile file, string condition, bool imputed, double intensity_sum)
        {
            this.input_file = file;
            this.condition = condition;
            this.imputed = imputed;
            this.intensity_sum = intensity_sum;
        }
    }
}
