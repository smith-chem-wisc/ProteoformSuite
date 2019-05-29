using MassSpectrometry;
using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chemistry;

namespace ProteoformSuiteInternal
{
    public class TopDownHit
    {
        public int ms2ScanNumber { get; set; }
        public double ms2_retention_time { get; set; }
        public string filename { get; set; }
        public string uniprot_id { get; set; }
        public string sequence { get; set; }
        public int begin { get; set; } //position one based
        public int end { get; set; } //position one based
        public List<Ptm> ptm_list { get; set; } = new List<Ptm>(); //position one based. this list is empty if unmodified.
        public double theoretical_mass { get; set; }
        public string accession { get; set; }
        public string name { get; set; }
        public double pscore { get; set; }
        public double reported_mass { get; set; } //reported in TD results file
        public double score { get; set; }//C-score
        public TopDownResultType tdResultType { get; set; }
        public InputFile file { get; set; }
        public string pfr_accession { get; set; }

        //for mass calibration
        public string biological_replicate { get; set; } = "";
        public string technical_replicate { get; set; } = "";
        public string fraction { get; set; } = "";
        public string condition { get; set; } = "";
        public double mz { get; set; }
        public int charge { get; set; }
        public MsDataScan ms1_scan { get; set; }

        public TopDownHit(Dictionary<char, double> aaIsotopeMassList, InputFile file, TopDownResultType tdResultType, string accession, string pfr, string uniprot_id, string name, string sequence, int begin, int end, List<Ptm> modifications, double reported_mass, double theoretical_mass, int scan, double retention_time, string filename, double pscore, double score)
        {
            this.pfr_accession = pfr;
            this.file = file;
            this.tdResultType = tdResultType;
            this.accession = accession;
            this.uniprot_id = uniprot_id;
            this.name = name;
            this.sequence = sequence;
            this.begin = begin;
            this.end = end;
            this.ptm_list = modifications;
            //if neucode labeled, calculate neucode mass....
            this.reported_mass = Sweet.lollipop.neucode_labeled ? Sweet.lollipop.get_neucode_mass(reported_mass, sequence.Count(s => s == 'K')) : reported_mass;
            this.theoretical_mass = CalculateProteoformMass(sequence, aaIsotopeMassList) + ptm_list.Where(p => p.modification != null).Sum(p => Math.Round((double)p.modification.MonoisotopicMass, 5));
            this.ms2ScanNumber = scan;
            this.ms2_retention_time = retention_time;
            this.filename = filename;
            this.score = score;
            this.pscore = pscore;
        }

        public TopDownHit()
        {
        }

        private double CalculateProteoformMass(string pForm, Dictionary<char, double> aaIsotopeMassList)
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

        public ChemicalFormula GetChemicalFormula()
        {
            var formula = new Proteomics.AminoAcidPolymer.Peptide(sequence).GetChemicalFormula();

            // append mod formulas
            foreach (var mod in ptm_list)
            {
                var modCf = mod.modification.ChemicalFormula;

                if (modCf != null)
                {
                    formula.Add(modCf);
                }
                else
                {
                    return null;
                }
            }

            return formula;
        }
    }

    public enum TopDownResultType
    {
        Biomarker,
        TightAbsoluteMass,
        Unknown //not read in if unknown...
    }
}