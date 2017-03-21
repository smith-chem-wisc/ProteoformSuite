using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.IO;
using Proteomics;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class TestTheoreticalDatabaseCreate
    {
        [Test]
        public void test_contaminant_check()
        {
            InputFile f = new InputFile("fake.txt", Purpose.ProteinDatabase);
            f.ContaminantDB = true;
            InputFile g = new InputFile("fake.txt", Purpose.ProteinDatabase);
            InputFile h = new InputFile("fake.txt", Purpose.ProteinDatabase);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            Dictionary<InputFile, Protein[]> dict = new Dictionary<InputFile, Protein[]> {
                { f, new Protein[] { p1 } },
                { g, new Protein[] { p2 } },
                { h, new Protein[] { p3 } },
            };
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("T1_T1_asdf", p1, dict);
            TheoreticalProteoform u = ConstructorsForTesting.make_a_theoretical("T2_T1_asdf_asdf", p2, dict);
            TheoreticalProteoform v = ConstructorsForTesting.make_a_theoretical("T3_T1_asdf_Asdf_Asdf", p3, dict);
            TheoreticalProteoform w = new TheoreticalProteoformGroup(new List<TheoreticalProteoform> { v, u, t }.OrderByDescending(theo => theo.contaminant ? 1 : 0));
            Assert.True(w.contaminant);
            Assert.True(w.accession.Contains(p1.Accession));

            //Not contaminant
            TheoreticalProteoform x = new TheoreticalProteoformGroup(new List<TheoreticalProteoform> { v, u });
            Assert.False(x.contaminant);

            //PTM mass test
            Assert.AreEqual(0, t.ptm_mass);
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
            Lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Lollipop.input_files);
            Lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Lollipop.input_files);

            Lollipop.theoretical_proteins.Clear();
            Lollipop.get_theoretical_proteoforms();
            Assert.AreEqual(54, Lollipop.theoretical_proteins.SelectMany(kv => kv.Value).Sum(p => p.DatabaseReferences.Where(dbRef => dbRef.Type == "GO").Count()));
            Assert.AreEqual(20, Lollipop.proteoform_community.theoretical_proteoforms.SelectMany(t => t.goTerms.Select(go => go.Id)).Distinct().Count());

            List<TheoreticalProteoform> peptides = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "peptide").ToList();
            List<TheoreticalProteoform> chains = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "chain").ToList();
            List<TheoreticalProteoform> fulls = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "full-met-cleaved").ToList();
            List<TheoreticalProteoform> propeps = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "propeptide").ToList();
            List<TheoreticalProteoform> signalpeps = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "signal-peptide").ToList();

            Assert.AreEqual(8, chains.Count);
            Assert.AreEqual(12, fulls.Count);
            Assert.AreEqual(2, peptides.Count);
            Assert.AreEqual(3, propeps.Count);
            Assert.AreEqual(3, signalpeps.Count);

            Assert.AreEqual(28, Lollipop.proteoform_community.theoretical_proteoforms.Length);
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
            Lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Lollipop.input_files);
            Lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Lollipop.input_files);

            Lollipop.theoretical_proteins.Clear();
            Lollipop.get_theoretical_proteoforms();

            List<TheoreticalProteoform> peptides = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "peptide").ToList();
            List<TheoreticalProteoform> chains = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "chain").ToList();
            List<TheoreticalProteoform> fulls = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "full-met-cleaved").ToList();
            List<TheoreticalProteoform> propeps = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "propeptide").ToList();
            List<TheoreticalProteoform> signalpeps = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "signal-peptide").ToList();

            Assert.AreEqual(8, chains.Count);
            Assert.AreEqual(9, fulls.Count);
            Assert.AreEqual(2, peptides.Count);
            Assert.AreEqual(3, propeps.Count);
            Assert.AreEqual(3, signalpeps.Count);

            Assert.AreEqual(25, Lollipop.proteoform_community.theoretical_proteoforms.Length);
        }
    }
}
