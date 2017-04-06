using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class TheoreticalProteoform : Proteoform
    {
        public IEnumerable<ProteinWithGoTerms> proteinList { get; set; } = new List<ProteinWithGoTerms>();
        public string name { get; set; }
        public string description { get; set; }
        public string fragment { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public double unmodified_mass { get; set; }
        public List<GoTerm> goTerms { get; set; } = new List<GoTerm>();
        public string goTerm_IDs { get { return String.Join("; ", goTerms.Select(g => g.Id)); } }
        public double ptm_mass { get { return ptm_set.mass; } }
        public string ptm_descriptions
        {
            get { return ptm_list_string(); }
        }
        public bool contaminant { get; set; }

        public TheoreticalProteoform(string accession, string description, IEnumerable<ProteinWithGoTerms> protein_list, bool is_metCleaved, double unmodified_mass, int lysine_count, PtmSet ptm_set, bool is_target, bool check_contaminants, Dictionary<InputFile, Protein[]> theoretical_proteins) 
            : base(accession, unmodified_mass + ptm_set.mass, lysine_count, is_target)
        {
            this.theoretical_reference = this;
            this.proteinList = protein_list;
            this.accession = accession;
            this.theoretical_reference_accession = accession;
            this.description = description;
            this.name = String.Join(";", protein_list.Select(p => p.Name));
            this.fragment = String.Join(";", protein_list.Select(p => p.ProteolysisProducts.FirstOrDefault().Type));
            this.theoretical_reference_fragment = fragment;
            this.begin = (int)protein_list.FirstOrDefault().ProteolysisProducts.FirstOrDefault().OneBasedBeginPosition + Convert.ToInt32(is_metCleaved);
            this.end = (int)protein_list.FirstOrDefault().ProteolysisProducts.FirstOrDefault().OneBasedEndPosition;
            this.goTerms = proteinList.SelectMany(p => p.GoTerms).ToList();
            this.gene_name = new GeneName(protein_list.SelectMany(t => t.GeneNames));
            this.ptm_set = ptm_set;
            this.unmodified_mass = unmodified_mass;
            if (check_contaminants) this.contaminant = theoretical_proteins.Where(item => item.Key.ContaminantDB).SelectMany(kv => kv.Value).Any(p => p.Accession == this.accession.Split(new char[] { '_' })[0]);
        }

        public static double CalculateProteoformMass(string pForm, Dictionary<char, double> aaIsotopeMassList)
        {
            double proteoformMass = 18.010565; // start with water
            char[] aminoAcids = pForm.ToCharArray();
            List<double> aaMasses = new List<double>();
            for (int i = 0; i < pForm.Length; i++)
            {
                if (aaIsotopeMassList.ContainsKey(aminoAcids[i])) aaMasses.Add(aaIsotopeMassList[aminoAcids[i]]);
            }
            return proteoformMass + aaMasses.Sum();
        }

        public string ptm_list_string()
        {
            if (ptm_set.ptm_combination.Count == 0)
                return "Unmodified";
            else
                return string.Join("; ", ptm_set.ptm_combination.Select(ptm => ptm.modification.id));
        }
    }
}
