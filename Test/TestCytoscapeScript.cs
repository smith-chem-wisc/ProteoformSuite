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
            string mod_title = "oxidation";
            ModificationWithMass m = new ModificationWithMass(mod_title, new Tuple<string, string>("", mod_title), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");

            Proteoform p = new TheoreticalProteoform("T1", "", "T1_1", "", 0, 0, 100, 20, new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm(0, m) }), 100, true);
            ProteoformFamily f = new ProteoformFamily(new List<Proteoform> { p }, 1);
            string node_table = CytoscapeScript.get_cytoscape_nodes_tsv(new List<ProteoformFamily> { f }, false, CytoscapeScript.color_scheme_names[0], 2);
            Assert.True(node_table.Contains(CytoscapeScript.modified_theoretical_label));
            Assert.AreNotEqual(f.theoretical_proteoforms[0].accession, CytoscapeScript.get_proteoform_shared_name(p, 2));
        }

        [Test]
        public void nodes_table_gives_meaningful_modified_theoreticals2()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            ModificationWithMass m = new ModificationWithMass("oxidation", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");

            Proteoform p = new TheoreticalProteoform("T1", "", "T1_1", "", 0, 0, 100, 20, new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm(0, m), new Ptm(0, m) }), 100, true);
            ProteoformFamily f = new ProteoformFamily(new List<Proteoform> { p }, 1);
            string node_table = CytoscapeScript.get_cytoscape_nodes_tsv(new List<ProteoformFamily> { f }, false, CytoscapeScript.color_scheme_names[0], 2);
            Assert.True(node_table.Contains(CytoscapeScript.modified_theoretical_label));
            Assert.AreNotEqual(f.theoretical_proteoforms[0].accession, CytoscapeScript.get_proteoform_shared_name(p, 2));
        }

        [Test]
        public void nodes_table_gives_meaningful_unmodified_theoreticals()
        {
            Proteoform p = new TheoreticalProteoform("T1", "", "T1_1", "", 0, 0, 100, 20, new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }), 100, true); //unmodified has one PTM labeled unmodified
            ProteoformFamily f = new ProteoformFamily(new List<Proteoform> { p }, 1);
            string node_table = CytoscapeScript.get_cytoscape_nodes_tsv(new List<ProteoformFamily> { f }, false, CytoscapeScript.color_scheme_names[0], 2);
            Assert.True(node_table.Contains(CytoscapeScript.unmodified_theoretical_label));
            Assert.AreNotEqual(f.theoretical_proteoforms[0].accession, CytoscapeScript.get_proteoform_shared_name(p, 2));
        }

        [Test]
        public void nodes_table_gives_meaningful_case_insensitive_unmodified_theoreticals()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            string mod_title = "unmodified".ToUpper();
            ModificationWithMass m = new ModificationWithMass(mod_title, new Tuple<string, string>("N/A", mod_title), motif, ModificationSites.K, 0, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");

            Proteoform p = new TheoreticalProteoform("T1", "", "T1_1", "", 0, 0, 100, 20, new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }), 100, true); //unmodified has one PTM labeled unmodified
            ProteoformFamily f = new ProteoformFamily(new List<Proteoform> { p }, 1);
            string node_table = CytoscapeScript.get_cytoscape_nodes_tsv(new List<ProteoformFamily> { f }, false, CytoscapeScript.color_scheme_names[0], 2);
            Assert.True(node_table.Contains(CytoscapeScript.unmodified_theoretical_label));
            Assert.AreNotEqual(f.theoretical_proteoforms[0].accession, CytoscapeScript.get_proteoform_shared_name(p, 2));
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
                    new TheoreticalProteoform("T1","","T1_1","",0,0,100,20,new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }),100,true)
                }, 1) };
            string message = CytoscapeScript.write_cytoscape_script(f, f, TestContext.CurrentContext.TestDirectory, "test", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0], 2);
            Assert.True(message.Contains("Error"));
        }

        [Test]
        public void test_write_families_regular_display()
        {
            TheoreticalProteoform t = new TheoreticalProteoform("T1", "", "T1_1", "", 0, 0, 100, 20, new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }), 100, true);
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
            Assert.True(message.StartsWith("Finished"));
        }

        [Test]
        public void cytoscape_improper_build_folder()
        {
            TheoreticalProteoform t = new TheoreticalProteoform("T1", "", "T1_1", "", 0, 0, 100, 20, new List<GoTerm>(), new PtmSet(new List<Ptm> { new Ptm() }), 100, true);
            ExperimentalProteoform e = new ExperimentalProteoform("E1");
            e.agg_intensity = 999.99;
            e.quant.lightIntensitySum = 444.44m;
            e.quant.heavyIntensitySum = 333.33m;
            e.quant.intensitySum = 777.77m;
            List<ProteoformFamily> f = new List<ProteoformFamily> { new ProteoformFamily(new List<Proteoform> { t, e }, 1) };
            string message = CytoscapeScript.write_cytoscape_script(f, f, "", "test", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0], 2);
            Assert.False(message.StartsWith("Finished"));
        }

        [Test]
        public void cytoscape_edges_and_nodes_match()
        {
            ProteoformCommunity community = TestProteoformFamilies.construct_two_families_with_potentially_colliding_theoreticals();
            string edges = CytoscapeScript.get_cytoscape_edges_tsv(community.families, 2);
            string[] lines = edges.Split(new char[] { '\n' });
            HashSet<string> shared_pf_names_edges = new HashSet<string>();
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i] == "") break;
                string[] line = lines[i].Split(new char[] { '\t' });
                shared_pf_names_edges.Add(line[0]);
                shared_pf_names_edges.Add(line[2]);
            }

            string nodes = CytoscapeScript.get_cytoscape_nodes_tsv(community.families, false, CytoscapeScript.color_scheme_names[0], 2);
            lines = nodes.Split(new char[] { '\n' });
            HashSet<string> shared_pf_names_nodes = new HashSet<string>();
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i] == "") break;
                string[] line = lines[i].Split(new char[] { '\t' });
                shared_pf_names_nodes.Add(line[0]);
            }

            Assert.True(shared_pf_names_nodes.All(name => shared_pf_names_edges.Contains(name)));
            Assert.True(shared_pf_names_edges.All(name => shared_pf_names_nodes.Contains(name)));
            Assert.AreEqual(8, shared_pf_names_nodes.Count); //both families
        }

        [Test]
        public void cytoscape_script_from_theoreticals()
        {
            ProteoformCommunity community = TestProteoformFamilies.construct_two_families_with_potentially_colliding_theoreticals();
            CytoscapeScript.write_cytoscape_script(community.families.SelectMany(f => f.theoretical_proteoforms).ToArray(), community.families, TestContext.CurrentContext.TestDirectory, "test", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0], 2);
            string[] edge_lines = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.edge_file_prefix + "test" + CytoscapeScript.edge_file_extension));
            HashSet<string> shared_pf_names_edges = new HashSet<string>();
            for (int i = 1; i < edge_lines.Length; i++)
            {
                if (edge_lines[i] == "") break;
                string[] line = edge_lines[i].Split(new char[] { '\t' });
                shared_pf_names_edges.Add(line[0]);
                shared_pf_names_edges.Add(line[2]);
            }

            string[] node_lines = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.node_file_prefix + "test" + CytoscapeScript.node_file_extension));
            HashSet<string> shared_pf_names_nodes = new HashSet<string>();
            for (int i = 1; i < node_lines.Length; i++)
            {
                if (node_lines[i] == "") break;
                string[] line = node_lines[i].Split(new char[] { '\t' });
                shared_pf_names_nodes.Add(line[0]);
            }

            Assert.True(shared_pf_names_nodes.All(name => shared_pf_names_edges.Contains(name)));
            Assert.True(shared_pf_names_edges.All(name => shared_pf_names_nodes.Contains(name)));
            Assert.AreEqual(5, shared_pf_names_nodes.Count); //only the first family
        }

        [Test]
        public void cytoscape_script_from_goterm()
        {
            ProteoformCommunity community = TestProteoformFamilies.construct_two_families_with_potentially_colliding_theoreticals();
            CytoscapeScript.write_cytoscape_script(new GoTerm[] { TestProteoformFamilies.p1_goterm }, community.families, TestContext.CurrentContext.TestDirectory, "test", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0], 2);
            string[] edge_lines = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.edge_file_prefix + "test" + CytoscapeScript.edge_file_extension));
            HashSet<string> shared_pf_names_edges = new HashSet<string>();
            for (int i = 1; i < edge_lines.Length; i++)
            {
                if (edge_lines[i] == "") break;
                string[] line = edge_lines[i].Split(new char[] { '\t' });
                shared_pf_names_edges.Add(line[0]);
                shared_pf_names_edges.Add(line[2]);
            }

            string[] node_lines = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.node_file_prefix + "test" + CytoscapeScript.node_file_extension));
            HashSet<string> shared_pf_names_nodes = new HashSet<string>();
            for (int i = 1; i < node_lines.Length; i++)
            {
                if (node_lines[i] == "") break;
                string[] line = node_lines[i].Split(new char[] { '\t' });
                shared_pf_names_nodes.Add(line[0]);
            }

            Assert.True(shared_pf_names_nodes.All(name => shared_pf_names_edges.Contains(name)));
            Assert.True(shared_pf_names_edges.All(name => shared_pf_names_nodes.Contains(name)));
            Assert.AreEqual(5, shared_pf_names_nodes.Count); //only the first family
        }

        [Test]
        public void cytoscape_script_from_quantValues()
        {
            ProteoformCommunity community = TestProteoformFamilies.construct_two_families_with_potentially_colliding_theoreticals();
            CytoscapeScript.write_cytoscape_script(community.families.SelectMany(f => f.experimental_proteoforms.Select(ex => ex.quant)).ToArray(), community.families, TestContext.CurrentContext.TestDirectory, "test", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0], 2);
            string[] edge_lines = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.edge_file_prefix + "test" + CytoscapeScript.edge_file_extension));
            HashSet<string> shared_pf_names_edges = new HashSet<string>();
            for (int i = 1; i < edge_lines.Length; i++)
            {
                if (edge_lines[i] == "") break;
                string[] line = edge_lines[i].Split(new char[] { '\t' });
                shared_pf_names_edges.Add(line[0]);
                shared_pf_names_edges.Add(line[2]);
            }

            string[] node_lines = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.node_file_prefix + "test" + CytoscapeScript.node_file_extension));
            HashSet<string> shared_pf_names_nodes = new HashSet<string>();
            for (int i = 1; i < node_lines.Length; i++)
            {
                if (node_lines[i] == "") break;
                string[] line = node_lines[i].Split(new char[] { '\t' });
                shared_pf_names_nodes.Add(line[0]);
            }

            Assert.True(shared_pf_names_nodes.All(name => shared_pf_names_edges.Contains(name)));
            Assert.True(shared_pf_names_edges.All(name => shared_pf_names_nodes.Contains(name)));
            Assert.AreEqual(8, shared_pf_names_nodes.Count); //both families
        }

        [Test]
        public void cytoscape_script_from_subset_of_families()
        {
            ProteoformCommunity community = TestProteoformFamilies.construct_two_families_with_potentially_colliding_theoreticals();
            CytoscapeScript.write_cytoscape_script(new List<ProteoformFamily> { community.families[0] }, community.families, TestContext.CurrentContext.TestDirectory, "test", false, false, false, false, CytoscapeScript.color_scheme_names[0], CytoscapeScript.node_label_positions[0], 2);
            string[] edge_lines = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.edge_file_prefix + "test" + CytoscapeScript.edge_file_extension));
            HashSet<string> shared_pf_names_edges = new HashSet<string>();
            for (int i = 1; i < edge_lines.Length; i++)
            {
                if (edge_lines[i] == "") break;
                string[] line = edge_lines[i].Split(new char[] { '\t' });
                shared_pf_names_edges.Add(line[0]);
                shared_pf_names_edges.Add(line[2]);
            }

            string[] node_lines = File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, CytoscapeScript.node_file_prefix + "test" + CytoscapeScript.node_file_extension));
            HashSet<string> shared_pf_names_nodes = new HashSet<string>();
            for (int i = 1; i < node_lines.Length; i++)
            {
                if (node_lines[i] == "") break;
                string[] line = node_lines[i].Split(new char[] { '\t' });
                shared_pf_names_nodes.Add(line[0]);
            }

            Assert.True(shared_pf_names_nodes.All(name => shared_pf_names_edges.Contains(name)));
            Assert.True(shared_pf_names_edges.All(name => shared_pf_names_nodes.Contains(name)));
            Assert.AreEqual(5, shared_pf_names_nodes.Count); //only the first family
        }
    }
}
