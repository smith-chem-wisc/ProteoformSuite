using NUnit.Framework;
using System;
using Proteomics;
using ProteoformSuiteInternal;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    class TestGoTermNumber
    {
        [Test]
        public void testLogOddsRatio()
        {
            int q = 1;//number of enriched proteins with the term
            int k = 2;//number of enriched proteins
            int m = 2;//number of proteins in the background with the term
            int t = 4;//number of proteins in the background

            DatabaseReference d = new DatabaseReference("GO", ":1", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:one") });
            GoTerm g = new GoTerm(d);
            GoTermNumber gtn = new GoTermNumber(g, q, k, m, t);
            Assert.AreEqual(gtn.log_odds_ratio, 0);
            Assert.AreEqual(Math.Round((decimal)gtn.p_value,6), 0.833333);

            q = 1;//number of enriched proteins with the term
            k = 2;//number of enriched proteins
            m = 4;//number of proteins in the background with the term
            t = 4;//number of proteins in the background

            gtn = new GoTermNumber(g, q, k, m, t);
            Assert.AreEqual(gtn.log_odds_ratio, -1);
            Assert.AreEqual(Math.Round((decimal)gtn.p_value, 6), 1);

            q = 2;//number of enriched proteins with the term
            k = 2;//number of enriched proteins
            m = 2;//number of proteins in the background with the term
            t = 4;//number of proteins in the background

            gtn = new GoTermNumber(g, q, k, m, t);
            Assert.AreEqual(gtn.log_odds_ratio, 1);
            Assert.AreEqual(Math.Round((decimal)gtn.p_value, 6), 0.166667);
        }

        [Test]
        public void testBinomialCoefficient()
        {
            GoTermNumber gtn = new GoTermNumber();
            int n = 10;
            int k = 2;
            Assert.AreEqual(45, (int)gtn.binomialCoefficient(n, k));
        }

        [Test]
        public void testGoTerm_pValue()
        {
            GoTermNumber gtn = new GoTermNumber();
            int q = 2; //count of proteins in selected subset with the particular Go term
            int k = 4; //count of proteins in selected subset
            int m = 4; //count of proteins in background with the particular Go term
            int t = 10; //count of proteins in background

            Assert.AreEqual(0.54762, Math.Round((double)gtn.GoTerm_pValue(q, k, m, t), 5));
        }
    }
}
