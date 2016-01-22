using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuite
{
    // Not an immutable class, so that settings can be changed in the future in dynamic feedback to/from user
    class DatabaseGenerator
    {
        private bool methionineOxidation = true;
        private bool carbamidoMethylation = true;
        private bool cleaved_N_TerminalMethionine = true;
        private string lysineIsotopes = "n"; // default to natural abundance 'n'. NeuCode light 'l' and heavy'h' are options.
        private int maxPTMs = 0;
        public const int maxMaxPtms = 4;

        // TODO: implement database generator contructor(s)
        public DatabaseGenerator(bool met_oxidation, string xml_file, bool carbam, bool cleaved_met, string lysine, int max_ptms, string outFile)
        {
            methionineOxidation = met_oxidation;
            carbamidoMethylation = carbam;
            cleaved_N_TerminalMethionine = cleaved_met;
            lysineIsotopes = lysine;
            maxPTMs = max_ptms;
        }

        // TODO: Implement integration of databases (proteoform database, UniProt, Ensembl) to form a dataframe for 
        // TODO: Implement pull-down of most recent databases (proteoform, UniProt, Ensembl) from the web
        // TODO: Implement this so that it can make it from any of these databases and also from an proteoform ID dataframeS
        public void Generate()
        {

        }

        // TODO: Set up write proteoformXML to outfile from proteoform database dataframe
        public void Export()
        {
            
        }

    }
}
