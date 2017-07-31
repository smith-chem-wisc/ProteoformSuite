using NUnit.Framework; 
using System;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using Proteomics;
using System.IO;

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
                t.ms2_retention_time = 50d;
                t.ptm_list = new List<Ptm>();
                t.pfr = "12345";
                t.sequence = "SEQUENCE";
                t.tdResultType = TopDownResultType.TightAbsoluteMass;
                tdhList.Add(t);
                t.pvalue = 1 / (i + 1);
            }
            Sweet.lollipop.clear_td();
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();

            int count = Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //both topdown hits should aggregate to a single proteoform
            Assert.AreEqual(11, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].root.score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.

            //Test no aggregation outside retention time range
            tdhList[1].ms2_retention_time += Convert.ToDouble(Sweet.lollipop.retention_time_tolerance + 1);
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count());

            //Test no aggregation different pfr
            tdhList[1].ms2_retention_time = tdhList[0].ms2_retention_time;
            tdhList[1].pfr = "12346";
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform

            //if hit score below threshold, don't aggregate
            tdhList[1].pfr = "12345";
            tdhList[1].score = 1;
            Sweet.lollipop.min_score_td = 3;
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count());
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());

            //if hit rt above threshold dont aggregate
            tdhList[1].score = 3;
            tdhList[1].ms2_retention_time = 100;
            Sweet.lollipop.max_RT_td = 90;
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count());
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());

            //if hit rt below threshold dont aggregate
            tdhList[1].score = 3;
            tdhList[1].ms2_retention_time = 10;
            Sweet.lollipop.min_RT_td = 20;
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count());
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());

            //if hits have same scan # and pvalue, should choose higher scoring one only, remove other one.
            tdhList[1].ms2_retention_time = tdhList[0].ms2_retention_time;
            tdhList[1].ms2ScanNumber = 100;
            tdhList[0].ms2ScanNumber = 100;
            tdhList[1].pvalue = .01;
            tdhList[0].pvalue = 0.01;
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count());
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());
            Assert.AreEqual(10, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits[0].score);
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
                t.ms2_retention_time = 50;
                t.accession = "accession";
                t.sequence = "sequence";
                t.pvalue = (double) 1 / (i + 1);
                t.ptm_list = new List<Ptm>();
                t.tdResultType = TopDownResultType.TightAbsoluteMass;
                tdhList.Add(t);
            }

            tdhList[9].ms2_retention_time = 52;

            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits).ToArray();

            int count = Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //all topdown hits should aggregate to a single proteoform
            Assert.AreEqual(19, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].root.score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(10, Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.
            Assert.IsTrue(Sweet.lollipop.target_proteoform_community.topdown_proteoforms[0].agg_RT < 52);
        }

        [Test]
        public void TestTopdownReader()
        {
            Sweet.lollipop.clear_td();
            Sweet.lollipop.max_RT_td = 120; 
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(4, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(2, Sweet.lollipop.topdownReader.topdown_ptms.Count);
            Assert.AreEqual(2, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(10892.196, Math.Round(Sweet.lollipop.top_down_hits.OrderByDescending(h => h.ptm_list.Count).First().theoretical_mass, 3));
            List<TopDownProteoform> topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits);
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = topdown_proteoforms.Where(p => p != null).ToArray();
            foreach (ProteoformCommunity community in Sweet.lollipop.decoy_proteoform_communities.Values)
            {
                community.topdown_proteoforms = Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Select(e => new TopDownProteoform(e)).ToArray();
            }
            Assert.AreEqual(3, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count());
            Assert.AreEqual(3, Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].topdown_proteoforms.Count());
            //made new td proteoform for decoy should not be same object
            Assert.IsFalse(Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Any(t => Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + "0"].topdown_proteoforms.Contains(t)));
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
            ExperimentalProteoform pf3  = ConstructorsForTesting.ExperimentalProteoform("A3", 1131.04, 1, true);
            pf3.agg_rt = 45;
            TopDownProteoform td1 = ConstructorsForTesting.TopDownProteoform("ACCESSION_1", 1000.0, 45);
            TopDownProteoform td2 = ConstructorsForTesting.TopDownProteoform("ACCESSION_2", 1001.0, 85);
            TopDownProteoform td3 = ConstructorsForTesting.TopDownProteoform("ACCESSION_3", 1131.04, 85);

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("ACCESSION", 1087.03, 1);
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

            //need missing error
            ModificationWithMass m2 = new ModificationWithMass("", new Tuple<string, string>("", ""), motif, ModificationSites.K, -87.03, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Missing");
            Sweet.lollipop.theoretical_database.all_mods_with_mass.Add(m2);
            PtmSet set2 = new PtmSet(new List<Ptm> { new Ptm(-1, m2) });
            Sweet.lollipop.theoretical_database.all_possible_ptmsets.Add(set2);
            Sweet.lollipop.modification_ranks.Add(-87.03, 2);
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.Add(-87.0, new List<PtmSet>() { set2 });

            Sweet.lollipop.target_proteoform_community.experimental_proteoforms = new List<ExperimentalProteoform>() { pf1, pf2, pf3 }.ToArray();
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new List<TheoreticalProteoform>() { t1 }.ToArray();
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = new List<TopDownProteoform> { td1, td2, td3 }.ToArray();
            List<ProteoformRelation> td_relations =  Sweet.lollipop.target_proteoform_community.relate_td( );

            //should be 6 relations
            Assert.AreEqual(5, td_relations.Count);

            //td3 shouldnt have any etd relations (wrong rt)
            Assert.AreEqual(0, td3.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental));

            //2 etd
            Assert.AreEqual(2, td_relations.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental));
            Assert.AreEqual(0, pf3.relationships.Count);
            Assert.AreEqual(1, pf1.relationships.Count);
            Assert.AreEqual(1, pf2.relationships.Count);
            Assert.AreEqual(-1, Math.Round(pf2.relationships.First().DeltaMass, 0));
            //3 ttd
            Assert.AreEqual(3, td_relations.Count(r => r.RelationType == ProteoformComparison.TopdownTheoretical));
            Assert.AreEqual(1, td1.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownTheoretical));
            Assert.AreEqual(1, td2.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownTheoretical));
            //added 2 new theoreticals because couldn't form relation with one of the topdowns.
            Assert.AreEqual(3, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length);
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Count(t => t.topdown_theoretical));
            //if do ET, should only for relations with non topdown theoreticals
            Sweet.lollipop.target_proteoform_community.experimental_proteoforms.ToList().Add(ConstructorsForTesting.ExperimentalProteoform("E4", 1087.03, 1, true));
            Sweet.lollipop.et_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, true, TestContext.CurrentContext.TestDirectory, true);
            Assert.AreEqual(2, Sweet.lollipop.et_relations.Count);
            Assert.AreEqual(0, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(t => t.topdown_theoretical).Sum(t => t.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical)));
            //be sure some are 0 distance mass from an experimental -- didn't form relation due to being topdown theoretical. 
            Assert.IsTrue(Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Any(t => t.topdown_theoretical && Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Any(e => e.modified_mass == t.modified_mass)));

            //if experimental is unaccepted, do not make td relations
            Sweet.lollipop.clear_td();
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = new List<TopDownProteoform> { td1, td2, td3 }.ToArray();
            pf1.accepted = false;
            td_relations = Sweet.lollipop.target_proteoform_community.relate_td();
            //1 etd
            Assert.AreEqual(0, pf1.relationships.Count);
            Assert.AreEqual(1, td_relations.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental));
            Assert.AreEqual(0, pf3.relationships.Count);
            Assert.AreEqual(1, pf2.relationships.Count);


            //experimental w/ multiple topdowns should relate to the one closest to 0, not with MM errors, regardless of order td relations made in
            Sweet.lollipop.clear_td();
            pf1.accepted = true;
             td3 = ConstructorsForTesting.TopDownProteoform("ACCESSION_3", 1000.0, 45);
            TopDownProteoform td4 = ConstructorsForTesting.TopDownProteoform("ACCESSION_4", 1001.0, 45);
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = new List<TopDownProteoform> { td3, td4 }.ToArray();
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms.OrderBy(p => p.modified_mass);
            td_relations = Sweet.lollipop.target_proteoform_community.relate_td();

            Assert.AreEqual(1, pf1.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental));
            Assert.AreEqual(1, td3.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental));
            Assert.AreEqual(0, td4.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental));
            Assert.AreEqual(3, td_relations.Count);

            //try in other order of topdown (other relation added first, needs to be removed)
            Sweet.lollipop.clear_td();
             td3 = ConstructorsForTesting.TopDownProteoform("ACCESSION_3", 1000.0, 45);
             td4 = ConstructorsForTesting.TopDownProteoform("ACCESSION_4", 1001.0, 45);
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms = new List<TopDownProteoform> { td3, td4 }.ToArray();
            Sweet.lollipop.target_proteoform_community.topdown_proteoforms.OrderByDescending(p => p.modified_mass);
            td_relations = Sweet.lollipop.target_proteoform_community.relate_td();
            Assert.AreEqual(1, pf1.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental));
            Assert.AreEqual(1, td3.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental));
            Assert.AreEqual(0, td4.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental));
            Assert.AreEqual(3, td_relations.Count);
        }
    }
}
