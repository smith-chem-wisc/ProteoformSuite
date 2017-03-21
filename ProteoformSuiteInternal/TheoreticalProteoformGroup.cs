using System;
using System.Collections.Generic;
using System.Linq;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class TheoreticalProteoformGroup : TheoreticalProteoform
    {
        public List<string> accessionList { get; set; } // this is the list of accession numbers for all proteoforms that share the same modified mass. the list gets alphabetical order

        public TheoreticalProteoformGroup(List<TheoreticalProteoform> theoreticals, bool contaminants, Dictionary<InputFile, Protein[]> theoretical_proteins)
            : base(theoreticals[0].accession + "_" + theoreticals.Count + "T", String.Join(";", theoreticals.Select(t => t.description)), String.Join(";", theoreticals.Select(t => t.name)), String.Join(";", theoreticals.Select(t => t.fragment)), theoreticals[0].begin, theoreticals[0].end, theoreticals[0].unmodified_mass, theoreticals[0].lysine_count, theoreticals[0].ptm_set, theoreticals[0].modified_mass, theoreticals[0].is_target)
        {
            this.accessionList = theoreticals.Select(p => p.accession).ToList();
            this.proteinList = theoreticals.SelectMany(p => p.proteinList).ToList();
            this.goTerms = this.proteinList.SelectMany(p => p.GoTerms).ToList();
            this.gene_name = new GeneName(theoreticals.Select(t => t.gene_name));
            if (contaminants)
            {
                List<Protein> matching_contaminants = theoretical_proteins.Where(item => item.Key.ContaminantDB).SelectMany(kv => kv.Value).Where(p => this.accessionList.Select(acc => acc.Split(new char[] { '_' })[0]).Contains(p.Accession)).ToList();
                this.contaminant = matching_contaminants.Count > 0;
                if (!contaminant) return;
                this.accession = matching_contaminants[0].Accession + "_T" + accessionList.Count();
                this.description = String.Join(";", matching_contaminants.Select(t => t.FullDescription));
                this.name = String.Join(";", matching_contaminants.Select(t => t.Name));
                TheoreticalProteoform first_contaminant = theoreticals.FirstOrDefault(t => t.contaminant);
                this.begin = first_contaminant.begin;
                this.end = first_contaminant.end;
                this.unmodified_mass = first_contaminant.unmodified_mass;
                this.modified_mass = first_contaminant.modified_mass;
                this.lysine_count = first_contaminant.lysine_count;
                this.goTerms = first_contaminant.goTerms;
                this.gene_name.set_preferred(first_contaminant.gene_name);
                this.ptm_set = first_contaminant.ptm_set;
                this.is_target = first_contaminant.is_target;
                this.is_decoy = first_contaminant.is_decoy;
            }
        }
    }
}
