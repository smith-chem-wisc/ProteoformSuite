using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    class TestExperimentalProteoform
    {
        public static double starter_mass = 1000.0;
        public static double starter_intensity = 100.0;
        public static double starter_rt = 50.0;
        public static int starter_lysine_count = 3;
        List<Component> empty_quant_components_list = new List<Component>();

        // The calculations for unlabeled and neucode components are the same, currently
        public static List<IAggregatable> generate_neucode_components(double starter_mass)
        {
            List<IAggregatable> components = new List<IAggregatable>();
            InputFile inFile = new InputFile("somepath", Labeling.NeuCode, Purpose.Identification);
            
            for (int i = 0; i < 2; i++)
            {
                Component light = new Component();
                Component heavy = new Component();
                light.input_file = inFile;
                heavy.input_file = inFile;
                light.id = 1.ToString();
                heavy.id = 2.ToString();
                light.weighted_monoisotopic_mass = starter_mass;
                heavy.weighted_monoisotopic_mass = starter_mass + starter_lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT;
                light.intensity_sum = starter_intensity; //using the special intensity sum for overlapping charge states in a neucode pair
                heavy.intensity_sum = starter_intensity / 2; //using the special intensity sum for overlapping charge states in a neucode pair
                light.rt_apex = starter_rt;
                heavy.rt_apex = starter_rt;
                light.accepted = true;
                heavy.accepted = true;
                ChargeState light_charge_state = new ChargeState(1, light.intensity_sum, light.weighted_monoisotopic_mass, 1.00727645D);
                ChargeState heavy_charge_state = new ChargeState(1, heavy.intensity_sum, heavy.weighted_monoisotopic_mass, 1.00727645D);
                light.charge_states = new List<ChargeState> { light_charge_state };
                heavy.charge_states = new List<ChargeState> { heavy_charge_state };
                NeuCodePair n = new NeuCodePair(light, light.intensity_sum, heavy, heavy.intensity_sum, 0, new List<int>(), true);
                n.lysine_count = starter_lysine_count;
                components.Add(n);
            }
            return components;
        }

        public static List<Component> generate_neucode_quantitative_components()
        {
            List<Component> components = new List<Component>();
            InputFile inFile = new InputFile("somepath", Labeling.NeuCode, Purpose.Quantification);

            Component light = new Component();
            Component heavy = new Component();
            light.input_file = inFile;
            heavy.input_file = inFile;
            light.id = 1.ToString();
            heavy.id = 2.ToString();
            light.weighted_monoisotopic_mass = starter_mass;
            heavy.weighted_monoisotopic_mass = starter_mass + starter_lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT;
            light.intensity_sum = starter_intensity; //using the special intensity sum for overlapping charge states in a neucode pair
            heavy.intensity_sum = starter_intensity / 2;
            light.rt_apex = starter_rt;
            heavy.rt_apex = starter_rt;
            light.accepted = true;
            heavy.accepted = true;
            components.Add(light);
            components.Add(heavy);
            light.calculate_properties();
            heavy.calculate_properties();
            return components;
        }

        public static List<IAggregatable> generate_unlabeled_components(double starter_mass)
        {
            List<IAggregatable> components = new List<IAggregatable>();
            for (int i = 0; i < 2; i++)
            {
                Component c = new Component();
                c.id = i.ToString();
                c.weighted_monoisotopic_mass = starter_mass;
                c.intensity_sum = starter_intensity;
                c.rt_apex = starter_rt;
                c.accepted = true;
                c.input_file = new InputFile("fake.txt", Purpose.Identification);
                components.Add(c);
            }
            return components;
        }

        [Test]
        public void neucode_proteoform_calculate_properties()
        {
            Sweet.lollipop.neucode_labeled = true;
            List<IAggregatable> components = generate_neucode_components(starter_mass);        
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated.Count);
            Assert.AreEqual(3, e.lysine_count);
            double expected_agg_intensity = components.Count * starter_intensity;
            Assert.AreEqual(expected_agg_intensity, e.agg_intensity);
            double intensity_normalization_factor = components.Count * starter_intensity / expected_agg_intensity;
            double expected_agg_mass = starter_mass * intensity_normalization_factor;
            Assert.AreEqual(starter_mass * intensity_normalization_factor, e.agg_mass);
            Assert.AreEqual(starter_rt * intensity_normalization_factor, e.agg_rt);
        }

        [Test]
        public void unlabeled_proteoform_calculate_properties()
        {
            Sweet.lollipop.min_num_bioreps = 0;
            Sweet.lollipop.neucode_labeled = false;
            List<IAggregatable> components = generate_unlabeled_components(starter_mass);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated.Count);
            double expected_agg_intensity = components.Count * starter_intensity;
            Assert.AreEqual(expected_agg_intensity, e.agg_intensity);
            double intensity_normalization_factor = components.Count * starter_intensity / expected_agg_intensity;
            double expected_agg_mass = starter_mass * intensity_normalization_factor;
            Assert.AreEqual(starter_mass * intensity_normalization_factor, e.agg_mass);
            Assert.AreEqual(starter_rt * intensity_normalization_factor, e.agg_rt);
        }

        public void aggregate_mass_corrects_for_monoisotopic_errors()
        {
            //Make a monoisotopic error, and test that it removes it before aggregation
            List<IAggregatable> components = generate_neucode_components(starter_mass);
            components[4].weighted_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            double expected_agg_intensity = components.Count * starter_intensity;
            double intensity_normalization_factor = components.Count * starter_intensity / expected_agg_intensity;
            double expected_agg_mass = starter_mass * intensity_normalization_factor;
            Assert.AreEqual(expected_agg_mass, e.agg_mass);
        }

        [Test]
        public void aggregate_outside_rt_tolerance()
        {
            Sweet.lollipop.neucode_labeled = true;
            List<IAggregatable> components = generate_neucode_components(starter_mass);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            components[1].rt_apex = starter_rt - Sweet.lollipop.retention_time_tolerance - 1;
            e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated.Count);
            components[1].rt_apex = starter_rt + Sweet.lollipop.retention_time_tolerance + 1;
            e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated.Count);
        }

        [Test]
        public void aggregate_outside_lysine_count_tolerance()
        {
            Sweet.lollipop.neucode_labeled = true;
            List<IAggregatable> components = generate_neucode_components(starter_mass);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            ((NeuCodePair)components[1]).lysine_count = starter_lysine_count + Sweet.lollipop.maximum_missed_lysines + 1;
            e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated.Count);
            ((NeuCodePair)components[1]).lysine_count = starter_lysine_count - Sweet.lollipop.maximum_missed_lysines - 1;
            e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated.Count);
        }


        //Maximum number of missed
        public static int missed_monoisotopics = Sweet.lollipop.maximum_missed_monos;

        [Test]
        public void aggregate_in_bounds_monoisotopic_tolerance()
        {
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-missed_monoisotopics, missed_monoisotopics * 2 + 1).ToList();
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Sweet.lollipop.neucode_labeled = true;
            List<IAggregatable> components = generate_neucode_components(starter_mass);

            // in bounds lowest monoisotopic error
            components[1].charge_states.Clear(); // must clear charge states because you can't set the weighted monoisotopic mass if there are charge states.
            components[1].weighted_monoisotopic_mass = min_monoisotopic_mass - min_monoisotopic_mass / 1000000 * Convert.ToDouble(Sweet.lollipop.mass_tolerance);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated.Count);
            // in bounds highest monoisotopic error
            components[1].weighted_monoisotopic_mass = max_monoisotopic_mass + max_monoisotopic_mass / 1000000 * Convert.ToDouble(Sweet.lollipop.mass_tolerance);
            e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated.Count);
        }

        [Test]
        public void aggregate_out_of_monoisotpic_tolerance()
        {
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-missed_monoisotopics, missed_monoisotopics * 2 + 1).ToList();
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Sweet.lollipop.neucode_labeled = true;
            List<IAggregatable> components = generate_neucode_components(starter_mass);

            // below lowest monoisotopic tolerance
            components[1].charge_states.Clear(); // must clear charge states because you can't set the weighted monoisotopic mass if there are charge states.
            components[1].weighted_monoisotopic_mass = min_monoisotopic_mass - 100;
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated.Count);
            // above highest monoisotopic tolerance
            components[1].weighted_monoisotopic_mass = (max_monoisotopic_mass + 100);
            e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated.Count);
        }

        [Test]
        public void aggregate_in_bounds_middle_monoisotopic_tolerance()
        {
            missed_monoisotopics -= 1;
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-missed_monoisotopics, missed_monoisotopics * 2 + 1).ToList();
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Sweet.lollipop.neucode_labeled = true;
            List<IAggregatable> components = generate_neucode_components(starter_mass);

            // in bounds lowest monoisotopic error
            components[1].charge_states.Clear(); // must clear charge states because you can't set the weighted monoisotopic mass if there are charge states.
            components[1].weighted_monoisotopic_mass = (min_monoisotopic_mass - min_monoisotopic_mass / 1000000 * Convert.ToDouble(Sweet.lollipop.mass_tolerance));
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated.Count);
            // in bounds highest monoisotopic error
            components[1].weighted_monoisotopic_mass = (max_monoisotopic_mass + max_monoisotopic_mass / 1000000 * Convert.ToDouble(Sweet.lollipop.mass_tolerance));
            e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated.Count);
        }

        [Test]
        public void aggregate_out_of_middle_monoisotopic_tolerance()
        {
            missed_monoisotopics -= 1;
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-missed_monoisotopics, missed_monoisotopics * 2 + 1).ToList();
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Sweet.lollipop.neucode_labeled = true;
            List<IAggregatable> components = generate_neucode_components(starter_mass);

            // below lowest monoisotopic tolerance
            components[1].charge_states.Clear(); // must clear charge states because you can't set the weighted monoisotopic mass if there are charge states.
            components[1].weighted_monoisotopic_mass = (min_monoisotopic_mass - min_monoisotopic_mass / 1000000 * Convert.ToDouble(Sweet.lollipop.mass_tolerance) - Double.MinValue);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated.Count);
            // above highest monoisotopic tolerance
            components[1].weighted_monoisotopic_mass = (max_monoisotopic_mass + max_monoisotopic_mass / 1000000 * Convert.ToDouble(Sweet.lollipop.mass_tolerance) + Double.MinValue);
            e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated.Count);
        }

        [Test]
        public void aggregate_just_the_root()
        {
            List<IAggregatable> components = generate_neucode_components(starter_mass);
            components.Remove(components[1]);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(1, e.aggregated.Count);
            Assert.AreEqual(components[0], e.aggregated.First());
        }

        [Test]
        public void test_aggregate_copy()
        {
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Sweet.lollipop.neucode_labeled = true;
            List<IAggregatable> components = generate_neucode_components(starter_mass);

            // in bounds lowest monoisotopic error
            components[1].charge_states.Clear(); // must clear charge states because you can't set the weighted monoisotopic mass if there are charge states.
            components[1].weighted_monoisotopic_mass = min_monoisotopic_mass - min_monoisotopic_mass / 1000000 * Convert.ToDouble(Sweet.lollipop.mass_tolerance);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            Assert.AreEqual(2, e.aggregated.Count);
            // in bounds highest monoisotopic error
            components[1].weighted_monoisotopic_mass = max_monoisotopic_mass + max_monoisotopic_mass / 1000000 * Convert.ToDouble(Sweet.lollipop.mass_tolerance);
            e = ConstructorsForTesting.ExperimentalProteoform("E1", components[0], components, empty_quant_components_list, true);
            e.manual_validation_id = "something";
            e.manual_validation_quant = "something";
            e.manual_validation_verification = "something";
            ExperimentalProteoform f = new ExperimentalProteoform(e);
            Assert.AreEqual(e.root, f.root);
            Assert.AreEqual(e.agg_intensity, f.agg_intensity);
            Assert.AreEqual(e.agg_mass, f.agg_mass);
            Assert.AreEqual(e.modified_mass, f.modified_mass);
            Assert.AreEqual(e.agg_rt, f.agg_rt);
            Assert.AreEqual(e.lysine_count, f.lysine_count);
            Assert.AreEqual(e.accepted, f.accepted);
            Assert.AreEqual("E1", f.quant.accession);
            Assert.AreEqual(e.mass_shifted, f.mass_shifted);
            Assert.AreEqual(e.is_target, f.is_target);
            Assert.AreEqual(e.family, f.family);
            Assert.AreEqual(e.manual_validation_id, f.manual_validation_id);
            Assert.AreEqual(e.manual_validation_quant, f.manual_validation_quant);
            Assert.AreEqual(e.manual_validation_verification, f.manual_validation_verification);
            Assert.AreNotEqual(e.aggregated.GetHashCode(), f.aggregated.GetHashCode());
            Assert.AreEqual(e.aggregated.Count, f.aggregated.Count);
            Assert.AreNotEqual(e.lt_quant_components.GetHashCode(), f.lt_quant_components.GetHashCode());
            Assert.AreEqual(e.lt_quant_components.Count, f.lt_quant_components.Count);
            Assert.AreNotEqual(e.lt_verification_components.GetHashCode(), f.lt_verification_components.GetHashCode());
            Assert.AreEqual(e.lt_verification_components.Count, f.lt_verification_components.Count);
            Assert.AreNotEqual(e.hv_quant_components.GetHashCode(), f.hv_quant_components.GetHashCode());
            Assert.AreEqual(e.hv_quant_components.Count, f.hv_quant_components.Count);
            Assert.AreNotEqual(e.hv_verification_components.GetHashCode(), f.hv_verification_components.GetHashCode());
            Assert.AreEqual(e.hv_verification_components.Count, f.hv_verification_components.Count);
            Assert.AreNotEqual(e.biorepIntensityList.GetHashCode(), f.biorepIntensityList.GetHashCode());
            Assert.AreEqual(e.biorepIntensityList.Count, f.biorepIntensityList.Count);
        }
    }
}
