using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuite
{
    //Please see ProteoformRelation class for notes on naming this one.
    public class DeltaMassPeak : ProteoformRelation
    {
        public double peak_width { get; } = Lollipop.peak_width_base;
        public double group_fdr { get; set; }
        public ProteoformRelation base_relation { get; set; }

        public DeltaMassPeak(ProteoformRelation base_relation) : base(base_relation)
        {
            this.base_relation = base_relation;
            Parallel.ForEach<ProteoformRelation>(base_relation.mass_difference_group, relation => relation.peak = this);
        }

        public void calculate_fdr(Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            List<int> nearby_decoy_counts = new List<int>(from relation_list in decoy_relations.Values select find_nearby_relations(relation_list).Count);
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
            this.group_fdr = median_false_peak_count / (double)group_count;
        }

        public string as_csv_row()
        {
            return String.Join(",", new List<string> { this.connected_proteoforms[0].accession.ToString(), this.connected_proteoforms[1].accession.ToString(), this.delta_mass.ToString(), this.group_adjusted_deltaM.ToString(),
                this.group_count.ToString(), group_fdr.ToString() });
        }

        public static string get_csv_header()
        {
            return String.Join(",", new List<string> { "proteoform1_accession", "proteoform2_accession", "delta_mass", "group_adjusted_deltaM", "group_count", "group_fdr" });
        }
    }
}
