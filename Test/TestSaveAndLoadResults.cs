using System;
using System.Collections;
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

            InputFile f1 = new InputFile(1, "UnitTestFiles\\file1.ext", Labeling.NeuCode, Purpose.Identification);
            InputFile f2 = new InputFile(2, "UnitTestFiles\\file2.ext", Labeling.NeuCode, Purpose.Identification);
            Lollipop.input_files.Add(f1);
            Lollipop.input_files.Add(f2);

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
            c1.input_file = f1;
            c2.input_file = f2;
            c1.accepted = true;
            c2.accepted = false;
            Lollipop.raw_experimental_components = new List<Component> { c1, c2 };
            Assert.AreEqual(2, Lollipop.raw_experimental_components.Count);
            Lollipop.neucode_labeled = true;
            string[] inputFile_results = Results.input_file_results().Split(new string[] { Environment.NewLine }, StringSplitOptions.None); // results strings
            string[] component_results = Results.raw_component_results().Split(new string[] { Environment.NewLine }, StringSplitOptions.None); // results strings
            Assert.AreEqual(3, component_results.Length);
            Lollipop.raw_experimental_components.Clear();
            Results.read_input_files(inputFile_results);
            Results.read_raw_components(component_results); // read the results strings
            
            Assert.AreEqual(2, Lollipop.raw_experimental_components.Count);
            //Parallel.For used for reading the results strings does not produce a stable order, so compare based on ID
            compare_components(c1, Lollipop.raw_experimental_components.Find(d => d.id == c1.id));
            compare_components(c2, Lollipop.raw_experimental_components.Find(d => d.id == c2.id));

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
            //Parallel.For used for reading the results strings does not produce a stable order, so compare based on ID
            NeuCodePair m1 = Lollipop.raw_neucode_pairs.Find(m => m.id == n1.id);
            NeuCodePair m2 = Lollipop.raw_neucode_pairs.Find(m => m.id == n2.id);
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
            foreach (Component c in e.aggregated_components) Assert.IsTrue(f.aggregated_components.Select(d => d.id).Contains(c.id));
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
            //Assert.AreEqual(c.input_file, d.input_file);
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
            Lollipop.uniprotModificationTable = new Dictionary<string, Modification>
            {
                { "unmodified", new Modification() },
                { "test", new Modification("test", "test", "test", "1", new char[] { 'X' }, 2.345, 2.344) }
            };
            Results.read_theoretical_proteoforms(theoretical_proteoform_results);
            compare_theoreticals(pf1, Lollipop.proteoform_community.theoretical_proteoforms.Find(qf => qf.accession == pf1.accession));
            compare_theoreticals(pf2, Lollipop.proteoform_community.theoretical_proteoforms.Find(qf => qf.accession == pf2.accession));

            // Load these into the decoy database and test that out
            Lollipop.decoy_databases = 2;
            Lollipop.proteoform_community.decoy_proteoforms = new Dictionary<string, List<TheoreticalProteoform>>
            {
                { Lollipop.decoy_database_name_prefix + "0", new List<TheoreticalProteoform>() { pf1 } },
                { Lollipop.decoy_database_name_prefix + "1", new List<TheoreticalProteoform>() { pf2 } }
            };
            string[] decoy_proteoform_results = Results.theoretical_proteoforms_results(false).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.AreEqual(3, decoy_proteoform_results.Length);
            Lollipop.proteoform_community.decoy_proteoforms.Clear();
            Results.read_theoretical_proteoforms(decoy_proteoform_results);
            Assert.IsTrue(Lollipop.proteoform_community.decoy_proteoforms.ContainsKey(Lollipop.decoy_database_name_prefix + "0"));
            Assert.IsTrue(Lollipop.proteoform_community.decoy_proteoforms.ContainsKey(Lollipop.decoy_database_name_prefix + "1"));
            TheoreticalProteoform qf1 = Lollipop.proteoform_community.decoy_proteoforms[Lollipop.decoy_database_name_prefix + "0"].First();
            TheoreticalProteoform qf2 = Lollipop.proteoform_community.decoy_proteoforms[Lollipop.decoy_database_name_prefix + "1"].First();
            compare_theoreticals(pf1, qf1);
            compare_theoreticals(pf2, qf2);
        }

        private void compare_theoreticals(TheoreticalProteoform pf, TheoreticalProteoform qf)
        {
            Assert.AreEqual(pf.accession, qf.accession);
            Assert.AreEqual(pf.name, qf.name);
            Assert.AreEqual(pf.description, qf.description);
            Assert.AreEqual(pf.fragment, qf.fragment);
            Assert.AreEqual(pf.begin, qf.begin);
            Assert.AreEqual(pf.end, qf.end);
            Assert.AreEqual(pf.unmodified_mass, qf.unmodified_mass);
            Assert.AreEqual(pf.modified_mass, qf.modified_mass);
            Assert.AreEqual(pf.lysine_count, qf.lysine_count);
            Assert.AreEqual(pf.is_target, qf.is_target);
            Assert.AreEqual(pf.is_decoy, qf.is_decoy);
            Assert.AreEqual(pf.ptm_mass, qf.ptm_mass);
            Assert.AreEqual(pf.ptm_list_string(), qf.ptm_list_string());
        }
    }
}
