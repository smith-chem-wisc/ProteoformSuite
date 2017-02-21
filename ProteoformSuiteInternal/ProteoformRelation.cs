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
        public int nearby_relations_count { get; set; } //"running sum"
        private List<ProteoformRelation> _nearby_relations;
        public List<ProteoformRelation> nearby_relations
        {
            get { return _nearby_relations; }
            set
            {
                _nearby_relations = value;
                this.nearby_relations_count = value.Count;
            }
        }
        public static bool mass_difference_is_outside_no_mans_land(double delta_mass)
        {
            return Math.Abs(delta_mass - Math.Truncate(delta_mass)) >= Lollipop.no_mans_land_upperBound ||
                Math.Abs(delta_mass - Math.Truncate(delta_mass)) <= Lollipop.no_mans_land_lowerBound;
        }
        public bool outside_no_mans_land
        {
            get { return ProteoformRelation.mass_difference_is_outside_no_mans_land(this.delta_mass); }
        }
        public int lysine_count { get; set; }
        public bool accepted { get; set; }

        public ProteoformRelation(Proteoform pf1, Proteoform pf2, ProteoformComparison relation_type, double delta_mass) : base(pf1, pf2, relation_type, delta_mass)
        {
            if (Lollipop.neucode_labeled) this.lysine_count = pf1.lysine_count;
        }

        public ProteoformRelation(ProteoformRelation relation) : base(relation.connected_proteoforms[0], relation.connected_proteoforms[1], relation.relation_type, relation.delta_mass)
        {
            this.peak = relation.peak;
            if (!Lollipop.opening_results) this.nearby_relations = relation.nearby_relations;
        }

        public List<ProteoformRelation> set_nearby_group(List<ProteoformRelation> all_relations)
        {
            double peak_width_base;
            if (all_relations[0].connected_proteoforms[1] is TheoreticalProteoform) peak_width_base = Lollipop.peak_width_base_et;
            else peak_width_base = Lollipop.peak_width_base_ee;
            double lower_limit_of_peak_width = this.delta_mass - peak_width_base / 2;
            double upper_limit_of_peak_width = this.delta_mass + peak_width_base / 2;
            this.nearby_relations = all_relations.Where(relation => relation.delta_mass >= lower_limit_of_peak_width
                && relation.delta_mass <= upper_limit_of_peak_width).ToList();
            return this.nearby_relations;
        }

        //public void generate_peak()
        //{
        //    this.peak = new DeltaMassPeak(this, Lollipop.proteoform_community.remaining_relations_outside_no_mans);
        //    if (Lollipop.decoy_databases > 0) this.peak.calculate_fdr(Lollipop.ed_relations);
        //}

        // FOR DATAGRIDVIEW DISPLAY
        public static string et_string = "Experiment-Theoretical";
        public static string ee_string = "Experiment-Experimental";
        public static string ed_string = "Experiment-Decoy";
        public static string ef_string = "Experiment-Unequal Lysine Count";

        public int peak_center_count
        {
            get { if (this.peak != null) return this.peak.peak_relation_group_count; else return -1000000; }
        }
        public double peak_center_deltaM
        {
            get { if (this.peak != null) return peak.peak_deltaM_average; else return Double.NaN; }
        }
        public string relation_type_string
        {
            get
            {
                if (this.relation_type == ProteoformComparison.et) return et_string;
                else if (this.relation_type == ProteoformComparison.ee) return ee_string;
                else if (this.relation_type == ProteoformComparison.ed) return ed_string;
                else if (this.relation_type == ProteoformComparison.ef) return ef_string;
                else return "";

            }
        }

        // For DataGridView display of proteoform1
        public double agg_intensity_1
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[0]).agg_intensity; } catch { return Double.NaN; } }
        }
        public double agg_RT_1
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[0]).agg_rt; } catch { return Double.NaN; } }
        }
        public int num_observations_1
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[0]).observation_count; } catch { return -1000000; } }
        }
        public double proteoform_mass_1
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[0]).agg_mass; } catch { return Double.NaN; } }
        }

        // For DataGridView display of proteform2
        public double proteoform_mass_2
        {
            get
            {
                if (connected_proteoforms[1] is ExperimentalProteoform)
                    return ((ExperimentalProteoform)connected_proteoforms[1]).agg_mass;
                else
                    return ((TheoreticalProteoform)connected_proteoforms[1]).modified_mass;
            }
        }

        public double agg_intensity_2
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[1]).agg_intensity; } catch { return 0; } }
        }
        public double agg_RT_2
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[1]).agg_rt; } catch { return 0; } }
        }
        public int num_observations_2
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[1]).observation_count; } catch { return 0; } }
        }
        public string accession_2
        {
            get { try { return (connected_proteoforms[1]).accession; } catch { return null; } }
        }
        public string accession_1
        {
            get { try { return ((ExperimentalProteoform)connected_proteoforms[0]).accession; } catch { return null; } }
        }
        public string name
        {
            get { try { return ((TheoreticalProteoform)connected_proteoforms[1]).name; } catch { return null; } }
        }
        public string fragment
        {
            get { try { return ((TheoreticalProteoform)connected_proteoforms[1]).fragment; } catch { return null; } }
        }
        public string ptm_list
        {
            get { try { return ((TheoreticalProteoform)connected_proteoforms[1]).ptm_descriptions; } catch { return null; } }
        }
        public int psm_count_BU
        {
            get { try { return ((TheoreticalProteoform)connected_proteoforms[1]).psm_count_BU; } catch { return 0; }}
        }
        public string of_interest
        {
            get { try { return ((TheoreticalProteoform)connected_proteoforms[1]).of_interest; } catch { return null; } }
        }
    }
}
