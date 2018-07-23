using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class GeneName
    {
        public IEnumerable<Tuple<string,string>> gene_names { get; private set; }
        public string ordered_locus { get; private set; }
        public string primary { get; private set; }

        public GeneName(IEnumerable<Tuple<string, string>> gene_names)
        {
            this.gene_names = gene_names;
            Tuple<string, string> first_ordered = gene_names.FirstOrDefault(v => v.Item1 == "ordered locus");
            Tuple<string, string> first_primary = gene_names.FirstOrDefault(v => v.Item1 == "primary");
            this.ordered_locus = first_ordered != null ? first_ordered.Item2 : null;
            this.primary = first_primary != null ? first_primary.Item2 : null;
        }

        public GeneName(IEnumerable<GeneName> gene_names)
        {
            this.ordered_locus = gene_names.Count(g => g.ordered_locus != null) > 0 ? gene_names.FirstOrDefault(g => g.ordered_locus != null).ordered_locus : null;
            this.primary = gene_names.Count(g => g.primary != null) > 0 ? gene_names.FirstOrDefault(g => g.primary != null).primary : null;
            this.gene_names = gene_names.SelectMany(g => g.gene_names);
        }

        public string get_prefered_name(string prefered_gene_label)
        {
            string name;

            if (prefered_gene_label != null && prefered_gene_label.ToLowerInvariant().StartsWith("primary"))
            {
                name = primary != null ? primary : ordered_locus;
            }
            else if (prefered_gene_label != null && prefered_gene_label.ToLowerInvariant().StartsWith("ordered locus"))
            {
                name = ordered_locus != null ? ordered_locus : primary;
            }
            else
            {
                name = null;
            }

            return name != null || gene_names.Count() == 0 ? name : gene_names.FirstOrDefault().Item2;
        }
    }
}
