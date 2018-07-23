using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    //Please see ProteoformRelation class for notes on naming this one.
    public class DeltaMassPeak : IMassDifference
    {
        #region Private Fields

        private static int instance_counter;

        #endregion Private Fields

        #region Public Properties

        public List<ProteoformRelation> grouped_relations { get; set; } //target relations only. Decoy relations in the peak range have relation.peak set to this
        public double DeltaMass { get; set; }
        public int peak_relation_group_count { get { return grouped_relations.Count; } }
        public double decoy_relation_count { get; set; }
        public double peak_group_fdr { get; set; }
        public bool Accepted { get; set; }
        public string mass_shifter { get; set; } = "0";
        public List<PtmSet> possiblePeakAssignments { get; set; }
        public string possiblePeakAssignments_string { get; set; }
        public ProteoformComparison RelationType { get; set; }
        public int InstanceId { get; set; }

        #endregion Public Properties

        #region Public Constructor

        public DeltaMassPeak(ProteoformRelation base_relation, HashSet<ProteoformRelation> relations_to_group)
        {
            lock (base_relation)
            {
                base_relation.peak = this;
            }

            lock (Sweet.lollipop)
            {
                IncrementInstanceCounter(); //Not thread safe
            }

            RelationType = base_relation.RelationType;
            DeltaMass = base_relation.DeltaMass;
            InstanceId = instance_counter;

            grouped_relations = find_nearby_relations(relations_to_group);

            bool are_positive_candidates = Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(DeltaMass, 1), out List<PtmSet> positive_candidates);
            bool are_negative_candidates = Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(-DeltaMass, 1), out List<PtmSet> negative_candidates)
                && (RelationType == ProteoformComparison.ExperimentalExperimental || RelationType == ProteoformComparison.ExperimentalFalse);

            if (are_positive_candidates || are_negative_candidates)
            {
                List<PtmSet> candidates = (are_positive_candidates ? positive_candidates : new List<PtmSet>()).Concat(are_negative_candidates ? negative_candidates : new List<PtmSet>()).ToList();
                possiblePeakAssignments = candidates.Where(c => RelationType == ProteoformComparison.ExperimentalTheoretical || RelationType == ProteoformComparison.ExperimentalDecoy ?
                    Math.Abs(DeltaMass - c.mass) <= (grouped_relations.First().RelationType == ProteoformComparison.ExperimentalTheoretical ? Sweet.lollipop.peak_width_base_et : Sweet.lollipop.peak_width_base_ee) :
                    Math.Abs(Math.Abs(DeltaMass) - Math.Abs(c.mass)) <= (grouped_relations.First().RelationType == ProteoformComparison.ExperimentalTheoretical ? Sweet.lollipop.peak_width_base_et : Sweet.lollipop.peak_width_base_ee)).ToList();
            }
            else
            {
                possiblePeakAssignments = new List<PtmSet>();
            }

            possiblePeakAssignments_string = "[" + String.Join("][", possiblePeakAssignments.OrderBy(p => p.ptm_rank_sum).Select(ptmset =>
                String.Join(";", ptmset.ptm_combination.Select(ptm =>
                    Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id))).Distinct()) + "]";

            Accepted = grouped_relations != null && grouped_relations.Count > 0 && grouped_relations.First().RelationType == ProteoformComparison.ExperimentalTheoretical ?
                (peak_relation_group_count >= Sweet.lollipop.min_peak_count_et && (!Sweet.lollipop.et_accept_peaks_based_on_rank || possiblePeakAssignments.Count > 0 && possiblePeakAssignments.Any(p => p.ptm_rank_sum < Sweet.lollipop.mod_rank_first_quartile)))
                :
                (peak_relation_group_count >= Sweet.lollipop.min_peak_count_ee && (!Sweet.lollipop.ee_accept_peaks_based_on_rank || possiblePeakAssignments.Count > 0 && possiblePeakAssignments.Any(p => p.ptm_rank_sum < Sweet.lollipop.mod_rank_first_quartile)));
        }

        #endregion Public Constructor

        #region Private Methods

        /*(this needs to be done at the actual time of forming peaks or else the average is wrong so the peak can be formed out
            of incorrect relations (average shouldn't include relations already grouped into peaks)*/

        private List<ProteoformRelation> find_nearby_relations(HashSet<ProteoformRelation> ungrouped_relations)
        {
            if (ungrouped_relations.Count <= 0)
            {
                grouped_relations = new List<ProteoformRelation>();
                return grouped_relations;
            }

            for (int i = 0; i < Sweet.lollipop.relation_group_centering_iterations; i++)
            {
                double peak_width_base = ungrouped_relations.First().connected_proteoforms[1] as TheoreticalProteoform != null ?
                    Sweet.lollipop.peak_width_base_et :
                    Sweet.lollipop.peak_width_base_ee;
                double lower_limit_of_peak_width = DeltaMass - peak_width_base / 2;
                double upper_limit_of_peak_width = DeltaMass + peak_width_base / 2;
                grouped_relations = ungrouped_relations.Where(relation => relation.DeltaMass >= lower_limit_of_peak_width && relation.DeltaMass <= upper_limit_of_peak_width).ToList();
                DeltaMass = grouped_relations.Select(r => r.DeltaMass).Average();
            }

            foreach (ProteoformRelation mass_difference in grouped_relations)
            {
                foreach (Proteoform p in mass_difference.connected_proteoforms)
                {
                    lock (p) p.relationships.Add(mass_difference);
                }
            }

            return grouped_relations;
        }

        public int count_nearby_decoys(List<ProteoformRelation> all_relations)
        {
            if (all_relations.Count == 0) { return 0; }
            double lower_limit_of_peak_width = (all_relations[0].RelationType == ProteoformComparison.ExperimentalDecoy) ?
                DeltaMass - Sweet.lollipop.peak_width_base_et / 2 :
                DeltaMass - Sweet.lollipop.peak_width_base_ee / 2;

            double upper_limit_of_peak_width = (all_relations[0].RelationType == ProteoformComparison.ExperimentalDecoy) ?
                DeltaMass + Sweet.lollipop.peak_width_base_et / 2 :
                DeltaMass + Sweet.lollipop.peak_width_base_ee / 2;

            List<ProteoformRelation> decoys_in_peaks = all_relations.Where(relation => relation.DeltaMass >= lower_limit_of_peak_width && relation.DeltaMass <= upper_limit_of_peak_width).ToList();

            foreach (ProteoformRelation r in decoys_in_peaks)
            {
                lock (r)
                {
                    if (r.peak == null || this.peak_relation_group_count > r.peak.peak_relation_group_count)
                    {
                        r.Accepted = this.Accepted;
                        r.peak = this; //assign relation this peak so possible peak assignments used when identifying experimentals
                    }
                }
            }
            return decoys_in_peaks.Count;
        }

        private static void IncrementInstanceCounter()
        {
            instance_counter += 1;
        }

        #endregion Private Methods

        #region Public Methods

        public void calculate_fdr(Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            List<int> nearby_decoy_counts = decoy_relations.Values.Select(v => count_nearby_decoys(v)).OrderBy(x => x).ToList();
            double median_false_peak_count;
            if (nearby_decoy_counts.Count % 2 == 0) //is even
            {
                int middle = nearby_decoy_counts.Count / 2;
                median_false_peak_count = 0.5 * ((double)nearby_decoy_counts[middle] + (double)nearby_decoy_counts[middle - 1]);
            }
            else
            {
                median_false_peak_count = (double)nearby_decoy_counts[(nearby_decoy_counts.Count - 1) / 2];
            }
            decoy_relation_count = median_false_peak_count;
            peak_group_fdr = median_false_peak_count / (double)peak_relation_group_count;
        }

        public bool shift_experimental_masses(int shift, bool neucode_labeled)
        {
            if (RelationType != ProteoformComparison.ExperimentalTheoretical)
            {
                return false; //Not currently intended for ee relations
            }

            foreach (ProteoformRelation r in this.grouped_relations)
            {
                Proteoform p = r.connected_proteoforms[0];
                if (p is ExperimentalProteoform && ((ExperimentalProteoform)p).mass_shifted == false && Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Contains(p))
                {
                    (p as ExperimentalProteoform).shift_masses(shift, neucode_labeled);
                }
            }

            return true;
        }

        #endregion Public Methods
    }
}