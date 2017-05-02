using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class TopDownProteoform : Proteoform
    {
        public string uniprot_id { get; set; }
        public string name { get; set; }
        public string sequence { get; set; }
        public int start_index { get; set; } //position one based
        public int stop_index { get; set; } //position one based
        public double monoisotopic_mass { get; set; } //calibrated mass
        public double theoretical_mass { get; set; }
        public double agg_rt { get; set; }
        public List<double> all_RTs { get; set; }
        public TopDownHit root;
        public List<TopDownHit> topdown_hits;
        public int etd_match_count { get { return relationships.Where(r => r.RelationType == ProteoformComparison.ExperimentalTopDown).ToList().Count; } }
        public int ttd_match_count { get { return relationships.Where(r => r.RelationType == ProteoformComparison.TheoreticalTopDown).ToList().Count; } }
        public bool targeted { get; set; } = false;
        public int observations { get { return topdown_hits.Count; } }
        public int bottom_up_PSMs
        {
            get
            {
                try
                {
                    return relationships.Sum(r => r.connected_proteoforms.OfType<TheoreticalProteoform>().Sum(t => t.psm_list.Count));
                }
                catch { return 0; }
            }
        }

        public TopDownProteoform(string accession, TopDownHit root, List<TopDownHit> hits) : base(accession)
        {
            this.linked_proteoform_references = new LinkedList<Proteoform>().ToList();
            this.root = root;
            this.name = root.name;
            this.ptm_set = new PtmSet(root.ptm_list);
            this.uniprot_id = root.uniprot_id;
            this.sequence = root.sequence;
            this.start_index = root.start_index;
            this.theoretical_mass = root.theoretical_mass;
            this.stop_index = root.stop_index;
            this.topdown_hits = hits;
            this.calculate_properties();
            this.targeted = root.targeted;
            this.lysine_count = sequence.Count(s => s == 'K');
        }


        private void calculate_properties()
        {
            this.monoisotopic_mass = topdown_hits.Select(h => (h.corrected_mass - Math.Round(h.corrected_mass - h.theoretical_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS)).Average();
            this.modified_mass = this.monoisotopic_mass;
            this.accession = accession+ "_TD1_" + Math.Round(this.theoretical_mass, 2) + "_Da_"  + start_index + "to" + stop_index ;
            this.all_RTs = get_retention_times();
        }

        private List<double> get_retention_times()
        {
            List<double> all_RT = topdown_hits.OrderByDescending(h => h.score).Select(h => h.retention_time).ToList();
            double rt = all_RT.First();
            Dictionary<double, List<double>> first_average = new Dictionary<double, List<double>>();
            Dictionary<double, List<double>> second_average = new Dictionary<double, List<double>>();
            //first average
            while (all_RT.Count > 0)
            {
                rt = all_RT.First();
                List<double> in_tol = all_RT.Where(r => Math.Abs(rt - r) <= Convert.ToDouble(SaveState.lollipop.retention_time_tolerance)).ToList();
                first_average.Add(in_tol.Average(), in_tol);
                all_RT = all_RT.Except(first_average.Values.SelectMany(v => v)).ToList();
            }
            //second average
            while(first_average.Count > 0)
            {
                rt = first_average.Keys.First();
                List<double> in_tol = first_average.Keys.Where(r => Math.Abs(rt - r) <= Convert.ToDouble(SaveState.lollipop.retention_time_tolerance)).ToList();
                second_average.Add(in_tol.Average(), in_tol);
                foreach (var rm in in_tol) first_average.Remove(rm);
            }
            this.agg_rt = second_average.OrderByDescending(sa => sa.Value.Count).First().Key;
            return second_average.Keys.ToList();
        }

         
        private bool tolerable_rt(TopDownHit candidate, double rt)
        {
            return candidate.retention_time >= rt - Convert.ToDouble(SaveState.lollipop.retention_time_tolerance) &&
                candidate.retention_time <= rt + Convert.ToDouble(SaveState.lollipop.retention_time_tolerance);
        }

        public bool same_ptms(TheoreticalProteoform theo)
        {                   
            //equal numbers of each type of modification
            if (this.ptm_set.ptm_combination.Count == theo.ptm_set.ptm_combination.Count)
            {
                foreach (ModificationWithMass mod in this.ptm_set.ptm_combination.Select(p => p.modification).Distinct())
                {
                    if (theo.ptm_set.ptm_combination.Where(p => p.modification == mod).Count() == this.ptm_set.ptm_combination.Where(p => p.modification == mod).Count()) continue;
                    else return false;
                }
                return true;
            }
            else return false;
        }
    }
}
