using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Lollipop.max_intensity_ratio = 3.5m;
            Lollipop.min_intensity_ratio = 1.5m;
            Lollipop.max_lysine_ct = 21m;
            Lollipop.min_lysine_ct = 3m;
            Lollipop.retention_time_tolerance = 1;
            Lollipop.missed_monos = 0;
            Lollipop.mass_tolerance = 10;
            Lollipop.missed_lysines = 0;
            Lollipop.min_rel_abundance = 8;
            Lollipop.min_num_CS = 0;
            Lollipop.min_agg_count = 2;
            Lollipop.max_ptms = 0;
            Lollipop.no_mans_land_upperBound = 0.64;
            Lollipop.peak_width_base_ee = 0.001;
            Lollipop.ee_max_mass_difference = 100;
            Lollipop.ee_max_RetentionTime_difference = 10;

            //save method
            string saved_method = Lollipop.method_toString();

            //change parameters back to defaults
            Lollipop.min_intensity_ratio = 1.4m;
            Lollipop.max_intensity_ratio = 6m;
            Lollipop.min_lysine_ct = 1.5m;
            Lollipop.max_lysine_ct = 26.2m;
            Lollipop.min_rel_abundance = 0;
            Lollipop.min_num_CS = 1;
            Lollipop.min_agg_count = 1;
            Lollipop.max_ptms = 3;
            Lollipop.no_mans_land_upperBound = 0.88;
            Lollipop.ee_max_mass_difference = 250;
            Lollipop.ee_max_RetentionTime_difference = 2.5;
            Lollipop.peak_width_base_ee = 0.015;
            //load method --> should switch parametetrs to saved 
            foreach (string setting_spec in saved_method.Split('\n'))
            {
                Lollipop.load_setting(setting_spec.Trim());
            }

            //tests that the method settings properly a) saved b)loaded up above
            Assert.AreEqual(3, Lollipop.min_lysine_ct);
            Assert.AreEqual(21, Lollipop.max_lysine_ct);
            Assert.AreEqual(1.5m, Lollipop.min_intensity_ratio);
            Assert.AreEqual(3.5m, Lollipop.max_intensity_ratio);
            Assert.AreEqual(10, Lollipop.mass_tolerance);
            Assert.AreEqual(1, Lollipop.retention_time_tolerance);
            Assert.AreEqual(0, Lollipop.missed_monos);
            Assert.AreEqual(0, Lollipop.missed_lysines);
            Assert.AreEqual(8, Lollipop.min_rel_abundance);
            Assert.AreEqual(0, Lollipop.min_num_CS);
            Assert.AreEqual(2, Lollipop.min_agg_count);
            Assert.AreEqual(0, Lollipop.max_ptms);
            Assert.AreEqual(0.64, Lollipop.no_mans_land_upperBound);
            Assert.AreEqual(100, Lollipop.ee_max_mass_difference);
            Assert.AreEqual(0.001, Lollipop.peak_width_base_ee);
            Assert.AreEqual(10, Lollipop.ee_max_RetentionTime_difference);
        }

    }
}

