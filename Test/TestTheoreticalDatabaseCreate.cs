using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using System.Text;
using System.Threading.Tasks;

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
        public void testmethod()
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
            Assert.AreEqual(27, Lollipop.proteoform_community.theoretical_proteoforms.Count());
        }
    }
}
