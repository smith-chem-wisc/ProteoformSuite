using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Test
{
    [TestFixture]

    class testMSPathFinderT
    {
        [Test]
        public void testTopDownAnalysis()
        {
            Sweet.lollipop = new Lollipop();

            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "sliced.raw") }, Lollipop.acceptable_extensions[4], Lollipop.file_types[4], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "Short.fasta") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);

            Assert.AreEqual("Successfully analyzed 1 file(s).", Sweet.lollipop.MSPathFinderT(TestContext.CurrentContext.TestDirectory));
        }
    }
}
