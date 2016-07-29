using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    public class TestDeltaMassPeak
    {

        [OneTimeSetUp]
        public void setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }


        [Test]
        public void TestDeltaMassPeakConstructor()
        {
            ProteomeDatabaseReader proteomeDatabaseReader = new ProteomeDatabaseReader();

            ProteomeDatabaseReader.oldPtmlistFilePath = "UnitTestFiles\\ptmlist.txt";
            Lollipop.uniprotModificationTable = proteomeDatabaseReader.ReadUniprotPtmlist();


            ExperimentalProteoform pf1 = new ExperimentalProteoform("acession1");
            TheoreticalProteoform pf2 = new TheoreticalProteoform("acession2");
            ProteoformComparison relation_type = ProteoformComparison.et;
            double delta_mass = 1 - 1e-7;

            ExperimentalProteoform pf3 = new ExperimentalProteoform("acession3");
            TheoreticalProteoform pf4 = new TheoreticalProteoform("acession4");
            ProteoformComparison relation_type2 = ProteoformComparison.et;
            double delta_mass2 = 1;

            ExperimentalProteoform pf5 = new ExperimentalProteoform("acession5");
            TheoreticalProteoform pf6 = new TheoreticalProteoform("acession6");
            ProteoformComparison relation_type3 = ProteoformComparison.et;
            double delta_mass3 = 1 + 1e-7;

            ExperimentalProteoform pf55 = new ExperimentalProteoform("acession5");
            TheoreticalProteoform pf65 = new TheoreticalProteoform("acession6");
            ProteoformComparison relation_type35 = ProteoformComparison.et;
            double delta_mass35 = 1 + 2e-7;

            List<ProteoformRelation> theList = new List<ProteoformRelation>();

            theList.Add(new ProteoformRelation(pf1, pf2, relation_type, delta_mass));
            theList.Add(new ProteoformRelation(pf3, pf4, relation_type2, delta_mass2));
            theList.Add(new ProteoformRelation(pf5, pf6, relation_type3, delta_mass3));
            theList.Add(new ProteoformRelation(pf55, pf65, relation_type35, delta_mass35));

            ProteoformRelation base_relation = new ProteoformRelation(pf3, pf4, relation_type2, delta_mass2);

            base_relation.nearby_relations = base_relation.find_nearby_relations(theList);

            Console.WriteLine("Creating deltaMassPeak");
            DeltaMassPeak deltaMassPeak = new DeltaMassPeak(base_relation);
            Console.WriteLine("Created deltaMassPeak");

            Assert.AreEqual(0, deltaMassPeak.peak_group_fdr);

            Dictionary<string, List<ProteoformRelation>> decoy_relations = new Dictionary<string, List<ProteoformRelation>>();

            decoy_relations["decoyDatabase1"] = new List<ProteoformRelation>();

            ExperimentalProteoform pf7 = new ExperimentalProteoform("experimental1");
            TheoreticalProteoform pf8 = new TheoreticalProteoform("decoy1");
            ProteoformComparison relation_type4 = ProteoformComparison.ed;
            double delta_mass4 = 1;

            decoy_relations["decoyDatabase1"].Add(new ProteoformRelation(pf7, pf8, relation_type4, delta_mass4));

            deltaMassPeak.calculate_fdr(decoy_relations);

            Assert.AreEqual(0.25, deltaMassPeak.peak_group_fdr);
        }

    }
}
