using NUnit.Framework;
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
        ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct> { new ProteolysisProduct(0, 0, "") }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });

        [Test]
        public void TestNeuCodeLabeledProteoformCommunityRelate_EE()
        {
            Sweet.lollipop.neucode_labeled = true;

            // Two proteoforms; lysine count equal; mass difference < 250 -- return 1
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true);
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, 1, true);
            ExperimentalProteoform[] pa1 = new ExperimentalProteoform[2];
            pa1[0] = pf1;
            pa1[1] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(1, prList.Count);

            // Two proteoforms; lysine count equal; mass difference > 250 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 2000;
            pf2.lysine_count = 1;
            pa1[0] = pf1;
            pa1[1] = pf2;
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(0, prList.Count);

            // Two proteoforms; lysine count NOT equal; mass difference < 250 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 1100;
            pf2.lysine_count = 2;
            pa1[0] = pf1;
            pa1[1] = pf2;
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(0, prList.Count);

            //Three proteoforms; lysine count equal; mass difference < 250 Da -- return 3
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true);
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, 1, true);
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("A3", 1020.0, 1, true);
            ExperimentalProteoform[] pa2 = new ExperimentalProteoform[3];
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
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
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
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
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
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
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(0, prList.Count);

        }

        [Test]
        public void TestUnabeledProteoformCommunityRelate_EE()
        {
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.ee_max_mass_difference = 250;

            // Two proteoforms; mass difference < 250 -- return 1
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, -1, true);
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, -1, true);
            ExperimentalProteoform[] pa1 = new ExperimentalProteoform[2];
            pa1[0] = pf1;
            pa1[1] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(1, prList.Count);

            // Two proteoforms; mass difference > 250 -- return 0
            pf1.modified_mass = 1000;
            pf2.modified_mass = 2000;
            pa1[0] = pf1;
            pa1[1] = pf2;
            prList = community.relate(pa1, pa1, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(0, prList.Count);

            //Three proteoforms; mass difference < 250 Da -- return 3
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, -1, true);
            ExperimentalProteoform pf4 = ConstructorsForTesting.ExperimentalProteoform("A2", 1010.0, -1, true);
            ExperimentalProteoform pf5 = ConstructorsForTesting.ExperimentalProteoform("A3", 1020.0, -1, true);
            ExperimentalProteoform[] pa2 = new ExperimentalProteoform[3];
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(3, prList.Count);

            //Three proteoforms; one mass difference < 250 Da -- return 1
            pf3.modified_mass = 1000;
            pf4.modified_mass = 1010;
            pf5.modified_mass = 2000;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            Assert.AreEqual(1, prList.Count);

            //Three proteoforms; mass difference > 250 Da -- return 0
            pf3.modified_mass = 1000;
            pf4.modified_mass = 2000;
            pf5.modified_mass = 3000;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = community.relate(pa2, pa2, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
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
            Sweet.lollipop.neucode_labeled = true;
            test_community.experimental_proteoforms = new ExperimentalProteoform[] {
                ConstructorsForTesting. ExperimentalProteoform("A1", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A2", 1000.0, 5, true),
                ConstructorsForTesting. ExperimentalProteoform("A3", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A4", 1000.0, 2, true)
            };
            Sweet.lollipop.ee_relations = test_community.relate(test_community.experimental_proteoforms, test_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            unequal_relations = test_community.relate_ef(test_community.experimental_proteoforms, test_community.experimental_proteoforms);
            Assert.AreNotEqual(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[2]);
            Assert.False(test_community.allowed_relation(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[0], ProteoformComparison.ExperimentalExperimental));
            Assert.AreNotEqual(test_community.experimental_proteoforms[0].lysine_count, test_community.experimental_proteoforms[1].lysine_count);
            //Assert.False(test_community.allowed_relation(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[1], ProteoformComparison.ExperimentalExperimental)); // this is taken care of in relate, now
            Assert.True(test_community.allowed_relation(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[2], ProteoformComparison.ExperimentalExperimental));
            //Assert.False(test_community.allowed_relation(test_community.experimental_proteoforms[0], test_community.experimental_proteoforms[3], ProteoformComparison.ExperimentalExperimental)); // this is taken care of in relate, now
            Assert.AreEqual(2, unequal_relations.Count); //only 2 relations are > 3 lysines apart

            //Two equal, two unequal lysine count. But one each has mass_difference > 250, so no relations
            test_community = new ProteoformCommunity();
            Sweet.lollipop.ee_relations.Clear();
            test_community.experimental_proteoforms = new ExperimentalProteoform[] {
                ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true),
                ConstructorsForTesting.ExperimentalProteoform("A2", 2000, 2, true),
                ConstructorsForTesting.ExperimentalProteoform("A3", 3000, 1, true),
                ConstructorsForTesting.ExperimentalProteoform("A4", 4000, 2, true)
            };
            Sweet.lollipop.ee_relations = test_community.relate(test_community.experimental_proteoforms, test_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            unequal_relations = test_community.relate_ef(test_community.experimental_proteoforms, test_community.experimental_proteoforms);
            Assert.AreEqual(0, unequal_relations.Count);

            //None equal lysine count (apart from itself), four unequal lysine count. Each should create no unequal relations, so no relations total
            test_community = new ProteoformCommunity();
            test_community.experimental_proteoforms = new ExperimentalProteoform[] {
                ConstructorsForTesting. ExperimentalProteoform("A1", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A2", 1000.0, 2, true),
                ConstructorsForTesting. ExperimentalProteoform("A3", 1000.0, 3, true),
                ConstructorsForTesting. ExperimentalProteoform("A4", 1000.0, 4, true)
            };
            Sweet.lollipop.ee_relations = test_community.relate(test_community.experimental_proteoforms, test_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true, TestContext.CurrentContext.TestDirectory, false);
            unequal_relations = test_community.relate_ef(test_community.experimental_proteoforms, test_community.experimental_proteoforms);
            Assert.AreEqual(0, unequal_relations.Count);

            //All equal, no unequal lysine count because there's an empty list of unequal lysine-count proteoforms. Each should create no unequal relations, so no relations total
            test_community = new ProteoformCommunity();
            test_community.experimental_proteoforms = new ExperimentalProteoform[] {
                ConstructorsForTesting. ExperimentalProteoform("A1", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A2", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A3", 1000.0, 1, true),
                ConstructorsForTesting. ExperimentalProteoform("A4", 1000.0, 1, true)
            };
            unequal_relations = test_community.relate_ef(test_community.experimental_proteoforms, test_community.experimental_proteoforms);
            Assert.AreEqual(0, unequal_relations.Count);
        }

        public static void prepare_for_et(List<double> delta_masses)
        {
            Sweet.lollipop.theoretical_database.all_mods_with_mass = new List<ModificationWithMass>();
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>();
            Sweet.lollipop.modification_ranks = new Dictionary<double, int>();

            //Prepare for making ET relation
            foreach (double delta_m in new HashSet<double>(delta_masses))
            {
                ModificationWithMass m = ConstructorsForTesting.get_modWithMass("fake" + delta_m.ToString(), delta_m);
                Sweet.lollipop.theoretical_database.all_mods_with_mass.Add(m);
                Sweet.lollipop.theoretical_database.all_possible_ptmsets.Add(new PtmSet(new List<Ptm> { new Ptm(-1, m) }));
                Sweet.lollipop.modification_ranks.Add(delta_m, 2);
            }
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = Sweet.lollipop.theoretical_database.make_ptmset_dictionary();

            if (!Sweet.lollipop.modification_ranks.TryGetValue(0, out int a))
                Sweet.lollipop.modification_ranks.Add(0, 1);

            Sweet.lollipop.mod_rank_sum_threshold = 2;
        }

        [Test]
        public void TestNeuCodeLabeledProteoformCommunityRelate_ET()
        {
            Sweet.lollipop.neucode_labeled = true;

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
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            Assert.AreEqual(1, prList.Count);

            // One experimental one theoretical proteoforms; lysine count equal; mass difference > 500 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 2000;
            pf2.lysine_count = 1;
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });

            paE[0] = pf1;
            paT[0] = pf2;
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            Assert.AreEqual(0, prList.Count);

            // One experimental one theoretical proteoforms; lysine count NOT equal; mass difference < 500 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 1100;
            pf2.lysine_count = 2;
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });

            paE[0] = pf1;
            paT[0] = pf2;
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
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
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
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
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
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
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
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
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            Assert.AreEqual(0, prList.Count);
        }

        [Test]
        public void TestUnabeledProteoformCommunityRelate_ET()
        {
            Sweet.lollipop.neucode_labeled = false;

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
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            Assert.AreEqual(1, prList.Count);

            // One experimental one theoretical protoeform; mass difference > 500 -- return 0
            pf1.modified_mass = 1000;
            pf2.modified_mass = 2000;
            paE[0] = pf1;
            paT[0] = pf2;
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });
            prList = community.relate(paE, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
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
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
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
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
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
            prList = community.relate(paE2, paT, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            Assert.AreEqual(0, prList.Count);
        }


        [Test]
        public void TestProteoformCommunityRelate_ED()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.decoy_databases = 1;
            // In empty comminity, relate ed is empty
            Assert.AreEqual(0, Sweet.lollipop.ed_relations.Count);

            //create a decoy proteoform community
            Sweet.lollipop.decoy_proteoform_communities.Add(Sweet.lollipop.decoy_community_name_prefix + "0", new ProteoformCommunity());            
            TheoreticalProteoform pf2 = ConstructorsForTesting.make_a_theoretical("decoyProteoform1", 0, -1);
            Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].theoretical_proteoforms = new TheoreticalProteoform[1] { pf2 };
            Sweet.lollipop.relate_ed();
            // Have a single decoy community --> have single ed_relations 
            Assert.AreEqual(1, Sweet.lollipop.ed_relations.Count);
            // But it's empty
            Assert.IsEmpty(Sweet.lollipop.ed_relations[Sweet.lollipop.decoy_community_name_prefix + "0"]);

            // In order to make it not empty, we must have relate_et method output a non-empty List
            // it must take as arguments non-empty pfs1 and pfs2
            // So testProteoformCommunity.experimental_proteoforms must be non-empty
            // And decoy_proteoforms["fake_decoy_proteoform1"] must be non-empty
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("experimentalProteoform1");
            Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].theoretical_proteoforms.First().ExpandedProteinList = new List<ProteinWithGoTerms> { p1 };

            Assert.IsEmpty(Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].experimental_proteoforms);
            Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].experimental_proteoforms = new ExperimentalProteoform[] { pf1 };

            Sweet.lollipop.clear_et();
            prepare_for_et(new List<double> { pf1.modified_mass - pf2.modified_mass });
            Sweet.lollipop.relate_ed();

            // Make sure there is one relation total, because only a single decoy was provided
            Assert.AreEqual(1, Sweet.lollipop.ed_relations.Count);
            Assert.IsNotEmpty(Sweet.lollipop.ed_relations);
            Assert.AreEqual(1, Sweet.lollipop.ed_relations[Sweet.lollipop.decoy_community_name_prefix + "0"].Count); // Make sure there is one relation for the provided fake_decoy_proteoform1

            ProteoformRelation rel = Sweet.lollipop.ed_relations[Sweet.lollipop.decoy_community_name_prefix + "0"][0];

            Assert.IsFalse(rel.Accepted);
            Assert.AreEqual("decoyProteoform1", rel.connected_proteoforms[1].accession);
            Assert.AreEqual(0, rel.DeltaMass);
            Assert.IsEmpty(((TheoreticalProteoform)rel.connected_proteoforms[1]).fragment);
            Assert.AreEqual(1, rel.nearby_relations_count);  //shows that calculate_unadjusted_group_count works
            //Assert.AreEqual(1, rel.mass_difference_group.Count);  //I don't think we need this test anymore w/ way peaks are made -LVS
            Assert.AreEqual(-1, rel.lysine_count);
            Assert.AreEqual("T2", ((TheoreticalProteoform)rel.connected_proteoforms[1]).name);
            Assert.AreEqual(0, ((ExperimentalProteoform)rel.connected_proteoforms[0]).aggregated.Count); //nothing aggregated with the basic constructor
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

        [Test]
        public void test_no_mans_land()
        {
            var e = ConstructorsForTesting.ExperimentalProteoform("");
            var ee = ProteoformComparison.ExperimentalExperimental;

            // Small masses, tight tolerance
            ProteoformRelation r1 = new ProteoformRelation(e, e, ee, 0.015 + Sweet.lollipop.peak_width_base_ee / 2, TestContext.CurrentContext.TestDirectory);
            Assert.IsTrue(r1.outside_no_mans_land);
            ProteoformRelation r2 = new ProteoformRelation(e, e, ee, 0.016 + Sweet.lollipop.peak_width_base_ee / 2, TestContext.CurrentContext.TestDirectory);
            Assert.IsFalse(r2.outside_no_mans_land);
            ProteoformRelation r3 = new ProteoformRelation(e, e, ee, -0.967 + Sweet.lollipop.peak_width_base_ee / 2, TestContext.CurrentContext.TestDirectory);
            Assert.IsTrue(r3.outside_no_mans_land);
            ProteoformRelation r4 = new ProteoformRelation(e, e, ee, -0.966 + Sweet.lollipop.peak_width_base_ee / 2, TestContext.CurrentContext.TestDirectory);
            Assert.IsFalse(r4.outside_no_mans_land);

            // Larger masses, larger tolerance
            ProteoformRelation r5 = new ProteoformRelation(e, e, ee, 200.22 + Sweet.lollipop.peak_width_base_ee / 2, TestContext.CurrentContext.TestDirectory);
            Assert.IsTrue(r5.outside_no_mans_land);
            ProteoformRelation r6 = new ProteoformRelation(e, e, ee, 200.23 + Sweet.lollipop.peak_width_base_ee / 2, TestContext.CurrentContext.TestDirectory);
            Assert.IsFalse(r6.outside_no_mans_land);
            ProteoformRelation r7 = new ProteoformRelation(e, e, ee, 200.92 - Sweet.lollipop.peak_width_base_ee / 2, TestContext.CurrentContext.TestDirectory);
            Assert.IsTrue(r7.outside_no_mans_land);
            ProteoformRelation r8 = new ProteoformRelation(e, e, ee, 200.91 - Sweet.lollipop.peak_width_base_ee / 2, TestContext.CurrentContext.TestDirectory);
            Assert.IsFalse(r8.outside_no_mans_land);
        }
    }
}
