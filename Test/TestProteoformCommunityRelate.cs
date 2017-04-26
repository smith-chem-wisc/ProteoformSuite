﻿using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class TestProteoformCommunityRelate
    {
        ProteoformCommunity community = new ProteoformCommunity();
        ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });

        [Test]
        public void TestNeuCodeLabeledProteoformCommunityRelate_EE()
        {
            SaveState.lollipop.neucode_labeled = true;

            // Two proteoforms; lysine count equal; mass difference < 250 -- return 1
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true);
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, 1, true);
            ExperimentalProteoform[] pa1 = new ExperimentalProteoform[2];
            pa1[0] = pf1;
            pa1[1] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(1, prList.Count);

            // Two proteoforms; lysine count equal; mass difference > 250 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 2000;
            pf2.lysine_count = 1;
            pa1[0] = pf1;
            pa1[1] = pf2;
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(0, prList.Count);

            // Two proteoforms; lysine count NOT equal; mass difference < 250 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 1100;
            pf2.lysine_count = 2;
            pa1[0] = pf1;
            pa1[1] = pf2;
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(0, prList.Count);

            //Three proteoforms; lysine count equal; mass difference < 250 Da -- return 3
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true);
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, 1, true);
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("A3", 1020.0, 1, true);
            ExperimentalProteoform[] pa2 = new ExperimentalProteoform[3];
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(3, prList.Count);

            //Three proteoforms; lysine count equal; one mass difference < 250 Da; one mass difference > 500 -- return 1
            pf3.modified_mass = 1000;
            pf3.lysine_count = 1;
            pf4.modified_mass = 1010;
            pf4.lysine_count = 1;
            pf5.modified_mass = 2020;
            pf5.lysine_count = 1;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(1, prList.Count);

            //Three proteoforms; lysine count NOT equal; mass difference < 250 Da -- return 0
            pf3.modified_mass = 1000;
            pf3.lysine_count = 1;
            pf4.modified_mass = 1010;
            pf4.lysine_count = 2;
            pf5.modified_mass = 1020;
            pf5.lysine_count = 3;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(0, prList.Count);

            //Three proteoforms; lysine count equal; mass difference > 250 Da -- return 0
            pf3.lysine_count = 1;
            pf3.modified_mass = 1000;
            pf4.lysine_count = 1;
            pf4.modified_mass = 1600;
            pf5.lysine_count = 1;
            pf5.modified_mass = 2500;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(0, prList.Count);

        }

        [Test]
        public void TestUnabeledProteoformCommunityRelate_EE()
        {
            SaveState.lollipop.neucode_labeled = false;

            // Two proteoforms; mass difference < 250 -- return 1
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, -1, true);
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, -1, true);
            ExperimentalProteoform[] pa1 = new ExperimentalProteoform[2];
            pa1[0] = pf1;
            pa1[1] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(1, prList.Count);

            // Two proteoforms; mass difference > 250 -- return 0
            pf1.modified_mass = 1000;
            pf2.modified_mass = 2000;
            pa1[0] = pf1;
            pa1[1] = pf2;
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(0, prList.Count);

            //Three proteoforms; mass difference < 250 Da -- return 3
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, -1, true);
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, -1, true);
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("A3", 1020.0, -1, true);
            ExperimentalProteoform[] pa2 = new ExperimentalProteoform[3];
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(3, prList.Count);

            //Three proteoforms; one mass difference < 250 Da -- return 1
            pf3.modified_mass = 1000;
            pf4.modified_mass = 1010;
            pf5.modified_mass = 2000;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(1, prList.Count);

            //Three proteoforms; mass difference > 250 Da -- return 0
            pf3.modified_mass = 1000;
            pf4.modified_mass = 2000;
            pf5.modified_mass = 3000;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true);
            Assert.AreEqual(0, prList.Count);
        }

        [Test]
        public void TestUnabeledProteoformCommunityRelate_EF()
        {
            ProteoformCommunity test_community;
            List<ProteoformRelation> unequal_relations;

            //Two equal, two unequal lysine count. Each should create two unequal relations, so eight relations total
            //However, it shouldn't compare to itself, so that would make 4 total relations
            test_community = new ProteoformCommunity();
            SaveState.lollipop.neucode_labeled = true;
            test_community.experimental_proteoforms = new ExperimentalProteoform[] {
                ConstructorsForTesting. ExperimentalProteoform("A1", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A2", 1000.0, 2, true),
                ConstructorsForTesting. ExperimentalProteoform("A3", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A4", 1000.0, 2, true)
            };
            SaveState.lollipop.ee_relations = test_community.relate(test_community.experimental_proteoforms, test_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true);
            unequal_relations = test_community.relate_ef(test_community.experimental_proteoforms, test_community.experimental_proteoforms).Values.First();
            Assert.AreNotEqual(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[2]);
            Assert.False(test_community.allowed_relation(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[0], ProteoformComparison.ExperimentalExperimental));
            Assert.AreNotEqual(test_community.experimental_proteoforms[0].lysine_count, test_community.experimental_proteoforms[1].lysine_count);
            Assert.False(test_community.allowed_relation(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[1], ProteoformComparison.ExperimentalExperimental));
            Assert.True(test_community.allowed_relation(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[2], ProteoformComparison.ExperimentalExperimental));
            Assert.False(test_community.allowed_relation(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[3], ProteoformComparison.ExperimentalExperimental));
            Assert.AreEqual(4, unequal_relations.Count);

            //Two equal, two unequal lysine count. But one each has mass_difference > 250, so no relations
            test_community = new ProteoformCommunity();
            Lollipop.ee_relations.Clear();
            test_community.experimental_proteoforms = new ExperimentalProteoform[] {
                ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true),
                ConstructorsForTesting.ExperimentalProteoform("A2", 2000, 2, true),
                ConstructorsForTesting.ExperimentalProteoform("A3", 3000, 1, true),
                ConstructorsForTesting.ExperimentalProteoform("A4", 4000, 2, true)
            };
            SaveState.lollipop.ee_relations = test_community.relate(test_community.experimental_proteoforms, test_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true);
            unequal_relations = test_community.relate_ef(test_community.experimental_proteoforms, test_community.experimental_proteoforms).Values.First();
            Assert.AreEqual(0, unequal_relations.Count);

            //None equal lysine count (apart from itself), four unequal lysine count. Each should create no unequal relations, so no relations total
            test_community = new ProteoformCommunity();
            test_community.experimental_proteoforms = new ExperimentalProteoform[] {
                ConstructorsForTesting. ExperimentalProteoform("A1", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A2", 1000.0, 2, true),
                ConstructorsForTesting. ExperimentalProteoform("A3", 1000.0, 3, true),
                ConstructorsForTesting. ExperimentalProteoform("A4", 1000.0, 4, true)
            };
            SaveState.lollipop.ee_relations = test_community.relate(test_community.experimental_proteoforms, test_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true);
            unequal_relations = test_community.relate_ef(test_community.experimental_proteoforms, test_community.experimental_proteoforms).Values.First();
            Assert.AreEqual(0, unequal_relations.Count);

            //All equal, no unequal lysine count because there's an empty list of unequal lysine-count proteoforms. Each should create no unequal relations, so no relations total
            test_community = new ProteoformCommunity();
            test_community.experimental_proteoforms = new ExperimentalProteoform[] {
                ConstructorsForTesting. ExperimentalProteoform("A1", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A2", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A3", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A4", 1000.0, 1, true)
            };
            unequal_relations = test_community.relate_ef(test_community.experimental_proteoforms, test_community.experimental_proteoforms).Values.First();
            Assert.AreEqual(0, unequal_relations.Count);
        }

        private void prepare_for_et(List<double> delta_masses)
        {
            SaveState.lollipop.theoretical_database.all_mods_with_mass = new List<ModificationWithMass>();
            SaveState.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>();
            SaveState.lollipop.modification_ranks = new Dictionary<double, int>();

            //Prepare for making ET relation
            foreach (double delta_m in new HashSet<double>(delta_masses))
            {
                ModificationWithMass m = ConstructorsForTesting.get_modWithMass("fake" + delta_m.ToString(), delta_m);
                SaveState.lollipop.theoretical_database.all_mods_with_mass.Add(m);
                SaveState.lollipop.theoretical_database.all_possible_ptmsets.Add(new PtmSet(new List<Ptm> { new Ptm(-1, m) }));
                SaveState.lollipop.modification_ranks.Add(delta_m, 2);
            }
            SaveState.lollipop.theoretical_database.possible_ptmset_dictionary = SaveState.lollipop.theoretical_database.make_ptmset_dictionary();

<<<<<<< HEAD
            Lollipop.make_ptmset_dictionary();

            if (!Lollipop.modification_ranks.TryGetValue(0, out int a))
                Lollipop.modification_ranks.Add(0, 1);
=======
            if (!SaveState.lollipop.modification_ranks.TryGetValue(0, out int a))
                SaveState.lollipop.modification_ranks.Add(0, 1);
>>>>>>> 4256719b0a3d908269a3d7b54f0a7594ccb09f5b

            SaveState.lollipop.mod_rank_sum_threshold = 2;
        }

        [Test]
        public void TestNeuCodeLabeledProteoformCommunityRelate_ET()
        {
            SaveState.lollipop.neucode_labeled = true;

            // One experimental one theoretical proteoforms; lysine count equal; mass difference < 500 -- return 1
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true);
            TheoreticalProteoform pf2 = ConstructorsForTesting.make_a_theoretical();
            pf2.modified_mass = 1010.0;
            pf2.lysine_count = 1;
            pf2.is_target = true;
            pf2.ExpandedProteinList = new List<ProteinWithGoTerms> { p1 };
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });

            ExperimentalProteoform[] paE = new ExperimentalProteoform[1];
            TheoreticalProteoform[] paT = new TheoreticalProteoform[1];
            paE[0] = pf1;
            paT[0] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(1, prList.Count);

            // One experimental one theoretical proteoforms; lysine count equal; mass difference > 500 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 2000;
            pf2.lysine_count = 1;
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });

            paE[0] = pf1;
            paT[0] = pf2;
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(0, prList.Count);

            // One experimental one theoretical proteoforms; lysine count NOT equal; mass difference < 500 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 1100;
            pf2.lysine_count = 2;
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });

            paE[0] = pf1;
            paT[0] = pf2;
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(0, prList.Count);

            //Two experimental one theoretical proteoforms; lysine count equal; mass difference < 500 Da -- return 2
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true);
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, 1, true);
            TheoreticalProteoform pf5 = ConstructorsForTesting.make_a_theoretical();
            pf5.modified_mass = 1020.0;
            pf5.lysine_count = 1;
            pf5.is_target = true;
            pf5.ExpandedProteinList = new List<ProteinWithGoTerms> { p1 };
            ExperimentalProteoform[] paE2 = new ExperimentalProteoform[2];
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prepare_for_et(new List<double> {
                pf3.modified_mass - pf5.modified_mass,
                pf4.modified_mass - pf5.modified_mass,
            });
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(2, prList.Count);

            //Two experimental one theoretical proteoforms; lysine count equal; one mass difference < 500 Da; one mass difference > 500 -- return 1
            pf3.modified_mass = 1000;
            pf3.lysine_count = 1;
            pf4.modified_mass = 1500;
            pf4.lysine_count = 1;
            pf5.modified_mass = 1510;
            pf5.lysine_count = 1;
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prepare_for_et(new List<double> {
                pf3.modified_mass - pf5.modified_mass,
                pf4.modified_mass - pf5.modified_mass,
            });
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(1, prList.Count);

            //Two experimental one theoretical proteoforms; lysine count NOT equal; mass difference < 500 Da -- return 0
            pf3.modified_mass = 1000;
            pf3.lysine_count = 1;
            pf4.modified_mass = 1010;
            pf4.lysine_count = 2;
            pf5.modified_mass = 1020;
            pf5.lysine_count = 3;
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prepare_for_et(new List<double> {
                pf3.modified_mass - pf5.modified_mass,
                pf4.modified_mass - pf5.modified_mass,
            });
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(0, prList.Count);

            //Two experimental one theoretical proteoforms; lysine count equal; mass difference > 500 Da -- return 0
            pf3.lysine_count = 1;
            pf3.modified_mass = 1000;
            pf4.lysine_count = 1;
            pf4.modified_mass = 1600;
            pf5.lysine_count = 1;
            pf5.modified_mass = 2500;
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prepare_for_et(new List<double> {
                pf3.modified_mass - pf5.modified_mass,
                pf4.modified_mass - pf5.modified_mass,
            });
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(0, prList.Count);
        }

        [Test]
        public void TestUnabeledProteoformCommunityRelate_ET()
        {
            SaveState.lollipop.neucode_labeled = false;

            // One experimental one theoretical protoeform; mass difference < 500 -- return 1
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, -1, true);
            TheoreticalProteoform pf2 = ConstructorsForTesting.make_a_theoretical();
            pf2.modified_mass = 1010.0;
            pf2.lysine_count = 1;
            pf2.is_target = true;
            pf2.ExpandedProteinList = new List<ProteinWithGoTerms> { p1 };
            ExperimentalProteoform[] paE = new ExperimentalProteoform[1];
            TheoreticalProteoform[] paT = new TheoreticalProteoform[1];
            paE[0] = pf1;
            paT[0] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(1, prList.Count);

            // One experimental one theoretical protoeform; mass difference > 500 -- return 0
            pf1.modified_mass = 1000;
            pf2.modified_mass = 2000;
            paE[0] = pf1;
            paT[0] = pf2;
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(0, prList.Count);

            //Two experimental one theoretical proteoforms; mass difference < 500 Da -- return 2
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, -1, true);
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, -1, true);
            TheoreticalProteoform pf5 = ConstructorsForTesting.make_a_theoretical();
            pf5.modified_mass = 1020.0;
            pf5.lysine_count = 1;
            pf5.is_target = true;
            pf5.ExpandedProteinList = new List<ProteinWithGoTerms> { p1 };
            ExperimentalProteoform[] paE2 = new ExperimentalProteoform[2];
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prepare_for_et(new List<double> {
                pf3.modified_mass - pf5.modified_mass,
                pf4.modified_mass - pf5.modified_mass,
            });
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(2, prList.Count);

            //Two experimental one theoretical proteoforms; one mass difference >500 Da -- return 0
            pf3.modified_mass = 1000;
            pf4.modified_mass = 1010;
            pf5.modified_mass = 2000;
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prepare_for_et(new List<double> {
                pf3.modified_mass - pf5.modified_mass,
                pf4.modified_mass - pf5.modified_mass,
            });
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(0, prList.Count);

            //Two experimental one theoretical proteoforms; mass difference > 500 Da -- return 0
            pf3.modified_mass = 1000;
            pf4.modified_mass = 2000;
            pf5.modified_mass = 3000;
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prepare_for_et(new List<double> {
                pf3.modified_mass - pf5.modified_mass,
                pf4.modified_mass - pf5.modified_mass,
            });
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true);
            Assert.AreEqual(0, prList.Count);
        }


        [Test]
        public void TestProteoformCommunityRelate_ED()
        {
            ProteoformCommunity testProteoformCommunity = new ProteoformCommunity();
            var edDictionary = testProteoformCommunity.relate_ed();
            // In empty comminity, relate ed is empty
            Assert.AreEqual(0, edDictionary.Count);

            testProteoformCommunity.decoy_proteoforms = new Dictionary<string, TheoreticalProteoform[]>();
            edDictionary = testProteoformCommunity.relate_ed();
            // In comminity with initialized decoy_proteoforms, still no relations
            Assert.AreEqual(0, edDictionary.Count);

            testProteoformCommunity.decoy_proteoforms["fake_decoy_proteoform1"] = new TheoreticalProteoform[0];
            edDictionary = testProteoformCommunity.relate_ed();
            // In community with a single decoy proteoform, have a single relation
            Assert.AreEqual(1, edDictionary.Count);
            // But it's empty
            Assert.IsEmpty(edDictionary["fake_decoy_proteoform1"]);

            // In order to make it not empty, we must have relate_et method output a non-empty List
            // it must take as arguments non-empty pfs1 and pfs2
            // So testProteoformCommunity.experimental_proteoforms must be non-empty
            // And decoy_proteoforms["fake_decoy_proteoform1"] must be non-empty
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("experimentalProteoform1");
            TheoreticalProteoform pf2 = ConstructorsForTesting.make_a_theoretical("decoyProteoform1", 0, -1);
            testProteoformCommunity.decoy_proteoforms["fake_decoy_proteoform1"] = new TheoreticalProteoform[] { pf2 };
            testProteoformCommunity.decoy_proteoforms["fake_decoy_proteoform1"].First().ExpandedProteinList = new List<ProteinWithGoTerms> { p1 };

            Assert.IsEmpty(testProteoformCommunity.experimental_proteoforms);
            testProteoformCommunity.experimental_proteoforms = new ExperimentalProteoform[] { pf1 };
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });
            edDictionary = testProteoformCommunity.relate_ed();

            // Make sure there is one relation total, because only a single decoy was provided
            Assert.AreEqual(1, edDictionary.Count);
            Assert.IsNotEmpty(edDictionary["fake_decoy_proteoform1"]);
            Assert.AreEqual(1, edDictionary["fake_decoy_proteoform1"].Count); // Make sure there is one relation for the provided fake_decoy_proteoform1

            ProteoformRelation rel = edDictionary["fake_decoy_proteoform1"][0];

            Assert.IsFalse(rel.Accepted);
            Assert.AreEqual("decoyProteoform1", rel.connected_proteoforms[1].accession);
            Assert.AreEqual(0, rel.DeltaMass);
            Assert.IsEmpty(((TheoreticalProteoform)rel.connected_proteoforms[1]).fragment);
            Assert.AreEqual(1, rel.nearby_relations_count);  //shows that calculate_unadjusted_group_count works
            //Assert.AreEqual(1, rel.mass_difference_group.Count);  //I don't think we need this test anymore w/ way peaks are made -LVS
            Assert.AreEqual(-1, rel.lysine_count);
            Assert.AreEqual("T2", ((TheoreticalProteoform)rel.connected_proteoforms[1]).name);
            Assert.AreEqual(0, ((ExperimentalProteoform)rel.connected_proteoforms[0]).aggregated_components.Count); //nothing aggregated with the basic constructor
            Assert.IsTrue(rel.outside_no_mans_land);
            Assert.IsNull(rel.peak);
            Assert.True(string.Equals("unmodified", ((TheoreticalProteoform)rel.connected_proteoforms[1]).ptm_description, StringComparison.CurrentCultureIgnoreCase));
            Assert.AreEqual(1, rel.nearby_relations_count);
        }

        [Test]
        public void test_community_has_proteoforms()
        {
            community.experimental_proteoforms = new ExperimentalProteoform[] { };
            community.theoretical_proteoforms = new TheoreticalProteoform[] { };
            Assert.False(community.has_e_proteoforms);
            Assert.False(community.has_e_and_t_proteoforms);
            community.experimental_proteoforms = new ExperimentalProteoform[] { ConstructorsForTesting.ExperimentalProteoform("E1") };
            Assert.True(community.has_e_proteoforms);
            Assert.False(community.has_e_and_t_proteoforms);
            community.theoretical_proteoforms = new TheoreticalProteoform[] { ConstructorsForTesting.make_a_theoretical() };
            Assert.True(community.has_e_proteoforms);
            Assert.True(community.has_e_and_t_proteoforms);
        }
    }
}
