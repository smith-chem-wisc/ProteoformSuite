using Proteomics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class TheoreticalProteoform : Proteoform
    {

        #region Public Properties

        public List<ProteinWithGoTerms> ExpandedProteinList { get; set; } = new List<ProteinWithGoTerms>();
        public string name { get; set; }
        public string description { get; set; }
        public string fragment { get; set; }
        public string sequence { get; set; }
        public double unmodified_mass { get; set; }
        public string goTerm_IDs { get; private set; }
        public double ptm_mass { get { return ptm_set.mass; } }
      //  public List<BottomUpPSM> psm_list { get; set; } = new List<BottomUpPSM>();
        public bool contaminant { get; set; }
        public List<GoTerm> goTerms { get; private set; }
        public bool topdown_theoretical { get; set; }
        #endregion Public Properties

        #region Public Constructor

        public TheoreticalProteoform(string accession, string description, string sequence, IEnumerable<ProteinWithGoTerms> expanded_protein_list, double unmodified_mass, int lysine_count, PtmSet ptm_set, bool is_target, bool check_contaminants, Dictionary<InputFile, Protein[]> theoretical_proteins)
            : base(accession, unmodified_mass + ptm_set.mass, lysine_count, is_target)
        {
            this.linked_proteoform_references = new List<Proteoform>();
            this.ExpandedProteinList = expanded_protein_list.ToList();
            this.accession = accession;
            this.description = description.Split('|').Length >= 3 ? description.Split('|')[2] : description;
            this.name = String.Join(";", expanded_protein_list.Select(p => p.Name));
            this.fragment = String.Join(";", expanded_protein_list.Select(p => p.ProteolysisProducts.FirstOrDefault().Type));
            this.begin = (int)expanded_protein_list.FirstOrDefault().ProteolysisProducts.FirstOrDefault().OneBasedBeginPosition;
            this.end = (int)expanded_protein_list.FirstOrDefault().ProteolysisProducts.FirstOrDefault().OneBasedEndPosition;
            this.sequence = sequence;
            this.goTerms = expanded_protein_list.SelectMany(p => p.GoTerms).ToList();
            goTerm_IDs = String.Join("; ", goTerms.Select(g => g.Id));
            this.gene_name = new GeneName(expanded_protein_list.SelectMany(t => t.GeneNames).ToList());
            this.ptm_set = ptm_set;
            this.unmodified_mass = unmodified_mass;
            if (check_contaminants) this.contaminant = theoretical_proteins.Where(item => item.Key.ContaminantDB).SelectMany(kv => kv.Value).Any(p => p.Accession == this.accession.Split(new char[] { '_' })[0]);
        }

        #endregion Public Constructor

        #region Public Method

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
        #endregion Public Method

    }
}
