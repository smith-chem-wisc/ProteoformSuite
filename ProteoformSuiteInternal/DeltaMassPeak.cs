using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    //Please see ProteoformRelation class for notes on naming this one.
    public class DeltaMassPeak : ProteoformRelation
    {
        public double peak_width { get; } = Lollipop.peak_width_base;
        public double decoy_count { get; set; }
        public double group_fdr { get; set; }
        public ProteoformRelation base_relation { get; set; }

        public DeltaMassPeak(ProteoformRelation base_relation) : base(base_relation)
        {
            this.base_relation = base_relation;
           Parallel.ForEach<ProteoformRelation>(base_relation.mass_difference_group, relation => relation.peak = this);
        }

        public void calculate_fdr(Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            List<int> nearby_decoy_counts = new List<int>(from relation_list in decoy_relations.Values select find_nearby_decoys(relation_list).Count);
            nearby_decoy_counts.Sort();
            double median_false_peak_count;
            if (nearby_decoy_counts.Count % 2 == 0) //is even
            {
                int middle = nearby_decoy_counts.Count / 2;
                median_false_peak_count = (double)nearby_decoy_counts[middle] + (double)nearby_decoy_counts[middle + 1];
            }
            else
            {
                median_false_peak_count = (double)nearby_decoy_counts[(nearby_decoy_counts.Count - 1) / 2];
            }
            this.decoy_count = median_false_peak_count;
            this.group_fdr = median_false_peak_count / (double)group_count;
        }

        public List<ProteoformRelation> find_nearby_decoys(List<ProteoformRelation> all_relations)
        {
            double lower_limit_of_peak_width = this.group_adjusted_deltaM - Lollipop.peak_width_base / 2;
            double upper_limit_of_peak_width = this.group_adjusted_deltaM + Lollipop.peak_width_base / 2;
            return all_relations.Where(relation => relation.group_adjusted_deltaM >= lower_limit_of_peak_width && relation.group_adjusted_deltaM <= upper_limit_of_peak_width).ToList();
        }

        new public string as_tsv_row()
        {
            return String.Join("\t", new List<string> { this.connected_proteoforms[0].accession.ToString(), this.connected_proteoforms[1].accession.ToString(), this.delta_mass.ToString(), this.group_adjusted_deltaM.ToString(),
                this.group_count.ToString(), group_fdr.ToString() });
        }

        new public static string get_tsv_header()
        {
            return String.Join("\t", new List<string> { "proteoform1_accession", "proteoform2_accession", "delta_mass", "group_adjusted_deltaM", "group_count", "group_fdr" });
        }
    }
}
