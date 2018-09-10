using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    public class TestProcessRawComponents
    {
        [OneTimeSetUp]
        public void setup()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        [Test]
        public void testProcessRawComponents()
        {
            Lollipop.correctionFactors = null;
            Lollipop.raw_experimental_components.Clear(); //for if neucode test is run first
            Func<string, IEnumerable<Component>> componentReader = c => new ExcelReader().read_components_from_xlsx(c, Lollipop.correctionFactors);
            Lollipop.deconResultsFileNames = new System.ComponentModel.BindingList<string>();
            Lollipop.deconResultsFileNames.Add("UnitTestFiles\\noisy.xlsx");
            Lollipop.neucode_labeled = true;
            Lollipop.process_raw_components(componentReader);

            Assert.AreEqual(224, Lollipop.raw_experimental_components.Count);
        }

    }
}
