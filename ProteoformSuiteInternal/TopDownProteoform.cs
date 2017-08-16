using System;
using System.Collections.Generic;
using System.Linq;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class TopDownProteoform : ExperimentalProteoform
    {
        public string uniprot_id { get; set; }
        public string pfr { get; set; }
        public string name { get; set; }
        public string sequence { get; set; }
        public int begin { get; set; } //position one based
        public int end { get; set; } //position one based
        public double theoretical_mass { get; set; }
        public TopDownHit root;
        public List<TopDownHit> topdown_hits;


        public TopDownProteoform(string accession, TopDownHit root, List<TopDownHit> hits) : base(accession, null, true)
        {
            this.root = root;
            this.name = root.name;
            this.pfr = root.pfr;
            this.ptm_set = new PtmSet(root.ptm_list);
            this.uniprot_id = root.uniprot_id;
            this.sequence = root.sequence;
            this.begin = root.begin;
            this.theoretical_mass = root.theoretical_mass;
            this.end = root.end;
            this.topdown_hits = hits;
            this.calculate_td_properties();
            this.accession = accession + "_1_" + Math.Round(this.modified_mass, 2) + "_Da_" + begin + "to" + end;
            this.lysine_count = sequence.Count(s => s == 'K');
            this.topdown_id = true;
        }

        public void calculate_td_properties()
        {
            this.agg_mass = topdown_hits.Select(h => (h.reported_mass - Math.Round(h.reported_mass - h.theoretical_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS)).Average();
            this.modified_mass = this.agg_mass;
            this.agg_rt = topdown_hits.Select(h => h.ms2_retention_time).Average();
        }
    }
}
