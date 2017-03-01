using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proteomics;

namespace ProteoformSuiteInternal
{
    //Please see ProteoformRelation class for notes on naming this one.
    public class DeltaMassPeak : ProteoformRelation
    {
        private List<ProteoformRelation> _grouped_relations;
        public List<ProteoformRelation> grouped_relations
        {
            get { return _grouped_relations; }
            set
            {
                _grouped_relations = value;
                this.peak_relation_group_count = value.Count;
                this.peak_deltaM_average = value.Select(r => r.delta_mass).Average();
                this.peak_accepted = typeof(TheoreticalProteoform).IsAssignableFrom(grouped_relations[0].connected_proteoforms[1].GetType()) ?       
                     this.peak_relation_group_count >= Lollipop.min_peak_count_et : 
                     this.peak_relation_group_count >= Lollipop.min_peak_count_ee;               
            }
        }
        public double peak_deltaM_average { get; set; }
        public int peak_relation_group_count { get; set; }
        public double decoy_relation_count { get; set; }
        public double peak_group_fdr { get; set; }
        public bool peak_accepted { get; set; } = false;
        public string mass_shifter { get; set; } = "0";
        public List<Modification> possiblePeakAssignments { get; set; }
        public string possiblePeakAssignments_string { get { return String.Join("; ", possiblePeakAssignments.Select(m => m.id).ToArray()); } }
        public ProteoformRelation base_relation { get; set; }

        public DeltaMassPeak(ProteoformRelation base_relation, List<ProteoformRelation> relations_to_group) : base(base_relation)
        {
            this.base_relation = base_relation;

            if (!Lollipop.opening_results) this.find_nearby_relations(relations_to_group);
            else this.grouped_relations = relations_to_group;

            Parallel.ForEach<ProteoformRelation>(this.grouped_relations, relation =>
            {
                relation.peak = this;
                relation.accepted = this.peak_accepted;
            });

            if (!Lollipop.opening_results && Lollipop.updated_theoretical) this.possiblePeakAssignments = nearestPTMs(this.peak_deltaM_average);
        }

        /*(this needs to be done at the actual time of forming peaks or else the average is wrong so the peak can be formed out
            of incorrect relations (average shouldn't include relations already grouped into peaks)*/
        public List<ProteoformRelation> find_nearby_relations(List<ProteoformRelation> ungrouped_relations)
        {
            for (int i = 0; i < Lollipop.relation_group_centering_iterations; i++)
            {
                double center_deltaM;
                double peak_width_base;
                if (typeof(TheoreticalProteoform).IsAssignableFrom(ungrouped_relations[0].connected_proteoforms[1].GetType())) peak_width_base = Lollipop.peak_width_base_et;
                else peak_width_base = Lollipop.peak_width_base_ee;
                if (i > 0)
                
                    center_deltaM = peak_deltaM_average;

                else center_deltaM = this.delta_mass;
                                
                double lower_limit_of_peak_width = center_deltaM - peak_width_base / 2;
                double upper_limit_of_peak_width = center_deltaM + peak_width_base / 2;
                this.grouped_relations = ungrouped_relations.
                    Where(relation => relation.delta_mass >= lower_limit_of_peak_width && relation.delta_mass <= upper_limit_of_peak_width).ToList();
            }

            foreach (ProteoformRelation mass_difference in grouped_relations)
                foreach (Proteoform p in mass_difference.connected_proteoforms)
                    p.relationships.Add(mass_difference);

            return this.grouped_relations;
        }

        private List<Modification> nearestPTMs(double dMass)
        {
            List<ModificationWithMass> all_modifications = Lollipop.uniprotModificationTable.SelectMany(i => i.Value).OfType<ModificationWithMass>().ToList();
            List<Modification> possiblePTMs = all_modifications.Where(m => Math.Abs(dMass - m.monoisotopicMass) <= Lollipop.peak_width_base_et / 2).ToList<Modification>();
            return possiblePTMs;
        }

        public void calculate_fdr(Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            List<int> nearby_decoy_counts = new List<int>(from relation_list in decoy_relations.Values select find_nearby_decoys(relation_list).Count);
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
            this.decoy_relation_count = median_false_peak_count;
            this.peak_group_fdr = median_false_peak_count / (double)this.peak_relation_group_count;
        }

        public List<ProteoformRelation> find_nearby_decoys(List<ProteoformRelation> all_relations)
        {
            double lower_limit_of_peak_width = this.peak_deltaM_average - Lollipop.peak_width_base_et / 2;
            double upper_limit_of_peak_width = this.peak_deltaM_average + Lollipop.peak_width_base_et / 2;
            return all_relations.Where(relation => relation.delta_mass >= lower_limit_of_peak_width && relation.delta_mass <= upper_limit_of_peak_width).ToList();
        }

        public bool shift_experimental_masses(int shift, bool neucode_labeled)
        {
            if (this.relation_type != ProteoformComparison.et)
                return false; //Not currently intended for ee relations

            foreach (ProteoformRelation r in this._grouped_relations)
            {
                Proteoform p = r.connected_proteoforms[0];
                if (p is ExperimentalProteoform && ((ExperimentalProteoform)p).mass_shifted == false && Lollipop.proteoform_community.experimental_proteoforms.Contains(p))
                    ((ExperimentalProteoform)p).shift_masses(shift, neucode_labeled);
            }

            return true;
        }
    }
}
