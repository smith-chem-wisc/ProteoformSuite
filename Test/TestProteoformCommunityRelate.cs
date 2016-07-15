using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    public class TestProteoformCommunityRelate
    {
        [Test]
        public void TestNeuCodeLabeledProteoformCommunityRelate_EE()
        {
            Lollipop.neucode_labeled = true;

            // Two proteoforms; lysine count equal; mass difference < 500 -- return 1
            Proteoform pf1 = new Proteoform("A1",1000.0,1,true);
            Proteoform pf2 = new Proteoform("A2", 1010.0, 1, true);
            Proteoform[] pa1 = new Proteoform[2];
            pa1[0] = pf1;
            pa1[1] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prList = ProteoformCommunity.relate(pa1, pa1, ProteoformComparison.ee);
            Assert.AreEqual(1, prList.Count);

            // Two proteoforms; lysine count equal; mass difference > 500 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 2000;
            pf2.lysine_count = 1;
            pa1[0] = pf1;
            pa1[1] = pf2;
            prList = ProteoformCommunity.relate(pa1, pa1, ProteoformComparison.ee);
            Assert.AreEqual(0, prList.Count);

            // Two proteoforms; lysine count NOT equal; mass difference < 500 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 1100;
            pf2.lysine_count = 2;
            pa1[0] = pf1;
            pa1[1] = pf2;
            prList = ProteoformCommunity.relate(pa1, pa1, ProteoformComparison.ee);
            Assert.AreEqual(0, prList.Count);

            //Three proteoforms; lysine count equal; mass difference < 500 Da -- return 3
            Proteoform pf3 = new Proteoform("A1", 1000.0, 1, true);
            Proteoform pf4 = new Proteoform("A2", 1010.0, 1, true);
            Proteoform pf5 = new Proteoform("A3", 1020.0, 1, true);
            Proteoform[] pa2 = new Proteoform[3];
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = ProteoformCommunity.relate(pa2, pa2, ProteoformComparison.ee);
            Assert.AreEqual(3, prList.Count);

            //Three proteoforms; lysine count equal; one mass difference < 500 Da; one mass difference > 500 -- return 1
            pf3.modified_mass = 1000;
            pf3.lysine_count = 1;
            pf4.modified_mass = 1010;
            pf4.lysine_count = 1;
            pf5.modified_mass = 2020;
            pf5.lysine_count = 1;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = ProteoformCommunity.relate(pa2, pa2, ProteoformComparison.ee);
            Assert.AreEqual(1, prList.Count);

            //Three proteoforms; lysine count NOT equal; mass difference < 500 Da -- return 0
            pf3.modified_mass = 1000;
            pf3.lysine_count = 1;
            pf4.modified_mass = 1010;
            pf4.lysine_count = 2;
            pf5.modified_mass = 1020;
            pf5.lysine_count = 3;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = ProteoformCommunity.relate(pa2, pa2, ProteoformComparison.ee);
            Assert.AreEqual(0, prList.Count);

            //Three proteoforms; lysine count equal; mass difference > 500 Da -- return 0
            pf3.lysine_count = 1;
            pf3.modified_mass = 1000;
            pf4.lysine_count = 1;
            pf4.modified_mass = 1600;
            pf5.lysine_count = 1;
            pf5.modified_mass = 2500;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = ProteoformCommunity.relate(pa2, pa2, ProteoformComparison.ee);
            Assert.AreEqual(0, prList.Count);


        }

        [Test]
        public void TestUnabeledProteoformCommunityRelate_EE()
        {
            Lollipop.neucode_labeled = false;

            // Two proteoforms; mass difference < 500 -- return 1
            Proteoform pf1 = new Proteoform("A1", 1000.0, -1, true);
            Proteoform pf2 = new Proteoform("A2", 1010.0, -1, true);
            Proteoform[] pa1 = new Proteoform[2];
            pa1[0] = pf1;
            pa1[1] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prList = ProteoformCommunity.relate(pa1, pa1, ProteoformComparison.ee);
            Assert.AreEqual(1, prList.Count);

            // Two proteoforms; mass difference > 500 -- return 0
            pf1.modified_mass = 1000;
            pf2.modified_mass = 2000;
            pa1[0] = pf1;
            pa1[1] = pf2;
            prList = ProteoformCommunity.relate(pa1, pa1, ProteoformComparison.ee);
            Assert.AreEqual(0, prList.Count);

            //Three proteoforms; mass difference < 500 Da -- return 3
            Proteoform pf3 = new Proteoform("A1", 1000.0, -1, true);
            Proteoform pf4 = new Proteoform("A2", 1010.0, -1, true);
            Proteoform pf5 = new Proteoform("A3", 1020.0, -1, true);
            Proteoform[] pa2 = new Proteoform[3];
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = ProteoformCommunity.relate(pa2, pa2, ProteoformComparison.ee);
            Assert.AreEqual(3, prList.Count);

            //Three proteoforms; one mass difference < 500 Da -- return 1
            pf3.modified_mass = 1000;
            pf4.modified_mass = 1010;
            pf5.modified_mass = 2000;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = ProteoformCommunity.relate(pa2, pa2, ProteoformComparison.ee);
            Assert.AreEqual(1, prList.Count);

            //Three proteoforms; mass difference > 500 Da -- return 0
            pf3.modified_mass = 1000;
            pf4.modified_mass = 2000;
            pf5.modified_mass = 3000;
            pa2[0] = pf3;
            pa2[1] = pf4;
            pa2[2] = pf5;
            prList = ProteoformCommunity.relate(pa2, pa2, ProteoformComparison.ee);
            Assert.AreEqual(0, prList.Count);
        }

        [Test]
        public void TestNeuCodeLabeledProteoformCommunityRelate_ET()
        {
            Lollipop.neucode_labeled = true;

            // One experimental one theoretical proteoforms; lysine count equal; mass difference < 500 -- return 1
            Proteoform pf1 = new Proteoform("A1", 1000.0, 1, true);
            Proteoform pf2 = new Proteoform("T1", 1010.0, 1, true);
            Proteoform[] paE = new Proteoform[1];
            Proteoform[] paT = new Proteoform[1];
            paE[0] = pf1;
            paT[0] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prList = ProteoformCommunity.relate(paE, paT, ProteoformComparison.et);
            Assert.AreEqual(1, prList.Count);

            // One experimental one theoretical proteoforms; lysine count equal; mass difference > 500 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 2000;
            pf2.lysine_count = 1;
            paE[0] = pf1;
            paT[0] = pf2;
            prList = ProteoformCommunity.relate(paE, paT, ProteoformComparison.et);
            Assert.AreEqual(0, prList.Count);

            // One experimental one theoretical proteoforms; lysine count NOT equal; mass difference < 500 -- return 0
            pf1.modified_mass = 1000;
            pf1.lysine_count = 1;
            pf2.modified_mass = 1100;
            pf2.lysine_count = 2;
            paE[0] = pf1;
            paT[0] = pf2;
            prList = ProteoformCommunity.relate(paE, paT, ProteoformComparison.et);
            Assert.AreEqual(0, prList.Count);

            //Two experimental one theoretical proteoforms; lysine count equal; mass difference < 500 Da -- return 2
            Proteoform pf3 = new Proteoform("A1", 1000.0, 1, true);
            Proteoform pf4 = new Proteoform("A2", 1010.0, 1, true);
            Proteoform pf5 = new Proteoform("T1", 1020.0, 1, true);
            Proteoform[] paE2 = new Proteoform[2];
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prList = ProteoformCommunity.relate(paE2, paT, ProteoformComparison.et);
            Assert.AreEqual(2, prList.Count);

            //Two experimental one theoretical proteoforms; lysine count equal; one mass difference < 500 Da; one mass difference > 500 -- return 0
            pf3.modified_mass = 1000;
            pf3.lysine_count = 1;
            pf4.modified_mass = 1010;
            pf4.lysine_count = 1;
            pf5.modified_mass = 2020;
            pf5.lysine_count = 1;
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prList = ProteoformCommunity.relate(paE2, paT, ProteoformComparison.et);
            Assert.AreEqual(0, prList.Count);

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
            prList = ProteoformCommunity.relate(paE2, paT, ProteoformComparison.et);
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
            prList = ProteoformCommunity.relate(paE2, paT, ProteoformComparison.et);
            Assert.AreEqual(0, prList.Count);

        }

        [Test]
        public void TestUnabeledProteoformCommunityRelate_ET()
        {
            Lollipop.neucode_labeled = false;

            // Two proteoforms; mass difference < 500 -- return 1
            Proteoform pf1 = new Proteoform("A1", 1000.0, -1, true);
            Proteoform pf2 = new Proteoform("T1", 1010.0, 1, true);
            Proteoform[] paE = new Proteoform[1];
            Proteoform[] paT = new Proteoform[1];
            paE[0] = pf1;
            paT[0] = pf2;
            List<ProteoformRelation> prList = new List<ProteoformRelation>();
            prList = ProteoformCommunity.relate(paE, paT, ProteoformComparison.et);
            Assert.AreEqual(1, prList.Count);

            // Two proteoforms; mass difference > 500 -- return 0
            pf1.modified_mass = 1000;
            pf2.modified_mass = 2000;
            paE[0] = pf1;
            paT[0] = pf2;
            prList = ProteoformCommunity.relate(paE, paT, ProteoformComparison.et);
            Assert.AreEqual(0, prList.Count);

            //Two experimental one theoretical proteoforms; mass difference < 500 Da -- return 2
            Proteoform pf3 = new Proteoform("A1", 1000.0, -1, true);
            Proteoform pf4 = new Proteoform("A2", 1010.0, -1, true);
            Proteoform pf5 = new Proteoform("T1", 1020.0, 1, true);
            Proteoform[] paE2 = new Proteoform[2];
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prList = ProteoformCommunity.relate(paE2, paT, ProteoformComparison.et);
            Assert.AreEqual(2, prList.Count);

            //Three proteoforms; one mass difference < 500 Da -- return 0
            pf3.modified_mass = 1000;
            pf4.modified_mass = 1010;
            pf5.modified_mass = 2000;
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prList = ProteoformCommunity.relate(paE2, paT, ProteoformComparison.et);
            Assert.AreEqual(0, prList.Count);

            //Three proteoforms; mass difference > 500 Da -- return 0
            pf3.modified_mass = 1000;
            pf4.modified_mass = 2000;
            pf5.modified_mass = 3000;
            paE2[0] = pf3;
            paE2[1] = pf4;
            paT[0] = pf5;
            prList = ProteoformCommunity.relate(paE2, paT, ProteoformComparison.et);
            Assert.AreEqual(0, prList.Count);
        }

    }
}
