using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.IO;
using Proteomics;
using System.Linq;
using System;
using Chemistry;
using PRISM;
using CommandLine;

namespace Test
{
    [TestFixture]

    class TestPromex
    {
        [Test]
        public void testpromex()
        {
            Sweet.lollipop = new Lollipop();

            Assert.AreEqual("No files deconvoluted. Ensure correct file locations and try again.", Sweet.lollipop.promex_deconvolute(60, 1, 50000, 2000, 100, 0, TestContext.CurrentContext.TestDirectory));

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "05-26-17_B7A_yeast_td_fract5_rep1_MS1.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            
            Assert.AreEqual("Successfully deconvoluted 1 raw file.", Sweet.lollipop.promex_deconvolute(60, 1, 50000, 2000, 100, 0, TestContext.CurrentContext.TestDirectory));

            Assert.IsTrue(File.Exists(Path.Combine(Path.GetDirectoryName(Sweet.lollipop.input_files[0].complete_path), Path.GetFileNameWithoutExtension(Sweet.lollipop.input_files[0].complete_path)) + "_deconv.xlsx"));
            
        }
    }
}
