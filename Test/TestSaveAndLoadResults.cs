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
    public class TestSaveAndLoadResults
    { 
        [Test]
        public void resultsIn_match_resultsOut_component_neucodepair_aggregatedproteoform()
        {
            // Create a couple raw components, make strings, read those strings, and make sure the components match
            Component c1 = new Component();
            Component c2 = new Component();
            c1.id = 1;
            c2.id = 2;
            c1.monoisotopic_mass = 1.0;
            c2.monoisotopic_mass = 2.0;
            c1.weighted_monoisotopic_mass = 1.1;
            c2.weighted_monoisotopic_mass = 2.1;
            c1.corrected_mass = 1.2;
            c2.corrected_mass = 2.2;
            c1.intensity_sum = 100.0;
            c2.intensity_sum = 200.0;
            c1.num_charge_states_fromFile = 5;
            c2.num_charge_states_fromFile = 6;
            c1.delta_mass = 10.0;
            c2.delta_mass = 20.0;
            c1.relative_abundance = 0.5;
            c2.relative_abundance = 0.6;
            c1.fract_abundance = 0.55;
            c2.fract_abundance = 0.66;
            c1.scan_range = "100-102";
            c2.scan_range = "200-202";
            c1.rt_range = "60-61";
            c2.rt_range = "62-63";
            c1.rt_apex = 60.5;
            c2.rt_apex = 62.5;
            c1.intensity_sum_olcs = 99.9;
            c2.intensity_sum_olcs = 199.9;
            c1.file_origin = "file1";
            c2.file_origin = "file2";
            c1.accepted = true;
            c2.accepted = false;
            Lollipop.raw_experimental_components = new List<Component> { c1, c2 };
            Assert.AreEqual(2, Lollipop.raw_experimental_components.Count);
            Lollipop.neucode_labeled = true;
            string[] component_results = Results.raw_component_results().Split(new string[] { Environment.NewLine }, StringSplitOptions.None); // results strings
            Assert.AreEqual(3, component_results.Length);
            Lollipop.raw_experimental_components.Clear();
            Results.read_raw_components(component_results); // read the results strings
            Assert.AreEqual(2, Lollipop.raw_experimental_components.Count);
            Component d1 = Lollipop.raw_experimental_components[0];
            Component d2 = Lollipop.raw_experimental_components[1];
            compare_components(c1, d1);
            compare_components(c2, d2);

            // Construct a couple neucode pairs, check that they are the same before and after results output
            NeuCodePair n1 = new NeuCodePair(c1, c2);
            NeuCodePair n2 = new NeuCodePair(c2, c1);
            n1.intensity_ratio = 0.5;
            n2.intensity_ratio = 0.6;
            n1.lysine_count = 5;
            n2.lysine_count = 6;
            Lollipop.raw_neucode_pairs = new List<NeuCodePair> { n1, n2 };
            string[] neucode_results = Results.raw_neucode_pair_results().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual(3, neucode_results.Length);
            Lollipop.raw_neucode_pairs.Clear();
            Results.read_raw_neucode_pairs(neucode_results);
            NeuCodePair m1 = Lollipop.raw_neucode_pairs[0];
            NeuCodePair m2 = Lollipop.raw_neucode_pairs[1];
            Assert.AreEqual(n1.id_light, m1.id_light);
            Assert.AreEqual(n1.id_heavy, m1.id_heavy);
            Assert.AreEqual(n1.intensity_ratio, m1.intensity_ratio);
            Assert.AreEqual(n1.lysine_count, m1.lysine_count);
            Assert.IsInstanceOf(typeof(Component), m1.neuCodeLight);
            Assert.IsInstanceOf(typeof(Component), m1.neuCodeHeavy);

            // Construct an experimental proteoform, check that it's the same before and after results output
            ExperimentalProteoform e = new ExperimentalProteoform("E1");
            e.aggregated_components = new List<Component> { c1, c2 };
            e.agg_intensity = 1.001;
            e.agg_mass = 1.002;
            e.agg_rt = 1.003;
            Lollipop.proteoform_community.experimental_proteoforms = new List<ExperimentalProteoform> { e };
            string[] proteoform_results = Results.aggregated_experimental_proteoform_results().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual(2, proteoform_results.Length);
            Lollipop.proteoform_community.experimental_proteoforms.Clear();
            Results.read_aggregated_proteoforms(proteoform_results);
            ExperimentalProteoform f = Lollipop.proteoform_community.experimental_proteoforms[0];
            Assert.AreEqual(e.agg_intensity, f.agg_intensity);
            Assert.AreEqual(e.agg_mass, f.agg_mass);
            Assert.AreEqual(e.agg_rt, f.agg_rt);
            Assert.AreEqual(e.aggregated_components[0].id, f.aggregated_components[0].id);
            Assert.AreEqual(e.aggregated_components[1].id, f.aggregated_components[1].id);
            Assert.AreEqual(e.observation_count, f.observation_count);
        }

        public void compare_components(Component c, Component d)
        {
            Assert.AreEqual(c.id, d.id);
            Assert.AreEqual(c.monoisotopic_mass, d.monoisotopic_mass);
            Assert.AreEqual(c.weighted_monoisotopic_mass, d.weighted_monoisotopic_mass);
            Assert.AreEqual(c.corrected_mass, d.corrected_mass);
            Assert.AreEqual(c.intensity_sum, d.intensity_sum);
            //Assert.AreEqual(c.num_charge_states_fromFile, d.num_charge_states_fromFile);
            Assert.AreEqual(c.delta_mass, d.delta_mass);
            Assert.AreEqual(c.relative_abundance, d.relative_abundance);
            Assert.AreEqual(c.fract_abundance, d.fract_abundance);
            Assert.AreEqual(c.scan_range, d.scan_range);
            Assert.AreEqual(c.rt_range, d.rt_range);
            Assert.AreEqual(c.rt_apex, d.rt_apex);
            Assert.AreEqual(c.intensity_sum_olcs, d.intensity_sum_olcs);
            Assert.AreEqual(c.file_origin, d.file_origin);
            Assert.AreEqual(c.accepted, d.accepted);
        }

        [Test]
        public void resultsIn_match_resultsOut_theoretical_protoeoforms()
        {
            // Create a couple theoretical proteoforms and try writing them to strings, and then reading those back into the data structure
            TheoreticalProteoform pf1 = new TheoreticalProteoform("target1");
            TheoreticalProteoform pf2 = new TheoreticalProteoform("target2");
            pf1.description = "something1";
            pf2.description = "something2";
            pf1.name = "name1";
            pf2.name = "name2";
            pf1.fragment = "fragment1";
            pf2.fragment = "fragment2";
            pf1.begin = 1;
            pf2.begin = 2;
            pf1.end = 100;
            pf2.end = 99;
            pf1.unmodified_mass = 1001.0;
            pf2.unmodified_mass = 999.0;
            pf1.ptm_set = new PtmSet(new List<Ptm> { new Ptm(1, new Modification("test", "test", "test", "1", new char[] { 'X' }, 2.345, 2.344)) });
            pf2.ptm_set = new PtmSet( new List<Ptm> { new Ptm() });
            Assert.AreEqual(2.345, pf1.ptm_set.mass);
            Assert.AreEqual(0, pf2.ptm_set.mass);
            pf1.modified_mass = pf1.unmodified_mass + pf1.ptm_set.mass;
            pf2.modified_mass = pf2.unmodified_mass + pf2.ptm_set.mass;
            pf1.lysine_count = 7;
            pf2.lysine_count = 8;
            pf1.is_target = true;
            pf2.is_target = true;
            pf1.is_decoy = false;
            pf2.is_decoy = false;
            Lollipop.proteoform_community.theoretical_proteoforms = new List<TheoreticalProteoform> { pf1, pf2 };
            string[] theoretical_proteoform_results = Results.theoretical_proteoforms_results(true).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual(3, theoretical_proteoform_results.Length);
            Lollipop.proteoform_community.theoretical_proteoforms.Clear();
            Lollipop.uniprotModificationTable = new Dictionary<string, Modification> {
                { "unmodified", new Modification() },
                { "test", new Modification("test", "test", "test", "1", new char[] { 'X' }, 2.345, 2.344) }
            };
            Results.read_theoretical_proteoforms(theoretical_proteoform_results);
            TheoreticalProteoform qf1 = Lollipop.proteoform_community.theoretical_proteoforms[0];
            TheoreticalProteoform qf2 = Lollipop.proteoform_community.theoretical_proteoforms[1];
            compare_theoreticals(new List<TheoreticalProteoform> { pf1, pf2 }, new List<TheoreticalProteoform> { qf1, qf2 });

            // Load these into the decoy database and test that out
            Lollipop.decoy_databases = 2;
            Lollipop.proteoform_community.decoy_proteoforms = new Dictionary<string, List<TheoreticalProteoform>> {
                { Lollipop.decoy_database_name_prefix + "0", new List<TheoreticalProteoform>() { pf1 } },
                { Lollipop.decoy_database_name_prefix + "1", new List<TheoreticalProteoform>() { pf2 } }
            };
            string[] decoy_proteoform_results = Results.theoretical_proteoforms_results(false).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual(3, decoy_proteoform_results.Length);
            Lollipop.proteoform_community.decoy_proteoforms.Clear();
            Results.read_theoretical_proteoforms(decoy_proteoform_results);
            Assert.IsTrue(Lollipop.proteoform_community.decoy_proteoforms.ContainsKey(Lollipop.decoy_database_name_prefix + "0"));
            Assert.IsTrue(Lollipop.proteoform_community.decoy_proteoforms.ContainsKey(Lollipop.decoy_database_name_prefix + "1"));
            qf1 = Lollipop.proteoform_community.decoy_proteoforms[Lollipop.decoy_database_name_prefix + "0"].First();
            qf2 = Lollipop.proteoform_community.decoy_proteoforms[Lollipop.decoy_database_name_prefix + "1"].First();
            compare_theoreticals(new List<TheoreticalProteoform> { pf1, pf2 }, new List<TheoreticalProteoform> { qf1, qf2 });
        }

        private void compare_theoreticals(List<TheoreticalProteoform> pfs, List<TheoreticalProteoform> qfs)
        {
            Assert.IsTrue(pfs.Select(p => p.accession).Contains(qfs[0].accession) && pfs.Select(p => p.accession).Contains(qfs[1].accession));
            Assert.IsTrue(pfs.Select(p => p.name).Contains(qfs[0].name) && pfs.Select(p => p.name).Contains(qfs[1].name));
            Assert.IsTrue(pfs.Select(p => p.description).Contains(qfs[0].description) && pfs.Select(p => p.description).Contains(qfs[1].description));
            Assert.IsTrue(pfs.Select(p => p.fragment).Contains(qfs[0].fragment) && pfs.Select(p => p.fragment).Contains(qfs[1].fragment));
            Assert.IsTrue(pfs.Select(p => p.begin).Contains(qfs[0].begin) && pfs.Select(p => p.begin).Contains(qfs[1].begin));
            Assert.IsTrue(pfs.Select(p => p.end).Contains(qfs[0].end) && pfs.Select(p => p.end).Contains(qfs[1].end));
            Assert.IsTrue(pfs.Select(p => p.unmodified_mass).Contains(qfs[0].unmodified_mass) && pfs.Select(p => p.unmodified_mass).Contains(qfs[1].unmodified_mass));
            Assert.IsTrue(pfs.Select(p => p.modified_mass).Contains(qfs[0].modified_mass) && pfs.Select(p => p.modified_mass).Contains(qfs[1].modified_mass));
            Assert.IsTrue(pfs.Select(p => p.lysine_count).Contains(qfs[0].lysine_count) && pfs.Select(p => p.lysine_count).Contains(qfs[1].lysine_count));
            Assert.IsTrue(pfs.Select(p => p.is_target).Contains(qfs[0].is_target) && pfs.Select(p => p.is_target).Contains(qfs[1].is_target));
            Assert.IsTrue(pfs.Select(p => p.is_decoy).Contains(qfs[0].is_decoy) && pfs.Select(p => p.is_decoy).Contains(qfs[1].is_decoy));
            Assert.IsTrue(pfs.Select(p => p.ptm_mass).Contains(qfs[0].ptm_mass) && pfs.Select(p => p.ptm_mass).Contains(qfs[1].ptm_mass));
            Assert.IsTrue(pfs.Select(p => p.ptm_list_string()).Contains(qfs[0].ptm_list_string()) && pfs.Select(p => p.ptm_list_string()).Contains(qfs[1].ptm_list_string()));
        }
    }
}
