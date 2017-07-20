using System;
using System.Collections.Generic;
using System.Linq;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class TopDownProteoform : Proteoform
    {
        public string uniprot_id { get; set; }
        public string pfr { get; set; }
        public string name { get; set; }
        public string sequence { get; set; }
        public int start_index { get; set; } //position one based
        public int stop_index { get; set; } //position one based
        public double monoisotopic_mass { get; set; } //calibrated mass
        public double theoretical_mass { get; set; }
        public double agg_RT { get; set; }
        public TopDownHit root;
        public List<TopDownHit> topdown_hits;


        public TopDownProteoform(string accession, TopDownHit root, List<TopDownHit> hits) : base(accession)
        {
            this.root = root;
            this.name = root.name;
            this.pfr = root.pfr;
            this.ptm_set = new PtmSet(root.ptm_list);
            this.uniprot_id = root.uniprot_id;
            this.sequence = root.sequence;
            this.start_index = root.start_index;
            this.theoretical_mass = root.theoretical_mass;
            this.stop_index = root.stop_index;
            this.topdown_hits = hits;
            this.calculate_properties();
            this.accession = accession + "_TD1_" + Math.Round(this.modified_mass, 2) + "_Da_" + start_index + "to" + stop_index;
            this.lysine_count = sequence.Count(s => s == 'K');
        }

        public TopDownProteoform(TopDownProteoform t) : base(t.accession)
        {
            this.root = t.root;
            this.name = t.name;
            this.ptm_set = new PtmSet(t.ptm_set.ptm_combination);
            this.uniprot_id = t.uniprot_id;
            this.sequence = t.sequence;
            this.start_index = t.start_index;
            this.theoretical_mass = t.theoretical_mass;
            this.stop_index = t.stop_index;
            this.topdown_hits = t.topdown_hits.Select(h => new TopDownHit(h)).ToList();
            this.monoisotopic_mass = t.monoisotopic_mass;
            this.modified_mass = t.monoisotopic_mass;
            this.accession = t.accession;
            this.agg_RT = t.agg_RT;
            this.lysine_count = t.lysine_count;
        }


        public void calculate_properties()
        {
            this.monoisotopic_mass = topdown_hits.Select(h => (h.reported_mass - Math.Round(h.reported_mass - h.theoretical_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS)).Average();
            this.modified_mass = this.monoisotopic_mass;
            this.agg_RT = topdown_hits.Select(h => h.retention_time).Average();
        }
         
        private bool tolerable_rt(TopDownHit candidate, double rt)
        {
            return candidate.retention_time >= rt - Convert.ToDouble(Sweet.lollipop.retention_time_tolerance) &&
                candidate.retention_time <= rt + Convert.ToDouble(Sweet.lollipop.retention_time_tolerance);
        }

        public bool same_ptms(Proteoform theo)
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
