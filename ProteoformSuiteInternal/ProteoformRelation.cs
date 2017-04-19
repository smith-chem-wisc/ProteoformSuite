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
    public class ProteoformRelation
    {
        private static int instanceCounter = 0;
        public int instanceId;
        public Proteoform[] connected_proteoforms = new Proteoform[2];
        public ProteoformComparison relation_type;
        public double delta_mass { get; set; }

        public DeltaMassPeak peak { get; set; }
        public List<ProteoformRelation> nearby_relations { get; set; } // count is the "running sum"
        public bool outside_no_mans_land { get; set; }
        public int lysine_count { get; set; }
        public PtmSet candidate_ptmset { get; set; }
        public PtmSet represented_ptmset { get; set; }

        /// <summary>
        /// Is this relation in an accepted peak?
        /// ProteoformRelation.accepted may not be the same as DeltaMassPeak.peak_accepted, which denotes whether the peak is accepted.
        /// </summary>
        public bool accepted { get; set; }

        public ProteoformRelation(Proteoform pf1, Proteoform pf2, ProteoformComparison relation_type, double delta_mass) 
        {
            this.connected_proteoforms[0] = pf1;
            this.connected_proteoforms[1] = pf2;
            this.relation_type = relation_type;
            this.delta_mass = delta_mass;
            instanceId = instanceCounter;
            lock (Lollipop.proteoform_community) instanceCounter += 1; //Not thread safe

            if (Lollipop.neucode_labeled)
            {
                this.lysine_count = pf1.lysine_count;
            }

            if ((relation_type == ProteoformComparison.ExperimentalTheoretical || relation_type == ProteoformComparison.ExperimentalDecoy) && Lollipop.possible_ptmset_dictionary.ContainsKey(Math.Round(delta_mass, 1)))
            {
                TheoreticalProteoform t = pf2 as TheoreticalProteoform;
                double mass_tolerance = t.modified_mass / 1000000 * (double)Lollipop.mass_tolerance;
                candidate_ptmset = t.generate_possible_added_ptmsets(Lollipop.possible_ptmset_dictionary[Math.Round(delta_mass, 1)].Where(s => Math.Abs(s.mass - delta_mass) < 0.1).ToList(), delta_mass, mass_tolerance, Lollipop.all_mods_with_mass, t, t.sequence, Lollipop.rank_first_quartile)
                    .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(Math.Abs(x.mass) - Math.Abs(delta_mass)) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                    .FirstOrDefault();
            }

            outside_no_mans_land = 
                Math.Abs(delta_mass - Math.Truncate(delta_mass)) >= Lollipop.no_mans_land_upperBound || 
                Math.Abs(delta_mass - Math.Truncate(delta_mass)) <= Lollipop.no_mans_land_lowerBound;
        }

        public ProteoformRelation(ProteoformRelation relation) 
        {
            connected_proteoforms = relation.connected_proteoforms.ToArray();
            relation_type = relation.relation_type;
            delta_mass = relation.delta_mass;
            instanceId = instanceCounter;
            lock (Lollipop.proteoform_community) instanceCounter += 1; //Not thread safe

            peak = relation.peak;
            outside_no_mans_land = relation.outside_no_mans_land;
            nearby_relations = relation.nearby_relations;
        }

        public List<ProteoformRelation> set_nearby_group(List<ProteoformRelation> all_ordered_relations, List<int> ordered_relation_ids)
        {
            double peak_width_base = typeof(TheoreticalProteoform).IsAssignableFrom(all_ordered_relations[0].connected_proteoforms[1].GetType()) ? 
                Lollipop.peak_width_base_et :
                Lollipop.peak_width_base_ee;
            double lower_limit_of_peak_width = this.delta_mass - peak_width_base / 2;
            double upper_limit_of_peak_width = this.delta_mass + peak_width_base / 2;
            int idx = ordered_relation_ids.IndexOf(this.instanceId);
            List<ProteoformRelation> within_range = new List<ProteoformRelation> { this };
            int curr_idx = idx - 1;
            while (curr_idx >= 0 && lower_limit_of_peak_width <= all_ordered_relations[curr_idx].delta_mass)
            {
                within_range.Add(all_ordered_relations[curr_idx]);
                curr_idx--;
            }
            curr_idx = idx + 1;
            while (curr_idx < all_ordered_relations.Count && all_ordered_relations[curr_idx].delta_mass <= upper_limit_of_peak_width)
            {
                within_range.Add(all_ordered_relations[curr_idx]);
                curr_idx++;
            }
            lock (this) nearby_relations = within_range;
            return this.nearby_relations;
        }

        public void generate_peak()
        {
            new DeltaMassPeak(this, Lollipop.proteoform_community.remaining_relations_outside_no_mans); //setting the peak takes place elsewhere, but this constructs it
            if (connected_proteoforms[1] is TheoreticalProteoform && Lollipop.ed_relations.Count > 0) this.peak.calculate_fdr(Lollipop.ed_relations);
            else if (connected_proteoforms[1] is ExperimentalProteoform && Lollipop.ef_relations.Count > 0) this.peak.calculate_fdr(Lollipop.ef_relations);
        }

        public override bool Equals(object obj)
        {
            ProteoformRelation r2 = obj as ProteoformRelation; 
            return r2 != null && 
                (this.instanceId == r2.instanceId ||
                this.connected_proteoforms[0] == r2.connected_proteoforms[1] && this.connected_proteoforms[1] == r2.connected_proteoforms[0] ||
                this.connected_proteoforms[0] == r2.connected_proteoforms[0] && this.connected_proteoforms[1] == r2.connected_proteoforms[1]);
        }

        public IEnumerable<PtmSet> nearestPTMs(double dMass, ProteoformComparison relation_type)
        {
            foreach (PtmSet set in Lollipop.all_possible_ptmsets)
            {
                bool valid_or_no_unmodified = set.ptm_combination.Count == 1 || !set.ptm_combination.Select(ptm => ptm.modification).Any(m => m.monoisotopicMass == 0);
                bool within_addition_tolerance = relation_type == ProteoformComparison.ExperimentalTheoretical || relation_type == ProteoformComparison.ExperimentalDecoy ?
                    Math.Abs(dMass - set.mass) <= 0.1 :
                    Math.Abs(Math.Abs(dMass) - Math.Abs(set.mass)) <= 0.1; //In Daltons. This is a liberal threshold because these are filtered upon actual assignment
                if (valid_or_no_unmodified && within_addition_tolerance)
                    yield return set;
            }
        }

        public override int GetHashCode()
        {
            return connected_proteoforms[0].GetHashCode() ^ connected_proteoforms[1].GetHashCode();
        }
    }
}
