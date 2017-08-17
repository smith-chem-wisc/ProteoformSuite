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
        private PtmSet _topdown_ptmset = new PtmSet(new List<Ptm>());
        public PtmSet topdown_ptmset //the ptmset read in with td data
        {
            get
            {
                return _topdown_ptmset;
            }

            set
            {
                _topdown_ptmset = value;
                topdown_ptm_description = topdown_ptmset == null || topdown_ptmset.ptm_combination == null ?
                    "Unknown" :
                    topdown_ptmset.ptm_combination.Count == 0 ?
                        "Unmodified" :
                        String.Join("; ", topdown_ptmset.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).ToList());

                //    String.Join("; ", topdown_ptmset.ptm_combination.Select(ptm => ptm.position > 0 ? ptm.modification.id + "@" + ptm.position : Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).ToList());
            }
        }
        public string topdown_ptm_description { get; set; }



        public TopDownProteoform(string accession, TopDownHit root, List<TopDownHit> hits) : base(accession, null, true)
        {
            this.root = root;
            this.name = root.name;
            this.pfr = root.pfr;
            this.topdown_ptmset = new PtmSet(root.ptm_list);
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
            //correct here for missed monoisotopic mass...
            this.agg_mass = topdown_hits.Select(h => (h.reported_mass - Math.Round(h.reported_mass - h.theoretical_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS)).Average();
            this.modified_mass = this.agg_mass;
            this.agg_rt = topdown_hits.Select(h => h.ms2_retention_time).Average();
        }
    }
}
