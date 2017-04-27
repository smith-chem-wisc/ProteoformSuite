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
            ProteoformCommunity test_community = new ProteoformCommunity();
            SaveState.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified", "unknown") } } };

            //One accepted ET relation; should give one ProteoformFamily
            SaveState.lollipop.min_peak_count_et = 1;
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1");
            pf1.accepted = true;
            TheoreticalProteoform pf2 = ConstructorsForTesting.make_a_theoretical();
            pf2.name = "T1";
            ProteoformComparison comparison = ProteoformComparison.ExperimentalTheoretical;
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf2, comparison, 0);
            pr1.Accepted = true;
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.InstanceId).ToList());
            DeltaMassPeak peak = new DeltaMassPeak(prs[0], prs);
            test_community.delta_mass_peaks = new List<DeltaMassPeak> { peak };
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
            ProteoformCommunity test_community = new ProteoformCommunity();
            SaveState.lollipop.proteoform_community = test_community;

            SaveState.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified", "unknown") } } };

            SaveState.lollipop.min_peak_count_ee = 2;
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
            Assert.AreEqual(1, test_community.delta_mass_peaks.Count);
            Assert.AreEqual(3, test_community.delta_mass_peaks[0].grouped_relations.Count);

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
            ProteoformCommunity test_community = new ProteoformCommunity();
            SaveState.lollipop.proteoform_community = test_community;

            SaveState.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified", "unknown") } } };

            SaveState.lollipop.ee_max_mass_difference = 20;
            SaveState.lollipop.peak_width_base_ee = 0.015;
            SaveState.lollipop.min_peak_count_ee = 3; //needs to be high so that 0 peak accepted, other peak isn't.... 

            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("E2");
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("E3");
            ExperimentalProteoform pf6 = ConstructorsForTesting.ExperimentalProteoform("E4");
            ExperimentalProteoform pf7 = ConstructorsForTesting.ExperimentalProteoform("E5");

            ProteoformComparison comparison34 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison45 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison56 = ProteoformComparison.ExperimentalExperimental;
            ProteoformComparison comparison67 = ProteoformComparison.ExperimentalExperimental;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, comparison34, 0);
            ProteoformRelation pr3 = new ProteoformRelation(pf4, pf5, comparison45, 19); //not accepted
            ProteoformRelation pr4 = new ProteoformRelation(pf5, pf6, comparison56, 0);
            ProteoformRelation pr5 = new ProteoformRelation(pf6, pf7, comparison67, 0);

            List<ProteoformRelation> prs2 = new List<ProteoformRelation> { pr2, pr3, pr4, pr5 }.OrderBy(r => r.DeltaMass).ToList();
            foreach (ProteoformRelation pr in prs2) pr.set_nearby_group(prs2, prs2.Select(r => r.InstanceId).ToList());
            Assert.AreEqual(3, pr2.nearby_relations_count);
            Assert.AreEqual(1, pr3.nearby_relations_count);
            Assert.AreEqual(3, pr4.nearby_relations_count);
            Assert.AreEqual(3, pr5.nearby_relations_count);

            test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            Assert.AreEqual(2, test_community.delta_mass_peaks.Count);
            Assert.AreEqual(1, test_community.delta_mass_peaks.Where(peak => peak.Accepted).Count());
            Assert.AreEqual(3, test_community.delta_mass_peaks.Where(peak => peak.Accepted).First().grouped_relations.Count());

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
            SaveState.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified", "unknown") } } };

            InputFile f = new InputFile("fake.txt", Purpose.ProteinDatabase);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "name", "full_name", true, false, new List<DatabaseReference>(), new List<GoTerm>());
            Dictionary<InputFile, Protein[]> dict = new Dictionary<InputFile, Protein[]> {
                { f, new Protein[] { p1 } }
            };
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("T1_T1_asdf", p1, dict);
            

            //One accepted ET relation; should give one ProteoformFamily
            SaveState.lollipop.min_peak_count_et = 1;
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("E1");
            TheoreticalProteoformGroup pf2 = new TheoreticalProteoformGroup(new List<TheoreticalProteoform> { t });
            ProteoformComparison comparison = ProteoformComparison.ExperimentalTheoretical;
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf2, comparison, 0);
            pr1.Accepted = true;
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs, prs.Select(r => r.InstanceId).ToList());
            DeltaMassPeak peak = new DeltaMassPeak(prs[0], prs);
            test_community.delta_mass_peaks = new List<DeltaMassPeak> { peak };
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
            ProteoformCommunity community = new ProteoformCommunity();
            SaveState.lollipop.proteoform_community = community;
            SaveState.lollipop.theoretical_database.uniprotModifications = new Dictionary<string, List<Modification>>
            {
                { "unmodified", new List<Modification> { ConstructorsForTesting.get_modWithMass("unmodified", 0) } },
                { "fake", new List<Modification> { ConstructorsForTesting.get_modWithMass("fake", 19) } },
            };

            SaveState.lollipop.modification_ranks = new Dictionary<double, int> { { 0, 1 }, { 19, 2 } };
            SaveState.lollipop.mod_rank_sum_threshold = 2;
            SaveState.lollipop.theoretical_database.all_possible_ptmsets = PtmCombos.generate_all_ptmsets(1, SaveState.lollipop.theoretical_database.uniprotModifications.SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList(), SaveState.lollipop.modification_ranks, 1);
            SaveState.lollipop.theoretical_database.all_mods_with_mass = SaveState.lollipop.theoretical_database.uniprotModifications.SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList();

            SaveState.lollipop.ee_max_mass_difference = 20;
            SaveState.lollipop.peak_width_base_ee = 0.015;
            SaveState.lollipop.min_peak_count_ee = 3; //needs to be high so that 0 peak accepted, other peak isn't.... 
            SaveState.lollipop.min_peak_count_et = 2; //needs to be lower so the 2 ET relations are accepted 

            //TheoreticalProteoformGroup
            InputFile f = new InputFile("fake.txt", Purpose.ProteinDatabase);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("", p1_accession, new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, p1_name, p1_fullName, true, false, new List<DatabaseReference> { p1_dbRef }, new List<GoTerm> { p1_goterm });
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
            SaveState.lollipop.et_relations = prs_et;
            List<ProteoformRelation> prs_ee = prs.Where(r => r.RelationType == ProteoformComparison.ExperimentalExperimental).OrderBy(r => r.DeltaMass).ToList();
            SaveState.lollipop.ee_relations = prs_ee;
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
            Assert.AreEqual(3, community.delta_mass_peaks.Count);
            Assert.AreEqual(2, community.delta_mass_peaks.Where(peak => peak.Accepted).Count()); // 1 ET peak, 1 EE peak accepted
            Assert.AreEqual(1, community.delta_mass_peaks.Where(peak => peak.Accepted && peak.RelationType == ProteoformComparison.ExperimentalExperimental).Count());
            Assert.AreEqual(1, community.delta_mass_peaks.Where(peak => peak.Accepted && peak.RelationType == ProteoformComparison.ExperimentalTheoretical).Count());
            Assert.AreEqual(4, community.delta_mass_peaks.Where(peak => peak.Accepted && peak.RelationType == ProteoformComparison.ExperimentalExperimental).First().grouped_relations.Count());
            Assert.AreEqual(3, community.delta_mass_peaks.Where(peak => peak.Accepted && peak.RelationType == ProteoformComparison.ExperimentalTheoretical).First().grouped_relations.Count());

            community.construct_families();

            //Testing the identification of experimentals   
            //test with a modificationwithmass that's 0 mass, and then see that it crawls around and labels them each with growing ptm sets with that modification
            //test that the relation.represented_modification gets set
            Assert.True(community.relations_in_peaks.All(r => r.peak.DeltaMass != 19 || r.represented_ptmset == null));
            Assert.True(community.relations_in_peaks.All(r => r.peak.DeltaMass != 0 || r.represented_ptmset.ptm_combination.First().modification.id == "unmodified"));
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
            ProteoformCommunity.gene_centric_families = false;
            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            SaveState.lollipop.proteoform_community = community;

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
        public void test_results_summary_with_peaks()
        {
            SaveState.lollipop.theoretical_database.theoretical_proteins = new Dictionary<InputFile, Protein[]>();
            SaveState.lollipop.theoretical_database.expanded_proteins = new ProteinWithGoTerms[0];
            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            SaveState.lollipop.et_peaks = community.delta_mass_peaks.Where(peak => peak.Accepted && peak.RelationType == ProteoformComparison.ExperimentalTheoretical).ToList();
            SaveState.lollipop.ee_peaks = community.delta_mass_peaks.Where(peak => peak.Accepted && peak.RelationType == ProteoformComparison.ExperimentalExperimental).ToList();
            Assert.True(ResultsSummaryGenerator.generate_full_report().Length > 0);
        }

        [Test]
        public void gene_centric_family()
        {
            ProteoformCommunity.gene_centric_families = true;
            ProteoformCommunity.preferred_gene_label = Lollipop.gene_name_labels[1];

            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            Assert.AreEqual(1, community.families.Count);

            //Only the distinct gene names are kept in list
            Assert.AreEqual(String.Join("; ", new string[] { "", "GENE", "GENE" }.Distinct()), community.families.FirstOrDefault(f => f.proteoforms.Select(p => p.accession).Contains("E1")).gene_list); //both would give null preferred gene names, since that field isn't set up
        }

        [Test]
        public void community_clear_et()
        {
            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            Assert.IsNotEmpty(SaveState.lollipop.et_relations);
            Assert.IsNotEmpty(SaveState.lollipop.ee_relations);
            Assert.IsNotEmpty(community.families);
            Assert.True(community.relations_in_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.True(community.delta_mass_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.IsNotNull(community.experimental_proteoforms.First().family);
            Assert.IsNotNull(community.experimental_proteoforms.First().gene_name);
            Assert.IsNotNull(community.experimental_proteoforms.First().linked_proteoform_references);
            Assert.IsNotEmpty(community.experimental_proteoforms.First().ptm_set.ptm_combination);
            Assert.True(community.experimental_proteoforms.First().relationships.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            community.clear_et();
            Assert.IsEmpty(SaveState.lollipop.et_relations);
            Assert.IsNotEmpty(SaveState.lollipop.ee_relations);
            Assert.IsEmpty(community.families);
            Assert.False(community.relations_in_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.False(community.delta_mass_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
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
            Assert.IsNotEmpty(SaveState.lollipop.et_relations);
            Assert.IsNotEmpty(SaveState.lollipop.ee_relations);
            Assert.IsNotEmpty(community.families);
            Assert.True(community.relations_in_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.True(community.delta_mass_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            Assert.IsNotNull(community.experimental_proteoforms.First().family);
            Assert.IsNotNull(community.experimental_proteoforms.First().gene_name);
            Assert.IsNotNull(community.experimental_proteoforms.First().linked_proteoform_references);
            Assert.IsNotEmpty(community.experimental_proteoforms.First().ptm_set.ptm_combination);
            Assert.True(community.experimental_proteoforms.First().relationships.Any(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical));
            community.clear_ee();
            Assert.IsNotEmpty(SaveState.lollipop.et_relations);
            Assert.IsEmpty(SaveState.lollipop.ee_relations);
            Assert.IsEmpty(community.families);
            Assert.False(community.relations_in_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalExperimental));
            Assert.False(community.delta_mass_peaks.Any(r => r.RelationType == ProteoformComparison.ExperimentalExperimental));
            Assert.Null(community.experimental_proteoforms.First().family);
            Assert.Null(community.experimental_proteoforms.First().gene_name);
            Assert.Null(community.experimental_proteoforms.First().linked_proteoform_references);
            Assert.IsEmpty(community.experimental_proteoforms.First().ptm_set.ptm_combination);
            Assert.False(community.experimental_proteoforms.First().relationships.Any(r => r.RelationType == ProteoformComparison.ExperimentalExperimental));
        }

        [Test]
        public void community_clear_families()
        {
            ProteoformCommunity community = construct_two_families_with_potentially_colliding_theoreticals();
            Assert.IsNotEmpty(community.families);
            Assert.True(community.experimental_proteoforms.Any(p => p.family != null));
            community.clear_families();
            Assert.IsEmpty(community.families);
            Assert.True(community.experimental_proteoforms.All(p => p.family == null));
        }
    }
}
