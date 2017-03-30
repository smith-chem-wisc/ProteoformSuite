using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class Proteoform
    {
        public string accession { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; } = -1;
        public bool is_target { get; set; } = true;
        public bool is_decoy { get; set; } = false;
        public List<Proteoform> candidate_relatives { get; set; }
        public List<ProteoformRelation> relationships { get; set; } = new List<ProteoformRelation>();
        public ProteoformFamily family { get; set; }
        public PtmSet ptm_set { get; set; } = new PtmSet(new List<Ptm>());
        public int ptm_set_rank_sum { get; set; }
        public LinkedList<Proteoform> linked_proteoform_references { get; set; } // TheoreticalProteoform is first, Experimental chain comes afterwards
        public GeneName gene_name { get; set; }

        public Proteoform(string accession, double modified_mass, int lysine_count, bool is_target)
        {
            this.accession = accession;
            this.modified_mass = modified_mass;
            this.lysine_count = lysine_count;
            this.is_target = is_target;
            this.is_decoy = !is_target;
        }

        public Proteoform(string accession)
        {
            this.accession = accession;
        }

        public List<Proteoform> get_connected_proteoforms()
        {
            return relationships.Where(r => r.peak.peak_accepted).SelectMany(r => r.connected_proteoforms).ToList();
        }

        public void compute_ptm_rank_sum(Dictionary<double, int> mod_ranks)
        {
            ptm_set_rank_sum = ptm_set.ptm_combination.Sum(ptm => mod_ranks[ptm.modification.monoisotopicMass]);
        }

        public List<ExperimentalProteoform> identify_connected_experimentals(List<PtmSet> all_possible_ptmsets)
        {
            List<ExperimentalProteoform> identified = new List<ExperimentalProteoform>();
            foreach (ProteoformRelation r in relationships.Where(r => r.peak.peak_accepted).Distinct().ToList())
            {
                ExperimentalProteoform e = r.connected_proteoforms.OfType<ExperimentalProteoform>().FirstOrDefault(p => p != this);
                if (e == null) continue; // Looking at an ET pair, expecting an EE pair

                double mass_tolerance = this.modified_mass / 1000000 * (double)Lollipop.mass_tolerance;
                int sign = Math.Sign(e.modified_mass - modified_mass);
                double deltaM = Math.Sign(r.peak_center_deltaM) < 0 ? r.peak_center_deltaM : sign * r.peak_center_deltaM; // give EE relations the correct sign, but don't switch negative ET relation deltaM's
                TheoreticalProteoform theoretical_base = this as TheoreticalProteoform != null ?
                    this as TheoreticalProteoform : //Theoretical starting point
                    (linked_proteoform_references.First.Value as TheoreticalProteoform != null ?
                        linked_proteoform_references.First.Value as TheoreticalProteoform : //Experimental with theoretical reference
                        null); //Experimental without theoretical reference
                string theoretical_base_sequence = theoretical_base != null ? theoretical_base.sequence : "";
                int n_terminal_degraded_aas = degraded_aas_count(theoretical_base_sequence, this.ptm_set, true);
                int c_terminal_degraded_aas = degraded_aas_count(theoretical_base_sequence, this.ptm_set, false);

                PtmSet best_addition = generate_possible_added_ptmsets(r.peak.possiblePeakAssignments, deltaM, mass_tolerance, Lollipop.all_mods_with_mass,
                    theoretical_base, theoretical_base_sequence, n_terminal_degraded_aas, c_terminal_degraded_aas)
                    .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(x.mass - deltaM) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                    .FirstOrDefault();
                //int best_addition_ranksum = ordered_additions.FirstOrDefault().ptm_rank_sum;
                //PtmSet best_addition = ordered_additions.OrderBy(set => Math.Abs(set.mass - deltaM)).FirstOrDefault(x => x.ptm_rank_sum == best_addition_ranksum);

                ModificationWithMass best_loss = null;
                foreach (ModificationWithMass m in Lollipop.all_mods_with_mass)
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
                if (best_addition == null && best_loss == null && Math.Abs(r.peak_center_deltaM) <= mass_tolerance)
                {
                    lock (r) lock (e) assign_pf_identity(e, this, ptm_set, r, sign, null);
                    identified.Add(e);
                }

                if (best_addition == null && best_loss == null)
                    continue;

                PtmSet with_mod_change = best_loss != null ?
                    new PtmSet(new List<Ptm>(this.ptm_set.ptm_combination.Except(this.ptm_set.ptm_combination.Where(ptm => ptm.modification.Equals(best_loss))))) :
                    new PtmSet(new List<Ptm>(this.ptm_set.ptm_combination.Where(ptm => ptm.modification.monoisotopicMass != 0)).Concat(best_addition.ptm_combination).ToList());
                lock (r) lock (e)
                    assign_pf_identity(e, this, with_mod_change, r, sign, best_loss != null ? new PtmSet(new List<Ptm> { new Ptm(-1, best_loss) }) : best_addition);
                identified.Add(e);
            }
            return identified;
        }

        private List<PtmSet> generate_possible_added_ptmsets(List<PtmSet> possible_peak_assignments, double deltaM, double mass_tolerance, List<ModificationWithMass> all_mods_with_mass,
            TheoreticalProteoform theoretical_base, string theoretical_base_sequence, int n_terminal_degraded_aas, int c_terminal_degraded_aas)
        {
            List<ModificationWithMass> known_mods = theoretical_base.ExpandedProteinList.SelectMany(p => p.OneBasedPossibleLocalizedModifications.ToList()).SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList();
            List<PtmSet> possible_ptmsets = new List<PtmSet>();
            foreach (PtmSet set in possible_peak_assignments)
            {
                List<ModificationWithMass> mods_in_set = set.ptm_combination.Select(ptm => ptm.modification).ToList();
                //bool valid_or_no_unmodified = set.ptm_combination.Count == 1 || !mods_in_set.Any(m => m.monoisotopicMass == 0);
                //bool within_addition_tolerance = deltaM >= set.mass - mass_tolerance && deltaM <= set.mass + mass_tolerance;
                int rank_sum = 0;
                foreach (ModificationWithMass m in mods_in_set)
                {
                    if (m.monoisotopicMass == 0)
                    {
                        rank_sum += Lollipop.modification_ranks[m.monoisotopicMass];
                        continue;
                    }

                    bool could_be_m_retention = m.modificationType == "AminoAcid" && m.motif.Motif == "M" && theoretical_base.begin == 2 && !mods_in_set.Contains(m);
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
                    bool likely_cleavage_site = could_be_n_term_degradation && Lollipop.likely_cleavages.Contains(theoretical_base_sequence[n_terminal_degraded_aas].ToString())
                        || could_be_c_term_degradation && Lollipop.likely_cleavages.Contains(theoretical_base_sequence[theoretical_base_sequence.Length - c_terminal_degraded_aas - 1].ToString());

                    if (likely_cleavage_site || m.modificationType == "FattyAcid" || m.modificationType == "Unlocalized" || m.modificationType == "Adduct")
                        rank_sum += Lollipop.rank_first_quartile;
                    else if (could_be_n_term_degradation || could_be_c_term_degradation || could_be_m_retention)
                        rank_sum += Lollipop.rank_third_quartile;
                    else
                        rank_sum += known_mods.Contains(m) ? 
                            Lollipop.modification_ranks[m.monoisotopicMass] :
                            Math.Min(Lollipop.modification_ranks[m.monoisotopicMass] + Lollipop.rank_first_quartile, Lollipop.rank_sum_threshold); //Penalize modifications that aren't known for this protein
                }
                set.ptm_rank_sum = rank_sum + Lollipop.rank_first_quartile * (set.ptm_combination.Count - 1); //penalize the second PTM
                if (rank_sum <= Lollipop.rank_sum_threshold)
                    possible_ptmsets.Add(set);
            }
            return possible_ptmsets;
        }

        private void assign_pf_identity(ExperimentalProteoform e, Proteoform theoretical_reference, PtmSet set, ProteoformRelation r, int sign, PtmSet change)
        {
            if (r.represented_ptmset == null)
            {
                r.represented_ptmset = change;
                if (r.relation_type == ProteoformComparison.ee) r.delta_mass *= sign;
            }
            if (e.linked_proteoform_references == null)
            {
                e.linked_proteoform_references = new LinkedList<Proteoform>(this.linked_proteoform_references);
                e.linked_proteoform_references.AddLast(this);
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
    }
}
