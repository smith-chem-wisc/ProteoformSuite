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
        public void test_contaminant_check()
        {
            InputFile f = new InputFile("fake.txt", Purpose.ProteinDatabase);
            f.ContaminantDB = true;
            InputFile g = new InputFile("fake.txt", Purpose.ProteinDatabase);
            InputFile h = new InputFile("fake.txt", Purpose.ProteinDatabase);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<goTerm>());
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<goTerm>());
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference>(), new List<goTerm>());
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
        [TestCase("uniprot_yeast_test_12entries.xml")]
        public void testTheoreticalDatabaseCreateWithPTMs(string database)
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.methionine_oxidation = false;
            Sweet.lollipop.carbamidomethylation = true;
            Sweet.lollipop.methionine_cleavage = true;
            Sweet.lollipop.max_ptms = 3;
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.min_peptide_length = 7;
            Sweet.lollipop.ptmset_mass_tolerance = 0.00001;
            Sweet.lollipop.combine_identical_sequences = true;
            Sweet.lollipop.limit_triples_and_greater = false;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, database) }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
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
            Sweet.lollipop.max_ptms = 0;
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.min_peptide_length = 7;
            Sweet.lollipop.ptmset_mass_tolerance = 0.00001;
            Sweet.lollipop.combine_identical_sequences = true;
            Sweet.lollipop.limit_triples_and_greater = false;
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
            Assert.Less(Sweet.lollipop.modification_ranks[0], Sweet.lollipop.modification_ranks[Math.Round((double)Sweet.lollipop.theoretical_database.variableModifications[0].MonoisotopicMass, 5)] - 1); // unmodified gets a lower score than variable oxidation, even with the prioritization (minus one rank)

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
            Sweet.lollipop.mod_types_to_exclude = Sweet.lollipop.mod_types_to_exclude.Concat(new string[] { "UniProt", "Mod" }).ToArray();
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
            Assert.AreEqual(1, new HashSet<Modification>(Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.SelectMany(t => t.ptm_set.ptm_combination.Select(ptm => ptm.modification))).Count); //Only the variable modification is in there; no sign of the fake oxidation of K I added
        }

        [Test]
        public void test_get_modification_dictionary()
        {
            Sweet.lollipop = new Lollipop();
            var mods = ConstructorsForTesting.read_mods();
            Assert.AreEqual(478, mods.Keys.Count);
            //Assert.True(mods.Values.All(v => v.Count == 1));
        }

        [Test]
        public void test_bottom_up_PSMs()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.et_high_mass_difference = 2;
            Sweet.lollipop.et_low_mass_difference = -2;
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "testHits.psmtsv") }, Lollipop.acceptable_extensions[7], Lollipop.file_types[7], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Assert.AreEqual(10, Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Count);

            Sweet.lollipop.min_bu_peptides = 0;
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1");
            e.modified_mass = 1816.0128; //Q3E770 - no bottomup-peptides
            var relations = Sweet.lollipop.target_proteoform_community.relate(new ExperimentalProteoform[] { e }, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(1, relations.Count);
            Sweet.lollipop.min_bu_peptides = 1;
            relations = Sweet.lollipop.target_proteoform_community.relate(new ExperimentalProteoform[] { e }, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(0, relations.Count);
            e.modified_mass = 29260.9826; //P0CX35
            Sweet.lollipop.min_bu_peptides = 1;
            relations = Sweet.lollipop.target_proteoform_community.relate(new ExperimentalProteoform[] { e }, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(0, relations.Count); //modified peptide, no mod theoretical with this accession so returns 0
            e.modified_mass = 59841.6191; //P32329
            relations = Sweet.lollipop.target_proteoform_community.relate(new ExperimentalProteoform[] { e }, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(1, relations.Count);

            Assert.AreEqual(0, Proteoform.get_possible_PSMs("Q3E770", new PtmSet(new List<Ptm>()), 3, 20, false).Count());
            Assert.AreEqual(0, Proteoform.get_possible_PSMs("P0CX35", new PtmSet(new List<Ptm>()), 3, 20, false).Count());
            Assert.AreEqual(1, Proteoform.get_possible_PSMs("P0CX35", Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[80].Where(p => p.ptm_description.Contains("Phospho")).First(), 2, 500, false).Count());
            Assert.AreEqual(0, Proteoform.get_possible_PSMs("P0CX35", Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[80].Where(p => p.ptm_description.Contains("Phospho")).First(), 1, 5, false).Count());
        }

        [Test]
        public void test_protein_grouping_by_sequence()
        {
            DatabaseReference d1 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d2 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d3 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            goTerm g1 = new goTerm(d1);
            goTerm g2 = new goTerm(d1);
            goTerm g3 = new goTerm(d1);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d1 }, new List<goTerm> { g1 });
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d2 }, new List<goTerm> { g2 });
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d3 }, new List<goTerm> { g3 });
            ProteinSequenceGroup psg = new ProteinSequenceGroup(new List<ProteinWithGoTerms> { p1, p2, p3 }.OrderByDescending(p => p.IsContaminant ? 1 : 0));
            Assert.AreEqual(3, psg.GoTerms.Count());
            Assert.AreEqual(3, psg.GeneNames.Count());
            Assert.AreEqual("T1_3G", psg.Accession);
            Assert.False(psg.IsContaminant);

            p3 = new ProteinWithGoTerms("MCSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d3 }, new List<goTerm> { g3 });
            ProteinSequenceGroup[] psgs = Sweet.lollipop.theoretical_database.group_proteins_by_sequence(new List<ProteinWithGoTerms> { p1, p2, p3 });
            Assert.AreEqual(2, psgs.Length);
        }

        [Test]
        public void test_protein_grouping_by_sequence_contaminant()
        {
            DatabaseReference d1 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d2 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d3 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            goTerm g1 = new goTerm(d1);
            goTerm g2 = new goTerm(d1);
            goTerm g3 = new goTerm(d1);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d1 }, new List<goTerm> { g1 });
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { d2 }, new List<goTerm> { g2 });
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, true, new List<DatabaseReference> { d3 }, new List<goTerm> { g3 });
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
            Loaders.LoadElements();
            TheoreticalProteoformDatabase tpd = new TheoreticalProteoformDatabase();
            List<Modification> mods = PtmListLoader.ReadModsFromFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "ptmlist.txt"), out List<(Modification, string)> filteredModificationsWithWarnings).ToList();
            foreach (Modification m in mods)
            {
                if (!Sweet.lollipop.modification_ranks.TryGetValue(Math.Round((double)m.MonoisotopicMass, 5), out int x))
                {
                    Sweet.lollipop.modification_ranks.Add(Math.Round((double)m.MonoisotopicMass, 5), -1);
                }
            }
            tpd.unlocalized_lookup = tpd.make_unlocalized_lookup(mods);
            tpd.load_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "stored_mods.modnames"));
            tpd.save_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "fake_stored_mods.modnames"));
            Modification firstAcetyl = mods.FirstOrDefault(x => x.OriginalId.StartsWith("N-acetyl"));
            Assert.AreNotEqual(firstAcetyl.OriginalId, tpd.unlocalized_lookup[firstAcetyl].id);

            //Test amending
            mods.AddRange(PtmListLoader.ReadModsFromFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "intact_mods.txt"), out filteredModificationsWithWarnings).OfType<Modification>());
            Sweet.lollipop.modification_ranks = mods.DistinctBy(m => m.MonoisotopicMass).ToDictionary(m => Math.Round((double)m.MonoisotopicMass, 5), m => -1);
            tpd.unlocalized_lookup = tpd.make_unlocalized_lookup(mods.OfType<Modification>());
            tpd.amend_unlocalized_names(Path.Combine(TestContext.CurrentContext.TestDirectory, "Mods", "fake_stored_mods.modnames"));
        }

        [Test]
        public void test_enter_theoretical_proteoform_family()
        {
            Sweet.lollipop = new Lollipop();
            List<TheoreticalProteoform> theoreticals = new List<TheoreticalProteoform>();
            Sweet.lollipop.theoretical_database.populate_aa_mass_dictionary();

            //bad sequence
            Sweet.lollipop.theoretical_database.EnterTheoreticalProteformFamily("BADSEQ", null, null, "asdf",
                theoreticals, -1, null);
            Assert.AreEqual(0, theoreticals.Count);
        }

        [Test]
        [TestCase("uniprot_yeast_test_12entries.xml")]
        public void average_mass(string database)
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.methionine_oxidation = false;
            Sweet.lollipop.carbamidomethylation = true;
            Sweet.lollipop.methionine_cleavage = true;
            Sweet.lollipop.max_ptms = 3;
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.min_peptide_length = 7;
            Sweet.lollipop.ptmset_mass_tolerance = 0.00001;
            Sweet.lollipop.combine_identical_sequences = true;
            Sweet.lollipop.limit_triples_and_greater = false;
            Sweet.lollipop.use_average_mass = true;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, database) }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
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

            var unmodified = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms
                .Where(t => t.ptm_set.ptm_combination.Count == 0).OrderBy(t => t.modified_mass).First();
            Assert.AreEqual(1683.97, Math.Round(unmodified.modified_mass, 2));
            Assert.AreEqual(1683.97, Math.Round(unmodified.unmodified_mass, 2));

            var modified = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms
                .Where(t => t.ptm_set.ptm_combination.Count > 0).OrderBy(t => t.modified_mass).First();
            Assert.AreEqual(29358.61, Math.Round(modified.modified_mass, 2));
            Assert.AreEqual(29278.63, Math.Round(modified.unmodified_mass, 2));
        }

        [Test]
        public void parallel_enter_theoreticals_doesnt_crash()
        {
            TheoreticalProteoformDatabase db = new TheoreticalProteoformDatabase();
            db.populate_aa_mass_dictionary();
            List<Modification> var = new List<Modification>();
            List<TheoreticalProteoform> ts = new List<TheoreticalProteoform>();
            ProteinWithGoTerms p = ConstructorsForTesting.make_a_theoretical().ExpandedProteinList.First();
            Parallel.Invoke(
                () => db.EnterTheoreticalProteformFamily("SEQ", p, p.OneBasedPossibleLocalizedModifications, p.Accession, ts, 1, var),
                () => db.EnterTheoreticalProteformFamily("SEQ", p, p.OneBasedPossibleLocalizedModifications, p.Accession, ts, 1, var)
            );
        }
    }
}