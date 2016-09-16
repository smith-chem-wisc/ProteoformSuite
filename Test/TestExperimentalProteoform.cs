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
    class TestExperimentalProteoform
    {
        double starter_mass = 1000.0;
        double starter_neucode_intensity = 100.0;
        double starter_rt = 50.0;
        int starter_lysine_count = 3;
        List<Component> empty_quant_components_list = new List<Component>();

        // The calculations for unlabeled and neucode components are the same, currently
        public List<Component> generate_neucode_components()
        {
            List<Component> components = new List<Component>();
            InputFile inFile = new ProteoformSuiteInternal.InputFile("somepath", Labeling.NeuCode, Purpose.Identification);
            
            for (int i = 0; i < 2; i++)
            {
                Component light = new Component();
                Component heavy = new Component();
                light.input_file = inFile;
                heavy.input_file = inFile;
                light.id = 1;
                heavy.id = 2;
                light.corrected_mass = starter_mass;
                heavy.corrected_mass = starter_mass + starter_lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT;
                light.intensity_sum_olcs = starter_neucode_intensity; //using the special intensity sum for overlapping charge states in a neucode pair
                light.rt_apex = starter_rt;
                heavy.rt_apex = starter_rt;
                light.accepted = true;
                heavy.accepted = true;
                NeuCodePair n = new NeuCodePair(light, heavy);
                n.lysine_count = starter_lysine_count;
                components.Add(n);
            }
            return components;
        }

        public List<Component> generate_neucode_quantitative_components()
        {
            List<Component> components = new List<Component>();
            InputFile inFile = new ProteoformSuiteInternal.InputFile("somepath", Labeling.NeuCode, Purpose.Quantification);

            Component light = new Component();
            Component heavy = new Component();
            light.input_file = inFile;
            heavy.input_file = inFile;
            light.id = 1;
            heavy.id = 2;
            light.corrected_mass = starter_mass;
            heavy.corrected_mass = starter_mass + starter_lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT;
            light.intensity_sum = starter_neucode_intensity; //using the special intensity sum for overlapping charge states in a neucode pair
            heavy.intensity_sum = starter_neucode_intensity / 2;
            light.rt_apex = starter_rt;
            heavy.rt_apex = starter_rt;
            light.accepted = true;
            heavy.accepted = true;
            //NeuCodePair n = new NeuCodePair(light, heavy);
            //n.lysine_count = starter_lysine_count;
            components.Add(light);
            components.Add(heavy);
            return components;
        }

        public List<Component> generate_unlabeled_components()
        {
            List<Component> components = new List<Component>();
            for (int i = 0; i < 2; i++)
            {
                Component c = new Component();
                c.id = 1;
                c.corrected_mass = starter_mass;
                c.intensity_sum_olcs = starter_neucode_intensity;
                c.rt_apex = starter_rt;
                c.accepted = true;
                components.Add(c);
            }
            return components;
        }

        [Test]
        public void neucode_quantification()
        {
            Lollipop.neucode_labeled = true;
            List<Component> quant_components_list = generate_neucode_quantitative_components();
            List<Component> components = generate_neucode_components();
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, quant_components_list, true);
            List<InputFile> inFileList = new List<InputFile>();
            quant_components_list.ForEach(q => {
                inFileList.Add(q.input_file);
            });

            weightedRatioIntensityVariance wRAWV  = e.weightedRatioAndWeightedVariance(inFileList.DistinctBy(x => x.UniqueId).ToList());
            Assert.AreEqual(1, Math.Round(wRAWV.ratio)); //Ratio
            Assert.AreEqual(150, wRAWV.intensity);
            //Assert.AreEqual(0, wRAWV.variance); //Variance
        }

        [Test]
        public void neucode_proteoform_calculate_properties()
        {
            Lollipop.neucode_labeled = true;
            List<Component> components = generate_neucode_components();        
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated_components.Count);
            Assert.AreEqual(3, e.lysine_count);
            double expected_agg_intensity = components.Count * starter_neucode_intensity;
            Assert.AreEqual(expected_agg_intensity, e.agg_intensity);
            double intensity_normalization_factor = components.Count * starter_neucode_intensity / expected_agg_intensity;
            double expected_agg_mass = starter_mass * intensity_normalization_factor;
            Assert.AreEqual(starter_mass * intensity_normalization_factor, e.agg_mass);
            Assert.AreEqual(starter_rt * intensity_normalization_factor, e.agg_rt);
        }

        [Test]
        public void unlabeled_proteoform_calculate_properties()
        {
            Lollipop.neucode_labeled = false;
            List<Component> components = generate_unlabeled_components();
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated_components.Count);
            double expected_agg_intensity = components.Count * starter_neucode_intensity;
            Assert.AreEqual(expected_agg_intensity, e.agg_intensity);
            double intensity_normalization_factor = components.Count * starter_neucode_intensity / expected_agg_intensity;
            double expected_agg_mass = starter_mass * intensity_normalization_factor;
            Assert.AreEqual(starter_mass * intensity_normalization_factor, e.agg_mass);
            Assert.AreEqual(starter_rt * intensity_normalization_factor, e.agg_rt);
        }

        public void aggregate_mass_corrects_for_monoisotopic_errors()
        {
            //Make a monoisotopic error, and test that it removes it before aggregation
            List<Component> components = generate_neucode_components();
            components[4].corrected_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            double expected_agg_intensity = components.Count * starter_neucode_intensity;
            double intensity_normalization_factor = components.Count * starter_neucode_intensity / expected_agg_intensity;
            double expected_agg_mass = starter_mass * intensity_normalization_factor;
            Assert.AreEqual(expected_agg_mass, e.agg_mass);
        }

        [Test]
        public void aggregate_outside_rt_tolerance()
        {
            Lollipop.neucode_labeled = true;
            List<Component> components = generate_neucode_components();
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            components[1].rt_apex = starter_rt - Convert.ToDouble(Lollipop.retention_time_tolerance) - 1;
            e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
            components[1].rt_apex = starter_rt + Convert.ToDouble(Lollipop.retention_time_tolerance) + 1;
            e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
        }

        [Test]
        public void aggregate_outside_lysine_count_tolerance()
        {
            Lollipop.neucode_labeled = true;
            List<Component> components = generate_neucode_components();
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            ((NeuCodePair)components[1]).lysine_count = starter_lysine_count + Convert.ToInt32(Lollipop.missed_lysines) + 1;
            e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
            ((NeuCodePair)components[1]).lysine_count = starter_lysine_count - Convert.ToInt32(Lollipop.missed_lysines) - 1;
            e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
        }

        [Test]
        public void quantification()
        {

        }

        //Maximum number of missed
        int missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);

        [Test]
        public void aggregate_in_bounds_monoisotopic_tolerance()
        {
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Lollipop.neucode_labeled = true;
            List<Component> components = generate_neucode_components();

            // in bounds lowest monoisotopic error
            components[1].corrected_mass = min_monoisotopic_mass - min_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance);
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated_components.Count);
            // in bounds highest monoisotopic error
            components[1].corrected_mass = max_monoisotopic_mass + max_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance);
            e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated_components.Count);
        }

        [Test]
        public void aggregate_out_of_monoisotpic_tolerance()
        {
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Lollipop.neucode_labeled = true;
            List<Component> components = generate_neucode_components();

            // below lowest monoisotopic tolerance
            components[1].corrected_mass = min_monoisotopic_mass - 100;
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
            // above highest monoisotopic tolerance
            components[1].corrected_mass = max_monoisotopic_mass + 100;
            e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
        }

        public void aggregate_in_bounds_middle_monoisotopic_tolerance()
        {
            missed_monoisotopics -= 1;
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Lollipop.neucode_labeled = true;
            List<Component> components = generate_neucode_components();

            // in bounds lowest monoisotopic error
            components[1].corrected_mass = min_monoisotopic_mass - min_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance);
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated_components.Count);
            // in bounds highest monoisotopic error
            components[1].corrected_mass = max_monoisotopic_mass + max_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance);
            e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated_components.Count);
        }

        public void aggregate_out_of_middle_monoisotopic_tolerance()
        {
            missed_monoisotopics -= 1;
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Lollipop.neucode_labeled = true;
            List<Component> components = generate_neucode_components();

            // below lowest monoisotopic tolerance
            components[1].corrected_mass = min_monoisotopic_mass - min_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance) - Double.MinValue;
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
            // above highest monoisotopic tolerance
            components[1].corrected_mass = max_monoisotopic_mass + max_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance) + Double.MinValue;
            e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
        }

        public void aggregate_just_the_root()
        {
            List<Component> components = generate_neucode_components();
            components.Remove(components[1]);
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
            Assert.AreEqual(components[0], e.aggregated_components.First());
        }

        
    }
}
