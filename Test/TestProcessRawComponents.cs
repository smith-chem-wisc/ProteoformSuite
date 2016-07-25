using DocumentFormat.OpenXml;
using NUnit.Framework;
using ProteoformSuite;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Lollipop.raw_experimental_components.Clear(); //for if neucode test is run first
            Func<string, IEnumerable<Component>> componentReader = c => new ExcelReader().read_components_from_xlsx(c);
            Lollipop.deconResultsFileNames = new System.ComponentModel.BindingList<string>();
            Lollipop.deconResultsFileNames.Add("UnitTestFiles\\noisy.xlsx");
            Lollipop.neucode_labeled = true;
            Lollipop.process_raw_components(componentReader);

            Assert.AreEqual(222,Lollipop.raw_experimental_components.Count);
        }



    }
}
