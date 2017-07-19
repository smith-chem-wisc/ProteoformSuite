using NUnit.Framework; 
using System;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using Proteomics;

namespace Test
{
    [TestFixture]
    class TestTopDown
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

        [Test]
        public void TestRelateTD()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.maximum_missed_monos = 1;
            // Two proteoforms; lysine count equal; mass difference < 250 -- return 1
            ExperimentalProteoform pf1 = ConstructorsForTesting.ExperimentalProteoform("A1", 1000.0, 1, true);
            pf1.agg_rt = 45;
            ExperimentalProteoform pf2 = ConstructorsForTesting.ExperimentalProteoform("A2", 1000.0, 1, true);
            pf2.agg_rt = 85;
            ExperimentalProteoform pf3 = ConstructorsForTesting.ExperimentalProteoform("A3", 1131.04, 1, true);
            TopDownProteoform td1 = ConstructorsForTesting.TopDownProteoform("ACCESSION_1", 1000.0, 45);
            TopDownProteoform td2 = ConstructorsForTesting.TopDownProteoform("ACCESSION_2", 1001.0, 85);

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("ACCESSION", 1087.03, 1); //td1 with methionine retention
            //td2 doesn't have matching theoretical... will make one

            //need to make theoretical accession database
            TestProteoformCommunityRelate.prepare_for_et(new List<double>() { 0 });
            Sweet.lollipop.target_proteoform_community.community_number = -100;
            Sweet.lollipop.theoretical_database.theoreticals_by_accession = new Dictionary<int, Dictionary<string, List<TheoreticalProteoform>>>();
            Sweet.lollipop.theoretical_database.theoreticals_by_accession.Add(-100, new Dictionary<string, List<TheoreticalProteoform>>());
            Sweet.lollipop.theoretical_database.theoreticals_by_accession[-100].Add(t1.accession, new List<TheoreticalProteoform>() { t1 });

            //need to make decon error top "deconvolution error"
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("S", out motif);
            ModificationWithMass m = new ModificationWithMass("", new Tuple<string, string>("", ""), motif, ModificationSites.K, -1.0023, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Deconvolution Error");
            Sweet.lollipop.theoretical_database.all_mods_with_mass.Add(m);
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(-1, m) });
            Sweet.lollipop.theoretical_database.all_possible_ptmsets.Add(set);
            Sweet.lollipop.modification_ranks.Add(-1.0023, 2);
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.Add(-1.0, new List<PtmSet>() { set });

            //need missing error...
            ModificationWithMass m2 = new ModificationWithMass("", new Tuple<string, string>("", ""), motif, ModificationSites.K, -87.03, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Missing");
            Sweet.lollipop.theoretical_database.all_mods_with_mass.Add(m2);
            PtmSet set2 = new PtmSet(new List<Ptm> { new Ptm(-1, m2) });
            Sweet.lollipop.theoretical_database.all_possible_ptmsets.Add(set2);
            Sweet.lollipop.modification_ranks.Add(-87.03, 2);
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.Add(-87.0, new List<PtmSet>() { set2 });



            Sweet.lollipop.target_proteoform_community.experimental_proteoforms = new List<ExperimentalProteoform>() { pf1, pf2, pf3 }.ToArray();
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new List<TheoreticalProteoform>() { t1 }.ToArray();
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = new List<TopDownProteoform> { td1, td2 }.ToArray();
            List<ProteoformRelation> td_relations =  Sweet.lollipop.target_proteoform_community.relate_td( );

            //should be 4 relations
            Assert.AreEqual(4, td_relations.Count);
            //2 etd
            Assert.AreEqual(2, td_relations.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown));
            //2 ttd
            Assert.AreEqual(2, td_relations.Count(r => r.RelationType == ProteoformComparison.TheoreticalTopDown));
            //added a new theoretical because couldn't form relation with one of the topdowns.
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length);
        }
    }
}
