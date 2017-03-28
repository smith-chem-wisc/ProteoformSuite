using System;
using System.Collections.Generic;
using System.Linq;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class TheoreticalProteoformGroup : TheoreticalProteoform
    {
        public List<string> accessionList { get; set; } // this is the list of accession numbers for all proteoforms that share the same modified mass. the list gets alphabetical order

        public TheoreticalProteoformGroup(IEnumerable<TheoreticalProteoform> theoreticals_with_contaminants_first)
            : base(theoreticals_with_contaminants_first.FirstOrDefault().accession + "_" + theoreticals_with_contaminants_first.Count() + "T",
                theoreticals_with_contaminants_first.FirstOrDefault().description,
                theoreticals_with_contaminants_first.SelectMany(p => p.proteinList),
                false, //don't increment begin...
                theoreticals_with_contaminants_first.FirstOrDefault().unmodified_mass,
                theoreticals_with_contaminants_first.FirstOrDefault().lysine_count,
                theoreticals_with_contaminants_first.FirstOrDefault().ptm_set,
                theoreticals_with_contaminants_first.FirstOrDefault().is_target,
                false,
                new Dictionary<InputFile, Protein[]>())
        {
            this.description = String.Join(";", theoreticals_with_contaminants_first.Select(t => t.description));
            this.name = String.Join(";", theoreticals_with_contaminants_first.Select(t => t.name));
            this.fragment = String.Join(";", theoreticals_with_contaminants_first.Select(t => t.fragment));
            this.theoretical_reference_fragment = fragment;
            this.accessionList = theoreticals_with_contaminants_first.Select(p => p.accession).ToList();
            this.contaminant = theoreticals_with_contaminants_first.FirstOrDefault().contaminant;
        }
    }
}
