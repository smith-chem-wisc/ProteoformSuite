﻿using Proteomics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class Proteoform
    {

        #region Public Properties

        public string accession { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; } = -1;
        public bool is_target { get; set; } = true;
        public List<Proteoform> candidate_relatives { get; set; } // Cleared after use
        public GeneName gene_name { get; set; }
        public string ptm_description { get; set; }
        public PtmSet ptm_set
        {
            get
            {
                return _ptm_set;
            }

            set
            {
                _ptm_set = value;
                ptm_description = ptm_set == null || ptm_set.ptm_combination == null ? 
                    "Unknown" : 
                    ptm_set.ptm_combination.Count == 0 ?
                        "Unmodified" : 
                        this is TopDownProteoform ?
                        String.Join("; ", ptm_set.ptm_combination.Select(ptm => ptm.modification.id + "@" + ptm.position)) :
                        String.Join("; ", ptm_set.ptm_combination.Select(ptm => ptm.modification.id));
            }
        }

        public ProteoformFamily family { get; set; }
        public List<ProteoformRelation> relationships { get; set; } = new List<ProteoformRelation>();

        /// <summary>
        /// Contains a list of proteoforms traced before arriving at this one. The first is a TheoreticalProteoform starting point in the family.
        /// </summary>
        public List<Proteoform> linked_proteoform_references { get; set; }

        #endregion Public Properties

        #region Private Fields

        private PtmSet _ptm_set = new PtmSet(new List<Ptm>());

        #endregion Private Fields

        #region Public Constructors

        public Proteoform(string accession, double modified_mass, int lysine_count, bool is_target)
        {
            this.accession = accession;
            this.modified_mass = modified_mass;
            this.lysine_count = lysine_count;
            this.is_target = is_target;
        }

        public Proteoform(string accession)
        {
            this.accession = accession;
        }

        #endregion Public Constructors

        #region Public Methods

        public List<Proteoform> get_connected_proteoforms()
        {
            return relationships.Where(r => r.Accepted).SelectMany(r => r.connected_proteoforms).ToList();
        }

        public List<ExperimentalProteoform> identify_connected_experimentals(List<PtmSet> all_possible_ptmsets, List<ModificationWithMass> all_mods_with_mass)
        {
            List<ExperimentalProteoform> identified = new List<ExperimentalProteoform>();
            foreach (ProteoformRelation r in relationships.Where(r => r.Accepted).Distinct().ToList())
            {
                ExperimentalProteoform e = r.connected_proteoforms.OfType<ExperimentalProteoform>().FirstOrDefault(p => p != this);
                if (e == null) continue; // Looking at an ET pair, expecting an EE pair

                double mass_tolerance = modified_mass / 1000000 * (double)SaveState.lollipop.mass_tolerance;
                int sign = Math.Sign(e.modified_mass - modified_mass);
                 //if peak is null (topdown relation) use relation deltamass. Otherwise, use peak delta mass. 
                double deltaM = r.peak == null? r.DeltaMass : Math.Sign(r.peak.DeltaMass) < 0 ? r.peak.DeltaMass : sign * r.peak.DeltaMass; // give EE relations the correct sign, but don't switch negative ET relation deltaM's
                Proteoform theoretical_base = (this as TheoreticalProteoform != null || this as TopDownProteoform != null) ?
                    this : //Theoetical or topdown starting point
                    ((linked_proteoform_references.First() as TheoreticalProteoform != null || linked_proteoform_references.First() != null) ?
                        linked_proteoform_references.First() : //Experimental with theoretical reference
                        null); //Experimental without theoretical reference
                string theoretical_base_sequence = theoretical_base == null ? "" : (theoretical_base is TopDownProteoform) ? ((TopDownProteoform)theoretical_base).sequence : ((TheoreticalProteoform)theoretical_base).sequence;
                PtmSet best_addition = generate_possible_added_ptmsets(r.peak == null ? new List<PtmSet> { r.candidate_ptmset } : r.peak.possiblePeakAssignments, deltaM, mass_tolerance, all_mods_with_mass, theoretical_base, theoretical_base_sequence, 1)
                    .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(x.mass - deltaM) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                    .FirstOrDefault();

                ModificationWithMass best_loss = null;
                foreach (ModificationWithMass m in all_mods_with_mass)
                {
                    bool within_loss_tolerance = deltaM >= -m.monoisotopicMass - mass_tolerance && deltaM <= -m.monoisotopicMass + mass_tolerance;
                    bool can_be_removed = this.ptm_set.ptm_combination.Select(ptm => ptm.modification).Contains(m);
                    bool better_than_current_best_loss = best_loss == null || Math.Abs(deltaM - (-m.monoisotopicMass)) < Math.Abs(deltaM - (-best_loss.monoisotopicMass)); 
                    if (can_be_removed && within_loss_tolerance && better_than_current_best_loss)
                    {
                        best_loss = m;
                    }
                }

                // If they're the same and someone hasn't labeled 0 difference with a "ModificationWithMass", then label it null
                if (best_addition == null && best_loss == null && Math.Abs(r.peak == null? r.DeltaMass : r.peak.DeltaMass) <= mass_tolerance)
                {
                    lock (r) lock (e) assign_pf_identity(e, this, ptm_set, r, sign, null);
                    identified.Add(e);
                }

                if (best_addition == null && best_loss == null)
                    continue;

                PtmSet with_mod_change = best_loss != null ?
                    new PtmSet(new List<Ptm>(this.ptm_set.ptm_combination.Where(ptm => !ptm.modification.Equals(best_loss)))) :
                    new PtmSet(new List<Ptm>(this.ptm_set.ptm_combination.Concat(best_addition.ptm_combination).Where(ptm => ptm.modification.monoisotopicMass != 0).ToList()));
                lock (r) lock (e)
                        assign_pf_identity(e, this, with_mod_change, r, sign, best_loss != null ? new PtmSet(new List<Ptm> { new Ptm(-1, best_loss) }) : best_addition);
                identified.Add(e);
            }
            return identified;
        }

        public List<PtmSet> generate_possible_added_ptmsets(List<PtmSet> possible_peak_assignments, double deltaM, double mass_tolerance, List<ModificationWithMass> all_mods_with_mass,
            Proteoform theoretical_base, string theoretical_base_sequence, int additional_ptm_penalty)
        {
            List<ModificationWithMass> known_mods = theoretical_base is TopDownProteoform? theoretical_base.ptm_set.ptm_combination.Select(m => m.modification).ToList() : ((TheoreticalProteoform)theoretical_base).ExpandedProteinList.SelectMany(p => p.OneBasedPossibleLocalizedModifications.ToList()).SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList();
            List<PtmSet> possible_ptmsets = new List<PtmSet>();

            int n_terminal_degraded_aas = degraded_aas_count(theoretical_base_sequence, ptm_set, true);
            int c_terminal_degraded_aas = degraded_aas_count(theoretical_base_sequence, ptm_set, false);
            foreach (PtmSet set in possible_peak_assignments)
            {
                List<ModificationWithMass> mods_in_set = set.ptm_combination.Select(ptm => ptm.modification).ToList();

                int rank_sum = additional_ptm_penalty * (set.ptm_combination.Count - 1); // penalize additional PTMs

                foreach (ModificationWithMass m in mods_in_set)
                {
                    if (m.monoisotopicMass == 0)
                    {
                        rank_sum += SaveState.lollipop.modification_ranks[m.monoisotopicMass];
                        continue;
                    }
                    int begin = theoretical_base is TopDownProteoform ? ((TopDownProteoform)theoretical_base).start_index : ((TheoreticalProteoform)theoretical_base).begin;

                    bool could_be_m_retention = m.modificationType == "AminoAcid" && m.motif.Motif == "M" && begin == 2 && !ptm_set.ptm_combination.Select(p => p.modification).Contains(m);
                    bool motif_matches_n_terminus = n_terminal_degraded_aas < theoretical_base_sequence.Length && m.motif.Motif == theoretical_base_sequence[n_terminal_degraded_aas].ToString();
                    bool motif_matches_c_terminus = c_terminal_degraded_aas < theoretical_base_sequence.Length && m.motif.Motif == theoretical_base_sequence[theoretical_base_sequence.Length - c_terminal_degraded_aas - 1].ToString();
                    bool cannot_be_degradation = !motif_matches_n_terminus && !motif_matches_c_terminus;
                    if (m.modificationType == "Missing" && cannot_be_degradation
                        || m.modificationType == "AminoAcid" && !could_be_m_retention)
                    {
                        rank_sum = Int32.MaxValue;
                        break;
                    }

                    bool could_be_n_term_degradation = m.modificationType == "Missing" && motif_matches_n_terminus;
                    bool could_be_c_term_degradation = m.modificationType == "Missing" && motif_matches_c_terminus;
                    bool likely_cleavage_site = could_be_n_term_degradation && SaveState.lollipop.likely_cleavages.Contains(theoretical_base_sequence[n_terminal_degraded_aas].ToString())
                        || could_be_c_term_degradation && SaveState.lollipop.likely_cleavages.Contains(theoretical_base_sequence[theoretical_base_sequence.Length - c_terminal_degraded_aas - 1].ToString());

                    rank_sum -= Convert.ToInt32(SaveState.lollipop.theoretical_database.variableModifications.Contains(m)); // favor variable modifications over regular modifications of the same mass

                    // In order of likelihood:
                    // 1. First, we observe I/L/A cleavage to be the most common, 
                    // 1. "Fatty Acid" is a list of modifications prevalent in yeast or bacterial analysis, 
                    // 1. and unlocalized modifications are a subset of modifications in the intact_mods.txt list that should be included in intact analysis
                    // 2. Second, other degradations and methionine cleavage are weighted mid-level
                    // 3. Missed monoisotopic errors are considered, but weighted towards the bottom. This should allow missed monoisotopics with common modifications like oxidation, but not rare ones.
                    if (likely_cleavage_site || m.modificationType == "FattyAcid" || m.modificationType == "Unlocalized")  
                        rank_sum += SaveState.lollipop.mod_rank_first_quartile / 2;
                    else if (could_be_m_retention || could_be_n_term_degradation || could_be_c_term_degradation)
                        rank_sum += SaveState.lollipop.mod_rank_second_quartile;
                    else if (m.modificationType == "Deconvolution Error")
                        rank_sum += SaveState.lollipop.neucode_labeled ? 
                            SaveState.lollipop.mod_rank_third_quartile :   //in neucode-labeled data, fewer missed monoisotopics - don't prioritize
                            1 ; //in label-free, more missed monoisotoipcs, should prioritize (set to same priority as variable modification)
                    else
                        rank_sum += known_mods.Concat(SaveState.lollipop.theoretical_database.variableModifications).Contains(m) ?
                            SaveState.lollipop.modification_ranks[m.monoisotopicMass] :
                            SaveState.lollipop.modification_ranks[m.monoisotopicMass] + SaveState.lollipop.mod_rank_first_quartile / 2; // Penalize modifications that aren't known for this protein and push really rare ones out of the running if they're not in the protein entry
                }

                if (rank_sum <= SaveState.lollipop.mod_rank_sum_threshold)
                {
                    PtmSet adjusted_ranksum = new PtmSet(set.ptm_combination);
                    adjusted_ranksum.ptm_rank_sum = rank_sum;
                    possible_ptmsets.Add(adjusted_ranksum);
                }
            }
            return possible_ptmsets;
        }

        #endregion Public Methods

        #region Private Methods

        private void assign_pf_identity(ExperimentalProteoform e, Proteoform theoretical_reference, PtmSet set, ProteoformRelation r, int sign, PtmSet change)
        {
            if (r.represented_ptmset == null)
            {
                r.represented_ptmset = change;
                if (r.RelationType == ProteoformComparison.ExperimentalExperimental) r.DeltaMass *= sign;
            }


            if (e.linked_proteoform_references == null)
            {
                e.linked_proteoform_references = new List<Proteoform>(this.linked_proteoform_references);
                e.linked_proteoform_references.Add(this);
                e.ptm_set = set;
            }

            if (theoretical_reference is TheoreticalProteoform && e.linked_proteoform_references.First() is TopDownProteoform) return;

            if (e.gene_name == null)
                e.gene_name = this.gene_name;
            else if (this.gene_name != null)
                e.gene_name.gene_names.Concat(this.gene_name.gene_names);
        }

        private int degraded_aas_count(string seq, PtmSet set, bool from_beginning)
        {
            List<string> missing_aas = set.ptm_combination.Select(ptm => ptm.modification).Where(m => m.modificationType == "Missing").Select(m => m.motif.Motif).ToList();
            int degraded = 0;
            if (missing_aas.Count != 0)
            {
                foreach (char c in from_beginning ? seq.ToCharArray() : seq.ToCharArray().Reverse())
                {
                    if (missing_aas.Contains(c.ToString().ToUpper()))
                        degraded++;
                    else break;
                }
            }
            return degraded;
        }
        #endregion Private Methods

    }
}
