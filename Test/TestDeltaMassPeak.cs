using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    public class TestDeltaMassPeak
    {

        [Test]
        public void TestDeltaMassPeakConstructor()
        {
            Proteoform pf1 = new Proteoform("acession1");
            Proteoform pf2 = new Proteoform("acession2");
            ProteoformComparison relation_type = ProteoformComparison.et;
            double delta_mass = 1 - 1e-7;

            Proteoform pf3 = new Proteoform("acession3");
            Proteoform pf4 = new Proteoform("acession4");
            ProteoformComparison relation_type2 = ProteoformComparison.et;
            double delta_mass2 = 1;

            Proteoform pf5 = new Proteoform("acession5");
            Proteoform pf6 = new Proteoform("acession6");
            ProteoformComparison relation_type3 = ProteoformComparison.et;
            double delta_mass3 = 1 + 1e-7;

            Proteoform pf55 = new Proteoform("acession5");
            Proteoform pf65 = new Proteoform("acession6");
            ProteoformComparison relation_type35 = ProteoformComparison.et;
            double delta_mass35 = 1 + 2e-7;

            List<ProteoformRelation> theList = new List<ProteoformRelation>();

            theList.Add(new ProteoformRelation(pf1, pf2, relation_type, delta_mass));
            theList.Add(new ProteoformRelation(pf3, pf4, relation_type2, delta_mass2));
            theList.Add(new ProteoformRelation(pf5, pf6, relation_type3, delta_mass3));
            theList.Add(new ProteoformRelation(pf55, pf65, relation_type35, delta_mass35));

            ProteoformRelation base_relation = new ProteoformRelation(pf3, pf4, relation_type2, delta_mass2);

            base_relation.mass_difference_group = base_relation.find_nearby_relations(theList);

            Console.WriteLine("Creating deltaMassPeak");
            DeltaMassPeak deltaMassPeak = new DeltaMassPeak(base_relation);
            Console.WriteLine("Created deltaMassPeak");

            Assert.AreEqual(0, deltaMassPeak.group_fdr);

            Dictionary<string, List<ProteoformRelation>> decoy_relations = new Dictionary<string, List<ProteoformRelation>>();

            decoy_relations["decoyDatabase1"] = new List<ProteoformRelation>();

            Proteoform pf7 = new Proteoform("experimental1");
            Proteoform pf8 = new Proteoform("decoy1");
            ProteoformComparison relation_type4 = ProteoformComparison.ed;
            double delta_mass4 = 1;

            decoy_relations["decoyDatabase1"].Add(new ProteoformRelation(pf7, pf8, relation_type4, delta_mass4));

            deltaMassPeak.calculate_fdr(decoy_relations);

            Assert.AreEqual(0.25, deltaMassPeak.group_fdr);
        }

    }
}
