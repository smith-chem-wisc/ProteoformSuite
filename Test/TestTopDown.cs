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
            Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits);

            int count = Sweet.lollipop.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //both topdown hits should aggregate to a single proteoform
            Assert.AreEqual(11, Sweet.lollipop.topdown_proteoforms[0].topdown_hits[0].score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(2, Sweet.lollipop.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.

            //Test no aggregation outside retention time range
            tdhList[1].ms2_retention_time += Convert.ToDouble(Sweet.lollipop.retention_time_tolerance + 1);
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits);
            Assert.AreEqual(2, Sweet.lollipop.topdown_proteoforms.Count());

            //Test no aggregation different pfr
            tdhList[1].ms2_retention_time = tdhList[0].ms2_retention_time;
            tdhList[1].pfr = "12346";
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits);
            Assert.AreEqual(2, Sweet.lollipop.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform

            //if hit score below threshold, don't aggregate
            tdhList[1].pfr = "12345";
            tdhList[1].score = 1;
            Sweet.lollipop.min_score_td = 3;
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits);
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms[0].topdown_hits.Count());

            //if hit mass error too large  dont aggregate
            tdhList[1].score = 3;
            tdhList[1].reported_mass = 100;
            tdhList[1].theoretical_mass = 102.1;
            Sweet.lollipop.max_mass_error = .015;
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits);
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms[0].topdown_hits.Count());
        }

        [Test]
        public void TestDoubleAverage()
        {
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
            Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits);

            int count = Sweet.lollipop.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //all topdown hits should aggregate to a single proteoform
            Assert.AreEqual(19, Sweet.lollipop.topdown_proteoforms[0].topdown_hits[0].score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(10, Sweet.lollipop.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.
            Assert.IsTrue(Sweet.lollipop.topdown_proteoforms[0].agg_rt < 52);
        }

        [Test]
        public void TestTopdownReader()
        {
            Sweet.lollipop.clear_td();
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
            Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits);
            Assert.AreEqual(3, Sweet.lollipop.topdown_proteoforms.Count());
        }

        [Test]
        public void TestRelateTD()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.maximum_missed_monos = 1;
            // Two proteoforms; lysine count equal; mass difference < 250 -- return 1
            Component c1 = new Component();
            c1.weighted_monoisotopic_mass = 1000.0;
            c1.rt_apex = 45;
            c1.accepted = true;
            c1.id = 1.ToString();
            c1.intensity_sum = 1e6;
            c1.input_file = new InputFile("path", Purpose.Identification);
            c1.charge_states = new List<ChargeState>() { new ChargeState(1, c1.intensity_sum, c1.weighted_monoisotopic_mass) };
            Component c2 = new Component();
            c2.weighted_monoisotopic_mass = 1000.0;
            c2.rt_apex = 85;
            c2.accepted = true;
            c2.input_file = new InputFile("path", Purpose.Identification);
            c2.intensity_sum = 1e6;
            c2.charge_states = new List<ChargeState>() { new ChargeState(1, c2.intensity_sum, c2.weighted_monoisotopic_mass) };
            c2.id = 2.ToString();
            Component c3 = new Component();
            c3.weighted_monoisotopic_mass = 1131.04;
            c3.rt_apex = 45;
            c3.accepted = true;
            c3.input_file = new InputFile("path", Purpose.Identification);
            c3.intensity_sum = 1e6;
            c3.charge_states = new List<ChargeState>() { new ChargeState(1, c3.intensity_sum, c3.weighted_monoisotopic_mass) };
            c3.id = 3.ToString();
            Component c4 = new Component();
            c4.weighted_monoisotopic_mass = 2000.00;
            c4.rt_apex = 45;
            c4.accepted = true;
            c4.input_file = new InputFile("path", Purpose.Identification);
            c4.intensity_sum = 1e6;
            c4.charge_states = new List<ChargeState>() { new ChargeState(1, c4.intensity_sum, c4.weighted_monoisotopic_mass) };
            c4.id = 4.ToString();
            Component c5 = new Component();
            c5.weighted_monoisotopic_mass = 1001.0;
            c5.rt_apex = 45;
            c5.accepted = true;
            c5.input_file = new InputFile("path", Purpose.Identification);
            c5.intensity_sum = 1e6;
            c5.charge_states = new List<ChargeState>() { new ChargeState(1, c5.intensity_sum, c5.weighted_monoisotopic_mass) };
            c5.id = 2.ToString();
            List<IAggregatable> components = new List<IAggregatable>() { c1, c2, c3, c4, c5};

            Sweet.lollipop.raw_experimental_components = components.OfType<Component>().ToList();

            TopDownProteoform td1 = ConstructorsForTesting.TopDownProteoform("ACCESSION_1", 1000.0, 45);
            TopDownProteoform td2 = ConstructorsForTesting.TopDownProteoform("ACCESSION_2", 1001.0, 85);
            TopDownProteoform td3 = ConstructorsForTesting.TopDownProteoform("ACCESSION_3", 1131.04, 45);

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("ACCESSION", 1000.0, 1);

            //need to make theoretical accession database
            TestProteoformCommunityRelate.prepare_for_et(new List<double>() { 0 });
            Sweet.lollipop.target_proteoform_community.community_number = -100;
            Sweet.lollipop.theoretical_database.theoreticals_by_accession = new Dictionary<int, Dictionary<string, List<TheoreticalProteoform>>>();
            Sweet.lollipop.theoretical_database.theoreticals_by_accession.Add(-100, new Dictionary<string, List<TheoreticalProteoform>>());
            Sweet.lollipop.theoretical_database.theoreticals_by_accession[-100].Add(t1.accession, new List<TheoreticalProteoform>() { t1 });

            //need to make decon error top "deconvolution error"
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("S", out motif);
            ModificationWithMass m = new ModificationWithMass("", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, -1.0023, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Deconvolution Error");
            Sweet.lollipop.theoretical_database.all_mods_with_mass.Add(m);
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(-1, m) });
            Sweet.lollipop.theoretical_database.all_possible_ptmsets.Add(set);
            Sweet.lollipop.modification_ranks.Add(-1.0023, 2);
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.Add(-1.0, new List<PtmSet>() { set });

            //need missing error
            ModificationWithMass m2 = new ModificationWithMass("", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, -87.03, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Missing");
            Sweet.lollipop.theoretical_database.all_mods_with_mass.Add(m2);
            PtmSet set2 = new PtmSet(new List<Ptm> { new Ptm(-1, m2) });
            Sweet.lollipop.theoretical_database.all_possible_ptmsets.Add(set2);
            Sweet.lollipop.modification_ranks.Add(-87.03, 2);
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.Add(-87.0, new List<PtmSet>() { set2 });

            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new List<TheoreticalProteoform>() { t1 }.ToArray();
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform> { td1, td2, td3 };
            Sweet.lollipop.aggregate_proteoforms(Sweet.lollipop.validate_proteoforms, Sweet.lollipop.raw_neucode_pairs, Sweet.lollipop.raw_experimental_components, Sweet.lollipop.raw_quantification_components, 0);
            List<ProteoformRelation> relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, true, Environment.CurrentDirectory, true);
            List<DeltaMassPeak> peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(relations, new Dictionary<string, List<ProteoformRelation>>());
            //should have 4 experimental proteoforms -- 3 topdown, 1 not topdown experimental
            Assert.AreEqual(3, Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.topdown_id));
            Assert.AreEqual(4, Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count());
            Assert.AreEqual(1, relations.Count);
            Assert.AreEqual(1, relations.Count(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical && (r.connected_proteoforms[0] as ExperimentalProteoform).topdown_id));
            Assert.AreEqual(1, td1.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical && (r.connected_proteoforms[0] as ExperimentalProteoform).topdown_id));
            Assert.AreEqual(0, td2.relationships.Count());
            Assert.AreEqual(0, td3.relationships.Count());


            //accession 3 has higher score... gets td
            Sweet.lollipop.clear_td();
             td3 = ConstructorsForTesting.TopDownProteoform("ACCESSION_3", 1000.0, 45);
            TopDownHit h3 = new TopDownHit();
            h3.score = 100;
            td3.topdown_hits = new List<TopDownHit>() { h3 };
            TopDownHit h4 = new TopDownHit();
            h4.score = 1;
            TopDownProteoform td4 = ConstructorsForTesting.TopDownProteoform("ACCESSION_4", 1001.0, 45);
            td4.topdown_hits = new List<TopDownHit>() { h4 }; 
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform> { td4, td3 };
            Sweet.lollipop.topdown_proteoforms.OrderBy(p => p.modified_mass);
            Sweet.lollipop.aggregate_proteoforms(Sweet.lollipop.validate_proteoforms, Sweet.lollipop.raw_neucode_pairs, Sweet.lollipop.raw_experimental_components, Sweet.lollipop.raw_quantification_components, 0);
            Assert.AreEqual(0, Math.Round(td3.modified_mass - td3.matching_experimental.modified_mass, 0));
            Assert.IsNull(td4.matching_experimental);
        }
    }
}
