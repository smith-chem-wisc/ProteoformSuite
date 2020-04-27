using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using Proteomics;
using System.IO;
using MassSpectrometry;

namespace Test
{
    [TestFixture]
    class TestTopDown
    {

        [Test]
        public void TestTopDownMetaMorpheusCommandLine()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            string result = Sweet.lollipop.metamorpheus_topdown(TestContext.CurrentContext.TestDirectory, true, 10, 10,
                DissociationType.HCD);
            Assert.AreEqual("Successfully ran MetaMorpheus top-down search.", result);

            //set toml with new parameters
            string[] toml_params = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory + "\\MetaMorpheusDotNetFrameworkAppveyor\\TopDownSearchSettingsMetaMorpheus0.0.300.toml"));
            Assert.AreEqual("ListOfModsFixed = \"Common Fixed\tCarbamidomethyl on C\t\tCommon Fixed\tCarbamidomethyl on U\"", toml_params[42]);
            Assert.AreEqual("ProductMassTolerance = \"±10 PPM\"", toml_params[50]);
            Assert.AreEqual("PrecursorMassTolerance = \"±10 PPM\"", toml_params[51]);
            Assert.AreEqual("DissociationType = \"HCD\"", toml_params[66]);


            result = Sweet.lollipop.metamorpheus_topdown(TestContext.CurrentContext.TestDirectory, false, 5, 25,
                DissociationType.CID);
            Assert.AreEqual("Successfully ran MetaMorpheus top-down search.", result);

            //set toml with new parameters
            toml_params = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory + "\\MetaMorpheusDotNetFrameworkAppveyor\\TopDownSearchSettingsMetaMorpheus0.0.300.toml"));
            Assert.AreEqual("ListOfModsFixed = \"\"", toml_params[42]);
            Assert.AreEqual("ProductMassTolerance = \"±25 PPM\"", toml_params[50]);
            Assert.AreEqual("PrecursorMassTolerance = \"±5 PPM\"", toml_params[51]);
            Assert.AreEqual("DissociationType = \"CID\"", toml_params[66]);
        }


        [Test]
        public void TestSimpleAggregation()
        {
            List<SpectrumMatch> tdhList = new List<SpectrumMatch>();
            for (int i = 0; i < 2; i++)
            {
                SpectrumMatch t = new SpectrumMatch();
                t.accession = "td";
                t.score = (10 + Convert.ToDouble(i));
                t.ms2_retention_time = 50d;
                t.ptm_list = new List<Ptm>();
                t.sequence = "SEQUENCE";
                t.tdResultType = TopDownResultType.TightAbsoluteMass;
                tdhList.Add(t);
                t.qValue = 1 / (i + 1);
            }
            Sweet.lollipop.clear_td();
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);

            int count = Sweet.lollipop.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //both topdown hits should aggregate to a single proteoform
            Assert.AreEqual(11, Sweet.lollipop.topdown_proteoforms[0].topdown_hits[0].score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(2, Sweet.lollipop.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.

            //Test no aggregation outside retention time range
            tdhList[1].ms2_retention_time += Convert.ToDouble(Sweet.lollipop.td_retention_time_tolerance + 1);
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(2, Sweet.lollipop.topdown_proteoforms.Count());

            //Test no aggregation different pfr
            tdhList[1].ms2_retention_time = tdhList[0].ms2_retention_time;
            tdhList[1].full_sequence = "NEWSEQUENCE";
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(2, Sweet.lollipop.topdown_proteoforms.Count()); //both topdown hits should aggregate to a single proteoform

            //if hit score below threshold, don't aggregate
            tdhList[1].full_sequence = "SEQUENCE";
            tdhList[1].score = 1;
            Sweet.lollipop.min_score_td = 3;
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms[0].topdown_hits.Count());
        }

        [Test]
        public void TestDoubleAverage()
        {
            List<SpectrumMatch> tdhList = new List<SpectrumMatch>();
            for (int i = 0; i < 10; i++)
            {
                SpectrumMatch t = new SpectrumMatch();
                t.score = (10 + Convert.ToDouble(i));
                t.ms2_retention_time = 50;
                t.accession = "accession";
                t.sequence = "sequence";
                t.qValue = (double)1 / (i + 1);
                t.ptm_list = new List<Ptm>();
                t.tdResultType = TopDownResultType.TightAbsoluteMass;
                tdhList.Add(t);
            }

            tdhList[9].ms2_retention_time = 52;
            Sweet.lollipop.top_down_hits = tdhList;
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);

            int count = Sweet.lollipop.topdown_proteoforms.Count();
            Assert.AreEqual(1, count); //all topdown hits should aggregate to a single proteoform
            Assert.AreEqual(19, Sweet.lollipop.topdown_proteoforms[0].topdown_hits[0].score);  //higher scoring topdown hit should be root.
            Assert.AreEqual(10, Sweet.lollipop.topdown_proteoforms[0].topdown_hits.Count());  //higher scoring topdown hit should be root.
            Assert.IsTrue(Sweet.lollipop.topdown_proteoforms[0].agg_rt < 52);
        }

        [Test]
        public void TestTopdownReaderTDPortal()
        {
            // unlabeled
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(7, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(2, Sweet.lollipop.topdownReader.bad_ptms.Count);
            Assert.AreEqual("RESID:AA929292 at S", Sweet.lollipop.topdownReader.bad_ptms.OrderByDescending(p => p).First());
            Assert.AreEqual(6, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(10894.157, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.pfr_accession).First().theoretical_mass, 3));
            Assert.AreEqual(10894.130, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.pfr_accession).First().reported_mass, 3));
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(4, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(10894.157, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.pfr_accession).First().theoretical_mass, 3));
            Assert.AreEqual(10894.130, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.pfr_accession).First().agg_mass, 3));

            // neucode labeled
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = true;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(7, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(2, Sweet.lollipop.topdownReader.bad_ptms.Count);
            Assert.AreEqual("RESID:AA929292 at S", Sweet.lollipop.topdownReader.bad_ptms.OrderByDescending(p => p).First());
            Assert.AreEqual(6, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(10934.228, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.pfr_accession).First().theoretical_mass, 3));
            Assert.AreEqual(10934.201, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.pfr_accession).First().reported_mass, 3));
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(4, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(10934.228, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.pfr_accession).First().theoretical_mass, 3));
            Assert.AreEqual(10934.201, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.pfr_accession).First().agg_mass, 3));

            // carbamidomethylated
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = true;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(7, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(2, Sweet.lollipop.topdownReader.bad_ptms.Count);
            Assert.AreEqual("RESID:AA929292 at S", Sweet.lollipop.topdownReader.bad_ptms.OrderByDescending(p => p).First());
            Assert.AreEqual(6, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(10951.178, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.pfr_accession).First().theoretical_mass, 3));
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(4, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(10951.178, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.pfr_accession).First().theoretical_mass, 3));

            // carbamidomethylated and neucode labeled
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = true;
            Sweet.lollipop.carbamidomethylation = true;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(7, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(2, Sweet.lollipop.topdownReader.bad_ptms.Count);
            Assert.AreEqual("RESID:AA929292 at S", Sweet.lollipop.topdownReader.bad_ptms.OrderByDescending(p => p).First());
            Assert.AreEqual(6, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(10991.249, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.pfr_accession).First().theoretical_mass, 3));
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(4, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(10991.249, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.pfr_accession).First().theoretical_mass, 3));
        }

        [Test]
        public void TestUniProtMods()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[7], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "stripped.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
        }

        [Test]
        public void TestTopdownReaderMetaMorpheus()
        {
            // unlabeled
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "testHits.psmtsv") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(13, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(1, Sweet.lollipop.topdownReader.bad_ptms.Count);
            Assert.AreEqual("\"N6,N6-dimethyllysine on K N-acetylserine on S|N6,N6-dimethyllysine on K N-acetylserine on S|N6,N6-dimethyllysine on K N6-acetyllysine on K|N6,N6-dimethyllysine on K N6-acetyllysine on K\" at 08-02-17_B9_myoblast_A_fract3and4_td_rep1 scan 2395", Sweet.lollipop.topdownReader.bad_ptms.OrderByDescending(p => p).First());
            Assert.AreEqual(10, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(10969.845, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.name).First().theoretical_mass, 3));
            Assert.AreEqual(10969.856, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.name).First().reported_mass, 3));
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(12, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(10969.845, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.name).First().theoretical_mass, 3));
            Assert.AreEqual(10969.856, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.name).First().agg_mass, 3));

            // neucode labeled
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = true;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "testHits.psmtsv") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(13, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(1, Sweet.lollipop.topdownReader.bad_ptms.Count);
            Assert.AreEqual("\"N6,N6-dimethyllysine on K N-acetylserine on S|N6,N6-dimethyllysine on K N-acetylserine on S|N6,N6-dimethyllysine on K N6-acetyllysine on K|N6,N6-dimethyllysine on K N6-acetyllysine on K\" at 08-02-17_B9_myoblast_A_fract3and4_td_rep1 scan 2395", Sweet.lollipop.topdownReader.bad_ptms.OrderByDescending(p => p).First());
            Assert.AreEqual(10, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(11058.001, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.name).First().theoretical_mass, 3));
            Assert.AreEqual(11058.012, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.name).First().reported_mass, 3));
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(12, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(11058.001, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.name).First().theoretical_mass, 3));
            Assert.AreEqual(11058.012, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.name).First().agg_mass, 3));

            // carbamidomethylated is labeled in MM output - should have same result as without it 
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = true;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "testHits.psmtsv") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(13, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(1, Sweet.lollipop.topdownReader.bad_ptms.Count);
            Assert.AreEqual("\"N6,N6-dimethyllysine on K N-acetylserine on S|N6,N6-dimethyllysine on K N-acetylserine on S|N6,N6-dimethyllysine on K N6-acetyllysine on K|N6,N6-dimethyllysine on K N6-acetyllysine on K\" at 08-02-17_B9_myoblast_A_fract3and4_td_rep1 scan 2395", Sweet.lollipop.topdownReader.bad_ptms.OrderByDescending(p => p).First());
            Assert.AreEqual(10, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(10969.845, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.name).First().theoretical_mass, 3));
            Assert.AreEqual(10969.856, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.name).First().reported_mass, 3));
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(12, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(10969.845, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.name).First().theoretical_mass, 3));
            Assert.AreEqual(10969.856, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.name).First().agg_mass, 3));

            // carbamidomethylated and neucode labeled
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = true;
            Sweet.lollipop.carbamidomethylation = true;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "testHits.psmtsv") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(13, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(1, Sweet.lollipop.topdownReader.bad_ptms.Count);
            Assert.AreEqual("\"N6,N6-dimethyllysine on K N-acetylserine on S|N6,N6-dimethyllysine on K N-acetylserine on S|N6,N6-dimethyllysine on K N6-acetyllysine on K|N6,N6-dimethyllysine on K N6-acetyllysine on K\" at 08-02-17_B9_myoblast_A_fract3and4_td_rep1 scan 2395", Sweet.lollipop.topdownReader.bad_ptms.OrderByDescending(p => p).First());
            Assert.AreEqual(10, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(11058.001, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.name).First().theoretical_mass, 3));
            Assert.AreEqual(11058.012, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.name).First().reported_mass, 3));
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(12, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(11058.001, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.name).First().theoretical_mass, 3));
            Assert.AreEqual(11058.012, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.name).First().agg_mass, 3));
        }

        [Test]
        public void TestTopdownAmbiguity()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "testHits.psmtsv") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(4, Sweet.lollipop.top_down_hits.Count(h => h.ambiguous_matches.Count > 0));
            Assert.AreEqual(9, Sweet.lollipop.top_down_hits.Count(h => h.ambiguous_matches.Count == 0));
            Assert.AreEqual(4, Sweet.lollipop.topdown_proteoforms.Count(h => h.ambiguous_topdown_hits.Count > 0));
            Assert.AreEqual(8, Sweet.lollipop.topdown_proteoforms.Count(h => h.ambiguous_topdown_hits.Count == 0));
            var ambiguous_td = Sweet.lollipop.topdown_proteoforms.Where(p => p.ambiguous_topdown_hits.Count > 0).OrderBy(p => p.name).ToList();
            Assert.AreEqual(2, ambiguous_td[0].topdown_level);
            Assert.AreEqual("PTM localization ambiguity; ", ambiguous_td[0].topdown_level_description);
            Assert.AreEqual(2, ambiguous_td[1].topdown_level);
            Assert.AreEqual("PTM localization ambiguity; ", ambiguous_td[1].topdown_level_description);
            Assert.AreEqual(3, ambiguous_td[2].topdown_level);
            Assert.AreEqual("PTM identity ambiguity; PTM localization ambiguity; ", ambiguous_td[2].topdown_level_description);
            Assert.AreEqual("Unambiguous", Sweet.lollipop.topdown_proteoforms.Where(h => h.ambiguous_topdown_hits.Count() == 0).Select(p => p.topdown_level_description).Distinct().First());
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms.Where(h => h.ambiguous_topdown_hits.Count() == 0).Select(p => p.topdown_level).Distinct().First());
            Assert.AreEqual("Phospho@32, 115, 247; ", Sweet.lollipop.topdown_proteoforms.Where(p => p.accession.Contains("P0CX35") && p.ambiguous_topdown_hits.Count == 0).First().topdown_uniprot_mods);
            Assert.IsTrue(Sweet.lollipop.topdown_proteoforms.Where(p => p.accession.Contains("P0CX35") && p.ambiguous_topdown_hits.Count == 0).First().topdown_novel_mods);
            Assert.AreEqual("N/A | N/A", Sweet.lollipop.topdown_proteoforms.Where(p => p.accession.Contains("P0CX35") && p.ambiguous_topdown_hits.Count > 0).First().topdown_uniprot_mods);
            Assert.IsTrue(Sweet.lollipop.topdown_proteoforms.Where(p => p.accession.Contains("P0CX35") && p.ambiguous_topdown_hits.Count > 0).First().topdown_novel_mods);
        }

        [Test]
        public void TestTopDownAggregationAmbiguity()
        {
            Sweet.lollipop = new Lollipop();
            SpectrumMatch pf1 = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 10, 20);
            SpectrumMatch ambiguous_pf = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 10, 20);
            ambiguous_pf.ambiguous_matches = new List<SpectrumMatch>() { ConstructorsForTesting.SpectrumMatch("B", 1000, 10, 10, 20) };
            Sweet.lollipop.top_down_hits = new List<SpectrumMatch>() { pf1, ambiguous_pf };
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, 0, true, true);
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms.Count);
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms.First().topdown_hits.Count);
            Assert.AreEqual(0, Sweet.lollipop.topdown_proteoforms.First().ambiguous_topdown_hits.Count);
        }

        [Test]
        public void TestBottomUpPSMsWithTopDownProteoforms()
        {
            Sweet.lollipop = new Lollipop();
            SpectrumMatch ambiguous_pf = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 10, 20);
            ambiguous_pf.ambiguous_matches = new List<SpectrumMatch>() { ConstructorsForTesting.SpectrumMatch("B", 1000, 10, 10, 20) };
            SpectrumMatch bu1 = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 10, 17);
            SpectrumMatch bu2 = ConstructorsForTesting.SpectrumMatch("B", 1000, 10, 10, 17);
            SpectrumMatch bu3 = ConstructorsForTesting.SpectrumMatch("B", 1000, 10, 9, 17);
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Add("A", new List<SpectrumMatch>() { bu1 });
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Add("B", new List<SpectrumMatch>() { bu2, bu3 });
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(new List<SpectrumMatch>() { ambiguous_pf }, 0, true, true);
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms.First().topdown_bottom_up_PSMs.Count);
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms.First().ambiguous_topdown_hits.First().bottom_up_PSMs.Count);
        }

        [Test]
        public void TestDescription()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            var mod = Sweet.lollipop.theoretical_database.all_mods_with_mass.Where(m => m.OriginalId == "Acetylation").First();
            var description = TopDownProteoform.get_description(new List<SpectrumMatch>(), "A", true, new PtmSet(new List<Ptm>() { new Ptm(1, mod) }));
            //no peptides
            Assert.AreEqual("no BU PSMs", description);
            //no modified peptides
            var unmodified_peptide = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 2, 10);
            description = TopDownProteoform.get_description(new List<SpectrumMatch>() { unmodified_peptide }, "A", true, new PtmSet(new List<Ptm>() { new Ptm(1, mod) }));
            Assert.AreEqual("no modified BU PSMs", description);
            //same location PTM
            var modified = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 2, 10);
            modified.ptm_list.Add(new Ptm(1, mod));
            description = TopDownProteoform.get_description(new List<SpectrumMatch>() { modified }, "A", true, new PtmSet(new List<Ptm>() { new Ptm(1, mod) }));
            Assert.AreEqual("Same location PTM. ", description);
            //other td PTM
             modified = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 2, 10);
            modified.ptm_list.Add(new Ptm(2, mod));
            description = TopDownProteoform.get_description(new List<SpectrumMatch>() { modified }, "A", true, new PtmSet(new List<Ptm>() { new Ptm(1, mod) }));
            Assert.AreEqual("Different locations", description);
            //additional td PTM
            modified = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 2, 10);
            modified.ptm_list.Add(new Ptm(2, mod));
            description = TopDownProteoform.get_description(new List<SpectrumMatch>() { modified }, "A", true, new PtmSet(new List<Ptm>() { new Ptm(1, mod), new Ptm(2, mod) }));
            Assert.AreEqual("Same location PTM. Additional TD location. ", description);
            //additional BU ptm
            //additional td PTM
            modified = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 2, 10);
            modified.ptm_list.Add(new Ptm(2, mod));
            modified.ptm_list.Add(new Ptm(1, mod));
            description = TopDownProteoform.get_description(new List<SpectrumMatch>() { modified }, "A", true, new PtmSet(new List<Ptm>() { new Ptm(1, mod) }));
            Assert.AreEqual("Same location PTM. Additional BU locations. ", description);

            var other_mod = Sweet.lollipop.theoretical_database.all_mods_with_mass.Where(m => m.OriginalId == "Phosphorylation").First();
            modified = ConstructorsForTesting.SpectrumMatch("A", 1000, 10, 2, 10);
            modified.ptm_list.Add(new Ptm(2, other_mod));
            description = TopDownProteoform.get_description(new List<SpectrumMatch>() { modified }, "A", true, new PtmSet(new List<Ptm>() { new Ptm(1, mod) }));
            Assert.AreEqual("Other TD PTM. Other BU PTM. ", description);

            modified.ptm_list.Add(new Ptm(2, mod));
            description = TopDownProteoform.get_description(new List<SpectrumMatch>() { modified }, "A", true, new PtmSet(new List<Ptm>() { new Ptm(1, mod) }));
            Assert.AreEqual("Other BU PTM. Different locations", description);

            description = TopDownProteoform.get_description(new List<SpectrumMatch>() { modified }, "A", true, new PtmSet(new List<Ptm>()));
            Assert.AreEqual("Other BU PTM. ", description);
        }


        [Test]
        public void TestTopdownGeneAmbiguity()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "testHitsGenes.psmtsv") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(2, Sweet.lollipop.top_down_hits.Count(h => h.ambiguous_matches.Count > 0));
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Assert.AreEqual(1, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual("YPS1", Sweet.lollipop.topdown_proteoforms.OrderBy(p => p.pfr_accession).First().topdown_geneName.primary);
            Assert.AreEqual("MF(ALPHA)2", Sweet.lollipop.topdown_proteoforms.OrderBy(p => p.pfr_accession).First().ambiguous_topdown_hits[0].gene_name.primary);
        }

        [Test]
        public void TestGlycanMetaMorpheus()
        {
            // unlabeled
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "glyco_fdr_fullSeqWithMod.psmtsv") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Assert.AreEqual(1670, Sweet.lollipop.theoretical_database.glycan_mods.Count);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(6, Sweet.lollipop.top_down_hits.Count);
            Assert.AreEqual(68, Sweet.lollipop.top_down_hits.Sum(h => h.ptm_list.Count));
            Assert.AreEqual(3755.495, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.ms2ScanNumber).First().theoretical_mass, 3));
            Assert.AreEqual(3755.485, Math.Round(Sweet.lollipop.top_down_hits.OrderBy(h => h.ms2ScanNumber).First().reported_mass, 3));
            Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
            Assert.AreEqual(5, Sweet.lollipop.topdown_proteoforms.Count());
            Assert.AreEqual(3755.495, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.topdown_hits.First().ms2ScanNumber).First().theoretical_mass, 3));
            Assert.AreEqual(3755.485, Math.Round(Sweet.lollipop.topdown_proteoforms.OrderBy(h => h.topdown_hits.First().ms2ScanNumber).First().agg_mass, 3));

            //remove glycan from db... should be bad
            Sweet.lollipop.theoretical_database.uniprotModifications.Remove("H");
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(0, Sweet.lollipop.top_down_hits.Count);
        }

        [Test]
        public void BadTDFile()
        {

            // unlabeled
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.clear_td();
            InputFile file = new InputFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml"), Purpose.TopDown);
            Sweet.lollipop.input_files.Add(file);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_td_hits();
            Assert.AreEqual(1, Sweet.lollipop.input_files.Count(f => f.purpose == Purpose.TopDown));
            Assert.AreEqual(0, Sweet.lollipop.top_down_hits.Count);
        }

        [Test]
        public void TestRelateTD()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.maximum_missed_monos = 1;
            Sweet.lollipop.agg_minBiorepsWithObservations = 0;
            InputFile f = new InputFile("path", Purpose.Identification);
            Sweet.lollipop.input_files.Add(f);
            // Two proteoforms; lysine count equal; mass difference < 250 -- return 1
            Component c1 = new Component();
            c1.weighted_monoisotopic_mass = 1000.0;
            c1.rt_apex = 45;
            c1.accepted = true;
            c1.id = 1.ToString();
            c1.intensity_sum = 1e6;
            c1.input_file = f;
            c1.charge_states = new List<ChargeState>() { new ChargeState(1, c1.intensity_sum, c1.weighted_monoisotopic_mass) };
            Component c2 = new Component();
            c2.weighted_monoisotopic_mass = 1000.0;
            c2.rt_apex = 85;
            c2.accepted = true;
            c2.input_file = f;
            c2.intensity_sum = 1e6;
            c2.charge_states = new List<ChargeState>() { new ChargeState(1, c2.intensity_sum, c2.weighted_monoisotopic_mass) };
            c2.id = 2.ToString();
            Component c3 = new Component();
            c3.weighted_monoisotopic_mass = 1131.04;
            c3.rt_apex = 45;
            c3.accepted = true;
            c3.input_file = f;
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
            c5.input_file = f;
            c5.intensity_sum = 1e6;
            c5.charge_states = new List<ChargeState>() { new ChargeState(1, c5.intensity_sum, c5.weighted_monoisotopic_mass) };
            c5.id = 2.ToString();
            List<IAggregatable> components = new List<IAggregatable>() { c1, c2, c3, c4, c5 };
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
            Modification m = new Modification("id", _modificationType : "modtype", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass: 1);
            Sweet.lollipop.theoretical_database.all_mods_with_mass.Add(m);
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(-1, m) });
            Sweet.lollipop.theoretical_database.all_possible_ptmsets.Add(set);
            Sweet.lollipop.modification_ranks.Add(-1.0023, 2);
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.Add(-1.0, new List<PtmSet>() { set });

            //need missing error
            Modification m2 = new Modification("id", _modificationType : "modtype", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass: 1);
            Sweet.lollipop.theoretical_database.all_mods_with_mass.Add(m2);
            PtmSet set2 = new PtmSet(new List<Ptm> { new Ptm(-1, m2) });
            Sweet.lollipop.theoretical_database.all_possible_ptmsets.Add(set2);
            Sweet.lollipop.modification_ranks.Add(-87.03, 2);
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.Add(-87.0, new List<PtmSet>() { set2 });

            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = new List<TheoreticalProteoform>() { t1 }.ToArray();
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform> { td1, td2, td3 };
            Sweet.lollipop.add_td_proteoforms = true;
            Sweet.lollipop.aggregate_proteoforms(Sweet.lollipop.validate_proteoforms, Sweet.lollipop.raw_neucode_pairs, Sweet.lollipop.raw_experimental_components, Sweet.lollipop.raw_quantification_components, 0);
            List<ProteoformRelation> relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, Environment.CurrentDirectory, true);
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
            SpectrumMatch h3 = new SpectrumMatch();
            h3.score = 100;
            td3.topdown_hits = new List<SpectrumMatch>() { h3 };
            SpectrumMatch h4 = new SpectrumMatch();
            h4.score = 1;
            TopDownProteoform td4 = ConstructorsForTesting.TopDownProteoform("ACCESSION_4", 1001.0, 45);
            td4.topdown_hits = new List<SpectrumMatch>() { h4 };
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform> { td4, td3 };
            Sweet.lollipop.topdown_proteoforms.OrderBy(p => p.modified_mass);
            Sweet.lollipop.aggregate_proteoforms(Sweet.lollipop.validate_proteoforms, Sweet.lollipop.raw_neucode_pairs, Sweet.lollipop.raw_experimental_components, Sweet.lollipop.raw_quantification_components, 0);
            Assert.AreEqual(0, Math.Round(td3.modified_mass - td3.matching_experimental.modified_mass, 0));
            Assert.IsNull(td4.matching_experimental);
        }

        [Test]
        public void TestConvertMassToNeucode()
        {
            double unlabeled_mass = 1000;
            int kcount = 1;
            double neucode_mass = Sweet.lollipop.get_neucode_mass(unlabeled_mass, kcount);
            Assert.AreEqual(1008.01, Math.Round(neucode_mass, 2));
        }

        [Test]
        public void TestCopyTopDown()
        {
            SpectrumMatch hit = new SpectrumMatch();
            hit.name = "name";
            hit.uniprot_id = "id";
            hit.sequence = "sequence";
            hit.begin = 0;
            hit.end = 20;
            hit.theoretical_mass = 1000;
            hit.reported_mass = 1000;
            hit.ms2_retention_time = 10;
            TopDownProteoform td1 = new TopDownProteoform("accession", new List<SpectrumMatch>() { hit });
            TopDownProteoform td2 = new TopDownProteoform(td1);
            Assert.AreNotEqual(td1, td2);
            Assert.AreEqual(td1.root, td2.root);
            Assert.AreEqual(td1.name, td2.name);
            Assert.AreEqual(td1.ptm_set.ptm_combination.Count, td2.ptm_set.ptm_combination.Count);
            Assert.AreEqual(td1.uniprot_id, td2.uniprot_id);
            Assert.AreEqual(td1.sequence, td2.sequence);
            Assert.AreEqual(td1.begin, td2.begin);
            Assert.AreEqual(td1.end, td2.end);
            Assert.AreEqual(td1.theoretical_mass, td2.theoretical_mass);
            Assert.AreEqual(td1.topdown_hits, td2.topdown_hits);
            Assert.AreEqual(td1.modified_mass, td2.modified_mass);
            Assert.AreEqual(td1.matching_experimental, td2.matching_experimental);
            Assert.AreEqual(td1.accession, td2.accession);
            Assert.AreEqual(td1.agg_rt, td2.agg_rt);
            Assert.AreEqual(td1.lysine_count, td2.lysine_count);
            Assert.AreEqual(td1.topdown_id, td2.topdown_id);
            Assert.AreEqual(td1.agg_mass, td2.agg_mass);
            Assert.AreEqual(td1.is_target, td2.is_target);
        }

        [Test]
        public void TestCorrectTopDownID()
        {
            TopDownProteoform td = ConstructorsForTesting.TopDownProteoform("TD1", 1000, 40);
            //linked reference null - should be false
            td.set_correct_id();
            Assert.IsFalse(td.correct_id);
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical();
            t.ExpandedProteinList.First().AccessionList.Add("TD1");
            td.linked_proteoform_references = new List<Proteoform>() { t };
            //no PTMs diff begin fail
            td.begin = 10;
            td.topdown_begin = 10;
            td.end = 20;
            td.topdown_end = 30;
            td.ptm_set = new PtmSet(new List<Ptm>());
            td.topdown_ptm_set = new PtmSet(new List<Ptm>());
            td.set_correct_id();
            Assert.IsFalse(td.correct_id);
            //no PTMs diff end fail
            td.begin = 10;
            td.topdown_begin = 20;
            td.end = 30;
            td.topdown_end = 30;
            td.set_correct_id();
            Assert.IsFalse(td.correct_id);
            //no PTMs same pass
            td.begin = 10;
            td.topdown_begin = 10;
            td.end = 30;
            td.topdown_end = 30;
            td.set_correct_id();
            Assert.IsTrue(td.correct_id);
            //same begin and end, T has more PTMs
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            td.ptm_set = new PtmSet(new List<Ptm>() { new Ptm(15, new Modification("Acetylation", _modificationType : "type", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass: 42.02)) });
            td.set_correct_id();
            Assert.IsFalse(td.correct_id);
            //same begin and end TD has more of a PTM type
            td.ptm_set = new PtmSet(new List<Ptm>());
            td.topdown_ptm_set = new PtmSet(new List<Ptm>() { new Ptm(15, new Modification("Acetylation", _modificationType : "type", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass: 42.02)) });
            td.set_correct_id();
            Assert.IsFalse(td.correct_id);
            //same begin and end and PTMs
            td.ptm_set = new PtmSet(new List<Ptm>() { new Ptm(15, new Modification("Acetylation", _modificationType : "type", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass: 42.02)) });
            td.topdown_ptm_set = new PtmSet(new List<Ptm>() { new Ptm(15, new Modification("Acetylation", _modificationType : "type", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass: 42.02)) });
            td.set_correct_id();
            Assert.IsTrue(td.correct_id);
        }

        [Test]
        public static void test_add_topdown_theoreticals()
        {
            Sweet.lollipop = new Lollipop();
            TopDownProteoform t = ConstructorsForTesting.TopDownProteoform("P32329_1", 1000, 10); //sequence not in database
            TopDownProteoform t2 = ConstructorsForTesting.TopDownProteoform("BADACCESSION", 1000, 10); //accession not in database
            TopDownProteoform t3 = ConstructorsForTesting.TopDownProteoform("P32329_3", 1000, 10); //ptmset not in database w/ this sequence...
            TopDownProteoform t4 = ConstructorsForTesting.TopDownProteoform("P32329_4", 1000, 10); //in database --> won't make a theoretical proteoform
            TopDownProteoform t5 = ConstructorsForTesting.TopDownProteoform("P32329_5", 1000, 10); //will have sequence not in database with ptmset
            t3.sequence = "ADGYEEIIITNQQSFYSVDLEVGTPPQNVTVLVDTGSSDLWIMGSDNPYCSSNSMGSSRRR";
            t4.sequence = "ADGYEEIIITNQQSFYSVDLEVGTPPQNVTVLVDTGSSDLWIMGSDNPYCSSNSMGSSRRR";
            t.sequence = "VKLTSIAAGVAAIAATASATTTLAQSDERVNLVELGVYVSDIRAHLA";
            t5.sequence = "VKLTSIAAGVAAIAATASATTTLAQSDERVNLVELGVYVSDIRAHLA";
            t3.topdown_begin = 68;
            t3.topdown_end = 128;
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            t3.topdown_ptm_set = new PtmSet(new List<Ptm>() { new Ptm(10, new Modification("Acetylation", _modificationType : "Unlocalized", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass: 79.96)) });
            t5.topdown_ptm_set = new PtmSet(new List<Ptm>() { new Ptm(15, new Modification("Acetylation", _modificationType : "Unloaclized", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass: 42.02)) });
            Sweet.lollipop.methionine_oxidation = false;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.methionine_cleavage = true;
            Sweet.lollipop.combine_identical_sequences = false;
            Sweet.lollipop.combine_theoretical_proteoforms_byMass = false;
            Sweet.lollipop.max_ptms = 3;
            Sweet.lollipop.decoy_databases = 1;
            Sweet.lollipop.min_peptide_length = 7;
            Sweet.lollipop.ptmset_mass_tolerance = 0.00001;
            Sweet.lollipop.combine_identical_sequences = true;
            Sweet.lollipop.limit_triples_and_greater = false;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.theoretical_proteins.Clear();
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(Path.Combine(TestContext.CurrentContext.TestDirectory));
            Assert.AreEqual(28, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length);
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform>() { t, t2, t3, t4, t5 };
            Sweet.lollipop.theoretical_database.make_theoretical_proteoforms();
            Assert.IsTrue(Sweet.lollipop.topdown_proteoforms.Contains(t));
            Assert.IsFalse(Sweet.lollipop.topdown_proteoforms.Contains(t2));
            Assert.IsTrue(Sweet.lollipop.topdown_proteoforms.Contains(t3));
            Assert.IsTrue(Sweet.lollipop.topdown_proteoforms.Contains(t4));
            Assert.IsTrue(Sweet.lollipop.topdown_proteoforms.Contains(t5));
            Assert.AreEqual(26, Sweet.lollipop.theoretical_database.expanded_proteins.Length); //should have new topdown protein added
            Assert.AreEqual(1, Sweet.lollipop.theoretical_database.expanded_proteins.Count(p => p.topdown_protein)); //only add 1 new sequence
            Assert.AreEqual("VKLTSIAAGVAAIAATASATTTLAQSDERVNLVELGVYVSDIRAHLA", Sweet.lollipop.theoretical_database.expanded_proteins.Where(p => p.topdown_protein).First().BaseSequence);
            List<TheoreticalProteoform> td_theoreticals = Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(p => p.new_topdown_proteoform).OrderBy(p => p.accession).ToList();
            Assert.AreEqual(1, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Count(p => !p.new_topdown_proteoform && p.sequence == "ADGYEEIIITNQQSFYSVDLEVGTPPQNVTVLVDTGSSDLWIMGSDNPYCSSNSMGSSRRR" && p.ptm_set.ptm_combination.Count == 0));
            Assert.AreEqual(1, td_theoreticals.Count(p => p.sequence == "VKLTSIAAGVAAIAATASATTTLAQSDERVNLVELGVYVSDIRAHLA" && p.ptm_set.ptm_combination.Count == 0));
            Assert.AreEqual(1, td_theoreticals.Count(p => p.sequence == "VKLTSIAAGVAAIAATASATTTLAQSDERVNLVELGVYVSDIRAHLA" && p.ptm_set.ptm_description == "Acetylation"));
            Assert.AreEqual(1, td_theoreticals.Count(p => p.sequence == "ADGYEEIIITNQQSFYSVDLEVGTPPQNVTVLVDTGSSDLWIMGSDNPYCSSNSMGSSRRR" && p.ptm_set.ptm_description == "Acetylation"));
            Assert.AreEqual(31, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length);
            Assert.AreEqual(2, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Count(p => p.ExpandedProteinList.Any(e => e.topdown_protein)));
        }
    }
}