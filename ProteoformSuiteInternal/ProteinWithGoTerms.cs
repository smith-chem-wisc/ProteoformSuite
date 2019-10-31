﻿using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class ProteinWithGoTerms : Protein
    {
        public List<string> AccessionList { get; set; }
        public List<goTerm> GoTerms { get; set; }
        public bool topdown_protein { get; set; }

        public ProteinWithGoTerms(string sequence, string accession, List<Tuple<string, string>> gene_names, IDictionary<int, List<Modification>> oneBasedModifications, List<ProteolysisProduct> proteolysisProducts, string name, string full_name, bool isDecoy, bool isContaminant, IEnumerable<DatabaseReference> databaseReferences, IEnumerable<goTerm> goTerms)
            : base(sequence, accession, geneNames: gene_names, oneBasedModifications: oneBasedModifications, proteolysisProducts: proteolysisProducts, name: name, fullName: full_name, isDecoy: isDecoy, isContaminant: isContaminant, databaseReferences: databaseReferences.ToList())
        {
            RestoreUnfilteredModifications();
            this.GoTerms = goTerms.ToList();
            this.AccessionList = new List<string> { accession };
        }
    }
}