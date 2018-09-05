using NUnit.Framework;
using ProteoformSuiteInternal;
using System.IO;

namespace Test
{
    [TestFixture]
    class TestLoadAndRun
    {
        [Test]
        public void test_load_and_run()
        {
            //set parameters -- change some parameters to other than the current defaults... 
            Sweet.lollipop.max_intensity_ratio = 3.5m;
            Sweet.lollipop.min_intensity_ratio = 1.5m;
            Sweet.lollipop.max_lysine_ct = 21m;
            Sweet.lollipop.min_lysine_ct = 3m;
            Sweet.lollipop.retention_time_tolerance = 1;
            Sweet.lollipop.maximum_missed_monos = 0;
            Sweet.lollipop.mass_tolerance = 10;
            Sweet.lollipop.maximum_missed_lysines = 0;
            Sweet.lollipop.min_num_CS = 0;
            Sweet.lollipop.max_ptms = 0;
            Sweet.lollipop.no_mans_land_upperBound = 0.64;
            Sweet.lollipop.peak_width_base_ee = 0.001;
            Sweet.lollipop.ee_max_mass_difference = 100;
            Sweet.lollipop.ee_max_RetentionTime_difference = 10;

            //save method
            string saved_method = Sweet.save_method().ToString();

            //change parameters back to defaults
            Sweet.lollipop.min_intensity_ratio = 1.4m;
            Sweet.lollipop.max_intensity_ratio = 6m;
            Sweet.lollipop.min_lysine_ct = 1.5m;
            Sweet.lollipop.max_lysine_ct = 26.2m;
            Sweet.lollipop.min_num_CS = 1;
            Sweet.lollipop.max_ptms = 3;
            Sweet.lollipop.no_mans_land_upperBound = 0.88;
            Sweet.lollipop.ee_max_mass_difference = 250;
            Sweet.lollipop.ee_max_RetentionTime_difference = 2.5;
            Sweet.lollipop.peak_width_base_ee = 0.015;
            //load method --> should switch parametetrs to saved 
            Sweet.open_method(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), saved_method, false, out string warning);

            //tests that the method settings properly a) saved b)loaded up above
            Assert.AreEqual(3, Sweet.lollipop.min_lysine_ct);
            Assert.AreEqual(21, Sweet.lollipop.max_lysine_ct);
            Assert.AreEqual(1.5m, Sweet.lollipop.min_intensity_ratio);
            Assert.AreEqual(3.5m, Sweet.lollipop.max_intensity_ratio);
            Assert.AreEqual(10, Sweet.lollipop.mass_tolerance);
            Assert.AreEqual(1, Sweet.lollipop.retention_time_tolerance);
            Assert.AreEqual(0, Sweet.lollipop.maximum_missed_monos);
            Assert.AreEqual(0, Sweet.lollipop.maximum_missed_lysines);
            Assert.AreEqual(0, Sweet.lollipop.min_num_CS);
            Assert.AreEqual(0, Sweet.lollipop.max_ptms);
            Assert.AreEqual(0.64, Sweet.lollipop.no_mans_land_upperBound);
            Assert.AreEqual(100, Sweet.lollipop.ee_max_mass_difference);
            Assert.AreEqual(0.001, Sweet.lollipop.peak_width_base_ee);
            Assert.AreEqual(10, Sweet.lollipop.ee_max_RetentionTime_difference);
        }
    }
}

