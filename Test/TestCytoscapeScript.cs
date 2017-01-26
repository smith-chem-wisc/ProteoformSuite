using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProteoformSuiteInternal;

namespace Test
{
    [TestFixture]
    class TestCytoscapeScript
    {
        [Test]
        public void nodes_table_gives_meaningful_modified_theoreticals()
        {
            ProteoformFamily f = new ProteoformFamily(
                new List<Proteoform> {
                    new TheoreticalProteoform("T1","","T1","",0,0,100,20,new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm(), new Ptm() }),100, "", true)
                }, 1);
            CytoscapeScript c = new CytoscapeScript(new List<ProteoformFamily> { f }, "yup", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0]);
            c.node_table.Contains(CytoscapeScript.modified_theoretical_label);
        }
        [Test]
        public void nodes_table_gives_meaningful_unmodified_theoreticals()
        {
            ProteoformFamily f = new ProteoformFamily(
                new List<Proteoform> {
                    new TheoreticalProteoform("T1","","T1","",0,0,100,20,new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }),100, "", true)
                }, 1);
            CytoscapeScript c = new CytoscapeScript(new List<ProteoformFamily> { f }, "yup", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0]);
            c.node_table.Contains(CytoscapeScript.unmodified_theoretical_label);
        }
    }
}
