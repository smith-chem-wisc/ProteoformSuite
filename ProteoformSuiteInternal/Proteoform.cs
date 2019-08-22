using Proteomics;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Chemistry;
using DocumentFormat.OpenXml.Wordprocessing;

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

        public PtmSet ptm_set
        {
            get
            {
                return _ptm_set;
            }

            set
            {
                _ptm_set = value;
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
        public ProteoformRelation relation_to_id { get; set; }

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

        public List<ExperimentalProteoform> identify_connected_experimentals(TheoreticalProteoform theoretical_base)
        {
            List<ExperimentalProteoform> identified = new List<ExperimentalProteoform>();
            //do relations first closest to candidate ptmset delta mass, then in order of relation delta mass (need to do in same order every round)
            foreach (ProteoformRelation r in relationships.Where(r => r.Accepted).OrderBy(r => r.candidate_ptmset != null ? Math.Abs(r.candidate_ptmset.mass - r.DeltaMass) : r.DeltaMass * 1e6).Distinct().ToList())
            {
                ExperimentalProteoform e = r.connected_proteoforms.OfType<ExperimentalProteoform>().FirstOrDefault(p => p != this);

                if (e == null) { continue; }// Looking at an ET pair, expecting an EE pair

                if (Sweet.lollipop.identify_from_td_nodes && this as TopDownProteoform != null && e as TopDownProteoform != null) continue; //between two TD nodes

                //going back the way you came
                if (this.linked_proteoform_references.Contains(e) && this.linked_proteoform_references.First() == e.linked_proteoform_references.First()
                   && (this as ExperimentalProteoform) != null && (this as ExperimentalProteoform).ambiguous_identifications.Count == 0)
                {
                    continue;
                }

                double mass_tolerance = modified_mass / 1000000 * Sweet.lollipop.mass_tolerance;
                PtmSet with_mod_change = determine_mod_change(e, this, theoretical_base, r, this.ptm_set, this.begin, this.end);

                if (with_mod_change == null && Math.Abs(r.peak.DeltaMass) <= mass_tolerance)
                {
                    lock (r) lock (e)
                        {
                            if (assign_pf_identity(e, ptm_set, this.begin, this.end, r, theoretical_base, true))
                            {
                                r.Identification = true;
                                identified.Add(e);
                            }
                        }
                    continue;
                }

                if (with_mod_change == null)
                {
                    continue;
                }

                lock (r) lock (e)
                    {
                        if (assign_pf_identity(e, with_mod_change, begin, end, r, theoretical_base, true))
                        {
                            {
                                r.Identification = true;
                                identified.Add(e);
                            }
                        }
                    }
            }

            return identified;
        }

        private static PtmSet determine_mod_change(ExperimentalProteoform e, Proteoform p,
            TheoreticalProteoform theoretical_base, ProteoformRelation r, PtmSet this_ptmset, int begin, int end)
        {
            double mass_tolerance = p.modified_mass / 1000000 * Sweet.lollipop.mass_tolerance;
            int sign = Math.Sign(e.modified_mass - p.modified_mass);
            double deltaM =
                Math.Sign(r.peak.DeltaMass) < 0
                    ? r.peak.DeltaMass
                    : sign * r.peak
                          .DeltaMass; // give EE relations the correct sign, but don't switch negative ET relation deltaM's


            List<PtmSet> possible_additions = r.peak.possiblePeakAssignments
                .Where(peak => Math.Abs(peak.mass - deltaM) <= 1)
                .ToList(); // EE relations have PtmSets around both positive and negative deltaM, so remove the ones around the opposite of the deltaM of interest

            PtmSet best_addition = generate_possible_added_ptmsets(possible_additions,
                    Sweet.lollipop.theoretical_database.all_mods_with_mass, theoretical_base, begin, end,
                    this_ptmset, 1, true)
                .OrderBy(x =>
                    (double)x.ptm_rank_sum +
                    Math.Abs(x.mass - deltaM) *
                    10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                .FirstOrDefault();


            PtmSet best_loss = null;

            foreach (PtmSet set in Sweet.lollipop.theoretical_database.all_possible_ptmsets)
            {
                bool within_loss_tolerance = deltaM >= -set.mass - mass_tolerance && deltaM <= -set.mass + mass_tolerance;
                if (within_loss_tolerance)
                {
                    List<Modification> these_mods = this_ptmset.ptm_combination.Select(ptm => ptm.modification).ToList();
                    List<Modification> those_mods = set.ptm_combination.Select(ptm => ptm.modification).ToList(); // all must be in the current set to remove them
                    bool can_be_removed = those_mods.All(m1 => these_mods.Count(m2 =>
                                                                   UnlocalizedModification.LookUpId(m2) ==
                                                                   UnlocalizedModification.LookUpId(m1)) >=
                                                               those_mods.Count(m2 =>
                                                                   UnlocalizedModification.LookUpId(m2) ==
                                                                   UnlocalizedModification.LookUpId(m1)));
                    bool better_than_current_best_loss = best_loss == null || Math.Abs(deltaM - (-set.mass)) < Math.Abs(deltaM - (-best_loss.mass));
                    if (can_be_removed && within_loss_tolerance && better_than_current_best_loss)
                    {
                        best_loss = set;
                    }
                }
            }


            if (best_addition == null && best_loss == null)
            {
                return null;
            }


            // Make the new ptmset with ptms removed or added
            PtmSet with_mod_change = null;
            if (best_loss == null)
            {
                with_mod_change = new PtmSet(new List<Ptm>(this_ptmset.ptm_combination
                    .Concat(best_addition.ptm_combination).Where(ptm => ptm.modification.MonoisotopicMass != 0)
                    .ToList()));
            }
            else
            {
                List<Ptm> new_combo = new List<Ptm>(this_ptmset.ptm_combination);
                foreach (Ptm ptm in best_loss.ptm_combination)
                {
                    new_combo.Remove(new_combo.FirstOrDefault(asdf => UnlocalizedModification.LookUpId(asdf.modification) == UnlocalizedModification.LookUpId(ptm.modification)));
                }
                with_mod_change = new PtmSet(new_combo);

            }


            if (r.represented_ptmset == null)
            {
                r.represented_ptmset = best_loss == null ? best_addition : best_loss;
                //if (r.RelationType == ProteoformComparison.ExperimentalExperimental)
                //{
                //    r.DeltaMass *= sign;
                //}
            }

            return with_mod_change;
        }

        public static List<PtmSet> generate_possible_added_ptmsets(List<PtmSet> possible_peak_assignments, List<Modification> all_mods_with_mass,
            TheoreticalProteoform theoretical_base, int begin, int end, PtmSet ptm_set, int additional_ptm_penalty, bool final_assignment)
        {
            List<Modification> known_mods = theoretical_base.ExpandedProteinList.SelectMany(p => p.OneBasedPossibleLocalizedModifications).SelectMany(kv => kv.Value).ToList();
            List<PtmSet> possible_ptmsets = new List<PtmSet>();

            foreach (PtmSet set in possible_peak_assignments)
            {
                List<Modification> mods_in_set = set.ptm_combination.Select(ptm => ptm.modification).ToList();

                int rank_sum = additional_ptm_penalty * (set.ptm_combination.Sum(m => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(m.modification, out UnlocalizedModification x) ? x.ptm_count : 1) - 1); // penalize additional PTMs
                foreach (Modification m in mods_in_set)
                {
                    int mod_rank = Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(m, out UnlocalizedModification u) ? u.ptm_rank : Sweet.lollipop.modification_ranks.TryGetValue(Math.Round((double)m.MonoisotopicMass, 5), out int x) ? x : Sweet.lollipop.mod_rank_sum_threshold;

                    bool could_be_m_retention = m.ModificationType == "AminoAcid" && m.Target.ToString() == "M" && theoretical_base.begin == 2 && begin == 2 && !ptm_set.ptm_combination.Any(p => p.modification.Equals(m));
                    bool motif_matches_n_terminus = begin - theoretical_base.begin >= 0 && begin - theoretical_base.begin < theoretical_base.sequence.Length && m.Target.ToString() == theoretical_base.sequence[begin - theoretical_base.begin].ToString() && !mods_in_set.Any(mod => mod.ModificationType == "AminoAcid" && mod.Target.ToString() == "M");
                    bool motif_matches_c_terminus = end - begin >= 0 && end - begin < theoretical_base.sequence.Length && m.Target.ToString() == theoretical_base.sequence[end - begin].ToString();

                    bool cannot_be_degradation = !motif_matches_n_terminus && !motif_matches_c_terminus;
                    if (m.ModificationType == "Missing" && cannot_be_degradation
                        || m.ModificationType == "AminoAcid" && !could_be_m_retention
                        || (u != null ? u.require_proteoform_without_mod : false) && set.ptm_combination.Count > 1)
                    {
                        rank_sum = Int32.MaxValue;
                        break;
                    }

                    bool could_be_n_term_degradation = m.ModificationType == "Missing" && motif_matches_n_terminus;
                    bool could_be_c_term_degradation = m.ModificationType == "Missing" && motif_matches_c_terminus;

                    //if selected, going to only allow mods in Mods folder (type "Common"), Missing, Missed Monoisotopic, known mods for that protein, or Unmodified
                    if (Sweet.lollipop.only_assign_common_or_known_mods && final_assignment)
                    {
                        if (!(m.MonoisotopicMass == 0 || m.ModificationType == "Common" || could_be_m_retention || could_be_n_term_degradation || could_be_c_term_degradation || m.ModificationType == "Deconvolution Error" || known_mods.Concat(Sweet.lollipop.theoretical_database.variableModifications).Contains(m) ||
                              known_mods.Select(mod => UnlocalizedModification.LookUpId(mod)).Contains(UnlocalizedModification.LookUpId(m))))
                        {
                            rank_sum = Int32.MaxValue;
                            break;
                        }
                    }

                    // In order of likelihood:
                    // 1. First, we observe I/L/A cleavage to be the most common, other degradations and methionine cleavage are weighted mid-level
                    // 2. Missed monoisotopic errors are considered, but weighted towards the bottom. This should allow missed monoisotopics with common modifications like oxidation, but not rare ones.  (handled in unlocalized modification)
                    if (m.MonoisotopicMass == 0)
                    {
                        rank_sum += mod_rank;
                        continue;
                    }

                    rank_sum -= Convert.ToInt32(Sweet.lollipop.theoretical_database.variableModifications.Contains(m)); // favor variable modifications over regular modifications of the same mass

                    if (could_be_m_retention || could_be_n_term_degradation || could_be_c_term_degradation)
                    {
                        rank_sum += Sweet.lollipop.mod_rank_first_quartile / 2;
                    }
                    else if (m.ModificationType == "Deconvolution Error")
                    {
                        rank_sum += Sweet.lollipop.neucode_labeled ?
                            Sweet.lollipop.mod_rank_third_quartile :   //in neucode-labeled data, fewer missed monoisotopics - don't prioritize
                            1; //in label-free, more missed monoisotoipcs, should prioritize (set to same priority as variable modification)
                        rank_sum -= additional_ptm_penalty;
                    }
                    else
                    {
                        //if annotated in DB for this, just add 1?
                        rank_sum += known_mods.Concat(Sweet.lollipop.theoretical_database.variableModifications).Contains(m) ||
                            known_mods.Select(mod => UnlocalizedModification.LookUpId(mod)).Contains(UnlocalizedModification.LookUpId(m))
                                ?
                            1 : //mod rank
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

        public static List<SpectrumMatch> get_possible_PSMs(string accession, PtmSet ptm_set, int begin, int end)
        {
            var bottom_up_PSMs = new List<SpectrumMatch>();
            //add BU PSMs
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.TryGetValue(accession.Split('_')[0].Split('-')[0], out var psms);
            if (psms != null)
            {
                bottom_up_PSMs.AddRange(psms.Where(s => s.begin >= begin && s.end <= end && s.ptm_list.All(m1 =>
                                                           ptm_set.ptm_combination.Count(m2 =>
                                                               UnlocalizedModification.LookUpId(m1.modification) ==
                                                                UnlocalizedModification.LookUpId(m2.modification)) >=
                                                            s.ptm_list.Count(m2 =>
                                                                UnlocalizedModification.LookUpId(m1.modification) ==
                                                                UnlocalizedModification.LookUpId(m2.modification)))));
            }

            return bottom_up_PSMs.OrderByDescending(p => p.ptm_list.Count).ToList();
        }

        #endregion Public Methods

        #region Private Methods

        private bool assign_pf_identity(ExperimentalProteoform e, PtmSet set, int begin, int end, ProteoformRelation r, TheoreticalProteoform theoretical_base, bool check_ambiguous_IDs)
        {
            bool identification_assigned = false;
            if (!Sweet.lollipop.id_use_ppm_tolerance || Math.Abs(e.calculate_mass_error(theoretical_base, set, begin, end) * 1e6 / e.modified_mass) < Sweet.lollipop.id_ppm_tolerance)
            {
                identification_assigned = true;
                if (e.linked_proteoform_references == null)
                {
                    if (this.linked_proteoform_references != null && this.linked_proteoform_references.Count > 0 && check_ambiguous_IDs)
                    {
                        e.linked_proteoform_references = new List<Proteoform>(this.linked_proteoform_references);
                        e.linked_proteoform_references.Add(this);
                    }
                    else //from top-down node or ambiguous ID
                    {
                        e.linked_proteoform_references = new List<Proteoform>() { theoretical_base };
                    }

                    e.relation_to_id = r;
                    e.ptm_set = new PtmSet(new List<Ptm>(set.ptm_combination));
                    e.begin = begin;
                    e.end = end;
                    List<Ptm> remove = new List<Ptm>();

                    //do retention of M first
                    foreach (var mod in e.ptm_set.ptm_combination.Where(m => m.modification.ModificationType == "AminoAcid"))
                    {
                        e.begin--;
                        remove.Add(mod);
                    }


                    foreach (var mod in e.ptm_set.ptm_combination.Where(m => m.modification.ModificationType == "Missing"))
                    {
                        if (!e.ptm_set.ptm_combination.Any(m => m.modification.ModificationType == "AminoAcid"))
                        {
                            if (theoretical_base.sequence[begin - theoretical_base.begin].ToString() ==
                                mod.modification.Target.ToString())
                            {
                                e.begin++;
                                remove.Add(mod); //dont have in ptmset --> change the begin & end
                            }
                        }
                        if (!remove.Contains(mod) && theoretical_base.sequence[end - begin].ToString() ==
                                 mod.modification.Target.ToString())
                        {
                            e.end--;
                            remove.Add(mod);
                        }
                    }

                    foreach (var ptm in remove)
                    {
                        e.ptm_set.ptm_combination.Remove(ptm);
                    }
                    e.ptm_set = new PtmSet(e.ptm_set.ptm_combination);

                    if (e.gene_name == null)
                    {
                        e.gene_name = theoretical_base.gene_name;
                    }
                    else
                    {
                        e.gene_name.gene_names.Concat(this.gene_name.gene_names);
                    }
                }

                else if(!this.linked_proteoform_references.Contains(theoretical_base))
                {
                    //check if assign 
                    int new_begin = begin;
                    int new_end = end;

                    PtmSet new_set = new PtmSet(new List<Ptm>(set.ptm_combination));
                    List<Ptm> remove = new List<Ptm>();
                    //do retention of M first
                    foreach (var mod in new_set.ptm_combination.Where(m => m.modification.ModificationType == "AminoAcid"))
                    {
                        new_begin--;
                        remove.Add(mod);
                    }

                    foreach (var mod in new_set.ptm_combination.Where(m => m.modification.ModificationType == "Missing"))
                    {
                        if (!new_set.ptm_combination.Any(m => m.modification.ModificationType == "AminoAcid"))
                        {
                            if (theoretical_base.sequence[begin - theoretical_base.begin].ToString() ==
                            mod.modification.Target.ToString())
                            {
                                new_begin++;
                                remove.Add(mod); //dont have in ptmset --> change the begin & end
                            }
                        }
                        if (!remove.Contains(mod) && theoretical_base.sequence[end - begin].ToString() ==
                                mod.modification.Target.ToString())
                        {
                            new_end--;
                            remove.Add(mod);
                        }
                    }

                    foreach (var ptm in remove)
                    {
                        new_set.ptm_combination.Remove(ptm);
                    }

                    new_set = new PtmSet(new_set.ptm_combination);

                    List<Modification> this_known_mods = theoretical_base.ExpandedProteinList.SelectMany(p => p.OneBasedPossibleLocalizedModifications).SelectMany(kv => kv.Value).ToList();
                    List<Modification> previous_id_known_mods = (e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.OneBasedPossibleLocalizedModifications).SelectMany(kv => kv.Value).ToList();

                    if (e.gene_name.primary!=
                  theoretical_base.gene_name.primary
                    || e.begin != new_begin || e.end != new_end || !e.ptm_set.same_ptmset(new_set, true))
                    {
                        AmbiguousIdentification new_id =
                            new AmbiguousIdentification(new_begin, new_end, new_set, r, theoretical_base);
                        lock (e.ambiguous_identifications)
                        {
                            if (!e.ambiguous_identifications.Any(p =>
                                p.theoretical_base.gene_name.primary ==
                                new_id.theoretical_base.gene_name.primary
                                && p.begin == new_id.begin && p.end == new_id.end &&
                                p.ptm_set.same_ptmset(new_id.ptm_set, true)))
                            {
                                e.ambiguous_identifications.Add(new_id);
                            }
                        }
                    }
                }
            }

            if (check_ambiguous_IDs)
            {
                if (this as ExperimentalProteoform != null && (this as ExperimentalProteoform).ambiguous_identifications.Count > 0)
                {
                    for (int i = 0; i < (this as ExperimentalProteoform).ambiguous_identifications.Count; i++)
                    {
                        // if ((this as ExperimentalProteoform).ambiguous_identifications.Count == 0) continue;
                        var id = (this as ExperimentalProteoform).ambiguous_identifications[i];

                        var new_set = determine_mod_change(e, this, id.theoretical_base, r, id.ptm_set, id.begin, id.end);
                        if (new_set != null)
                        {

                            if (assign_pf_identity(e, new_set, id.begin, id.end, r, id.theoretical_base, false))
                            {
                                identification_assigned = true;
                            }
                        }
                    }
                }
            }

            return identification_assigned;
        }

        #endregion Private Methods
    }
}