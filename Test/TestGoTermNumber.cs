using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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

            ProteoformSuiteInternal.GoTerm g = new ProteoformSuiteInternal.GoTerm("1", "one", ProteoformSuiteInternal.Aspect.biologicalProcess);
            ProteoformSuiteInternal.GoTermNumber gtn = new ProteoformSuiteInternal.GoTermNumber(g, q, k, m, t);
            Assert.AreEqual(gtn.log_odds_ratio, 0);
            Assert.AreEqual(Math.Round((decimal)gtn.p_value,6), 0.833333);

            q = 1;//number of enriched proteins with the term
            k = 2;//number of enriched proteins
            m = 4;//number of proteins in the background with the term
            t = 4;//number of proteins in the background

            gtn = new ProteoformSuiteInternal.GoTermNumber(g, q, k, m, t);
            Assert.AreEqual(gtn.log_odds_ratio, -1);
            Assert.AreEqual(Math.Round((decimal)gtn.p_value, 6), 1);

            q = 2;//number of enriched proteins with the term
            k = 2;//number of enriched proteins
            m = 2;//number of proteins in the background with the term
            t = 4;//number of proteins in the background

            gtn = new ProteoformSuiteInternal.GoTermNumber(g, q, k, m, t);
            Assert.AreEqual(gtn.log_odds_ratio, 1);
            Assert.AreEqual(Math.Round((decimal)gtn.p_value, 6), 0.166667);
        }

        [Test]
        public void testBinomialCoefficient()
        {
            ProteoformSuiteInternal.GoTermNumber gtn = new ProteoformSuiteInternal.GoTermNumber();
            int n = 10;
            int k = 2;
            Assert.AreEqual(45, (int)gtn.binomialCoefficient(n, k));
        }

        [Test]
        public void testGoTerm_pValue()
        {
            ProteoformSuiteInternal.GoTermNumber gtn = new ProteoformSuiteInternal.GoTermNumber();
            int q = 2; //count of proteins in selected subset with the particular Go term
            int k = 4; //count of proteins in selected subset
            int m = 4; //count of proteins in background with the particular Go term
            int t = 10; //count of proteins in background

            Assert.AreEqual(0.54762, Math.Round((double)gtn.GoTerm_pValue(q, k, m, t), 5));
        }
    }
}
