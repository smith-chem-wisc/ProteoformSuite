using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class TestDeltaMassPeak
    {

        [OneTimeSetUp]
        public void setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        [Test]
        public void TestDeltaMassPeakConstructor()
        {
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            ConstructorsForTesting.read_mods();
            Sweet.lollipop.et_high_mass_difference = 250;
            Sweet.lollipop.et_low_mass_difference = -250;
            Sweet.lollipop.peak_width_base_ee = 0.015;
            Sweet.lollipop.peak_width_base_et = 0.015;


            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("acession1");
            TheoreticalProteoform pf2 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type = ProteoformComparison.ExperimentalTheoretical;
            double delta_mass = 1 - 1e-7;

            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("acession3");
            TheoreticalProteoform pf4 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type2 = ProteoformComparison.ExperimentalTheoretical;
            double delta_mass2 = 1;

            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("acession5");
            TheoreticalProteoform pf6 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type3 = ProteoformComparison.ExperimentalTheoretical;
            double delta_mass3 = 1 + 1e-7;

            ExperimentalProteoform pf55 = ConstructorsForTesting.ExperimentalProteoform("acession5");
            TheoreticalProteoform pf65 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type35 = ProteoformComparison.ExperimentalTheoretical;
            double delta_mass35 = 1 + 2e-7;

            List<ProteoformRelation> theList = new List<ProteoformRelation>();

            theList.Add(new ProteoformRelation(pf1, pf2, relation_type, delta_mass, TestContext.CurrentContext.TestDirectory));
            theList.Add(new ProteoformRelation(pf3, pf4, relation_type2, delta_mass2, TestContext.CurrentContext.TestDirectory));
            theList.Add(new ProteoformRelation(pf5, pf6, relation_type3, delta_mass3, TestContext.CurrentContext.TestDirectory));
            theList.Add(new ProteoformRelation(pf55, pf65, relation_type35, delta_mass35, TestContext.CurrentContext.TestDirectory));

            ProteoformRelation base_relation = new ProteoformRelation(pf3, pf4, relation_type2, delta_mass2, TestContext.CurrentContext.TestDirectory);

            //base_relation.nearby_relations = base_relation.set_nearby_group(theList, theList.Select(r => r.InstanceId).ToList());
            Console.WriteLine("Creating deltaMassPeak");
            DeltaMassPeak deltaMassPeak = new DeltaMassPeak(base_relation, new HashSet<ProteoformRelation>(theList));
            Console.WriteLine("Created deltaMassPeak");


            Assert.AreEqual(0, deltaMassPeak.peak_group_fdr);
            Dictionary<string, List<ProteoformRelation>> decoy_relations = new Dictionary<string, List<ProteoformRelation>>();

            decoy_relations["decoyDatabase1"] = new List<ProteoformRelation>();

            ExperimentalProteoform pf7 = ConstructorsForTesting.ExperimentalProteoform("experimental1");
            TheoreticalProteoform pf8 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type4 = ProteoformComparison.ExperimentalDecoy;
            double delta_mass4 = 1;
            ProteoformRelation decoy_relation = new ProteoformRelation(pf7, pf8, relation_type4, delta_mass4, TestContext.CurrentContext.TestDirectory);

            decoy_relations["decoyDatabase1"].Add(decoy_relation);

            deltaMassPeak.calculate_fdr(decoy_relations);
            Assert.AreEqual(0.25, deltaMassPeak.peak_group_fdr); // 1 decoy database, (1 decoy relation, median=1), 4 target relations

            decoy_relations["decoyDatabase2"] = new List<ProteoformRelation>();
            decoy_relations["decoyDatabase2"].Add(decoy_relation);
            decoy_relations["decoyDatabase2"].Add(decoy_relation);

            deltaMassPeak.calculate_fdr(decoy_relations);
            Assert.AreEqual(0.375, deltaMassPeak.peak_group_fdr); // 2 decoy databases (1 & 2 decoy relations, median=1.5), 4 target relations
        }

        [Test]
        public void TestAcceptDeltaMassPeaks()
        {
            Sweet.lollipop = new Lollipop();
            ProteoformCommunity test_community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community = test_community;

            Sweet.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>>
            {
                { "unmodified", new List<Modification>() { ConstructorsForTesting.get_modWithMass("unmodified", 0) } }
            };

            //Testing the acceptance of peaks. The FDR is tested above, so I'm not going to work with that here.
            //Four proteoforms, three relations (linear), middle one isn't accepted; should give 2 families
            Sweet.lollipop.min_peak_count_ee = 2;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2");
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("E3");
            ExperimentalProteoform pf6 = ConstructorsForTesting.ExperimentalProteoform("E4");

            ProteoformComparison comparison34 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison45 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison56 = ProteoformComparison.ExperimentalExperimental;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, comparison34, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr3 = new ProteoformRelation(pf4, pf5, comparison45, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr4 = new ProteoformRelation(pf5, pf6, comparison56, 0, TestContext.CurrentContext.TestDirectory);

            //Test display strings
            Assert.AreEqual("E1", pr2.connected_proteoforms[0].accession);
            Assert.AreEqual("E2", pr2.connected_proteoforms[1].accession);
            pr2.RelationType = ProteoformComparison.ExperimentalExperimental;
            pr2.RelationType = ProteoformComparison.ExperimentalTheoretical;
            pr2.RelationType = ProteoformComparison.ExperimentalDecoy;
            pr2.RelationType = ProteoformComparison.ExperimentalFalse;
            pr2.RelationType = comparison34;

            List<ProteoformRelation> prs2 = new List<ProteoformRelation> { pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs2) pr.set_nearby_group(prs2, prs2.Select(r => r.InstanceId).ToList());
            Assert.AreEqual(3, pr2.nearby_relations_count);
            Assert.AreEqual(3, pr3.nearby_relations_count);
            Assert.AreEqual(3, pr4.nearby_relations_count);

            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet> { new PtmSet(new List<Ptm> { new Ptm(-1, ConstructorsForTesting.get_modWithMass("unmodified", 0)) }) };
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = Sweet.lollipop.theoretical_database.make_ptmset_dictionary();
            //auto accept set to false
            Sweet.lollipop.ee_accept_peaks_based_on_rank = false;
            Sweet.lollipop.ee_peaks = test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            Assert.AreEqual(1, Sweet.lollipop.ee_peaks.Count);
            DeltaMassPeak peak = Sweet.lollipop.ee_peaks[0];
            Assert.IsTrue(peak.Accepted);
            Assert.AreEqual(3, peak.grouped_relations.Count);
            Assert.AreEqual(3, pr2.peak.peak_relation_group_count);
            Assert.AreEqual(0, pr2.peak.DeltaMass);
            Assert.AreEqual("[unmodified]", peak.possiblePeakAssignments_string);

            //Test that the relations in the peak are added to each of the proteoforms referenced in the peak
            Assert.True(pf3.relationships.Contains(pr2));
            Assert.True(pf4.relationships.Contains(pr2) && pf4.relationships.Contains(pr3));
            Assert.True(pf5.relationships.Contains(pr3) && pf5.relationships.Contains(pr4));

            //autoaccept set to true, must be less than first quartile rank...
            Sweet.lollipop.clear_ee();
            Sweet.lollipop.mod_rank_first_quartile = 0;
            Sweet.lollipop.ee_accept_peaks_based_on_rank = true;
            Sweet.lollipop.ee_peaks = test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
             peak = Sweet.lollipop.ee_peaks[0];
            Assert.IsFalse(peak.Accepted);
            Assert.AreEqual(0, peak.possiblePeakAssignments.Min(p => p.ptm_rank_sum));

            Sweet.lollipop.clear_ee();
            Sweet.lollipop.mod_rank_first_quartile = 1;
            Sweet.lollipop.ee_accept_peaks_based_on_rank = true;
            Sweet.lollipop.ee_peaks = test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            peak = Sweet.lollipop.ee_peaks[0];
            Assert.IsTrue(peak.Accepted);
            Assert.AreEqual(0, peak.possiblePeakAssignments.Min(p => p.ptm_rank_sum));

            //test autoaccept for et relations
            foreach (var r in prs2) r.RelationType = ProteoformComparison.ExperimentalTheoretical;
            Sweet.lollipop.clear_ee();
            Sweet.lollipop.clear_et();
            Sweet.lollipop.mod_rank_first_quartile = 0;
            Sweet.lollipop.et_accept_peaks_based_on_rank = true;
            Sweet.lollipop.et_peaks = test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            peak = Sweet.lollipop.et_peaks[0];
            Assert.IsFalse(peak.Accepted);
            Assert.AreEqual(0, peak.possiblePeakAssignments.Min(p => p.ptm_rank_sum));

            Sweet.lollipop.clear_et();
            Sweet.lollipop.min_peak_count_et = 2;
            Sweet.lollipop.mod_rank_first_quartile = 1;
            Sweet.lollipop.et_accept_peaks_based_on_rank = true;
            Sweet.lollipop.et_peaks = test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            peak = Sweet.lollipop.et_peaks[0];
            Assert.IsTrue(peak.Accepted);
            Assert.AreEqual(0, peak.possiblePeakAssignments.Min(p => p.ptm_rank_sum));
        }

        [Test]
        public void wrong_relation_shifting()
        {
            ProteoformCommunity test_community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community = test_community;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2");
            ProteoformComparison wrong_comparison = ProteoformComparison.ExperimentalExperimental;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, wrong_comparison, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr3 = new ProteoformRelation(pf3, pf4, wrong_comparison, 0, TestContext.CurrentContext.TestDirectory);
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr2, pr3 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.InstanceId).ToList());
            test_community.accept_deltaMass_peaks(prs, new List<ProteoformRelation>());
            Assert.False(Sweet.lollipop.ee_peaks[0].shift_experimental_masses(1, true));
            Sweet.lollipop.clear_ee();
            Assert.AreEqual(0, Sweet.lollipop.ee_peaks.Count);
        }

        [Test]
        public void artificial_deltaMPeak()
        {
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>();
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2");
            ProteoformComparison comparison = ProteoformComparison.ExperimentalExperimental;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, comparison, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr3 = new ProteoformRelation(pf3, pf4, comparison, 0, TestContext.CurrentContext.TestDirectory);
            HashSet<ProteoformRelation> prs = new HashSet<ProteoformRelation> { pr2, pr3 };
            Assert.AreEqual(prs, new DeltaMassPeak(pr2, prs).grouped_relations);
        }

        [Test]
        public void shift_et_peak_neucode()
        {
            Sweet.lollipop = new Lollipop();
            ProteoformCommunity test_community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community = test_community;

            //Make a few experimental proteoforms
            List<IAggregatable> n1 = TestExperimentalProteoform.generate_neucode_components(100);
            List<IAggregatable> n2 = TestExperimentalProteoform.generate_neucode_components(100);
            List<IAggregatable> n3 = TestExperimentalProteoform.generate_neucode_components(200);
            List<IAggregatable> n4 = TestExperimentalProteoform.generate_neucode_components(200);
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1");
            pf1.aggregated = n1;
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("E2");
            pf2.aggregated = n2;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E3");
            pf3.aggregated = n3;
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E4");
            pf4.aggregated = n4;

            Sweet.lollipop.target_proteoform_community.experimental_proteoforms = new List<ExperimentalProteoform> { pf1, pf2, pf3, pf4 }.ToArray();

            //Connect them to theoreticals to form two peaks
            ProteoformComparison comparison14 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison25 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison36 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison47 = ProteoformComparison.ExperimentalTheoretical;
            TheoreticalProteoform pf5 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf6 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf7 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf8 = ConstructorsForTesting.make_a_theoretical();
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf5, comparison14, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr2 = new ProteoformRelation(pf2, pf6, comparison25, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr3 = new ProteoformRelation(pf3, pf7, comparison36, 1, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr4 = new ProteoformRelation(pf4, pf8, comparison47, 1, TestContext.CurrentContext.TestDirectory);
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1, pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.InstanceId).ToList());
            test_community.accept_deltaMass_peaks(prs, new List<ProteoformRelation>());
            Assert.AreEqual(2, Sweet.lollipop.et_peaks.Count);

            //Shift the peaks, which shifts all of the proteoforms
            DeltaMassPeak d2 = Sweet.lollipop.et_peaks[1];
            d2.shift_experimental_masses(-1, true);

            Assert.IsTrue(pf3.mass_shifted);
            Assert.IsTrue(pf4.mass_shifted);

            foreach (IAggregatable c in 
                n3.Select(n => (n as NeuCodePair).neuCodeLight).
                Concat(n4.Select(n => (n as NeuCodePair).neuCodeLight)))
            {
                Assert.AreEqual(-1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, (c as Component).manual_mass_shift);
                Assert.AreEqual(200 - 1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.weighted_monoisotopic_mass);
            }

            foreach (IAggregatable c in 
                n3.Select(n => (n as NeuCodePair).neuCodeHeavy).
                Concat(n4.Select(n => (n as NeuCodePair).neuCodeHeavy)))
            {
                Assert.AreEqual(-1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, (c as Component).manual_mass_shift);
                Assert.AreEqual(200 + TestExperimentalProteoform.starter_lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT - 1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.weighted_monoisotopic_mass);
            }

            Sweet.lollipop.clear_et();
            Assert.AreEqual(0, Sweet.lollipop.et_peaks.Count);
        }

        [Test]
        public void shift_et_peak_neucode_from_actions()
        {
            Sweet.lollipop = new Lollipop();
            ProteoformCommunity test_community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community = test_community;

            //Make a few experimental proteoforms
            List<IAggregatable> n1 = TestExperimentalProteoform.generate_neucode_components(100);
            List<IAggregatable> n2 = TestExperimentalProteoform.generate_neucode_components(100);
            List<IAggregatable> n3 = TestExperimentalProteoform.generate_neucode_components(200);
            List<IAggregatable> n4 = TestExperimentalProteoform.generate_neucode_components(200);
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1");
            pf1.aggregated = n1;
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("E2");
            pf2.aggregated = n2;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E3");
            pf3.aggregated = n3;
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E4");
            pf4.aggregated = n4;

            Sweet.lollipop.target_proteoform_community.experimental_proteoforms = new List<ExperimentalProteoform> { pf1, pf2, pf3, pf4 }.ToArray();

            //Connect them to theoreticals to form two peaks
            ProteoformComparison comparison14 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison25 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison36 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison47 = ProteoformComparison.ExperimentalTheoretical;
            TheoreticalProteoform pf5 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf6 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf7 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf8 = ConstructorsForTesting.make_a_theoretical();
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf5, comparison14, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr2 = new ProteoformRelation(pf2, pf6, comparison25, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr3 = new ProteoformRelation(pf3, pf7, comparison36, 1, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr4 = new ProteoformRelation(pf4, pf8, comparison47, 1, TestContext.CurrentContext.TestDirectory);
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1, pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.InstanceId).ToList());
            test_community.accept_deltaMass_peaks(prs, new List<ProteoformRelation>());
            Assert.AreEqual(2, Sweet.lollipop.et_peaks.Count);

            //Shift the peaks, which shifts all of the proteoforms
            DeltaMassPeak d2 = Sweet.lollipop.et_peaks[1];
            d2.mass_shifter = "-1";
            Sweet.shift_peak_action(d2);
            d2.mass_shifter = null;
            using (StreamWriter file = new StreamWriter(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml")))
                file.WriteLine(Sweet.save_method());
            Sweet.open_method(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), String.Join(Environment.NewLine, File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"))), true, out string warning);
            Sweet.mass_shifts_from_presets();
            d2.shift_experimental_masses(Convert.ToInt32(d2.mass_shifter), true);

            Assert.IsTrue(pf3.mass_shifted);
            Assert.IsTrue(pf4.mass_shifted);

            foreach (Component c in
                n3.OfType<NeuCodePair>().Select(n => n.neuCodeLight).
                Concat(n4.OfType<NeuCodePair>().Select(n => n.neuCodeLight)))
            {
                Assert.AreEqual(-1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.manual_mass_shift);
                Assert.AreEqual(200 - 1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.weighted_monoisotopic_mass);
            }

            foreach (Component c in
                n3.OfType<NeuCodePair>().Select(n => n.neuCodeHeavy).
                Concat(n4.OfType<NeuCodePair>().Select(n => n.neuCodeHeavy)))
            {
                Assert.AreEqual(-1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.manual_mass_shift);
                Assert.AreEqual(200 + TestExperimentalProteoform.starter_lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT - 1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.weighted_monoisotopic_mass);
            }
        }

        [Test]
        public void shift_et_peak_unlabeled()
        {
            Sweet.lollipop = new Lollipop();
            ProteoformCommunity test_community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community = test_community;
            Sweet.lollipop.neucode_labeled = false;

            //Make a few experimental proteoforms
            List<IAggregatable> n1 = TestExperimentalProteoform.generate_neucode_components(100);
            List<IAggregatable> n2 = TestExperimentalProteoform.generate_neucode_components(200);
            List<IAggregatable> n3 = TestExperimentalProteoform.generate_neucode_components(200);
            List<IAggregatable> n4 = TestExperimentalProteoform.generate_neucode_components(200);
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1");
            pf1.aggregated = n1.Select(n => (n as NeuCodePair).neuCodeLight).ToList<IAggregatable>();
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("E2");
            pf2.aggregated = n2.Select(n => (n as NeuCodePair).neuCodeLight).ToList<IAggregatable>();
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E3");
            pf3.aggregated = n3.Select(n => (n as NeuCodePair).neuCodeLight).ToList<IAggregatable>();
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E4");
            pf4.aggregated = n4.Select(n => (n as NeuCodePair).neuCodeLight).ToList<IAggregatable>();

            Sweet.lollipop.target_proteoform_community.experimental_proteoforms = new List<ExperimentalProteoform> { pf1, pf2, pf3, pf4 }.ToArray();

            //Connect them to theoreticals to form two peaks
            ProteoformComparison comparison14 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison25 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison36 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison47 = ProteoformComparison.ExperimentalTheoretical;
            TheoreticalProteoform pf5 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf6 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf7 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf8 = ConstructorsForTesting.make_a_theoretical();
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf5, comparison14, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr2 = new ProteoformRelation(pf2, pf6, comparison25, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr3 = new ProteoformRelation(pf3, pf7, comparison36, 1, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr4 = new ProteoformRelation(pf4, pf8, comparison47, 1, TestContext.CurrentContext.TestDirectory);
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1, pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.InstanceId).ToList());
            test_community.accept_deltaMass_peaks(prs, new List<ProteoformRelation>());
            Assert.AreEqual(2, Sweet.lollipop.et_peaks.Count);

            //Shift the peaks, which shifts all of the proteoforms
            DeltaMassPeak d2 = Sweet.lollipop.et_peaks[1];
            d2.shift_experimental_masses(-1, false);

            Assert.IsTrue(pf3.mass_shifted);
            Assert.IsTrue(pf4.mass_shifted);
            foreach (Component c in pf3.aggregated.Concat(pf4.aggregated).OfType<Component>())
            {
                Assert.AreEqual(-1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.manual_mass_shift);
                Assert.AreEqual(200 - 1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.weighted_monoisotopic_mass);
            }
        }

        [Test]
        public static void accept_peaks_doesnt_crash_with_empty_list()
        {
            ProteoformCommunity c = new ProteoformCommunity();
            Assert.AreEqual(0, c.accept_deltaMass_peaks(new List<ProteoformRelation>(), new List<ProteoformRelation>()).Count);
        }

        [Test]
        public static void accept_peaks_doesnt_crash_with_weird_relation()
        {
            ProteoformCommunity c = new ProteoformCommunity();
            ProteoformRelation r = new ProteoformRelation(ConstructorsForTesting.ExperimentalProteoform("E1"), ConstructorsForTesting.ExperimentalProteoform("E1"), ProteoformComparison.ExperimentalFalse, 0, TestContext.CurrentContext.TestDirectory);
            r.outside_no_mans_land = true;
            r.nearby_relations = new List<ProteoformRelation>();
            Assert.Throws<ArgumentException>(() => c.accept_deltaMass_peaks(new List<ProteoformRelation> { r }, new List<ProteoformRelation>()));
        }

        [Test]
        public static void no_relations_doesnt_crash()
        {
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("e1");
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("e2");
            ConstructorsForTesting.make_relation(e1, e2);
            DeltaMassPeak p = e1.relationships.First().peak;
            Assert.AreEqual(0, p.count_nearby_decoys(new List<ProteoformRelation>()));
        }
    }
}
