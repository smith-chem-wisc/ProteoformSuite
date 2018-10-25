using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Chemistry;
using System;

namespace Test
{
    [TestFixture]

    class TestPromex
    {
        [Test]
        public void testPromex()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;
            
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);

            string filepath = Path.Combine(Path.GetDirectoryName(Sweet.lollipop.input_files[0].complete_path), Path.GetFileNameWithoutExtension(Sweet.lollipop.input_files[0].complete_path));

            if (File.Exists(Path.Combine(filepath + "_ms1ft.png")))
            {
                File.Delete(Path.Combine(filepath + "_ms1ft.png"));
            }
            if (File.Exists(Path.Combine(filepath + ".pbf")))
            {
                File.Delete(Path.Combine(filepath + ".pbf"));
            }
            if (File.Exists(Path.Combine(filepath + ".ms1ft")))
            {
                File.Delete(Path.Combine(filepath + ".ms1ft"));
            }
            if (File.Exists(Path.Combine(filepath + "_ms1ft.csv")))
            {
                File.Delete(Path.Combine(filepath + "_ms1ft.csv"));
            }

            // Make sure no initial problems with running deconvolution
            Assert.AreEqual("Successfully deconvoluted 1 raw file.", Sweet.lollipop.promex_deconvolute(60, 1, TestContext.CurrentContext.TestDirectory));
            // Ensure the deconvolution output a file
            Assert.IsFalse(File.Exists(filepath + "_ms1ft.csv"));
            Assert.IsFalse(File.Exists(filepath + ".ms1ft"));
            Assert.IsFalse(File.Exists(filepath + "_ms1ft.png"));
            Assert.IsFalse(File.Exists(filepath + ".pbf"));
            Assert.IsTrue(File.Exists(filepath + ".tsv"));
            // Check contents of file to ensure number of components match
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.min_likelihood_ratio = -100000;
            Sweet.lollipop.max_fit = 10000;
            Sweet.lollipop.enter_input_files(new string[] { filepath + ".tsv" }, Lollipop.acceptable_extensions[0], Lollipop.file_types[0], Sweet.lollipop.input_files, false);
            List<Component> deconv_components = new List<Component>();
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), deconv_components, Purpose.Identification, false);
            Assert.AreEqual(204, deconv_components.Count);
            Assert.AreEqual(2248.2764, Math.Round(deconv_components.OrderBy(c => c.id).First().reported_monoisotopic_mass), 4);
            Assert.AreEqual(2248.2764, Math.Round(deconv_components.OrderBy(c => c.id).First().weighted_monoisotopic_mass), 4);
            Assert.AreEqual(51985.25, Math.Round(deconv_components.OrderBy(c => c.id).First().intensity_reported), 2);
            Assert.AreEqual(1517513.21, Math.Round(deconv_components.OrderBy(c => c.id).First().intensity_sum), 2);


            Sweet.lollipop.min_likelihood_ratio = 20;
            deconv_components.Clear();
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), deconv_components, Purpose.Identification, false);
            Assert.AreEqual(160, deconv_components.Count());

            Sweet.lollipop.max_fit = .07;
            deconv_components.Clear();
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), deconv_components, Purpose.Identification, false);
            Assert.AreEqual(123, deconv_components.Count());


            Sweet.lollipop = new Lollipop();
            // Make sure it hits else statements at end
            Assert.AreEqual("No files deconvoluted. Ensure correct file locations and try again.", Sweet.lollipop.promex_deconvolute(60, 1, TestContext.CurrentContext.TestDirectory));

            //test promex calibration
            //get raw file
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.calibrate_td_files = true;
            Sweet.lollipop.calibrate_raw_files = false; //shouldn't cali raw files!
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.neucode_labeled = false;

            int raw_file_index = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.SpectraFile)).First());
            int cali_id_file = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.CalibrationIdentification)).First());
            int cali_td_file = Lollipop.file_types.ToList().IndexOf(Lollipop.file_types.Where(f => f.Contains(Purpose.CalibrationTopDown)).First());
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[raw_file_index], Lollipop.file_types[raw_file_index], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).First().lt_condition = "1";

            //get deconvolution results - same file as topdown file/diff tech rep - should be calibrated
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.tsv") }, Lollipop.acceptable_extensions[cali_id_file], Lollipop.file_types[cali_id_file], Sweet.lollipop.input_files, false);
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().fraction = "5";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().biological_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().technical_replicate = "1";
            Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).First().lt_condition = "1";

            //td calibration hits -- treat as same file as topdown file
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration.xlsx") }, Lollipop.acceptable_extensions[cali_td_file], Lollipop.file_types[cali_td_file], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.tsv")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.tsv"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.tsv")));
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML")));
            if (File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")))
                File.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx"));
            Assert.False(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")));
            Sweet.lollipop.read_in_calibration_td_hits();
            Assert.AreEqual(6, Sweet.lollipop.td_hits_calibration.Count);
            Assert.AreEqual(5, Sweet.lollipop.td_hits_calibration.Count(h => h.score > 40));
            Assert.AreEqual("Successfully calibrated files.", Sweet.lollipop.calibrate_files());
            Assert.AreEqual(204, Sweet.lollipop.component_correction.Count);
            Assert.AreEqual(6, Sweet.lollipop.td_hit_correction.Count);
            Assert.IsFalse(Sweet.lollipop.component_correction.Keys.Select(k => k.Item1).Any(k => k == "noisy"));
            Assert.IsFalse(Sweet.lollipop.component_correction.Keys.Select(k => k.Item1).Any(k => k != "05-26-17_B7A_yeast_td_fract5_rep1"));
            Assert.IsFalse(Sweet.lollipop.td_hits_calibration.Any(h => h.mz == h.reported_mass.ToMz(h.charge))); //if calibrated, hit mz is changed  
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx")));
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.tsv")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.mzML")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1_calibrated.mzML")));
            //make sure calibrated file contains correct points...
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "test_topdown_hits_calibration_calibrated.xlsx") }, Lollipop.acceptable_extensions[3], Lollipop.file_types[3], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_calibrated.tsv") }, Lollipop.acceptable_extensions[0], Lollipop.file_types[0], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.tsv") }, Lollipop.acceptable_extensions[cali_id_file], Lollipop.file_types[cali_id_file], Sweet.lollipop.input_files, false);
            List<TopDownHit> calibrated_td_hits = Sweet.lollipop.topdownReader.ReadTDFile(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.TopDown).First());
            List<Component> calibrated_components = new List<Component>();
            List<Component> uncalibrated_components = new List<Component>();
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), calibrated_components, Purpose.Identification, false);
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).ToList(), uncalibrated_components, Purpose.CalibrationIdentification, false);

            Assert.AreEqual(204, calibrated_components.Count());
            Assert.AreEqual(204, uncalibrated_components.Count());
            Assert.AreEqual(6, calibrated_td_hits.Count);
            foreach (TopDownHit h in calibrated_td_hits)
            {
                Assert.IsTrue(Sweet.lollipop.td_hit_correction.ContainsValue(h.reported_mass));
                Assert.IsFalse(Sweet.lollipop.td_hits_calibration.Any(p => p.reported_mass == h.reported_mass));
            }
            foreach (Component c in calibrated_components)
            {
                Assert.True(Sweet.lollipop.component_correction.Values.Contains(System.Math.Round(c.weighted_monoisotopic_mass, 5)));
                Assert.IsFalse(uncalibrated_components.Any(c2 => c2.reported_monoisotopic_mass == c.reported_monoisotopic_mass));
            }
        }
    }
}
