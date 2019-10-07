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
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(false, false).AA_Masses;
            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 886.45, 0); // sequence with all serines
            t.sequence = "AAAAAAAAAAAS";
            t.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("Gene", "Gene") });
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 886.46, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("", 799.43, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 87.03);
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, new Modification("missing serine", _modificationType : "Missing", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : -87.03)) });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[Math.Round(set.mass, 1)] = new List<PtmSet> { set };
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[Math.Round(set_unmodified.mass, 1)] = new List<PtmSet> { set_unmodified };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() {set, set_unmodified};
            ConstructorsForTesting.make_relation(e, t, ProteoformComparison.ExperimentalTheoretical, 0);
            t.relationships.First().Accepted = true;
            t.relationships.First().peak = new DeltaMassPeak(t.relationships.First(), new HashSet<ProteoformRelation> { t.relationships.First() }); // should assign the possible ptmset
            t.identify_connected_experimentals(t, t.begin, t.end, t.ptm_set, t.linked_proteoform_references);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() }); // should assign the possible ptmset
            e.identify_connected_experimentals(t, e.begin, e.end, e.ptm_set, e.linked_proteoform_references);
            Assert.IsNotNull(e.linked_proteoform_references);
            Assert.AreEqual(0, e.ptm_set.ptm_combination.Count);
            Assert.AreEqual(1, e.begin);
            Assert.AreEqual(12, e.end);
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(0, e2.ptm_set.ptm_combination.Count);
            Assert.AreEqual(1, e2.begin);
            Assert.AreEqual(11, e2.end);
            Assert.AreEqual(0, e2.ptm_set.ptm_combination.Count);
            Assert.AreEqual(0.01, Math.Round(e.calculate_mass_error(e.linked_proteoform_references.First() as TheoreticalProteoform, e.ptm_set, e.begin, e.end), 2));
            Assert.AreEqual(0.01, Math.Round(e2.calculate_mass_error(e2.linked_proteoform_references.First() as TheoreticalProteoform, e2.ptm_set, e2.begin, e2.end), 2));

            t = ConstructorsForTesting.make_a_theoretical("", 886.45, 0); // sequence with all serines
            t.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("Gene", "Gene") });
            t.sequence = "SAAAAAAAAAAA";
            e = ConstructorsForTesting.ExperimentalProteoform("", 886.47, 0, true);
            e2 = ConstructorsForTesting.ExperimentalProteoform("", 799.44, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 87.03);
            ConstructorsForTesting.make_relation(e, t, ProteoformComparison.ExperimentalTheoretical, 0);
            t.relationships.First().Accepted = true;
            t.relationships.First().peak = new DeltaMassPeak(t.relationships.First(), new HashSet<ProteoformRelation> { t.relationships.First() }); // should assign the possible ptmset
            t.identify_connected_experimentals(t, t.begin, t.end, t.ptm_set, t.linked_proteoform_references);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() }); // should assign the possible ptmset
            e.identify_connected_experimentals(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end, e.ptm_set, e.linked_proteoform_references);
            Assert.IsNotNull(e.linked_proteoform_references);
            Assert.AreEqual(0, e.ptm_set.ptm_combination.Count);
            Assert.AreEqual(1, e.begin);
            Assert.AreEqual(12, e.end);
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(2, e2.begin);
            Assert.AreEqual(12, e2.end);
            Assert.AreEqual(0, e2.ptm_set.ptm_combination.Count);
            Assert.AreEqual(0.02, Math.Round(e.calculate_mass_error(e.linked_proteoform_references.First() as TheoreticalProteoform, e.ptm_set, e.begin, e.end), 2));
            Assert.AreEqual(0.02, Math.Round(e2.calculate_mass_error(e2.linked_proteoform_references.First() as TheoreticalProteoform, e2.ptm_set, e2.begin, e2.end), 2));

            t = ConstructorsForTesting.make_a_theoretical("", 815.41, 0); // sequence with all serines
            t.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("Gene", "Gene") });
            t.sequence = "SAAAAAAAAAA";
            t.begin = 2;
            e = ConstructorsForTesting.ExperimentalProteoform("", 815.41, 0, true);
            e2 = ConstructorsForTesting.ExperimentalProteoform("", 946.45, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 113);
            ModificationMotif.TryGetMotif("M", out motif);
            set = new PtmSet(new List<Ptm> { new Ptm(0, new Modification("M retention", _modificationType : "AminoAcid", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 113)) });
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[Math.Round(set.mass, 1)] = new List<PtmSet> { set };
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary[Math.Round(set_unmodified.mass, 1)] = new List<PtmSet> { set_unmodified };
            ConstructorsForTesting.make_relation(e, t, ProteoformComparison.ExperimentalTheoretical, 0);
            t.relationships.First().Accepted = true;
            t.relationships.First().peak = new DeltaMassPeak(t.relationships.First(), new HashSet<ProteoformRelation> { t.relationships.First() }); // should assign the possible ptmset
            t.identify_connected_experimentals(t, t.begin, t.end, t.ptm_set, t.linked_proteoform_references);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() }); // should assign the possible ptmset
            e.identify_connected_experimentals(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end, e.ptm_set, e.linked_proteoform_references);
            Assert.IsNotNull(e.linked_proteoform_references);
            Assert.AreEqual(0, e.ptm_set.ptm_combination.Count);
            Assert.AreEqual(2, e.begin);
            Assert.AreEqual(12, e.end);
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(0, e2.ptm_set.ptm_combination.Count);
            Assert.AreEqual(1, e2.begin);
            Assert.AreEqual(12, e2.end);
            Assert.AreEqual(-.004, Math.Round(e.calculate_mass_error(e.linked_proteoform_references.First() as TheoreticalProteoform, e.ptm_set, e.begin, e.end), 3));
            Assert.AreEqual(-0.004, Math.Round(e2.calculate_mass_error(e2.linked_proteoform_references.First() as TheoreticalProteoform, e2.ptm_set, e2.begin, e2.end), 3));
        }

        [Test]
        public void loss_of_ptm_set()
        {
            Sweet.lollipop = new Lollipop();

            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(false,false).AA_Masses;

            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 1106.40, 0); // sequence with all serines
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 1232.43, 0, true);
            e.linked_proteoform_references = new List<Proteoform> { t };
            e.begin = 1;
            e.end = 12;
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new Modification("acetyl", _modificationType :"", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01)),
                new Ptm(0, new Modification("acetyl",_modificationType : "", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01)),
                new Ptm(0, new Modification("acetyl",_modificationType : "", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01))
            });
            PtmSet set2 = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new Modification("acetyl",_modificationType : "", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01)),
                new Ptm(0, new Modification("acetyl",_modificationType : "", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01))
            });
            PtmSet set3 = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new Modification("acetyl", _modificationType :"", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01))
            });
            PtmSet set4 = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new Modification("acetyl",_modificationType : "", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01, _databaseReference : new Dictionary<string, IList<string>>())),
                new Ptm(0, new Modification("acetyl", _modificationType :"", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01, _databaseReference : new Dictionary<string, IList<string>>())),
                new Ptm(0, new Modification("acetyl", _modificationType :"", _target : motif, _locationRestriction : "Anywhere.",  _monoisotopicMass :42.01, _databaseReference : new Dictionary<string, IList<string>>())),
                new Ptm(0, new Modification("acetyl",_modificationType : "", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01, _databaseReference : new Dictionary<string, IList<string>>())),
            });

            e.ptm_set = set;
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(set.mass, 1), new List<PtmSet> { set } },
                { Math.Round(set2.mass, 1), new List<PtmSet> { set2 } },
                { Math.Round(set3.mass, 1), new List<PtmSet> { set3 } },
                { Math.Round(set4.mass, 1), new List<PtmSet> { set4 } },
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() {set, set2, set3, set4};
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("", 1106.4, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 126.03);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            e.identify_connected_experimentals(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end, e.ptm_set, e.linked_proteoform_references);
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(0, e2.ptm_set.mass);
            Assert.AreEqual(0, Math.Round(e2.calculate_mass_error(e2.linked_proteoform_references.First() as TheoreticalProteoform, e2.ptm_set, e2.begin, e2.end), 2));

            e.relationships.Clear();
            e2 = ConstructorsForTesting.ExperimentalProteoform("", 1148.41, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 84.02);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            e.identify_connected_experimentals(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end, e.ptm_set, e.linked_proteoform_references);
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(42.01, e2.ptm_set.mass);
            Assert.AreEqual(0, Math.Round(e2.calculate_mass_error(e2.linked_proteoform_references.First() as TheoreticalProteoform, e2.ptm_set, e2.begin, e2.end), 2));

            e.relationships.Clear();
            e2 = ConstructorsForTesting.ExperimentalProteoform("", 1190.42, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 42.01);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            e.identify_connected_experimentals(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end, e.ptm_set, e.linked_proteoform_references);
            Assert.IsNotNull(e2.linked_proteoform_references);
            Assert.AreEqual(84.02, e2.ptm_set.mass);
            Assert.AreEqual(0, Math.Round(e2.calculate_mass_error(e2.linked_proteoform_references.First() as TheoreticalProteoform, e2.ptm_set, e2.begin, e2.end), 2));

            //can't remove more acetylations than in e's ptmset
            e.relationships.Clear();
            e2 = ConstructorsForTesting.ExperimentalProteoform("", 1148.41, 0, true);
            ConstructorsForTesting.make_relation(e, e2, ProteoformComparison.ExperimentalExperimental, 168.04);
            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            e.identify_connected_experimentals(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end, e.ptm_set, e.linked_proteoform_references);
            Assert.IsNull(e2.linked_proteoform_references);
        }

        [Test]
        public void unmodified_identification()
        {
            Sweet.lollipop = new Lollipop();

            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(false,false).AA_Masses;

            TheoreticalProteoform t = ConstructorsForTesting.make_a_theoretical("", 1106.40, 0); // sequence with all serines
            ExperimentalProteoform e = ConstructorsForTesting.ExperimentalProteoform("", 1106.42, 0, true);
            ConstructorsForTesting.make_relation(t, e, ProteoformComparison.ExperimentalTheoretical, 0);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set_not_quite_zero = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new Modification("acetyl", _modificationType : "", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.01)),
                new Ptm(0, new Modification("acetyl loss",  _modificationType :"", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : -42.01)),
            });

            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(set_not_quite_zero.mass, 1), new List<PtmSet> { set_not_quite_zero, set_unmodified } },
            };

            e.relationships.First().Accepted = true;
            e.relationships.First().peak = new DeltaMassPeak(e.relationships.First(), new HashSet<ProteoformRelation> { e.relationships.First() });
            TestProteoformCommunityRelate.prepare_for_et(new List<double>() { set_not_quite_zero.mass });
            // Identify adds nothing to the PtmSet of the Experimental, so it will be labeled Unmodified. It adds the TheoreticalProteoform to the linked reference.
            t.identify_connected_experimentals(t, t.begin, t.end, t.ptm_set, t.linked_proteoform_references);
            Assert.IsNotNull(e.linked_proteoform_references);
            Assert.AreEqual(0, e.ptm_set.mass);
            Assert.AreEqual(new Ptm().modification.OriginalId, e.ptm_set.ptm_description); // it's unmodified
            Assert.AreEqual(0.02, Math.Round(e.calculate_mass_error(e.linked_proteoform_references.First() as TheoreticalProteoform, e.ptm_set, e.begin, e.end), 2));
        }

        [Test]
        public void adduct_experimental()
        {
            Sweet.lollipop = new Lollipop();

            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, new Modification("Sulfate Adduct", _modificationType : "Common", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 97.97)) });
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
        public void ambiguous_experimentals_addition()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform
                t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene1", "gene1")});
            TheoreticalProteoform
                t2 = ConstructorsForTesting.make_a_theoretical("T2", 10000, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene2", "gene2")});
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100042.01, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 100084.02, 0, true);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 100126.03, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 =
                ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm acetyl_ptm = new Ptm(0,
                new Modification("acetyl", _modificationType: "Common", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                acetyl_ptm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> {new Ptm()});

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                {Math.Round(acetyl.mass, 1), new List<PtmSet> {acetyl}},
                {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() {set_unmodified, acetyl};
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> {e1.relationships[i]});
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> {e2.relationships[i]});
            }

            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() {relation34});

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList =
                new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);
            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual(1, e2.ambiguous_identifications.Count);
            Assert.AreEqual(1, e1.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);

            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 1);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("acetyl", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2",
                e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("acetyl", e3.ambiguous_identifications.First().ptm_set.ptm_description);
            Assert.AreEqual(3, e3.proteoform_level);
            Assert.AreEqual("Gene ambiguity; PTM localization ambiguity; ", e3.proteoform_level_description);

            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 1);
            Assert.AreEqual(e4.end, 12);
            Assert.AreEqual("acetyl; acetyl", e4.ptm_set.ptm_description);
            Assert.AreEqual("gene2",
                e4.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e4.ambiguous_identifications.First().end);
            Assert.AreEqual("acetyl; acetyl", e4.ambiguous_identifications.First().ptm_set.ptm_description);
            Assert.AreEqual(3, e4.proteoform_level);
            Assert.AreEqual("Gene ambiguity; PTM localization ambiguity; ", e4.proteoform_level_description);

            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 1);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);
            Assert.AreEqual("gene1",
                e2.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e2.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e2.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e2.ambiguous_identifications.First().ptm_set.ptm_description);
            Assert.AreEqual(2, e2.proteoform_level);
            Assert.AreEqual("Gene ambiguity; ", e2.proteoform_level_description);

            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 1);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);
            Assert.AreEqual("gene2",
                e1.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e1.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e1.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e1.ambiguous_identifications.First().ptm_set.ptm_description);
            Assert.AreEqual(2, e1.proteoform_level);
            Assert.AreEqual("Gene ambiguity; ", e1.proteoform_level_description);

            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily>() {fam};
            Assert.AreEqual(4, ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, null).Rows.Count);
        }




        [Test]
        public void ambiguous_experimentals_addition_withTopDownPform()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform
                t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene1", "gene1")});
            TheoreticalProteoform
                t2 = ConstructorsForTesting.make_a_theoretical("T2", 10000, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene2", "gene2")});
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100042.01, 0, true);
            TopDownProteoform e3 = ConstructorsForTesting.TopDownProteoform("E3", 100084.02, 0);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 100126.03, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 =
                ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm acetyl_ptm = new Ptm(0,
                new Modification("acetyl", _modificationType: "Common", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                acetyl_ptm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                {Math.Round(acetyl.mass, 1), new List<PtmSet> {acetyl}},
                {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, acetyl };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
            }

            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() { relation34 });

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList =
                new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(3, fam.gene_names.Count);
            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual(1, e2.ambiguous_identifications.Count);
            Assert.AreEqual(1, e1.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);

            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 1);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("acetyl", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2",
                e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("acetyl", e3.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 1);
            Assert.AreEqual(e4.end, 12);
            Assert.AreEqual("acetyl; acetyl", e4.ptm_set.ptm_description);
            Assert.AreEqual("gene2",
                e4.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e4.ambiguous_identifications.First().end);
            Assert.AreEqual("acetyl; acetyl", e4.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 1);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);
            Assert.AreEqual("gene1",
                e2.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e2.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e2.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e2.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 1);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);
            Assert.AreEqual("gene2",
                e1.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e1.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e1.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e1.ambiguous_identifications.First().ptm_set.ptm_description);

            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily>() { fam };
            Assert.AreEqual(4, ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, null).Rows.Count);

            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily>() { fam };
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform>() {e3};
            Assert.AreEqual(4, ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, null).Rows.Count);
            Assert.AreEqual(1, ResultsSummaryGenerator.topdown_results_dataframe().Rows.Count);
        }

        [Test]
        public void identify_from_td_nodes()
        {
            TestTopDown.test_add_topdown_theoreticals();
            Lollipop.preferred_gene_label = "primary";
            Sweet.lollipop.topdown_theoretical_reduce_ambiguity = true;
            Sweet.lollipop.identify_from_td_nodes = true;
           
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            TopDownProteoform e3 = Sweet.lollipop.topdown_proteoforms.First();
            ConstructorsForTesting.make_relation(e3, e1, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm acetyl_ptm = new Ptm(0,
                new Modification("acetyl", _modificationType: "Common", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                acetyl_ptm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                {Math.Round(acetyl.mass, 1), new List<PtmSet> {acetyl}},
                {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, acetyl };
            e1.relationships[0].Accepted = true;
            e1.relationships[0].peak = new DeltaMassPeak(e1.relationships[0],
                new HashSet<ProteoformRelation> { e1.relationships[0] });

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList =
                new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();


            Assert.AreEqual(2, fam.experimental_proteoforms.Count);
            Assert.AreEqual(0, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(1, fam.gene_names.Count);
            Assert.AreEqual(0, e3.ambiguous_identifications.Count);
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);

            Assert.AreEqual("YPS1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 10);
            Assert.AreEqual(e3.end, 20);
            Assert.AreEqual("Unmodified", e3.ptm_set.ptm_description);

            Assert.AreEqual("YPS1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 10);
            Assert.AreEqual(e1.end, 20);
            Assert.AreEqual("acetyl", e1.ptm_set.ptm_description);
        }

        [Test]
        public void ambiguous_experimentals_addition_withTopDownPform_reduce_ambig()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";
            Sweet.lollipop.topdown_theoretical_reduce_ambiguity = true;

            TheoreticalProteoform
                t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene1", "gene1")});
            t1.topdown_theoretical = true;
            TheoreticalProteoform
                t2 = ConstructorsForTesting.make_a_theoretical("T2", 10000, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene2", "gene2")});
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100042.01, 0, true);
            TopDownProteoform e3 = ConstructorsForTesting.TopDownProteoform("E3", 100084.02, 0);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 100126.03, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 =
                ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm acetyl_ptm = new Ptm(0,
                new Modification("acetyl", _modificationType: "Common", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                acetyl_ptm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                {Math.Round(acetyl.mass, 1), new List<PtmSet> {acetyl}},
                {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, acetyl };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
            }
            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() { relation34 });

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList =
                new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();



            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(3, fam.gene_names.Count);
            Assert.AreEqual(0, e3.ambiguous_identifications.Count);
            Assert.AreEqual(0, e2.ambiguous_identifications.Count);
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);
            Assert.AreEqual(0, e4.ambiguous_identifications.Count);

            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 1);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("acetyl", e3.ptm_set.ptm_description);
          
            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 1);
            Assert.AreEqual(e4.end, 12);
            Assert.AreEqual("acetyl; acetyl", e4.ptm_set.ptm_description);
           
            Assert.AreEqual("gene1", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 1);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);

            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 1);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);

            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily>() { fam };
            Assert.AreEqual(4, ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, null).Rows.Count);

            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily>() { fam };
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform>() { e3 };
            Assert.AreEqual(4, ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, null).Rows.Count);
            Assert.AreEqual(1, ResultsSummaryGenerator.topdown_results_dataframe().Rows.Count);
        }

        [Test]
        public void ambiguous_experimentals_addition_annotated_PTMs_reduce_ambig()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";
            Sweet.lollipop.annotated_PTMs_reduce_ambiguity = true;
            Sweet.lollipop.mod_rank_sum_threshold = 10;
            TheoreticalProteoform
                t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene1", "gene1")});
            TheoreticalProteoform
                t2 = ConstructorsForTesting.make_a_theoretical("T2", 10000, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene2", "gene2")});
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100042.01, 0, true);
            TopDownProteoform e3 = ConstructorsForTesting.TopDownProteoform("E3", 100084.02, 0);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 100126.03, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 =
            ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm acetyl_ptm = new Ptm(0,
                new Modification("acetyl", _modificationType: "Common", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                acetyl_ptm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                {Math.Round(acetyl.mass, 1), new List<PtmSet> {acetyl}},
                {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, acetyl };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
            }
            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() { relation34 });

            t1.ExpandedProteinList.First().OneBasedPossibleLocalizedModifications.Add(new KeyValuePair<int, List<Modification>>(1, new List<Modification>() { acetyl_ptm.modification }));

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList =
                new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(3, fam.gene_names.Count);
            Assert.AreEqual(0, e3.ambiguous_identifications.Count);
            Assert.AreEqual(1, e2.ambiguous_identifications.Count); //unmodified from gene2 vs. unmodified from gene1
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);
            Assert.AreEqual(0, e4.ambiguous_identifications.Count);

            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 1);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("acetyl", e3.ptm_set.ptm_description);

            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 1);
            Assert.AreEqual(e4.end, 12);
            Assert.AreEqual("acetyl; acetyl", e4.ptm_set.ptm_description);

            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 1);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);
            Assert.AreEqual("gene1",
                e2.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e2.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e2.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e2.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 1);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);

            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily>() { fam };
            Assert.AreEqual(4, ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, null).Rows.Count);

            Sweet.lollipop.target_proteoform_community.families = new List<ProteoformFamily>() { fam };
            Sweet.lollipop.topdown_proteoforms = new List<TopDownProteoform>() { e3 };
            Assert.AreEqual(4, ResultsSummaryGenerator.experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, null).Rows.Count);
            Assert.AreEqual(1, ResultsSummaryGenerator.topdown_results_dataframe().Rows.Count);
        }

        [Test]
        public void remove_bad_relation_cantRemovePtmSet()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.remove_bad_connections = false;
            Lollipop.preferred_gene_label = "primary";
            Sweet.lollipop.annotated_PTMs_reduce_ambiguity = true;
            Sweet.lollipop.mod_rank_sum_threshold = 10;
            TheoreticalProteoform
                t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene1", "gene1")});
            TheoreticalProteoform
                t2 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene2", "gene2")});
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 42.01);
            ConstructorsForTesting.make_relation(t2, e1, ProteoformComparison.ExperimentalTheoretical, -42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm acetyl_ptm = new Ptm(0,
                new Modification("acetyl", _modificationType: "Common", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                acetyl_ptm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                {Math.Round(acetyl.mass, 1), new List<PtmSet> {acetyl}},
                {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, acetyl };
            for (int i = 0; i < 2; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
            }

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList =
                new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();

            Assert.AreEqual(0, e1.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 1);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("acetyl", e1.ptm_set.ptm_description);
            Assert.IsTrue(t2.relationships.First().Accepted);
            Assert.IsFalse(t2.relationships.First().Identification);
            Assert.IsTrue(t1.relationships.First().Accepted);
            Assert.IsTrue(t1.relationships.First().Identification);
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);

            //now remove bad relations
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.remove_bad_connections = true;
            Lollipop.preferred_gene_label = "primary";
            Sweet.lollipop.annotated_PTMs_reduce_ambiguity = true;
            Sweet.lollipop.mod_rank_sum_threshold = 10;
            t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene1", "gene1")});
            
            t2 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene2", "gene2")});
            e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 42.01);
            ConstructorsForTesting.make_relation(t2, e1, ProteoformComparison.ExperimentalTheoretical, -42.01);

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                {Math.Round(acetyl.mass, 1), new List<PtmSet> {acetyl}},
                {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, acetyl };
            for (int i = 0; i < 2; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
            }

            fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList =
                new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();

            Assert.IsFalse(t2.relationships.First().Accepted);
            Assert.IsFalse(t2.relationships.First().Identification);
            Assert.IsTrue(t1.relationships.First().Accepted);
            Assert.IsTrue(t1.relationships.First().Identification);
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 1);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("acetyl", e1.ptm_set.ptm_description);
        }

        [Test]
        public void remove_bad_relation_ppm_error()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.remove_bad_connections = true;
            Lollipop.preferred_gene_label = "primary";
            Sweet.lollipop.annotated_PTMs_reduce_ambiguity = true;
            Sweet.lollipop.mod_rank_sum_threshold = 10;
            TheoreticalProteoform
                t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene1", "gene1")});
            TheoreticalProteoform
                t2 = ConstructorsForTesting.make_a_theoretical("T2", 100042.08, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene2", "gene2")});
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 42.01);
            ConstructorsForTesting.make_relation(t2, e1, ProteoformComparison.ExperimentalTheoretical, 0.07);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm acetyl_ptm = new Ptm(0,
                new Modification("acetyl", _modificationType: "Common", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                acetyl_ptm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                {Math.Round(acetyl.mass, 1), new List<PtmSet> {acetyl}},
                {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, acetyl };
            for (int i = 0; i < 2; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
            }

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList =
                new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();

            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 1);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("acetyl", e1.ptm_set.ptm_description);
            Assert.IsTrue(t2.relationships.First().Accepted);
            Assert.IsTrue(t2.relationships.First().Identification);
            Assert.IsTrue(t1.relationships.First().Accepted);
            Assert.IsTrue(t1.relationships.First().Identification);
            Assert.AreEqual(1, e1.ambiguous_identifications.Count);
            Assert.AreEqual("gene2", e1.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.ambiguous_identifications.First().begin, 1);
            Assert.AreEqual(e1.ambiguous_identifications.First().end, 12);
            Assert.AreEqual("Unmodified", e1.ambiguous_identifications.First().ptm_set.ptm_description);


            //now remove set tolerance
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.id_ppm_tolerance = 1;
            Sweet.lollipop.id_use_ppm_tolerance = false;
            Sweet.lollipop.remove_bad_connections = true;
            Lollipop.preferred_gene_label = "primary";
            Sweet.lollipop.annotated_PTMs_reduce_ambiguity = true;
            Sweet.lollipop.mod_rank_sum_threshold = 10;
            t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene1", "gene1")});

            t2 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>()
                {new Tuple<string, string>("gene2", "gene2")});
            e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 42.01);
            ConstructorsForTesting.make_relation(t2, e1, ProteoformComparison.ExperimentalTheoretical, -42.01);

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                {Math.Round(acetyl.mass, 1), new List<PtmSet> {acetyl}},
                {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, acetyl };
            for (int i = 0; i < 2; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
            }

            fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList =
                new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();

            Assert.IsFalse(t2.relationships.First().Accepted);
            Assert.IsFalse(t2.relationships.First().Identification);
            Assert.IsTrue(t1.relationships.First().Accepted);
            Assert.IsTrue(t1.relationships.First().Identification);
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 1);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("acetyl", e1.ptm_set.ptm_description);
        }



        [Test]
        public void ambiguous_experimentals_addition_another_scenario()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 100042.01, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene2", "gene2") });
            t1.sequence = "SSAASDFSDFSSS";
            t2.sequence = "SSAASDFSDFSSS";
            t1.begin = 2;
            t2.begin = 2;
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100084.02, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 100042.01, 0, true);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 100042.01, 0, true);

            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 0);
            ConstructorsForTesting.make_relation(e1, e2, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 0);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm serine_loss_tpm = new Ptm(0,
                new Modification("Serine Loss", _modificationType: "Missing", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: -42.011));
            PtmSet serine_loss = new PtmSet(new List<Ptm>
            {
                serine_loss_tpm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(serine_loss.mass, 1), new List<PtmSet> { serine_loss }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, serine_loss };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
                e3.relationships[i].Accepted = true;
                e3.relationships[i].peak = new DeltaMassPeak(e3.relationships[i],
                    new HashSet<ProteoformRelation> {e3.relationships[i]});
            }


            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);;
            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual(0, e2.ambiguous_identifications.Count);
            Assert.AreEqual(1, e1.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);

            Assert.AreEqual(1, e1.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 2);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);
            Assert.AreEqual(3, e1.proteoform_level);
            Assert.AreEqual("Gene ambiguity; Sequence ambiguity; ", e1.proteoform_level_description);
            Assert.AreEqual("gene2", e1.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(3, e1.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e1.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e1.ambiguous_identifications.First().ptm_set.ptm_description);
            Assert.AreEqual(1, e2.proteoform_level);
            Assert.AreEqual("Unambiguous", e2.proteoform_level_description);

            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 2);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("Unmodified", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(3, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e3.ambiguous_identifications.First().ptm_set.ptm_description);
            Assert.AreEqual(3, e3.proteoform_level);
            Assert.AreEqual("Gene ambiguity; Sequence ambiguity; ", e3.proteoform_level_description);

            Assert.AreEqual(1, e4.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 2);
            Assert.AreEqual(e4.end, 12);
            Assert.AreEqual("Unmodified", e4.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e4.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(3, e4.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e4.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e4.ambiguous_identifications.First().ptm_set.ptm_description);
            Assert.AreEqual(3, e4.proteoform_level);
            Assert.AreEqual("Gene ambiguity; Sequence ambiguity; ", e4.proteoform_level_description);

            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 2);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);
        }

        [Test]
        public void ambiguous_experimentals_addition_nullPTMset()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 100042.01, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene2", "gene2") });
            t1.sequence = "SSAASDFSDFSSS";
            t2.sequence = "SSAASDFSDFSSS";
            t1.begin = 2;
            t2.begin = 2;
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100042.01, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100084.02, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 100042.01, 0, true);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 100046.01, 0, true);

            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 0);
            ConstructorsForTesting.make_relation(e1, e2, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 4);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm serine_loss_tpm = new Ptm(0,
                new Modification("Serine Loss", _modificationType: "Missing", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: -42.011));
            PtmSet serine_loss = new PtmSet(new List<Ptm>
            {
                serine_loss_tpm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(serine_loss.mass, 1), new List<PtmSet> { serine_loss }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, serine_loss };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
                e3.relationships[i].Accepted = true;
                e3.relationships[i].peak = new DeltaMassPeak(e3.relationships[i],
                    new HashSet<ProteoformRelation> { e3.relationships[i] });
            }


            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);
            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual(0, e2.ambiguous_identifications.Count);
            Assert.AreEqual(1, e1.ambiguous_identifications.Count);
            Assert.AreEqual(0, e4.ambiguous_identifications.Count);

            Assert.AreEqual(1, e1.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 2);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e1.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(3, e1.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e1.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e1.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 2);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("Unmodified", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(3, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e3.ambiguous_identifications.First().ptm_set.ptm_description);


            Assert.IsNull( e4.gene_name);
            Assert.AreEqual(e4.begin, 0);
            Assert.AreEqual(e4.end, 0);
            Assert.AreEqual("Unmodified", e4.ptm_set.ptm_description);

            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 2);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);
        }


        [Test]
        public void ambiguous_experimentals_missing()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 100042.01, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene2", "gene2") });
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100084.02, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100084.02, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 100042.01, 0, true);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 10000, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 = ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm acetyl_ptm = new Ptm(0,
                new Modification("acetyl", _modificationType: "Common", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                acetyl_ptm
            });
            t1.ptm_set = new PtmSet(new List<Ptm>() { acetyl_ptm, acetyl_ptm });
            t2.ptm_set = new PtmSet(new List<Ptm>(){ acetyl_ptm , acetyl_ptm });

            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(acetyl.mass, 1), new List<PtmSet> { acetyl }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() {set_unmodified, acetyl};
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> {e1.relationships[i]});
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> {e2.relationships[i]});
            }

            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() {relation34 });

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);
            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual(1, e2.ambiguous_identifications.Count);
            Assert.AreEqual(1, e1.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);

            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 1);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("acetyl", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("acetyl", e3.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 1);
            Assert.AreEqual(e4.end, 12);
            Assert.AreEqual("Unmodified", e4.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e4.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e4.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e4.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 1);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("acetyl; acetyl", e2.ptm_set.ptm_description);
            Assert.AreEqual("gene1", e2.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e2.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e2.ambiguous_identifications.First().end);
            Assert.AreEqual("acetyl; acetyl", e2.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 1);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("acetyl; acetyl", e1.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e1.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e1.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e1.ambiguous_identifications.First().end);
            Assert.AreEqual("acetyl; acetyl", e1.ambiguous_identifications.First().ptm_set.ptm_description);

        }

        [Test]
        public void ambiguous_experimentals_missing_from_left()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 100084.02, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene2", "gene2") });
            t1.sequence = "SSAASDFSDFSSS";
            t2.sequence = "SSAASDFSDFSSS";
            t1.begin = 2;
            t2.begin = 2;
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100084.02, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100084.02, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 100042.01, 0, true);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 10000, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 = ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm serine_loss_tpm = new Ptm(0,
                new Modification("Serine Loss", _modificationType: "Missing", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: -42.011));
            PtmSet serine_loss = new PtmSet(new List<Ptm>
            {
                serine_loss_tpm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(serine_loss.mass, 1), new List<PtmSet> { serine_loss }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, serine_loss };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
            }

            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() { relation34 });

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);
            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual(0, e2.ambiguous_identifications.Count);
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);

            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 3);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("Unmodified", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(3, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e3.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 4);
            Assert.AreEqual(e4.end, 12);
            Assert.AreEqual("Unmodified", e4.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e4.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);
            Assert.AreEqual(4, e4.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e4.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e4.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 2);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);
         
            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 2);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);
        }

        [Test]
        public void ambiguous_experimentals_missing_from_right()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 100084.02, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene2", "gene2") });
            t1.sequence = "ASSAASDFSSSS";
            t2.sequence = "ASSAASDFSSSS";
            t1.begin = 2;
            t2.begin = 2;
            t1.end = 12;
            t2.end = 12;
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100084.02, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100084.02, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 100042.01, 0, true);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 10000, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 = ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm serine_loss_tpm = new Ptm(0,
                new Modification("Serine Loss", _modificationType: "Missing", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: -42.011));
            PtmSet serine_loss = new PtmSet(new List<Ptm>
            {
                serine_loss_tpm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(serine_loss.mass, 1), new List<PtmSet> { serine_loss }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, serine_loss };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
            }

            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() { relation34 });

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);
            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual(0, e2.ambiguous_identifications.Count);
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);

            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 2);
            Assert.AreEqual(e3.end, 11);
            Assert.AreEqual("Unmodified", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(2, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(11, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e3.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 2);
            Assert.AreEqual(e4.end, 10);
            Assert.AreEqual("Unmodified", e4.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e4.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);
            Assert.AreEqual(2, e4.ambiguous_identifications.First().begin);
            Assert.AreEqual(10, e4.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e4.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 2);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);

            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 2);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);
        }

        [Test]
        public void ambiguous_experimentals_missing_from_right_cantbeMissing()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 100084.02, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene2", "gene2") });
            TheoreticalProteoform t3 = ConstructorsForTesting.make_a_theoretical("T2", 100084.02, 0); // sequence with all serines
            t3.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene3", "gene3") });
            t1.sequence = "ASSAASDFSSS";
            t2.sequence = "ASSAASDFAAS";
            t3.sequence = "ASSAASDFAAA";
            t1.begin = 2;
            t2.begin = 2;
            t3.begin = 3;
            t1.end = 12;
            t2.end = 12;
            t3.end = 12;
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 100084.02, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 100084.02, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 100042.01, 0, true);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 10000, 0, true);
            ExperimentalProteoform e5 = ConstructorsForTesting.ExperimentalProteoform("E5", 100084.06, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(t3, e5, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e5, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 = ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            Ptm serine_loss_tpm = new Ptm(0,
                new Modification("Serine Loss", _modificationType: "Missing", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: -42.011));
            PtmSet serine_loss = new PtmSet(new List<Ptm>
            {
                serine_loss_tpm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(serine_loss.mass, 1), new List<PtmSet> { serine_loss }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, serine_loss };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
                e5.relationships[i].Accepted = true;
                e5.relationships[i].peak = new DeltaMassPeak(e5.relationships[i],
                    new HashSet<ProteoformRelation> { e5.relationships[i] });
            }

            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() { relation34 });

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(5, fam.experimental_proteoforms.Count);
            Assert.AreEqual(3, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(3, fam.gene_names.Count); 
            Assert.AreEqual(1, e3.ambiguous_identifications.Count); //e5 to e3 doesn't lead to ID: no serine to lose...
            Assert.AreEqual(0, e2.ambiguous_identifications.Count);
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);
            Assert.AreEqual(0, e4.ambiguous_identifications.Count);//not ambiguous cause oen couldn't lose another serine

            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 2);
            Assert.AreEqual(e3.end, 11);
            Assert.AreEqual("Unmodified", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(2, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(11, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e3.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 2);
            Assert.AreEqual(e4.end, 10);
            Assert.AreEqual("Unmodified", e4.ptm_set.ptm_description);
 
            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 2);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);

            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 2);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);
        }

        [Test]
        public void ambiguous_experimentals_methionine_retention()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("T2", 10000, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene2", "gene2") });
            t1.begin = 2;
            t2.begin = 2;
            t1.end = 12;
            t2.end = 12;
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 10000, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 10000, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 100042.01, 0, true);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 100042.01, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 = ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 0);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 42.01);

            ModificationMotif.TryGetMotif("M", out ModificationMotif motif);
            Ptm serine_loss_tpm = new Ptm(0,
                new Modification("Methionine", _modificationType: "AminoAcid", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet serine_loss = new PtmSet(new List<Ptm>
            {
                serine_loss_tpm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(serine_loss.mass, 1), new List<PtmSet> { serine_loss }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, serine_loss };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
            }

            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() { relation34 });

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);
            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual(0, e2.ambiguous_identifications.Count);
            Assert.AreEqual(0, e1.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);

            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 1);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("Unmodified", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e3.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 1);
            Assert.AreEqual(e4.end, 12);
            Assert.AreEqual("Unmodified", e4.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e4.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e4.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e4.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene2", e2.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e2.begin, 2);
            Assert.AreEqual(e2.end, 12);
            Assert.AreEqual("Unmodified", e2.ptm_set.ptm_description);

            Assert.AreEqual("gene1", e1.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e1.begin, 2);
            Assert.AreEqual(e1.end, 12);
            Assert.AreEqual("Unmodified", e1.ptm_set.ptm_description);
        }

        [Test]
        public void ambiguous_experimentals_methionine_retention_2()
        {
            Sweet.lollipop = new Lollipop();
            Lollipop.preferred_gene_label = "primary";

            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 10000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            TheoreticalProteoform t2 = ConstructorsForTesting.make_a_theoretical("T2", 10000, 0); // sequence with all serines
            t2.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene2", "gene2") });
            t1.begin = 2;
            t2.begin = 2;
            t1.end = 12;
            t2.end = 12;
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 10000, 0, true);
            ExperimentalProteoform e2 = ConstructorsForTesting.ExperimentalProteoform("E2", 10000, 0, true);
            ExperimentalProteoform e3 = ConstructorsForTesting.ExperimentalProteoform("E3", 10000, 0, true);
            ExperimentalProteoform e4 = ConstructorsForTesting.ExperimentalProteoform("E4", 100042.01, 0, true);
            ConstructorsForTesting.make_relation(t1, e1, ProteoformComparison.ExperimentalTheoretical, 0);
            ConstructorsForTesting.make_relation(e2, e3, ProteoformComparison.ExperimentalExperimental, 0);
            ConstructorsForTesting.make_relation(t2, e2, ProteoformComparison.ExperimentalTheoretical, 0);
            var relation34 = ConstructorsForTesting.make_relation(e3, e4, ProteoformComparison.ExperimentalExperimental, 42.01);
            ConstructorsForTesting.make_relation(e1, e3, ProteoformComparison.ExperimentalExperimental, 0);

            ModificationMotif.TryGetMotif("M", out ModificationMotif motif);
            Ptm serine_loss_tpm = new Ptm(0,
                new Modification("Methionine", _modificationType: "AminoAcid", _target: motif,
                    _locationRestriction: "Anywhere.", _monoisotopicMass: 42.011));
            PtmSet serine_loss = new PtmSet(new List<Ptm>
            {
                serine_loss_tpm
            });
            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });

            // Proteoforms start without any modifications in the PtmSet
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(serine_loss.mass, 1), new List<PtmSet> { serine_loss }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            Sweet.lollipop.theoretical_database.all_possible_ptmsets = new List<PtmSet>() { set_unmodified, serine_loss };
            for (int i = 0; i < 3; i++)
            {
                e1.relationships[i].Accepted = true;
                e1.relationships[i].peak = new DeltaMassPeak(e1.relationships[i],
                    new HashSet<ProteoformRelation> { e1.relationships[i] });
                e2.relationships[i].Accepted = true;
                e2.relationships[i].peak = new DeltaMassPeak(e2.relationships[i],
                    new HashSet<ProteoformRelation> { e2.relationships[i] });
            }

            relation34.Accepted = true;
            relation34.peak = new DeltaMassPeak(relation34, new HashSet<ProteoformRelation>() { relation34 });

            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            fam.identify_experimentals();
            Assert.AreEqual(4, fam.experimental_proteoforms.Count);
            Assert.AreEqual(2, fam.theoretical_proteoforms.Count);
            Assert.AreEqual(2, fam.gene_names.Count);
            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);

            Assert.AreEqual(1, e3.ambiguous_identifications.Count);
            Assert.AreEqual("gene1", e3.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e3.begin, 2);
            Assert.AreEqual(e3.end, 12);
            Assert.AreEqual("Unmodified", e3.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e3.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(2, e3.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e3.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e3.ambiguous_identifications.First().ptm_set.ptm_description);

            Assert.AreEqual("gene1", e4.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(e4.begin, 1);
            Assert.AreEqual(e4.end, 12);
            Assert.AreEqual("Unmodified", e4.ptm_set.ptm_description);
            Assert.AreEqual("gene2", e4.ambiguous_identifications.First().theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label));
            Assert.AreEqual(1, e4.ambiguous_identifications.Count);
            Assert.AreEqual(1, e4.ambiguous_identifications.First().begin);
            Assert.AreEqual(12, e4.ambiguous_identifications.First().end);
            Assert.AreEqual("Unmodified", e4.ambiguous_identifications.First().ptm_set.ptm_description);

        }

        [Test]
        public void only_common_and_known_mods()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.mod_rank_sum_threshold = 1;
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, false).AA_Masses;
            Sweet.lollipop.only_assign_common_or_known_mods = true;
            Lollipop.preferred_gene_label = "primary";
            TheoreticalProteoform t1 = ConstructorsForTesting.make_a_theoretical("T1", 100000, 0); // sequence with all serines
            t1.gene_name = new GeneName(new List<Tuple<string, string>>() { new Tuple<string, string>("gene1", "gene1") });
            ExperimentalProteoform e1 = ConstructorsForTesting.ExperimentalProteoform("E1", 10000, 0, true);
            ModificationMotif.TryGetMotif("S", out ModificationMotif motif);
            PtmSet acetyl = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new Modification("acetyl", _modificationType : "Common", _target : motif, _locationRestriction : "Anywhere.", _monoisotopicMass : 42.011))
            });

            PtmSet set_unmodified = new PtmSet(new List<Ptm> { new Ptm() });
            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(acetyl.mass, 1), new List<PtmSet> { acetyl }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };

            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(acetyl.mass, 1), new List<PtmSet> { acetyl }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };

            //unmodified = OK
            ConstructorsForTesting.make_relation(e1, t1, ProteoformComparison.ExperimentalTheoretical, 0);
            e1.linked_proteoform_references = null;
            e1.relationships.First().Accepted = true;
            e1.relationships.First().peak = new DeltaMassPeak(e1.relationships.First(), new HashSet<ProteoformRelation> { e1.relationships.First() });
            ProteoformFamily fam = new ProteoformFamily(e1);
            fam.construct_family();
            fam.identify_experimentals();
            Assert.IsNotNull(e1.linked_proteoform_references);

            //acetylated OK if common
            t1.modified_mass = 1000;
            e1.modified_mass = 1042;
            e1.linked_proteoform_references = null;
            e1.relationships.Clear();
            t1.relationships.Clear();
            ConstructorsForTesting.make_relation(e1, t1, ProteoformComparison.ExperimentalTheoretical, 42.01);
            e1.relationships.First().Accepted = true;
            e1.relationships.First().peak = new DeltaMassPeak(e1.relationships.First(), new HashSet<ProteoformRelation> { e1.relationships.First() });
            Assert.AreEqual(1, e1.relationships.First().peak.possiblePeakAssignments.Count);
            fam = new ProteoformFamily(e1);
            fam.construct_family();
            fam.identify_experimentals();
            Assert.IsNotNull(e1.linked_proteoform_references);

            //acetylated not ok if Uniprot ptm type...
            e1.linked_proteoform_references = null;
            acetyl = new PtmSet(new List<Ptm>
            {
                new Ptm(0, new Modification("acetyl", _modificationType : "UniProt", _target : motif, _locationRestriction : "Anywhere.",  _monoisotopicMass :42.011))
            });

            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(acetyl.mass, 1), new List<PtmSet> { acetyl }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };

            Sweet.lollipop.theoretical_database.possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>
            {
                { Math.Round(acetyl.mass, 1), new List<PtmSet> { acetyl }  }, {Math.Round(set_unmodified.mass, 1), new List<PtmSet> {set_unmodified}}
            };
            e1.linked_proteoform_references = null;
            e1.relationships.Clear();
            t1.relationships.Clear();
            ConstructorsForTesting.make_relation(e1, t1, ProteoformComparison.ExperimentalTheoretical, 42.01);
            e1.relationships.First().Accepted = true;
            e1.relationships.First().peak = new DeltaMassPeak(e1.relationships.First(), new HashSet<ProteoformRelation> { e1.relationships.First() });
            Assert.AreEqual(1, e1.relationships.First().peak.possiblePeakAssignments.Count);
            fam = new ProteoformFamily(e1);
            fam.construct_family();
            fam.identify_experimentals();
            Assert.IsNull(e1.linked_proteoform_references);

            //acetylated ok if theo has in ptmset...
            e1.linked_proteoform_references = null;
            e1.relationships.Clear();
            t1.relationships.Clear();
            t1.ExpandedProteinList.First().OneBasedPossibleLocalizedModifications.Add(new KeyValuePair<int, List<Modification>>(22, new List<Modification>() { acetyl.ptm_combination.First().modification }));
            ConstructorsForTesting.make_relation(e1, t1, ProteoformComparison.ExperimentalTheoretical, 42.01);
            e1.relationships.First().Accepted = true;
            e1.relationships.First().peak = new DeltaMassPeak(e1.relationships.First(), new HashSet<ProteoformRelation> { e1.relationships.First() });
            Assert.AreEqual(1, e1.relationships.First().peak.possiblePeakAssignments.Count);
            fam = new ProteoformFamily(e1);
            fam.construct_family();
            fam.identify_experimentals();
            Assert.IsNotNull(e1.linked_proteoform_references);
        }

        [Test]
        public void ptm_description_test()
        {

            PtmSet set = new PtmSet(null);
            Assert.AreEqual("Unknown", set.ptm_description);
            Assert.AreEqual(0, set.mass);

            set = new PtmSet(new List<Ptm>());
            Assert.AreEqual("Unmodified", set.ptm_description);
            Assert.AreEqual(0, set.mass);
        }
    }
}