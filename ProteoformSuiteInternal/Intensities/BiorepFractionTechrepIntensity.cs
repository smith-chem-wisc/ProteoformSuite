namespace ProteoformSuiteInternal
{
    public class BiorepFractionTechrepIntensity
        : IBiorepIntensity, IFileIntensity
    {
        public InputFile input_file { get; set; }
        public string condition { get; set; }
        public string biorep { get { return input_file.biological_replicate; } }
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
