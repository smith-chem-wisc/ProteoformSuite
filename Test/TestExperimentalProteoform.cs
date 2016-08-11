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
        [Test]
        public void test_proteoform_aggregation_and_calculate_properties()
        {
            // All within tolerances of the root component; should be the root plus four
            List<Component> components = new List<Component>();
            double starter_neucode_intensity = 100.0;
            double starter_mass_base = 1000.0d;
            double starter_mass_decimal = 0.1d;
            double starter_mass = starter_mass_base + starter_mass_decimal;
            double starter_rt = 50.0;
            int starter_lysine_count = 3;
            for (int i = 0; i < 5; i++)
            {
                Component light = new Component();
                Component heavy = new Component();
                light.id = 1;
                heavy.id = 2;
                light.corrected_mass = starter_mass;
                light.intensity_sum_olcs = starter_neucode_intensity; //using the special intensity sum for overlapping charge states in a neucode pair
                light.rt_apex = starter_rt;
                light.accepted = true;
                NeuCodePair n = new NeuCodePair(light, heavy);
                n.lysine_count = starter_lysine_count;
                components.Add(n);
            }
            Lollipop.neucode_labeled = true;
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(5, e.aggregated_components.Count);
            Assert.AreEqual(3, e.lysine_count);
            double expected_agg_intensity = components.Count * starter_neucode_intensity;
            Assert.AreEqual(expected_agg_intensity, e.agg_intensity);
            double intensity_normalization_factor = components.Count * starter_neucode_intensity / expected_agg_intensity;
            double expected_agg_mass = starter_mass * intensity_normalization_factor;
            Assert.AreEqual(starter_mass * intensity_normalization_factor, e.agg_mass);
            Assert.AreEqual(starter_rt * intensity_normalization_factor, e.agg_rt);

            // One within tolerance of the root component, each of the other three are outside for 3 different reasons; should be the root plus one
            components[2].rt_apex = starter_rt - Convert.ToDouble(Lollipop.retention_time_tolerance) - 1;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(4, e.aggregated_components.Count);
            components[2].rt_apex = starter_rt + Convert.ToDouble(Lollipop.retention_time_tolerance) + 1;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(4, e.aggregated_components.Count);

            ((NeuCodePair)components[3]).lysine_count = starter_lysine_count + Convert.ToInt32(Lollipop.missed_lysines) + 1;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(3, e.aggregated_components.Count);
            ((NeuCodePair)components[3]).lysine_count = starter_lysine_count - Convert.ToInt32(Lollipop.missed_lysines) - 1;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(3, e.aggregated_components.Count);

            // Maximum number of missed
            int missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            double max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            //Make a monoisotopic error, and test that it removes it before aggregation
            components[4].corrected_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(expected_agg_mass, e.agg_mass);

            // in bounds of lowest monoisotopic tolerance
            components[4].corrected_mass = min_monoisotopic_mass - min_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance);
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(3, e.aggregated_components.Count);
            // in bounds of highest monoisotopic tolerance
            components[4].corrected_mass = max_monoisotopic_mass + max_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance);
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(3, e.aggregated_components.Count);
            // below lowest monoisotopic tolerance
            components[4].corrected_mass = min_monoisotopic_mass - 100;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(2, e.aggregated_components.Count);
            // above highest monoisotopic tolerance
            components[4].corrected_mass = max_monoisotopic_mass + 100;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(2, e.aggregated_components.Count);

            // Middle of the range of number of missed monoisotopics allowed
            missed_monoisotopics -= 1;
            max_monoisotopic_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            min_monoisotopic_mass = starter_mass - missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            // in bounds of lowest monoisotopic tolerance
            components[4].corrected_mass = min_monoisotopic_mass - min_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance);
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(3, e.aggregated_components.Count);
            // in bounds of highest monoisotopic tolerance
            components[4].corrected_mass = max_monoisotopic_mass + max_monoisotopic_mass / 1000000 * Convert.ToDouble(Lollipop.mass_tolerance);
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(3, e.aggregated_components.Count);
            // below lowest monoisotopic tolerance
            components[4].corrected_mass = min_monoisotopic_mass - 100;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(2, e.aggregated_components.Count);
            // above highest monoisotopic tolerance
            components[4].corrected_mass = max_monoisotopic_mass + 100;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(2, e.aggregated_components.Count);            


            // None within tolerance; should just be the root
            components.Remove(components[1]);
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(1, e.aggregated_components.Count);
            Assert.AreEqual(components[0], e.aggregated_components.First());
        }

        [Test]
        public void test_proteoform_calculate_properties_unlabeled()
        {
            // All within tolerances of the root component; should be the root plus four
            List<Component> components = new List<Component>();
            double starter_unlabeled_intensity = 100.0;
            double starter_mass = 1000.0;
            double starter_rt = 50.0;
            for (int i = 0; i < 5; i++)
            {
                Component c = new Component();
                c.corrected_mass = starter_mass;
                c.rt_apex = starter_rt;
                c.intensity_sum_olcs = starter_unlabeled_intensity; //using only the regular intensity_sum
                c.accepted = true;
                components.Add(c);
            }
            // One within tolerance of the root component, each of the other three are outside for 3 different reasons; should be the root plus one
            Lollipop.neucode_labeled = false;
            ExperimentalProteoform e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(5, e.aggregated_components.Count);
            double expected_agg_intensity = components.Count * starter_unlabeled_intensity;
            Assert.AreEqual(expected_agg_intensity, e.agg_intensity);
            double intensity_normalization_factor = starter_unlabeled_intensity * components.Count / expected_agg_intensity;
            double expected_agg_mass = starter_mass * intensity_normalization_factor;
            Assert.AreEqual(starter_mass * intensity_normalization_factor, e.agg_mass);
            Assert.AreEqual(starter_rt * intensity_normalization_factor, e.agg_rt);

            //Make a monoisotopic error, and test that it removes it before aggregation
            int missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            components[4].corrected_mass = starter_mass + missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            e = new ExperimentalProteoform("E1", components[0], components, true);
            Assert.AreEqual(expected_agg_mass, e.agg_mass);
        }
    }
}
