using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class BiorepIntensity
    {
        public bool light { get; set; } = true; // true if unlabelled or neucode light; false if neucode heavy
        public bool imputed { get; set; } = false;
        public int biorep { get; set; }
        public string condition { get; set; }
        public double intensity { get; set; }// this should be linear intensity not log intensity

        public BiorepIntensity(bool light, bool imputed, int biorep, string condition, double intensity)
        {
            this.light = light;
            this.imputed = imputed;
            this.biorep = biorep;
            this.condition = condition;
            this.intensity = intensity;// this should be linear intensity not log intensity
        }
    }

    //public class bftIntensity
    //{        
    //    public bool light { get; set; } = true; // true if unlabelled or neucode light; false if neucode heavy
    //    public int biorep { get; set; }
    //    public int fraction { get; set; }
    //    public int techrep { get; set; }
    //    public double intensity { get; set; }
    //    public bftIntensity(bool light, int biorep, int fraction, int techrep, double intensity)
    //    {
    //        this.light = light;
    //        this.biorep = biorep;
    //        this.fraction = fraction;
    //        this.techrep = techrep;
    //        this.intensity = intensity;
    //    }
    //}
}
