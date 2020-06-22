﻿using NUnit.Framework;
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
    class TestSweet
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

            Sweet.open_method("", builder.ToString(), false, out string warning);
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
            using (StreamWriter file = new StreamWriter(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml")))
                file.WriteLine(Sweet.save_method());
            Sweet.open_method(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), string.Join(Environment.NewLine, File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"))), true, out string warning);
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

        [Test]
        public void other_method_file_issue()
        {
            //have a name other than setting or action
            using (StreamWriter file = new StreamWriter(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml")))
                file.WriteLine(Sweet.save_method());
            Assert.IsTrue(Sweet.open_method(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), string.Join(Environment.NewLine, File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"))), true, out string warning1));
            string[] edit = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"));
            edit[2] = "  <badname field_type=\"System.Boolean\" field_name=\"badfieldname\" field_value=\"True\" />";

            File.WriteAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), edit);
            Assert.IsFalse(Sweet.open_method(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), string.Join(Environment.NewLine, File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"))), true, out string warning2));

            using (StreamWriter file = new StreamWriter(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml")))
                file.WriteLine(Sweet.save_method());
            edit = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"));
            string[] new_edit = new string[edit.Length + 1];
            new_edit[0] = edit[0];
            new_edit[1] = edit[1];
            new_edit[2] = "  <setting field_type=\"System.Boolean\" field_name=\"badfieldname\" field_value=\"True\" />";
            for (int i = 3; i < edit.Length + 1; i++)
            {
                new_edit[i] = edit[i - 1];
            }
            string message;
            File.WriteAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), new_edit);
            Assert.IsTrue(Sweet.open_method(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), string.Join(Environment.NewLine, File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"))), true, out message));
            Assert.AreEqual("Setting badfieldname has changed, and it was not changed to preset System.Boolean True in the current run\r\n", message);

            using (StreamWriter file = new StreamWriter(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml")))
                file.WriteLine(Sweet.save_method());
            edit = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"));
            new_edit = new string[edit.Length - 1];
            new_edit[0] = edit[0];
            new_edit[1] = edit[1];
            for (int i = 2; i < edit.Length - 1; i++)
            {
                new_edit[i] = edit[i + 1];
            }
            File.WriteAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), new_edit);
            Assert.IsTrue(Sweet.open_method(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), string.Join(Environment.NewLine, File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"))), true, out message));
            Assert.AreEqual("The following parameters did not have a setting specified: neucode_labeled\r\n", message);

            Sweet.add_file_action(new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx"), Purpose.TopDown));
            using (StreamWriter file = new StreamWriter(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml")))
                file.WriteLine(Sweet.save_method());
            edit = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"));
            edit[81] = "  <action action=\"badaction file filepath with purpose TopDown\" />";
            File.WriteAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), edit);
            Assert.IsFalse(Sweet.open_method(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"), string.Join(Environment.NewLine, File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "method.xml"))), true, out message));
        }

        #endregion Methods and Settings

        #region Results Summary

        [Test]
        public void results_summary_doesnt_crash_without_initializing()
        {
            Sweet.lollipop = new Lollipop();
            Assert.True(ResultsSummaryGenerator.generate_full_report().Length > 0);
            Assert.True(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, new TusherAnalysis1())).Length > 0);
            Assert.True(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.topdown_results_dataframe()).Length > 0);
            Assert.True(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.experimental_intensities_dataframe()).Length > 0);
        }

        [Test]
        public void results_dataframe_with_something()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.input_files.Add(ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Identification, "n", "s", "1", "1", "1")); //0
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1");
            e.linked_proteoform_references = new List<Proteoform>(new List<Proteoform> { ConstructorsForTesting.make_a_theoretical() });
            e.ptm_set = e.linked_proteoform_references.Last().ptm_set;
            ProteoformFamily f = new ProteoformFamily(e);
            f.construct_family();
            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily> { f };
            string[] lines = ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, Sweet.lollipop.TusherAnalysis1)).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(lines.Count() == 3);
            Assert.True(lines.Any(a => a.Contains("E1")));
            TopDownProteoform td = ConstructorsForTesting.TopDownProteoform("TD1", 1000, 10);
            td.linked_proteoform_references = new List<Proteoform>(new List<Proteoform> { ConstructorsForTesting.make_a_theoretical() });
            td.ptm_set = e.linked_proteoform_references.Last().ptm_set;
            ProteoformFamily f2 = new ProteoformFamily(td);
            f2.construct_family();
            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily> { f2 };
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform>() { td };
            lines = ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, Sweet.lollipop.TusherAnalysis1)).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(lines.Count() == 3);
            lines = ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.topdown_results_dataframe()).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(lines.Count() == 3);
            Assert.True(lines.Any(a => a.Contains("TD1")));
            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily> { f, f2 };
            lines = ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.experimental_intensities_dataframe()).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual(lines.Count(), 4);
            Assert.True(lines.Any(a => a.Contains("E1")));
            Assert.True(lines.Any(a => a.Contains("TD1")));
        }

        [Test]
        public void results_decoy_dataframe_with_something()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.input_files.Add(ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Identification, "n", "s", "1", "1", "1")); //0
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1");
            e.linked_proteoform_references = new List<Proteoform>(new List<Proteoform> { ConstructorsForTesting.make_a_theoretical() });
            e.ptm_set = e.linked_proteoform_references.Last().ptm_set;
            ProteoformFamily f = new ProteoformFamily(e);
            f.construct_family();
            Sweet.lollipop.decoy_proteoform_communities.Add("Decoy1", new ProteoformCommunity());
            Sweet.lollipop.decoy_proteoform_communities["Decoy1"].families = new List<ProteoformFamily> { f };
            var time_stamp = Sweet.time_stamp();
            var directory = TestContext.CurrentContext.TestDirectory;
            ResultsSummaryGenerator.save_all(TestContext.CurrentContext.TestDirectory, time_stamp, Sweet.lollipop.TusherAnalysis1 as IGoAnalysis, Sweet.lollipop.TusherAnalysis1 as TusherAnalysis);
            Assert.IsTrue(File.Exists(Path.Combine(directory, "decoy_experimental_results_" + time_stamp + ".tsv")));
            var lines = File.ReadAllLines(Path.Combine(directory, "decoy_experimental_results_" + time_stamp + ".tsv"));
            Assert.AreEqual(2, lines.Length);
        }

        [Test]
        public void saveall()
        {
            Sweet.lollipop = new Lollipop();
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("asdf");
            Sweet.lollipop.qVals.Add(e.quant);
            GoTermNumber g = new GoTermNumber(new goTerm("id", "desc", Aspect.BiologicalProcess), 0, 0, 0, 0);
            g.by = -1;
            Sweet.lollipop.TusherAnalysis1.GoAnalysis.goTermNumbers.Add(g);
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform>() { ConstructorsForTesting.TopDownProteoform("td1", 1000, 10) };
            ResultsSummaryGenerator.save_all(TestContext.CurrentContext.TestDirectory, Sweet.time_stamp(), Sweet.lollipop.TusherAnalysis1 as IGoAnalysis, Sweet.lollipop.TusherAnalysis1 as TusherAnalysis);
        }

        [Test]
        public void experimental_intensitites_dataframe()
        {
            Sweet.lollipop = new Lollipop();
            int id_file_index = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.Identification)).First());
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.xlsx") }, Lollipop.acceptable_extensions[id_file_index], Lollipop.file_types[id_file_index], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_3columns.tsv") }, Lollipop.acceptable_extensions[id_file_index], Lollipop.file_types[id_file_index], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).First().lt_condition = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList()[1].fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList()[1].biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList()[1].technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList()[1].lt_condition = "1";
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), Sweet.lollipop.raw_experimental_components, Purpose.Identification, false);
            Sweet.lollipop.aggregate_proteoforms(true, null, Sweet.lollipop.raw_experimental_components, null, 1);
            Sweet.lollipop.construct_target_and_decoy_families();
            var dt = ResultsSummaryGenerator.experimental_intensities_dataframe();
            Assert.AreEqual(111, dt.Rows.Count);
            Assert.AreEqual(10, dt.Columns.Count);

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
            Sweet.lollipop.TusherAnalysis1.conditionBiorep_sums = conditionsBioReps.SelectMany(kv => kv.Value.Select(v => new Tuple<string, string>(kv.Key, v))).ToDictionary(t => t, t => 1d);
            List<InputFile> input_files = new List<InputFile>
            {
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "1", "1", "1"), //0
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "1", "2", "1"), //1
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "2", "1", "1"), //2
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "2", "2", "1"), //3
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "3", "1", "1"), //4
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "3", "2", "1"), //5
            };
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("asdf");

            e.quant.TusherValues1.allIntensities = new Dictionary<Tuple<string, string>, BiorepIntensity>
            {
                {new Tuple<string, string>("n", 1.ToString()), new BiorepIntensity(false, 1.ToString(), "n", 1) },
                {new Tuple<string, string>("n", 2.ToString()), new BiorepIntensity(true, 2.ToString(), "n", 1) },
                {new Tuple<string, string>("n", 3.ToString()), new BiorepIntensity(false, 3.ToString(), "n", 1) },
                {new Tuple<string, string>("s", 1.ToString()), new BiorepIntensity(false, 1.ToString(), "s", 1) },
                {new Tuple<string, string>("s", 2.ToString()), new BiorepIntensity(false, 2.ToString(), "s", 1) },
                {new Tuple<string, string>("s", 3.ToString()), new BiorepIntensity(false, 3.ToString(), "s", 1) },
            };

            // Biorep intensities with imputation uses processed values
            Assert.False(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.TusherAnalysis1 as IGoAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, true, true)).Contains("NaN"));

            // Biorep intensities without imputation uses raw values
            e.biorepIntensityList = e.quant.TusherValues1.allIntensities.Values.Where(x => !x.imputed).ToList();
            Assert.True(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.TusherAnalysis1 as IGoAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false, true)).Contains("\t0\t"));

            string[] line = ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.TusherAnalysis1 as IGoAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false, true)).Split('\n')[1].Split('\t');
            Assert.True(line.First() == e.accession);
            Assert.True(line[1] == "1" && line[2] == "0" && line[3] == "1" && line[4] == "1" && line[5] == "1" && line[6] == "1");
        }

        [Test]
        public void bioreptechrepintensitytable()
        {
            Sweet.lollipop = new Lollipop();
            Dictionary<string, List<string>> conditionsBioReps = new Dictionary<string, List<string>>
            {
                {"n", new List<string>{1.ToString(), 2.ToString(), 3.ToString() } },
                {"s", new List<string>{1.ToString(), 2.ToString(), 3.ToString() } },
            };
            Sweet.lollipop.TusherAnalysis2.conditionBiorep_sums = conditionsBioReps.SelectMany(kv => kv.Value.Select(v => new Tuple<string, string>(kv.Key, v))).ToDictionary(t => t, t => 1d);
            List<InputFile> input_files = new List<InputFile>
            {
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "1", "1", "1"), //0
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "1", "2", "1"), //1
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "2", "1", "1"), //2
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "2", "2", "1"), //3
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "3", "1", "1"), //4
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "3", "2", "1"), //5
            };
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("asdf");

            e.quant.TusherValues2.allIntensities = new Dictionary<Tuple<string, string, string>, BiorepTechrepIntensity>
            {
                {new Tuple<string, string, string>("n", 1.ToString(), 1.ToString()), new BiorepTechrepIntensity(false, 1.ToString(),"n", 1.ToString(),  1) },
                {new Tuple<string, string, string>("n", 2.ToString(), 1.ToString()), new BiorepTechrepIntensity(true, 2.ToString(),"n", 1.ToString(),  1) },
                {new Tuple<string, string, string>("n", 3.ToString(), 1.ToString()), new BiorepTechrepIntensity(false, 3.ToString(), "n", 1.ToString(), 1) },
                {new Tuple<string, string, string>("s", 1.ToString(), 1.ToString()), new BiorepTechrepIntensity(false, 1.ToString(),"s",  1.ToString(), 1) },
                {new Tuple<string, string, string>("s", 2.ToString(), 1.ToString()), new BiorepTechrepIntensity(false, 2.ToString(), "s", 1.ToString(), 1) },
                {new Tuple<string, string, string>("s", 3.ToString(), 1.ToString()), new BiorepTechrepIntensity(false, 3.ToString(), "s", 1.ToString(),  1) },
            };

            // Biorep intensities with imputation uses processed values
            Assert.False(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.TusherAnalysis2 as IGoAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, true, true)).Contains("NaN"));

            // Biorep intensities without imputation uses raw values
            e.biorepTechrepIntensityList = e.quant.TusherValues2.allIntensities.Values.Where(x => !x.imputed).ToList();
            Assert.True(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.TusherAnalysis2 as IGoAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false, true)).Contains("\t0\t"));

            string[] line = ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.TusherAnalysis2 as IGoAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false, true)).Split('\n')[1].Split('\t');
            Assert.True(line.First() == e.accession);
            Assert.True(line[1] == "1" && line[2] == "0" && line[3] == "1" && line[4] == "1" && line[5] == "1" && line[6] == "1");
        }

        [Test]
        public void biorepintensitytableLog2Fold()
        {
            Sweet.lollipop = new Lollipop();
            Dictionary<string, List<string>> conditionsBioReps = new Dictionary<string, List<string>>
            {
                {"n", new List<string>{1.ToString(), 2.ToString(), 3.ToString() } },
                {"s", new List<string>{1.ToString(), 2.ToString(), 3.ToString() } },
            };
            Sweet.lollipop.Log2FoldChangeAnalysis.conditionBiorepIntensitySums = conditionsBioReps.SelectMany(kv => kv.Value.Select(v => new Tuple<string, string>(kv.Key, v))).ToDictionary(t => t, t => 1d);
            List<InputFile> input_files = new List<InputFile>
            {
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "1", "1", "1"), //0
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "1", "2", "1"), //1
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "2", "1", "1"), //2
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "2", "2", "1"), //3
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "3", "1", "1"), //4
                ConstructorsForTesting.InputFile("fake.txt", Labeling.NeuCode, Purpose.Quantification, "n", "s", "3", "2", "1"), //5
            };
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("asdf");

            e.quant.Log2FoldChangeValues.allIntensities = new Dictionary<Tuple<string, string>, BiorepIntensity>
            {
                {new Tuple<string, string>("n", 1.ToString()), new BiorepIntensity(false, 1.ToString(), "n", 1) },
                {new Tuple<string, string>("n", 2.ToString()), new BiorepIntensity(true, 2.ToString(), "n", 1) },
                {new Tuple<string, string>("n", 3.ToString()), new BiorepIntensity(false, 3.ToString(), "n", 1) },
                {new Tuple<string, string>("s", 1.ToString()), new BiorepIntensity(false, 1.ToString(), "s", 1) },
                {new Tuple<string, string>("s", 2.ToString()), new BiorepIntensity(false, 2.ToString(), "s", 1) },
                {new Tuple<string, string>("s", 3.ToString()), new BiorepIntensity(false, 3.ToString(), "s", 1) },
            };
            // With imputation uses processed values
            Assert.False(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.Log2FoldChangeAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, true, true)).Contains("NaN"));

            // Without imputation uses raw values
            e.biorepIntensityList = e.quant.Log2FoldChangeValues.allIntensities.Values.Where(x => !x.imputed).ToList();
            Assert.True(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.Log2FoldChangeAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false, true)).Contains("\t0\t"));

            // Headers should be condition_biorep_fraction_techrep
            string[] header = ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.Log2FoldChangeAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false, true)).Split('\n')[0].Split('\t');
            Assert.True(header[1] == "n_1" && header[2] == "n_2" && header[3] == "n_3" && header[4] == "s_1" && header[5] == "s_2" && header[6] == "s_3");
            string[] line = ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.biological_replicate_intensities(Sweet.lollipop.Log2FoldChangeAnalysis, new List<ExperimentalProteoform> { e }, input_files, conditionsBioReps, false, true)).Split('\n')[1].Split('\t');
            Assert.True(line.First() == e.accession);
            Assert.True(line[1] == "1" && line[2] == "0" && line[3] == "1" && line[4] == "1" && line[5] == "1" && line[6] == "1"); // all "n" condition is imputed
        }

        [Test]
        public void test_bottom_up_save()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "testBottomUp.psmtsv") }, Lollipop.acceptable_extensions[7], Lollipop.file_types[7], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Assert.AreEqual(2, Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Count);
            var time_stamp = Sweet.time_stamp();
            var directory = TestContext.CurrentContext.TestDirectory;
            ResultsSummaryGenerator.save_all(TestContext.CurrentContext.TestDirectory, time_stamp, Sweet.lollipop.TusherAnalysis1 as IGoAnalysis, Sweet.lollipop.TusherAnalysis1 as TusherAnalysis);
            Assert.IsTrue(File.Exists(Path.Combine(directory, "bottomup_results_" + time_stamp + ".tsv")));
            Assert.IsTrue(File.Exists(Path.Combine(directory, "shared_peptide_bottomup_results_" + time_stamp + ".tsv")));
            Assert.IsTrue(File.Exists(Path.Combine(directory, "proteoform_bottomup_evidence_" + time_stamp + ".tsv")));
            Assert.True(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.bottomup_results_dataframe()).Length > 0);
            Assert.True(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.putative_proteoforms_bottom_up()).Length > 0);
            Assert.True(ResultsSummaryGenerator.datatable_tostring(ResultsSummaryGenerator.shared_peptide_results_dataframe()).Length > 0);
        }

        [Test]
        public void test_putative_proteoforms()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            var mod = Sweet.lollipop.theoretical_database.all_mods_with_mass.Where(m => m.OriginalId == "Acetylation").First();
           
            //should have 1 pform
            SpectrumMatch p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            p1.ptm_list = new List<Ptm>() { new Ptm(2, mod)};
            SpectrumMatch p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 15);
            p2.ptm_list = new List<Ptm>() { new Ptm(2, mod)};
            SpectrumMatch p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 3, 25);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Add("P32329", new List<SpectrumMatch>() { p1, p2, p3 });

            var result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(1, result.Rows.Count);
            Assert.AreEqual(1, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@2", result.Rows[0].ItemArray[5]);

            //should have 2 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 22, 30);
            p1.ptm_list = new List<Ptm>() { new Ptm(26, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 26, 60);
            p2.ptm_list = new List<Ptm>() { new Ptm(26, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 65, 80);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };
            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(2, result.Rows.Count);
            Assert.AreEqual(22, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@26", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(2, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Unmodified", result.Rows[1].ItemArray[5]);

            //should have 1 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            p1.ptm_list = new List<Ptm>() { new Ptm(2, mod), new Ptm(9, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 15);
            p2.ptm_list = new List<Ptm>() { new Ptm(2, mod), new Ptm(9, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 3, 25);
            p3.ptm_list = new List<Ptm>() { new Ptm(9, mod) };
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };

            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(1, result.Rows.Count);
            Assert.AreEqual(1, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@2; Acetyl@9", result.Rows[0].ItemArray[5]);

            //should have 2 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 22, 30);
            p1.ptm_list = new List<Ptm>() { new Ptm(26, mod), new Ptm(65, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 26, 60);
            p2.ptm_list = new List<Ptm>() { new Ptm(26, mod), new Ptm(65, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 65, 80);
            p3.ptm_list = new List<Ptm>() { new Ptm(65, mod) };
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };
            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(2, result.Rows.Count);
            Assert.AreEqual(22, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@26; Acetyl@65", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(2, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Acetyl@65", result.Rows[1].ItemArray[5]);

            //should have 2 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            p1.ptm_list = new List<Ptm>() { new Ptm(2, mod), new Ptm(9, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 11, 22);
            p2.ptm_list = new List<Ptm>() { new Ptm(11, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 2, 10);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };

            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(2, result.Rows.Count);
            Assert.AreEqual(1, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@2; Acetyl@9", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(2, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Acetyl@11", result.Rows[1].ItemArray[5]);

            //should have 2 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            p1.ptm_list = new List<Ptm>() { new Ptm(2, mod), new Ptm(9, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 11, 22);
            p2.ptm_list = new List<Ptm>() { new Ptm(11, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 11, 22);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };

            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(2, result.Rows.Count);
            Assert.AreEqual(1, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@2; Acetyl@9", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(2, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Acetyl@11", result.Rows[1].ItemArray[5]);

            //should have 3 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            p1.ptm_list = new List<Ptm>() { new Ptm(2, mod), new Ptm(9, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 11, 22);
            p2.ptm_list = new List<Ptm>() { new Ptm(11, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 9, 22);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };

            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(3, result.Rows.Count);
            Assert.AreEqual(1, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@2; Acetyl@9", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(2, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Acetyl@11", result.Rows[1].ItemArray[5]);
            Assert.AreEqual(2, result.Rows[2].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[2].ItemArray[3]);
            Assert.AreEqual("Unmodified", result.Rows[2].ItemArray[5]);

            //should have 3 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            p1.ptm_list = new List<Ptm>() { new Ptm(2, mod), new Ptm(9, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 11, 22);
            p2.ptm_list = new List<Ptm>() { new Ptm(11, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };

            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(3, result.Rows.Count);
            Assert.AreEqual(1, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@2; Acetyl@9", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(2, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Acetyl@11", result.Rows[1].ItemArray[5]);
            Assert.AreEqual(1, result.Rows[2].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[2].ItemArray[3]);
            Assert.AreEqual("Unmodified", result.Rows[2].ItemArray[5]);

            //should have 3 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            p1.ptm_list = new List<Ptm>() { new Ptm(2, mod), new Ptm(9, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            p2.ptm_list = new List<Ptm>() { new Ptm(2, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 1, 10);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };

            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(3, result.Rows.Count);
            Assert.AreEqual(1, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@2; Acetyl@9", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(1, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Acetyl@2", result.Rows[1].ItemArray[5]);
            Assert.AreEqual(1, result.Rows[2].ItemArray[2]);
            Assert.AreEqual(569, result.Rows[2].ItemArray[3]);
            Assert.AreEqual("Unmodified", result.Rows[2].ItemArray[5]);

            //should have 3 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 60, 67);
            p1.ptm_list = new List<Ptm>() { new Ptm(60, mod), new Ptm(62, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 60, 67);
            p2.ptm_list = new List<Ptm>() { new Ptm(60, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 60, 67);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };

            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(3, result.Rows.Count);
            Assert.AreEqual(22, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@60; Acetyl@62", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(22, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Acetyl@60", result.Rows[1].ItemArray[5]);
            Assert.AreEqual(22, result.Rows[2].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[2].ItemArray[3]);
            Assert.AreEqual("Unmodified", result.Rows[2].ItemArray[5]);

            //should have 3 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 22, 30);
            p1.ptm_list = new List<Ptm>() { new Ptm(25, mod), new Ptm(27, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 22, 30);
            p2.ptm_list = new List<Ptm>() { new Ptm(25, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 22, 30);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };

            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(3, result.Rows.Count);
            Assert.AreEqual(22, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@25; Acetyl@27", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(22, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Acetyl@25", result.Rows[1].ItemArray[5]);
            Assert.AreEqual(22, result.Rows[2].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[2].ItemArray[3]);
            Assert.AreEqual("Unmodified", result.Rows[2].ItemArray[5]);


            //should have 2 pform
            p1 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 22, 30);
            p1.ptm_list = new List<Ptm>() { new Ptm(25, mod), new Ptm(27, mod) };
            p2 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 22, 25);
            p2.ptm_list = new List<Ptm>() { new Ptm(25, mod) };
            p3 = ConstructorsForTesting.SpectrumMatch("P32329", 100, 100, 22, 30);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession["P32329"] = new List<SpectrumMatch>() { p1, p2, p3 };
            result = ResultsSummaryGenerator.putative_proteoforms_bottom_up();
            Assert.AreEqual(2, result.Rows.Count);
            Assert.AreEqual(22, result.Rows[0].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[0].ItemArray[3]);
            Assert.AreEqual("Acetyl@25; Acetyl@27", result.Rows[0].ItemArray[5]);
            Assert.AreEqual(22, result.Rows[1].ItemArray[2]);
            Assert.AreEqual(67, result.Rows[1].ItemArray[3]);
            Assert.AreEqual("Unmodified", result.Rows[1].ItemArray[5]);

        }



        #endregion Results Summary
    }
}