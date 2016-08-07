using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProteoformSuiteInternal;

namespace Test
{
    [TestFixture]
    public class TestSaveAndLoadResults
    {
        [Test]
        public void test_component_result_io()
        {
            Component c1 = new Component();
            Component c2 = new Component();
            c1.id = 1;
            c2.id = 2;
            c1.monoisotopic_mass = 1.0;
            c2.monoisotopic_mass = 2.0;
            c1.weighted_monoisotopic_mass = 1.1;
            c2.weighted_monoisotopic_mass = 2.1;
            c1.corrected_mass = 1.2;
            c2.corrected_mass = 2.2;
            c1.intensity_sum = 100.0;
            c2.intensity_sum = 200.0;
            c1.num_charge_states_fromFile = 5;
            c2.num_charge_states_fromFile = 6;
            c1.delta_mass = 10.0;
            c2.delta_mass = 20.0;
            c1.relative_abundance = 0.5;
            c2.relative_abundance = 0.6;
            c1.fract_abundance = 0.55;
            c2.fract_abundance = 0.66;
            c1.scan_range = "100-102";
            c2.scan_range = "200-202";
            c1.rt_range = "60-61";
            c2.rt_range = "62-63";
            c1.rt_apex = 60.5;
            c2.rt_apex = 62.5;
            c1.intensity_sum_olcs = 99.9;
            c2.intensity_sum_olcs = 199.9;
            c1.file_origin = "file1";
            c2.file_origin = "file2";
            c1.accepted = true;
            c2.accepted = false;
            Lollipop.raw_experimental_components = new List<Component> { c1, c2 };
            string[] component_results = Results.raw_component_results().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string[] component_strings = new string[2] { component_results[1], component_results[2] };
            Lollipop.raw_experimental_components.Clear();
            Results.read_raw_components(component_strings);
            Component d1 = Lollipop.raw_experimental_components[0];
            Component d2 = Lollipop.raw_experimental_components[1];
            Assert.AreEqual(c1.id, d1.id);
            Assert.AreEqual(c1.monoisotopic_mass, d1.monoisotopic_mass);
            Assert.AreEqual(c1.weighted_monoisotopic_mass, d1.weighted_monoisotopic_mass);
            Assert.AreEqual(c1.corrected_mass, d1.corrected_mass);
            Assert.AreEqual(c1.intensity_sum, d1.intensity_sum);
            Assert.AreEqual(c1.num_charge_states_fromFile, d1.num_charge_states_fromFile);
            Assert.AreEqual(c1.delta_mass, d1.delta_mass);
            Assert.AreEqual(c1.relative_abundance, d1.relative_abundance);
            Assert.AreEqual(c1.fract_abundance, d1.fract_abundance);
            Assert.AreEqual(c1.scan_range, d1.scan_range);
            Assert.AreEqual(c1.rt_range, d1.rt_range);
            Assert.AreEqual(c1.rt_apex, d1.rt_apex);
            Assert.AreEqual(c1.intensity_sum_olcs, d1.intensity_sum_olcs);
            Assert.AreEqual(c1.file_origin, d1.file_origin);
            Assert.AreEqual(c1.accepted, d1.accepted);
        }
    }
}
