using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class Correction
    {
        public string file_name { get; set; }
        public int scan_number { get; set; }
        public double correction { get; set; }

        public Correction(string file_name, int scan_number, double correction)
        {
            this.file_name = file_name;
            this.scan_number = scan_number;
            this.correction = correction;
        }

        public static List<Correction> CorrectionFactorInterpolation(IEnumerable<Correction> cFactors)
        {
            List<Correction> correction_factors = cFactors.OrderBy(p => p.scan_number).ToList();
            bool found_first_defined = false; 
            double recent_correction = 0;
            List<Correction> undefined_corrections = new List<Correction>();
            for (int i = 0; i < correction_factors.Count; i++)
            {
                Correction this_correction = correction_factors[i];
                if (Double.IsNaN(this_correction.correction)) undefined_corrections.Add(this_correction); // Add all undefined corrections to a list for interpolating later
                else if (!found_first_defined) //Found the first defined correction factor.
                {
                    //Copy the first defined correction factor to all undefined correction factors at the head of the list.
                    found_first_defined = true;
                    foreach (Correction c in undefined_corrections) c.correction = this_correction.correction;
                    recent_correction = this_correction.correction;
                    undefined_corrections.Clear();
                }
                else
                {
                    //Average this defined correction value and the last one observed, and copy this to each recently seen undefined correction.
                    double correction_value = (recent_correction + this_correction.correction) / 2;
                    foreach (Correction c in undefined_corrections) c.correction = correction_value;
                    recent_correction = this_correction.correction;
                    undefined_corrections.Clear();
                }
            }
            //We ran out of real estate. Let's do something with any remaining undefined corrections.
            foreach (Correction c in undefined_corrections) c.correction = recent_correction;
            return correction_factors;
        }
    }
}
