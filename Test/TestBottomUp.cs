using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProteoformSuiteInternal;
using System.IO;

namespace Test
{
    [TestFixture]
    class TestBottomUp
    {
        [Test]
        public void test_bottomUp_shared_protein()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "testBottomUp.psmtsv") }, Lollipop.acceptable_extensions[7], Lollipop.file_types[7], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            var bottom_up_psms = Sweet.lollipop.bottomupReader.ReadTDFile(Sweet.lollipop.input_files.Where(b => b.purpose == Purpose.BottomUp).First());
            Assert.AreEqual(2, bottom_up_psms.Count(p => p.shared_protein));
        }

        [Test]
        public void test_get_bottom_up_evidence_for_all_PTMs()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            var mod = Sweet.lollipop.theoretical_database.all_mods_with_mass.Where(m => m.OriginalId == "Phosphorylation").First();

            //not ptm specific
            var peptide = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 1, 10);
            peptide.ptm_list.Add(new Ptm(2, mod));
            bool BU_evidence_for_all_PTMs = Proteoform.get_bottom_up_evidence_for_all_PTMs(new List<SpectrumMatch>() { peptide }, new PtmSet(new List<Ptm>() { new Ptm(3, mod) }), false);
            Assert.IsTrue(BU_evidence_for_all_PTMs);
            BU_evidence_for_all_PTMs = Proteoform.get_bottom_up_evidence_for_all_PTMs(new List<SpectrumMatch>() { peptide }, new PtmSet(new List<Ptm>() { new Ptm(3, mod)}), true);
            Assert.IsFalse(BU_evidence_for_all_PTMs);
            BU_evidence_for_all_PTMs = Proteoform.get_bottom_up_evidence_for_all_PTMs(new List<SpectrumMatch>() { peptide }, new PtmSet(new List<Ptm>() { new Ptm(2, mod) }), true);
            Assert.IsTrue(BU_evidence_for_all_PTMs);

            var t = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(m => Math.Round(m.ptm_mass) == 80).First();
            t.bottom_up_PSMs.Add(ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 1, 10));
            Assert.IsFalse(t.bottom_up_evidence_for_all_PTMs);
            t.bottom_up_PSMs.Add(peptide);
            Assert.IsTrue(t.bottom_up_evidence_for_all_PTMs);
        }


        [Test]
        public void test_bu_ptms()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            var mod = Sweet.lollipop.theoretical_database.all_mods_with_mass.Where(m => m.OriginalId == "Phosphorylation").First();

            var peptide = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 1, 10);
            var peptide2 = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 1, 10);
            var peptide3 = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 1, 10);
            var peptide4 = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 1, 10);

            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Add("A", new List<SpectrumMatch>() { peptide, peptide2, peptide3 , peptide4});
            var topdown = ConstructorsForTesting.TopDownProteoform("A", 10000, 10);
            topdown.ambiguous_topdown_hits.Add(ConstructorsForTesting.SpectrumMatch("A", 10000, 10, 2, 250));
            topdown.topdown_bottom_up_PSMs.Add(peptide);
            topdown.ambiguous_topdown_hits.First().bottom_up_PSMs.Add(peptide2);
            topdown.ambiguous_topdown_hits.First().bottom_up_PSMs.Add(peptide3);
            peptide.ptm_list = new List<Ptm>() { new Ptm(2, mod) };
            peptide2.ptm_list = new List<Ptm>() { new Ptm(3, mod), new Ptm(4, mod) };
            peptide3.ptm_list = new List<Ptm>() { new Ptm(4, mod) };
            peptide4.ptm_list = new List<Ptm>() { new Ptm(5, mod) };


            topdown.topdown_ptm_set = new PtmSet(new List<Ptm>() { new Ptm(2, mod) });
            topdown.ambiguous_topdown_hits.First().ptm_list.Add(new Ptm(3, mod));
            topdown.ambiguous_topdown_hits.First().ptm_list.Add(new Ptm(4, mod));
            Assert.AreEqual("Phospho@2 | Phospho@3, Phospho@4", topdown.bu_PTMs);
            Assert.AreEqual("Phospho@2 | Phospho@3; Phospho@4, Phospho@4", topdown.bu_PTMs_separatepeptides);
            Assert.AreEqual("Phospho@2, Phospho@3, Phospho@4, Phospho@5 | Phospho@2, Phospho@3, Phospho@4, Phospho@5", topdown.setter_bu_PTMs_all_from_protein());
            topdown.topdown_bottom_up_PSMs.Clear();
            topdown.ambiguous_topdown_hits.First().bottom_up_PSMs.Clear();
            Assert.AreEqual("N/A | N/A", topdown.bu_PTMs);
            Assert.AreEqual("Phospho@2, Phospho@3, Phospho@4, Phospho@5 | Phospho@2, Phospho@3, Phospho@4, Phospho@5", topdown.setter_bu_PTMs_all_from_protein());
            Assert.AreEqual("N/A | N/A", topdown.bu_PTMs_separatepeptides);

        }

    }
}
