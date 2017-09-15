using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class TestProteoformIdentification
    {
        [Test]
        public void assign_missing_aa_identity()
        {
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 100087.03, 0); // sequence with all serines
            t.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("Gene", "Gene") } );
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 100087.03, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("", 10000, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 87.03);
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, new ModificationWithMass("missing serine", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, -87.03, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Missing")) });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[Math.Round(set.mass, 1)] = new List<PtmSet> { set };
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[Math.Round(set_unmodified.mass, 1)] = new List<PtmSet> { set_unmodified };
            ConstructorsForTesting.make_relation(e, t, ProteoformComparison.ExperimentalTheoretical, 0);
            t.relationships.First().Accepted = true;
            t.relationships.First().peak = new DeltaMassPeak(t.relationships.First(), new HashSet<ProteoformRelation> { t.relationships.First() }); // should assign the possible ptmset
            t.identify_connected_experimentals(new List<PtmSet> { set_unmodified }, new List<ModificationWithMass>());
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() }); // should assign the possible ptmset
            e.identify_connected_experimentals(new List<PtmSet> { set }, new List<ModificationWithMass> { set.ptm_combination.First().modification });
            Assert.IsNotNull(e.linked_proteoform_references);
            Assert.AreEqual(0, e.ptm_set.ptm_combination.Count);
            Assert.AreEqual(1, e.begin);
            Assert.AreEqual(12, e.end);
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(0, e2.ptm_set.ptm_combination.Count);
            Assert.AreEqual(1, e2.begin);
            Assert.AreEqual(11, e2.end);
        }

        [Test]
        public void loss_of_ptm_set()
        {
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 100000, 0); // sequence with all serines
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 100126.03, 0, true);
            e.linked_proteoform_references = new List<Proteoform> { t };
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
                new Ptm(0, new ModificationWithMass("acetyl", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
                new Ptm(0, new ModificationWithMass("acetyl", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
            });
            PtmSet set2 = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
                new Ptm(0, new ModificationWithMass("acetyl", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
            });
            PtmSet set3 = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
            });
            e.ptm_set = set;
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(set.mass, 1), new List<PtmSet> { set } },
                { Math.Round(set2.mass, 1), new List<PtmSet> { set2 } },
                { Math.Round(set3.mass, 1), new List<PtmSet> { set3 } },
            };
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("", 10042.01, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 84.02);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            e.identify_connected_experimentals(new List<PtmSet> { set, set2, set3 }, new List<ModificationWithMass> { set.ptm_combination.First().modification });
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(42.01, e2.ptm_set.mass);
        }

        [Test]
        public void unmodified_identification()
        {
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 100000, 0); // sequence with all serines
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 10000, 0, true);
            ConstructorsForTesting.make_relation(t, e, ProteoformComparison.ExperimentalTheoretical, 0);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set_not_quite_zero = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 42.011, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
                new Ptm(0, new ModificationWithMass("acetyl loss", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, -42.01, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
            });

            PtmSet set_unmodified = new PtmSet(new List<Ptm>{ new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(set_not_quite_zero.mass, 1), new List<PtmSet> { set_not_quite_zero, set_unmodified } },
            };

            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });

            // Identify adds nothing to the PtmSet of the Experimental, so it will be labeled Unmodified. It adds the TheoreticalProteoform to the linked reference. 
            t.identify_connected_experimentals(new List<PtmSet> { set_not_quite_zero, set_unmodified }, new List<ModificationWithMass> { set_not_quite_zero.ptm_combination[0].modification, set_not_quite_zero.ptm_combination[1].modification, set_unmodified.ptm_combination[0].modification, });
            Assert.IsNotNull(e.linked_proteoform_references);
            Assert.AreEqual(0, e.ptm_set.mass);
            Assert.AreEqual(new Ptm().modification.id, e.ptm_description); // it's unmodified
        }

        [Test]
        public void adduct_experimental()
        {
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, new ModificationWithMass("Sulfate Adduct", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 97.97, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")) });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[Math.Round(set.mass, 1)] = new List<PtmSet> { set };
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[Math.Round(set_unmodified.mass, 1)] = new List<PtmSet> { set_unmodified };
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 1000, 0);
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 1000, 0, true);
            ConstructorsForTesting.make_relation(e, t, ProteoformComparison.ExperimentalTheoretical, 0);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("", 1097.97, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 97.97);
            e.relationships.First().Accepted = true;
            e2.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() }); // should assign the possible ptmset
            e2.relationships.First().peak = new DeltaMassPeak(e2.relationships.First(), new HashSet<ProteoformRelation> { e2.relationships.First() }); // should assign the possible ptmset
            ProteoformFamily fam = new ProteoformFamily(e);
            fam.construct_family();
            fam.identify_experimentals();
            Assert.IsTrue(e2.adduct);
            Assert.IsFalse(e.adduct);
        }

        [Test]
        public void ambiguous_experimentals()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 100000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 10000, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);

            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("T2", 100042.01, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene2", "gene2") });
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100042.01, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 100042.01, 0, true);
            ConstructorsForTesting.make_relation(e3, e1, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e3, e2, ProteoformComparison.ExperimentalExperimental, 0);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e1, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 42.011, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
            });

            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(acetyl.mass, 1), new List<PtmSet> { acetyl }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };


            acetyl = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", new Tuple<string, string>("", ""), motif, TerminusLocalization.Any, 42.011, new Dictionary<string, IList<string>>(), new List<double>(), new List<double>(), "Mod")),
            });

             set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(acetyl.mass, 1), new List<PtmSet> { acetyl }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i], new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i], new HashSet<ProteoformRelation> { e2.relationships[i] });
            }
            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            fam.identify_experimentals();
            Assert.AreEqual(3, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);
            Assert.IsFalse(e2.ambiguous); //dont have same path length so not ambiguous
            Assert.IsFalse(e1.ambiguous);
            Assert.IsTrue(e3.ambiguous);

        }
    }
}
