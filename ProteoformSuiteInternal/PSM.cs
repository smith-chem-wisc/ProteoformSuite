using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class Psm
    {
        //bottom up data
        public string sequence { get; set; }
        public string filename { get; set; }
        public int start_residue { get; set; }
        public int stop_residue { get; set; }   
        public double total_intensity { get; set; }
        public double precursor_intensity { get; set; }
        public double morpheus_score { get; set; }
        public int spectrum { get; set; }
        public string protein_description { get; set; }
        public double precursor_mz { get; set; }
        public int precursor_charge { get; set; }
        public double precursor_mass_error { get; set; }

        public Psm (string sequence, string filename, int start_residue, int stop_residue, double total_intensity, double precursor_intensity, double morpheus_score, int spectrum,string protein_description, double precursor_mz, int precursor_charge, double precursor_mass_error)
        {
            this.sequence = sequence;
            this.filename = filename;
            this.start_residue = start_residue;
            this.stop_residue = stop_residue;
            this.spectrum = spectrum;
            this.total_intensity = total_intensity;
            this.morpheus_score = morpheus_score;
            this.precursor_intensity = precursor_intensity;
            this.protein_description = protein_description;
            this.precursor_mz = precursor_mz;
            this.precursor_charge = precursor_charge;
            this.precursor_mass_error = precursor_mass_error;
        }
    }
}
