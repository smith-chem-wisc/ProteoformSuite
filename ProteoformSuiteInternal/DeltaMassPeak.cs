using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    //Please see ProteoformRelation class for notes on naming this one.
    public class DeltaMassPeak : ProteoformRelation
    {
        #region Public Properties

        public List<ProteoformRelation> grouped_relations { get; set; }
        public double peak_deltaM_average { get; set; }
        public int peak_relation_group_count { get { return grouped_relations.Count; } }
        public double decoy_relation_count { get; set; }
        public double peak_group_fdr { get; set; }
        public bool peak_accepted { get; set; } = false;
        public string mass_shifter { get; set; } = "0";
        public ProteoformRelation base_relation { get; set; }
        public List<PtmSet> possiblePeakAssignments { get; set; }
        public string possiblePeakAssignments_string { get; set; }

        #endregion Public Properties

        #region Public Constructor

        public DeltaMassPeak(ProteoformRelation base_relation, List<ProteoformRelation> relations_to_group)
            : base(base_relation)
        {
            peak = this;
            lock (base_relation)
            {
                this.base_relation = base_relation;
                base_relation.peak = this;
            }

            grouped_relations = find_nearby_relations(relations_to_group);
            peak_accepted = grouped_relations != null && grouped_relations.Count > 0 && grouped_relations.First().relation_type == ProteoformComparison.ExperimentalTheoretical ?
                peak_relation_group_count >= Lollipop.min_peak_count_et :
                peak_relation_group_count >= Lollipop.min_peak_count_ee;

            possiblePeakAssignments = nearestPTMs(peak_deltaM_average, relation_type).ToList();
            possiblePeakAssignments_string = "[" + String.Join("][", possiblePeakAssignments.Select(ptmset => String.Join(";", ptmset.ptm_combination.Select(m => m.modification.id)))) + "]";
        }

        #endregion Public Constructor

        #region Private Methods

        /*(this needs to be done at the actual time of forming peaks or else the average is wrong so the peak can be formed out
            of incorrect relations (average shouldn't include relations already grouped into peaks)*/
        private List<ProteoformRelation> find_nearby_relations(List<ProteoformRelation> ungrouped_relations)
        {
            if (ungrouped_relations.Count <= 0)
            {
                grouped_relations = new List<ProteoformRelation>();
                return grouped_relations;
            }

            for (int i = 0; i < Lollipop.relation_group_centering_iterations; i++)
            {
                double center_deltaM = i > 0 ? peak_deltaM_average : this.delta_mass;
                double peak_width_base = ungrouped_relations.First().connected_proteoforms[1] as TheoreticalProteoform != null ?
                    Lollipop.peak_width_base_et :
                    Lollipop.peak_width_base_ee;
                double lower_limit_of_peak_width = center_deltaM - peak_width_base / 2;
                double upper_limit_of_peak_width = center_deltaM + peak_width_base / 2;
                grouped_relations = ungrouped_relations.Where(relation => relation.delta_mass >= lower_limit_of_peak_width && relation.delta_mass <= upper_limit_of_peak_width).ToList();
                peak_deltaM_average = grouped_relations.Select(r => r.delta_mass).Average();
            }

            foreach (ProteoformRelation mass_difference in grouped_relations)
                foreach (Proteoform p in mass_difference.connected_proteoforms)
                    lock (p) p.relationships.Add(mass_difference);

            return grouped_relations;
        }

        private List<ProteoformRelation> find_nearby_decoys(List<ProteoformRelation> all_relations)
        {
            double lower_limit_of_peak_width = (all_relations[0].relation_type == ProteoformComparison.ExperimentalDecoy) ? peak_deltaM_average - Lollipop.peak_width_base_et / 2 : peak_deltaM_average - Lollipop.peak_width_base_ee / 2;
            double upper_limit_of_peak_width = (all_relations[0].relation_type == ProteoformComparison.ExperimentalDecoy) ? peak_deltaM_average + Lollipop.peak_width_base_et / 2 : peak_deltaM_average + Lollipop.peak_width_base_ee / 2;
            return all_relations.Where(relation => relation.delta_mass >= lower_limit_of_peak_width && relation.delta_mass <= upper_limit_of_peak_width).ToList();
        }

        #endregion Private Methods

        #region Public Methods

        public void calculate_fdr(Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            List<int> nearby_decoy_counts = decoy_relations.Values.Select(v => find_nearby_decoys(v).Count).ToList();
            nearby_decoy_counts.Sort();
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
            if (relation_type != ProteoformComparison.ExperimentalTheoretical)
                return false; //Not currently intended for ee relations

            foreach (ProteoformRelation r in this.grouped_relations)
            {
                Proteoform p = r.connected_proteoforms[0];
                if (p is ExperimentalProteoform && ((ExperimentalProteoform)p).mass_shifted == false && Lollipop.proteoform_community.experimental_proteoforms.Contains(p))
                    ((ExperimentalProteoform)p).shift_masses(shift, neucode_labeled);
            }

            return true;
        }

        #endregion Public Methods
    }
}
