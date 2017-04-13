using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using System.IO;
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
            Correction correction1 = new Correction("filename.txt", 1, Double.NaN);
            Correction correction2 = new Correction("filename.txt", 2, Double.NaN);
            Correction correction3 = new Correction("filename.txt", 3, 4D);
            Correction correction4 = new Correction("filename.txt", 4, Double.NaN);
            Correction correction5 = new Correction("filename.txt", 5, 6D);
            Correction correction6 = new Correction("filename.txt", 6, Double.NaN);
            List<Correction> correctionFactorTestList = new List<Correction> { correction1, correction2, correction3, correction4, correction5, correction6 };
            List<Correction> interpolated = Correction.CorrectionFactorInterpolation(correctionFactorTestList);

            foreach(Correction corr in interpolated)
            {
                Assert.AreEqual(Double.IsNaN(corr.correction), false);//checking to see that all values are defined.
            }

            Assert.AreEqual(interpolated.First().correction, 4D); // checking to see if the first undefined values were given the first defined value
            Assert.AreEqual(interpolated.Last().correction, 6D); //checing to see if undefined values at the end of the list are given last defined value
            Assert.AreEqual(interpolated.Where(s => s.scan_number == 4).First().correction, 5D);//checking to see that undefined values in the middle of the list are given average
        }

        [Test]
        public void correction_factors_into_chargestates()
        {
            string filename = "filename.txt";
            string scan_range = "1-3";
            Component c = new Component();
            c.input_file = new InputFile("fake.txt", Purpose.Identification);
            c.scan_range = scan_range;
            Correction correction1 = new Correction(filename, 1, Double.NaN);
            Correction correction2 = new Correction(filename, 2, Double.NaN);
            Correction correction3 = new Correction(filename, 3, 4D);
            List<Correction> corrections = Correction.CorrectionFactorInterpolation(new List<Correction> { correction1, correction2, correction3 });
            string charge_count = "1";
            string intensity = "1001.1";
            string mz_centroid = "123.2";
            string reported_mass = "125.0";
<<<<<<< HEAD
            c.add_charge_state(new List<string> { charge_count, intensity, mz_centroid, reported_mass });
=======
            c.add_charge_state(new List<string> { charge_count, intensity, mz_centroid, reported_mass }, Correction.GetCorrectionFactor(filename, scan_range, corrections));
>>>>>>> 510389744ba7efd6dc450972c0426a811f0261b4
        }

        //[Test]
        //public void basic_read_correction_factors()
        //{
        //    InputFile corr = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "example_corrections.tsv"), Purpose.Calibration);
        //    Assert.IsNotEmpty(Lollipop.read_corrections(corr));
        //}
    }
}
