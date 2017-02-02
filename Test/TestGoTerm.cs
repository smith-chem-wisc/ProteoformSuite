using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [TestFixture]
    class TestGoTerm
    {

        [Test]
        public void testGoTermCreation()
        {
            GoTerm g = new GoTerm();
            g.id = "id";
            g.description = "description";
            g.aspect = aspect.biologicalProcess;

            Assert.AreEqual("id", g.id);
            Assert.AreEqual("description", g.description);
            Assert.AreEqual(aspect.biologicalProcess, g.aspect);
        }

        [Test]
        public void testGoTermNumberClass()
        {
            List<GoTerm> oneProteinGoTerms = new List<GoTerm>();
            GoTerm g = new GoTerm();
            g.id = "id";
            g.description = "description";
            g.aspect = aspect.biologicalProcess;
            oneProteinGoTerms.Add(g);

            Dictionary<GoTerm, int> goMasterSet = new Dictionary<GoTerm, int>();
            goMasterSet.Add(g, 1);

            List<Protein> proteinsInSample = new List<Protein>();
            for (int i = 0; i < 4; i++)
            {
                Protein p = new Protein("accession_"+i.ToString(),oneProteinGoTerms);
                proteinsInSample.Add(p);
            }

            Assert.That(() => new GoTermNumber(g, proteinsInSample, goMasterSet), Throws.TypeOf<ArgumentOutOfRangeException>()
                .With.Message
                .EqualTo("GO Term Range is illegal"));

            //GoTermNumber gTN = new GoTermNumber(g, proteinsInSample, goMasterSet);
            //Assert.AreEqual("id", gTN.id);
            //Assert.AreEqual("description", gTN.description);
            //Assert.AreEqual(aspect.biologicalProcess, gTN.aspect);
            //Assert.AreEqual(1, gTN.k);
            //Assert.AreEqual(4, gTN.f);
            //Assert.AreEqual(1d, gTN.pValue);
            //Assert.AreEqual(1d, gTN.logfold);
            //Assert.AreEqual("", gTN.proteinInCategoryFromSample);

        }
    }
}
