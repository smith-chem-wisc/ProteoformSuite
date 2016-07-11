using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuite
{
    //Types of comparisons, aka ProteoformFamily edges
    public enum ProteoformComparison 
    { 
        et, //Experiment-Theoretical comparisons
        ed, //Experiment-Decoy comparisons
        ee, //Experiment-Experiment comparisons
        ef  //Experiment-Experiment comparisons using unequal lysine counts
    }

    //I have not used MassDifference objects in the logic, since it is better to cast the comparisons immediately as
    //ProteoformRelation objects. However, I believe this hierarchy clarifies that ProteoformRelations are in fact
    //containers of mass differences, grouped nearby mass differences. Simply having only ProteoformRelation makes this distinction vague.
    //Calling ProteoformRelation "MassDifference" isn't appropriate, since it contains groups of mass differences.
    //That said, I like calling the grouped ProteoformRelations a peak, since a peak is often comprised of multiple observations:
    //of light in spectroscopy, of ions in mass spectrometry, and since the peak is a pileup on the mass difference axis, I've called it
    //DeltaMassPeak. I debated MassDifferencePeak, but I wanted to distance this class from MassDifference and draw the imagination
    //closer to the picture of the graph, in which we often say "deltaM" colloquially, whereas we tend to say "mass difference" when we're
    //referring to an individual value. 
    public class MassDifference
    {
        public Proteoform[] connected_proteoforms = new Proteoform[2];
        public ProteoformComparison relation_type;
        public double delta_mass { get; set; }
        public bool accepted { get; set; } = false;

        public MassDifference(Proteoform pf1, Proteoform pf2, ProteoformComparison relation_type, double delta_mass)
        {
            this.connected_proteoforms[0] = pf1;
            this.connected_proteoforms[1] = pf2;
            this.relation_type = relation_type;
            this.delta_mass = delta_mass;
        }
    }

    public class ProteoformRelation : MassDifference
    {
        public DeltaMassPeak peak { get; set; }
        private List<ProteoformRelation> _mass_difference_group;
        public List<ProteoformRelation> mass_difference_group
        {
            get { return _mass_difference_group; }
            set
            {
                this.group_count = value.Count;
                this.group_adjusted_deltaM = value.Select(r => r.delta_mass).Average();
                _mass_difference_group = value;
            }
        }
        public int unadjusted_group_count { get; set; }
        public double group_adjusted_deltaM { get; set; }
        public int group_count { get; set; }

        public ProteoformRelation(Proteoform pf1, Proteoform pf2, ProteoformComparison relation_type, double delta_mass) : base(pf1, pf2, relation_type, delta_mass)
        {
            this.group_adjusted_deltaM = delta_mass;
        }
        public ProteoformRelation(ProteoformRelation relation) : base(relation.connected_proteoforms[0], relation.connected_proteoforms[1], relation.relation_type, relation.delta_mass)
        {
            this.peak = relation.peak;
            this.mass_difference_group = relation.mass_difference_group;
        }

        public List<ProteoformRelation> accept_exclusive(List<ProteoformRelation> already_grouped)
        {
            this.mass_difference_group = this.mass_difference_group.Except(already_grouped).ToList();
            Parallel.ForEach<ProteoformRelation>(mass_difference_group, mass_difference =>
            {
                mass_difference.connected_proteoforms[0].relationships.Add(mass_difference);
                mass_difference.connected_proteoforms[1].relationships.Add(mass_difference);
            });
            return this.mass_difference_group;
        }

        public void set_nearby_group(List<ProteoformRelation> all_relations)
        {
            this.mass_difference_group = find_nearby_relations(all_relations);
        }

        public List<ProteoformRelation> find_nearby_relations(List<ProteoformRelation> all_relations)
        {
            List<ProteoformRelation> nearby_relations = new List<ProteoformRelation>();
            double nearby_deltaM = this.group_adjusted_deltaM;
            for (int i = 0; i < Lollipop.relation_group_centering_iterations; i++)
            {
                double lower_limit_of_peak_width = nearby_deltaM - Lollipop.peak_width_base / 2;
                double upper_limit_of_peak_width = nearby_deltaM + Lollipop.peak_width_base / 2;
                nearby_relations = all_relations.Where(relation =>
                    relation.group_adjusted_deltaM >= lower_limit_of_peak_width && relation.group_adjusted_deltaM <= upper_limit_of_peak_width).ToList();
                nearby_deltaM = nearby_relations.Select(r => r.delta_mass).Average();
            }
            return nearby_relations;
        }

        public string as_tsv_row()
        {
            return String.Join("\t", new List<string> { this.connected_proteoforms[0].accession.ToString(), this.connected_proteoforms[1].accession.ToString(), this.delta_mass.ToString(), this.group_adjusted_deltaM.ToString(),
                this.group_count.ToString() });
        }

        public static string get_tsv_header()
        {
            return String.Join("\t", new List<string> { "proteoform1_accession", "proteoform2_accession", "delta_mass", "group_adjusted_deltaM", "group_count" });
        }
    }
}
