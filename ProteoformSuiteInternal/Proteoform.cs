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
        public string ptm_description { get; set; } = "";

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
                        String.Join("; ", ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).ToList());
            }
        }

        public int begin { get; set; }
        public int end { get; set; }

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
            //do relations first closest to candidate ptmset delta mass, then in order of relation delta mass (need to do in same order every round)
            foreach (ProteoformRelation r in relationships.Where(r => r.Accepted).OrderBy(r => r.candidate_ptmset != null ? Math.Abs(r.candidate_ptmset.mass - r.DeltaMass) : r.DeltaMass * 1e6).Distinct().ToList())
            {
                ExperimentalProteoform e = r.connected_proteoforms.OfType<ExperimentalProteoform>().FirstOrDefault(p => p != this);
                if (e == null) { continue; }// Looking at an ET pair, expecting an EE pair

                double mass_tolerance = modified_mass / 1000000 * Sweet.lollipop.mass_tolerance;
                int sign = Math.Sign(e.modified_mass - modified_mass);
                double deltaM = Math.Sign(r.peak.DeltaMass) < 0 ? r.peak.DeltaMass : sign * r.peak.DeltaMass; // give EE relations the correct sign, but don't switch negative ET relation deltaM's
                TheoreticalProteoform theoretical_base = this as TheoreticalProteoform != null ?
                    this as TheoreticalProteoform : //Theoretical starting point
                    (linked_proteoform_references.First() as TheoreticalProteoform != null ?
                        linked_proteoform_references.First() as TheoreticalProteoform : //Experimental with theoretical reference
                        null); //Experimental without theoretical reference

                List<PtmSet> possible_additions = r.peak.possiblePeakAssignments.Where(p => Math.Abs(p.mass - deltaM) <= 1).ToList(); // EE relations have PtmSets around both positive and negative deltaM, so remove the ones around the opposite of the deltaM of interest
                PtmSet best_addition = generate_possible_added_ptmsets(possible_additions, deltaM, mass_tolerance, all_mods_with_mass, theoretical_base, 1)
                    .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(x.mass - deltaM) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                    .FirstOrDefault();

                PtmSet best_loss = null;
                foreach (PtmSet set in all_possible_ptmsets)
                {
                    bool within_loss_tolerance = deltaM >= -set.mass - mass_tolerance && deltaM <= -set.mass + mass_tolerance;
                    List<ModificationWithMass> these_mods = this.ptm_set.ptm_combination.Select(ptm => ptm.modification).ToList();
                    List<ModificationWithMass> those_mods = set.ptm_combination.Select(ptm => ptm.modification).ToList(); // all must be in the current set to remove them
                    bool can_be_removed = those_mods.All(m1 => these_mods.Count(m2 => m2.id == m1.id) >= those_mods.Count(m2 => m2.id == m1.id)); //# of each mod in current set must be greater than or equal to # in set to remove.
                    bool better_than_current_best_loss = best_loss == null || Math.Abs(deltaM - (-set.mass)) < Math.Abs(deltaM - (-best_loss.mass));
                    if (can_be_removed && within_loss_tolerance && better_than_current_best_loss)
                    {
                        best_loss = set;
                    }
                }

                // If they're the same and someone hasn't labeled 0 difference with a "ModificationWithMass", then label it null
                if (best_addition == null && best_loss == null && Math.Abs(r.peak.DeltaMass) <= mass_tolerance)
                {
                    lock (r) lock (e) assign_pf_identity(e, ptm_set, r, sign, null, theoretical_base);
                    identified.Add(e);
                }

                if (best_addition == null && best_loss == null)
                {
                    continue;
                }

                // Make the new ptmset with ptms removed or added
                PtmSet with_mod_change = null;
                if (best_loss == null)
                {
                    with_mod_change = new PtmSet(new List<Ptm>(this.ptm_set.ptm_combination.Concat(best_addition.ptm_combination).Where(ptm => ptm.modification.monoisotopicMass != 0).ToList()));
                }
                else
                {
                    List<Ptm> new_combo = new List<Ptm>(this.ptm_set.ptm_combination);
                    foreach (Ptm ptm in best_loss.ptm_combination)
                    {
                        new_combo.Remove(new_combo.FirstOrDefault(asdf => asdf.modification.Equals(ptm.modification)));
                    }
                    with_mod_change = new PtmSet(new_combo);
                }

                lock (r) lock (e)
                        assign_pf_identity(e, with_mod_change, r, sign, best_loss != null ? best_loss : best_addition, theoretical_base);
                identified.Add(e);
            }
            return identified;
        }

        public List<PtmSet> generate_possible_added_ptmsets(List<PtmSet> possible_peak_assignments, double deltaM, double mass_tolerance, List<ModificationWithMass> all_mods_with_mass,
            TheoreticalProteoform theoretical_base, int additional_ptm_penalty)
        {
            List<ModificationWithMass> known_mods = theoretical_base.ExpandedProteinList.SelectMany(p => p.OneBasedPossibleLocalizedModifications.ToList()).SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList();
            List<PtmSet> possible_ptmsets = new List<PtmSet>();

            foreach (PtmSet set in possible_peak_assignments)
            {
                List<ModificationWithMass> mods_in_set = set.ptm_combination.Select(ptm => ptm.modification).ToList();

                int rank_sum = additional_ptm_penalty * (set.ptm_combination.Sum(m => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(m.modification, out UnlocalizedModification x) ? x.ptm_count : 1) - 1); // penalize additional PTMs

                foreach (ModificationWithMass m in mods_in_set)
                {
                    int mod_rank = Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(m, out UnlocalizedModification u) ? u.ptm_rank : Sweet.lollipop.modification_ranks.TryGetValue(m.monoisotopicMass, out int x) ? x : Sweet.lollipop.mod_rank_sum_threshold;

                    if (m.monoisotopicMass == 0)
                    {
                        rank_sum += mod_rank;
                        continue;
                    }

                    bool could_be_m_retention = m.modificationType == "AminoAcid" && m.motif.ToString() == "M" && theoretical_base.begin == 2 && this.begin == 2 && !ptm_set.ptm_combination.Any(p => p.modification.Equals(m));
                    bool motif_matches_n_terminus = begin >= 1 && begin - 1 < theoretical_base.sequence.Length && m.motif.ToString() == theoretical_base.sequence[begin - 1].ToString();
                    bool motif_matches_c_terminus = end >= 1 && end - 1 < theoretical_base.sequence.Length && m.motif.ToString() == theoretical_base.sequence[end - 1].ToString();

                    bool cannot_be_degradation = !motif_matches_n_terminus && !motif_matches_c_terminus;
                    if (m.modificationType == "Missing" && cannot_be_degradation
                        || m.modificationType == "AminoAcid" && !could_be_m_retention
                        || (u != null ? u.require_proteoform_without_mod : false) && set.ptm_combination.Count > 1)
                    {
                        rank_sum = Int32.MaxValue;
                        break;
                    }

                    bool could_be_n_term_degradation = m.modificationType == "Missing" && motif_matches_n_terminus;
                    bool could_be_c_term_degradation = m.modificationType == "Missing" && motif_matches_c_terminus;
                    rank_sum -= Convert.ToInt32(Sweet.lollipop.theoretical_database.variableModifications.Contains(m)); // favor variable modifications over regular modifications of the same mass

                    // In order of likelihood:
                    // 1. First, we observe I/L/A cleavage to be the most common, other degradations and methionine cleavage are weighted mid-level
                    // 2. Missed monoisotopic errors are considered, but weighted towards the bottom. This should allow missed monoisotopics with common modifications like oxidation, but not rare ones.  (handled in unlocalized modification)
                    if (could_be_m_retention || could_be_n_term_degradation || could_be_c_term_degradation)
                    {
                        rank_sum += Sweet.lollipop.mod_rank_first_quartile / 2;
                    }
                    else if (m.modificationType == "Deconvolution Error")
                    {
                        rank_sum += Sweet.lollipop.neucode_labeled ?
                            Sweet.lollipop.mod_rank_third_quartile :   //in neucode-labeled data, fewer missed monoisotopics - don't prioritize
                            1; //in label-free, more missed monoisotoipcs, should prioritize (set to same priority as variable modification)
                    }
                    else
                    {
                        rank_sum += known_mods.Concat(Sweet.lollipop.theoretical_database.variableModifications).Contains(m) ?
                            mod_rank :
                            mod_rank + Sweet.lollipop.mod_rank_first_quartile / 2; // Penalize modifications that aren't known for this protein and push really rare ones out of the running if they're not in the protein entry
                    }
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

        private void assign_pf_identity(ExperimentalProteoform e, PtmSet set, ProteoformRelation r, int sign, PtmSet change, TheoreticalProteoform theoretical_base)
        {
            if (r.represented_ptmset == null)
            {
                r.represented_ptmset = change;
                if (r.RelationType == ProteoformComparison.ExperimentalExperimental)
                {
                    r.DeltaMass *= sign;
                }
            }

            if (e.linked_proteoform_references == null)
            {
                e.linked_proteoform_references = new List<Proteoform>(this.linked_proteoform_references);
                e.linked_proteoform_references.Add(this);
                e.ptm_set = set;
                e.ambiguous = this as ExperimentalProteoform != null ? (this as ExperimentalProteoform).ambiguous : false;
                e.begin = this.begin;
                e.end = this.end;
                List<Ptm> remove = new List<Ptm>();
                foreach (var mod in set.ptm_combination.Where(m => m.modification.modificationType == "Missing"))
                {
                    if (theoretical_base.sequence[this.begin - theoretical_base.begin].ToString() == mod.modification.motif.ToString())
                    {
                        e.begin++;
                        remove.Add(mod); //dont have in ptmset --> change the begin & end
                    }
                    else if (theoretical_base.sequence[this.end - this.begin].ToString() == mod.modification.motif.ToString())
                    {
                        e.end--;
                        remove.Add(mod);
                    }
                }
                foreach (var mod in set.ptm_combination.Where(m => m.modification.modificationType == "AminoAcid"))
                {
                    e.begin--;
                    remove.Add(mod);
                }
                foreach (var ptm in remove)
                {
                    e.ptm_set.ptm_combination.Remove(ptm);
                }
                e.ptm_set = new PtmSet(e.ptm_set.ptm_combination);
            }
            //if already been assigned -- check if gene name != this gene name ==> ambiguous and same length path
            else if (!e.topdown_id && (e.gene_name.get_prefered_name(Lollipop.preferred_gene_label) != this.gene_name.get_prefered_name(Lollipop.preferred_gene_label)))
            {
                e.ambiguous = true;
            }

            if (e.gene_name == null)
            {
                e.gene_name = this.gene_name;
            }
            else if (!e.topdown_id)
            {
                e.gene_name.gene_names.Concat(this.gene_name.gene_names);
            }
        }

        #endregion Private Methods
    }
}