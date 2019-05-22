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

        //if ambiguous id's store here
        //proteoform: theoretical starting point; first int: begin residue; last ent: end residue; PtmSet
        public List<Tuple<Proteoform, int, int, PtmSet>> ambiguous_identifications { get; set; } = new List<Tuple<Proteoform, int, int, PtmSet>>();

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

        public List<ExperimentalProteoform> identify_connected_experimentals()
        {
            List<ExperimentalProteoform> identified = new List<ExperimentalProteoform>();
            //do relations first closest to candidate ptmset delta mass, then in order of relation delta mass (need to do in same order every round)
            foreach (ProteoformRelation r in relationships.Where(r => r.Accepted).OrderBy(r => r.candidate_ptmset != null ? Math.Abs(r.candidate_ptmset.mass - r.DeltaMass) : r.DeltaMass * 1e6).Distinct().ToList())
            {
                ExperimentalProteoform e = r.connected_proteoforms.OfType<ExperimentalProteoform>().FirstOrDefault(p => p != this);
                if (e == null) { continue; }// Looking at an ET pair, expecting an EE pair

                   TheoreticalProteoform theoretical_base = this as TheoreticalProteoform != null ?
                    this as TheoreticalProteoform : //Theoretical starting point
                    (linked_proteoform_references.First() as TheoreticalProteoform != null ?
                        linked_proteoform_references.First() as TheoreticalProteoform : //Experimental with theoretical reference
                        null); //Experimental without theoretical reference

                double mass_tolerance = modified_mass / 1000000 * Sweet.lollipop.mass_tolerance;
                PtmSet with_mod_change = determine_mod_change(e, this, theoretical_base, r, this.ptm_set);

                if (with_mod_change == null && Math.Abs(r.peak.DeltaMass) <= mass_tolerance)
                {
                    lock (r) lock (e) assign_pf_identity(e, ptm_set, r, theoretical_base);
                    identified.Add(e);
                }

                if (with_mod_change == null)
                {
                    continue;
                }

                lock (r) lock (e)
                        assign_pf_identity(e, with_mod_change, r, theoretical_base);
                identified.Add(e);
            }
            return identified;
        }

        private static PtmSet determine_mod_change(ExperimentalProteoform e, Proteoform p, TheoreticalProteoform theoretical_base, ProteoformRelation r, PtmSet this_ptmset)
        {
            double mass_tolerance = p.modified_mass / 1000000 * Sweet.lollipop.mass_tolerance;
            int sign = Math.Sign(e.modified_mass - p.modified_mass);
            double deltaM = Math.Sign(r.peak.DeltaMass) < 0 ? r.peak.DeltaMass : sign * r.peak.DeltaMass; // give EE relations the correct sign, but don't switch negative ET relation deltaM's


            List<PtmSet> possible_additions = r.peak.possiblePeakAssignments.Where(peak => Math.Abs(peak.mass - deltaM) <= 1).ToList(); // EE relations have PtmSets around both positive and negative deltaM, so remove the ones around the opposite of the deltaM of interest
            PtmSet best_addition = generate_possible_added_ptmsets(possible_additions, Sweet.lollipop.theoretical_database.all_mods_with_mass, theoretical_base, p.begin, p.end, p.ptm_set, 1, true)
                .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(x.mass - deltaM) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                .FirstOrDefault();

            PtmSet best_loss = null;
            foreach (PtmSet set in Sweet.lollipop.theoretical_database.all_possible_ptmsets)
            {
                bool within_loss_tolerance = deltaM >= -set.mass - mass_tolerance && deltaM <= -set.mass + mass_tolerance;
                List<Modification> these_mods = this_ptmset.ptm_combination.Select(ptm => ptm.modification).ToList();
                List<Modification> those_mods = set.ptm_combination.Select(ptm => ptm.modification).ToList(); // all must be in the current set to remove them
                bool can_be_removed = those_mods.All(m1 => these_mods.Count(m2 => m2.OriginalId == m1.OriginalId) >= those_mods.Count(m2 => m2.OriginalId == m1.OriginalId)); //# of each mod in current set must be greater than or equal to # in set to remove.
                bool better_than_current_best_loss = best_loss == null || Math.Abs(deltaM - (-set.mass)) < Math.Abs(deltaM - (-best_loss.mass));
                if (can_be_removed && within_loss_tolerance && better_than_current_best_loss)
                {
                    best_loss = set;
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
                with_mod_change = new PtmSet(new List<Ptm>(this_ptmset.ptm_combination.Concat(best_addition.ptm_combination).Where(ptm => ptm.modification.MonoisotopicMass != 0).ToList()));
            }
            else
            {
                List<Ptm> new_combo = new List<Ptm>(this_ptmset.ptm_combination);
                foreach (Ptm ptm in best_loss.ptm_combination)
                {
                    new_combo.Remove(new_combo.FirstOrDefault(asdf => asdf.modification.Equals(ptm.modification)));
                }
                with_mod_change = new PtmSet(new_combo);
            }

            if (r.represented_ptmset == null)
            {
                r.represented_ptmset = best_loss == null ? best_addition : best_loss;
                if (r.RelationType == ProteoformComparison.ExperimentalExperimental)
                {
                    r.DeltaMass *= sign;
                }
            }

            return with_mod_change;
        }

        public static List<PtmSet> generate_possible_added_ptmsets(List<PtmSet> possible_peak_assignments, List<Modification> all_mods_with_mass,
            TheoreticalProteoform theoretical_base, int begin, int end, PtmSet ptm_set, int additional_ptm_penalty, bool final_assignment)
        {
            List<Modification> known_mods = theoretical_base.ExpandedProteinList.SelectMany(p => p.OneBasedPossibleLocalizedModifications.ToList()).SelectMany(kv => kv.Value).ToList();
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

                    if (could_be_m_retention || could_be_n_term_degradation || could_be_c_term_degradation )
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

        #endregion Public Methods

        #region Private Methods

        private void assign_pf_identity(ExperimentalProteoform e, PtmSet set, ProteoformRelation r, TheoreticalProteoform theoretical_base)
        {
            if (e.linked_proteoform_references == null)
            {
                e.linked_proteoform_references = new List<Proteoform>(this.linked_proteoform_references);
                e.linked_proteoform_references.Add(this);
                e.ptm_set = set;
                e.begin = this.begin;
                e.end = this.end;
                List<Ptm> remove = new List<Ptm>();

                //do retention of M first
                foreach (var mod in set.ptm_combination.Where(m => m.modification.ModificationType == "AminoAcid"))
                {
                    e.begin--;
                    remove.Add(mod);
                }


                foreach (var mod in set.ptm_combination.Where(m => m.modification.ModificationType == "Missing"))
                {
                    if (theoretical_base.sequence[this.begin - theoretical_base.begin].ToString() ==
                        mod.modification.Target.ToString())
                    {
                        e.begin++;
                        remove.Add(mod); //dont have in ptmset --> change the begin & end
                    }
                    else if (theoretical_base.sequence[this.end - this.begin].ToString() ==
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
                    e.gene_name = this.gene_name;
                }
                else if (!e.topdown_id)
                {
                    e.gene_name.gene_names.Concat(this.gene_name.gene_names);
                }

            }
            else
            {
                //check if assign 
                int begin = this.begin;
                int end = this.end;
                PtmSet ptm_set = set;
                List<Ptm> remove = new List<Ptm>();
                //do retention of M first
                foreach (var mod in set.ptm_combination.Where(m => m.modification.ModificationType == "AminoAcid"))
                {
                    begin--;
                    remove.Add(mod);
                }

                foreach (var mod in set.ptm_combination.Where(m => m.modification.ModificationType == "Missing"))
                {
                    if (theoretical_base.sequence[this.begin - theoretical_base.begin].ToString() ==
                        mod.modification.Target.ToString())
                    {
                        begin++;
                        remove.Add(mod); //dont have in ptmset --> change the begin & end
                    }
                    else if (theoretical_base.sequence[this.end - this.begin].ToString() ==
                             mod.modification.Target.ToString())
                    {
                        end--;
                        remove.Add(mod);
                    }
                }

                foreach (var ptm in remove)
                {
                    ptm_set.ptm_combination.Remove(ptm);
                }

                ptm_set = new PtmSet(ptm_set.ptm_combination);

                if (e.gene_name.get_prefered_name(Lollipop.preferred_gene_label) !=
                    this.gene_name.get_prefered_name(Lollipop.preferred_gene_label)
                    || e.begin != begin || e.end != end || !e.ptm_set.same_ptmset(ptm_set, true))
                {
                    e.ambiguous = true;
                    Proteoform linked_proteoform_reference =
                        this.linked_proteoform_references == null || this.linked_proteoform_references.Count == 0
                            ? this
                            : this.linked_proteoform_references.First();
                    Tuple<Proteoform, int, int, PtmSet> new_id =
                        new Tuple<Proteoform, int, int, PtmSet>(linked_proteoform_reference, begin, end, ptm_set);
                    lock (e.ambiguous_identifications)
                    {
                        if (!e.ambiguous_identifications.Any(p =>
                            p.Item1.gene_name.get_prefered_name(Lollipop.preferred_gene_label) ==
                            new_id.Item1.gene_name.get_prefered_name(Lollipop.preferred_gene_label)
                            && p.Item2 == new_id.Item2 && p.Item3 == new_id.Item3 &&
                            p.Item4.same_ptmset(new_id.Item4, true)))
                        {
                            e.ambiguous_identifications.Add(new_id);
                        }
                    }
                }
            }

            if (this as ExperimentalProteoform != null && (this as ExperimentalProteoform).ambiguous)
            {
                foreach (var id in this.ambiguous_identifications)
                {
                    TheoreticalProteoform id_theoretical_base = id.Item1 as TheoreticalProteoform;
                    int begin = id.Item2;
                    int end = id.Item3;
                    var remove = new List<Ptm>();

                    var ptm_set = determine_mod_change(e, this, id_theoretical_base, r, id.Item4);
                    if (ptm_set == null) continue;
                    //do retention of M first
                    foreach (var mod in ptm_set.ptm_combination.Where(m => m.modification.ModificationType == "AminoAcid"))
                    {
                        begin--;
                        remove.Add(mod);
                    }
                    foreach (var mod in ptm_set.ptm_combination.Where(m => m.modification.ModificationType == "Missing"))
                    {
                        if (id_theoretical_base.sequence[id.Item2 - id.Item1.begin].ToString() == mod.modification.Target.ToString())
                        {
                            begin++;
                            remove.Add(mod); //dont have in ptmset --> change the begin & end
                        }
                        else if (id_theoretical_base.sequence[id.Item3 - id.Item2].ToString() == mod.modification.Target.ToString())
                        {
                            end--;
                            remove.Add(mod);
                        }
                    }
                    foreach (var ptm in remove)
                    {
                        ptm_set.ptm_combination.Remove(ptm);
                    }
                    ptm_set = new PtmSet(ptm_set.ptm_combination);
                    lock (e.ambiguous_identifications)
                    {
                        var new_id = new Tuple<Proteoform, int, int, PtmSet>(id.Item1, begin, end, ptm_set);
                        if ((e.gene_name.get_prefered_name(Lollipop.preferred_gene_label) !=
                                new_id.Item1.gene_name.get_prefered_name(Lollipop.preferred_gene_label)
                                || e.begin != new_id.Item2 || e.end != new_id.Item3 || !e.ptm_set.same_ptmset(new_id.Item4, true)) &&
                                !e.ambiguous_identifications.Any(p =>
                            p.Item1.gene_name.get_prefered_name(Lollipop.preferred_gene_label) ==
                            new_id.Item1.gene_name.get_prefered_name(Lollipop.preferred_gene_label)
                            && p.Item2 == new_id.Item2 && p.Item3 == new_id.Item3 &&
                            p.Item4.same_ptmset(new_id.Item4, true)))
                        {
                            e.ambiguous_identifications.Add(new_id);
                            e.ambiguous = true;
                        }
                    }
                }
            }

            e.uniprot_mods = "";
            foreach (string mod in e.ptm_set.ptm_combination.Concat(e.ambiguous_identifications.SelectMany(i => i.Item4.ptm_combination)).Where(ptm => ptm.modification.ModificationType != "Deconvolution Error").Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).ToList().Distinct().OrderBy(m => m))
            {
                // positions with mod
                List<int> theo_ptms = theoretical_base.ExpandedProteinList.First()
                    .OneBasedPossibleLocalizedModifications
                    .Where(p => p.Key >= e.begin && p.Key <= e.end
                                                 && p.Value.Select(m => UnlocalizedModification.LookUpId(m)).Contains(mod))
                    .Select(m => m.Key).ToList();
                if (theo_ptms.Count > 0)
                {
                    e.uniprot_mods += mod + " @ " + string.Join(", ", theo_ptms) + "; ";
                }
                if (e.ptm_set.ptm_combination.Select(ptm => UnlocalizedModification.LookUpId(ptm.modification))
                        .Count(m => m == mod) > theo_ptms.Count
                    || e.ambiguous_identifications.Any(i => i.Item4.ptm_combination.Select(ptm => UnlocalizedModification.LookUpId(ptm.modification))
                                                                .Count(m => m == mod) > theo_ptms.Count))
                {
                    e.novel_mods = true;
                }
            }

            //else if (!e.topdown_id && e.gene_name.get_prefered_name(Lollipop.preferred_gene_label) != this.gene_name.get_prefered_name(Lollipop.preferred_gene_label)
            // && e.linked_proteoform_references.Count == this.linked_proteoform_references.Count + 1)
            //{
            //    e.ambiguous = true;
            //}
        }

        #endregion Private Methods
    }
}