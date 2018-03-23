﻿using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Test
{
    [TestFixture]

    class TestPromex
    {
        [Test]
        public void testPromex()
        {
            Sweet.lollipop = new Lollipop();
            // Make sure it hits else statements at end
            Assert.AreEqual("No files deconvoluted. Ensure correct file locations and try again.", Sweet.lollipop.promex_deconvolute(60, 1, 100, 0, 100, -10, TestContext.CurrentContext.TestDirectory));

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            // Make sure no initial problems with running deconvolution
            Assert.AreEqual("Successfully deconvoluted 1 raw file.", Sweet.lollipop.promex_deconvolute(60, 1, 100, 0, 100, -10, TestContext.CurrentContext.TestDirectory));
            // Ensure the deconvolution output a file
            string filepath = Path.Combine(Path.GetDirectoryName(Sweet.lollipop.input_files[0].complete_path), Path.GetFileNameWithoutExtension(Sweet.lollipop.input_files[0].complete_path));
            Assert.IsTrue(File.Exists(filepath + "_deconv.xlsx"));
            Assert.IsTrue(File.Exists(filepath + "_ms1ft.csv"));
            Assert.IsTrue(File.Exists(filepath + ".ms1ft"));
            Assert.IsTrue(File.Exists(filepath + "_parameters.txt"));
            Assert.IsFalse(File.Exists(filepath + "_ms1ft.png"));
            Assert.IsFalse(File.Exists(filepath + ".pbf"));

            // Check contents of file to ensure number of components match
            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { filepath + "_deconv.xlsx" }, Lollipop.acceptable_extensions[0], Lollipop.file_types[0], Sweet.lollipop.input_files, false);
            List<Component> deconv_components = new List<Component>();
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), deconv_components, Purpose.Identification, false);
            Assert.AreEqual(165, deconv_components.Count());
        }

        [Test]
        public void testPromexParameters()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Assert.AreEqual("Successfully deconvoluted 1 raw file.", Sweet.lollipop.promex_deconvolute(60, 1, 46, 45, 0.1, 0, TestContext.CurrentContext.TestDirectory));

            string filepath = Path.Combine(Path.GetDirectoryName(Sweet.lollipop.input_files[0].complete_path), Path.GetFileNameWithoutExtension(Sweet.lollipop.input_files[0].complete_path));

            Sweet.lollipop.input_files.Clear();
            Sweet.lollipop.enter_input_files(new string[] { filepath + "_deconv.xlsx" }, Lollipop.acceptable_extensions[0], Lollipop.file_types[0], Sweet.lollipop.input_files, false);
            List<Component> deconv_components = new List<Component>();
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), deconv_components, Purpose.Identification, false);
            //foreach (Component i in deconv_components)
            //{
            //    Debug.WriteLine(i.reported_monoisotopic_mass);
            //}
            Assert.AreEqual(69, deconv_components.Count());
        }
    }
}
