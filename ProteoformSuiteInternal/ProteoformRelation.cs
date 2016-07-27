using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
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
                this.accepted = set_accepted(); //dependent on peak count
            }
        }

        public int unadjusted_group_count { get; set; } //"running sum"
        public string accession { get; set; } //theoretical for ET, null for EE
        public string name { get; set; } //theoretical for ET, null for EE
        public string fragment { get; set; }
        public string ptm_list { get; set; }
        public double proteoform_mass_1 { get; set; } //experiment for ET, experiment_1 for EE
        public double proteoform_mass_2 { get; set; } //theoretical for EE, experiment_2 for EE
        public double agg_intensity_1 { get; set; } //experiment for ET, experiment_1 for EE
        public double agg_intensity_2 { get; set; } //0 for ET, experiment_2 for EE
        public double agg_RT_1 { get; set; } //experiment for ET, experiment_1 for EE
        public double agg_RT_2 { get; set; } //0 for ET, experiment_2 for EE
        public int num_observations_1 { get; set; }
        public int num_observations_2 { get; set; } //0 for ET
        public int lysine_count { get; set; }
        public double group_adjusted_deltaM { get; set; }
        public int group_count { get; set; }
        public bool outside_no_mans_land { get; set; }

        public ProteoformRelation(Proteoform pf1, Proteoform pf2, ProteoformComparison relation_type, double delta_mass) : base(pf1, pf2, relation_type, delta_mass)
        {
            this.relation_type = relation_type;
            this.group_adjusted_deltaM = delta_mass;
            //pf1 is always experimental proteoform
            if (Lollipop.neucode_labeled) { this.lysine_count = pf1.lysine_count; }
            var ep = pf1 as ExperimentalProteoform;
            this.proteoform_mass_1 = ep.agg_mass;
            this.agg_intensity_1 = ep.agg_intensity;
            this.agg_RT_1 = ep.agg_rt;
            this.num_observations_1 = ep.observation_count;
            if (pf2.GetType() == typeof(TheoreticalProteoform))
            {
                var tp = pf2 as TheoreticalProteoform;
                this.fragment = tp.fragment;
                this.ptm_list = tp.ptm_descriptions;
                this.name = tp.name;
                this.proteoform_mass_2 = tp.modified_mass;
                this.accession = tp.accession;
            }
            else if (pf2.GetType() == typeof(ExperimentalProteoform))
            {
                var ep_2 = pf2 as ExperimentalProteoform;
                this.proteoform_mass_2 = ep_2.agg_mass;
                this.agg_intensity_2 = ep_2.agg_intensity;
                this.agg_RT_2 = ep_2.agg_rt;
                this.num_observations_2 = ep_2.observation_count;
            }
            this.outside_no_mans_land = set_outside_no_mans_land(); //only relations outside no-mans-land should be formed into peaks.
        }

        public ProteoformRelation(ProteoformRelation relation) : base(relation.connected_proteoforms[0], relation.connected_proteoforms[1], relation.relation_type, relation.delta_mass)
        {
            this.peak = relation.peak;
            this.mass_difference_group = relation.mass_difference_group;
        }

        private bool set_outside_no_mans_land()
        {
          if(Math.Abs(this.delta_mass - Math.Truncate(this.delta_mass)) >= Lollipop.no_mans_land_upperBound ||
                Math.Abs(this.delta_mass- Math.Truncate(this.delta_mass)) <= Lollipop.no_mans_land_lowerBound) { return true; }
          else { return false; }
        }

        private bool set_accepted()
        {
            if (this.group_count >= Lollipop.min_peak_count && outside_no_mans_land) { return true; }
            else { return false; }
        } 

        public void set_nearby_group(List<ProteoformRelation> all_relations)
        {
            calculate_unadjusted_group_count(all_relations);
        }

        /*(this needs to be done at the actual time of forming peaks or else the average is wrong so the peak can be formed out
            of incorrect relations (average shouldn't include relations already grouped into peaks)*/
                public List<ProteoformRelation> find_nearby_relations(List<ProteoformRelation> ungrouped_relations)
        {
            List<ProteoformRelation> nearby_relations = new List<ProteoformRelation>();
            double nearby_deltaM = this.delta_mass;
            for (int i = 0; i < Lollipop.relation_group_centering_iterations; i++)
            {
                double lower_limit_of_peak_width = nearby_deltaM - Lollipop.peak_width_base / 2;
                double upper_limit_of_peak_width = nearby_deltaM + Lollipop.peak_width_base / 2;
                nearby_relations = ungrouped_relations.Where(relation =>
                    relation.delta_mass >= lower_limit_of_peak_width && relation.delta_mass <= upper_limit_of_peak_width).ToList();
                nearby_deltaM = nearby_relations.Select(r => r.delta_mass).Average();
            }
            this.group_count = nearby_relations.Count;
            this.mass_difference_group = nearby_relations;

            Parallel.ForEach<ProteoformRelation>(mass_difference_group, mass_difference =>
            {
                mass_difference.connected_proteoforms[0].relationships.Add(mass_difference);
                mass_difference.connected_proteoforms[1].relationships.Add(mass_difference);
            });

            return this.mass_difference_group;
        }


        private void calculate_unadjusted_group_count(List<ProteoformRelation> all_relations)
        {
            double lower_limit_of_peak_width = this.delta_mass - Lollipop.peak_width_base / 2;
            double upper_limit_of_peak_width = this.delta_mass + Lollipop.peak_width_base / 2;
            this.unadjusted_group_count = all_relations.Where(relation => relation.delta_mass >= lower_limit_of_peak_width
            && relation.delta_mass <= upper_limit_of_peak_width).ToList().Count;
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
