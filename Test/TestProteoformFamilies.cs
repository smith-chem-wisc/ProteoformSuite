using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;

namespace Test
{
    [TestFixture]
    class TestProteoformFamilies
    {
        [Test]
        public void test_construct_one_proteform_family_from_ET()
        {
            ProteoformCommunity test_community = new ProteoformCommunity();
            Lollipop.uniprotModificationTable = new Dictionary<string, IList<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified") } } };

            //One accepted ET relation; should give one ProteoformFamily
            Lollipop.min_peak_count_et = 1;
            ExperimentalProteoform pf1 = new ExperimentalProteoform("E1");
            TheoreticalProteoform pf2 = new TheoreticalProteoform("T1");
            ProteoformComparison comparison = ProteoformComparison.et;
            ProteoformRelation pr1 = new ProteoformRelation(pf1, pf2, comparison, 0);
            List<ProteoformRelation> prs = new List<ProteoformRelation> { pr1 };
            foreach (ProteoformRelation pr in prs) pr.set_nearby_group(prs);
            DeltaMassPeak peak = new DeltaMassPeak(prs[0], prs);
            test_community.delta_mass_peaks = new List<DeltaMassPeak> { peak };
            test_community.experimental_proteoforms = new ExperimentalProteoform[] { pf1 };
            test_community.theoretical_proteoforms = new TheoreticalProteoform[] { pf2 };
            test_community.construct_families();
            Assert.AreEqual("T1", test_community.families.First().accession_list);
            Assert.AreEqual(1, test_community.families.Count);
            Assert.AreEqual(2, test_community.families[0].proteoforms.Count);
            Assert.AreEqual(1, test_community.families.First().experimental_count);
            Assert.AreEqual(1, test_community.families.First().theoretical_count);
        }

        [Test]
        public void test_construct_multi_member_family()
        {
            //Four experimental proteoforms, three relations (linear), all accepted; should give 1 bundled family
            ProteoformCommunity test_community = new ProteoformCommunity();
            Lollipop.uniprotModificationTable = new Dictionary<string, IList<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified") } } };

            Lollipop.min_peak_count_ee = 2;
            ExperimentalProteoform pf3 = new ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = new ExperimentalProteoform("E2");
            ExperimentalProteoform pf5 = new ExperimentalProteoform("E3");
            ExperimentalProteoform pf6 = new ExperimentalProteoform("E4");

            ProteoformComparison comparison34 = ProteoformComparison.ee;
            ProteoformComparison comparison45 = ProteoformComparison.ee;
            ProteoformComparison comparison56 = ProteoformComparison.ee;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, comparison34, 0);
            ProteoformRelation pr3 = new ProteoformRelation(pf4, pf5, comparison45, 0);
            ProteoformRelation pr4 = new ProteoformRelation(pf5, pf6, comparison56, 0);

            List<ProteoformRelation> prs2 = new List<ProteoformRelation> { pr2, pr3, pr4 };
            foreach (ProteoformRelation pr in prs2) pr.set_nearby_group(prs2);
            Assert.AreEqual(3, pr2.nearby_relations_count);
            Assert.AreEqual(3, pr3.nearby_relations_count);
            Assert.AreEqual(3, pr4.nearby_relations_count);

            test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            Assert.AreEqual(1, test_community.delta_mass_peaks.Count);
            Assert.AreEqual(3, test_community.delta_mass_peaks[0].grouped_relations.Count);

            test_community.experimental_proteoforms = new ExperimentalProteoform[] { pf3, pf4, pf5, pf6 };
            test_community.construct_families();
            Assert.AreEqual(1, test_community.families.Count);
            Assert.AreEqual("", test_community.families.First().accession_list);
            Assert.AreEqual(4, test_community.families.First().proteoforms.Count);
            Assert.AreEqual(4, test_community.families.First().experimental_count);
            Assert.AreEqual(0, test_community.families.First().theoretical_count);
        }

        [Test]
        public void test_construct_two_families()
        {
            //Five experimental proteoforms, four relations (linear), second on not accepted into a peak, one peak; should give 2 families
            ProteoformCommunity test_community = new ProteoformCommunity();
            Lollipop.uniprotModificationTable = new Dictionary<string, IList<Modification>> { { "unmodified", new List<Modification> { new Modification("unmodified") } } };

            Lollipop.ee_max_mass_difference = 20;
            Lollipop.peak_width_base_ee = 0.015;
            Lollipop.min_peak_count_ee = 3; //needs to be high so that 0 peak accepted, other peak isn't.... 

            ExperimentalProteoform pf3 = new ExperimentalProteoform("E1");
            ExperimentalProteoform pf4 = new ExperimentalProteoform("E2");
            ExperimentalProteoform pf5 = new ExperimentalProteoform("E3");
            ExperimentalProteoform pf6 = new ExperimentalProteoform("E4");
            ExperimentalProteoform pf7 = new ExperimentalProteoform("E5");

            ProteoformComparison comparison34 = ProteoformComparison.ee;
            ProteoformComparison comparison45 = ProteoformComparison.ee;
            ProteoformComparison comparison56 = ProteoformComparison.ee;
            ProteoformComparison comparison67 = ProteoformComparison.ee;
            ProteoformRelation pr2 = new ProteoformRelation(pf3, pf4, comparison34, 0);
            ProteoformRelation pr3 = new ProteoformRelation(pf4, pf5, comparison45, 19); //not accepted
            ProteoformRelation pr4 = new ProteoformRelation(pf5, pf6, comparison56, 0);
            ProteoformRelation pr5 = new ProteoformRelation(pf6, pf7, comparison67, 0);

            List<ProteoformRelation> prs2 = new List<ProteoformRelation> { pr2, pr3, pr4, pr5 };
            foreach (ProteoformRelation pr in prs2) pr.set_nearby_group(prs2);
            Assert.AreEqual(3, pr2.nearby_relations_count);
            Assert.AreEqual(1, pr3.nearby_relations_count);
            Assert.AreEqual(3, pr4.nearby_relations_count);
            Assert.AreEqual(3, pr5.nearby_relations_count);

            test_community.accept_deltaMass_peaks(prs2, new List<ProteoformRelation>());
            Assert.AreEqual(2, test_community.delta_mass_peaks.Count);
            Assert.AreEqual(1, test_community.delta_mass_peaks.Where(peak => peak.peak_accepted).Count());
            Assert.AreEqual(3, test_community.delta_mass_peaks.Where(peak => peak.peak_accepted).First().grouped_relations.Count());

            test_community.experimental_proteoforms = new ExperimentalProteoform[] { pf3, pf4, pf5, pf6, pf7 };
            test_community.construct_families();
            Assert.AreEqual(2, test_community.families.Count);
            Assert.AreEqual("", test_community.families[0].accession_list);
            Assert.AreEqual(2, test_community.families[0].proteoforms.Count);
            Assert.AreEqual(2, test_community.families[0].experimental_count);
            Assert.AreEqual(0, test_community.families[0].theoretical_count);
            Assert.AreEqual("", test_community.families[1].accession_list);
            Assert.AreEqual(3, test_community.families[1].proteoforms.Count);
            Assert.AreEqual(3, test_community.families[1].experimental_count);
            Assert.AreEqual(0, test_community.families[1].theoretical_count);
        }
    }
}
