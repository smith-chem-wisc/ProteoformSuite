using Chemistry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UsefulProteomicsDatabases;

namespace ProteoformSuiteInternal
{
    //Types of comparisons, aka ProteoformFamily edges
    public enum ProteoformComparison
    {
        ExperimentalTheoretical, //Experiment-Theoretical comparisons
        ExperimentalDecoy, //Experiment-Decoy comparisons
        ExperimentalExperimental, //Experiment-Experiment comparisons
        ExperimentalFalse,  //Experiment-Experiment comparisons using unequal lysine counts
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
    public class ProteoformRelation : IMassDifference
    {
        #region Fields

        private static int instanceCounter = 0;
        public Proteoform[] connected_proteoforms = new Proteoform[2];
        public PtmSet candidate_ptmset = null;
        public PtmSet represented_ptmset = null;
        private static ChemicalFormula CH2 = null;
        private static ChemicalFormula HPO3 = null;

        #endregion Fields

        #region Public Properties

        public int InstanceId { get; set; }
        public double DeltaMass { get; set; }
        public DeltaMassPeak peak { get; set; }
        public List<ProteoformRelation> nearby_relations { get; set; } // cleared after accepting peaks
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

        public ProteoformRelation(Proteoform pf1, Proteoform pf2, ProteoformComparison relation_type, double delta_mass, string current_directory)
        {
            connected_proteoforms[0] = pf1;
            connected_proteoforms[1] = pf2;
            RelationType = relation_type;
            DeltaMass = delta_mass;
            InstanceId = instanceCounter;
            lock (Sweet.lollipop) instanceCounter += 1; //Not thread safe

            if (CH2 == null || HPO3 == null)
            {
                Loaders.LoadElements(Path.Combine(current_directory, "elements.dat"));
                CH2 = ChemicalFormula.ParseFormula("C1 H2");
                HPO3 = ChemicalFormula.ParseFormula("H1 O3 P1");
            }

            if (Sweet.lollipop.neucode_labeled)
            {
                lysine_count = pf1.lysine_count;
            }

            if ((relation_type == ProteoformComparison.ExperimentalTheoretical || relation_type == ProteoformComparison.ExperimentalDecoy)
                && Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(delta_mass, 1), out List<PtmSet> candidate_sets)
                && pf2 as TheoreticalProteoform != null)
            {
                TheoreticalProteoform t = pf2 as TheoreticalProteoform;
                double mass_tolerance = t.modified_mass / 1000000 * Sweet.lollipop.mass_tolerance;
                List<PtmSet> narrower_range_of_candidates = candidate_sets.Where(s => Math.Abs(s.mass - delta_mass) < Sweet.lollipop.peak_width_base_et).ToList();
                candidate_ptmset = pf1.generate_possible_added_ptmsets(narrower_range_of_candidates, delta_mass, mass_tolerance, Sweet.lollipop.theoretical_database.all_mods_with_mass, t, Sweet.lollipop.mod_rank_first_quartile)
                    .OrderBy(x => x.ptm_rank_sum + Math.Abs(Math.Abs(x.mass) - Math.Abs(delta_mass)) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                    .FirstOrDefault();
            }

            // Start the model (0 Da) at the mass defect of CH2 or HPO3 itself, allowing the peak width tolerance on either side
            double half_peak_width = RelationType == ProteoformComparison.ExperimentalTheoretical || RelationType == ProteoformComparison.ExperimentalDecoy ?
                Sweet.lollipop.peak_width_base_et / 2 :
                Sweet.lollipop.peak_width_base_ee / 2;
            double low_decimal_bound = half_peak_width + ((CH2.MonoisotopicMass - Math.Truncate(CH2.MonoisotopicMass)) / CH2.MonoisotopicMass) * (Math.Abs(delta_mass) <= CH2.MonoisotopicMass ? CH2.MonoisotopicMass : Math.Abs(delta_mass));
            double high_decimal_bound = 1 - half_peak_width + ((HPO3.MonoisotopicMass - Math.Ceiling(HPO3.MonoisotopicMass)) / HPO3.MonoisotopicMass) * (Math.Abs(delta_mass) <= HPO3.MonoisotopicMass ? HPO3.MonoisotopicMass : Math.Abs(delta_mass));
            double delta_mass_decimal = Math.Abs(delta_mass - Math.Truncate(delta_mass));

            outside_no_mans_land = delta_mass_decimal <= low_decimal_bound || delta_mass_decimal >= high_decimal_bound
                || high_decimal_bound <= low_decimal_bound;
        }

        #endregion Public Constructors

        #region Public Methods

        public List<ProteoformRelation> set_nearby_group(List<ProteoformRelation> all_ordered_relations, List<int> ordered_relation_ids)
        {
            double peak_width_base = typeof(TheoreticalProteoform).IsAssignableFrom(all_ordered_relations[0].connected_proteoforms[1].GetType()) ?
                Sweet.lollipop.peak_width_base_et :
                Sweet.lollipop.peak_width_base_ee;
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
            new DeltaMassPeak(this, Sweet.lollipop.target_proteoform_community.remaining_relations_outside_no_mans); //setting the peak takes place elsewhere, but this constructs it
            if (connected_proteoforms[1] as TheoreticalProteoform != null && Sweet.lollipop.ed_relations.Count > 0)
                lock (peak) peak.calculate_fdr(Sweet.lollipop.ed_relations);
            else if (connected_proteoforms[1] as ExperimentalProteoform != null && Sweet.lollipop.ef_relations.Count > 0)
                lock (peak) peak.calculate_fdr(Sweet.lollipop.ef_relations);
        }

        public override bool Equals(object obj)
        {
            ProteoformRelation r2 = obj as ProteoformRelation;
            return r2 != null &&
                (InstanceId == r2.InstanceId) ||
                (connected_proteoforms[0] == r2.connected_proteoforms[1] && connected_proteoforms[1] == r2.connected_proteoforms[0]) ||
                (connected_proteoforms[0] == r2.connected_proteoforms[0] && connected_proteoforms[1] == r2.connected_proteoforms[1]);
        }

        public override int GetHashCode()
        {
            return connected_proteoforms[0].GetHashCode() ^ connected_proteoforms[1].GetHashCode();
        }

        #endregion Public Methods
    }
}