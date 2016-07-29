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
        private List<ProteoformRelation> _peak_group;
        public List<ProteoformRelation> peak_group
        {
            get { return _peak_group; }
            set
            {
                this.peak_group_count = value.Count;
                this.peak_group_deltaM = value.Select(r => r.delta_mass).Average();
                _peak_group = value;
            }
        }
        public int peak_group_count { get; set; }
        public double peak_group_deltaM { get; set; }
        public double peak_group_fdr { get; set; }
        public bool peak_accepted { get; set; }
        public List<Modification> possiblePeakAssignments { get; set; }
        public string possiblePeakAssignments_string
        {
            get { return String.Join("; ", possiblePeakAssignments.Select(m => m.description).ToArray()); }
        }
        public ProteoformRelation base_relation { get; set; }

        public DeltaMassPeak(ProteoformRelation base_relation) : base(base_relation)
        {
            this.base_relation = base_relation;
            this.peak_accepted = set_peak_accepted();
            foreach(ProteoformRelation relation in base_relation.nearby_relations)
            {
                relation.nearby_relations = base_relation.nearby_relations;
                relation.peak = this;
                relation.accepted = this.peak_accepted;
            }
            this.possiblePeakAssignments = nearestPTMs(this.peak_group_deltaM);
        }

        private bool set_peak_accepted()
        {
            return this.peak_group_count >= Lollipop.min_peak_count;
        }

        /*(this needs to be done at the actual time of forming peaks or else the average is wrong so the peak can be formed out
            of incorrect relations (average shouldn't include relations already grouped into peaks)*/
        public List<ProteoformRelation> find_nearby_relations(List<ProteoformRelation> ungrouped_relations)
        {
            List<ProteoformRelation> peak_group = new List<ProteoformRelation>();
            double nearby_deltaM = this.delta_mass;
            for (int i = 0; i < Lollipop.relation_group_centering_iterations; i++)
            {
                double lower_limit_of_peak_width = nearby_deltaM - Lollipop.peak_width_base / 2;
                double upper_limit_of_peak_width = nearby_deltaM + Lollipop.peak_width_base / 2;
                peak_group = ungrouped_relations.Where(relation =>
                    relation.delta_mass >= lower_limit_of_peak_width && relation.delta_mass <= upper_limit_of_peak_width).ToList();
                nearby_deltaM = peak_group.Select(r => r.delta_mass).Average();
            }
            this.peak_group_count = peak_group.Count;
            this.peak_group = peak_group;

            foreach (ProteoformRelation mass_difference in nearby_relations)
            {
                mass_difference.connected_proteoforms[0].relationships.Add(mass_difference);
                mass_difference.connected_proteoforms[1].relationships.Add(mass_difference);
            }

            return this.nearby_relations;
        }

        private List<Modification> nearestPTMs(double dMass)
        {
            List<Modification> possiblePTMs = new List<Modification>();
            foreach (KeyValuePair<string, Modification> knownMod in Lollipop.uniprotModificationTable)
            {
                decimal modMass = Convert.ToDecimal(knownMod.Value.monoisotopic_mass_shift);
                if (Math.Abs(Convert.ToDecimal(dMass) - modMass) <= Convert.ToDecimal(Lollipop.peak_width_base) / 2)
                    possiblePTMs.Add(knownMod.Value);
            }
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
                median_false_peak_count = (double)nearby_decoy_counts[middle] + (double)nearby_decoy_counts[middle + 1];
            }
            else
            {
                median_false_peak_count = (double)nearby_decoy_counts[(nearby_decoy_counts.Count - 1) / 2];
            }
            this.decoy_count = median_false_peak_count;
            this.peak_group_fdr = median_false_peak_count / (double)this.peak_group_count;
        }

        public List<ProteoformRelation> find_nearby_decoys(List<ProteoformRelation> all_relations)
        {
            double lower_limit_of_peak_width = this.peak_group_deltaM - Lollipop.peak_width_base / 2;
            double upper_limit_of_peak_width = this.peak_group_deltaM + Lollipop.peak_width_base / 2;
            return all_relations.Where(relation => relation.group_adjusted_deltaM >= lower_limit_of_peak_width && relation.group_adjusted_deltaM <= upper_limit_of_peak_width).ToList();
        }

        new public string as_tsv_row()
        {
            return String.Join("\t", new List<string> { this.connected_proteoforms[0].accession.ToString(), this.connected_proteoforms[1].accession.ToString(), this.delta_mass.ToString(), this.peak_group_deltaM.ToString(),
                this.peak_group_count.ToString(), peak_group_fdr.ToString() });
        }

        new public static string get_tsv_header()
        {
            return String.Join("\t", new List<string> { "proteoform1_accession", "proteoform2_accession", "delta_mass", "group_adjusted_deltaM", "group_count", "group_fdr" });
        }
    }
}
