using System.Collections.Generic;
using System.Linq;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class ProteinSequenceGroup : ProteinWithGoTerms
    {
        public List<ProteinWithGoTerms> proteinList { get; private set; }
        public List<string> accessionList { get; private set; }

        public ProteinSequenceGroup(List<ProteinWithGoTerms> proteins)
            : base(proteins[0].BaseSequence, 
                proteins[0].Accession + "_G" + proteins.Count(),
                proteins.SelectMany(p => p.OneBasedPossibleLocalizedModifications.Keys).Distinct().ToDictionary(i => i, i => proteins.Where(p => p.OneBasedPossibleLocalizedModifications.ContainsKey(i)).SelectMany(p => p.OneBasedPossibleLocalizedModifications[i]).ToList()), 
                proteins[0].ProteolysisProducts.Select(p => p.OneBasedBeginPosition).ToArray(), 
                proteins[0].ProteolysisProducts.Select(p => p.OneBasedEndPosition).ToArray(), 
                proteins[0].ProteolysisProducts.Select(p => p.Type).ToArray(), 
                proteins[0].Name, 
                proteins[0].FullName, 
                false, 
                proteins[0].IsContaminant, 
                proteins.SelectMany(p => p.DatabaseReferences),
                proteins.SelectMany(p => p.GoTerms))
        {
            this.proteinList = proteins;
            this.accessionList = proteins.Select(p => p.Accession).ToList();
            this.accessionList.Sort();
        }
    }
}
