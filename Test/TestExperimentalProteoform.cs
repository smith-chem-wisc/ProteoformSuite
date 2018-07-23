using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Proteomics;


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
                ChargeState light_charge_state = new ChargeState(1, light.intensity_sum, light.weighted_monoisotopic_mass + 1.00727645D);
                ChargeState heavy_charge_state = new ChargeState(1, heavy.intensity_sum, heavy.weighted_monoisotopic_mass + 1.00727645D);
                light.charge_states = new List<ChargeState> { light_charge_state };
                heavy.charge_states = new List<ChargeState> { heavy_charge_state };
                double mass_difference = heavy.weighted_monoisotopic_mass - light.weighted_monoisotopic_mass;
                NeuCodePair n = new NeuCodePair(light, light.intensity_sum, heavy, heavy.intensity_sum, mass_difference, new HashSet<int>() { 1 }, true);
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
            Sweet.lollipop.set_missed_monoisotopic_range();
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
            Sweet.lollipop.set_missed_monoisotopic_range();
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
            Sweet.lollipop.set_missed_monoisotopic_range();
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
            Sweet.lollipop.set_missed_monoisotopic_range();
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
        public void aggregate_consecutive_charge_states()
        {
            List<ChargeState> cs = new List<ChargeState>();
            for(int i = 5; i < 8; i++)
            {
                cs.Add(new ChargeState(i, 1000, 1000));
            }
            Assert.IsTrue(Sweet.lollipop.consecutive_charge_states(3, cs));
            Assert.IsFalse(Sweet.lollipop.consecutive_charge_states(4, cs));
            cs[0].charge_count = 4;
            Assert.IsFalse(Sweet.lollipop.consecutive_charge_states(3, cs));
            Assert.IsTrue(Sweet.lollipop.consecutive_charge_states(2, cs));
            cs[0].charge_count = 5;
            cs[2].charge_count = 9;
            Assert.IsFalse(Sweet.lollipop.consecutive_charge_states(3, cs));
            Assert.IsTrue(Sweet.lollipop.consecutive_charge_states(2, cs));
            cs = new List<ChargeState>() { new ChargeState(1, 1000, 1000) };
            Assert.IsFalse(Sweet.lollipop.consecutive_charge_states(2, cs));
            Assert.IsTrue(Sweet.lollipop.consecutive_charge_states(1, cs));
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

        [Test]
        public void agg_proteoforms_meeting_criteria()
        {
            Sweet.lollipop = new Lollipop();
            string anysingle = "Bioreps From Any Single Condition";
            string any = "Bioreps From Any Condition";
            string fromeach = "Bioreps From Each Condition";

            List<string> conditions = new List<string> { "s", "ns" };
            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { ConstructorsForTesting.ExperimentalProteoform("E") };
            Component c1 = new Component();
            c1.input_file = new InputFile("somePath", Purpose.Identification);
            c1.input_file.lt_condition = conditions[0];
            c1.input_file.biological_replicate = "1";
            exps[0].aggregated.Add(c1);

            List<ExperimentalProteoform> exps_out = new List<ExperimentalProteoform>();

            //PASSES WHEN THERE ARE ENOUGH IN SPECIFIED CONDITIONS
            //One biorep obs passes any-single-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 1);
            Assert.AreEqual(1, exps_out.Count);

            //One biorep obs passes any-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 1);
            Assert.AreEqual(1, exps_out.Count);

            //One biorep obs doesn't pass for-each-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(0, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD
            //One biorep obs doesn't pass 2 from any-single-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            //One biorep obs doesn't pass 2 from any-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(0, exps_out.Count);

            //One biorep obs doesn't pass 2 from for-each-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);

            Component c2 = new Component();
            c2.input_file = new InputFile("somePath", Purpose.Identification);
            c2.input_file.lt_condition = conditions[1];
            c2.input_file.biological_replicate = "100";
            exps[0].aggregated.Add(c2);

            //One biorep in each condition passes for-each-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(1, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD IN SPECIFIC CONDITIONS UNLESS ANY-CONDITION
            //One biorep obs in two different conditions doesn't pass 2 from any-single-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            //One biorep obs in two different conditions passes 2 from any-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            //One biorep obs in two different conditions doesn't pass 2 from for-each-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);


            //DOESN'T PASS WHEN NOT MATCHING LISTED CONDITIONS, EXCEPT FOR ANY-CONDITION
            foreach (var x in exps[0].aggregated)
            {
                x.input_file.lt_condition = "not_a_condition";
            }
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);


            //NOT JUST COUNTING BIOREP INTENSITIES, BUT RATHER BIOREPS WITH OBSERVATIONS
            exps[0].aggregated.Clear();
            Component c3 = new Component();
            c3.input_file = new InputFile("somePath", Purpose.Identification);
            c3.input_file.lt_condition = conditions[0];
            c3.input_file.biological_replicate = "1";
            exps[0].aggregated.Add(c3);
            Component c4 = new Component();
            c4.input_file = new InputFile("somePath", Purpose.Identification);
            c4.input_file.lt_condition = conditions[0];
            c4.input_file.biological_replicate = "1";
            exps[0].aggregated.Add(c4);
            Component c5 = new Component();
            c5.input_file = new InputFile("somePath", Purpose.Identification);
            c5.input_file.lt_condition = conditions[0];
            c5.input_file.biological_replicate = "1";
            exps[0].aggregated.Add(c5);
            Component c6 = new Component();
            c6.input_file = new InputFile("somePath", Purpose.Identification);
            c6.input_file.lt_condition = conditions[0];
            c6.input_file.biological_replicate = "1";
            exps[0].aggregated.Add(c6);
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);

            Component c7 = new Component();
            c7.input_file = new InputFile("somePath", Purpose.Identification);
            c7.input_file.lt_condition = conditions[1];
            c7.input_file.biological_replicate = "1";
            exps[0].aggregated.Add(c7);
            Component c8 = new Component();
            c8.input_file = new InputFile("somePath", Purpose.Identification);
            c8.input_file.lt_condition = conditions[1];
            c8.input_file.biological_replicate = "1";
            exps[0].aggregated.Add(c8);
            Component c9 = new Component();
            c9.input_file = new InputFile("somePath", Purpose.Identification);
            c9.input_file.lt_condition = conditions[1];
            c9.input_file.biological_replicate = "1";
            exps[0].aggregated.Add(c9);
            Component c10 = new Component();
            c10.input_file = new InputFile("somePath", Purpose.Identification);
            c10.input_file.lt_condition = conditions[1];
            c10.input_file.biological_replicate = "1";
            exps[0].aggregated.Add(c10);
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);
        }

        [Test]
        public void agg_proteoforms_meeting_criteria2()
        {
            Sweet.lollipop = new Lollipop();

            string anysingle = "Biorep+Techreps From Any Single Condition";
            string any = "Biorep+Techreps From Any Condition";
            string fromeach = "Biorep+Techreps From Each Condition";
            List<ExperimentalProteoform> exps = new List<ExperimentalProteoform> { ConstructorsForTesting.ExperimentalProteoform("E") };
            List<string> conditions = new List<string> { "s", "ns" };
            Component c1 = new Component();
            c1.input_file = new InputFile("somePath", Purpose.Identification);
            c1.input_file.lt_condition = conditions[0];
            c1.input_file.biological_replicate = "1";
            c1.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c1);
            List<ExperimentalProteoform> exps_out = new List<ExperimentalProteoform>();

            //PASSES WHEN THERE ARE ENOUGH IN SPECIFIED CONDITIONS
            //One biorep+techrep obs passes any-single-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 1);
            Assert.AreEqual(1, exps_out.Count);

            //Two biorep+techrep obs in single condition passes any-single-conditon test
            Component c2 = new Component();
            c2.input_file = new InputFile("somePath", Purpose.Identification);
            c2.input_file.lt_condition = conditions[0];
            c2.input_file.biological_replicate = "1";
            c2.input_file.technical_replicate = "2";
            exps[0].aggregated.Add(c2);
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(1, exps_out.Count);

            //Two biorep+techrep obs in single condition passes any-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            //Two biorep+techrep obs in single condition doesn't pass for-each-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(0, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD
            //Two biorep+techrep obs in single condition doesn't pass 3 from any-single-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 3);
            Assert.AreEqual(0, exps_out.Count);

            //Two biorep+techrep obs in single condition doesn't pass 3 from any-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 3);
            Assert.AreEqual(0, exps_out.Count);

            //Two biorep+techrep obs in single condition  doesn't pass 2 from for-each-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);

            Component c3 = new Component();
            c3.input_file = new InputFile("somePath", Purpose.Identification);
            c3.input_file.lt_condition = conditions[1];
            c3.input_file.biological_replicate = "100";
            c3.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c3);

            //At least one biorep+techrep in each condition passes for-each-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 1);
            Assert.AreEqual(1, exps_out.Count);


            // DOESN'T PASS WHEN LESS THAN THRESHOLD IN SPECIFIC CONDITIONS UNLESS ANY-CONDITION
            //Two and one biorep+techrep obs in two different conditions doesn't pass 3 from any-single-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 3);
            Assert.AreEqual(0, exps_out.Count);

            //Two and one biorep+techrep obs in two different conditions passes 3 from any-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 3);
            Assert.AreEqual(1, exps_out.Count);

            //Two and one biorep+techrep obs in two different conditions doesn't pass 3 from for-each-conditon test
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 3);
            Assert.AreEqual(0, exps_out.Count);


            //DOESN'T PASS WHEN NOT MATCHING LISTED CONDITIONS, EXCEPT FOR ANY-CONDITION
            foreach (var x in exps[0].aggregated) x.input_file.lt_condition = "not_a_condition";
            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 3);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 3);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 3);
            Assert.AreEqual(0, exps_out.Count);



            //NOT JUST COUNTING BIOREP INTENSITIES, BUT RATHER BIOREPS WITH OBSERVATIONS
            exps[0].aggregated.Clear();
            Component c4 = new Component();
            c4.input_file = new InputFile("somePath", Purpose.Identification);
            c4.input_file.lt_condition = conditions[0];
            c4.input_file.biological_replicate = "1";
            c4.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c4);
            Component c5 = new Component();
            c5.input_file = new InputFile("somePath", Purpose.Identification);
            c5.input_file.lt_condition = conditions[0];
            c5.input_file.biological_replicate = "1";
            c5.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c5);
            Component c6 = new Component();
            c6.input_file = new InputFile("somePath", Purpose.Identification);
            c6.input_file.lt_condition = conditions[0];
            c6.input_file.biological_replicate = "1";
            c6.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c6);
            Component c7 = new Component();
            c7.input_file = new InputFile("somePath", Purpose.Identification);
            c7.input_file.lt_condition = conditions[0];
            c7.input_file.biological_replicate = "1";
            c7.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c7);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);

            Component c8 = new Component();
            c8.input_file = new InputFile("somePath", Purpose.Identification);
            c8.input_file.lt_condition = conditions[1];
            c8.input_file.biological_replicate = "1";
            c8.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c8);
            Component c9 = new Component();
            c9.input_file = new InputFile("somePath", Purpose.Identification);
            c9.input_file.lt_condition = conditions[1];
            c9.input_file.biological_replicate = "1";
            c9.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c9);
            Component c10 = new Component();
            c10.input_file = new InputFile("somePath", Purpose.Identification);
            c10.input_file.lt_condition = conditions[1];
            c10.input_file.biological_replicate = "1";
            c10.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c10);
            Component c11 = new Component();
            c11.input_file = new InputFile("somePath", Purpose.Identification);
            c11.input_file.lt_condition = conditions[1];
            c11.input_file.biological_replicate = "1";
            c11.input_file.technical_replicate = "1";
            exps[0].aggregated.Add(c11);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, anysingle, 2);
            Assert.AreEqual(0, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, any, 2);
            Assert.AreEqual(1, exps_out.Count);

            exps_out = Sweet.lollipop.determineAggProteoformsMeetingCriteria(conditions, exps, fromeach, 2);
            Assert.AreEqual(0, exps_out.Count);
        }
    }
}
