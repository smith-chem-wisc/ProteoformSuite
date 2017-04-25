using Chemistry;
using NetSerializer;
using NUnit.Framework;
using Proteomics;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UsefulProteomicsDatabases;

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
            StringBuilder builder = SaveState.save_method();
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

            SaveState.open_method(builder.ToString());
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
            SaveState.lollipop.proteoform_community.families = new List<ProteoformFamily> { f };
            string[] lines = ResultsSummaryGenerator.results_dataframe().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.True(lines.Count() == 3);
            Assert.True(lines.Any(a => a.Contains("E1")));
        }

        #endregion Results Summary


        [Test]
        public void basic_serialization()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
        }

        [Test]
        public void save_and_load_grouped_components()
        {
            //reading in test excel file, process raw components before testing neucode pairs.
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.correctionFactors = null;
            SaveState.lollipop.raw_experimental_components.Clear();
            Func<InputFile, IEnumerable<Component>> componentReader = c => new ComponentReader().read_components_from_xlsx(c, SaveState.lollipop.correctionFactors);
            InputFile noisy = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "noisy.xlsx"), Labeling.NeuCode, Purpose.Identification);
            SaveState.lollipop.input_files.Add(noisy);

            string inFileId = noisy.UniqueId.ToString();

            SaveState.lollipop.neucode_labeled = true;
            SaveState.lollipop.process_raw_components(SaveState.lollipop.input_files, SaveState.lollipop.raw_experimental_components, Purpose.Identification);
            SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
        }

        [Test]
        public void single_pf_serialization()
        {
            SaveState.lollipop = new Lollipop();
            Serializer ser = new Serializer(new Type[] { typeof(Proteoform) });
            Proteoform exp = new Proteoform("");
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "exp")))
                ser.Serialize(file, exp);

            Proteoform exp2;
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "exp")))
                exp2 = (Proteoform)ser.Deserialize(file);
        }

        [Test]
        public void single_epf_serialization()
        {
            SaveState.lollipop = new Lollipop();
            Serializer ser = new Serializer(new Type[] { typeof(ExperimentalProteoform) });
            ExperimentalProteoform exp = ConstructorsForTesting.ExperimentalProteoform("");
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "exp")))
                ser.Serialize(file, exp);

            ExperimentalProteoform exp2;
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "exp")))
                exp2 = (ExperimentalProteoform)ser.Deserialize(file);
        }

        [Test]
        public void single_adv_exp_serialization()
        {
            SaveState.lollipop = new Lollipop();
            Serializer ser = new Serializer(new Type[] { typeof(ExperimentalProteoform) });
            ExperimentalProteoform exp = ConstructorsForTesting.ExperimentalProteoform("", new Component(), new List<Component>(), new List<Component>(), true);
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "exp")))
                ser.Serialize(file, exp);

            ExperimentalProteoform exp2;
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "exp")))
                exp2 = (ExperimentalProteoform)ser.Deserialize(file);
        }

        [Test]
        public void single_quantitative_value_serialization()
        {
            SaveState.lollipop = new Lollipop();
            Serializer ser = new Serializer(new Type[] { typeof(QuantitativeProteoformValues) });
            Component root = new Component();
            QuantitativeProteoformValues exp = new QuantitativeProteoformValues(ConstructorsForTesting.ExperimentalProteoform("", root, new List<Component> { root }, new List<Component>(), true));
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "exp")))
                ser.Serialize(file, exp);

            QuantitativeProteoformValues exp2;
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "exp")))
                exp2 = (QuantitativeProteoformValues)ser.Deserialize(file);
        }

        [Test]
        public void exp_serialization()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.proteoform_community.experimental_proteoforms = new ExperimentalProteoform[] { ConstructorsForTesting.ExperimentalProteoform("", new Component(), new List<Component>(), new List<Component>(), true) };
            SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            SaveState.lollipop.proteoform_community.experimental_proteoforms = new ExperimentalProteoform[0];
            SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            Assert.AreEqual(1, SaveState.lollipop.proteoform_community.experimental_proteoforms.Length);
        }

        [Test]
        public void single_theo_serialization()
        {
            SaveState.lollipop = new Lollipop();
            Serializer ser = new Serializer(new Type[]
            {
                typeof(TheoreticalProteoform),
                typeof(List<ProteinWithGoTerms>),
                typeof(List<Tuple<string,string>>),
                typeof(Protein),
                typeof(ModificationWithLocation),
                typeof(ModificationWithMass),
                typeof(ModificationWithMassAndCf),
                typeof(List<DatabaseReference>),
                typeof(List<Tuple<string,string>>),
                typeof(Dictionary<int, List<Modification>>),
                typeof(Dictionary<string, IList<string>>),
                typeof(List<ProteolysisProduct>),
                typeof(ChemicalFormulaTerminus),
                typeof(List<double>)
            });
            TheoreticalProteoform exp = ConstructorsForTesting.make_a_theoretical();
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "theo")))
                ser.Serialize(file, exp);

            TheoreticalProteoform exp2;
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "theo")))
                exp2 = (TheoreticalProteoform)ser.Deserialize(file);
        }

        [Test]
        public void theo_serialization()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[] { ConstructorsForTesting.make_a_theoretical() };
            SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            SaveState.lollipop.proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            Assert.AreEqual(1, SaveState.lollipop.proteoform_community.theoretical_proteoforms.Length);
        }

        [Test]
        public void relation_serialization()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.proteoform_community.relations_in_peaks = new List<ProteoformRelation> { ConstructorsForTesting.make_relation(ConstructorsForTesting.ExperimentalProteoform("", new Component(), new List<Component>(), new List<Component>(), true), ConstructorsForTesting.make_a_theoretical(), ProteoformComparison.ExperimentalTheoretical, 100.01) };
            SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            SaveState.lollipop.proteoform_community.relations_in_peaks.Clear();
            SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            Assert.AreEqual(1, SaveState.lollipop.proteoform_community.relations_in_peaks.Count);
        }

        [Test]
        public void family_serialization()
        {
            SaveState.lollipop = new Lollipop();
            Serializer ser = new Serializer(new Type[] {
                typeof(ProteoformFamily),
                typeof(TheoreticalProteoform),
                typeof(List<ProteinWithGoTerms>),
                typeof(List<Tuple<string,string>>),
                typeof(Protein),
                typeof(ModificationWithLocation),
                typeof(ModificationWithMass),
                typeof(ModificationWithMassAndCf),
                typeof(List<DatabaseReference>),
                typeof(List<Tuple<string,string>>),
                typeof(Dictionary<int, List<Modification>>),
                typeof(Dictionary<string, IList<string>>),
                typeof(List<ProteolysisProduct>),
                typeof(ChemicalFormulaTerminus),
                typeof(List<double>)
            });
            TestProteoformFamilies.construct_two_families_with_potentially_colliding_theoreticals();
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "fam")))
                ser.Serialize(file, SaveState.lollipop.proteoform_community.families[0]);

            ProteoformFamily fam;
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "fam")))
                fam = (ProteoformFamily)ser.Deserialize(file);
        }

        [Test]
        public void intermediate_serialization() //Nested objects in ProteoformRelation and ProteoformCommunity are throwing this for a loop
        {
            SaveState.lollipop = new Lollipop();
            TestProteoformFamilies.construct_two_families_with_potentially_colliding_theoreticals();
            SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
        }

        [Test]
        public void protein_dict_serialization()
        {
            Serializer ser = new Serializer(new Type[] { 
                typeof(List<Protein>),
                typeof(ModificationWithLocation),
                typeof(ModificationWithMass),
                typeof(ModificationWithMassAndCf),
                typeof(List<DatabaseReference>),
                typeof(List<Tuple<string,string>>),
                typeof(Dictionary<int, List<Modification>>),
                typeof(Dictionary<string, IList<string>>),
                typeof(List<ProteolysisProduct>),
            });

            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            ModificationWithMass m = new ModificationWithMass("asdf", new Tuple<string, string>("", ""), motif, ModificationSites.K, 10.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "");
            ModificationWithMassAndCf mcf = new ModificationWithMassAndCf("asdf", new Tuple<string, string>("", ""), motif, ModificationSites.K, new ChemicalFormula(), 10.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "");

            List<Modification> nice = new List<Modification>
            {
                new Modification("", ""),
                new ModificationWithLocation("fayk",null, null,ModificationSites.A,null,  null),
                m,
                mcf
            };
            var ok = ProteinDbLoader.LoadProteinXML(Path.Combine(TestContext.CurrentContext.TestDirectory, @"xml2.xml"), false, nice, false, null, out Dictionary<string, Modification> un);

            List<Protein> prots;
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "prots")))
                ser.Serialize(file, ok);
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "prots")))
                prots = (List<Protein>)ser.Deserialize(file);
        }

        [Test]
        public void proteinwithgo_dict_serialization()
        {
            Serializer ser = new Serializer(new Type[] {
                typeof(List<ProteinWithGoTerms>),
                typeof(ModificationWithLocation),
                typeof(ModificationWithMass),
                typeof(ModificationWithMassAndCf),
                typeof(List<DatabaseReference>),
                typeof(List<Tuple<string,string>>),
                typeof(Dictionary<int, List<Modification>>),
                typeof(Dictionary<string, IList<string>>),
                typeof(List<ProteolysisProduct>),
            });

            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            ModificationWithMass m = new ModificationWithMass("asdf", new Tuple<string, string>("", ""), motif, ModificationSites.K, 10.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "");
            ModificationWithMassAndCf mcf = new ModificationWithMassAndCf("asdf", new Tuple<string, string>("", ""), motif, ModificationSites.K, new ChemicalFormula(), 10.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "");

            List<Modification> nice = new List<Modification>
            {
                new Modification("", ""),
                new ModificationWithLocation("fayk",null, null,ModificationSites.A,null,  null),
                m,
                mcf
            };
            List<Protein> ok = ProteinDbLoader.LoadProteinXML(Path.Combine(TestContext.CurrentContext.TestDirectory, @"xml2.xml"), false, nice, false, null, out Dictionary<string, Modification> un);
            ProteinWithGoTerms[] okok = TheoreticalProteoformDatabase.expand_protein_entries(ok.ToArray());

            ProteinWithGoTerms[] prots;
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "prots")))
                ser.Serialize(file, okok);
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "prots")))
                prots = (ProteinWithGoTerms[])ser.Deserialize(file);
        }

        [Test]
        public void ptmsets_serialization()
        {
            Serializer ser = new Serializer(new Type[] {
                typeof(List<ModificationWithMass>),
                typeof(List<PtmSet>),
                typeof(ModificationWithLocation),
                typeof(ModificationWithMass),
                typeof(ModificationWithMassAndCf),
                typeof(List<DatabaseReference>),
                typeof(List<double>),
                typeof(Dictionary<int, List<Modification>>),
                typeof(Dictionary<string, IList<string>>),
                typeof(List<string>),
            });

            Loaders.LoadElements(Path.Combine(TestContext.CurrentContext.TestDirectory, "elements.txt"));
            List<ModificationWithMass> mods = PtmListLoader.ReadModsFromFile(Path.Combine(TestContext.CurrentContext.TestDirectory, @"ptmlist.txt")).OfType<ModificationWithMass>().ToList();
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(1, mods[0]) });

            List<ModificationWithMass> asdf;
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdf")))
                ser.Serialize(file, mods);
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdf")))
                asdf = (List<ModificationWithMass>)ser.Deserialize(file);

            List<PtmSet> asdfg;
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
                ser.Serialize(file, new List<PtmSet> { set });
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
                asdfg = (List<PtmSet>)ser.Deserialize(file);
        }

        [Test]
        public void db_serialization()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));

            Serializer ser = new Serializer(new Type[] {
                typeof(TheoreticalProteoformDatabase),
                typeof(TheoreticalProteoform),
                typeof(List<ProteinWithGoTerms>),
                typeof(List<Tuple<string,string>>),
                typeof(Protein),
                typeof(ModificationWithLocation),
                typeof(ProteinSequenceGroup),
                typeof(ModificationWithMass),
                typeof(ModificationWithMassAndCf),
                typeof(List<DatabaseReference>),
                typeof(List<Tuple<string,string>>),
                typeof(Dictionary<int, List<Modification>>),
                typeof(Dictionary<string, IList<string>>),
                typeof(List<ProteolysisProduct>),
                typeof(ChemicalFormulaTerminus),
                typeof(List<double>)
            });

            SaveState.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet> { SaveState.lollipop.theoretical_database.all_possible_ptmsets[0] }; // make the test faster

            //List<GoTerm> asdfg;
            //using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    ser.Serialize(file, SaveState.lollipop.theoretical_database.expanded_proteins[0].GoTerms);
            //using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    asdfg = (List<GoTerm>)ser.Deserialize(file);

            //ProteinWithGoTerms[] asdfgh;
            //using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    ser.Serialize(file, SaveState.lollipop.theoretical_database.expanded_proteins);
            //using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    asdfgh = (ProteinWithGoTerms[])ser.Deserialize(file);

            //Dictionary<string, List<Modification>> j;
            //using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    ser.Serialize(file, SaveState.lollipop.theoretical_database.uniprotModifications);
            //using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    j = (Dictionary<string, List<Modification>>)ser.Deserialize(file);

            //Dictionary<InputFile, Protein[]> k;
            //using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    ser.Serialize(file, SaveState.lollipop.theoretical_database.theoretical_proteins);
            //using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    k = (Dictionary<InputFile, Protein[]>)ser.Deserialize(file);

            //Dictionary<double, List<PtmSet>> asd;
            //using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    ser.Serialize(file, SaveState.lollipop.theoretical_database.possible_ptmset_dictionary);
            //using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfg")))
            //    asd = (Dictionary<double, List<PtmSet>>)ser.Deserialize(file);

            TheoreticalProteoformDatabase db;
            using (var file = File.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, "theodb")))
                ser.Serialize(file, SaveState.lollipop.theoretical_database);
            using (var file = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "theodb")))
                db = (TheoreticalProteoformDatabase)ser.Deserialize(file);
        }

        [Test]
        public void db_in_lollipop_serialization()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            SaveState.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet> { SaveState.lollipop.theoretical_database.all_possible_ptmsets[0] }; // make the test faster
            SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
        }
    }
}
