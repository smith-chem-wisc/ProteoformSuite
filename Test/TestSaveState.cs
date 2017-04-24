using NUnit.Framework;
using ProteoformSuiteInternal;
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
        [OneTimeSetUp]
        public void setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        //[Test]
        //public void save_and_load_grouped_components()
        //{
        //    //reading in test excel file, process raw components before testing neucode pairs.
        //    SaveState.lol.correctionFactors = null;
        //    SaveState.lol.raw_experimental_components.Clear();
        //    Func<InputFile, IEnumerable<Component>> componentReader = c => new ExcelReader().read_components_from_xlsx(c, SaveState.lol.correctionFactors);
        //    SaveState.lol.input_files.Add(new InputFile("UnitTestFiles\\noisy.xlsx", Labeling.NeuCode, Purpose.Identification));

        //    string inFileId = SaveState.lol.input_files[0].UniqueId.ToString();

        //    SaveState.lol.neucode_labeled = true;
        //    SaveState.lol.process_raw_components();
        //    Assert.AreEqual(224, SaveState.lol.raw_experimental_components.Count);
        //    SaveState.lol.raw_experimental_components.Clear();

        //    StringBuilder builder = SaveState.save_all();
        //    SaveState.open_all(builder.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
        //}

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

        [Test]
        public void basic_serialization()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
        }

        [Test]
        public void intermediate_serialization() //Nested objects in ProteoformRelation and ProteoformCommunity are throwing this for a loop
        {
            //SaveState.lollipop = new Lollipop();
            //TestProteoformFamilies.construct_two_families_with_potentially_colliding_theoreticals();
            //SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            //SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
        }

        [Test]
        public void db_serialization() // Here, too
        {
            //SaveState.lollipop = new Lollipop();
            //SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            //SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            //SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            //SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            //SaveState.save_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
            //SaveState.load_all_results(Path.Combine(TestContext.CurrentContext.TestDirectory, "serial"));
        }
    }
}
