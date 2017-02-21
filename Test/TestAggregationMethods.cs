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
            ExperimentalProteoform exp = new ExperimentalProteoform();
            exp.root = is_running;
            List<ExperimentalProteoform> active2 = new List<ExperimentalProteoform> { exp };
            Component next2 = Lollipop.find_next_root(ordered, active2);
            Assert.True(Math.Abs(next.weighted_monoisotopic_mass - is_running.weighted_monoisotopic_mass) > 2 * (double)Lollipop.missed_monos);
            Assert.AreEqual(4, next.intensity_sum_olcs);
        }

        [Test]
        public void choose_next_exp_proteoform()
        {
            ExperimentalProteoform c = new ExperimentalProteoform();
            ExperimentalProteoform d = new ExperimentalProteoform();
            ExperimentalProteoform e = new ExperimentalProteoform();
            ExperimentalProteoform f = new ExperimentalProteoform();
            c.agg_mass = 100;
            d.agg_mass = 119;
            e.agg_mass = 121;
            f.agg_mass = 122;
            c.agg_intensity = 1;
            d.agg_intensity = 2;
            e.agg_intensity = 3;
            f.agg_intensity = 4;
            List<ExperimentalProteoform> ordered = new List<ExperimentalProteoform> { c, d, e, f }.OrderByDescending(cc => cc.agg_intensity).ToList();
            ExperimentalProteoform is_running = new ExperimentalProteoform();
            is_running.agg_mass = 100;
            is_running.agg_intensity = 100;

            List<ExperimentalProteoform> active = new List<ExperimentalProteoform> { is_running };
            ExperimentalProteoform next = Lollipop.find_next_root(ordered, active);
            Assert.True(Math.Abs(next.agg_mass - is_running.agg_mass) > 2 * (double)Lollipop.missed_monos);
            Assert.AreEqual(4, next.agg_intensity);
        }
    }
}
