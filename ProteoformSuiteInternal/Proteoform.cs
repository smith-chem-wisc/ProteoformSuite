using Proteomics;
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
                        String.Join("; ", ptm_set.ptm_combination.Select(ptm => ptm.position > 0 ? ptm.modification.id + "@" + ptm.position  : Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).ToList()) :
                        String.Join("; ", ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).ToList());
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

        public List<Proteoform> identify_connected_experimentals(List<PtmSet> all_possible_ptmsets, List<ModificationWithMass> all_mods_with_mass)
        {
            List<Proteoform> identified = new List<Proteoform>();
            //order by relation canddiate delta mass - observed delta mass. Null candidate ptmsets ordered last. 
            foreach (ProteoformRelation r in relationships.Where(r => r.Accepted).Distinct().OrderBy(r => r.candidate_ptmset != null ? Math.Abs(r.candidate_ptmset.mass - r.DeltaMass) : 1e6).ToList())
            {
                //check for connected experimental
                Proteoform e = r.connected_proteoforms.OfType<ExperimentalProteoform>().FirstOrDefault(p => p != this);
                if (e == null) //if no connected experimental...
                {
                    if (this as ExperimentalProteoform == null) //make sure this isn't experimental (don't want to go E --> T or E --> TD) 
                    {
                        e = r.connected_proteoforms.OfType<TopDownProteoform>().FirstOrDefault(p => p != this); //check for connected topdowns
                        if (e == null) continue; //on TD looking at T
                    }
                    else continue; //on E looking at TD or T
                }
    
            
                double mass_tolerance = modified_mass / 1000000 * (double)Sweet.lollipop.mass_tolerance;
                int sign = Math.Sign(e.modified_mass - modified_mass);
                 //if peak is null (topdown relation) use relation deltamass. Otherwise, use peak delta mass. 
                double deltaM = r.peak == null? r.DeltaMass : Math.Sign(r.peak.DeltaMass) < 0 ? r.peak.DeltaMass : sign * r.peak.DeltaMass; // give EE relations the correct sign, but don't switch negative ET relation deltaM's
                TheoreticalProteoform theoretical_base = this as TheoreticalProteoform != null ?
                    this as TheoreticalProteoform : //Theoretical starting point
                    (linked_proteoform_references.First() as TheoreticalProteoform != null ?
                        linked_proteoform_references.First() as TheoreticalProteoform : //Experimental or TD with theoretical reference
                        null); //Experimental or TD without theoretical reference
                string theoretical_base_sequence = theoretical_base != null ? theoretical_base.sequence : "";
                //when form TD relations, already generate best candidate ptmset for relation
                PtmSet best_addition = (this as TopDownProteoform != null || e as TopDownProteoform != null) ? r.candidate_ptmset : generate_possible_added_ptmsets(r.peak.possiblePeakAssignments.Where(p => Math.Sign(p.mass) == Math.Sign(deltaM)).ToList() , deltaM, mass_tolerance, all_mods_with_mass, theoretical_base, 1)
                    .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(x.mass - deltaM) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                    .FirstOrDefault();

                PtmSet best_loss = null;
                if (!(this as TopDownProteoform != null || e as TopDownProteoform != null)) //don't want to remove any PTMs from topdown node (for now, TD only matches to exact E match)
                {
                    foreach (PtmSet set in all_possible_ptmsets)
                    {
                        bool within_loss_tolerance = deltaM >= -set.mass - mass_tolerance && deltaM <= -set.mass + mass_tolerance;
                        var these_mods = this.ptm_set.ptm_combination.Select(ptm => ptm.modification);
                        var those_mods = set.ptm_combination.Select(ptm => ptm.modification); // all must be in the current set to remove them
                        bool can_be_removed = those_mods.All(m => these_mods.Contains(m));
                        bool better_than_current_best_loss = best_loss == null || Math.Abs(deltaM - (-set.mass)) < Math.Abs(deltaM - (-best_loss.mass));
                        if (can_be_removed && within_loss_tolerance && better_than_current_best_loss)
                        {
                            best_loss = set;
                        }
                    }
                }

                // If they're the same and someone hasn't labeled 0 difference with a "ModificationWithMass", then label it null
                if (best_addition == null && best_loss == null && Math.Abs(r.peak == null? r.DeltaMass : r.peak.DeltaMass) <= mass_tolerance)
                {
                    lock (r) lock (e) assign_pf_identity(e, e as TopDownProteoform != null? e.ptm_set : ptm_set, r, sign, null); //if on T going to TD, want TD to keep its ptmset (has positional info)
                    identified.Add(e);
                }

                if (best_addition == null && best_loss == null)
                    continue;

                // Make the new ptmset with ptms removed or added
                PtmSet with_mod_change = null;
                if (best_loss == null)
                {
                    PtmSet set = e as TopDownProteoform != null ? e.ptm_set : this.ptm_set; //if on T going to TD, want TD to keep its ptmset (has positional info)
                    with_mod_change = new PtmSet(new List<Ptm>(set.ptm_combination.Concat(best_addition.ptm_combination).Where(ptm => ptm.modification.monoisotopicMass != 0).ToList()));
                }
                else
                {
                    List<Ptm> new_combo = new List<Ptm>(this.ptm_set.ptm_combination);
                    foreach (Ptm ptm in best_loss.ptm_combination)
                    {
                        new_combo.Remove(new_combo.FirstOrDefault(asdf => asdf.modification == ptm.modification));
                    }
                    with_mod_change = new PtmSet(new_combo);
                }

                lock (r) lock (e)
                    assign_pf_identity(e, with_mod_change, r, sign, best_loss != null ? best_loss : best_addition);
                identified.Add(e);
            }
            return identified;
        }

        public List<PtmSet> generate_possible_added_ptmsets(List<PtmSet> possible_peak_assignments, double deltaM, double mass_tolerance, List<ModificationWithMass> all_mods_with_mass,
            TheoreticalProteoform theoretical_base, int additional_ptm_penalty)
        {
            List<ModificationWithMass> known_mods = theoretical_base.ExpandedProteinList.SelectMany(p => p.OneBasedPossibleLocalizedModifications.ToList()).SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList();
            List<PtmSet> possible_ptmsets = new List<PtmSet>();

            int n_terminal_degraded_aas = degraded_aas_count(theoretical_base.sequence, ptm_set, true);
            int c_terminal_degraded_aas = degraded_aas_count(theoretical_base.sequence, ptm_set, false);
            foreach (PtmSet set in possible_peak_assignments)
            {
                List<ModificationWithMass> mods_in_set = set.ptm_combination.Select(ptm => ptm.modification).ToList();

                int rank_sum = additional_ptm_penalty * (set.ptm_combination.Sum(m => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(m.modification, out UnlocalizedModification x) ? x.ptm_count : 1) - 1); // penalize additional PTMs

                foreach (ModificationWithMass m in mods_in_set)
                {
                    int mod_rank = Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(m, out UnlocalizedModification u) ? u.ptm_rank : Sweet.lollipop.modification_ranks[m.monoisotopicMass];

                    if (m.monoisotopicMass == 0)
                    {
                        rank_sum += mod_rank;
                        continue;
                    }
                    int begin = theoretical_base.begin;
                    bool could_be_m_retention = m.modificationType == "AminoAcid" && m.motif.Motif == "M" && begin == 2 && !ptm_set.ptm_combination.Select(p => p.modification).Contains(m);
                    bool motif_matches_n_terminus = n_terminal_degraded_aas < theoretical_base.sequence.Length && m.motif.Motif == theoretical_base.sequence[n_terminal_degraded_aas].ToString();
                    bool motif_matches_c_terminus = c_terminal_degraded_aas < theoretical_base.sequence.Length && m.motif.Motif == theoretical_base.sequence[theoretical_base.sequence.Length - c_terminal_degraded_aas - 1].ToString();
                    bool cannot_be_degradation = !motif_matches_n_terminus && !motif_matches_c_terminus;
                    if ((m.modificationType == "Missing" && cannot_be_degradation)
                        || (m.modificationType == "AminoAcid" && !could_be_m_retention)
                        || (u != null ? u.require_proteoform_without_mod : false && set.ptm_combination.Count > 1)
                        //if topdown relation, only allow mod types of missing, amino acid, or deconvolution error
                        || (this as TopDownProteoform != null && m.modificationType != "Missing" && m.modificationType != "AminoAcid" && m.modificationType != "Deconvolution Error"))
                    {
                        rank_sum = Int32.MaxValue;
                        break;
                    }

                    bool could_be_n_term_degradation = m.modificationType == "Missing" && motif_matches_n_terminus;
                    bool could_be_c_term_degradation = m.modificationType == "Missing" && motif_matches_c_terminus;
                    bool likely_cleavage_site = could_be_n_term_degradation && Sweet.lollipop.likely_cleavages.Contains(theoretical_base.sequence[n_terminal_degraded_aas].ToString())
                        || could_be_c_term_degradation && Sweet.lollipop.likely_cleavages.Contains(theoretical_base.sequence[theoretical_base.sequence.Length - c_terminal_degraded_aas - 1].ToString());

                    rank_sum -= Convert.ToInt32(Sweet.lollipop.theoretical_database.variableModifications.Contains(m)); // favor variable modifications over regular modifications of the same mass

                    // In order of likelihood:
                    // 1. First, we observe I/L/A cleavage to be the most common, 
                    // 1. "Fatty Acid" is a list of modifications prevalent in yeast or bacterial analysis, 
                    // 1. and unlocalized modifications are a subset of modifications in the intact_mods.txt list that should be included in intact analysis (handled in unlocalized modification)
                    // 2. Second, other degradations and methionine cleavage are weighted mid-level
                    // 3. Missed monoisotopic errors are considered, but weighted towards the bottom. This should allow missed monoisotopics with common modifications like oxidation, but not rare ones.  (handled in unlocalized modification)
                    if (likely_cleavage_site)  
                        rank_sum += Sweet.lollipop.mod_rank_first_quartile / 2;
                    else if (could_be_m_retention || could_be_n_term_degradation || could_be_c_term_degradation)
                        rank_sum += Sweet.lollipop.mod_rank_second_quartile;
                    else if (m.modificationType == "Deconvolution Error")
                        rank_sum += Sweet.lollipop.neucode_labeled ? 
                            Sweet.lollipop.mod_rank_third_quartile :   //in neucode-labeled data, fewer missed monoisotopics - don't prioritize
                            1 ; //in label-free, more missed monoisotoipcs, should prioritize (set to same priority as variable modification)
                    else
                        rank_sum += known_mods.Concat(Sweet.lollipop.theoretical_database.variableModifications).Contains(m) ?
                            mod_rank :
                            mod_rank + Sweet.lollipop.mod_rank_first_quartile / 2; // Penalize modifications that aren't known for this protein and push really rare ones out of the running if they're not in the protein entry
                }

                if (rank_sum <= Sweet.lollipop.mod_rank_sum_threshold)
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

        private void assign_pf_identity(Proteoform e, PtmSet set, ProteoformRelation r, int sign, PtmSet change)
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

            if (e.gene_name == null)
                e.gene_name = this.gene_name;
            else
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
