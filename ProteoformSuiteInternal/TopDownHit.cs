using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteomics;

namespace ProteoformSuiteInternal
{

    public class TopDownHit
    {
        public MsScan ms1Scan { get; set; }
        public int ms2ScanNumber { get; set; }
        public double retention_time { get; set; }
        public string filename { get; set; }
        public string uniprot_id { get; set; }
        public string sequence { get; set; }
        public int start_index { get; set; } //position one based
        public int stop_index { get; set; } //position one based
        public List<Ptm> ptm_list { get; set; } = new List<Ptm>(); //position one based. this list is empty if unmodified.
        public double theoretical_mass { get; set; }
        public string accession { get; set; }
        public string name { get; set; }

        public double reported_mass { get; set; } //reported in TD results file

        public int charge { get; set; }
        public double mass_error { get; set; }
        public double mz { get; set; }
        public bool targeted { get; set; }
        public double score { get; set; }//C-score
        public TopDownResultType tdResultType { get; set; }
        public InputFile file { get; set; }

        public TopDownHit(Dictionary<char, double> aaIsotopeMassList, InputFile file, TopDownResultType tdResultType, string accession, string uniprot_id, string name, string sequence, int start_index, int stop_index, List<Ptm> modifications, double reported_mass, double theoretical_mass, int scan, double retention_time, string filename, bool targeted, double score)
        {
            this.file = file;
            this.tdResultType = tdResultType;
            this.accession = accession;
            this.uniprot_id = uniprot_id;
            this.name = name;
            this.sequence = sequence;
            this.start_index = start_index;
            this.stop_index = stop_index;
            this.ptm_list = modifications;
            this.reported_mass = reported_mass;
            this.theoretical_mass = TheoreticalProteoform.CalculateProteoformMass(sequence, aaIsotopeMassList) + ptm_list.Sum(p => p.modification.monoisotopicMass);
            this.ms2ScanNumber = scan;
            this.retention_time = retention_time;
            this.filename = filename;
            this.targeted = targeted;
            this.score = score;
        }

        public TopDownHit()
        {

        }

        public bool same_ptm_hits(TopDownHit root)
        {
            if (this.ptm_list.Count == root.ptm_list.Count)
            {
                foreach (Ptm mod in this.ptm_list)
                {
                    if (root.ptm_list.Where(p => p.modification == mod.modification && p.position == mod.position).Count() == 1) continue;
                    else return false;
                }
                return true;
            }
            else return false;
        }

        public double get_mass_error(double theoretical, double observed)
        {
            return (observed - theoretical_mass) - Math.Round(observed - theoretical, 0);
        }

        public string GetSequenceWithChemicalFormula()
        {

            var sbsequence = new StringBuilder();

            // variable modification on peptide N-terminus
            ModificationWithMass pep_n_term_variable_mod = ptm_list.Where(p => p.position == 1).Select(m => m.modification).FirstOrDefault();
            if (pep_n_term_variable_mod != null)
            {
                var jj = pep_n_term_variable_mod as ModificationWithMassAndCf;
                if (jj != null && Math.Abs(jj.chemicalFormula.MonoisotopicMass - jj.monoisotopicMass) < 1e-5)
                    sbsequence.Append('[' + jj.chemicalFormula.Formula + ']');
                else
                    return null;
            }

            for (int r = 0; r < sequence.Length; r++)
            {
                sbsequence.Append(sequence[r]);
                // variable modification on this residue
                ModificationWithMass residue_variable_mod = ptm_list.Where(p => p.position == r + 2).Select(m => m.modification).FirstOrDefault();
                if (residue_variable_mod != null)
                {
                    var jj = residue_variable_mod as ModificationWithMassAndCf;
                    if (jj != null && Math.Abs(jj.chemicalFormula.MonoisotopicMass - jj.monoisotopicMass) < 1e-5)
                        sbsequence.Append('[' + jj.chemicalFormula.Formula + ']');
                    else
                        return null;
                }
            }

            // variable modification on peptide C-terminus
            ModificationWithMass pep_c_term_variable_mod = ptm_list.Where(p => p.position == sequence.Length + 2).Select(m => m.modification).FirstOrDefault();
            if (pep_c_term_variable_mod != null)
            {
                var jj = pep_c_term_variable_mod as ModificationWithMassAndCf;
                if (jj != null && Math.Abs(jj.chemicalFormula.MonoisotopicMass - jj.monoisotopicMass) < 1e-5)
                    sbsequence.Append('[' + jj.chemicalFormula.Formula + ']');
                else
                    return null;
            }

            return sbsequence.ToString();
        }

    }


    public enum TopDownResultType
    {
        Biomarker,
        TightAbsoluteMass
    }

}