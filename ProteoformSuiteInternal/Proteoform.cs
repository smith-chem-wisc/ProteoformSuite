using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class Proteoform
    {

        #region Public Properties

        public string accession { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; } = -1;
        public bool is_target { get; set; } = true;
        public List<Proteoform> candidate_relatives { get; set; }
        public List<ProteoformRelation> relationships { get; set; } = new List<ProteoformRelation>();
        public ProteoformFamily family { get; set; }

        /// <summary>
        /// Contains a list of proteoforms traced before arriving at this one. The first is a TheoreticalProteoform starting point in the family.
        /// </summary>
        public LinkedList<Proteoform> linked_proteoform_references { get; set; }
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
                        String.Join("; ", ptm_set.ptm_combination.Select(ptm => ptm.modification.id));
            }
        }

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
            return relationships.Where(r => r.peak.peak_accepted).SelectMany(r => r.connected_proteoforms).ToList();
        }

        public List<ExperimentalProteoform> identify_connected_experimentals(List<PtmSet> all_possible_ptmsets, List<ModificationWithMass> all_mods_with_mass)
        {
            List<ExperimentalProteoform> identified = new List<ExperimentalProteoform>();
            foreach (ProteoformRelation r in relationships.Where(r => r.peak.peak_accepted).Distinct().ToList())
            {
                ExperimentalProteoform e = r.connected_proteoforms.OfType<ExperimentalProteoform>().FirstOrDefault(p => p != this);
                if (e == null) continue; // Looking at an ET pair, expecting an EE pair

                double mass_tolerance = this.modified_mass / 1000000 * (double)Lollipop.mass_tolerance;
                int sign = Math.Sign(e.modified_mass - modified_mass);
                double deltaM = Math.Sign(r.peak.peak_deltaM_average) < 0 ? r.peak.peak_deltaM_average : sign * r.peak.peak_deltaM_average; // give EE relations the correct sign, but don't switch negative ET relation deltaM's
                TheoreticalProteoform theoretical_base = this as TheoreticalProteoform != null ?
                    this as TheoreticalProteoform : //Theoretical starting point
                    (linked_proteoform_references.First.Value as TheoreticalProteoform != null ?
                        linked_proteoform_references.First.Value as TheoreticalProteoform : //Experimental with theoretical reference
                        null); //Experimental without theoretical reference
                string theoretical_base_sequence = theoretical_base != null ? theoretical_base.sequence : "";

                PtmSet best_addition = generate_possible_added_ptmsets(r.peak.possiblePeakAssignments, deltaM, mass_tolerance, all_mods_with_mass, theoretical_base, theoretical_base_sequence, Lollipop.rank_first_quartile / 2)
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
                if (best_addition == null && best_loss == null && Math.Abs(r.peak.peak_deltaM_average) <= mass_tolerance)
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
            TheoreticalProteoform theoretical_base, string theoretical_base_sequence, int additional_ptm_penalty)
        {
            List<ModificationWithMass> known_mods = theoretical_base.ExpandedProteinList.SelectMany(p => p.OneBasedPossibleLocalizedModifications.ToList()).SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList();
            List<PtmSet> possible_ptmsets = new List<PtmSet>();

            int n_terminal_degraded_aas = degraded_aas_count(theoretical_base_sequence, ptm_set, true);
            int c_terminal_degraded_aas = degraded_aas_count(theoretical_base_sequence, ptm_set, false);
            foreach (PtmSet set in possible_peak_assignments)
            {
                List<ModificationWithMass> mods_in_set = set.ptm_combination.Select(ptm => ptm.modification).ToList();

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

                    rank_sum -= Convert.ToInt32(Lollipop.variableModifications.Contains(m)); // favor variable modifications over regular modifications of the same mass

                    // In list of priority:
                    // 1. First, we observe I/L/A cleavage to be the most common, 
                    // 1. "Fatty Acid" is a list of modifications prevalent in yeast or bacterial analysis, 
                    // 1. and unlocalized modifications are a subset of modifications in the intact_mods.txt list that should be included in intact analysis
                    // 2. Second, other degradations and methionine cleavage are weighted mid-level
                    // 3. Missed monoisotopic errors are considered, but weighted towards the bottom. This should allow missed monoisotopics with common modifications like oxidation, but not rare ones.
                    if (likely_cleavage_site || m.modificationType == "FattyAcid" || m.modificationType == "Unlocalized")  
                        rank_sum += Lollipop.rank_first_quartile;
                    else if (could_be_m_retention || could_be_n_term_degradation || could_be_c_term_degradation)
                        rank_sum += Lollipop.rank_second_quartile;
                    else if (m.modificationType == "Deconvolution Error")
                        rank_sum += Lollipop.rank_third_quartile;
                    else
                        rank_sum += known_mods.Concat(Lollipop.variableModifications).Contains(m) ?
                            Lollipop.modification_ranks[m.monoisotopicMass] :
                            Lollipop.modification_ranks[m.monoisotopicMass] + Lollipop.rank_first_quartile / 2; // Penalize modifications that aren't known for this protein and push really rare ones out of the running if they're not in the protein entry
                }
                set.ptm_rank_sum = rank_sum + additional_ptm_penalty * (set.ptm_combination.Count - 1); // penalize additional PTMs
                if (rank_sum <= Lollipop.rank_sum_threshold)
                    possible_ptmsets.Add(set);
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
                if (r.relation_type == ProteoformComparison.ExperimentalExperimental) r.delta_mass *= sign;
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
        #endregion Private Methods

    }
}
