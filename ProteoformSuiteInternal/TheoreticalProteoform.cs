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
        public List<ProteinWithGoTerms> proteinList { get; set; } = new List<ProteinWithGoTerms>();
        public string name { get; set; }
        public string description { get; set; }
        public string fragment { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public double unmodified_mass { get; set; }
        public List<GoTerm> goTerms { get; set; } = new List<GoTerm>();
        public string goTerm_IDs { get { return String.Join("; ", goTerms.Select(g => g.Id)); } }
        public GeneName gene_name { get; set; }
        public PtmSet ptm_set { get; set; } = new PtmSet(new List<Ptm>());
        public List<Ptm> ptm_list { get { return ptm_set.ptm_combination.ToList(); } }
        public double ptm_mass { get { return ptm_set.mass; } }
        public string ptm_descriptions
        {
            get { return ptm_list_string(); }
        }
        public List<Psm> psm_list { get; set; } = new List<Psm>();
        //private int _psm_count_BU;
        //public int psm_count_BU
        //{
        //    get
        //    {
        //        if (!Lollipop.opened_results_originally)
        //            return psm_list.Where(p => p.psm_type == PsmType.BottomUp).ToList().Count;
        //        else return _psm_count_BU;
        //    }
        //    set { _psm_count_BU = value; }
        //}
        //private int _psm_count_TD;
        //public int psm_count_TD
        //{
        //    get
        //    {
        //        if (!Lollipop.opened_results_originally)
        //            return psm_list.Where(p => p.psm_type == PsmType.TopDown).ToList().Count;
        //        else return _psm_count_TD;
        //    }
        //    set { _psm_count_TD = value; }
        //}
        public string of_interest { get; set; } = "";
        public bool contaminant { get; set; }

        public TheoreticalProteoform(string accession, string description, ProteinWithGoTerms protein, bool is_metCleaved, double unmodified_mass, int lysine_count, PtmSet ptm_set, double modified_mass, bool is_target, bool check_contaminants, Dictionary<InputFile, Protein[]> theoretical_proteins) 
            : base(accession, modified_mass, lysine_count, is_target)
        {
            this.proteinList.Add(protein);
            this.accession = accession;
            this.description = description;
            this.name = protein.Name;
            this.fragment = protein.ProteolysisProducts.FirstOrDefault().Type;
            this.begin = (int)protein.ProteolysisProducts.FirstOrDefault().OneBasedBeginPosition + Convert.ToInt32(is_metCleaved);
            this.end = (int)protein.ProteolysisProducts.FirstOrDefault().OneBasedEndPosition;
            this.goTerms = protein.GoTerms.ToList();
            this.gene_name = new GeneName(protein.GeneNames);
            this.ptm_set = ptm_set;
            this.unmodified_mass = unmodified_mass;
            if (check_contaminants) this.contaminant = theoretical_proteins.Where(item => item.Key.ContaminantDB).SelectMany(kv => kv.Value).Any(p => p.Accession == this.accession.Split(new char[] { '_' })[0]);
        }

        public TheoreticalProteoform(string accession, string description, string name, string fragment, int begin, int end, double unmodified_mass, int lysine_count, PtmSet ptm_set, double modified_mass, bool is_target)
            : base(accession, modified_mass, lysine_count, is_target)
        {
            this.accession = accession;
            this.description = description;
            this.name = name;
            this.fragment = fragment;
            this.begin = begin;
            this.end = end;
            this.ptm_set = ptm_set;
            this.unmodified_mass = unmodified_mass;
        }

        //for Tests
        public TheoreticalProteoform(string accession) 
            : base(accession)
        {
            this.accession = accession;
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
            if (ptm_list.Count == 0)
                return "Unmodified";
            else
                return string.Join("; ", ptm_list.Select(ptm => ptm.modification.id));
        }
    }
}
