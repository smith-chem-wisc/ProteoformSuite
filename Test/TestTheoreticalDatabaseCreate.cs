using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UsefulProteomicsDatabases;

namespace Test
{
    [TestFixture]
    public class TestTheoreticalDatabaseCreate
    {
        [Test]
        public void load_uniprot_ptmlist()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.enter_uniprot_ptmlist(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.PtmList);
            Sweet.lollipop = new Lollipop();
        }

        [Test]
        public void test_contaminant_check()
        {
            InputFile f = new InputFile("fake.txt", Purpose.ProteinDatabase);
            f.ContaminantDB = true;
            InputFile g = new InputFile("fake.txt", Purpose.ProteinDatabase);
            InputFile h = new InputFile("fake.txt", Purpose.ProteinDatabase);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<GoTerm>());
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
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.methionine_oxidation = false;
            Sweet.lollipop.carbamidomethylation = true;
            Sweet.lollipop.methionine_cleavage = true;
            Sweet.lollipop.natural_lysine_isotope_abundance = false;
            Sweet.lollipop.neucode_light_lysine = true;
            Sweet.lollipop.neucode_heavy_lysine = false;
            Sweet.lollipop.max_ptms = 3;
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.min_peptide_length = 7;
            Sweet.lollipop.ptmset_mass_tolerance = 0.00001;
            Sweet.lollipop.combine_identical_sequences = true;
            Sweet.lollipop.theoretical_database.limit_triples_and_greater = false;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);

            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(54, Sweet.lollipop.theoretical_database.theoretical_proteins.SelectMany(kv => kv.Value).Sum(p => p.DatabaseReferences.Where(dbRef => dbRef.Type == "GO").Count()));
            Assert.AreEqual(20, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.SelectMany(t => t.goTerms.Select(go => go.Id)).Distinct().Count());

            List<TheoreticalProteoform> peptides = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "peptide").ToList();
            List<TheoreticalProteoform> chains = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "chain").ToList();
            List<TheoreticalProteoform> fulls = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "full-met-cleaved").ToList();
            List<TheoreticalProteoform> propeps = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "propeptide").ToList();
            List<TheoreticalProteoform> signalpeps = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "signal-peptide").ToList();

            Assert.AreEqual(8, chains.Count);
            Assert.AreEqual(12, fulls.Count);
            Assert.AreEqual(2, peptides.Count);
            Assert.AreEqual(3, propeps.Count);
            Assert.AreEqual(3, signalpeps.Count);

            Assert.AreEqual(28, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length);
        }

        [Test]
        public void testTheoreticalDatabaseCreateWithoutPTMs()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.methionine_oxidation = false;
            Sweet.lollipop.carbamidomethylation = true;
            Sweet.lollipop.methionine_cleavage = true;
            Sweet.lollipop.natural_lysine_isotope_abundance = false;
            Sweet.lollipop.neucode_light_lysine = true;
            Sweet.lollipop.neucode_heavy_lysine = false;
            Sweet.lollipop.max_ptms = 0;
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.min_peptide_length = 7;
            Sweet.lollipop.ptmset_mass_tolerance = 0.00001;
            Sweet.lollipop.combine_identical_sequences = true;
            Sweet.lollipop.theoretical_database.limit_triples_and_greater = false;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);

            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));

            List<TheoreticalProteoform> peptides = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "peptide").ToList();
            List<TheoreticalProteoform> chains = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "chain").ToList();
            List<TheoreticalProteoform> fulls = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "full-met-cleaved").ToList();
            List<TheoreticalProteoform> propeps = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "propeptide").ToList();
            List<TheoreticalProteoform> signalpeps = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.fragment == "signal-peptide").ToList();

            Assert.AreEqual(8, chains.Count);
            Assert.AreEqual(9, fulls.Count);
            Assert.AreEqual(2, peptides.Count);
            Assert.AreEqual(3, propeps.Count);
            Assert.AreEqual(3, signalpeps.Count);
            Assert.AreEqual(13, Sweet.lollipop.theoretical_database.theoreticals_by_accession[-100].Count);
            Assert.AreEqual(25, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length);

            //if don't combine by mass, need to make theoretical accession dictionary still
            Sweet.lollipop.combine_theoretical_proteoforms_byMass = false;
            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(13, Sweet.lollipop.theoretical_database.theoreticals_by_accession[-100].Count);
        }

        [Test]
        public void testTheoreticalWithAndWithoutVariableMethionineOx()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.methionine_oxidation = false;
            Sweet.lollipop.mod_types_to_exclude = Sweet.lollipop.mod_types_to_exclude.Concat(new string[] { "UniProt" }).ToArray();
            Sweet.lollipop.methionine_cleavage = true;
            Sweet.lollipop.max_ptms = 0;
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "stripped.xml"), Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //no methionine oxidation

            Sweet.lollipop.methionine_oxidation = true;
            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //only one methionine to oxidize, but no PTMs allowed
            Assert.Less(Sweet.lollipop.modification_ranks[0], Sweet.lollipop.modification_ranks[Sweet.lollipop.theoretical_database.variableModifications[0].monoisotopicMass] - 1); // unmodified gets a lower score than variable oxidation, even with the prioritization (minus one rank)

            Sweet.lollipop.max_ptms = 1;
            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //only one methionine to oxidize
        }

        [Test]
        public void testTheoreticalWithMoreMethionineOx()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.methionine_oxidation = true;
            Sweet.lollipop.methionine_cleavage = true;
            Sweet.lollipop.max_ptms = 0;
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "stripped_plus2M.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //3 methionines for variable

            Sweet.lollipop.max_ptms = 1;
            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //three methionine sites to oxidize, but all three are equivalent, so just one more added

            Sweet.lollipop.max_ptms = 2;
            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(3, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //three methionine sites to oxidize, but all three are equivalent, so just one more added

            Sweet.lollipop.max_ptms = 3;
            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(4, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length); //three methionine sites to oxidize, but all three are equivalent, so just one more added
            Assert.AreEqual(1, new HashSet<ModificationWithMass>(Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.SelectMany(t => t.ptm_set.ptm_combination.Select(ptm => ptm.modification))).Count); //Only the variable modification is in there; no sign of the fake oxidation of K I added
        }

        [Test]
        public void test_get_modification_dictionary()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(new string[] { TestContext.CurrentContext.TestDirectory, "Mods", "amino_acids.txt" }) }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            var mods = ConstructorsForTesting.read_mods();
            Assert.AreEqual(21, mods.Keys.Count);
            Assert.True(mods.Values.All(v => v.Count == 1));

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(new string[] { TestContext.CurrentContext.TestDirectory, "amino_acids_duplicates.txt" }) }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
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
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d1 }, new List<GoTerm> { g1 });
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d2 }, new List<GoTerm> { g2 });
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d3 }, new List<GoTerm> { g3 });
            ProteinSequenceGroup psg = new ProteinSequenceGroup(new List<ProteinWithGoTerms> { p1, p2, p3 }.OrderByDescending(p => p.IsContaminant ? 1 : 0));
            Assert.AreEqual(3, psg.GoTerms.Count());
            Assert.AreEqual(3, psg.GeneNames.Count());
            Assert.AreEqual("T1_3G", psg.Accession);
            Assert.False(psg.IsContaminant);

            p3 = new ProteinWithGoTerms("MCSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d3 }, new List<GoTerm> { g3 });
            ProteinSequenceGroup[] psgs = Sweet.lollipop.theoretical_database.group_proteins_by_sequence(new List<ProteinWithGoTerms> { p1, p2, p3 });
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
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d1 }, new List<GoTerm> { g1 });
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d2 }, new List<GoTerm> { g2 });
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, true, new List<DatabaseReference> { d3 }, new List<GoTerm> { g3 });
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
            Sweet.lollipop.modification_ranks = mods.ToDictionary(m => m.monoisotopicMass, m => -1);
            tpd.unlocalized_lookup = tpd.make_unlocalized_lookup(mods);
            tpd.load_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "stored_mods.modnames"));
            tpd.save_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "fake_stored_mods.modnames"));
            Assert.AreNotEqual(mods.First().id, tpd.unlocalized_lookup.Values.First().id);

            //Test amending
            mods.AddRange(PtmListLoader.ReadModsFromFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "intact_mods.txt")).OfType<ModificationWithMass>());
            Sweet.lollipop.modification_ranks = mods.DistinctBy(m => m.monoisotopicMass).ToDictionary(m => m.monoisotopicMass, m => -1);
            tpd.unlocalized_lookup = tpd.make_unlocalized_lookup(mods.OfType<ModificationWithMass>());
            tpd.amend_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "fake_stored_mods.modnames"));
        }

        //[Test]
        //public void bottom_up_peptides()
        //{
        //    Sweet.lollipop = new Lollipop();
        //    Sweet.lollipop.max_ptms = 0;
        //    Sweet.lollipop.methionine_oxidation = false;
        //    Sweet.lollipop.decoy_databases = 1;
        //    Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "BU_result_sliced-raw_5ppmAroundZero.mzid") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
        //    Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
        //    Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
        //    Sweet.lollipop.BottomUpPSMList.AddRange(BottomUpReader.ReadBUFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "BU_result_sliced-raw_5ppmAroundZero.mzid"), Sweet.lollipop.theoretical_database.theoreticals_by_accession.Values.ToList()));

        //    Assert.AreEqual(2, Sweet.lollipop.BottomUpPSMList.Count);
        //    Assert.AreEqual(1, Sweet.lollipop.BottomUpPSMList.Count(p => p.modifications.Count > 0));
        //    Assert.AreEqual(1, Sweet.lollipop.BottomUpPSMList.Where(p => p.modifications.Count > 0).First().modifications.Count);
        //    Assert.AreEqual("(3R,4R)-3,4-dihydroxyproline", Sweet.lollipop.BottomUpPSMList.Where(p => p.modifications.Count > 0).First().modifications.First().modification.id);
        //    Assert.AreEqual("EGFQVADGP[(3R,4R)-3,4-dihydroxyproline]LYR", Sweet.lollipop.BottomUpPSMList.Where(p => p.modifications.Count > 0).First().sequence_with_modifications);
        //    Assert.AreEqual("(3R,4R)-3,4-dihydroxyproline@10", Sweet.lollipop.BottomUpPSMList.Where(p => p.modifications.Count > 0).First().ptm_descriptions);
        //    Assert.AreEqual("Unmodified", Sweet.lollipop.BottomUpPSMList.Where(p => p.modifications.Count == 0).First().ptm_descriptions);
        //    Assert.AreEqual("EGFQVADGPLYR", Sweet.lollipop.BottomUpPSMList.Where(p => p.modifications.Count == 0).First().sequence_with_modifications);
        //    Assert.AreEqual(1, BottomUpReader.bottom_up_PTMs_not_in_dictionary.Count);
        //    Assert.AreEqual("PTM not in database", BottomUpReader.bottom_up_PTMs_not_in_dictionary.First());

        //    //accession matches one of the accessions of proteins collapsed into theoretical proteoforms (combine identical sequences/masses)
        //    Assert.AreEqual(3, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Count(t => t.psm_list.Count > 0));
        //    Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(t => t.psm_list.Count > 0).ToList()[0].psm_list.Count);
        //    Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(t => t.psm_list.Count > 0).ToList()[1].psm_list.Count);
        //    Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(t => t.psm_list.Count > 0).ToList()[2].psm_list.Count);
        //    //should also add to decoy communities
        //    Assert.AreEqual(3, Sweet.lollipop.decoy_proteoform_communities.First().Value.theoretical_proteoforms.Count(t => t.psm_list.Count > 0));
        //    Assert.AreEqual(2, Sweet.lollipop.decoy_proteoform_communities.First().Value.theoretical_proteoforms.Where(t => t.psm_list.Count > 0).ToList()[0].psm_list.Count);
        //    Assert.AreEqual(2, Sweet.lollipop.decoy_proteoform_communities.First().Value.theoretical_proteoforms.Where(t => t.psm_list.Count > 0).ToList()[1].psm_list.Count);
        //    Assert.AreEqual(2, Sweet.lollipop.decoy_proteoform_communities.First().Value.theoretical_proteoforms.Where(t => t.psm_list.Count > 0).ToList()[2].psm_list.Count);

        //}

        [Test]
        public void parallel_enter_theoreticals_doesnt_crash()
        {
            TheoreticalProteoformDatabase db = new TheoreticalProteoformDatabase();
            db.populate_aa_mass_dictionary();
            List<ModificationWithMass> var = new List<ModificationWithMass>();
            List<TheoreticalProteoform> ts = new List<TheoreticalProteoform>();
            ProteinWithGoTerms p = ConstructorsForTesting.make_a_theoretical().ExpandedProteinList.First();
            Parallel.Invoke(
                () => db.EnterTheoreticalProteformFamily("SEQ", p, p.OneBasedPossibleLocalizedModifications, p.Accession, ts, 1, var),
                () => db.EnterTheoreticalProteformFamily("SEQ", p, p.OneBasedPossibleLocalizedModifications, p.Accession, ts, 1, var)
            );
        }
    }
}
