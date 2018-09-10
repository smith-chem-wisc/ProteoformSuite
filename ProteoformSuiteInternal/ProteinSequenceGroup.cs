﻿using Proteomics;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class ProteinSequenceGroup : ProteinWithGoTerms
    {
        public List<ProteinWithGoTerms> proteinWithGoTermList = new List<ProteinWithGoTerms>();

        public ProteinSequenceGroup(IEnumerable<ProteinWithGoTerms> proteins_with_contaminants_first)
            : base(proteins_with_contaminants_first.First().BaseSequence,
                proteins_with_contaminants_first.First().Accession + "_" + proteins_with_contaminants_first.Count() + "G",
                proteins_with_contaminants_first.SelectMany(p => p.GeneNames).ToList(),
                CollapseModifications(proteins_with_contaminants_first),
                proteins_with_contaminants_first.First().ProteolysisProducts.ToList(),
                string.Join(";", proteins_with_contaminants_first.Select(p => p.Name).Distinct()),
                proteins_with_contaminants_first.First().FullName,
                false,
                proteins_with_contaminants_first.Any(p => p.IsContaminant),
                proteins_with_contaminants_first.SelectMany(p => p.DatabaseReferences),
                proteins_with_contaminants_first.SelectMany(p => p.GoTerms))
        {
            RestoreUnfilteredModifications();
            proteinWithGoTermList = proteins_with_contaminants_first.ToList();
            this.AccessionList = proteins_with_contaminants_first.SelectMany(p => p.AccessionList).ToList();
            this.AccessionList.Sort();
            topdown_protein = proteinWithGoTermList.Any(p => p.topdown_protein);
        }

        private static Dictionary<int, List<Modification>> CollapseModifications(IEnumerable<ProteinWithGoTerms> proteins_with_contaminants_first)
        {
            List<int> positions = proteins_with_contaminants_first.SelectMany(p => p.OneBasedPossibleLocalizedModifications.Keys).Distinct().ToList();
            return positions.ToDictionary(pos => pos, pos =>
                proteins_with_contaminants_first.SelectMany(p => p.OneBasedPossibleLocalizedModifications.TryGetValue(pos, out List<Modification> mods) ? mods : new List<Modification>()).ToList());
        }
    }
}