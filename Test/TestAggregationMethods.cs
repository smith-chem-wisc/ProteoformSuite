using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;

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
            c.intensity_sum = 1;
            d.intensity_sum = 2;
            e.intensity_sum = 3;
            f.intensity_sum = 4;
            List<IAggregatable> ordered = new List<IAggregatable> { c, d, e, f }.OrderByDescending(cc => cc.intensity_sum).ToList();
            Component is_running = new Component();
            is_running.weighted_monoisotopic_mass = 100;
            is_running.intensity_sum = 100;

            //Based on components
            List<IAggregatable> active = new List<IAggregatable> { is_running };
            IAggregatable next = Sweet.lollipop.find_next_root(ordered, active);
            Assert.True(Math.Abs(next.weighted_monoisotopic_mass - is_running.weighted_monoisotopic_mass) > 2 * (double)Sweet.lollipop.maximum_missed_monos);
            Assert.AreEqual(4, next.intensity_sum);

            //Based on experimental proteoforms
            ExperimentalProteoform exp = ConstructorsForTesting.ExperimentalProteoform("E");
            exp.root = is_running;
            List<ExperimentalProteoform> active2 = new List<ExperimentalProteoform> { exp };
            IAggregatable next2 = Sweet.lollipop.find_next_root(ordered, active2);
            Assert.True(Math.Abs(next.weighted_monoisotopic_mass - is_running.weighted_monoisotopic_mass) > 2 * (double)Sweet.lollipop.maximum_missed_monos);
            Assert.AreEqual(4, next.intensity_sum);
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
            ExperimentalProteoform next = Sweet.lollipop.find_next_root(ordered, active);
            Assert.True(Math.Abs(next.agg_mass - is_running.agg_mass) > 2 * (double)Sweet.lollipop.maximum_missed_monos);
            Assert.AreEqual(4, next.agg_intensity);
        }

        [Test]
        public void create_proteoforms_in_bounds_monoisotopic_tolerance()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            List<IAggregatable> components = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass);

            Sweet.lollipop.neucode_labeled = true;
            List<ExperimentalProteoform> pfs = Sweet.lollipop.createProteoforms(components.OfType<NeuCodePair>(), components.OfType<Component>(), 0);
            Assert.AreEqual(1, pfs.Count);
            Assert.AreEqual(2, pfs[0].aggregated.Count);
            Assert.AreEqual(2, components.Count);
            Assert.AreEqual(0, Sweet.lollipop.remaining_to_aggregate.Count);
        }

        [Test]
        public void vet_proteoforms_in_bounds_monoisotopic_tolerance()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            IEnumerable<NeuCodePair> neucodes = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass).OfType<NeuCodePair>();

            List<Component> components = neucodes.Select(nc => nc.neuCodeLight).Concat(neucodes.Select(nc => nc.neuCodeHeavy)).ToList();

            // in bounds lowest monoisotopic error
            Sweet.lollipop.neucode_labeled = true;
            List<ExperimentalProteoform> pfs = Sweet.lollipop.createProteoforms(neucodes, components, 0);
            List<ExperimentalProteoform> vetted = Sweet.lollipop.vetExperimentalProteoforms(pfs, components, new List<ExperimentalProteoform>());
            Assert.AreEqual(1, vetted.Count);
            Assert.AreEqual(2, vetted[0].aggregated.Count);
            Assert.AreEqual(2, vetted[0].lt_verification_components.Count);
            Assert.AreEqual(2, vetted[0].hv_verification_components.Count);
            Assert.AreEqual(4, components.Count);
            Assert.AreEqual(0, Sweet.lollipop.remaining_verification_components.Count);
        }

        [Test]
        public void assign_quant_components_in_bounds_monoisotopic_tolerance()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-TestExperimentalProteoform.missed_monoisotopics, TestExperimentalProteoform.missed_monoisotopics * 2 + 1).ToList();

            IEnumerable<NeuCodePair> neucodes = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass).OfType<NeuCodePair>();
            List<Component> quant_components = TestExperimentalProteoform.generate_neucode_quantitative_components();

            // in bounds lowest monoisotopic error
            Sweet.lollipop.neucode_labeled = true;
            List<ExperimentalProteoform> pfs = Sweet.lollipop.createProteoforms(neucodes, neucodes.Select(x => x.neuCodeLight), 0);
            List<ExperimentalProteoform> vetted_quant = Sweet.lollipop.assignQuantificationComponents(pfs, quant_components);
            Assert.AreEqual(1, vetted_quant.Count);
            Assert.AreEqual(2, vetted_quant[0].aggregated.Count);
            Assert.AreEqual(1, vetted_quant[0].lt_quant_components.Count);
            Assert.AreEqual(1, vetted_quant[0].hv_quant_components.Count);
            Assert.AreEqual(2, quant_components.Count);
            Assert.AreEqual(0, Sweet.lollipop.remaining_quantification_components.Count);
        }

        [Test]
        public void assign_quant_components_large_tolerance_split_range()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.mass_tolerance = 10; //ppm
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-3, 3 * 2 + 1).ToList();
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 20000, 2, true); // tolerance is 0.2 Da
            double hv_mass = e.agg_mass + e.lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT; // 20000.0703

            Component bb = new Component();
            Component cc = new Component();
            Component dd = new Component();
            Component ee = new Component();
            Component ff = new Component();
            Component gg = new Component();
            Component hh = new Component();
            Component ii = new Component();
            bb.weighted_monoisotopic_mass = 19999.79;
            cc.weighted_monoisotopic_mass = 19999.99;
            dd.weighted_monoisotopic_mass = 20000;
            ee.weighted_monoisotopic_mass = 20000.03;
            //boundary is 20000.036
            ff.weighted_monoisotopic_mass = 20000.04;
            gg.weighted_monoisotopic_mass = 20000.07;
            hh.weighted_monoisotopic_mass = 20000.08;
            ii.weighted_monoisotopic_mass = 20000.28;

            Sweet.lollipop.remaining_quantification_components = new HashSet<Component> { bb, cc, dd, ee, ff, gg, hh, ii };
            e.assign_quantitative_components();
            Assert.AreEqual(3, e.lt_quant_components.Count);
            Assert.AreEqual(3, e.hv_quant_components.Count);
            Assert.False(e.lt_quant_components.Any(c => e.hv_quant_components.Contains(c)));
        }


        [Test]
        public void full_agg()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            IEnumerable<NeuCodePair> neucodes = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass).OfType<NeuCodePair>();
            List<Component> components = neucodes.Select(nc => nc.neuCodeLight).Concat(neucodes.Select(nc => nc.neuCodeHeavy)).ToList();
            List<Component> quant_components = TestExperimentalProteoform.generate_neucode_quantitative_components();

            //Must use Sweet.lol.remaining_components because ThreadStart only uses void methods
            //Must use Sweet.lol.remaining_components because ThreadStart only uses void methods
            Sweet.lollipop.neucode_labeled = true;
            Sweet.lollipop.input_files = new List<InputFile> { new InputFile("fake.txt", Purpose.Quantification) };
            List<ExperimentalProteoform> vetted_quant = Sweet.lollipop.aggregate_proteoforms(true, neucodes, components, quant_components, 0);
            Assert.AreEqual(1, vetted_quant.Count);
            Assert.AreEqual(2, vetted_quant[0].aggregated.Count);
            Assert.AreEqual(2, vetted_quant[0].lt_verification_components.Count);
            Assert.AreEqual(2, vetted_quant[0].hv_verification_components.Count);
            Assert.AreEqual(1, vetted_quant[0].lt_quant_components.Count);
            Assert.AreEqual(1, vetted_quant[0].hv_quant_components.Count);
            Assert.AreEqual(2, quant_components.Count);
            Assert.AreEqual(0, Sweet.lollipop.remaining_quantification_components.Count);
        }        
        
        [Test]
        public void full_agg_without_validation()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            IEnumerable<NeuCodePair> neucodes = TestExperimentalProteoform.generate_neucode_components(TestExperimentalProteoform.starter_mass).OfType<NeuCodePair>();
            List<Component> components = neucodes.Select(nc => nc.neuCodeLight).Concat(neucodes.Select(nc => nc.neuCodeHeavy)).ToList();
            List<Component> quant_components = TestExperimentalProteoform.generate_neucode_quantitative_components();

            //Must use Sweet.lol.remaining_components because ThreadStart only uses void methods
            Sweet.lollipop.neucode_labeled = true;
            Sweet.lollipop.decoy_proteoform_communities = new Dictionary<string, ProteoformCommunity> { { "1", new ProteoformCommunity() } };
            Sweet.lollipop.input_files = new List<InputFile> { new InputFile("fake.txt", Purpose.Quantification) };
            List<ExperimentalProteoform> vetted_quant = Sweet.lollipop.aggregate_proteoforms(false, neucodes, components, quant_components, 0);
            Assert.AreEqual(1, vetted_quant.Count);
            Assert.AreEqual(1, Sweet.lollipop.decoy_proteoform_communities.First().Value.experimental_proteoforms.Length);
            Assert.AreEqual(2, vetted_quant[0].aggregated.Count);
            Assert.AreEqual(0, vetted_quant[0].lt_verification_components.Count);
            Assert.AreEqual(0, vetted_quant[0].hv_verification_components.Count);
            Assert.AreEqual(1, vetted_quant[0].lt_quant_components.Count);
            Assert.AreEqual(1, vetted_quant[0].hv_quant_components.Count);
            Assert.AreEqual(2, quant_components.Count);
            Assert.AreEqual(0, Sweet.lollipop.remaining_quantification_components.Count);

            Sweet.lollipop.clear_aggregation();
            Assert.True(Sweet.lollipop.decoy_proteoform_communities.All(x => x.Value.experimental_proteoforms.Length == 0));
            Assert.IsEmpty(Sweet.lollipop.target_proteoform_community.experimental_proteoforms);
            Assert.IsEmpty(Sweet.lollipop.remaining_to_aggregate);
            Assert.IsEmpty(Sweet.lollipop.remaining_quantification_components);
            Assert.IsEmpty(Sweet.lollipop.remaining_verification_components);
        }

        [Test]
        public void unlabeled_agg()
        {
            double max_monoisotopic_mass = TestExperimentalProteoform.starter_mass + TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;
            double min_monoisotopic_mass = TestExperimentalProteoform.starter_mass - TestExperimentalProteoform.missed_monoisotopics * Lollipop.MONOISOTOPIC_UNIT_MASS;

            List<IAggregatable> components = TestExperimentalProteoform.generate_unlabeled_components(TestExperimentalProteoform.starter_mass);

            Sweet.lollipop.neucode_labeled = false;
            Sweet.lollipop.remaining_to_aggregate = new List<IAggregatable>(components);
            Sweet.lollipop.remaining_verification_components = new HashSet<Component>(components.OfType<Component>());
            Sweet.lollipop.missed_monoisotopics_range = Enumerable.Range(-3, 3 * 2 + 1).ToList();
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("E");
            e.root = components[0];
            e.aggregate();
            e.verify();

            Assert.AreEqual(2, e.aggregated.Count);
            Assert.AreEqual(2, e.lt_verification_components.Count);
            Assert.AreEqual(0, e.hv_verification_components.Count); // everything goes into light with unlabeled
            Assert.AreEqual(0, e.lt_quant_components.Count); // no quantitation for unlabeled, yet
            Assert.AreEqual(0, e.hv_quant_components.Count);
        }
    }
}
