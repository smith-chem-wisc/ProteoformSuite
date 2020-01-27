using MassSpectrometry;
using Proteomics.Fragmentation;
using System;
using System.Collections.Generic;
using System.Linq;
using Chemistry;
using Proteomics.ProteolyticDigestion;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class SpectrumMatch
    {
        public int ms2ScanNumber { get; set; }
        public double ms2_retention_time { get; set; }
        public string filename { get; set; }
        public string uniprot_id { get; set; }
        public string sequence { get; set; }
        public int begin { get; set; } //position one based
        public int end { get; set; } //position one based
        public double theoretical_mass { get; set; }
        public string accession { get; set; } = "";
        public string name { get; set; }
        public GeneName gene_name { get; set; }
        public double pscore { get; set; }
        public double reported_mass { get; set; } //reported in TD results file
        public double score { get; set; }//C-score
        public TopDownResultType tdResultType { get; set; }
        public InputFile file { get; set; }

        public string pfr_accession
        {
            get
            {
                return accession.Split('-')[0] + "_" + begin + "_" + end + "_" + full_sequence + (ambiguous_matches.Count > 0 ? 
                    "|" + string.Join("|", ambiguous_matches.Select(a => a.accession.Split('-')[0] + "_" + a.begin + "_" + a.end + "_" + a.full_sequence)) : "");
            }
        }

        public string original_pfr_accession { get; set; }

        public string full_sequence { get; set; } = "";

        public List<Ptm> _ptm_list { get; set; } = new List<Ptm>(); //position one based. this list is empty if unmodified.
        public List<Ptm> ptm_list //the ptmset read in with td data
        {
            get
            {
                return _ptm_list;
            }

            set
            {
                _ptm_list = value;
                ptm_description = _ptm_list == null || _ptm_list == null ?
                    "Unknown" :
                    _ptm_list.Count(m => m.modification.ModificationType != "Common Fixed") == 0 ?
                        "Unmodified" :
                        string.Join("; ", _ptm_list.Where(ptm => ptm.modification.ModificationType != "Common Fixed").OrderBy(m => m.position).ThenBy(m => UnlocalizedModification.LookUpId(m.modification)).Select(ptm => ptm.position > 0 ? UnlocalizedModification.LookUpId(ptm.modification) + "@" + ptm.position : UnlocalizedModification.LookUpId(ptm.modification)).ToList());

            }
        }

        public bool shared_protein { get; set; }
        public string ptm_description { get; set; }
        public List<SpectrumMatch> ambiguous_matches = new List<SpectrumMatch>();
        public List<MatchedFragmentIon> matched_fragment_ions = new List<MatchedFragmentIon>();

        //for calibration
        public string biological_replicate { get; set; } = "";
        public string technical_replicate { get; set; } = "";
        public string fraction { get; set; } = "";
        public string condition { get; set; } = "";
        public double mz { get; set; }
        public double calibrated_retention_time { get; set; }
        public int charge { get; set; }
        public MsDataScan ms1_scan { get; set; }

        public SpectrumMatch(Dictionary<char, double> aaIsotopeMassList, InputFile file, TopDownResultType tdResultType, string accession, string full_sequence, string uniprot_id, string name, string sequence, int begin, int end, List<Ptm> modifications, double reported_mass, double theoretical_mass, int scan, double retention_time, string filename, double pscore, double score, List<MatchedFragmentIon> matched_fragment_ions)
        {
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
            this.matched_fragment_ions = matched_fragment_ions;
            this.original_pfr_accession = full_sequence;

            Dictionary<int, Modification> allModsOneIsNTerminus = new Dictionary<int, Modification>();
            Dictionary<int, List<Modification>> allModsOneIsNTerminus_dictionary = new Dictionary<int, List<Modification>>();
            foreach (var ptm in ptm_list)
            {
                if(allModsOneIsNTerminus_dictionary.ContainsKey(ptm.position))
                {
                    allModsOneIsNTerminus_dictionary[ptm.position].Add(ptm.modification);
                }
                else
                {
                    allModsOneIsNTerminus_dictionary.Add(ptm.position, new List<Modification>() { ptm.modification });
                }
            }
            foreach(var ptm in allModsOneIsNTerminus_dictionary)
            {
                allModsOneIsNTerminus.Add(ptm.Key - (begin - 2), new Modification(String.Join(",", ptm.Value.Select(m => UnlocalizedModification.LookUpId(m)).OrderBy(m => m)) + " on X"));
            }
            Protein protein = new Protein(sequence, accession);
            PeptideWithSetModifications peptide = new PeptideWithSetModifications(protein, new DigestionParams(), 1, sequence.Length, CleavageSpecificity.Full, "", 0, allModsOneIsNTerminus, 0);
            this.full_sequence = peptide.FullSequence;
        }

        public SpectrumMatch()
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