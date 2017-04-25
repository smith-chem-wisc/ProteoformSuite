using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    //Types of comparisons, aka ProteoformFamily edges
    public enum ProteoformComparison
    {
        ExperimentalTheoretical, //Experiment-Theoretical comparisons
        ExperimentalDecoy, //Experiment-Decoy comparisons
        ExperimentalExperimental, //Experiment-Experiment comparisons
        ExperimentalFalse  //Experiment-Experiment comparisons using unequal lysine counts
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
    [Serializable]
    public class ProteoformRelation : IMassDifference
    {

        #region Fields

        private static int instanceCounter = 0;
        public Proteoform[] connected_proteoforms = new Proteoform[2];
        public PtmSet candidate_ptmset = null;
        public PtmSet represented_ptmset = null;

        [NonSerialized]
        private DeltaMassPeak _peak;

        [NonSerialized]
        private List<ProteoformRelation> _nearby_relations;

        #endregion Fields

        #region Public Properties

        public int InstanceId { get; set; }
        public double DeltaMass { get; set; }
        public DeltaMassPeak peak { get { return _peak; } set { _peak = value; } }
        public List<ProteoformRelation> nearby_relations { get { return _nearby_relations; } set { _nearby_relations = value; } }
        public int nearby_relations_count { get; set; } // count is the "running sum"; relations are not saved
        public bool outside_no_mans_land { get; set; }
        public int lysine_count { get; set; }
        public ProteoformComparison RelationType { get; set; }

        /// <summary>
        /// Is this relation in an accepted peak?
        /// ProteoformRelation.accepted may not be the same as DeltaMassPeak.peak_accepted, which denotes whether the peak is accepted.
        /// </summary>
        public bool Accepted { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public ProteoformRelation(Proteoform pf1, Proteoform pf2, ProteoformComparison relation_type, double delta_mass)
        {
            connected_proteoforms[0] = pf1;
            connected_proteoforms[1] = pf2;
            RelationType = relation_type;
            DeltaMass = delta_mass;
            InstanceId = instanceCounter;
            lock (SaveState.lollipop) instanceCounter += 1; //Not thread safe

            if (SaveState.lollipop.neucode_labeled)
            {
                lysine_count = pf1.lysine_count;
            }

            if ((relation_type == ProteoformComparison.ExperimentalTheoretical || relation_type == ProteoformComparison.ExperimentalDecoy) 
                && SaveState.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(delta_mass, 1), out List<PtmSet> candidate_sets))
            {
                TheoreticalProteoform t = pf2 as TheoreticalProteoform;
                double mass_tolerance = t.modified_mass / 1000000 * (double)SaveState.lollipop.mass_tolerance;
                List<PtmSet> narrower_range_of_candidates = candidate_sets.Where(s => Math.Abs(s.mass - delta_mass) < 0.05).ToList();
                candidate_ptmset = t.generate_possible_added_ptmsets(narrower_range_of_candidates, delta_mass, mass_tolerance, SaveState.lollipop.theoretical_database.all_mods_with_mass, t, t.sequence, SaveState.lollipop.mod_rank_first_quartile)
                    .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(Math.Abs(x.mass) - Math.Abs(delta_mass)) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                    .FirstOrDefault();
            }

            outside_no_mans_land =
                Math.Abs(delta_mass - Math.Truncate(delta_mass)) >= SaveState.lollipop.no_mans_land_upperBound ||
                Math.Abs(delta_mass - Math.Truncate(delta_mass)) <= SaveState.lollipop.no_mans_land_lowerBound;
        }

        #endregion Public Constructors

        #region Public Methods

        public List<ProteoformRelation> set_nearby_group(List<ProteoformRelation> all_ordered_relations, List<int> ordered_relation_ids)
        {
            double peak_width_base = typeof(TheoreticalProteoform).IsAssignableFrom(all_ordered_relations[0].connected_proteoforms[1].GetType()) ?
                SaveState.lollipop.peak_width_base_et :
                SaveState.lollipop.peak_width_base_ee;
            double lower_limit_of_peak_width = DeltaMass - peak_width_base / 2;
            double upper_limit_of_peak_width = DeltaMass + peak_width_base / 2;
            int idx = ordered_relation_ids.IndexOf(InstanceId);
            List<ProteoformRelation> within_range = new List<ProteoformRelation> { this };
            int curr_idx = idx - 1;
            while (curr_idx >= 0 && lower_limit_of_peak_width <= all_ordered_relations[curr_idx].DeltaMass)
            {
                within_range.Add(all_ordered_relations[curr_idx]);
                curr_idx--;
            }
            curr_idx = idx + 1;
            while (curr_idx < all_ordered_relations.Count && all_ordered_relations[curr_idx].DeltaMass <= upper_limit_of_peak_width)
            {
                within_range.Add(all_ordered_relations[curr_idx]);
                curr_idx++;
            }
            lock (this)
            {
                nearby_relations = within_range;
                nearby_relations_count = within_range.Count;
            }
            return nearby_relations;
        }

        public void generate_peak()
        {
            new DeltaMassPeak(this, SaveState.lollipop.proteoform_community.remaining_relations_outside_no_mans); //setting the peak takes place elsewhere, but this constructs it
            if (connected_proteoforms[1] as TheoreticalProteoform != null && SaveState.lollipop.ed_relations.Count > 0)
                peak.calculate_fdr(SaveState.lollipop.ed_relations);
            else if (connected_proteoforms[1] as ExperimentalProteoform != null && SaveState.lollipop.ef_relations.Count > 0)
                peak.calculate_fdr(SaveState.lollipop.ef_relations);
        }

        public override bool Equals(object obj)
        {
            ProteoformRelation r2 = obj as ProteoformRelation;
            return r2 != null &&
                (InstanceId == r2.InstanceId ||
                connected_proteoforms[0] == r2.connected_proteoforms[1] && connected_proteoforms[1] == r2.connected_proteoforms[0] ||
                connected_proteoforms[0] == r2.connected_proteoforms[0] && connected_proteoforms[1] == r2.connected_proteoforms[1]);
        }

        public override int GetHashCode()
        {
            return connected_proteoforms[0].GetHashCode() ^ connected_proteoforms[1].GetHashCode();
        }

        #endregion Public Methods

    }
}
