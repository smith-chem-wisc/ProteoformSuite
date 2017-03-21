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
            GeneName first = gene_names.FirstOrDefault();
            this.ordered_locus = first != null ? first.ordered_locus : null;
            this.primary = first != null ? first.primary : null;
            this.gene_names = gene_names.SelectMany(g => g.gene_names);
        }

        public string get_prefered_name(string prefered_gene_label)
        {
            string name;
            if (prefered_gene_label.ToLower().StartsWith("primary")) name = primary != null ? primary : ordered_locus;
            else if (prefered_gene_label.ToLower().StartsWith("ordered locus")) name = ordered_locus != null ? ordered_locus : primary;
            else name = null;
            return name != null || gene_names.Count() == 0 ? name : gene_names.FirstOrDefault().Item2;
        }

        public void merge(GeneName more_names)
        {
            gene_names = gene_names.Concat(more_names.gene_names).Distinct();
        }

        public void set_preferred(GeneName top_name)
        {
            this.primary = top_name.primary;
            this.ordered_locus = top_name.primary;
        }
    }
}
