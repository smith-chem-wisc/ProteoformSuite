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
        public string gene_list { get { return String.Join("; ", gene_names.Select(p => p.get_prefered_name(ProteoformCommunity.preferred_gene_label)).Where(n => n != null).Distinct()); } }
        public string experimentals_list { get { return String.Join("; ", experimental_proteoforms.Select(p => p.accession)); } }
        public string agg_mass_list { get { return String.Join("; ", experimental_proteoforms.Select(p => Math.Round(p.agg_mass, Lollipop.deltaM_edge_display_rounding))); } }
        public int lysine_count { get; set; } = -1;
        public List<ExperimentalProteoform> experimental_proteoforms { get; set; }
        public int experimental_count { get { return this.experimental_proteoforms.Count; } }
        public List<TheoreticalProteoform> theoretical_proteoforms { get; set; }
        public int theoretical_count { get { return this.theoretical_proteoforms.Count; } }
        public List<GeneName> gene_names { get; set; }
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
            separate_proteoforms();
        }

        private void separate_proteoforms()
        {
            this.theoretical_proteoforms = proteoforms.OfType<TheoreticalProteoform>().ToList();
            this.gene_names = theoretical_proteoforms.Select(t => t.gene_name).ToList();
            HashSet<int> lysine_counts = new HashSet<int>(proteoforms.Select(p => p.lysine_count));
            if (lysine_counts.Count == 1) this.lysine_count = lysine_counts.FirstOrDefault();
            this.experimental_proteoforms = proteoforms.OfType<ExperimentalProteoform>().ToList();
            this.relations = new HashSet<ProteoformRelation>(proteoforms.SelectMany(p => p.relationships.Where(r => r.peak.peak_accepted)));
        }

        public List<Proteoform> construct_family(List<Proteoform> seed)
        {
            List<Proteoform> seed_expansion = seed.SelectMany(p => p.get_connected_proteoforms()).Except(seed).ToList();
            if (seed_expansion.Count == 0) return seed;
            seed.AddRange(seed_expansion);
            return construct_family(seed);
        }

        public void merge_families()
        {
            IEnumerable<ProteoformFamily> gene_family =
                    from f in Lollipop.proteoform_community.families
                    from n in this.gene_names.Select(g => g.get_prefered_name(ProteoformCommunity.preferred_gene_label)).Distinct()
                    where f.gene_names.Select(g => g.get_prefered_name(ProteoformCommunity.preferred_gene_label)).Contains(n)
                    select f;
            proteoforms = new HashSet<Proteoform>(proteoforms.Concat(gene_family.SelectMany(f => f.proteoforms)));
            separate_proteoforms();
        }
    }
}
