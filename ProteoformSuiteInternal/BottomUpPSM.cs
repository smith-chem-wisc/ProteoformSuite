//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Chemistry;

//namespace ProteoformSuiteInternal
//{
//    public class BottomUpPSM
//    {
//        public string base_sequence { get; set; }
//        public string sequence_with_modifications
//        {
//            get {
//                string _base_sequence = base_sequence;
//                foreach (Ptm mod in modifications.OrderByDescending(m => m.position)) _base_sequence = _base_sequence.Insert(mod.position - 1, "[" + mod.modification.id + "]");
//                return _base_sequence;
//            }
//        }
//        public string start_residue { get; set; }
//        public string stop_residue { get; set; }
//        public string spectrum { get; set; }
//        public string protein_description { get; set; }
//        public string protein_accession { get; set; }
//        public double precursor_mz { get; set; }
//        public int precursor_charge { get; set; }
//        public double precursor_mass_error { get; set; }
//        public List<Ptm> modifications = new List<Ptm>();
//        public string ptm_descriptions { get
//            {
//                return
//                   modifications == null || modifications.Count == 0 ?
//                       "Unmodified" :
//                       String.Join("; ", modifications.Select(ptm => ptm.position > 0 ? ptm.modification.id + "@" + ptm.position : Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id));

//            }
//        }

//        public BottomUpPSM(string sequence, string start_residue, string stop_residue, List<Ptm> modifications,  string spectrum, string protein_accession, string protein_description, double precursor_mz, int precursor_charge, double theoretical_precursor_mz)
//        {
//            this.base_sequence = sequence;
//            this.start_residue = start_residue;
//            this.stop_residue = stop_residue;
//            this.spectrum = spectrum;
//            this.protein_accession = protein_accession;
//            this.protein_description = protein_description;
//            this.precursor_mz = precursor_mz;
//            this.precursor_charge = precursor_charge;
//            this.precursor_mass_error = precursor_mz.ToMass(precursor_charge) - theoretical_precursor_mz.ToMass(precursor_charge);
//            this.modifications = modifications;
//        }
//    }
//}
