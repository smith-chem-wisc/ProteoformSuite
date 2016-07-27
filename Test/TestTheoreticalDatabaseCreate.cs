using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Linq;

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
            Lollipop.uniprot_xml_filepath = "UnitTestFiles\\uniprot_yeast_test_12entries.xml";
            Lollipop.ptmlist_filepath = "UnitTestFiles\\ptmlist.txt";

            Lollipop.get_theoretical_proteoforms();
            Assert.AreEqual(29, Lollipop.proteoform_community.theoretical_proteoforms.Count());

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
            Lollipop.uniprot_xml_filepath = "UnitTestFiles\\uniprot_yeast_test_12entries.xml";
            Lollipop.ptmlist_filepath = "UnitTestFiles\\ptmlist.txt";

            Lollipop.get_theoretical_proteoforms();
            Assert.AreEqual(26, Lollipop.proteoform_community.theoretical_proteoforms.Count());

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
