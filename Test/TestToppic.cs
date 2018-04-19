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

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "sliced_08-02-17_B9_myoblast_A_fract3and4_td_rep2.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "mouse_121317.fasta") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);

            Assert.AreEqual("Successfully analyzed top-down file(s).", Lollipop.Toppic(TestContext.CurrentContext.TestDirectory, 30));

            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\sliced_08-02-17_B9_myoblast_A_fract3and4_td_rep2.mzML"));
            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\sliced_08-02-17_B9_myoblast_A_fract3and4_td_rep2_ms1.msalign"));
            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\sliced_08-02-17_B9_myoblast_A_fract3and4_td_rep2_ms2.msalign"));
            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\sliced_08-02-17_B9_myoblast_A_fract3and4_td_rep2.feature"));
            Assert.IsTrue(File.Exists(TestContext.CurrentContext.TestDirectory + @"\sliced_08-02-17_B9_myoblast_A_fract3and4_td_rep2_ms2.OUTPUT_TABLE"));
        }

    }
}
