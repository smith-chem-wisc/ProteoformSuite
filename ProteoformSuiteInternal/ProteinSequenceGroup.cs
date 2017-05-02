using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class ProteinSequenceGroup : ProteinWithGoTerms
    {
        public List<ProteinWithGoTerms> proteinWithGoTermList;
        public ProteinSequenceGroup(IEnumerable<ProteinWithGoTerms> proteins_with_contaminants_first)
            : base(proteins_with_contaminants_first.First().BaseSequence,
                proteins_with_contaminants_first.First().Accession + "_" + proteins_with_contaminants_first.Count() + "G",
                proteins_with_contaminants_first.SelectMany(p => p.GeneNames).ToList(),
                proteins_with_contaminants_first.SelectMany(p => p.OneBasedPossibleLocalizedModifications.Keys).Distinct()
                  .ToDictionary(i => i, i => proteins_with_contaminants_first.Where(p => p.OneBasedPossibleLocalizedModifications.ContainsKey(i)).SelectMany(p => p.OneBasedPossibleLocalizedModifications[i]).ToList()),
                proteins_with_contaminants_first.First().ProteolysisProducts.Select(p => p.OneBasedBeginPosition).ToArray(),
                proteins_with_contaminants_first.First().ProteolysisProducts.Select(p => p.OneBasedEndPosition).ToArray(),
                proteins_with_contaminants_first.First().ProteolysisProducts.Select(p => p.Type).ToArray(),
                String.Join(";", proteins_with_contaminants_first.Select(p => p.Name)),
                proteins_with_contaminants_first.First().FullName,
                false, 
                proteins_with_contaminants_first.Any(p => p.IsContaminant), 
                proteins_with_contaminants_first.SelectMany(p => p.DatabaseReferences),
                proteins_with_contaminants_first.SelectMany(p => p.GoTerms))
        {
            proteinWithGoTermList = proteins_with_contaminants_first.ToList();
            this.AccessionList = proteins_with_contaminants_first.Select(p => p.Accession).ToList();
            this.AccessionList.Sort();
        }
    }
}
