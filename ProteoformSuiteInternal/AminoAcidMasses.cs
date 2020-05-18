using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public class AminoAcidMasses
    {
        public Dictionary<char, double> AA_Masses { get; set; }

        public AminoAcidMasses(bool cBn, bool neucode_labeled)
        {
            var aaMasses = new Dictionary<char, double>();
            aaMasses.Add('A', 71.037114);
            aaMasses.Add('R', 156.101111);
            aaMasses.Add('N', 114.042927);
            aaMasses.Add('D', 115.026943);
            if (cBn)
            {
                aaMasses.Add('C', 160.030649);
            }
            else
            {
                aaMasses.Add('C', 103.009185);
            }
            aaMasses.Add('E', 129.042593);
            aaMasses.Add('Q', 128.058578);
            aaMasses.Add('G', 57.021464);
            aaMasses.Add('H', 137.058912);
            aaMasses.Add('I', 113.084064);
            aaMasses.Add('L', 113.084064);
            if (neucode_labeled)
            {
                aaMasses.Add('K', 136.109162);
            }
            else
            {
                aaMasses.Add('K', 128.094963);
            }
            aaMasses.Add('M', 131.040485);
            aaMasses.Add('F', 147.068414);
            aaMasses.Add('P', 97.052764);
            aaMasses.Add('S', 87.032028);
            aaMasses.Add('T', 101.047679);
            aaMasses.Add('W', 186.079313);
            aaMasses.Add('Y', 163.06332);
            aaMasses.Add('V', 99.068414);
            aaMasses.Add('O', Chemistry.ChemicalFormula.ParseFormula("C12H19N3O2").MonoisotopicMass);
            aaMasses.Add('U', Chemistry.ChemicalFormula.ParseFormula("C3H5NOSe").MonoisotopicMass);


            this.AA_Masses = aaMasses;
        }

        private static string WhichLysineIsotopeComposition(bool natural_lysine_isotope_abundance, bool neucode_light_lysine, bool neucode_heavy_lysine)
        {
            if (natural_lysine_isotope_abundance) { return "n"; }
            else if (neucode_light_lysine) { return "l"; }
            else if (neucode_heavy_lysine) { return "h"; }
            else { return ""; }
        }
    }
}