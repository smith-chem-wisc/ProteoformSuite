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
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_calibration_td_hits();
            Assert.IsTrue(Sweet.lollipop.calibrate_files().Contains("Error: need to input all raw files for top-down hits:"));

            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "noisy.xlsx") }, Lollipop.acceptable_extensions[5], Lollipop.file_types[5], Sweet.lollipop.input_files, false);
            Assert.AreEqual("Label condition, fraction, biological replicate, and techincal replicate of all Uncalibrated Proteoform Identification Results and Raw Files.", Sweet.lollipop.calibrate_files());

            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().fraction = "4";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().lt_condition = "1";
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Assert.AreEqual("Label condition, fraction, biological replicate, and techincal replicate of all Uncalibrated Proteoform Identification Results and Raw Files.", Sweet.lollipop.calibrate_files());

            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = "4";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().lt_condition = "1";
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).ToList()[1].fraction = "4";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).ToList()[1].technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).ToList()[1].biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).ToList()[1].lt_condition = "1";
            Assert.AreEqual("Error: Multiple raw files have the same labels for biological replicate, technical replicate, and fraction.", Sweet.lollipop.calibrate_files());
        }

        [Test]
        public void get_td_hit_chargestates()
        {
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().lt_condition = "1";
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_td_hits_file.xlsx") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_calibration_td_hits();
            Assert.IsTrue(Sweet.lollipop.calibrate_files().Contains("Error: need to input all raw files for top-down hits:"));

            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().lt_condition = "1";
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration.xlsx") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            Sweet.lollipop.read_in_calibration_td_hits();
            Assert.AreEqual(0, Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().charge);
            Assert.AreEqual(0, Math.Round(Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().mz, 2));
            Assert.AreEqual(1873, Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().ms2ScanNumber);
            Assert.AreEqual(45.04, Math.Round(Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().ms2_retention_time, 2));
            Assert.AreEqual(0, Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().ms1_retention_time);
            Sweet.lollipop.get_td_hit_chargestates();
            Assert.IsFalse(Sweet.lollipop.td_hits_calibration.Select(h => h.filename.Split('.')[0]).Any(h => Sweet.lollipop.input_files.Count(f => f.purpose == Purpose.RawFile && f.filename == h) == 0));
            Assert.AreEqual(13, Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().charge);
            Assert.AreEqual(503.60, Math.Round(Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().mz, 2));
            Assert.AreEqual(38, Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().ms2ScanNumber);
            Assert.AreEqual(45.00, Math.Round(Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().ms1_retention_time, 2));
            Assert.AreEqual(45.04, Math.Round(Sweet.lollipop.td_hits_calibration.OrderByDescending(h => h.score).First().ms2_retention_time, 2));
        }

        [Test]
        public void calibrate_file_with_same_techrep()
        {
            //get raw file
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.calibrate_td_files = true;
            Sweet.lollipop.calibrate_raw_files = false; //shouldn't cali raw files!
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.neucode_labeled = false;
            int raw_file_index = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.RawFile)).First());
            int cali_id_file = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.CalibrationIdentification)).First());
            int cali_td_file = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.CalibrationTopDown)).First());
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[raw_file_index], Lollipop.file_types[raw_file_index], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().lt_condition = "1";

            //get deconvolution results - same file as topdown file/diff tech rep - should be calibrated
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.xlsx") }, Lollipop.acceptable_extensions[cali_id_file], Lollipop.file_types[cali_id_file], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().lt_condition = "1";

            //add another file - not same fraction as td file... should not be calibrated
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "noisy.xlsx") }, Lollipop.acceptable_extensions[cali_id_file], Lollipop.file_types[cali_id_file], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].fraction = "4";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].lt_condition = "1";

            //td calibration hits -- treat as same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration.xlsx") }, Lollipop.acceptable_extensions[cali_td_file], Lollipop.file_types[cali_td_file], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx")));
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML")));
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1_calibrated.mzML")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1_calibrated.mzML"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1_calibrated.mzML")));
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx"))) 
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")));
            Sweet.lollipop.read_in_calibration_td_hits();
            Assert.AreEqual(6, Sweet.lollipop.td_hits_calibration.Count);
            Assert.AreEqual(5, Sweet.lollipop.td_hits_calibration.Count(h => h.score > 40));
            Assert.AreEqual("Successfully calibrated files.", Sweet.lollipop.calibrate_files());
            Assert.AreEqual(91, Sweet.lollipop.file_mz_correction.Count);
            Assert.AreEqual(6, Sweet.lollipop.td_hit_correction.Count);
            Assert.IsFalse(Sweet.lollipop.file_mz_correction.Keys.Select(k => k.Item1).Any(k => k == "noisy"));
            Assert.IsFalse(Sweet.lollipop.file_mz_correction.Keys.Select(k => k.Item1).Any(k => k != "05-26-17_B7A_yeast_td_fract5_rep1"));
            Assert.IsFalse(Sweet.lollipop.td_hits_calibration.Any(h => h.mz == h.reported_mass.ToMz(h.charge))); //if calibrated, hit mz is changed  
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")));
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1_calibrated.mzML")));
            //make sure calibrated file contains correct points...
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx") }, Lollipop.acceptable_extensions[0], Lollipop.file_types[0], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.xlsx") }, Lollipop.acceptable_extensions[cali_id_file], Lollipop.file_types[cali_id_file], Sweet.lollipop.input_files, false);
            List<TopDownHit> calibrated_td_hits = Sweet.lollipop.topdownReader.ReadTDFile(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.TopDown).First());
            List<Component> calibrated_components = new List<Component>();
            List<Component> uncalibrated_components = new List<Component>();
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), calibrated_components, Purpose.Identification, false);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList(), uncalibrated_components, Purpose.CalibrationIdentification, false);

            Assert.AreEqual(10, calibrated_components.Count());
            Assert.AreEqual(10, uncalibrated_components.Count());

            foreach (TopDownHit h in calibrated_td_hits)
            {
                Assert.IsTrue(Sweet.lollipop.td_hit_correction.ContainsValue(h.reported_mass));
                Assert.IsFalse(Sweet.lollipop.td_hits_calibration.Any(p => p.reported_mass == h.reported_mass));
            }
            foreach (ChargeState cs in calibrated_components.SelectMany(c => c.charge_states))
            {
              Assert.True(Sweet.lollipop.file_mz_correction.Values.Contains(Math.Round(cs.mz_centroid, 5)));
              Assert.IsFalse(uncalibrated_components.SelectMany(c => c.charge_states).All(p => p.mz_centroid == cs.mz_centroid && p.intensity == cs.intensity));
            }
        }

        [Test]
        public void calibrate_file_of_different_tech_rep()
        {
            //get raw files -- one for topdown one for intact mass file (same raw file for both...)
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.calibrate_td_files = false;
            Sweet.lollipop.calibrate_raw_files = true;
            int raw_file_index = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.RawFile)).First());
            int cali_id_file = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.CalibrationIdentification)).First());
            int cali_td_file = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.CalibrationTopDown)).First());

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw") }, Lollipop.acceptable_extensions[raw_file_index], Lollipop.file_types[raw_file_index], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().lt_condition = "normal";
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[raw_file_index], Lollipop.file_types[raw_file_index], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).ToList()[1].fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).ToList()[1].biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).ToList()[1].technical_replicate = "2";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).ToList()[1].lt_condition = "normal";

            //get deconvolution results - same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.xlsx") }, Lollipop.acceptable_extensions[cali_id_file], Lollipop.file_types[cali_id_file], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().lt_condition = "normal";

            //add another file - not same fraction as td file... should not be calibrated
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "noisy.xlsx") }, Lollipop.acceptable_extensions[cali_id_file], Lollipop.file_types[cali_id_file], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].fraction = "4";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].lt_condition = "normal";


            //add another file - not same biorep as td file... should not be calibrated
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].fraction = "4";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].biological_replicate = "2";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList()[1].lt_condition = "normal";

            //td calibration hits -- treat as same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration.xlsx") }, Lollipop.acceptable_extensions[cali_td_file], Lollipop.file_types[cali_td_file], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx")));
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML")));
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1_calibrated.mzml")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1_calibrated.mzml"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1_calibrated.mzML")));
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")));
            Sweet.lollipop.read_in_calibration_td_hits();
            Assert.AreEqual(6, Sweet.lollipop.td_hits_calibration.Count);
            Assert.AreEqual(5, Sweet.lollipop.td_hits_calibration.Count(h => h.score > 40));
            Assert.AreEqual("Successfully calibrated files.", Sweet.lollipop.calibrate_files());
            Assert.AreEqual(91, Sweet.lollipop.file_mz_correction.Count);
            Assert.AreEqual(0, Sweet.lollipop.td_hit_correction.Count);
            Assert.IsFalse(Sweet.lollipop.file_mz_correction.Keys.Select(k => k.Item1).Any(k => k == "noisy"));
            Assert.IsFalse(Sweet.lollipop.file_mz_correction.Keys.Select(k => k.Item1).Any(k => k != "05-26-17_B7A_yeast_td_fract5_rep1"));
            Assert.IsFalse(Sweet.lollipop.td_hits_calibration.Any(h => h.mz == h.reported_mass.ToMz(h.charge))); //if calibrated, hit mz is changed      
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML"))); //didn't calibrate topdown
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1_calibrated.mzML")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-test_topdown_hits_calibration_calibrated.xlsx"))); //didn't calibrate topdown
            //make sure calibrated file contains correct points...
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx") }, Lollipop.acceptable_extensions[0], Lollipop.file_types[0], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.xlsx") }, Lollipop.acceptable_extensions[cali_id_file], Lollipop.file_types[cali_id_file], Sweet.lollipop.input_files, false);
            List<Component> calibrated_components = new List<Component>();
            List<Component> uncalibrated_components = new List<Component>();
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), calibrated_components, Purpose.Identification, false);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList(), uncalibrated_components, Purpose.CalibrationIdentification, false);

            Assert.AreEqual(10, calibrated_components.Count());
            Assert.AreEqual(10, uncalibrated_components.Count());

            foreach (ChargeState cs in calibrated_components.SelectMany(c => c.charge_states))
            {
                Assert.True(Sweet.lollipop.file_mz_correction.Values.Contains(Math.Round(cs.mz_centroid, 5)));
                Assert.IsFalse(uncalibrated_components.SelectMany(c => c.charge_states).All(p => p.mz_centroid == cs.mz_centroid && p.intensity == cs.intensity));
            }
        }

        [Test]
        public void too_few_td_hits_to_calibrate()
        {
            //get raw file
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.calibrate_td_files = true;
            Sweet.lollipop.calibrate_raw_files = true;
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile).First().lt_condition = "1";

            //get deconvolution results - same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.xlsx") }, Lollipop.acceptable_extensions[5], Lollipop.file_types[5], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().lt_condition = "1";

            //td calibration hits -- treat as same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration.xlsx") }, Lollipop.acceptable_extensions[6], Lollipop.file_types[6], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx"))); if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")));
            Sweet.lollipop.read_in_calibration_td_hits();
            Sweet.lollipop.td_hits_calibration = Sweet.lollipop.td_hits_calibration.Where(h => h.score > 150).ToList(); //only 4 hits.
            Assert.AreEqual(4, Sweet.lollipop.td_hits_calibration.Count);
            Assert.AreEqual("Successfully calibrated files. The following files did not calibrate due to not enough calibration points: 05-26-17_B7A_yeast_td_fract5_rep1", Sweet.lollipop.calibrate_files());
            Assert.AreEqual(0, Sweet.lollipop.calibration_components.Count);
            Assert.AreEqual(0, Sweet.lollipop.file_mz_correction.Count);
            Assert.AreEqual(0, Sweet.lollipop.td_hit_correction.Count);
            Assert.IsFalse(Sweet.lollipop.td_hits_calibration.Any(h => h.mz != h.reported_mass.ToMz(h.charge))); //if calibrated, hit mz is changed  
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.xlsx")));
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
            ModificationWithMassAndCf badNtermMod = new ModificationWithMassAndCf("badNtermMod", null, null, TerminusLocalization.NProt, Chemistry.ChemicalFormula.ParseFormula("H") , -1000, null, null, null, null);
            hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>() { new Ptm(1, badNtermMod) };
            Assert.IsNull(hit.GetSequenceWithChemicalFormula());

            //should return null if mod chem formula wrong or doesn't match mass 
            ModificationWithMassAndCf badMod = new ModificationWithMassAndCf("badMod", null, null, TerminusLocalization.Any, Chemistry.ChemicalFormula.ParseFormula("H"), -1000, null, null, null, null);
            hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>() { new Ptm(1, badMod) };
            Assert.IsNull(hit.GetSequenceWithChemicalFormula());

            //should return null if N term formula wrong or doesn't match mass 
            ModificationWithMassAndCf badCtermMod = new ModificationWithMassAndCf("badCtermMod", null, null, TerminusLocalization.ProtC, Chemistry.ChemicalFormula.ParseFormula("H"), -1000, null, null, null, null);
            hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>() { new Ptm(1, badCtermMod) };
            Assert.IsNull(hit.GetSequenceWithChemicalFormula());
        }
    }
}
