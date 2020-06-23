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
    class TestFLASHDeconv
    {
        [Test]
        public void testFLASHDeconv()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.neucode_labeled = false;

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.mzML") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            
            string filepath = Path.Combine(Path.GetDirectoryName(Sweet.lollipop.input_files[0].complete_path), Path.GetFileNameWithoutExtension(Sweet.lollipop.input_files[0].complete_path));

            // Make sure no initial problems with running deconvolution.
            Assert.AreEqual("Successfully deconvoluted 1 raw file.", Sweet.lollipop.flash_deconv(60, 1, TestContext.CurrentContext.TestDirectory));

            // Ensure the deconvolution output a file.
            Assert.IsTrue(File.Exists(filepath + "_decon.tsv"));

            //Check contents of file to ensure number of components match.
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.min_likelihood_ratio = -100000;
            Sweet.lollipop.max_fit = 10000;
            Sweet.lollipop.enter_input_files(new string[] { filepath + "_decon.tsv" }, Lollipop.acceptable_extensions[0], Lollipop.file_types[0], Sweet.lollipop.input_files, false);
            List<Component> deconv_components = new List<Component>();
            Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Identification).ToList(), deconv_components, Purpose.Identification, false);
            Assert.AreEqual(54, deconv_components.Count);
            Assert.AreEqual(6999.01, Math.Round(deconv_components.OrderBy(c => c.id).First().reported_monoisotopic_mass, 2));
            Assert.AreEqual(6999.01, Math.Round(deconv_components.OrderBy(c => c.id).First().weighted_monoisotopic_mass, 2));
            Assert.AreEqual(15184400000, Math.Round(deconv_components.OrderBy(c => c.id).First().intensity_reported, 2));
            Assert.AreEqual(15184400000, Math.Round(deconv_components.OrderBy(c => c.id).First().intensity_sum, 2));

            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            // Make sure no initial problems with running deconvolution.
            Assert.AreEqual("Error: please convert .raw files to .mzML", Sweet.lollipop.flash_deconv(60, 1, TestContext.CurrentContext.TestDirectory));


        }
    }
}
