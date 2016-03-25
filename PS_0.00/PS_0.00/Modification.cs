using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Adapted from the class by the same name from Morpheus (http://cwenger.github.io/Morpheus) by Craig Wenger
namespace PS_0._00
{
    class Modification
    {
        // unused but available public string pA, cF, lC, tR, kW, dR;

        public string Description { get; set; } //ID
        public string Accession { get; set; } //AC
        public string FeatureType { get; set; } //FT
        public string Position { get; set; } //PP
        public char[] TargetAAs { get; set; } //TG
        public double MonoisotopicMassShift { get; set; } //MM
        public double AverageMassShift { get; set; } //MA

        public Modification(string description, string accession, string featureType, 
            string position, char[] targetAAs, double monoisotopicMassShift, double averageMassShift)
        {
            this.Description = description;
            this.Accession = accession;
            this.FeatureType = featureType;
            this.Position = position;
            this.TargetAAs = targetAAs;
            this.MonoisotopicMassShift = monoisotopicMassShift;
            this.AverageMassShift = averageMassShift;
        }

        public override string ToString()
        {
            return "Description=" + this.Description + " Accession=" + this.Accession + 
                " FeatureType=" + this.FeatureType + " MonisotopicMass=" + this.MonoisotopicMassShift;
        }
    }
}
