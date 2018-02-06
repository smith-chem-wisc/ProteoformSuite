using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Test
{
    [TestFixture]
    public class TestProteoformIdentification
    {
        [Test]
        public void assign_missing_aa_identity()
        {
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(false, true, false, false).AA_Masses;
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 886.45, 0); // sequence with all serines
            t.sequence = "AAAAAAAAAAAS";
            t.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("Gene", "Gene") } );
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 886.46, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("", 799.43, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 87.03);
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, new ModificationWithMass("missing serine", "Missing", motif, TerminusLocalization.Any, -87.03)) });
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
            Assert.AreEqual(0, e2.ptm_set.ptm_combination.Count);
            Assert.AreEqual(0.01, Math.Round(e.calculate_mass_error(), 2));
            Assert.AreEqual(0.01, Math.Round(e2.calculate_mass_error(), 2));

            t = ConstructorsForTesting.make_a_theoretical("", 886.45, 0); // sequence with all serines
            t.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("Gene", "Gene") });
            t.sequence = "SAAAAAAAAAAA";
             e = ConstructorsForTesting.ExperimentalProteoform("", 886.47, 0, true);
             e2 = ConstructorsForTesting.ExperimentalProteoform("", 799.44, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 87.03);
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
            Assert.AreEqual(2, e2.begin);
            Assert.AreEqual(12, e2.end);
            Assert.AreEqual(0, e2.ptm_set.ptm_combination.Count);
            Assert.AreEqual(0.02, Math.Round(e.calculate_mass_error(), 2));
            Assert.AreEqual(0.02, Math.Round(e2.calculate_mass_error(), 2));

            t = ConstructorsForTesting.make_a_theoretical("", 815.41, 0); // sequence with all serines
            t.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("Gene", "Gene") });
            t.sequence = "SAAAAAAAAAA";
            t.begin = 2;
             e = ConstructorsForTesting.ExperimentalProteoform("", 815.41, 0, true);
             e2 = ConstructorsForTesting.ExperimentalProteoform("", 946.45, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 113);
            ModificationMotif.TryGetMotif("M", out motif);
             set = new PtmSet(new List<Ptm> { new Ptm(0, new ModificationWithMass("M retention", "AminoAcid", motif, TerminusLocalization.Any, 113)) });
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
            Assert.AreEqual(2, e.begin);
            Assert.AreEqual(12, e.end);
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(0, e2.ptm_set.ptm_combination.Count);
            Assert.AreEqual(1, e2.begin);
            Assert.AreEqual(12, e2.end);
            Assert.AreEqual(-.004, Math.Round(e.calculate_mass_error(), 3));
            Assert.AreEqual(-0.004, Math.Round(e2.calculate_mass_error(), 3));
        }

        [Test]
        public void loss_of_ptm_set()
        {
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(false, true, false, false).AA_Masses;

            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 1106.40, 0); // sequence with all serines
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 1232.43, 0, true);
            e.linked_proteoform_references = new List<Proteoform> { t };
            e.begin = 1;
            e.end = 12;
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01)),
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01)),
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01))
            });
            PtmSet set2 = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01)),
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01))
            });
            PtmSet set3 = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01))
            });
            PtmSet set4 = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>())),
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>())),
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>())),
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>())),
            });

            e.ptm_set = set;
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(set.mass, 1), new List<PtmSet> { set } },
                { Math.Round(set2.mass, 1), new List<PtmSet> { set2 } },
                { Math.Round(set3.mass, 1), new List<PtmSet> { set3 } },
                { Math.Round(set4.mass, 1), new List<PtmSet> { set4 } },
            };

            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("", 1106.4, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 126.03);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            e.identify_connected_experimentals(new List<PtmSet> { set }, new List<ModificationWithMass> { set.ptm_combination.First().modification });
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(0, e2.ptm_set.mass);
            Assert.AreEqual(0, Math.Round(e2.calculate_mass_error(), 2));

            e.relationships.Clear();
            e2 = ConstructorsForTesting.ExperimentalProteoform("", 1148.41, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 84.02);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            e.identify_connected_experimentals(new List<PtmSet> { set2 }, new List<ModificationWithMass> { set2.ptm_combination.First().modification });
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(42.01, e2.ptm_set.mass);
            Assert.AreEqual(0, Math.Round(e2.calculate_mass_error(), 2));

            e.relationships.Clear();
            e2 = ConstructorsForTesting.ExperimentalProteoform("", 1190.42, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 42.01);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            e.identify_connected_experimentals(new List<PtmSet> { set3 }, new List<ModificationWithMass> { set3.ptm_combination.First().modification });
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(84.02, e2.ptm_set.mass);
            Assert.AreEqual(0, Math.Round(e2.calculate_mass_error(), 2));

            //can't remove more acetylations than in e's ptmset
            e.relationships.Clear();
            e2 = ConstructorsForTesting.ExperimentalProteoform("", 1148.41, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 168.04);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            e.identify_connected_experimentals(new List<PtmSet> { set4 }, new List<ModificationWithMass> { set4.ptm_combination.First().modification });
            Assert.IsNull(e2.linked_proteoform_references);
        }

        [Test]
        public void unmodified_identification()
        {
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(false, true, false, false).AA_Masses;

            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 1106.40, 0); // sequence with all serines
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 1106.42, 0, true);
            ConstructorsForTesting.make_relation(t, e, ProteoformComparison.ExperimentalTheoretical, 0);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set_not_quite_zero = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.01)),
                new Ptm(0, new ModificationWithMass("acetyl loss", "", motif, TerminusLocalization.Any, -42.01)),
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
            Assert.AreEqual(0.02, Math.Round(e.calculate_mass_error(), 2));
        }

        [Test]
        public void adduct_experimental()
        {
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, !Sweet.lollipop.neucode_labeled, Sweet.lollipop.neucode_labeled, false).AA_Masses;
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, new ModificationWithMass("Sulfate Adduct", "", motif, TerminusLocalization.Any, 97.97)) });
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
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.011))
            });

            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(acetyl.mass, 1), new List<PtmSet> { acetyl }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };


            acetyl = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new ModificationWithMass("acetyl", "", motif, TerminusLocalization.Any, 42.011))
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
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, Sweet.lollipop.natural_lysine_isotope_abundance, Sweet.lollipop.neucode_light_lysine, Sweet.lollipop.neucode_heavy_lysine).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(3, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);
            Assert.IsTrue(e2.ambiguous); //dont have same path length, still ambiguous
            Assert.IsFalse(e1.ambiguous);
            Assert.IsTrue(e3.ambiguous);
        }
    }
}
