using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    //Please see ProteoformRelation class for notes on naming this one.
    public class DeltaMassPeak : IMassDifference
    {

        #region Private Fields

        private static int instance_counter = 0;

        #endregion Private Fields

        #region Public Properties

        public List<ProteoformRelation> grouped_relations { get; set; } //target relations only. Decoy relations in the peak range have relation.peak set to this
        public double DeltaMass { get; set; }
        public int peak_relation_group_count { get { return grouped_relations.Count; } }
        public double decoy_relation_count { get; set; }
        public double peak_group_fdr { get; set; }
        public bool Accepted { get; set; } = false;
        public string mass_shifter { get; set; } = "0";
        public List<PtmSet> possiblePeakAssignments { get; set; }
        public string possiblePeakAssignments_string { get; set; }
        public ProteoformComparison RelationType { get; set; }
        public int InstanceId { get; set; }
        
        #endregion Public Properties

        #region Public Constructor

        public DeltaMassPeak(ProteoformRelation base_relation, List<ProteoformRelation> relations_to_group)
        {
            lock (base_relation)
            {
                base_relation.peak = this;
            }

            lock (SaveState.lollipop)
            {
                instance_counter += 1; //Not thread safe
            }

            RelationType = base_relation.RelationType;
            DeltaMass = base_relation.DeltaMass;
            InstanceId = instance_counter;

            grouped_relations = find_nearby_relations(relations_to_group);
            Accepted = grouped_relations != null && grouped_relations.Count > 0 && grouped_relations.First().RelationType == ProteoformComparison.ExperimentalTheoretical ?
                peak_relation_group_count >= SaveState.lollipop.min_peak_count_et :
                peak_relation_group_count >= SaveState.lollipop.min_peak_count_ee;

            List<PtmSet> candidates;
            if (SaveState.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(DeltaMass, 1), out candidates))
            {
                possiblePeakAssignments = candidates.Where(c => RelationType == ProteoformComparison.ExperimentalTheoretical || RelationType == ProteoformComparison.ExperimentalDecoy ?
                        Math.Abs(DeltaMass - c.mass) <= 0.05 :
                        Math.Abs(Math.Abs(DeltaMass) - Math.Abs(c.mass)) <= 0.05).ToList();
            }
            else possiblePeakAssignments = new List<PtmSet>();
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

            for (int i = 0; i < SaveState.lollipop.relation_group_centering_iterations; i++)
            {
                double peak_width_base = ungrouped_relations.First().connected_proteoforms[1] as TheoreticalProteoform != null ?
                    SaveState.lollipop.peak_width_base_et :
                    SaveState.lollipop.peak_width_base_ee;
                double lower_limit_of_peak_width = DeltaMass - peak_width_base / 2;
                double upper_limit_of_peak_width = DeltaMass + peak_width_base / 2;
                grouped_relations = ungrouped_relations.Where(relation => relation.DeltaMass >= lower_limit_of_peak_width && relation.DeltaMass <= upper_limit_of_peak_width).ToList();
                DeltaMass = grouped_relations.Select(r => r.DeltaMass).Average();
            }

            foreach (ProteoformRelation mass_difference in grouped_relations)
                foreach (Proteoform p in mass_difference.connected_proteoforms)
                    lock (p) p.relationships.Add(mass_difference);

            return grouped_relations;
        }

        public int count_nearby_decoys(List<ProteoformRelation> all_relations)
        {
            double lower_limit_of_peak_width = (all_relations[0].RelationType == ProteoformComparison.ExperimentalDecoy) ? DeltaMass - SaveState.lollipop.peak_width_base_et / 2 : DeltaMass - SaveState.lollipop.peak_width_base_ee / 2;
            double upper_limit_of_peak_width = (all_relations[0].RelationType == ProteoformComparison.ExperimentalDecoy) ? DeltaMass + SaveState.lollipop.peak_width_base_et / 2 : DeltaMass + SaveState.lollipop.peak_width_base_ee / 2;
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

        #endregion Private Methods

        #region Public Methods

        public void calculate_fdr(Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            List<int> nearby_decoy_counts = decoy_relations.Values.Select(v => count_nearby_decoys(v)).OrderBy(x => x).ToList();
            double median_false_peak_count;
            if (nearby_decoy_counts.Count == 1) //1 decoy database
            {
                median_false_peak_count = nearby_decoy_counts[0];
            }
            else if (nearby_decoy_counts.Count % 2 == 0) //is even
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
                return false; //Not currently intended for ee relations

            foreach (ProteoformRelation r in this.grouped_relations)
            {
                Proteoform p = r.connected_proteoforms[0];
                if (p is ExperimentalProteoform && ((ExperimentalProteoform)p).mass_shifted == false && SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Contains(p))
                    ((ExperimentalProteoform)p).shift_masses(shift, neucode_labeled);
            }

            return true;
        }

        #endregion Public Methods

    }
}
