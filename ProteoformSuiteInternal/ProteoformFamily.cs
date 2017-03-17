using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace ProteoformSuiteInternal
{
    public class ProteoformFamily
    {
        private static int family_counter = 0;
        public int family_id { get; set; }
        public string name_list { get { return String.Join("; ", theoretical_proteoforms.Select(p => p.name)); } }
        public string accession_list { get { return String.Join("; ", theoretical_proteoforms.Select(p => p.accession)); } }
        public string experimentals_list { get { return String.Join("; ", experimental_proteoforms.Select(p => p.accession)); } }
        public string agg_mass_list { get { return String.Join("; ", experimental_proteoforms.Select(p => Math.Round(p.agg_mass, Lollipop.deltaM_edge_display_rounding))); } }
        public int lysine_count { get; set; } = -1;
        public List<ExperimentalProteoform> experimental_proteoforms { get; set; }
        public int experimental_count { get { return this.experimental_proteoforms.Count; } }
        public List<TheoreticalProteoform> theoretical_proteoforms { get; set; }
        public int theoretical_count { get { return this.theoretical_proteoforms.Count; } }
        public List<TopDownProteoform> topdown_proteoforms { get; set; }
        public int topdown_count { get { return this.topdown_proteoforms.Count; }}
        public HashSet<ProteoformRelation> relations { get; set; }
        public int relation_count { get { return this.relations.Count; } }
        public HashSet<Proteoform> proteoforms { get; set; }
        private Proteoform seed { get; set; }
        public ProteoformFamily(Proteoform seed)
        {
            family_counter++;
            this.family_id = family_counter;
            this.seed = seed;
        }

        public void construct_family()
        {
            this.proteoforms = new HashSet<Proteoform>(construct_family(new List<Proteoform> { seed }));
            HashSet<int> lysine_counts = new HashSet<int>(proteoforms.Select(p => p.lysine_count));
            if (lysine_counts.Count == 1) this.lysine_count = lysine_counts.FirstOrDefault();
            this.experimental_proteoforms = proteoforms.OfType<ExperimentalProteoform>().ToList();
            this.theoretical_proteoforms = proteoforms.OfType<TheoreticalProteoform>().ToList();
            this.topdown_proteoforms = proteoforms.OfType<TopDownProteoform>().ToList();
            this.relations = new HashSet<ProteoformRelation>(proteoforms.SelectMany(p => p.relationships.Where(r => r.accepted))); //etd relations don't have peaks, so check if relation accepted (will be unacceptedi if peak unaccepted)
        }

        public List<Proteoform> construct_family(List<Proteoform> seed)
        {
            List<Proteoform> seed_expansion = seed.SelectMany(p => p.get_connected_proteoforms()).Except(seed).ToList();
            if (seed_expansion.Count == 0) return seed;
            seed.AddRange(seed_expansion);
            return construct_family(seed);
        }
    }
}
