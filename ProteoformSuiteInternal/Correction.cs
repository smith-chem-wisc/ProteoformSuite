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
            //double firstValue = (from s in cFactors
            //                     where s.correction != Double.NaN
            //                     select s).First().correction;
            
            Correction corrInHand = cFactors.First();

            double lowCorrection = 0;

            //Find the first defined correction factor.
            if (Double.IsNaN(corrInHand.correction))
            {
                Correction someCorrection = GetNext(cFactors, corrInHand);
                while (Double.IsNaN(someCorrection.correction))
                {
                    someCorrection = GetNext(cFactors, someCorrection);
                }
                lowCorrection = someCorrection.correction;
            }
            else
            {
                lowCorrection = corrInHand.correction;
            }

            //copy the first defined correctin factor to all undefinded correction factors at the head of the list.
            while (Double.IsNaN(corrInHand.correction))
            {
                corrInHand.correction = lowCorrection;
                corrInHand = GetNext(cFactors, corrInHand);
            }

            //pointer is to the first defined correction factor
            double hiCorrection = 0;

            while (corrInHand != cFactors.Last())
            {
                if (!Double.IsNaN(corrInHand.correction)) // has defined value
                {
                    lowCorrection = corrInHand.correction;
                    corrInHand = GetNext(cFactors, corrInHand);
                }
                else // has undefined value. we have to find the next defined value
                {
                    Correction nextCorr = GetNext(cFactors, corrInHand);
                    while (Double.IsNaN(nextCorr.correction) && nextCorr != cFactors.Last())
                    {
                        nextCorr = GetNext(cFactors, nextCorr);
                    }
                    // we either found one that is defined or we are at the end of the line
                    if (!Double.IsNaN(nextCorr.correction))//value is defined
                    {
                        hiCorrection = nextCorr.correction;
                        corrInHand.correction = (lowCorrection + hiCorrection) / 2;
                        corrInHand = GetNext(cFactors, corrInHand);
                    }
                    else //we ran out of real estate. no defined value after current
                    {
                        corrInHand.correction = lowCorrection;
                        corrInHand = GetNext(cFactors, corrInHand);
                    }
                }
            }
            //Do something with the last one
            if(Double.IsNaN(corrInHand.correction))//if undefined
            {
                corrInHand.correction = lowCorrection;
            }

            return new List<Correction>(cFactors);
        }

        private Correction GetNext(IEnumerable<Correction> correctionList, Correction currentCorrection)
        {
            return correctionList.SkipWhile(x => !x.Equals(currentCorrection)).Skip(1).First();
        }

        private Correction GetPrevious(IEnumerable<Correction> correctionList, Correction currentCorrection)
        {
            return correctionList.TakeWhile(x => !x.Equals(currentCorrection)).Last();
        }

    }

    

}
