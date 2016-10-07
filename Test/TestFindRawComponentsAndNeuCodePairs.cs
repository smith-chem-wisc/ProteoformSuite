using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
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
            Lollipop.correctionFactors = null;
            Lollipop.raw_experimental_components.Clear();
            Func<InputFile, IEnumerable<Component>> componentReader = c => new ExcelReader().read_components_from_xlsx(c, Lollipop.correctionFactors);
            Lollipop.input_files.Add(new InputFile("UnitTestFiles\\noisy.xlsx", Labeling.NeuCode, Purpose.Identification));

            string inFileId = Lollipop.input_files[0].UniqueId.ToString();

            Lollipop.neucode_labeled = true;
            Lollipop.process_raw_components();
            Assert.AreEqual(224, Lollipop.raw_experimental_components.Count);

            //Check the validity of one component read from the Excel file
            Component c1 = Lollipop.raw_experimental_components[0];
            Component c2 = Lollipop.raw_experimental_components[6]; // this one doesn't have the same number of charge states, which makes for a good example
            List<int> c1_charges = c1.charge_states.Select(charge_state => charge_state.charge_count).ToList<int>();
            List<int> c2_charges = c2.charge_states.Select(charge_states => charge_states.charge_count).ToList<int>();
            List<int> overlapping_charge_states = c1_charges.Intersect(c2_charges).ToList();
            Assert.AreEqual(9, c1.charge_states.Count);
            Assert.AreEqual(8, overlapping_charge_states.Count);
            Assert.AreEqual("8_1", c1.id);
            Assert.AreEqual(Math.Round(8982.7258, 4), Math.Round(c1.monoisotopic_mass, 4));
            Assert.AreEqual(Math.Round(32361626.3, 1), Math.Round(c1.intensity_sum, 1));
            Assert.AreEqual(Math.Round(32135853.39, 2), Math.Round(c1.calculate_sum_intensity(overlapping_charge_states), 2));
            Assert.AreEqual(9, c1.num_charge_states_fromFile);
            Assert.AreEqual(Math.Round(2127.5113, 4), Math.Round(c1.delta_mass, 4));
            Assert.AreEqual(Math.Round(54.97795307, 8), Math.Round(c1.relative_abundance, 8));
            Assert.AreEqual(Math.Round(1.141297566, 8), Math.Round(c1.fract_abundance, 8));
            Assert.AreEqual("413-415", c1.scan_range);
            Assert.AreEqual("56.250-56.510", c1.rt_range);
            Assert.AreEqual(Math.Round(56.3809775, 7), Math.Round(c1.rt_apex, 7));

            //testing intensity ratio
            List<NeuCodePair> neucode_pair = Lollipop.raw_neucode_pairs.Where(i => i.id_heavy == inFileId + "_5" && i.id_light == inFileId + "_1").ToList();
            Assert.AreEqual(2.0595679693624596, neucode_pair[0].intensity_ratio);

            //testing K-count
            Assert.AreEqual(7, neucode_pair[0].lysine_count);

            //testing that only overlapping charge states go into intensity ratio
            neucode_pair = Lollipop.raw_neucode_pairs.Where(i => i.id_heavy == inFileId + "_122" && i.id_light == inFileId + "_57").ToList();
            Assert.AreEqual(1.7231604062234347, neucode_pair[0].intensity_ratio);

            //testing that if Neucode-light is "heavier", K value still correctly calculated  //Not really allowed anymore
            //neucode_pair = Lollipop.raw_neucode_pairs.Where(i => i.id_heavy == inFileId + "_217" && i.id_light == inFileId + "_218").ToList();
            //Assert.AreEqual(15, neucode_pair[0].lysine_count);

            //test that pair w/ out of bounds I-ratio is marked unaccepted //Not really allowed anymore
            //neucode_pair = Lollipop.raw_neucode_pairs.Where(i => i.id_heavy == inFileId + "_222" && i.id_light == inFileId + "_221").ToList();
            //Assert.AreEqual(false, neucode_pair[0].accepted);

            ////test that pair w/ out of bounds K-count is marked unaccepted
            //neucode_pair = Lollipop.raw_neucode_pairs.Where(i => i.id_heavy == inFileId + "_224" && i.id_light == inFileId + "_223").ToList();
            //Assert.AreEqual(false, neucode_pair[0].accepted);
        }

    }
}
