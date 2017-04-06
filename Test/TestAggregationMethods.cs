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
    class TestAggregationMethods
    {
        [Test]
        public void choose_next_agg_component()
        {
            Component c = new Component();
            Component d = new Component();
            Component e = new Component();
            Component f = new Component();
            c.weighted_monoisotopic_mass = 100;
            d.weighted_monoisotopic_mass = 119;
            e.weighted_monoisotopic_mass = 121;
            f.weighted_monoisotopic_mass = 122;
            c.intensity_sum_olcs = 1;
            d.intensity_sum_olcs = 2;
            e.intensity_sum_olcs = 3;
            f.intensity_sum_olcs = 4;
            List<Component> ordered = new List<Component> { c, d, e, f }.OrderByDescending(cc => cc.intensity_sum_olcs).ToList();
            Component is_running = new Component();
            is_running.weighted_monoisotopic_mass = 100;
            is_running.intensity_sum_olcs = 100;

            //Based on components
            List<Component> active = new List<Component> { is_running };
            Component next = Lollipop.find_next_root(ordered, active);
            Assert.True(Math.Abs(next.weighted_monoisotopic_mass - is_running.weighted_monoisotopic_mass) > 2 * (double)Lollipop.missed_monos);
            Assert.AreEqual(4, next.intensity_sum_olcs);

            //Based on experimental proteoforms
            ExperimentalProteoform exp = ConstructorsForTesting.ExperimentalProteoform("E");
            exp.root = is_running;
            List<ExperimentalProteoform> active2 = new List<ExperimentalProteoform> { exp };
            Component next2 = Lollipop.find_next_root(ordered, active2);
            Assert.True(Math.Abs(next.weighted_monoisotopic_mass - is_running.weighted_monoisotopic_mass) > 2 * (double)Lollipop.missed_monos);
            Assert.AreEqual(4, next.intensity_sum_olcs);
        }

        [Test]
        public void choose_next_exp_proteoform()
        {
            ExperimentalProteoform c = ConstructorsForTesting.ExperimentalProteoform("E");
            ExperimentalProteoform d = ConstructorsForTesting.ExperimentalProteoform("E");
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            ExperimentalProteoform f = ConstructorsForTesting.ExperimentalProteoform("E");
            c.agg_mass = 100;
            d.agg_mass = 119;
            e.agg_mass = 121;
            f.agg_mass = 122;
            c.agg_intensity = 1;
            d.agg_intensity = 2;
            e.agg_intensity = 3;
            f.agg_intensity = 4;
            List<ExperimentalProteoform> ordered = new List<ExperimentalProteoform> { c, d, e, f }.OrderByDescending(cc => cc.agg_intensity).ToList();
            ExperimentalProteoform is_running = ConstructorsForTesting.ExperimentalProteoform("E");
            is_running.agg_mass = 100;
            is_running.agg_intensity = 100;

            List<ExperimentalProteoform> active = new List<ExperimentalProteoform> { is_running };
            ExperimentalProteoform next = Lollipop.find_next_root(ordered, active);
            Assert.True(Math.Abs(next.agg_mass - is_running.agg_mass) > 2 * (double)Lollipop.missed_monos);
            Assert.AreEqual(4, next.agg_intensity);
        }

        [Test]
        public void create_proteoforms_in_bounds_monoisotopic_tolerance()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            List<Component> components = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass);

            Lollipop.neucode_labeled = true;
            List<ExperimentalProteoform> pfs = Lollipop.createProteoforms(components.OfType<NeuCodePair>(), components, 0);
            Assert.AreEqual(1, pfs.Count);
            Assert.AreEqual(2, pfs[0].aggregated_components.Count);
            Assert.AreEqual(2, components.Count);
            Assert.AreEqual(0, Lollipop.remaining_components.Count);
        }

        [Test]
        public void vet_proteoforms_in_bounds_monoisotopic_tolerance()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            IEnumerable<NeuCodePair> neucodes = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass).OfType<NeuCodePair>();

            List<Component> components = neucodes.Select(nc => nc.neuCodeLight).Concat(neucodes.Select(nc => nc.neuCodeHeavy)).ToList();

            // in bounds lowest monoisotopic error
            Lollipop.neucode_labeled = true;
            List<ExperimentalProteoform> pfs = Lollipop.createProteoforms(neucodes, components, 0);
            List<ExperimentalProteoform> vetted = Lollipop.vetExperimentalProteoforms(pfs, components, new List<ExperimentalProteoform>());
            Assert.AreEqual(1, vetted.Count);
            Assert.AreEqual(2, vetted[0].aggregated_components.Count);
            Assert.AreEqual(2, vetted[0].lt_verification_components.Count);
            Assert.AreEqual(2, vetted[0].hv_verification_components.Count);
            Assert.AreEqual(4, components.Count);
            Assert.AreEqual(0, Lollipop.remaining_verification_components.Count);
        }

        [Test]
        public void assign_quant_components_in_bounds_monoisotopic_tolerance()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            IEnumerable<NeuCodePair> neucodes = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass).OfType<NeuCodePair>();
            List<Component> quant_components = TestExperimentalProteoform.generate_neucode_quantitative_components();

            // in bounds lowest monoisotopic error
            Lollipop.neucode_labeled = true;
            List<ExperimentalProteoform> pfs = Lollipop.createProteoforms(neucodes, neucodes,0);
            List<ExperimentalProteoform> vetted_quant = Lollipop.assignQuantificationComponents(pfs, quant_components);
            Assert.AreEqual(1, vetted_quant.Count);
            Assert.AreEqual(2, vetted_quant[0].aggregated_components.Count);
            Assert.AreEqual(1, vetted_quant[0].lt_quant_components.Count);
            Assert.AreEqual(1, vetted_quant[0].hv_quant_components.Count);
            Assert.AreEqual(2, quant_components.Count);
            Assert.AreEqual(0, Lollipop.remaining_quantification_components.Count);
        }


        [Test]
        public void full_agg()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            IEnumerable<NeuCodePair> neucodes = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass).OfType<NeuCodePair>();
            List<Component> components = neucodes.Select(nc => nc.neuCodeLight).Concat(neucodes.Select(nc => nc.neuCodeHeavy)).ToList();
            List<Component> quant_components = TestExperimentalProteoform.generate_neucode_quantitative_components();

            //Must use Lollipop.remaining_components because ThreadStart only uses void methods
            //Must use Lollipop.remaining_components because ThreadStart only uses void methods
            Lollipop.neucode_labeled = true;
            Lollipop.input_files = new List<InputFile> { new InputFile("fake.txt", Purpose.Quantification) };
            List<ExperimentalProteoform> vetted_quant = Lollipop.aggregate_proteoforms(true, neucodes, components, quant_components, 0);
            Assert.AreEqual(1, vetted_quant.Count);
            Assert.AreEqual(2, vetted_quant[0].aggregated_components.Count);
            Assert.AreEqual(2, vetted_quant[0].lt_verification_components.Count);
            Assert.AreEqual(2, vetted_quant[0].hv_verification_components.Count);
            Assert.AreEqual(1, vetted_quant[0].lt_quant_components.Count);
            Assert.AreEqual(1, vetted_quant[0].hv_quant_components.Count);
            Assert.AreEqual(2, quant_components.Count);
            Assert.AreEqual(0, Lollipop.remaining_quantification_components.Count);
        }        
        
        [Test]
        public void full_agg_without_validation()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            IEnumerable<NeuCodePair> neucodes = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass).OfType<NeuCodePair>();
            List<Component> components = neucodes.Select(nc => nc.neuCodeLight).Concat(neucodes.Select(nc => nc.neuCodeHeavy)).ToList();
            List<Component> quant_components = TestExperimentalProteoform.generate_neucode_quantitative_components();

            //Must use Lollipop.remaining_components because ThreadStart only uses void methods
            Lollipop.neucode_labeled = true;
            Lollipop.input_files = new List<InputFile> { new InputFile("fake.txt", Purpose.Quantification) };
            List<ExperimentalProteoform> vetted_quant = Lollipop.aggregate_proteoforms(false, neucodes, components, quant_components, 0);
            Assert.AreEqual(1, vetted_quant.Count);
            Assert.AreEqual(2, vetted_quant[0].aggregated_components.Count);
            Assert.AreEqual(0, vetted_quant[0].lt_verification_components.Count);
            Assert.AreEqual(0, vetted_quant[0].hv_verification_components.Count);
            Assert.AreEqual(1, vetted_quant[0].lt_quant_components.Count);
            Assert.AreEqual(1, vetted_quant[0].hv_quant_components.Count);
            Assert.AreEqual(2, quant_components.Count);
            Assert.AreEqual(0, Lollipop.remaining_quantification_components.Count);
        }

        [Test]
        public void unlabeled_agg()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            List<Component> components = TestExperimentalProteoform.generate_unlabeled_components(TestExperimentalProteoform.starter_mass);

            Lollipop.neucode_labeled = false;
            Lollipop.remaining_components = new List<Component>(components);
            Lollipop.remaining_verification_components = new List<Component>(components);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            e.root = components[0];
            e.aggregate();
            e.verify();

            Assert.AreEqual(2, e.aggregated_components.Count);
            Assert.AreEqual(2, e.lt_verification_components.Count);
            Assert.AreEqual(0, e.hv_verification_components.Count); // everything goes into light with unlabeled
            Assert.AreEqual(0, e.lt_quant_components.Count); // no quantitation for unlabeled, yet
            Assert.AreEqual(0, e.hv_quant_components.Count);
        }

        [Test]
        public void basic_regroup_test()
        {
            Lollipop.raw_neucode_pairs.Add(new NeuCodePair());
            Assert.IsNotEmpty(Lollipop.raw_neucode_pairs);
            Assert.IsEmpty(Lollipop.regroup_components(true, false, new List<InputFile>(), Lollipop.raw_neucode_pairs, Lollipop.raw_experimental_components, Lollipop.raw_quantification_components, 0));
            Assert.IsEmpty(Lollipop.raw_neucode_pairs);
        }
    }
}
