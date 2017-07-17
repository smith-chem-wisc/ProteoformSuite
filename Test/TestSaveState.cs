using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Test
{
    [TestFixture]
    class TestSaveState
    {

        #region Setup

        [OneTimeSetUp]
        public void setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        #endregion Setup

        #region Methods and Settings

        [Test]
        public void restore_lollipop_settings()
        {
            Lollipop defaults = new Lollipop();
            StringBuilder builder = Sweet.save_method();
            foreach (PropertyInfo property in typeof(Lollipop).GetProperties())
            {
                if (property.PropertyType == typeof(int))
                {
                    property.SetValue(null, Convert.ToInt32(property.GetValue(null)) + 1);
                    Assert.AreEqual(Convert.ToInt32(property.GetValue(defaults)) + 1, Convert.ToInt32(property.GetValue(null))); //the int values were changed in the current program
                }
                else if (property.PropertyType == typeof(double))
                {
                    property.SetValue(null, Convert.ToDouble(property.GetValue(null)) + 1);
                    Assert.AreEqual(Convert.ToDouble(property.GetValue(defaults)) + 1, Convert.ToDouble(property.GetValue(null))); //the double values were changed in the current program
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(null, property.GetValue(null).ToString() + "hello");
                    Assert.AreEqual(property.GetValue(defaults).ToString() + "hello", Convert.ToDouble(property.GetValue(null)).ToString()); //the string values were changed in the current program
                }
                else if (property.PropertyType == typeof(decimal))
                {
                    property.SetValue(null, Convert.ToDecimal(property.GetValue(null)) + 1);
                    Assert.AreEqual(Convert.ToDecimal(property.GetValue(defaults)) + 1, Convert.ToDecimal(property.GetValue(null))); //the decimal value were changed in the current program
                }
                else if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(null, !Convert.ToBoolean(property.GetValue(null)));
                    Assert.AreEqual(!Convert.ToBoolean(property.GetValue(defaults)), Convert.ToBoolean(property.GetValue(null))); //the bool value were changed in the current program
                }
                else continue;
            }

            Sweet.open_method(builder.ToString(), false);
            foreach (PropertyInfo property in typeof(Lollipop).GetProperties())
            {
                if (property.PropertyType == typeof(int))
                    Assert.AreEqual(Convert.ToInt32(property.GetValue(defaults)), Convert.ToInt32(property.GetValue(null))); //the int values were changed back
                else if (property.PropertyType == typeof(double))
                    Assert.AreEqual(Convert.ToDouble(property.GetValue(defaults)), Convert.ToDouble(property.GetValue(null))); //the double values were changed back
                else if (property.PropertyType == typeof(string))
                    Assert.AreEqual(property.GetValue(defaults).ToString(), Convert.ToDouble(property.GetValue(null)).ToString()); //the string values were changed back
                else if (property.PropertyType == typeof(decimal))
                    Assert.AreEqual(Convert.ToDecimal(property.GetValue(defaults)), Convert.ToDecimal(property.GetValue(null))); //the decimal value were changed back
                else if (property.PropertyType == typeof(bool))
                    Assert.AreEqual(Convert.ToBoolean(property.GetValue(defaults)), Convert.ToBoolean(property.GetValue(null))); //the bool value were changed back
                else continue;
            }
        }

        [Test]
        public void test_accept_from_presets()
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

            List<ProteoformRelation> prs2 = new List<ProteoformRelation> { pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs2) pr.set_nearby_group(prs2, prs2.Select(r => r.InstanceId).ToList());
            Assert.AreEqual(3, pr2.nearby_relations_count);
            Assert.AreEqual(3, pr3.nearby_relations_count);
            Assert.AreEqual(3, pr4.nearby_relations_count);

            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet> { new PtmSet(new List<Ptm> { new Ptm(-1, ConstructorsForTesting.get_modWithMass("unmodified", 0)) }) };
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = Sweet.lollipop.theoretical_database.make_ptmset_dictionary();
            Sweet.unaccept_peak_action(pr2);
            Sweet.lollipop.ee_peaks = test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            Assert.AreEqual(1, Sweet.lollipop.ee_peaks.Count);
            DeltaMassPeak peak = Sweet.lollipop.ee_peaks[0];
            Assert.IsFalse(peak.Accepted); // <-- even though it's above the threshold
            Assert.AreEqual(3, peak.grouped_relations.Count);
            Assert.AreEqual(3, pr2.peak.peak_relation_group_count);
            Assert.AreEqual(0, pr2.peak.DeltaMass);
            Assert.AreEqual("[unmodified]", peak.possiblePeakAssignments_string);

            //Test that the relations in the peak are added to each of the proteoforms referenced in the peak
            Assert.True(pf3.relationships.Contains(pr2));
            Assert.True(pf4.relationships.Contains(pr2) && pf4.relationships.Contains(pr3));
            Assert.True(pf5.relationships.Contains(pr3) && pf5.relationships.Contains(pr4));
        }

        #endregion Methods and Settings

        #region Results Summary

        [Test]
        public void results_summary_doesnt_crash_without_initializing()
        {
            Assert.True(ResultsSummaryGenerator.generate_full_report().Length > 0);
            Assert.True(ResultsSummaryGenerator.results_dataframe().Length > 0);
        }

        [Test]
        public void results_dataframe_with_something()
        {
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1");
            e.linked_proteoform_references = new List<Proteoform>(new List<Proteoform> { ConstructorsForTesting.make_a_theoretical() });
            e.ptm_set = e.linked_proteoform_references.Last().ptm_set;
            ProteoformFamily f = new ProteoformFamily(e);
            f.construct_family();
            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily> { f };
            string[] lines = ResultsSummaryGenerator.results_dataframe().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(lines.Count() == 3);
            Assert.True(lines.Any(a => a.Contains("E1")));
        }

        [Test]
        public void saveall()
        {
            Sweet.lollipop = new Lollipop();
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("asdf");
            Sweet.lollipop.qVals.Add(e.quant);
            GoTermNumber g = new GoTermNumber(new GoTerm("id", "desc", Aspect.BiologicalProcess), 0, 0, 0, 0);
            g.by = -1;
            Sweet.lollipop.goTermNumbers.Add(g);
            ResultsSummaryGenerator.save_all(TestContext.CurrentContext.TestDirectory, Sweet.time_stamp());
        }

        [Test]
        public void biorepintensitytable()
        {
            Sweet.lollipop = new Lollipop();
            Dictionary<string, List<string>> conditionsBioReps = new Dictionary<string, List<string>>
            {
                {"n", new List<string>{1.ToString(), 2.ToString(), 3.ToString() } },
                {"s", new List<string>{1.ToString(), 2.ToString(), 3.ToString() } },
            };
            List<InputFile> input_files = new List<InputFile>
            {
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "1", "1", "1"),
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "1", "2", "1"),
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "2", "1", "1"),
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "2", "2", "1"),
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "3", "1", "1"),
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "3", "2", "1"),
            };
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("asdf");
            e.quant.allIntensities = new Dictionary<Tuple<string, string>, BiorepIntensity>
            {
                {new Tuple<string, string>("n", 1.ToString()), new BiorepIntensity(false, 1.ToString(), "n", 1, new List<BiorepFractionTechrepIntensity>()) },
                {new Tuple<string, string>("n", 2.ToString()), new BiorepIntensity(true, 2.ToString(), "n", 1, new List<BiorepFractionTechrepIntensity>()) },
                {new Tuple<string, string>("n", 3.ToString()), new BiorepIntensity(false, 3.ToString(), "n", 1, new List<BiorepFractionTechrepIntensity>()) },
                {new Tuple<string, string>("s", 1.ToString()), new BiorepIntensity(false, 1.ToString(), "s", 1, new List<BiorepFractionTechrepIntensity>()) },
                {new Tuple<string, string>("s", 2.ToString()), new BiorepIntensity(false, 2.ToString(), "s", 1, new List<BiorepFractionTechrepIntensity>()) },
                {new Tuple<string, string>("s", 3.ToString()), new BiorepIntensity(false, 3.ToString(), "s", 1, new List<BiorepFractionTechrepIntensity>()) },
            };
            Assert.False(ResultsSummaryGenerator.biological_replicate_intensities(new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, true).Contains("NaN"));
            Assert.True(ResultsSummaryGenerator.biological_replicate_intensities(new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false).Contains("NaN"));
            e.quant.allIntensities = new Dictionary<Tuple<string, string>, BiorepIntensity>
            {
                {new Tuple<string, string>("n", 1.ToString()), new BiorepIntensity(true, 1.ToString(), "n", 1, new List<BiorepFractionTechrepIntensity>()) },
                {new Tuple<string, string>("n", 2.ToString()), new BiorepIntensity(true, 2.ToString(), "n", 1, new List<BiorepFractionTechrepIntensity>()) },
                {new Tuple<string, string>("n", 3.ToString()), new BiorepIntensity(true, 3.ToString(), "n", 1, new List<BiorepFractionTechrepIntensity>()) },
                {new Tuple<string, string>("s", 1.ToString()), new BiorepIntensity(false, 1.ToString(), "s", 1, new List<BiorepFractionTechrepIntensity> { new BiorepFractionTechrepIntensity("s", 1.ToString(), 2.ToString(), 1.ToString(), 1d) }) },
                {new Tuple<string, string>("s", 2.ToString()), new BiorepIntensity(false, 2.ToString(), "s", 1, new List<BiorepFractionTechrepIntensity> { new BiorepFractionTechrepIntensity("s", 2.ToString(), 2.ToString(), 1.ToString(), 1d) }) },
                {new Tuple<string, string>("s", 3.ToString()), new BiorepIntensity(false, 3.ToString(), "s", 1, new List<BiorepFractionTechrepIntensity> { new BiorepFractionTechrepIntensity("s", 3.ToString(), 2.ToString(), 1.ToString(), 1d) }) },
            };
            
            // Headers should be condition_biorep_fraction_techrep
            string[] header = ResultsSummaryGenerator.biological_replicate_intensities(new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false).Split('\n')[0].Split('\t');
            Assert.True(header[1] == "n_1_1_1" && header[2] == "n_1_2_1" && header[3] == "n_2_1_1" && header[4] == "n_2_2_1" && header[5] == "n_3_1_1" && header[6] == "n_3_2_1" 
                && header[7] == "s_1_1_1" && header[8] == "s_1_2_1" && header[9] == "s_2_1_1" && header[10] == "s_2_2_1" && header[11] == "s_3_1_1" && header[12] == "s_3_2_1");

            string[] line = ResultsSummaryGenerator.biological_replicate_intensities(new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false).Split('\n')[1].Split('\t');
            Assert.True(line.First() == e.accession);
            Assert.True(line[1] == "NaN" && line[2] == "NaN" && line[3] == "NaN" && line[4] == "NaN" && line[5] == "NaN" && line[6] == "NaN" // all "n" condition is imputed
                && line[7] == "NaN" && line[8] != "NaN" && line[9] == "NaN" && line[10] != "NaN" && line[11] == "NaN" && line[12] != "NaN"); // all fract2 of "s" condition has values

            ResultsSummaryGenerator.save_biological_replicate_intensities(Path.Combine(TestContext.CurrentContext.TestDirectory, "biorep.txt"), true, new List<ExperimentalProteoform> { e });
            Assert.True(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "biorep.txt")));
        }
        
        #endregion Results Summary

    }
}