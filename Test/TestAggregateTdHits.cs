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
            SaveState.lollipop.top_down_hits = tdhList;
            SaveState.lollipop.target_proteoform_community.topdown_proteoforms = SaveState.lollipop.AggregateTdHits(SaveState.lollipop.top_down_hits).ToArray();

            int count = SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //both topdown hits should aggregate to a single proteoform
            Assert.AreEqual(11, SaveState.lollipop.target_proteoform_community.topdown_proteoforms[0].root.score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(2, SaveState.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.

            //Test no aggregation outside retention time range
            tdhList[1].retention_time += Convert.ToDouble(SaveState.lollipop.retention_time_tolerance + 1);
            SaveState.lollipop.top_down_hits = tdhList;
            SaveState.lollipop.target_proteoform_community.topdown_proteoforms = SaveState.lollipop.AggregateTdHits(SaveState.lollipop.top_down_hits).ToArray();

            Assert.AreEqual(2, SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform


            //Test no aggregation different sequence
            tdhList[1].retention_time = 10d;
            tdhList[1].sequence = "differentSequence";
            SaveState.lollipop.top_down_hits = tdhList;
            SaveState.lollipop.AggregateTdHits(SaveState.lollipop.top_down_hits);

            Assert.AreEqual(2, SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform

            //Test no aggregation different accession
            tdhList[1].sequence = "sequence";
            tdhList[1].accession = "differentAccession";
            SaveState.lollipop.top_down_hits = tdhList;
            SaveState.lollipop.AggregateTdHits(SaveState.lollipop.top_down_hits);

            Assert.AreEqual(2, SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform

            //Test no aggregation different ptms
            tdhList[1].accession = "accession";
            tdhList[1].ptm_list.Add(new Ptm()); 
            SaveState.lollipop.top_down_hits = tdhList;
            SaveState.lollipop.AggregateTdHits(SaveState.lollipop.top_down_hits);

            Assert.AreEqual(2, SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform
        }

        [Test]
        public void TestDoubleAverage()
        {
            SaveState.lollipop.min_RT_td = 0;
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

            SaveState.lollipop.top_down_hits = tdhList;
            SaveState.lollipop.target_proteoform_community.topdown_proteoforms = SaveState.lollipop.AggregateTdHits(SaveState.lollipop.top_down_hits).ToArray();

            int count = SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //all topdown hits should aggregate to a single proteoform
            Assert.AreEqual(19, SaveState.lollipop.target_proteoform_community.topdown_proteoforms[0].root.score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(10, SaveState.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.
            Assert.IsTrue(SaveState.lollipop.target_proteoform_community.topdown_proteoforms[0].agg_RT < 52);
        }
    }
}
