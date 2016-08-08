using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class Correction
    {
        public string file_origin { get; set; }
        public int scan_number { get; set; }
        public double correction { get; set; }

        public List<Correction> CorrectionFactorInterpolation(IEnumerable<Correction> cFactors)
        {
            cFactors = cFactors.OrderBy(p => p.scan_number);
            double firstValue = (from s in cFactors
                                 where s.correction != Double.NaN
                                 select s).First().correction;
            foreach (Correction c in cFactors)
            {
                if(c.correction != Double.NaN)
                {
                    firstValue = c.correction;
                }
                else
                {
                    c.correction = firstValue;
                }
            }
            return new List<Correction> (cFactors);
        }

    }

    

}
