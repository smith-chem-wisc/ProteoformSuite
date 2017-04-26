using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class TheoreticalProteoformGroup : TheoreticalProteoform
    {

        #region Public Property

        public List<string> accessionList { get; set; } // this is the list of accession numbers for all proteoforms that share the same modified mass. the list gets alphabetical order

        #endregion Public Property

        #region Public Constructor

        public TheoreticalProteoformGroup(IEnumerable<TheoreticalProteoform> theoreticals_with_contaminants_first)
            : base(theoreticals_with_contaminants_first.FirstOrDefault().accession + "_" + theoreticals_with_contaminants_first.Count() + "T",
                theoreticals_with_contaminants_first.FirstOrDefault().description,
                theoreticals_with_contaminants_first.SelectMany(p => p.ExpandedProteinList),
                theoreticals_with_contaminants_first.FirstOrDefault().unmodified_mass,
                theoreticals_with_contaminants_first.FirstOrDefault().lysine_count,
                theoreticals_with_contaminants_first.FirstOrDefault().ptm_set,
                theoreticals_with_contaminants_first.FirstOrDefault().is_target,
                false,
                new Dictionary<InputFile, Protein[]>())
        {
            description = String.Join(";", theoreticals_with_contaminants_first.Select(t => t.description));
            name = String.Join(";", theoreticals_with_contaminants_first.Select(t => t.name));
            fragment = String.Join(";", theoreticals_with_contaminants_first.Select(t => t.fragment));
            accessionList = theoreticals_with_contaminants_first.Select(p => p.accession).ToList();
            contaminant = theoreticals_with_contaminants_first.FirstOrDefault().contaminant;
        }

        #endregion Public Constructor

    }
}
