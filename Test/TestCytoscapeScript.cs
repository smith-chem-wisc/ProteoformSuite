using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System.IO;
using System.Xml;

namespace Test
{
    [TestFixture]
    class TestCytoscapeScript
    {
        [Test]
        public void nodes_table_gives_meaningful_modified_theoreticals()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            ModificationWithMass m = new ModificationWithMass("oxidation", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");

            ProteoformFamily f = new ProteoformFamily(new List<Proteoform> {
                    new TheoreticalProteoform("T1","","T1","",0,0,100,20,new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm(0, m) }),100,true)
                }, 1);
            string node_table = CytoscapeScript.get_cytoscape_nodes_tsv(new List<ProteoformFamily> { f }, false, CytoscapeScript.color_scheme_names[0], 2);
            Assert.True(node_table.Contains(CytoscapeScript.modified_theoretical_label));
            Assert.True(node_table.Contains(f.theoretical_proteoforms[0].ptm_list_string()));
        }

        [Test]
        public void nodes_table_gives_meaningful_modified_theoreticals2()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            ModificationWithMass m = new ModificationWithMass("oxidation", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");

            ProteoformFamily f = new ProteoformFamily(new List<Proteoform> {
                    new TheoreticalProteoform("T1","","T1","",0,0,100,20,new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm(0, m), new Ptm(0, m) }),100,true)
                }, 1);
            string node_table = CytoscapeScript.get_cytoscape_nodes_tsv(new List<ProteoformFamily> { f }, false, CytoscapeScript.color_scheme_names[0], 2);
            Assert.True(node_table.Contains(CytoscapeScript.modified_theoretical_label));
            Assert.True(node_table.Contains(f.theoretical_proteoforms[0].ptm_list_string()));
        }

        [Test]
        public void nodes_table_gives_meaningful_unmodified_theoreticals()
        {
            ProteoformFamily f = new ProteoformFamily(new List<Proteoform> {
                    new TheoreticalProteoform("T1","","T1","",0,0,100,20,new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }),100,true) //unmodified has one PTM labeled unmodified
                }, 1);
            string node_table = CytoscapeScript.get_cytoscape_nodes_tsv(new List<ProteoformFamily> { f }, false, CytoscapeScript.color_scheme_names[0], 2);
            Assert.True(node_table.Contains(CytoscapeScript.unmodified_theoretical_label));
            Assert.True(node_table.Contains(f.theoretical_proteoforms[0].ptm_list_string()));
        }

        [Test]
        public void nodes_table_gives_meaningful_case_insensitive_unmodified_theoreticals()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            ModificationWithMass m = new ModificationWithMass("Unmodified", new Tuple<string, string>("", ""), motif, ModificationSites.K, 0, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");

            ProteoformFamily f = new ProteoformFamily(new List<Proteoform> {
                    new TheoreticalProteoform("T1","","T1","",0,0,100,20,new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }),100,true) //unmodified has one PTM labeled unmodified
                }, 1);
            string node_table = CytoscapeScript.get_cytoscape_nodes_tsv(new List<ProteoformFamily> { f }, false, CytoscapeScript.color_scheme_names[0], 2);
            Assert.True(node_table.Contains(CytoscapeScript.unmodified_theoretical_label));
            Assert.True(node_table.Contains(f.theoretical_proteoforms[0].ptm_list_string()));
        }

        [Test]
        public void nodes_table_gives_meaningful_experimentals()
        {
            ExperimentalProteoform e = new ExperimentalProteoform("E1");
            e.agg_intensity = 999.99;
            e.agg_mass = 888.88;
            e.agg_rt = 777.77;
            ProteoformFamily f = new ProteoformFamily(new List<Proteoform> { e }, 1);
            string node_table = CytoscapeScript.get_cytoscape_nodes_tsv(new List<ProteoformFamily> { f }, false, CytoscapeScript.color_scheme_names[0], 2);
            Assert.True(node_table.Contains("E1"));
            Assert.True(node_table.Contains("999.99"));
        }

        [Test]
        public void test_write_families_no_experimentals_which_shouldnt_happen()
        {
            List<ProteoformFamily> f = new List<ProteoformFamily> { new ProteoformFamily(new List<Proteoform> {
                    new TheoreticalProteoform("T1","","T1","",0,0,100,20,new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }),100,true)
                }, 1) };
            string message = CytoscapeScript.write_cytoscape_script(f, f, TestContext.CurrentContext.TestDirectory, "test", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0], 2);
            Assert.True(message.Contains("Error"));
        }

        [Test]
        public void test_write_families_regular_display()
        {
            TheoreticalProteoform t = new TheoreticalProteoform("T1", "", "T1", "", 0, 0, 100, 20, new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }), 100, true);
            ExperimentalProteoform e = new ExperimentalProteoform("E1");
            e.agg_intensity = 999.99;
            e.quant.lightIntensitySum = 444.44m;
            e.quant.heavyIntensitySum = 333.33m;
            e.quant.intensitySum = 777.77m;
            List<ProteoformFamily> f = new List<ProteoformFamily> { new ProteoformFamily(new List<Proteoform> { t, e }, 1) };
            string message = CytoscapeScript.write_cytoscape_script(f, f, TestContext.CurrentContext.TestDirectory, "test", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0], 2);
            Assert.True(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.script_file_prefix + "test" + CytoscapeScript.script_file_extension)));
            Assert.True(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.style_file_prefix + "test" + CytoscapeScript.style_file_extension)));
            Assert.True(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.node_file_prefix + "test" + CytoscapeScript.node_file_extension)));
            Assert.True(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.edge_file_prefix + "test" + CytoscapeScript.edge_file_extension)));

            //Check that the style xml is all there
            List<string> vizProperties = new List<string>();
            using (XmlReader xml = XmlReader.Create(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.style_file_prefix + "test" + CytoscapeScript.style_file_extension)))
            {
                while (xml.Read())
                {
                    switch (xml.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xml.Name == "visualProperty") vizProperties.Add(xml.GetAttribute("name"));
                            break;
                    }
                }
            }
            Assert.AreEqual(CytoscapeScript.default_styles.Keys.Where(s => s.StartsWith("NETWORK")).Count(), vizProperties.Where(s => s.StartsWith("NETWORK")).Count());
            Assert.AreEqual(CytoscapeScript.default_styles.Keys.Where(s => s.StartsWith("NODE")).Count(), vizProperties.Where(s => s.StartsWith("NODE")).Count());
            Assert.AreEqual(CytoscapeScript.default_styles.Keys.Where(s => s.StartsWith("EDGE")).Count(), vizProperties.Where(s => s.StartsWith("EDGE")).Count());
            Assert.AreEqual(CytoscapeScript.default_styles.Keys.Where(s => !s.StartsWith("NETWORK") && !s.StartsWith("NODE") && !s.StartsWith("EDGE")).Count(), vizProperties.Where(s => !s.StartsWith("NETWORK") && !s.StartsWith("NODE") && !s.StartsWith("EDGE")).Count());
            Assert.AreEqual(CytoscapeScript.default_styles.Keys.Count, vizProperties.Count);
        }

        // Reminder: Test making edges
        // Reminder: Test making scripts from object lists of TheoreticalProteoforms
        // Reminder: Test making scripts from object lists of GoTerm
        // Reminder: Test making scripts from subset of families
    }
}
