using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class TopDownProteoform : Proteoform
    {
        public string uniprot_id { get; set; }
        public string name { get; set; }
        public string sequence { get; set; }
        public int start_index { get; set; }
        public int stop_index { get; set; }
        public List<Ptm> ptm_list { get; set; } = new List<Ptm>();
        public double monoisotopic_mass { get; set; } //calibrated mass
        public double theoretical_mass { get; set; }
        public double agg_rt { get; set; }
        public string ptm_descriptions
        {
            get { return ptm_list_string(); }
        }
        private TopDownHit root;
        public List<TopDownHit> topdown_hits;
        public bool observed_theoretical_mass { get; set; } = false; //if tight abs mass or biomarker search, observed mass is close to theoretical. If only find unexpected mods search result, could be co-isolation
        public int etd_match_count { get { return relationships.Where(r => r.relation_type == ProteoformComparison.etd).ToList().Count; } }
        public int ttd_match_count { get { return relationships.Where(r => r.relation_type == ProteoformComparison.ttd).ToList().Count; } }
        public bool targeted { get; set; } = false;
        public int observations { get { return topdown_hits.Count; } }
        public int bottom_up_PSMs
        {
            get
            {
                try
                {
                    return ((TheoreticalProteoform)relationships.Where(r => r.relation_type == ProteoformComparison.ttd).First().connected_proteoforms[1]).psm_count_BU;
                }
                catch { return 0; }
            }
        }

        public TopDownProteoform(string accession, TopDownHit root, List<TopDownHit> candidate_hits) : base(accession)
        {
            this.root = root;
            this.name = root.name;
            this.ptm_list = root.ptm_list;
            this.uniprot_id = root.uniprot_id;
            this.sequence = root.sequence;
            this.start_index = root.start_index;
            this.theoretical_mass = root.theoretical_mass;
            this.stop_index = root.stop_index;
            this.topdown_hits = new List<TopDownHit>() { root };
            topdown_hits.AddRange(candidate_hits.Where(h => this.tolerable_rt(h, root.retention_time)));//&& this.tolerable_mass(h, root.corrected_mass)));
            this.calculate_properties();
            this.targeted = root.targeted;
        }


        private void calculate_properties()
        {
            if (this.topdown_hits.Where(h => h.result_set == Result_Set.tight_absolute_mass || h.result_set == Result_Set.biomarker).Count() > 0) this.observed_theoretical_mass = true;
            this.agg_rt = topdown_hits.Select(h => h.retention_time).Average(); //need to use average (no intensity info)
            if (observed_theoretical_mass) this.monoisotopic_mass = topdown_hits.Where(h => h.result_set == Result_Set.tight_absolute_mass || h.result_set == Result_Set.biomarker).Select(h => (h.corrected_mass - Math.Round(h.corrected_mass - root.corrected_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS)).Average();
            else this.monoisotopic_mass = root.theoretical_mass; //if only found w/ unexpected mod search, use theoretical mass
            this.modified_mass = this.monoisotopic_mass;
            this.accession = accession + "_" + Math.Round(this.theoretical_mass, 2);
        }

        public string ptm_list_string()
        {
            if (ptm_list.Count == 0)
                return "unmodified";
            string _modifications_string = "";
            foreach (Ptm ptm in ((TopDownProteoform)this).ptm_list) _modifications_string += (ptm.modification.id + "@" + ptm.position + "; ");
            return _modifications_string;
        }


        private bool tolerable_rt(TopDownHit candidate, double rt)
        {
            return candidate.retention_time >= rt - Convert.ToDouble(Lollipop.retention_time_tolerance) &&
                candidate.retention_time <= rt + Convert.ToDouble(Lollipop.retention_time_tolerance);
        }

        private bool tolerable_mass(TopDownHit candidate, double corrected_mass)
        {
            double mass_tolerance = corrected_mass / 1000000 * (double)Lollipop.mass_tolerance;
            double low = corrected_mass - mass_tolerance;
            double high = corrected_mass + mass_tolerance;
            bool tolerable_mass = candidate.corrected_mass >= low && candidate.corrected_mass <= high;
            if (tolerable_mass) return true; //Return a true result immediately; acts as an OR between these conditions
            return false;
        }
    }
}
