using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.IO;
using Proteomics;
using System.Linq;
using System;
using Chemistry;


namespace Test
{
    [TestFixture]
    class TestCalibration
    {

        [Test]
        public void get_file_descriptions()
        {
            Sweet.lollipop.file_descriptions = new string[]
            {
                "filedescriptions\tbiorep\tfraction\ttechrep",
                "sameFilename\t1\t1\t1",
                "sameFilename\t1\t1\t1"
            };
            Assert.AreEqual("Error in file descriptions file - multiple entries for one filename", Sweet.lollipop.get_file_descriptions());

            Sweet.lollipop.file_descriptions = new string[]
            {
                "filedescriptions\tbiorep\tfraction\ttechrep",
                "badFileValue\ta\t1\t1"
            };
            Assert.AreEqual("Error in file descriptions file values", Sweet.lollipop.get_file_descriptions());
            Sweet.lollipop.file_descriptions = new string[]
            {
                "filedescriptions\tbiorep\tfraction\ttechrep",
                "badFileValue\t1\ta\t1"
            };
            Assert.AreEqual("Error in file descriptions file values", Sweet.lollipop.get_file_descriptions());

            Sweet.lollipop.file_descriptions = new string[]
            {
                "filedescriptions\tbiorep\tfraction\ttechrep",
                "badFileValue\t1\t1\ta"
            };
            Assert.AreEqual("Error in file descriptions file values", Sweet.lollipop.get_file_descriptions());


            Sweet.lollipop.file_descriptions = new string[]
            {
               "filedescriptions\tbiorep\tfraction\ttechrep",
               "10-28-16_A17C_td_yeast_4uscan_fract5_rep2.mzML\t1\t5\t2",
               "10-26-16_A17B_td_yeast_4uscan_fract4_rep2.mzML\t1\t4\t2"
            };
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[7], Lollipop.file_types[7], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_calibration_td_hits();
            Assert.AreEqual("Error in file descriptions file - top-down hit(s) with no matching description.", Sweet.lollipop.get_file_descriptions());

            Sweet.lollipop.file_descriptions = new string[]
            {
               "filedescriptions\tbiorep\tfraction\ttechrep",
               "10-28-16_A17C_td_yeast_4uscan_fract5_rep2.mzML\t1\t5\t2",
               "10-26-16_A17B_td_yeast_4uscan_fract4_rep2.mzML\t1\t4\t2",
               "10-26-16_A17B_td_yeast_4uscan_fract4_rep1.mzML\t1\t4\t1"
            };
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "noisy.xlsx") }, Lollipop.acceptable_extensions[5], Lollipop.file_types[5], Sweet.lollipop.input_files, false);
            Assert.AreEqual("Label fraction, biological replicate, and techincal replicate of input files.", Sweet.lollipop.get_file_descriptions());

            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().fraction = 4;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Assert.AreEqual("Label fraction, biological replicate, and techincal replicate of input files.", Sweet.lollipop.get_file_descriptions());

            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = 4;
            Assert.IsNull(Sweet.lollipop.get_file_descriptions());
        }

        [Test]
        public void get_td_hit_chargestates()
        {
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = 5;

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[7], Lollipop.file_types[7], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_calibration_td_hits();
            Assert.AreEqual("Error: need to input all raw files for top-down hits. Be sure top-down file box is checked.", Sweet.lollipop.get_td_hit_chargestates());

            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = 5;
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().topdown_file = true;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration.xlsx") }, Lollipop.acceptable_extensions[7], Lollipop.file_types[7], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_calibration_td_hits();
            Assert.IsNull(Sweet.lollipop.get_td_hit_chargestates());
            Assert.AreEqual(13, Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().charge);
            Assert.AreEqual(503.60, Math.Round(Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().mz, 2));
            Assert.AreEqual(1873, Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().ms2ScanNumber);
            Assert.AreEqual(45.00, Math.Round(Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().retention_time, 2));
        } 
        
        [Test]
        public void calibrate_td_file()
        {
            //get raw file
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = 5;
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().topdown_file = true;

            //get deconvolution results - same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.xlsx") }, Lollipop.acceptable_extensions[5], Lollipop.file_types[5], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().fraction = 5;
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().topdown_file = true;

            //td calibration hits -- treat as same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[7], Lollipop.file_types[7], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.file_descriptions = new string[]
            {
               "filedescriptions\tbiorep\tfraction\ttechrep",
               "10-28-16_A17C_td_yeast_4uscan_fract5_rep1\t1\t5\t1"
            };
            Assert.AreEqual("Successfully calibrated files.", Sweet.lollipop.calibrate_files());

        }

        [Test]
        public void calibrate_intact_file_with_td_results()
        {
            //get raw file
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = 5;
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().topdown_file = true;

            //add same raw file -- treat as intact-only file NOT td file.
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = 5;

            //get deconvolution results - same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.xlsx") }, Lollipop.acceptable_extensions[5], Lollipop.file_types[5], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().fraction = 5;
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().topdown_file = true;

            //td calibration hits -- treat as same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[7], Lollipop.file_types[7], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.file_descriptions = new string[]
            {
               "filedescriptions\tbiorep\tfraction\ttechrep",
               "10-28-16_A17C_td_yeast_4uscan_fract5_rep1\t1\t5\t1"
            };
            Assert.AreEqual("Successfully calibrated files.", Sweet.lollipop.calibrate_files());
        }

        [Test]
        public void get_topdown_hit_sequence()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);

            //make theo database so have ptm's and sequences... 
            TopDownHit hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>();
            ModificationWithMass mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains("AA0502")).FirstOrDefault();
            hit.ptm_list.Add(new Ptm(hit.sequence.Length, mod));
            ModificationWithMass mod2 = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains("AA0170")).FirstOrDefault();
            hit.ptm_list.Add(new Ptm(3, mod2));
            ModificationWithMass mod3 = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains("AA0433")).FirstOrDefault();
            hit.ptm_list.Add(new Ptm(1, mod3));

            string sequencewithchemicalformula = hit.GetSequenceWithChemicalFormula();
            Assert.AreEqual("[C2H4]AS[C5H12NO5P]DACSDAS[C6H9NO3]D", sequencewithchemicalformula);

            //should add carbamidomethylation to C... 
            Sweet.lollipop.carbamidomethylation = true;
            sequencewithchemicalformula = hit.GetSequenceWithChemicalFormula();
            Assert.AreEqual("[C2H4]AS[C5H12NO5P]DA[H3C2N1O1]CSDAS[C6H9NO3]D", sequencewithchemicalformula);

            //should return null if N term formula wrong or doesn't match mass 
            ModificationWithMassAndCf badNtermMod = new ModificationWithMassAndCf("badNtermMod", null, null, ModificationSites.NTerminus, Chemistry.ChemicalFormula.ParseFormula("H") , -1000, null, null, null, null);
            hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>() { new Ptm(1, badNtermMod) };
            Assert.IsNull(hit.GetSequenceWithChemicalFormula());

            //should return null if mod chem formula wrong or doesn't match mass 
            ModificationWithMassAndCf badMod = new ModificationWithMassAndCf("badMod", null, null, ModificationSites.Any, Chemistry.ChemicalFormula.ParseFormula("H"), -1000, null, null, null, null);
            hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>() { new Ptm(1, badMod) };
            Assert.IsNull(hit.GetSequenceWithChemicalFormula());

            //should return null if N term formula wrong or doesn't match mass 
            ModificationWithMassAndCf badCtermMod = new ModificationWithMassAndCf("badCtermMod", null, null, ModificationSites.TerminusC, Chemistry.ChemicalFormula.ParseFormula("H"), -1000, null, null, null, null);
            hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>() { new Ptm(1, badCtermMod) };
            Assert.IsNull(hit.GetSequenceWithChemicalFormula());

        }
    }
}
