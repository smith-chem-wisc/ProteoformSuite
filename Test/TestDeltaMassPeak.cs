using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using Proteomics;
using System.IO;
using UsefulProteomicsDatabases;

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
            Lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Lollipop.input_files);
            Lollipop.read_mods();
            Lollipop.et_high_mass_difference = 250;
            Lollipop.et_low_mass_difference = -250;
            Lollipop.peak_width_base_ee = 0.015;
            Lollipop.peak_width_base_et = 0.015;


            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("acession1");
            TheoreticalProteoform pf2 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type = ProteoformComparison.et;
            double delta_mass = 1 - 1e-7;

            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("acession3");
            TheoreticalProteoform pf4 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type2 = ProteoformComparison.et;
            double delta_mass2 = 1;

            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("acession5");
            TheoreticalProteoform pf6 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type3 = ProteoformComparison.et;
            double delta_mass3 = 1 + 1e-7;

            ExperimentalProteoform pf55 = ConstructorsForTesting.ExperimentalProteoform("acession5");
            TheoreticalProteoform pf65 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type35 = ProteoformComparison.et;
            double delta_mass35 = 1 + 2e-7;

            List<ProteoformRelation> theList = new List<ProteoformRelation>();

            theList.Add(new ProteoformRelation(pf1, pf2, relation_type, delta_mass));
            theList.Add(new ProteoformRelation(pf3, pf4, relation_type2, delta_mass2));
            theList.Add(new ProteoformRelation(pf5, pf6, relation_type3, delta_mass3));
            theList.Add(new ProteoformRelation(pf55, pf65, relation_type35, delta_mass35));

            ProteoformRelation base_relation = new ProteoformRelation(pf3, pf4, relation_type2, delta_mass2);

            base_relation.nearby_relations = base_relation.set_nearby_group(theList, theList.Select(r => r.instanceId).ToList());
            Console.WriteLine("Creating deltaMassPeak");
            DeltaMassPeak deltaMassPeak = new DeltaMassPeak(base_relation, theList);
            Console.WriteLine("Created deltaMassPeak");


            Assert.AreEqual(0, deltaMassPeak.peak_group_fdr);
            Dictionary<string, List<ProteoformRelation>> decoy_relations = new Dictionary<string, List<ProteoformRelation>>();

            decoy_relations["decoyDatabase1"] = new List<ProteoformRelation>();

            ExperimentalProteoform pf7 = ConstructorsForTesting.ExperimentalProteoform("experimental1");
            TheoreticalProteoform pf8 = ConstructorsForTesting.make_a_theoretical();
            ProteoformComparison relation_type4 = ProteoformComparison.ed;
            double delta_mass4 = 1;
            ProteoformRelation decoy_relation = new ProteoformRelation(pf7, pf8, relation_type4, delta_mass4);

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
            ProteoformCommunity test_community = new ProteoformCommunity();
            Lollipop.proteoform_community = test_community;

            Lollipop.uniprotModificationTable = new Dictionary<string, IList<Modification>> {
                { "unmodified", new List<Modification>() {
                    new ModificationWithMass("unmodified", new Tuple<string, string>("", ""), null, ModificationSites.K, 0, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "") }
                }
            };
            Lollipop.updated_theoretical = true;

            //Testing the acceptance of peaks. The FDR is tested above, so I'm not going to work with that here.
            //Four proteoforms, three relations (linear), middle one isn't accepted; should give 2 families
            Lollipop.min_peak_count_ee = 2;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2");
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("E3");
            ExperimentalProteoform pf6 = ConstructorsForTesting.ExperimentalProteoform("E4");

            ProteoformComparison comparison34 = ProteoformComparison.ee;
            ProteoformComparison comparison45 = ProteoformComparison.ee;
            ProteoformComparison comparison56 = ProteoformComparison.ee;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, comparison34, 0);
            ProteoformRelation pr3 = new ProteoformRelation(pf4, pf5, comparison45, 0);
            ProteoformRelation pr4 = new ProteoformRelation(pf5, pf6, comparison56, 0);

            //Test display strings
            Assert.AreEqual("E1", pr2.accession_1);
            Assert.AreEqual("E2", pr2.accession_2);
            pr2.relation_type = ProteoformComparison.ee;
            Assert.AreEqual(ProteoformRelation.ee_string, pr2.relation_type_string);
            pr2.relation_type = ProteoformComparison.et;
            Assert.AreEqual(ProteoformRelation.et_string, pr2.relation_type_string);
            pr2.relation_type = ProteoformComparison.ed;
            Assert.AreEqual(ProteoformRelation.ed_string, pr2.relation_type_string);
            pr2.relation_type = ProteoformComparison.ef;
            Assert.AreEqual(ProteoformRelation.ef_string, pr2.relation_type_string);
            pr2.relation_type = comparison34;

            List<ProteoformRelation> prs2 = new List<ProteoformRelation> { pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs2) pr.set_nearby_group(prs2, prs2.Select(r => r.instanceId).ToList());
            Assert.AreEqual(3, pr2.nearby_relations_count);
            Assert.AreEqual(3, pr3.nearby_relations_count);
            Assert.AreEqual(3, pr4.nearby_relations_count);

            test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            Assert.AreEqual(1, test_community.delta_mass_peaks.Count);
            DeltaMassPeak peak = test_community.delta_mass_peaks[0];
            Assert.AreEqual(3, peak.grouped_relations.Count);
            Assert.AreEqual(3, pr2.peak_center_count);
            Assert.AreEqual(0, pr2.peak_center_deltaM);
            Assert.AreEqual("unmodified", peak.possiblePeakAssignments_string);
            peak.possiblePeakAssignments.Add(new Modification("unmodified"));
            Assert.AreEqual("unmodified; unmodified", peak.possiblePeakAssignments_string);

            //Test that the relations in the peak are added to each of the proteoforms referenced in the peak
            Assert.True(pf3.relationships.Contains(pr2));
            Assert.True(pf4.relationships.Contains(pr2) && pf4.relationships.Contains(pr3));
            Assert.True(pf5.relationships.Contains(pr3) && pf5.relationships.Contains(pr4));
        }

        [Test]
        public void wrong_relation_shifting()
        {
            ProteoformCommunity test_community = new ProteoformCommunity();
            Lollipop.proteoform_community = test_community;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2");
            ProteoformComparison wrong_comparison = ProteoformComparison.ee;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, wrong_comparison, 0);
            ProteoformRelation pr3 = new ProteoformRelation(pf3, pf4, wrong_comparison, 0);
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr2, pr3 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.instanceId).ToList());
            test_community.accept_deltaMass_peaks(prs, new List<ProteoformRelation>());
            Assert.False(test_community.delta_mass_peaks[0].shift_experimental_masses(1, true));
        }

        [Test]
        public void artificial_deltaMPeak()
        {
            Lollipop.opening_results = true;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2");
            ProteoformComparison comparison = ProteoformComparison.ee;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, comparison, 0);
            ProteoformRelation pr3 = new ProteoformRelation(pf3, pf4, comparison, 0);
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr2, pr3 };
            Assert.AreEqual(prs, new DeltaMassPeak(pr2, prs).grouped_relations);
            Lollipop.opening_results = false;
        }

        [Test]
        public void shift_et_peak_neucode()
        {
            ProteoformCommunity test_community = new ProteoformCommunity();
            Lollipop.proteoform_community = test_community;

            //Make a few experimental proteoforms
            List<Component> n1 = TestExperimentalProteoform.generate_neucode_components(100);
            List<Component> n2 = TestExperimentalProteoform.generate_neucode_components(100);
            List<Component> n3 = TestExperimentalProteoform.generate_neucode_components(200);
            List<Component> n4 = TestExperimentalProteoform.generate_neucode_components(200);
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1");
            pf1.aggregated_components = n1;
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("E2");
            pf2.aggregated_components = n2;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E3");
            pf3.aggregated_components = n3;
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E4");
            pf4.aggregated_components = n4;

            Lollipop.proteoform_community.experimental_proteoforms = new List<ExperimentalProteoform> { pf1, pf2, pf3, pf4 }.ToArray();

            //Connect them to theoreticals to form two peaks
            ProteoformComparison comparison14 = ProteoformComparison.et;
            ProteoformComparison comparison25 = ProteoformComparison.et;
            ProteoformComparison comparison36 = ProteoformComparison.et;
            ProteoformComparison comparison47 = ProteoformComparison.et;
            TheoreticalProteoform pf5 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf6 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf7 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf8 = ConstructorsForTesting.make_a_theoretical();
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf5, comparison14, 0);
            ProteoformRelation pr2 = new ProteoformRelation(pf2, pf6, comparison25, 0);
            ProteoformRelation pr3 = new ProteoformRelation(pf3, pf7, comparison36, 1);
            ProteoformRelation pr4 = new ProteoformRelation(pf4, pf8, comparison47, 1);
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1, pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.instanceId).ToList());
            test_community.accept_deltaMass_peaks(prs, new List<ProteoformRelation>());
            Assert.AreEqual(2, test_community.delta_mass_peaks.Count);

            //Shift the peaks, which shifts all of the proteoforms
            DeltaMassPeak d2 = test_community.delta_mass_peaks[1];
            d2.shift_experimental_masses(-1, true);

            Assert.IsTrue(pf3.mass_shifted);
            Assert.IsTrue(pf4.mass_shifted);
            foreach (Component c in 
                n3.
                Concat(n4).
                Concat(n3.Select(n => ((NeuCodePair)n).neuCodeLight)).
                Concat(n4.Select(n => ((NeuCodePair)n).neuCodeLight)))
            {
                Assert.AreEqual(-1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.manual_mass_shift);
                Assert.AreEqual(200 - 1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.weighted_monoisotopic_mass);
            }

            foreach (Component c in 
                n3.Select(n => ((NeuCodePair)n).neuCodeHeavy).
                Concat(n4.Select(n => ((NeuCodePair)n).neuCodeHeavy)))
            {
                Assert.AreEqual(-1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.manual_mass_shift);
                Assert.AreEqual(200 + TestExperimentalProteoform.starter_lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT - 1.0 * Lollipop.MONOISOTOPIC_UNIT_MASS, c.weighted_monoisotopic_mass);
            }
        }

        [Test]
        public void shift_et_peak_unlabeled()
        {
            ProteoformCommunity test_community = new ProteoformCommunity();
            Lollipop.proteoform_community = test_community;

            //Make a few experimental proteoforms
            List<Component> n1 = TestExperimentalProteoform.generate_neucode_components(100);
            List<Component> n2 = TestExperimentalProteoform.generate_neucode_components(200);
            List<Component> n3 = TestExperimentalProteoform.generate_neucode_components(200);
            List<Component> n4 = TestExperimentalProteoform.generate_neucode_components(200);
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1");
            pf1.aggregated_components = n1;
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("E2");
            pf2.aggregated_components = n2;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E3");
            pf3.aggregated_components = n3;
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E4");
            pf4.aggregated_components = n4;

            Lollipop.proteoform_community.experimental_proteoforms = new List<ExperimentalProteoform> { pf1, pf2, pf3, pf4 }.ToArray();

            //Connect them to theoreticals to form two peaks
            ProteoformComparison comparison14 = ProteoformComparison.et;
            ProteoformComparison comparison25 = ProteoformComparison.et;
            ProteoformComparison comparison36 = ProteoformComparison.et;
            ProteoformComparison comparison47 = ProteoformComparison.et;
            TheoreticalProteoform pf5 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf6 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf7 = ConstructorsForTesting.make_a_theoretical();
            TheoreticalProteoform pf8 = ConstructorsForTesting.make_a_theoretical();
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf5, comparison14, 0);
            ProteoformRelation pr2 = new ProteoformRelation(pf2, pf6, comparison25, 0);
            ProteoformRelation pr3 = new ProteoformRelation(pf3, pf7, comparison36, 1);
            ProteoformRelation pr4 = new ProteoformRelation(pf4, pf8, comparison47, 1);
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1, pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.instanceId).ToList());
            test_community.accept_deltaMass_peaks(prs, new List<ProteoformRelation>());
            Assert.AreEqual(2, test_community.delta_mass_peaks.Count);

            //Shift the peaks, which shifts all of the proteoforms
            DeltaMassPeak d2 = test_community.delta_mass_peaks[1];
            d2.shift_experimental_masses(-1, false);

            Assert.IsTrue(pf3.mass_shifted);
            Assert.IsTrue(pf4.mass_shifted);
            foreach (Component c in n3.Concat(n4))
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
    }
}
