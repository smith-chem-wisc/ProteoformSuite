using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class Envelope
    {
        public int scan_num { get; set; }
        public int charge { get; set; }
        public double abundance { get; set; }
        public double mz { get; set; }
        public double fit { get; set; }
        public double monoisotopicMass { get; set; }
        public int FeatureID { get; set; }
        public double retentionTime { get; set; }

        public Envelope(string line)
        {            
                var split = line.Split(',');

                scan_num = int.Parse(split[0]);
                charge = int.Parse(split[1]);
                abundance = double.Parse(split[2]);
                mz = double.Parse(split[3]);
                fit = double.Parse(split[4]);
                monoisotopicMass = double.Parse(split[5]);
                FeatureID = int.Parse(split[6]);            
        }
    }
}
