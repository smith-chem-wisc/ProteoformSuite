using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Proteomics;

namespace Test
{
    [TestFixture]
    public class TestTheoreticalDatabaseCreate
    {

        [OneTimeSetUp]
        public void setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        [Test]
        public void test_contaminant_check()
        {
            InputFile f = new InputFile();
            f.ContaminantDB = true;
            InputFile g = new InputFile();
            InputFile h = new InputFile();
            Protein p1 = new Protein("", "T1", new Dictionary<int, List<Modification>>(), null, null, new string[0], "T2", "T3", true, false, new List<GoTerm>());
            Protein p2 = new Protein("", "T1", new Dictionary<int, List<Modification>>(), null, null, new string[0], "T2", "T3", true, false, new List<GoTerm>());
            Protein p3 = new Protein("", "T1", new Dictionary<int, List<Modification>>(), null, null, new string[0], "T2", "T3", true, false, new List<GoTerm>());
            Dictionary<InputFile, Protein[]> dict = new Dictionary<InputFile, Protein[]> {
                { f, new Protein[] { p1 } },
                { g, new Protein[] { p2 } },
                { h, new Protein[] { p3 } },
            };
            TheoreticalProteoform t = new TheoreticalProteoform("T1_asdf");
            TheoreticalProteoform u = new TheoreticalProteoform("T2_asdf_asdf");
            TheoreticalProteoform v = new TheoreticalProteoform("T3_asdf_Asdf_Asdf");
            t.check_contaminant_status(dict);
            u.check_contaminant_status(dict);
            v.check_contaminant_status(dict);
            Assert.True(t.contaminant);
            Assert.False(u.contaminant);
            Assert.False(v.contaminant);
            TheoreticalProteoform w = new TheoreticalProteoformGroup(new List<TheoreticalProteoform> { v, u, t });
            w.check_contaminant_status(dict); // the override is used
            Assert.True(w.contaminant);
        }


        [Test]
        public void testTheoreticalDatabaseCreateWithPTMs()
        {
            Lollipop.methionine_oxidation = false;
            Lollipop.carbamidomethylation = true;
            Lollipop.methionine_cleavage = true;
            Lollipop.natural_lysine_isotope_abundance = false;
            Lollipop.neucode_light_lysine = true;
            Lollipop.neucode_heavy_lysine = false;
            Lollipop.max_ptms = 3;
            Lollipop.decoy_databases = 1;
            Lollipop.min_peptide_length = 7;
            Lollipop.ptmset_mass_tolerance = 0.00001;
            Lollipop.combine_identical_sequences = true;
            Lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2]);
            Lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2]);

            Lollipop.get_theoretical_proteoforms();
            Assert.AreEqual(29, Lollipop.proteoform_community.theoretical_proteoforms.Length);

            int peptide = 0;
            int chain = 0;
            int Full_MetCleaved = 0;
            int propeptide = 0;
            int signalPeptide = 0;

            foreach (TheoreticalProteoform p in Lollipop.proteoform_community.theoretical_proteoforms)
            {
                switch (p.fragment)
                {
                    case "peptide":
                        peptide++;
                        break;
                    case "chain":
                        chain++;
                        break;
                    case "full-met-cleaved":
                        Full_MetCleaved++;
                        break;
                    case "propeptide":
                        propeptide++;
                        break;
                    case "signal-peptide":
                        signalPeptide++;
                        break;
                    default:
                        break;
                }

            }
            Assert.AreEqual(9, chain);
            Assert.AreEqual(12, Full_MetCleaved);
            Assert.AreEqual(2, peptide);
            Assert.AreEqual(3, propeptide);
            Assert.AreEqual(3, signalPeptide);
        }

        [Test]
        public void testTheoreticalDatabaseCreateWithoutPTMs()
        {
            Lollipop.methionine_oxidation = false;
            Lollipop.carbamidomethylation = true;
            Lollipop.methionine_cleavage = true;
            Lollipop.natural_lysine_isotope_abundance = false;
            Lollipop.neucode_light_lysine = true;
            Lollipop.neucode_heavy_lysine = false;
            Lollipop.max_ptms = 0;
            Lollipop.decoy_databases = 1;
            Lollipop.min_peptide_length = 7;
            Lollipop.ptmset_mass_tolerance = 0.00001;
            Lollipop.combine_identical_sequences = true;
            Lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2]);
            Lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2]);

            Lollipop.get_theoretical_proteoforms();
            Assert.AreEqual(26, Lollipop.proteoform_community.theoretical_proteoforms.Length);

            int peptide = 0;
            int chain = 0;
            int Full_MetCleaved = 0;
            int propeptide = 0;
            int signalPeptide = 0;

            foreach (TheoreticalProteoform p in Lollipop.proteoform_community.theoretical_proteoforms)
            {
                switch (p.fragment)
                {
                    case "peptide":
                        peptide++;
                        break;
                    case "chain":
                        chain++;
                        break;
                    case "full-met-cleaved":
                        Full_MetCleaved++;
                        break;
                    case "propeptide":
                        propeptide++;
                        break;
                    case "signal-peptide":
                        signalPeptide++;
                        break;
                    default:
                        break;
                }

            }
            Assert.AreEqual(9, chain);
            Assert.AreEqual(9, Full_MetCleaved);
            Assert.AreEqual(2, peptide);
            Assert.AreEqual(3, propeptide);
            Assert.AreEqual(3, signalPeptide);
        }
    }
}
