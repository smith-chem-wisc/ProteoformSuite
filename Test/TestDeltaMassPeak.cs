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

            base_relation.nearby_relations = base_relation.set_nearby_group(theList);

            Console.WriteLine("Creating deltaMassPeak");
            DeltaMassPeak deltaMassPeak = new DeltaMassPeak(base_relation, theList);
            Console.WriteLine("Created deltaMassPeak");

            Assert.AreEqual(0, deltaMassPeak.peak_group_fdr);

            Dictionary<string, List<ProteoformRelation>> decoy_relations = new Dictionary<string, List<ProteoformRelation>>();

            decoy_relations["decoyDatabase1"] = new List<ProteoformRelation>();

            ExperimentalProteoform pf7 = new ExperimentalProteoform("experimental1");
            TheoreticalProteoform pf8 = new TheoreticalProteoform("decoy1");
            ProteoformComparison relation_type4 = ProteoformComparison.ed;
            double delta_mass4 = 1;
            ProteoformRelation decoy_relation = new ProteoformRelation(pf7, pf8, relation_type4, delta_mass4);

            decoy_relations["decoyDatabase1"].Add(decoy_relation);

            deltaMassPeak.calculate_fdr(decoy_relations);
            Assert.AreEqual(0.25, deltaMassPeak.peak_group_fdr); // 1 decoy database, (1 decoy relation, median=1), 4 target relations

            decoy_relations["decoyDatabase2"] = new List<ProteoformRelation>();
            decoy_relations["decoyDatabase2"].Add(decoy_relation);
            decoy_relations["decoyDatabase2"].Add(decoy_relation);

            deltaMassPeak.calculate_fdr(decoy_relations);
            Assert.AreEqual(0.375, deltaMassPeak.peak_group_fdr); // 2 decoy databases (1 & 2 decoy relations, median=1.5), 4 target relations
        }

        [Test]
        public void TestAcceptDeltaMassPeaks()
        {
            ProteoformCommunity test_community = new ProteoformCommunity();
            Lollipop.uniprotModificationTable = new Dictionary<string, Modification> { { "unmodified", new Modification() } };

            //Testing the acceptance of peaks. The FDR is tested above, so I'm not going to work with that here.
            //Four proteoforms, three relations (linear), middle one isn't accepted; should give 2 families
            Lollipop.min_peak_count = 2;
            ExperimentalProteoform pf3 = new ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = new ExperimentalProteoform("E2");
            ExperimentalProteoform pf5 = new ExperimentalProteoform("E3");
            ExperimentalProteoform pf6 = new ExperimentalProteoform("E4");

            ProteoformComparison comparison34 = ProteoformComparison.ee;
            ProteoformComparison comparison45 = ProteoformComparison.ee;
            ProteoformComparison comparison56 = ProteoformComparison.ee;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, comparison34, 0);
            ProteoformRelation pr3 = new ProteoformRelation(pf4, pf5, comparison45, 0);
            ProteoformRelation pr4 = new ProteoformRelation(pf5, pf6, comparison56, 0);

            List<ProteoformRelation> prs2 = new List<ProteoformRelation> { pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs2) pr.set_nearby_group(prs2);
            Assert.AreEqual(3, pr2.nearby_relations_count);
            Assert.AreEqual(3, pr3.nearby_relations_count);
            Assert.AreEqual(3, pr4.nearby_relations_count);

            test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            Assert.AreEqual(1, test_community.delta_mass_peaks.Count);
            DeltaMassPeak peak = test_community.delta_mass_peaks[0];
            Assert.AreEqual(3, peak.grouped_relations.Count);
            Assert.AreEqual("unmodified", peak.possiblePeakAssignments_string);
            peak.possiblePeakAssignments.Add(new Modification());
            Assert.AreEqual("unmodified; unmodified", peak.possiblePeakAssignments_string);

            //Test that the relations in the peak are added to each of the proteoforms referenced in the peak
            Assert.True(pf3.relationships.Contains(pr2));
            Assert.True(pf4.relationships.Contains(pr2) && pf4.relationships.Contains(pr3));
            Assert.True(pf5.relationships.Contains(pr3) && pf5.relationships.Contains(pr4));
        }
    }
}
