using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using System.Text;
using System.Threading.Tasks;


namespace Test
{
    [TestFixture]
    public class TestCorrectionFactorInterpolation
    {

        [Test]
        public void testCorrectionFactorInterpolation()
        {
            Correction c = new Correction();

            List<Correction> correctionFactorTestList = new List<Correction>();

            Correction correction1 = new Correction();
            correction1.correction = Double.NaN;
            correction1.file_origin = "filename.txt";
            correction1.scan_number = 1;
            correctionFactorTestList.Add(correction1);

            Correction correction2 = new Correction();
            correction2.correction = Double.NaN;
            correction2.file_origin = "filename.txt";
            correction2.scan_number = 2;
            correctionFactorTestList.Add(correction2);

            Correction correction3 = new Correction();
            correction3.correction = 4D;
            correction3.file_origin = "filename.txt";
            correction3.scan_number = 3;
            correctionFactorTestList.Add(correction3);

            Correction correction4 = new Correction();
            correction4.correction = Double.NaN;
            correction4.file_origin = "filename.txt";
            correction4.scan_number = 4;
            correctionFactorTestList.Add(correction4);

            Correction correction5 = new Correction();
            correction5.correction = 6D;
            correction5.file_origin = "filename.txt";
            correction5.scan_number = 5;
            correctionFactorTestList.Add(correction5);

            Correction correction6 = new Correction();
            correction6.correction = Double.NaN;
            correction6.file_origin = "filename.txt";
            correction6.scan_number = 6;
            correctionFactorTestList.Add(correction6);

            correctionFactorTestList = c.CorrectionFactorInterpolation(correctionFactorTestList);

            foreach(Correction corr in correctionFactorTestList)
            {
                Assert.AreEqual(Double.IsNaN(corr.correction), false);//checking to see that all values are defined.
            }

            Assert.AreEqual(correctionFactorTestList.First().correction, 4D); // checking to see if the first undefined values were given the first defined value
            Assert.AreEqual(correctionFactorTestList.Last().correction, 6D); //checing to see if undefined values at the end of the list are given last defined value
            Assert.AreEqual((from s in correctionFactorTestList where s.scan_number == 4 select s).First().correction, 5D);//checking to see that undefined values in the middle of the list are given average

        }
        
    }
}
