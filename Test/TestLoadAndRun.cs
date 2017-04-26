using NUnit.Framework;
using ProteoformSuiteInternal;

namespace Test
{
    [TestFixture]
    class TestLoadAndRun
    {
        [Test]
        public void test_load_and_run()
        {
            //set parameters -- change some parameters to other than the current defaults... 
            SaveState.lollipop.max_intensity_ratio = 3.5m;
            SaveState.lollipop.min_intensity_ratio = 1.5m;
            SaveState.lollipop.max_lysine_ct = 21m;
            SaveState.lollipop.min_lysine_ct = 3m;
            SaveState.lollipop.retention_time_tolerance = 1;
            SaveState.lollipop.missed_monos = 0;
            SaveState.lollipop.mass_tolerance = 10;
            SaveState.lollipop.missed_lysines = 0;
            SaveState.lollipop.min_num_CS = 0;
            SaveState.lollipop.min_agg_count = 2;
            SaveState.lollipop.max_ptms = 0;
            SaveState.lollipop.no_mans_land_upperBound = 0.64;
            SaveState.lollipop.peak_width_base_ee = 0.001;
            SaveState.lollipop.ee_max_mass_difference = 100;
            SaveState.lollipop.ee_max_RetentionTime_difference = 10;

            //save method
            string saved_method = SaveState.save_method().ToString();

            //change parameters back to defaults
            SaveState.lollipop.min_intensity_ratio = 1.4m;
            SaveState.lollipop.max_intensity_ratio = 6m;
            SaveState.lollipop.min_lysine_ct = 1.5m;
            SaveState.lollipop.max_lysine_ct = 26.2m;
            SaveState.lollipop.min_num_CS = 1;
            SaveState.lollipop.min_agg_count = 1;
            SaveState.lollipop.max_ptms = 3;
            SaveState.lollipop.no_mans_land_upperBound = 0.88;
            SaveState.lollipop.ee_max_mass_difference = 250;
            SaveState.lollipop.ee_max_RetentionTime_difference = 2.5;
            SaveState.lollipop.peak_width_base_ee = 0.015;
            //load method --> should switch parametetrs to saved 
            SaveState.open_method(saved_method);

            //tests that the method settings properly a) saved b)loaded up above
            Assert.AreEqual(3, SaveState.lollipop.min_lysine_ct);
            Assert.AreEqual(21, SaveState.lollipop.max_lysine_ct);
            Assert.AreEqual(1.5m, SaveState.lollipop.min_intensity_ratio);
            Assert.AreEqual(3.5m, SaveState.lollipop.max_intensity_ratio);
            Assert.AreEqual(10, SaveState.lollipop.mass_tolerance);
            Assert.AreEqual(1, SaveState.lollipop.retention_time_tolerance);
            Assert.AreEqual(0, SaveState.lollipop.missed_monos);
            Assert.AreEqual(0, SaveState.lollipop.missed_lysines);
            Assert.AreEqual(0, SaveState.lollipop.min_num_CS);
            Assert.AreEqual(2, SaveState.lollipop.min_agg_count);
            Assert.AreEqual(0, SaveState.lollipop.max_ptms);
            Assert.AreEqual(0.64, SaveState.lollipop.no_mans_land_upperBound);
            Assert.AreEqual(100, SaveState.lollipop.ee_max_mass_difference);
            Assert.AreEqual(0.001, SaveState.lollipop.peak_width_base_ee);
            Assert.AreEqual(10, SaveState.lollipop.ee_max_RetentionTime_difference);
        }
    }
}

