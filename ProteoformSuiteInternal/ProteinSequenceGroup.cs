using System.Collections.Generic;
using System.Linq;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class ProteinSequenceGroup : ProteinWithGoTerms
    {
        public List<ProteinWithGoTerms> proteinList { get; private set; }

        public ProteinSequenceGroup(IEnumerable<ProteinWithGoTerms> proteins)
            : base(proteins.First().BaseSequence,
                proteins.Any(p => p.IsContaminant) ? proteins.FirstOrDefault(p => p.IsContaminant).Accession + "_G" + proteins.Count() : proteins.First().Accession + "_G" + proteins.Count(),
                proteins.SelectMany(p => p.GeneNames),
                proteins.SelectMany(p => p.OneBasedPossibleLocalizedModifications.Keys).Distinct().ToDictionary(i => i, i => proteins.Where(p => p.OneBasedPossibleLocalizedModifications.ContainsKey(i)).SelectMany(p => p.OneBasedPossibleLocalizedModifications[i]).ToList()),
                proteins.Any(p => p.IsContaminant) ? proteins.FirstOrDefault(p => p.IsContaminant).ProteolysisProducts.Select(p => p.OneBasedBeginPosition).ToArray() : proteins.First().ProteolysisProducts.Select(p => p.OneBasedBeginPosition).ToArray(),
                proteins.Any(p => p.IsContaminant) ? proteins.FirstOrDefault(p => p.IsContaminant).ProteolysisProducts.Select(p => p.OneBasedEndPosition).ToArray() : proteins.First().ProteolysisProducts.Select(p => p.OneBasedEndPosition).ToArray(),
                proteins.Any(p => p.IsContaminant) ? proteins.FirstOrDefault(p => p.IsContaminant).ProteolysisProducts.Select(p => p.Type).ToArray() : proteins.First().ProteolysisProducts.Select(p => p.Type).ToArray(),
                proteins.Any(p => p.IsContaminant) ? proteins.FirstOrDefault(p => p.IsContaminant).Name : proteins.First().Name,
                proteins.Any(p => p.IsContaminant) ? proteins.FirstOrDefault(p => p.IsContaminant).FullName : proteins.First().FullName, 
                false, 
                proteins.Any(p => p.IsContaminant), 
                proteins.SelectMany(p => p.DatabaseReferences),
                proteins.SelectMany(p => p.GoTerms))
        {
            this.proteinList = proteins.ToList();
            this.AccessionList = proteins.Select(p => p.Accession).ToList();
            this.AccessionList.Sort();
        }
    }
}
