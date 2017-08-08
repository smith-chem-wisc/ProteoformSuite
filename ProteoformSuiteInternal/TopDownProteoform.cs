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
        public int begin { get; set; } //position one based
        public int end { get; set; } //position one based
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
            this.begin = root.begin;
            this.theoretical_mass = root.theoretical_mass;
            this.end = root.end;
            this.topdown_hits = hits;
            this.calculate_properties();
            this.accession = accession + "_1_" + Math.Round(this.modified_mass, 2) + "_Da_" + begin + "to" + end;
            this.lysine_count = sequence.Count(s => s == 'K');
        }

        public TopDownProteoform(TopDownProteoform t) : base(t.accession)
        {
            this.root = t.root;
            this.name = t.name;
            this.ptm_set = new PtmSet(t.ptm_set.ptm_combination);
            this.uniprot_id = t.uniprot_id;
            this.sequence = t.sequence;
            this.begin = t.begin;
            this.theoretical_mass = t.theoretical_mass;
            this.end = t.end;
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
            this.agg_RT = topdown_hits.Select(h => h.ms2_retention_time).Average();
        }
    }
}
