﻿using NUnit.Framework;
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
            Sweet.lollipop = new Lollipop();
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
            HashSet<int> overlapping_charge_states = new HashSet<int>(c1_charges.Intersect(c2_charges));
            Assert.AreEqual(9, c1.charge_states.Count);
            Assert.AreEqual(8, overlapping_charge_states.Count);
            Assert.AreEqual(Sweet.lollipop.input_files.Where(f => f.filename == "noisy").FirstOrDefault().UniqueId + "_1", c1.id); //this line behaving strangely.
            Assert.AreEqual(Math.Round(8982.7258, 4), Math.Round(c1.reported_monoisotopic_mass, 4));
            Assert.AreEqual(Math.Round(2868299.6, 1), Math.Round(c1.intensity_sum, 1)); //charge state normalized
            Assert.AreEqual(Math.Round(2836046.31, 2), Math.Round(NeuCodePair.calculate_sum_intensity_olcs(c1.charge_states, overlapping_charge_states), 2));
            Assert.AreEqual(9, c1.charge_states.Count);
            Assert.AreEqual("413-415", c1.min_scan + "-" + c1.max_scan);
            Assert.AreEqual("56.25-56.51", c1.min_rt + "-" + c1.max_rt);
            Assert.AreEqual(Math.Round(56.3809775, 7), Math.Round(c1.rt_apex, 7));
            Assert.AreEqual(8981.69, Math.Round(c1.charge_states.OrderBy(s => s.charge_count).First().reported_mass, 2));

            //testing intensity ratio
            NeuCodePair neucode_pair = Sweet.lollipop.raw_neucode_pairs.Where(i => i.neuCodeHeavy.id == inFileId + "_5" && i.neuCodeLight.id == inFileId + "_1").First();
            Assert.AreEqual(2.0695212171812423d, neucode_pair.intensity_ratio); //intensities are charge state normalized

            //testing K-count
            Assert.AreEqual(7, neucode_pair.lysine_count);

            //testing that only overlapping charge states go into intensity ratio
            neucode_pair = Sweet.lollipop.raw_neucode_pairs.Where(i => i.neuCodeHeavy.id == inFileId + "_122" && i.neuCodeLight.id == inFileId + "_57").First();
            Assert.AreEqual(1.7301034510740836, neucode_pair.intensity_ratio);
        }

        [Test]
        public void testComponentReaderClear()
        {
            Sweet.lollipop = new Lollipop();
            Func<InputFile, IEnumerable<Component>> componentReader = c => new ComponentReader().read_components_from_xlsx(c, true);
            InputFile noisy = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "noisy.xlsx"), Labeling.NeuCode, Purpose.Identification);
            Sweet.lollipop.input_files.Add(noisy);

            string inFileId = noisy.UniqueId.ToString();

            Sweet.lollipop.neucode_labeled = true;
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true);
            Assert.AreEqual(223, Sweet.lollipop.raw_experimental_components.Count);
            Assert.AreEqual(223, noisy.reader.final_components.Count());
            Assert.AreEqual(68, noisy.reader.scan_ranges.Count());
            Assert.AreEqual(224, noisy.reader.unprocessed_components);
            Assert.AreEqual(1, noisy.reader.missed_mono_merges);
            Assert.AreEqual(0, noisy.reader.harmonic_merges);
            noisy.reader.Clear();
            Assert.AreEqual(0, noisy.reader.final_components.Count());
            Assert.AreEqual(0, noisy.reader.scan_ranges.Count());
            Assert.AreEqual(0, noisy.reader.unprocessed_components);
            Assert.AreEqual(0, noisy.reader.missed_mono_merges);
            Assert.AreEqual(0, noisy.reader.harmonic_merges);
        }

        [Test]
        public void testBadValuesComponentFile()
        {
            Sweet.lollipop = new Lollipop();
            InputFile badValues = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "badValues1.xlsx"), Labeling.NeuCode, Purpose.Identification);
            Sweet.lollipop.input_files.Add(badValues);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true);

            Assert.AreEqual(0, Sweet.lollipop.raw_experimental_components.Count);
            Assert.AreEqual(0, badValues.reader.final_components.Count());
            Assert.AreEqual(1, ComponentReader.components_with_errors.Count);
            Assert.AreEqual("badValues1 component 1", ComponentReader.components_with_errors.First());

            Sweet.lollipop = new Lollipop();
            badValues = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "badValues2.xlsx"), Labeling.NeuCode, Purpose.Identification);
            Sweet.lollipop.input_files.Add(badValues);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true);

            Assert.AreEqual(0, Sweet.lollipop.raw_experimental_components.Count);
            Assert.AreEqual(0, badValues.reader.final_components.Count());
            Assert.AreEqual(1, ComponentReader.components_with_errors.Count);

            Assert.AreEqual("badValues2 component 2", ComponentReader.components_with_errors.First());
        }

        [Test]
        public void testBadPromexComponents()
        {
            Sweet.lollipop = new Lollipop();
            InputFile badValues = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_bad1.tsv"), Labeling.Unlabeled, Purpose.Identification);
            Sweet.lollipop.input_files.Add(badValues);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true);
            Assert.AreEqual(0, Sweet.lollipop.raw_experimental_components.Count);

            Sweet.lollipop = new Lollipop();
            badValues = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_bad2.tsv"), Labeling.Unlabeled, Purpose.Identification);
            Sweet.lollipop.input_files.Add(badValues);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true);
            Assert.AreEqual(0, Sweet.lollipop.raw_experimental_components.Count);
            Assert.AreEqual(0, badValues.reader.final_components.Count());
            Assert.AreEqual(1, ComponentReader.components_with_errors.Count);
        }

        [Test]
        public void testThreeColumnDeconInput()
        {
            //good
            Sweet.lollipop = new Lollipop();
            var values = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_3columns.tsv"), Labeling.Unlabeled, Purpose.Identification);
            Sweet.lollipop.input_files.Add(values);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, false);
            Assert.AreEqual(204, Sweet.lollipop.raw_experimental_components.Count);
            Assert.AreEqual(204, values.reader.final_components.Count());
            Assert.AreEqual(0, ComponentReader.components_with_errors.Count);

            //bad
            Sweet.lollipop = new Lollipop();
            InputFile badValues = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_bad1_3columns.tsv"), Labeling.Unlabeled, Purpose.Identification);
            Sweet.lollipop.input_files.Add(badValues);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true);
            Assert.AreEqual(0, Sweet.lollipop.raw_experimental_components.Count);

            Sweet.lollipop = new Lollipop();
            badValues = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_bad2_3columns.tsv"), Labeling.Unlabeled, Purpose.Identification);
            Sweet.lollipop.input_files.Add(badValues);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true);
            Assert.AreEqual(2, Sweet.lollipop.raw_experimental_components.Count);
            Assert.AreEqual(2, badValues.reader.final_components.Count());
            Assert.AreEqual(1, ComponentReader.components_with_errors.Count);
        }
    }
}