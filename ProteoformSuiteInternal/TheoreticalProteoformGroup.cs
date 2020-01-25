using Proteomics;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class TheoreticalProteoformGroup : TheoreticalProteoform
    {
        #region Public Constructor

        public TheoreticalProteoformGroup(IEnumerable<TheoreticalProteoform> theoreticals_with_contaminants_first)
            : base(theoreticals_with_contaminants_first.FirstOrDefault().accession + "_" + theoreticals_with_contaminants_first.Count() + "T",
                theoreticals_with_contaminants_first.FirstOrDefault().description,
                theoreticals_with_contaminants_first.FirstOrDefault().sequence,
                theoreticals_with_contaminants_first.SelectMany(p => p.ExpandedProteinList),
                theoreticals_with_contaminants_first.FirstOrDefault().unmodified_mass,
                theoreticals_with_contaminants_first.FirstOrDefault().lysine_count,
                theoreticals_with_contaminants_first.FirstOrDefault().ptm_set,
                theoreticals_with_contaminants_first.FirstOrDefault().is_target,
                false,
                new Dictionary<InputFile, Protein[]>())
        {
            description = string.Join(";", theoreticals_with_contaminants_first.Select(t => t.description));
            name = string.Join(";", theoreticals_with_contaminants_first.Select(t => t.name));
            fragment = string.Join(";", theoreticals_with_contaminants_first.Select(t => t.fragment));
            contaminant = theoreticals_with_contaminants_first.FirstOrDefault().contaminant;
            topdown_theoretical = theoreticals_with_contaminants_first.Any(t => t.topdown_theoretical);
            new_topdown_proteoform = theoreticals_with_contaminants_first.Any(t => t.new_topdown_proteoform);
            bottom_up_PSMs = theoreticals_with_contaminants_first.SelectMany(t => t.bottom_up_PSMs).Distinct().ToList();
        }

        #endregion Public Constructor
    }
}