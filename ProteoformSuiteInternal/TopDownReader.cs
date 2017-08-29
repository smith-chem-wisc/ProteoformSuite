using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class TopDownReader
    {

        #region Private Fields

        private Dictionary<char, double> aaIsotopeMassList;

        #endregion Private Fields

        public List<string> topdown_ptms = new List<string>(); //PTMs not in theoretical database added to warning file.
        //Reading in Top-down excel
        public List<TopDownHit> ReadTDFile(InputFile file)
        {
            aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, true, false, false).AA_Masses; //always use natural K mass
            List<TopDownHit> td_hits = new List<TopDownHit>();

            List<List<string>> cells = ExcelReader.get_cell_strings(file, true);//This returns the entire sheet except for the header. Each row of cells is one List<string>
            //get ptms on proteoform -- check for mods. IF not in database, make new topdown mod, show Warning message. 
            Parallel.ForEach(cells, cellStrings =>
            {
                bool add_topdown_hit = true; //if PTM or accession not found, will not add (show warning)
                TopDownResultType tdResultType = (cellStrings[15] == "BioMarker") ? TopDownResultType.Biomarker : ((cellStrings[15] == "Tight Absolute Mass") ? TopDownResultType.TightAbsoluteMass : TopDownResultType.Unknown);
                if (tdResultType != TopDownResultType.Unknown) //uknown result type! 
                {
                    List<Ptm> ptm_list = new List<Ptm>(); // if nothing gets added, an empty ptmlist is passed to the topdownhit constructor.
                                                          //N-term modifications
                    if (cellStrings[10].Length > 0) //N Terminal Modification Code
                    {
                        int position = 1;
                        if (cellStrings[10].Split(':')[1] == "1458")//PSI-MOD 1458 is supposed to be N-terminal acetylation
                        {
                            ModificationWithMass mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.monoisotopicMass == 42.010565 && m.terminusLocalization == TerminusLocalization.NProt && m.motif.Motif == cellStrings[4][0].ToString()).FirstOrDefault();
                            if (mod != null)
                            {
                                ptm_list.Add(new Ptm(position, mod));
                            }
                            else
                            {
                                lock (topdown_ptms)
                                {
                                    topdown_ptms.Add("N-terminal acetylation at " + cellStrings[4][0]);
                                }
                                add_topdown_hit = false;
                            }
                        }
                    }
                    //don't have example of c-term modification to write code
                    //other mods
                    if (cellStrings[9].Length > 0)//Modification Codes
                    {
                        string[] res_ids = cellStrings[9].Split('|');
                        foreach (string ptm in res_ids)
                        {
                            string resid = ptm.Split(':')[1].Split('@')[0];//The number after the @ is the position in the protein
                            while (resid.Length < 4) resid = "0" + resid;//short part should be the accession number, which is an integer
                            resid = "AA" + resid;
                            int position = Convert.ToInt16(ptm.Split(':')[1].Split('@')[1]) + 1; //one based sequence
                            ModificationWithMass mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains(resid)).FirstOrDefault();
                            if (mod != null) ptm_list.Add(new Ptm(position, mod));
                            else
                            {
                                lock (topdown_ptms)
                                {
                                    topdown_ptms.Add(resid + " at " + cellStrings[4][position - 1]);
                                }
                                add_topdown_hit = false;
                            }
                        }
                    }

                    //This is the excel file header:
                    //convert into new td hit
                    //cellStrings[0]=PFR
                    //cellStrings[1]=Uniprot Id
                    //cellStrings[2]=Accession
                    //cellStrings[3]=Description
                    //cellStrings[4]=Sequence
                    //cellStrings[5]=Start Index
                    //cellStrings[6]=End Index
                    //cellStrings[7]=Sequence Length
                    //cellStrings[8]=Modifications
                    //cellStrings[9]=Modification Codes
                    //cellStrings[10]=N Terminal Modification Code
                    //cellStrings[11]=C Terminal Modification Code
                    //cellStrings[12]=Monoisotopic Mass
                    //cellStrings[13]=Average Mass
                    //cellStrings[14]=File Name
                    //cellStrings[15]=Result Set
                    //cellStrings[16]=Observed Precursor Mass
                    //cellStrings[17]=ScanIndex
                    //cellStrings[18]=RetentionTime
                    //cellStrings[19]=Global Q-value
                    //cellStrings[20]=P-score
                    //cellStrings[21]=E-value
                    //cellStrings[22]=C-score
                    //cellStrings[23]=% Cleavages

                    if (add_topdown_hit)
                    {
                        TopDownHit td_hit = new TopDownHit(aaIsotopeMassList, file, tdResultType, cellStrings[2], cellStrings[0], cellStrings[1], cellStrings[3], cellStrings[4],
                        Convert.ToInt16(cellStrings[5]), Convert.ToInt16(cellStrings[6]), ptm_list, Convert.ToDouble(cellStrings[16]), Convert.ToDouble(cellStrings[12]),
                        Convert.ToInt16(cellStrings[17]), Convert.ToDouble(cellStrings[18]), cellStrings[14].Split('.')[0], Convert.ToDouble(cellStrings[20]), Convert.ToDouble(cellStrings[22]));
                        lock (td_hits) td_hits.Add(td_hit);
                    }
                }
            });
            return td_hits;
        }
    }
}