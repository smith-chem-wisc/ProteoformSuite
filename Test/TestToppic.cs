using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Test
{
    [TestFixture]

    class TestToppic
    {
        [Test]
        public void testToppic()
        {
            Sweet.lollipop = new Lollipop();

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "st.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot-st.fasta") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);

            Assert.AreEqual("Successfully analyzed top-down file.", Sweet.lollipop.Toppic(TestContext.CurrentContext.TestDirectory, 30, 100000, 0.02, 1, 3.0, 4, true, 15, 500, 1));

            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\st.mzML"));
            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\st_ms1.msalign"));
            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\st_ms2.msalign"));
            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\st_ms1.OUTPUT_TABLE"));
            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\st_ms1.FORM_OUTPUT_TABLE"));
        }
    }
}
