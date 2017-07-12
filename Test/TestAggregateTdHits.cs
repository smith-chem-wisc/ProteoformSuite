using NUnit.Framework; 
using System;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [TestFixture]
    class TestAggregateTdHits
    {
        

        [Test]
        public void TestSimpleAggregation()
        {
            List<TopDownHit> tdhList = new List<TopDownHit>();
            for (int i = 0; i < 2; i++)
            {
                TopDownHit t = new TopDownHit();
                t.score = (10 + Convert.ToDouble(i));
                t.retention_time = 50d;
                t.accession = "accession";
                t.sequence = "sequence";
                t.ptm_list = new List<Ptm>();
                t.tdResultType = TopDownResultType.TightAbsoluteMass;
                tdhList.Add(t);
                t.pvalue = 1 / (i + 1);
            }
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();

            int count = Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //both topdown hits should aggregate to a single proteoform
            Assert.AreEqual(11, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].root.score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.

            //Test no aggregation outside retention time range
            tdhList[1].retention_time += Convert.ToDouble(Sweet.lollipop.retention_time_tolerance + 1);
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();

            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform


            //Test no aggregation different sequence
            tdhList[1].retention_time = 10d;
            tdhList[1].sequence = "differentSequence";
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits);

            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform

            //Test no aggregation different accession
            tdhList[1].sequence = "sequence";
            tdhList[1].accession = "differentAccession";
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits);

            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform

            //Test no aggregation different ptms
            tdhList[1].accession = "accession";
            tdhList[1].ptm_list.Add(new Ptm()); 
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits);

            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform
        }

        [Test]
        public void TestDoubleAverage()
        {
            Sweet.lollipop.min_RT_td = 0;
            List<TopDownHit> tdhList = new List<TopDownHit>();
            for (int i = 0; i < 10; i++)
            {
                TopDownHit t = new TopDownHit();
                t.score = (10 + Convert.ToDouble(i));
                t.retention_time = 50;
                t.accession = "accession";
                t.sequence = "sequence";
                t.pvalue = (double) 1 / (i + 1);
                t.ptm_list = new List<Ptm>();
                t.tdResultType = TopDownResultType.TightAbsoluteMass;
                tdhList.Add(t);
            }

            tdhList[9].retention_time = 52;

            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();

            int count = Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //all topdown hits should aggregate to a single proteoform
            Assert.AreEqual(19, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].root.score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(10, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.
            Assert.IsTrue(Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].agg_RT < 52);
        }
    }
}
