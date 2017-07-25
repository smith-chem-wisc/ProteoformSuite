using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class TestFindRawComponentsAndNeuCodePairs
    {
        [OneTimeSetUp]
        public void setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        [Test]
        public void testFindRawNeuCodePairsMethod()
        {
            //reading in test excel file, process raw components before testing neucode pairs.
            Sweet.lollipop.raw_experimental_components.Clear();
            Func<InputFile, IEnumerable<Component>> componentReader = c => new ComponentReader().read_components_from_xlsx(c, true);
            InputFile noisy = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "noisy.xlsx"), Labeling.NeuCode, Purpose.Identification);
            Sweet.lollipop.input_files.Add(noisy);

            string inFileId = noisy.UniqueId.ToString();

            Sweet.lollipop.neucode_labeled = true;
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true);
            Assert.AreEqual(223, Sweet.lollipop.raw_experimental_components.Count);

            //Check the validity of one component read from the Excel file
            Component c1 = Sweet.lollipop.raw_experimental_components[0];
            Component c2 = Sweet.lollipop.raw_experimental_components[6]; // this one doesn't have the same number of charge states, which makes for a good example
            List<int> c1_charges = c1.charge_states.Select(charge_state => charge_state.charge_count).ToList();
            List<int> c2_charges = c2.charge_states.Select(charge_states => charge_states.charge_count).ToList();
            List<int> overlapping_charge_states = c1_charges.Intersect(c2_charges).ToList();
            Assert.AreEqual(9, c1.charge_states.Count);
            Assert.AreEqual(8, overlapping_charge_states.Count);
            Assert.AreEqual(Sweet.lollipop.input_files.Where(f => f.filename == "noisy").FirstOrDefault().UniqueId + "_1", c1.id); //this line behaving strangely.
            Assert.AreEqual(Math.Round(8982.7258, 4), Math.Round(c1.reported_monoisotopic_mass, 4));
            Assert.AreEqual(Math.Round(32361626.3, 1), Math.Round(c1.intensity_sum, 1));
            c1.calculate_sum_intensity_olcs(overlapping_charge_states);
            Assert.AreEqual(Math.Round(32135853.39, 2), Math.Round(c1.intensity_sum_olcs, 2));
            Assert.AreEqual(9, c1.num_charge_states);
            Assert.AreEqual(Math.Round(2127.5113, 4), Math.Round(c1.delta_mass, 4));
            Assert.AreEqual(Math.Round(54.97795307, 8), Math.Round(c1.relative_abundance, 8));
            Assert.AreEqual(Math.Round(1.141297566, 8), Math.Round(c1.fract_abundance, 8));
            Assert.AreEqual("413-415", c1.scan_range);
            Assert.AreEqual("56.250-56.510", c1.rt_range);
            Assert.AreEqual(Math.Round(56.3809775, 7), Math.Round(c1.rt_apex, 7));

            //testing intensity ratio
            NeuCodePair neucode_pair = Sweet.lollipop.raw_neucode_pairs.Where(i => i.neuCodeHeavy.id == inFileId + "_5" && i.neuCodeLight.id == inFileId + "_1").First();
            Assert.AreEqual(2.0595679693624596, neucode_pair.intensity_ratio);

            //testing K-count
            Assert.AreEqual(7, neucode_pair.lysine_count);

            //testing that only overlapping charge states go into intensity ratio
            neucode_pair = Sweet.lollipop.raw_neucode_pairs.Where(i => i.neuCodeHeavy.id == inFileId + "_122" && i.neuCodeLight.id == inFileId + "_57").First();
            Assert.AreEqual(1.7231604062234347, neucode_pair.intensity_ratio);
        }

    }
}
