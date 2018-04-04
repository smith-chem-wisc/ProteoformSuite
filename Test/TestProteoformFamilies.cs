using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    class TestProteoformFamilies
    {
        [Test]
        public void test_construct_one_proteform_family_from_ET()
        {
            Sweet.lollipop = new Lollipop();
            ProteoformCommunity test_community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community = test_community;
            Sweet.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified", "unknown") } } };

            //One accepted ET relation; should give one ProteoformFamily
            Sweet.lollipop.min_peak_count_et = 1;
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1");
            pf1.accepted = true;
            TheoreticalProteoform pf2 = ConstructorsForTesting.make_a_theoretical();
            pf2.name = "T1";
            ProteoformComparison comparison = ProteoformComparison.ExperimentalTheoretical;
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf2, comparison, 0, TestContext.CurrentContext.TestDirectory);
            pr1.Accepted = true;
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.InstanceId).ToList());
            DeltaMassPeak peak = new DeltaMassPeak(prs[0],new HashSet<ProteoformRelation>(prs));
            Sweet.lollipop.et_peaks = new List<DeltaMassPeak> { peak };
            test_community.experimental_proteoforms = new ExperimentalProteoform[] { pf1 };
            test_community.theoretical_proteoforms = new TheoreticalProteoform[] { pf2 };
            test_community.construct_families();
            Assert.AreEqual("T1", test_community.families.First().name_list);
            Assert.AreEqual("T1", test_community.families.First().accession_list);
            Assert.AreEqual("E1", test_community.families.First().experimentals_list);
            Assert.AreEqual(1, test_community.families.Count);
            Assert.AreEqual(2, test_community.families[0].proteoforms.Count);
            Assert.AreEqual(1, test_community.families.First().experimental_proteoforms.Count);
            Assert.AreEqual(1, test_community.families.First().theoretical_proteoforms.Count);
        }

        [Test]
        public void test_construct_multi_member_family()
        {
            //Four experimental proteoforms, three relations (linear), all accepted; should give 1 bundled family
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.et_accept_peaks_based_on_rank = false;
            Sweet.lollipop.ee_accept_peaks_based_on_rank = false;
            ProteoformCommunity test_community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community = test_community;

            Sweet.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified", "unknown") } } };

            Sweet.lollipop.min_peak_count_ee = 2;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2");
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("E3");
            ExperimentalProteoform pf6 = ConstructorsForTesting.ExperimentalProteoform("E4");
            test_community.experimental_proteoforms = new ExperimentalProteoform[] { pf3, pf4, pf5, pf6 };

            ProteoformComparison comparison34 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison45 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison56 = ProteoformComparison.ExperimentalExperimental;
            ConstructorsForTesting.make_relation(pf3, pf4, comparison34, 0);
            ConstructorsForTesting.make_relation(pf4, pf5, comparison45, 0);
            ConstructorsForTesting.make_relation(pf5, pf6, comparison56, 0);

            List<ProteoformRelation> prs2 = new HashSet<ProteoformRelation>(test_community.experimental_proteoforms.SelectMany(p => p.relationships).Concat(test_community.theoretical_proteoforms.SelectMany(p => p.relationships))).OrderBy(r => r.DeltaMass).ToList();
            foreach (ProteoformRelation pr in prs2) pr.set_nearby_group(prs2, prs2.Select(r => r.InstanceId).ToList());
            Assert.AreEqual(3, pf3.relationships.First().nearby_relations_count);
            Assert.AreEqual(3, pf5.relationships.First().nearby_relations_count);
            Assert.AreEqual(3, pf6.relationships.First().nearby_relations_count);

            test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            Assert.AreEqual(1, Sweet.lollipop.ee_peaks.Count);
            Assert.AreEqual(3, Sweet.lollipop.ee_peaks[0].grouped_relations.Count);

            test_community.experimental_proteoforms = new ExperimentalProteoform[] { pf3, pf4, pf5, pf6 };
            test_community.construct_families();
            Assert.AreEqual(1, test_community.families.Count);
            Assert.AreEqual("", test_community.families.First().accession_list);
            Assert.AreEqual(4, test_community.families.First().proteoforms.Count);
            Assert.AreEqual(4, test_community.families.First().experimental_proteoforms.Count);
            Assert.AreEqual(0, test_community.families.First().theoretical_proteoforms.Count);
        }    
       
        [Test]
        public void test_construct_two_families()
        {
            //Five experimental proteoforms, four relations (linear), second on not accepted into a peak, one peak; should give 2 families
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.ee_accept_peaks_based_on_rank = false;
            Sweet.lollipop.et_accept_peaks_based_on_rank = false;
            ProteoformCommunity test_community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community = test_community;

            Sweet.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified", "unknown") } } };

            Sweet.lollipop.ee_max_mass_difference = 20;
            Sweet.lollipop.peak_width_base_ee = 0.015;
            Sweet.lollipop.min_peak_count_ee = 3; //needs to be high so that 0 peak accepted, other peak isn't.... 

            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2");
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("E3");
            ExperimentalProteoform pf6 = ConstructorsForTesting.ExperimentalProteoform("E4");
            ExperimentalProteoform pf7 = ConstructorsForTesting.ExperimentalProteoform("E5");

            ProteoformComparison comparison34 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison45 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison56 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison67 = ProteoformComparison.ExperimentalExperimental;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, comparison34, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr3 = new ProteoformRelation(pf4, pf5, comparison45, 19, TestContext.CurrentContext.TestDirectory); //not accepted
            ProteoformRelation pr4 = new ProteoformRelation(pf5, pf6, comparison56, 0, TestContext.CurrentContext.TestDirectory);
            ProteoformRelation pr5 = new ProteoformRelation(pf6, pf7, comparison67, 0, TestContext.CurrentContext.TestDirectory);

            List<ProteoformRelation> prs2 = new List<ProteoformRelation> { pr2, pr3, pr4, pr5 }.OrderBy(r => r.DeltaMass).ToList();
            foreach (ProteoformRelation pr in prs2) pr.set_nearby_group(prs2, prs2.Select(r => r.InstanceId).ToList());
            Assert.AreEqual(3, pr2.nearby_relations_count);
            Assert.AreEqual(1, pr3.nearby_relations_count);
            Assert.AreEqual(3, pr4.nearby_relations_count);
            Assert.AreEqual(3, pr5.nearby_relations_count);

            test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            Assert.AreEqual(2, Sweet.lollipop.ee_peaks.Count);
            Assert.AreEqual(1, Sweet.lollipop.ee_peaks.Where(peak => peak.Accepted).Count());
            Assert.AreEqual(3, Sweet.lollipop.ee_peaks.Where(peak => peak.Accepted).First().grouped_relations.Count());

            test_community.experimental_proteoforms = new ExperimentalProteoform[] { pf3, pf4, pf5, pf6, pf7 };
            test_community.construct_families();
            Assert.AreEqual(2, test_community.families.Count);
            Assert.AreEqual("", test_community.families.FirstOrDefault(f => f.proteoforms.Contains(pf3)).accession_list);
            Assert.AreEqual(2, test_community.families.FirstOrDefault(f => f.proteoforms.Contains(pf3)).proteoforms.Count);
            Assert.AreEqual(2, test_community.families.FirstOrDefault(f => f.proteoforms.Contains(pf3)).experimental_proteoforms.Count);
            Assert.AreEqual(0, test_community.families.FirstOrDefault(f => f.proteoforms.Contains(pf3)).theoretical_proteoforms.Count);
            Assert.AreEqual("", test_community.families.FirstOrDefault(f => f.proteoforms.Contains(pf5)).accession_list);
            Assert.AreEqual(3, test_community.families.FirstOrDefault(f => f.proteoforms.Contains(pf5)).proteoforms.Count);
            Assert.AreEqual(3, test_community.families.FirstOrDefault(f => f.proteoforms.Contains(pf5)).experimental_proteoforms.Count);
            Assert.AreEqual(0, test_community.families.FirstOrDefault(f => f.proteoforms.Contains(pf5)).theoretical_proteoforms.Count);
        }

        [Test]
        public void test_construct_one_proteform_family_from_ET_with_theoretical_pf_group()
        {
            ProteoformCommunity test_community = new ProteoformCommunity();
            Sweet.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified", "unknown") } } };

            InputFile f = new InputFile("fake.txt", Purpose.ProteinDatabase);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("ASDF", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "name", "full_name", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            Dictionary<InputFile, Protein[]> dict = new Dictionary<InputFile, Protein[]> {
                { f, new Protein[] { p1 } }
            };
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("T1_T1_asdf", p1, dict);
            

            //One accepted ET relation; should give one ProteoformFamily
            Sweet.lollipop.min_peak_count_et = 1;
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1");
            TheoreticalProteoformGroup pf2 = new TheoreticalProteoformGroup(new List<TheoreticalProteoform> { t });
            pf2.begin = 1;
            pf2.end = 4;
            ProteoformComparison comparison = ProteoformComparison.ExperimentalTheoretical;
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf2, comparison, 0, TestContext.CurrentContext.TestDirectory);
            pr1.Accepted = true;
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.InstanceId).ToList());
            DeltaMassPeak peak = new DeltaMassPeak(prs[0], new HashSet<ProteoformRelation>(prs));
            Sweet.lollipop.et_peaks = new List<DeltaMassPeak> { peak };
            test_community.experimental_proteoforms = new ExperimentalProteoform[] { pf1 };
            test_community.theoretical_proteoforms = new TheoreticalProteoform[] { pf2 };
            test_community.construct_families();
            Assert.AreEqual(1, test_community.families.Count);
            Assert.AreEqual(2, test_community.families[0].proteoforms.Count);
            Assert.AreEqual(1, test_community.families.First().experimental_proteoforms.Count);
            Assert.AreEqual(1, test_community.families.First().theoretical_proteoforms.Count);
            Assert.AreEqual("E1", test_community.families.First().experimentals_list);
            Assert.AreEqual(p1.Name, test_community.families.First().name_list);
            Assert.AreEqual(pf2.accession, test_community.families.First().accession_list);
        }
        

        public static string p1_accession = "T1";
        public static string p1_name = "name";
        public static string p1_fullName = "full_name_p1";
        public static DatabaseReference p1_dbRef = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
        public static GoTerm p1_goterm = new GoTerm(p1_dbRef);
        public static string pf1_accession = "T1_asdf";
        public static ProteoformCommunity construct_two_families_with_potentially_colliding_theoreticals()
        {
            //Five experimental proteoforms, four relations (linear), second on not accepted into a peak, one peak; should give 2 families
            Sweet.lollipop = new Lollipop();
            ProteoformCommunity community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community = community;
            Sweet.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>>
            {
                { "unmodified", new List<Modification> { ConstructorsForTesting.get_modWithMass("unmodified", 0) } },
                { "fake", new List<Modification> { ConstructorsForTesting.get_modWithMass("fake", 19) } },
            };
            Sweet.lollipop.ee_accept_peaks_based_on_rank = false;
            Sweet.lollipop.et_accept_peaks_based_on_rank = false;
            Sweet.lollipop.modification_ranks = new Dictionary<double, int> { { 0, 1 }, { 19, 2 } };
            Sweet.lollipop.mod_rank_sum_threshold = 2;
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = PtmCombos.generate_all_ptmsets(1, Sweet.lollipop.theoretical_database.uniprotModifications.SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList(), Sweet.lollipop.modification_ranks, 1);
            Sweet.lollipop.theoretical_database.all_mods_with_mass = Sweet.lollipop.theoretical_database.uniprotModifications.SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList();
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = Sweet.lollipop.theoretical_database.make_ptmset_dictionary();

            Sweet.lollipop.ee_max_mass_difference = 20;
            Sweet.lollipop.peak_width_base_ee = 0.015;
            Sweet.lollipop.min_peak_count_ee = 3; //needs to be high so that 0 peak accepted, other peak isn't.... 
            Sweet.lollipop.min_peak_count_et = 2; //needs to be lower so the 2 ET relations are accepted 

            //TheoreticalProteoformGroup
            InputFile f = new InputFile("fake.txt", Purpose.ProteinDatabase);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", p1_accession, new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, p1_name, p1_fullName, true, false, new List<DatabaseReference> { p1_dbRef }, new List<GoTerm> { p1_goterm });
            Dictionary<InputFile, Protein[]> dict = new Dictionary<InputFile, Protein[]> { { f, new Protein[] { p1 } } };
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("T1_asdf", "T1_asdf", 1234.56, p1, dict);
            TheoreticalProteoformGroup pf1 = new TheoreticalProteoformGroup(new List<TheoreticalProteoform> { t });
            TheoreticalProteoform pf2 = ConstructorsForTesting.make_a_theoretical("T1_asdf_pf2", "T1_asdf_1", 1234.56, dict);


            //ExperimentalProteoforms
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1", 0, 0, true);
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2", 0, 0, true);
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("E3", 0, 0, true);
            ExperimentalProteoform pf6 = ConstructorsForTesting.ExperimentalProteoform("E4", 0, 0, true);
            ExperimentalProteoform pf7 = ConstructorsForTesting.ExperimentalProteoform("E5", 0, 0, true);
            ExperimentalProteoform pf8 = ConstructorsForTesting.ExperimentalProteoform("E6", 0, 0, true);
            TheoreticalProteoform pf9 = ConstructorsForTesting.make_a_theoretical("T1_asdf_pf9", "T1_asdf_1", 1253.56, dict);
            community.theoretical_proteoforms = new TheoreticalProteoform[] { pf1, pf2, pf9 };
            community.experimental_proteoforms = new ExperimentalProteoform[] { pf3, pf4, pf5, pf6, pf7, pf8 };
            pf3.agg_mass = 1234.56;
            pf4.agg_mass = 1234.56;
            pf5.agg_mass = 1234.56;
            pf6.agg_mass = 1253.56;
            pf7.agg_mass = 1253.56;
            pf8.agg_mass = 1253.56;

            ProteoformComparison comparison13 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison23 = ProteoformComparison.ExperimentalTheoretical;
            ProteoformComparison comparison34 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison45 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison56 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison67 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison78 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison89 = ProteoformComparison.ExperimentalTheoretical;
            ConstructorsForTesting.make_relation(pf3, pf1, comparison13, 0);
            ConstructorsForTesting.make_relation(pf3, pf2, comparison23, 0);
            ConstructorsForTesting.make_relation(pf3, pf4, comparison34, 0);
            ConstructorsForTesting.make_relation(pf4, pf5, comparison45, 0);
            ConstructorsForTesting.make_relation(pf5, pf6, comparison56, 19); //not accepted
            ConstructorsForTesting.make_relation(pf6, pf7, comparison67, 0);
            ConstructorsForTesting.make_relation(pf7, pf8, comparison78, 0);
            ConstructorsForTesting.make_relation(pf8, pf9, comparison89, 0);

            List<ProteoformRelation> prs = new HashSet<ProteoformRelation>(community.experimental_proteoforms.SelectMany(p => p.relationships).Concat(community.theoretical_proteoforms.SelectMany(p => p.relationships))).ToList();
            foreach (Proteoform p in prs.SelectMany(r => r.connected_proteoforms)) Assert.IsNotNull(p);
            List<ProteoformRelation> prs_et = prs.Where(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical).OrderBy(r => r.DeltaMass).ToList();
            Sweet.lollipop.et_relations = prs_et;
            List<ProteoformRelation> prs_ee = prs.Where(r => r.RelationType == ProteoformComparison.ExperimentalExperimental).OrderBy(r => r.DeltaMass).ToList();
            Sweet.lollipop.ee_relations = prs_ee;
            foreach (ProteoformRelation pr in prs_et) pr.set_nearby_group(prs_et, prs_et.Select(r => r.InstanceId).ToList());
            foreach (ProteoformRelation pr in prs_ee) pr.set_nearby_group(prs_ee, prs_ee.Select(r => r.InstanceId).ToList());
            Assert.AreEqual(3, pf1.relationships.First().nearby_relations_count); // 2 ET relations at 0 delta mass
            Assert.AreEqual(3, pf2.relationships.First().nearby_relations_count);
            Assert.AreEqual(4, pf4.relationships.First().nearby_relations_count); // 4 EE relations at 0 delta mass
            Assert.AreEqual(4, pf5.relationships.First().nearby_relations_count);
            Assert.AreEqual(1, pf6.relationships.First().nearby_relations_count); // 1 EE relation at 19 delta mass
            Assert.AreEqual(4, pf7.relationships.First().nearby_relations_count);
            Assert.AreEqual(4, pf8.relationships.First().nearby_relations_count);

            community.accept_deltaMass_peaks(prs_et, new List<ProteoformRelation>());
            community.accept_deltaMass_peaks(prs_ee, new List<ProteoformRelation>());
            Assert.AreEqual(3, Sweet.lollipop.et_peaks.Count + Sweet.lollipop.ee_peaks.Count);
            Assert.AreEqual(1, Sweet.lollipop.et_peaks.Where(peak => peak.Accepted).Count()); // 1 ET peak
            Assert.AreEqual(1, Sweet.lollipop.ee_peaks.Where(peak => peak.Accepted).Count()); // 1 EE peak accepted
            Assert.AreEqual(4, Sweet.lollipop.ee_peaks.Where(peak => peak.Accepted && peak.RelationType == ProteoformComparison.ExperimentalExperimental).First().grouped_relations.Count());
            Assert.AreEqual(3, Sweet.lollipop.et_peaks.Where(peak => peak.Accepted && peak.RelationType == ProteoformComparison.ExperimentalTheoretical).First().grouped_relations.Count());

            community.construct_families();

            //Testing the identification of experimentals   
            //test with a modificationwithmass that's 0 mass, and then see that it crawls around and labels them each with growing ptm sets with that modification
            //test that the relation.represented_modification gets set
            Assert.True(Sweet.lollipop.et_relations.All(r => r.peak.DeltaMass != 19 || r.represented_ptmset == null));
            Assert.True(Sweet.lollipop.et_relations.All(r => r.peak.DeltaMass != 0 || r.represented_ptmset.ptm_combination.First().modification.id == "unmodified"));
            Assert.True(pf1 == pf3.linked_proteoform_references.First() || pf2 == pf3.linked_proteoform_references.First());

            //test I don't get re-reassignments
            Assert.AreEqual(pf3, pf4.linked_proteoform_references.Last()); //test that the proteoform.theoretical_reference gets set to each successive PF base
            Assert.AreEqual((pf3.linked_proteoform_references.First() as TheoreticalProteoform).accession, (pf4.linked_proteoform_references.First() as TheoreticalProteoform).accession);
            Assert.AreEqual((pf3.linked_proteoform_references.First() as TheoreticalProteoform).fragment, (pf4.linked_proteoform_references.First() as TheoreticalProteoform).fragment);
            Assert.AreEqual(pf4, pf5.linked_proteoform_references.Last());
            Assert.AreEqual((pf3.linked_proteoform_references.First() as TheoreticalProteoform).accession, (pf5.linked_proteoform_references.First() as TheoreticalProteoform).accession); //test that the accession gets carried all the way through the depth of connections
            Assert.AreEqual((pf3.linked_proteoform_references.First() as TheoreticalProteoform).fragment, (pf5.linked_proteoform_references.First() as TheoreticalProteoform).fragment);
            Assert.AreEqual(pf9, pf8.linked_proteoform_references.Last());

            return community;
        }

        [Test]
        public void test_construct_one_proteform_family_from_ET_with_two_theoretical_pf_groups_with_same_accession()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.gene_centric_families = false;
            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            Sweet.lollipop.target_proteoform_community = community;

            Assert.AreEqual(2, community.families.Count);
            Assert.AreEqual(9, community.families.SelectMany(f => f.proteoforms).Count());
            Assert.AreEqual(5, community.families.FirstOrDefault(f => f.proteoforms.Select(p => p.accession).Contains("E1")).proteoforms.Count);
            Assert.AreEqual(4, community.families.FirstOrDefault(f => f.proteoforms.Select(p => p.accession).Contains("E6")).proteoforms.Count);
            Assert.AreEqual(3, community.families.FirstOrDefault(f => f.proteoforms.Select(p => p.accession).Contains("E1")).experimental_proteoforms.Count);
            Assert.AreEqual(2, community.families.FirstOrDefault(f => f.proteoforms.Select(p => p.accession).Contains("E1")).theoretical_proteoforms.Count);
            Assert.AreEqual("" + "; " + "GENE", community.families.FirstOrDefault(f => f.proteoforms.Select(p => p.accession).Contains("E1")).gene_list); //both would give null preferred gene names, since that field isn't set up
            Assert.True(String.Join("", community.families.Select(f => f.experimentals_list)).Contains("E1"));
            Assert.True(String.Join("", community.families.Select(f => f.name_list)).Contains(p1_name));
            Assert.True(String.Join("", community.families.Select(f => f.accession_list)).Contains(pf1_accession));
            Assert.True(String.Join("", community.families.Select(f => f.agg_mass_list)).Contains(1234.56.ToString()));

            //Check that the list of proteoforms is the same in the relations as in the proteoform lists
            HashSet<Proteoform> relation_proteoforms = new HashSet<Proteoform>(community.families.SelectMany(f => f.relations).SelectMany(r => r.connected_proteoforms));
            Assert.True(community.experimental_proteoforms.OfType<Proteoform>().Concat(community.theoretical_proteoforms).All(p => relation_proteoforms.Contains(p)));
            Assert.True(relation_proteoforms.All(p => community.experimental_proteoforms.Contains(p) || community.theoretical_proteoforms.Contains(p)));
        }

        [Test]
        public void test_construct_target_and_decoy_families()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.ee_accept_peaks_based_on_rank = false;
            Sweet.lollipop.et_accept_peaks_based_on_rank = false;
            Sweet.lollipop.neucode_labeled = true;
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1", 1000, 5, true);
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("E2", 1020, 5, true);
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E3", 1040, 8, true);
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E4", 1050, 5, true);
            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("t1", 1000, 5);
            t1.sequence = "ASDF";
            t1.begin = 1;
            t1.end = 4;
            TheoreticalProteoform decoy1 = ConstructorsForTesting.make_a_theoretical("Decoy1", 1020, 5);

            TestProteoformCommunityRelate.prepare_for_et(new List<double>() { 0 });

            Sweet.lollipop.target_proteoform_community = new ProteoformCommunity();
            Sweet.lollipop.target_proteoform_community.experimental_proteoforms = new ExperimentalProteoform[4] { pf1, pf2, pf3, pf4 };
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[1] { t1 };
            Sweet.lollipop.decoy_proteoform_communities.Add(Sweet.lollipop.decoy_community_name_prefix + "0", new ProteoformCommunity());
            Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].experimental_proteoforms = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Select(e => new ExperimentalProteoform(e)).ToArray();
            Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].theoretical_proteoforms = new TheoreticalProteoform[1] { decoy1 };

            Sweet.lollipop.et_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            Sweet.lollipop.ee_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, true);
            Sweet.lollipop.relate_ed();
            Sweet.lollipop.relate_ef();
            foreach (ProteoformRelation pr in Sweet.lollipop.et_relations) pr.set_nearby_group(Sweet.lollipop.et_relations, Sweet.lollipop.et_relations.Select(r => r.InstanceId).ToList());
            foreach (ProteoformRelation pr in Sweet.lollipop.ee_relations) pr.set_nearby_group(Sweet.lollipop.ee_relations, Sweet.lollipop.ee_relations.Select(r => r.InstanceId).ToList());

            Sweet.lollipop.et_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.et_relations, Sweet.lollipop.ed_relations);
            Sweet.lollipop.ee_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.ee_relations, Sweet.lollipop.ef_relations);

            //one ED relation
            Assert.AreEqual(1, Sweet.lollipop.ed_relations.Values.SelectMany(v => v).Count());
            Assert.AreEqual(1, Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].theoretical_proteoforms.SelectMany(t => t.relationships).Count());

            //peak is unaccepted --> relation should be unaccepted, peak added to relation
            Assert.IsFalse(Sweet.lollipop.ed_relations.First().Value.FirstOrDefault().Accepted); //should be false if peak unaccepted
            Assert.AreEqual(1, Sweet.lollipop.et_peaks.Count);
            Assert.IsNotNull(Sweet.lollipop.ed_relations.Values.SelectMany(v => v).First().peak);


            //peak is accepted --> relation should be accepted, peak added to relation
            Sweet.lollipop.clear_et();
            Sweet.lollipop.min_peak_count_et = 1;
            Sweet.lollipop.et_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            Sweet.lollipop.relate_ed();
            foreach (ProteoformRelation pr in Sweet.lollipop.et_relations) pr.set_nearby_group(Sweet.lollipop.et_relations, Sweet.lollipop.et_relations.Select(r => r.InstanceId).ToList());
            foreach (ProteoformRelation pr in Sweet.lollipop.ee_relations) pr.set_nearby_group(Sweet.lollipop.ee_relations, Sweet.lollipop.ee_relations.Select(r => r.InstanceId).ToList());
            Sweet.lollipop.et_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.et_relations, Sweet.lollipop.ed_relations);

            Sweet.lollipop.et_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.et_relations, Sweet.lollipop.ed_relations);
            Assert.IsTrue(Sweet.lollipop.ed_relations.First().Value.FirstOrDefault().Accepted); //should be true if peak accepted
            Assert.IsNotNull(Sweet.lollipop.ed_relations.Values.SelectMany(v => v).First().peak);
            Assert.AreEqual(1, Sweet.lollipop.ed_relations.Values.SelectMany(v => v).Count(r => r.Accepted)); //only 1 relation is in delta mass range of accepted peak - only 1 accepted

            //2 ef relations, unaccepted because peak unaccepted
            Assert.AreEqual(3, Sweet.lollipop.ef_relations.Values.SelectMany(v => v).Count()); //2 ef relations
            Assert.AreEqual(0, Sweet.lollipop.ef_relations.Values.SelectMany(v => v).Count(r => r.Accepted)); //all peaks accepted are false, so relation accepted should be false

            //one peak accepted --> one of the EF relations falls into range of peak and should be accepted
            Sweet.lollipop.clear_ee();
            Sweet.lollipop.min_peak_count_ee = 1;
            Sweet.lollipop.ee_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, true);
            Sweet.lollipop.relate_ef();
            Sweet.lollipop.ee_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.ee_relations, Sweet.lollipop.ef_relations);
            Assert.AreEqual(3, Sweet.lollipop.ee_peaks.Count(p => p.Accepted));
            Assert.AreEqual(3, Sweet.lollipop.ee_relations.Count);
            Assert.AreEqual(1, Sweet.lollipop.ef_relations.Values.SelectMany(v => v).Count(r => r.Accepted)); //only 1 relation is in delta mass range of accepted peak - only 1 accepted
            Sweet.lollipop.construct_target_and_decoy_families();

            //should be 2 target families and 3 decoy families
            //only make decoy relations out of accepted relations --> 1 family with accepted relations
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.families.Count);
            Assert.AreEqual(3, Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].families.Count);
            Assert.AreEqual(1, Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].families.Count(f => f.relations.Count > 0));
            Assert.AreEqual(2, Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].families.SelectMany(f => f.relations).Count());
        }

        [Test]
        public void construct_families_with_or_without_td_nodes()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.et_accept_peaks_based_on_rank = false;
            Sweet.lollipop.ee_accept_peaks_based_on_rank = false;
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.target_proteoform_community = construct_community_with_td_proteoforms(-100);
            Sweet.lollipop.et_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            foreach (ProteoformRelation pr in Sweet.lollipop.et_relations) pr.set_nearby_group(Sweet.lollipop.et_relations, Sweet.lollipop.et_relations.Select(r => r.InstanceId).ToList());
            Sweet.lollipop.min_peak_count_et = 1;
            Sweet.lollipop.et_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.et_relations, Sweet.lollipop.ed_relations);

            //include td nodes
            Sweet.lollipop.clear_all_families();
            Sweet.lollipop.construct_target_and_decoy_families();
            Assert.AreEqual(3, Sweet.lollipop.target_proteoform_community.families.Count);
            Sweet.lollipop.target_proteoform_community.families = Sweet.lollipop.target_proteoform_community.families.OrderBy(f => f.proteoforms.Count).ToList();
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.families[0].proteoforms.Count);
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.families[1].proteoforms.Count);
            Assert.AreEqual(3, Sweet.lollipop.target_proteoform_community.families[2].proteoforms.Count);
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null));
        }

        public static ProteoformCommunity construct_community_with_td_proteoforms(int community_number)
        {
            Sweet.lollipop.maximum_missed_monos = 1;
            ProteoformCommunity community = new ProteoformCommunity();
            // Two proteoforms; lysine count equal; mass difference < 250 -- return 1
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true);
            pf1.agg_rt = 45;
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("A2", 1001.0, 1, true);
            pf2.agg_rt = 85;
            TopDownProteoform td1 = ConstructorsForTesting.TopDownProteoform("DIFFACCESSION_2", 1000.0, 45);
            TopDownProteoform td2 = ConstructorsForTesting.TopDownProteoform("ACCESSION_2", 1001.0, 85);

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("ACCESSION", 1000.0, 1);
            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("ACCESSION2", 1000.0, 1);
            t2.topdown_theoretical = true;

            TestProteoformCommunityRelate.prepare_for_et(new List<double>() { 0 });
            //need to make theoretical accession database
            community.community_number = community_number;
            Sweet.lollipop.theoretical_database.theoreticals_by_accession.Add(community_number, new Dictionary<string, List<TheoreticalProteoform>>());
            Sweet.lollipop.theoretical_database.theoreticals_by_accession[community_number].Add(t1.accession, new List<TheoreticalProteoform>() { t1 });
            Sweet.lollipop.theoretical_database.theoreticals_by_accession[community_number].Add(t2.accession, new List<TheoreticalProteoform>() { t2 });

            community.theoretical_proteoforms = new List<TheoreticalProteoform>() { t1, t2 }.ToArray();
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform> { td1, td2 };
            community.experimental_proteoforms = new List<ExperimentalProteoform>(Sweet.lollipop.topdown_proteoforms) { pf1, pf2 }.ToArray();
            return community;
        }

        [Test]
        public void test_results_summary_with_peaks()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.theoretical_database.theoretical_proteins = new Dictionary<InputFile, Protein[]>();
            Sweet.lollipop.theoretical_database.expanded_proteins = new ProteinWithGoTerms[0];
            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            Assert.True(ResultsSummaryGenerator.generate_full_report().Length > 0);
        }

        [Test]
        public void gene_centric_family()
        {
            Lollipop.gene_centric_families = true;
            Lollipop.preferred_gene_label = Lollipop.gene_name_labels[1];

            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            Assert.AreEqual(1, community.families.Count);

            //Only the distinct gene names are kept in list
            Assert.AreEqual(String.Join("; ", new string[] { "", "GENE", "GENE" }.Distinct()), community.families.FirstOrDefault(f => f.proteoforms.Select(p => p.accession).Contains("E1")).gene_list); //both would give null preferred gene names, since that field isn't set up
        }

        [Test]
        public void community_clear_et()
        {
            Sweet.lollipop = new Lollipop();
            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            Assert.IsNotEmpty(Sweet.lollipop.et_relations);
            Assert.IsNotEmpty(Sweet.lollipop.ee_relations);
            Assert.IsNotEmpty(community.families);
            Assert.True(Sweet.lollipop.et_relations.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.True(Sweet.lollipop.et_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.IsNotNull(community.experimental_proteoforms.First().family);
            Assert.IsNotNull(community.experimental_proteoforms.First().gene_name);
            Assert.IsNotNull(community.experimental_proteoforms.First().linked_proteoform_references);
            Assert.IsNotEmpty(community.experimental_proteoforms.First().ptm_set.ptm_combination);
            Assert.True(community.experimental_proteoforms.First().relationships.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Sweet.lollipop.clear_et();
            Assert.IsEmpty(Sweet.lollipop.et_peaks);
            Assert.IsEmpty(Sweet.lollipop.et_relations);
            Assert.IsNotEmpty(Sweet.lollipop.ee_relations);
            Assert.IsNotEmpty(Sweet.lollipop.ee_peaks);
            Assert.IsEmpty(community.families);
            Assert.False(Sweet.lollipop.et_relations.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.False(Sweet.lollipop.et_relations.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.Null(community.experimental_proteoforms.First().family);
            Assert.Null(community.experimental_proteoforms.First().gene_name);
            Assert.Null(community.experimental_proteoforms.First().linked_proteoform_references);
            Assert.IsEmpty(community.experimental_proteoforms.First().ptm_set.ptm_combination);
            Assert.False(community.experimental_proteoforms.First().relationships.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
        }

        [Test]
        public void community_clear_ee()
        {
            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            Assert.IsNotEmpty(Sweet.lollipop.et_relations);
            Assert.IsNotEmpty(Sweet.lollipop.ee_relations);
            Assert.IsNotEmpty(community.families);
            Assert.True(Sweet.lollipop.et_relations.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.True(Sweet.lollipop.et_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.IsNotNull(community.experimental_proteoforms.First().family);
            Assert.IsNotNull(community.experimental_proteoforms.First().gene_name);
            Assert.IsNotNull(community.experimental_proteoforms.First().linked_proteoform_references);
            Assert.IsNotEmpty(community.experimental_proteoforms.First().ptm_set.ptm_combination);
            Assert.True(community.experimental_proteoforms.First().relationships.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Sweet.lollipop.clear_ee();
            Assert.IsNotEmpty(Sweet.lollipop.et_relations);
            Assert.IsEmpty(Sweet.lollipop.ee_relations);
            Assert.IsEmpty(Sweet.lollipop.ee_peaks);
            Assert.IsNotEmpty(Sweet.lollipop.et_peaks);
            Assert.IsEmpty(community.families);
            Assert.False(Sweet.lollipop.ee_relations.Any(r => r.RelationType == ProteoformComparison.ExperimentalExperimental));
            Assert.False(Sweet.lollipop.ee_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalExperimental));
            Assert.Null(community.experimental_proteoforms.First().family);
            Assert.Null(community.experimental_proteoforms.First().gene_name);
            Assert.Null(community.experimental_proteoforms.First().linked_proteoform_references);
            Assert.IsEmpty(community.experimental_proteoforms.First().ptm_set.ptm_combination);
            Assert.False(community.experimental_proteoforms.First().relationships.Any(r => r.RelationType == ProteoformComparison.ExperimentalExperimental));
        }

        [Test]
        public void community_clear_td()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.target_proteoform_community = construct_community_with_td_proteoforms(-100);
            Sweet.lollipop.decoy_proteoform_communities.Add(Sweet.lollipop.decoy_community_name_prefix + "0", new ProteoformCommunity());
            Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"] = construct_community_with_td_proteoforms(0);
            Sweet.lollipop.et_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            foreach (ProteoformRelation pr in Sweet.lollipop.et_relations) pr.set_nearby_group(Sweet.lollipop.et_relations, Sweet.lollipop.et_relations.Select(r => r.InstanceId).ToList());
            Sweet.lollipop.min_peak_count_et = 1;
            Sweet.lollipop.et_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.et_relations, Sweet.lollipop.ed_relations);
            Sweet.lollipop.target_proteoform_community.construct_families();
            Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].construct_families();
            Sweet.lollipop.top_down_hits = new List<TopDownHit>() { new TopDownHit() };
            Assert.AreEqual(1, Sweet.lollipop.top_down_hits.Count);
            Assert.IsNotEmpty(Sweet.lollipop.topdown_proteoforms);
            Assert.IsTrue(Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Any(t => t.topdown_theoretical));
            Assert.IsTrue(Sweet.lollipop.theoretical_database.theoreticals_by_accession[-100].Values.SelectMany(t => t).Where(t => t.topdown_theoretical).Count() > 0);
            Assert.IsTrue(Sweet.lollipop.theoretical_database.theoreticals_by_accession[0].Values.SelectMany(t => t).Where(t => t.topdown_theoretical).Count() > 0);
            Sweet.lollipop.clear_td();
            Assert.IsEmpty(Sweet.lollipop.topdown_proteoforms);
            Assert.IsFalse(Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Any(t => t.topdown_theoretical));
            Assert.IsFalse(Sweet.lollipop.theoretical_database.theoreticals_by_accession[-100].Values.SelectMany(t => t).Where(t => t.topdown_theoretical).Count() > 0);
            Assert.IsFalse(Sweet.lollipop.theoretical_database.theoreticals_by_accession[0].Values.SelectMany(t => t).Where(t => t.topdown_theoretical).Count() > 0);
        }

        [Test]
        public void community_clear_families()
        {
            Sweet.lollipop = new Lollipop();
            ProteoformCommunity target = construct_two_families_with_potentially_colliding_theoreticals();
            Sweet.lollipop = new Lollipop();
            ProteoformCommunity decoy = construct_two_families_with_potentially_colliding_theoreticals();
            Sweet.lollipop.target_proteoform_community = target;
            Sweet.lollipop.decoy_proteoform_communities.Add(Sweet.lollipop.decoy_community_name_prefix + "0", decoy);
            Assert.IsNotEmpty(Sweet.lollipop.target_proteoform_community.families);
            Assert.True(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Any(p => p.family != null));
            Assert.IsNotEmpty(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].families);
            Assert.True(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].experimental_proteoforms.Any(p => p.family != null));
            Sweet.lollipop.clear_all_families();
            Assert.IsEmpty(Sweet.lollipop.target_proteoform_community.families);
            Assert.True(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.All(p => p.family == null));
            Assert.IsEmpty(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].families);
            Assert.True(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].experimental_proteoforms.All(p => p.family == null));

            Sweet.lollipop = new Lollipop();
            target = construct_community_with_td_proteoforms(-100);
            decoy = construct_community_with_td_proteoforms(0);
            target.clear_families();
            target.construct_families();
            decoy.clear_families();
            decoy.construct_families();
            Sweet.lollipop.target_proteoform_community = target;
            Sweet.lollipop.decoy_proteoform_communities.Add(Sweet.lollipop.decoy_community_name_prefix + "0", decoy);
            Assert.IsNotEmpty(Sweet.lollipop.target_proteoform_community.families);
            Assert.True(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Any(p => p.family != null));
            Assert.True(Sweet.lollipop.topdown_proteoforms.Any(p => p.family != null));
            Assert.IsNotEmpty(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].families);
            Assert.True(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].experimental_proteoforms.Any(p => p.family != null));
            Sweet.lollipop.clear_all_families();
            Assert.IsEmpty(Sweet.lollipop.target_proteoform_community.families);
            Assert.True(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.All(p => p.family == null));
            Assert.True(Sweet.lollipop.topdown_proteoforms.All(p => p.family == null));
            Assert.IsEmpty(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].families);
            Assert.True(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].experimental_proteoforms.All(p => p.family == null));
        }
    }
}
