using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public class BiorepIntensity
    {
        public bool imputed { get; set; } = false;
        public string biorep { get; set; }
        public string condition { get; set; }
        public List<BiorepFractionTechrepIntensity> summed_intensities { get; set; } = new List<BiorepFractionTechrepIntensity>();
        public double intensity { get; set; }// this should be linear intensity not log intensity

        public BiorepIntensity(bool imputed, string biorep, string condition, double intensity, List<BiorepFractionTechrepIntensity> summed_intensities)
        {
            this.imputed = imputed;
            this.biorep = biorep;
            this.condition = condition;
            this.intensity = intensity;// this should be linear intensity not log intensity
            this.summed_intensities = summed_intensities;
        }
    }
}
