using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UsefulProteomicsDatabases;

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
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.methionine_oxidation = false;
            SaveState.lollipop.carbamidomethylation = true;
            SaveState.lollipop.methionine_cleavage = true;
            SaveState.lollipop.natural_lysine_isotope_abundance = false;
            SaveState.lollipop.neucode_light_lysine = true;
            SaveState.lollipop.neucode_heavy_lysine = false;
            SaveState.lollipop.max_ptms = 3;
            SaveState.lollipop.decoy_databases = 1;
            SaveState.lollipop.min_peptide_length = 7;
            SaveState.lollipop.ptmset_mass_tolerance = 0.00001;
            SaveState.lollipop.combine_identical_sequences = true;
            SaveState.lollipop.theoretical_database.limit_triples_and_greater = false;
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);

            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(54, SaveState.lollipop.theoretical_database.theoretical_proteins.SelectMany(kv => kv.Value).Sum(p => p.DatabaseReferences.Where(dbRef => dbRef.Type == "GO").Count()));
            Assert.AreEqual(20, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.SelectMany(t => t.goTerms.Select(go => go.Id)).Distinct().Count());

            List<TheoreticalProteoform> peptides = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "peptide").ToList();
            List<TheoreticalProteoform> chains = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "chain").ToList();
            List<TheoreticalProteoform> fulls = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "full-met-cleaved").ToList();
            List<TheoreticalProteoform> propeps = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "propeptide").ToList();
            List<TheoreticalProteoform> signalpeps = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "signal-peptide").ToList();

            Assert.AreEqual(8, chains.Count);
            Assert.AreEqual(12, fulls.Count);
            Assert.AreEqual(2, peptides.Count);
            Assert.AreEqual(3, propeps.Count);
            Assert.AreEqual(3, signalpeps.Count);

            Assert.AreEqual(28, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length);
        }

        [Test]
        public void testTheoreticalDatabaseCreateWithoutPTMs()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.methionine_oxidation = false;
            SaveState.lollipop.carbamidomethylation = true;
            SaveState.lollipop.methionine_cleavage = true;
            SaveState.lollipop.natural_lysine_isotope_abundance = false;
            SaveState.lollipop.neucode_light_lysine = true;
            SaveState.lollipop.neucode_heavy_lysine = false;
            SaveState.lollipop.max_ptms = 0;
            SaveState.lollipop.decoy_databases = 1;
            SaveState.lollipop.min_peptide_length = 7;
            SaveState.lollipop.ptmset_mass_tolerance = 0.00001;
            SaveState.lollipop.combine_identical_sequences = true;
            SaveState.lollipop.theoretical_database.limit_triples_and_greater = false;
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);

            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));

            List<TheoreticalProteoform> peptides = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "peptide").ToList();
            List<TheoreticalProteoform> chains = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "chain").ToList();
            List<TheoreticalProteoform> fulls = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "full-met-cleaved").ToList();
            List<TheoreticalProteoform> propeps = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "propeptide").ToList();
            List<TheoreticalProteoform> signalpeps = SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "signal-peptide").ToList();

            Assert.AreEqual(8, chains.Count);
            Assert.AreEqual(9, fulls.Count);
            Assert.AreEqual(2, peptides.Count);
            Assert.AreEqual(3, propeps.Count);
            Assert.AreEqual(3, signalpeps.Count);

            Assert.AreEqual(25, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length);
        }

        [Test]
        public void testTheoreticalWithAndWithoutVariableMethionineOx()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.methionine_oxidation = false;
            SaveState.lollipop.mod_types_to_exclude = SaveState.lollipop.mod_types_to_exclude.Concat(new string[] { "Uniprot" }).ToArray();
            SaveState.lollipop.methionine_cleavage = true;
            SaveState.lollipop.max_ptms = 0;
            SaveState.lollipop.input_files.Clear();
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "stripped.xml"), Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(1, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //no methionine oxidation

            SaveState.lollipop.methionine_oxidation = true;
            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(1, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //only one methionine to oxidize, but no PTMs allowed
            Assert.Less(SaveState.lollipop.modification_ranks[0], SaveState.lollipop.modification_ranks[SaveState.lollipop.theoretical_database.variableModifications[0].monoisotopicMass] - 1); // unmodified gets a lower score than variable oxidation, even with the prioritization (minus one rank)

            SaveState.lollipop.max_ptms = 1;
            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(2, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //only one methionine to oxidize
        }

        [Test]
        public void testTheoreticalWithMoreMethionineOx()
        {
            SaveState.lollipop = new Lollipop();
            SaveState.lollipop.methionine_oxidation = true;
            SaveState.lollipop.methionine_cleavage = true;
            SaveState.lollipop.max_ptms = 0;
            SaveState.lollipop.input_files.Clear();
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "stripped_plus2M.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(1, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //3 methionines for variable

            SaveState.lollipop.max_ptms = 1;
            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(2, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //three methionine sites to oxidize, but all three are equivalent, so just one more added

            SaveState.lollipop.max_ptms = 2;
            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(3, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //three methionine sites to oxidize, but all three are equivalent, so just one more added

            SaveState.lollipop.max_ptms = 3;
            SaveState.lollipop.theoretical_database.theoretical_proteins.Clear();
            SaveState.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            SaveState.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(4, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //three methionine sites to oxidize, but all three are equivalent, so just one more added
            Assert.AreEqual(1, new HashSet<ModificationWithMass>(SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.SelectMany(t => t.ptm_set.ptm_combination.Select(ptm => ptm.modification))).Count); //Only the variable modification is in there; no sign of the fake oxidation of K I added
        }

        [Test]
        public void test_get_modification_dictionary()
        {
            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(new string[] { TestContext.CurrentContext.TestDirectory, "Mods", "amino_acids.txt" }) }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            var mods = ConstructorsForTesting.read_mods();
            Assert.AreEqual(21, mods.Keys.Count);
            Assert.True(mods.Values.All(v => v.Count == 1));

            SaveState.lollipop.enter_input_files(new string[] { Path.Combine(new string[] { TestContext.CurrentContext.TestDirectory, "amino_acids_duplicates.txt" }) }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], SaveState.lollipop.input_files);
            mods = ConstructorsForTesting.read_mods();
            Assert.AreEqual(21, mods.Keys.Count);
            Assert.True(mods.Values.All(v => v.Count == 2)); // a bunch of duplicates, now
        }

        [Test]
        public void test_protein_grouping_by_sequence()
        {
            DatabaseReference d1 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d2 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d3 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            GoTerm g1 = new GoTerm(d1);
            GoTerm g2 = new GoTerm(d1);
            GoTerm g3 = new GoTerm(d1);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d1 }, new List<GoTerm> { g1 });
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d2 }, new List<GoTerm> { g2 });
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d3 }, new List<GoTerm> { g3 });
            ProteinSequenceGroup psg = new ProteinSequenceGroup(new List<ProteinWithGoTerms> { p1, p2, p3 }.OrderByDescending(p => p.IsContaminant ? 1 : 0));
            Assert.AreEqual(3, psg.GoTerms.Count());
            Assert.AreEqual(3, psg.GeneNames.Count());
            Assert.AreEqual("T1_3G", psg.Accession);
            Assert.False(psg.IsContaminant);

            p3 = new ProteinWithGoTerms("MCSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d3 }, new List<GoTerm> { g3 });
            ProteinSequenceGroup[] psgs = SaveState.lollipop.theoretical_database.group_proteins_by_sequence(new List<ProteinWithGoTerms> { p1, p2, p3 });
            Assert.AreEqual(2, psgs.Length);
        }

        [Test]
        public void test_protein_grouping_by_sequence_contaminant()
        {
            DatabaseReference d1 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d2 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d3 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            GoTerm g1 = new GoTerm(d1);
            GoTerm g2 = new GoTerm(d1);
            GoTerm g3 = new GoTerm(d1);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d1 }, new List<GoTerm> { g1 });
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d2 }, new List<GoTerm> { g2 });
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, true, new List<DatabaseReference> { d3 }, new List<GoTerm> { g3 });
            ProteinSequenceGroup psg = new ProteinSequenceGroup(new List<ProteinWithGoTerms> { p1, p2, p3 }.OrderByDescending(p => p.IsContaminant ? 1 : 0));
            Assert.AreEqual(3, psg.GoTerms.Count());
            Assert.AreEqual(3, psg.GeneNames.Count());
            Assert.AreEqual("T3_3G", psg.Accession);
            Assert.True(psg.IsContaminant);
        }


        [Test]
        public void test_not_ready_to_make_db_doesnt_crash()
        {
            TheoreticalProteoformDatabase tpd = new TheoreticalProteoformDatabase();
            tpd.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void group_by_mass()
        {
            TheoreticalProteoformDatabase tpd = new TheoreticalProteoformDatabase();
            List<TheoreticalProteoform> pfs_with_mass_redundancy = new List<TheoreticalProteoform>
            {
                ConstructorsForTesting.make_a_theoretical("", 100.0, 1), // bundled
                ConstructorsForTesting.make_a_theoretical("", 100.0, 1), // bundled
                ConstructorsForTesting.make_a_theoretical("", 100.1, 1), // different mass
                ConstructorsForTesting.make_a_theoretical("", 100.0, 2) // still gets bundled even if different lysine count
            };
            TheoreticalProteoformGroup[] x = tpd.group_proteoforms_by_mass(pfs_with_mass_redundancy);
            Assert.AreEqual(2, x.Length);
        }

        [Test]
        public void save_unlocalized_doesntcrash()
        {
            TheoreticalProteoformDatabase tpd = new TheoreticalProteoformDatabase();
            tpd.save_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfghjkl", "fake.txt")); // directory doesn't exist
        }

        [Test]
        public void load_unlocalized_doesntcrash()
        {
            TheoreticalProteoformDatabase tpd = new TheoreticalProteoformDatabase();
            tpd.load_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "asdfghjkl", "fake.txt"));
        }

        [Test]
        public void load_save_unlocalized()
        {
            TheoreticalProteoformDatabase tpd = new TheoreticalProteoformDatabase();
            tpd.ready_to_make_database(TestContext.CurrentContext.TestDirectory);
            List<ModificationWithMass> mods = PtmListLoader.ReadModsFromFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "variable.txt")).OfType<ModificationWithMass>().ToList();
            SaveState.lollipop.modification_ranks = mods.ToDictionary(m => m.monoisotopicMass, m => -1);
            tpd.unlocalized_lookup = tpd.make_unlocalized_lookup(mods);
            tpd.load_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "stored_mods.modnames"));
            tpd.save_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "fake_stored_mods.modnames"));
            Assert.AreNotEqual(mods.First().id, tpd.unlocalized_lookup.Values.First().id);

            //Test amending
            mods.AddRange(PtmListLoader.ReadModsFromFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "intact_mods.txt")).OfType<ModificationWithMass>());
            SaveState.lollipop.modification_ranks = mods.DistinctBy(m => m.monoisotopicMass).ToDictionary(m => m.monoisotopicMass, m => -1);
            tpd.unlocalized_lookup = tpd.make_unlocalized_lookup(mods.OfType<ModificationWithMass>());
            tpd.amend_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "fake_stored_mods.modnames"));
        }
    }
}
