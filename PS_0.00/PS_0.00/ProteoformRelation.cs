using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS_0._00
{
    public enum RelationType
    { //Types of comparisons, aka ProteoformFamily edges
        et, //Experiment-Theoretical comparisons
        ed, //Experiment-Decoy comparisons
        ee, //Experiment-Experiment comparisons
        ef  //Experiment-Experiment comparisons using unequal lysine counts
    }

    public class ProteoformRelation
    {
        public Proteoform pf1;
        public Proteoform pf2;
        public RelationType relation_type;
        private double _delta_mass;
        public double delta_mass
        {
            get { return this._delta_mass; }
            set
            {
                this._delta_mass = value;
                this.group_deltaM = value;
            }
        }
        public bool accepted { get; set; } = false;

        public double group_deltaM;
        public int group_count;
        public double group_fdr { get; set; }
        private List<ProteoformRelation> _relations_group;
        public List<ProteoformRelation> relations_group
        {
            get { return _relations_group; }
            set
            {
                this.group_count = value.Count;
                this.group_deltaM = value.Select(r => r.delta_mass).Average();
                _relations_group = value;
            }
        }

        public ProteoformRelation(Proteoform pf1, Proteoform pf2, RelationType relation_type, double delta_mass)
        {
            this.pf1 = pf1;
            this.pf2 = pf2;
            this.relation_type = relation_type;
            pf1.add_relationship(this, pf2);
            pf2.add_relationship(this, pf1);
            this.delta_mass = delta_mass;
        }

        //Assert this relation as the representative of the group
        //Excludes proteoforms that were already grouped, 
        //and then re-centers and calculates the FDR from the decoy relations within the re-centered peak range
        public void accept_exclusive_group(List<ProteoformRelation> already_grouped)
        {
            this.relations_group = this.relations_group.Except(already_grouped).ToList();
            Parallel.ForEach(this.relations_group, relation => relation.accepted = false);
            this.accepted = true;
        }

        public void set_nearby_group(List<ProteoformRelation> all_relations)
        {
            this.relations_group = find_nearby_relations(all_relations);
        }

        private List<ProteoformRelation> find_nearby_relations(List<ProteoformRelation> all_relations)
        {
            List<ProteoformRelation> nearby_relations = new List<ProteoformRelation>();
            double nearby_deltaM = this.group_deltaM;
            for (int i = 0; i < Lollipop.relation_group_centering_iterations; i++)
            {
                double lower_limit_of_peak_width = nearby_deltaM - Convert.ToDouble(Lollipop.peak_width_base) / 2;
                double upper_limit_of_peak_width = nearby_deltaM + Convert.ToDouble(Lollipop.peak_width_base) / 2;
                nearby_relations = all_relations.Where(relation =>
                    relation.group_deltaM >= lower_limit_of_peak_width && relation.group_deltaM <= upper_limit_of_peak_width).ToList();
                nearby_deltaM = nearby_relations.Select(r => r.delta_mass).Average();
            }
            return nearby_relations;
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
    }
}
