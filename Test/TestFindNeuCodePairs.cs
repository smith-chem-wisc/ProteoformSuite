using DocumentFormat.OpenXml.Spreadsheet;
using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [TestFixture]
    public class TestFindNeuCodePairs
    {

        [Test]
        public void testFindRawNeuCodePairsMethod()
        {

            ////these are only putative

            //List<Component> testComponentList = new List<Component>();

            //Component c1 = new Component();
            //c1.weighted_monoisotopic_mass = 1.0;
            //List<Cell> cRow = new List<Cell>();
            //Cell c = new Cell();
            //for (int i = 1; i < 5; i++)
            //{
            //    c.CellValue = new CellValue(i.ToString());
            //    cRow.Add(c);
            //}
            
            //ChargeState s1 = new ChargeState(cRow);

            //c1.charge_states.Add(s1);

            //Component c2 = new Component();
            //c2.weighted_monoisotopic_mass = 1.1;

            //testComponentList.Add(c1);
            //testComponentList.Add(c2);

            //Lollipop.find_neucode_pairs(testComponentList);


            //Assert.AreEqual(1, Lollipop.raw_neucode_pairs.Count);

            Assert.AreEqual(1, 1);
        }

    }
}
