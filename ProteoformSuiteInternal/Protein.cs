using System.Collections.Generic;
using System.Linq;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class ProteinSequenceGroup : Protein
    {
        public List<string> accessionList { get; set; } // this is the list of accession numbers for all proteins that share the same sequence. the list gets alphabetical order
        public ProteinSequenceGroup(List<Protein> proteins)
            : base(proteins[0].BaseSequence, 
                proteins[0].Accession + "_G" + proteins.Count(),
                proteins.SelectMany(p => p.OneBasedPossibleLocalizedModifications.Keys).Distinct().ToDictionary(i => i, i => proteins.Where(p => p.OneBasedPossibleLocalizedModifications.ContainsKey(i)).SelectMany(p => p.OneBasedPossibleLocalizedModifications[i]).ToList()), 
                proteins[0].OneBasedBeginPositions, 
                proteins[0].OneBasedEndPositions, 
                proteins[0].BigPeptideTypes, 
                proteins[0].Name, 
                proteins[0].FullName, 
                false, 
                proteins[0].IsContaminant, 
                proteins[0].GoTerms)
        {
            this.accessionList = proteins.Select(p => p.Accession).ToList();
        }
    }
}
