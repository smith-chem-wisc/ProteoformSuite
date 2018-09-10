using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            //if neucode labeled, calculate neucode light theoretical AND observed mass! --> better for matching up
            //if carbamidomethylated, add 57 to theoretical mass (already in observed mass...)
            aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, !Sweet.lollipop.neucode_labeled, Sweet.lollipop.neucode_labeled, false).AA_Masses;
            List<TopDownHit> td_hits = new List<TopDownHit>();

            List<List<string>> cells = ExcelReader.get_cell_strings(file, true);//This returns the entire sheet except for the header. Each row of cells is one List<string>
            //get ptms on proteoform -- check for mods. IF not in database, make new topdown mod, show Warning message.
            Parallel.ForEach(cells, cellStrings =>
            {
                bool add_topdown_hit = true; //if PTM or accession not found, will not add (show warning)
                if (cellStrings.Count == 24)
                {
                    TopDownResultType tdResultType = (cellStrings[15] == "BioMarker") ? TopDownResultType.Biomarker : ((cellStrings[15] == "Tight Absolute Mass") ? TopDownResultType.TightAbsoluteMass : TopDownResultType.Unknown);
                    if (tdResultType != TopDownResultType.Unknown) //uknown result type!
                    {
                        List<Ptm> ptm_list = new List<Ptm>(); // if nothing gets added, an empty ptmlist is passed to the topdownhit constructor.
                                                              //N-term modifications
                        if (cellStrings[10].Length > 0) //N Terminal Modification Code
                        {
                            string[] ptms = cellStrings[10].Split('|');
                            foreach (string ptm in ptms)
                            {
                                int position = Int32.TryParse(cellStrings[5], out int i) ? i : 0;
                                if (position == 0)
                                {
                                    add_topdown_hit = false;
                                    continue;
                                }
                                if (cellStrings[10].Split(':')[1] == "1458")//PSI-MOD 1458 is supposed to be N-terminal acetylation
                                {
                                    ptm_list.Add(new Ptm(position, Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).Where(m => m.OriginalId == "N-terminal Acetyl").FirstOrDefault()));
                                }
                                else
                                {
                                    string psimod = ptm.Split(':')[1].Split('@')[0];//The number after the @ is the position in the protein
                                    while (psimod.Length < 5) psimod = "0" + psimod;//short part should be the accession number, which is an integer
                                    Modification mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).Where(m => m.DatabaseReference != null && m.DatabaseReference.ContainsKey("PSI-MOD") && m.DatabaseReference["PSI-MOD"].Contains(psimod)).FirstOrDefault();
                                    if (mod == null)
                                    {
                                        psimod = "MOD:" + psimod;
                                        mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).Where(m => m.DatabaseReference != null && m.DatabaseReference.ContainsKey("PSI-MOD") && m.DatabaseReference["PSI-MOD"].Contains(psimod)).FirstOrDefault();
                                    }
                                    if (mod != null) ptm_list.Add(new Ptm(position, mod));
                                    else
                                    {
                                        lock (topdown_ptms)
                                        {
                                            topdown_ptms.Add("PSI-MOD:" + psimod + " at " + position);
                                        }
                                        add_topdown_hit = false;
                                    }
                                }
                            }
                        }
                        //don't have example of c-term modification to write code
                        //other mods
                        if (cellStrings[9].Length > 0)//Modification Codes
                        {
                            string[] ptms = cellStrings[9].Split('|');
                            foreach (string ptm in ptms)
                            {
                                Modification mod = null;
                                string id = "";
                                if (ptm.Split(':').Length < 2)
                                {
                                    add_topdown_hit = false;
                                    continue;
                                }
                                if (ptm.Split(':')[1].Split('@').Length < 2)
                                {
                                    add_topdown_hit = false;
                                    continue;
                                }
                                int position_after_begin = (Int32.TryParse(ptm.Split(':')[1].Split('@')[1], out int j) ? j : -1) + 1; //one based sequence
                                                                                                                                      //they give position # as from begin site -> want to report in terms of overall sequence #'s
                                                                                                                                      //begin + position from begin - 1 => position in overall sequence
                                if (position_after_begin == 0)
                                {
                                    add_topdown_hit = false;
                                    continue;
                                }
                                int begin = Int32.TryParse(cellStrings[5], out int k) ? k : 0;
                                if (begin == 0)
                                {
                                    add_topdown_hit = false;
                                    continue;
                                }
                                int position = begin + position_after_begin - 1;
                                if (ptm.Split(':')[0] == "RESID")
                                {
                                    string resid = ptm.Split(':')[1].Split('@')[0];//The number after the @ is the position in the protein
                                    while (resid.Length < 4) resid = "0" + resid;//short part should be the accession number, which is an integer
                                    resid = "AA" + resid;
                                    id = "RESID:" + resid;
                                    mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).Where(m => m.DatabaseReference != null && m.DatabaseReference.ContainsKey("RESID") && m.DatabaseReference["RESID"].Contains(resid)).FirstOrDefault();
                                }
                                else if (ptm.Split(':')[0] == "PSI-MOD")
                                {
                                    string psimod = ptm.Split(':')[1].Split('@')[0];//The number after the @ is the position in the protein
                                    while (psimod.Length < 5) psimod = "0" + psimod;//short part should be the accession number, which is an integer
                                    mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).Where(m => m.DatabaseReference != null && m.DatabaseReference.ContainsKey("PSI-MOD") && m.DatabaseReference["PSI-MOD"].Contains(psimod)).FirstOrDefault();
                                    if (mod == null)
                                    {
                                        psimod = "MOD:" + psimod;
                                        mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).Where(m => m.DatabaseReference != null && m.DatabaseReference.ContainsKey("PSI-MOD") && m.DatabaseReference["PSI-MOD"].Contains(psimod)).FirstOrDefault();
                                    }
                                    id = "PSI-MOD:" + psimod;
                                }
                                if (mod != null)
                                {
                                    ptm_list.Add(new Ptm(position, mod));
                                }
                                else
                                {
                                    lock (topdown_ptms)
                                    {
                                        topdown_ptms.Add(id + " at " + cellStrings[4][position_after_begin - 1]);
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
                            Int32.TryParse(cellStrings[5], out int i) ? i : 0, Int32.TryParse(cellStrings[6], out i) ? i : 0, ptm_list, Double.TryParse(cellStrings[16], out double d) ? d : 0, Double.TryParse(cellStrings[12], out d) ? d : 0,
                            Int32.TryParse(cellStrings[17], out i) ? i : 0, Double.TryParse(cellStrings[18], out d) ? d : 0, cellStrings[14].Split('.')[0], Double.TryParse(cellStrings[20], out d) ? d : 0, Double.TryParse(cellStrings[22], out d) ? d : 0);

                            if (td_hit.begin > 0 && td_hit.end > 0 && td_hit.theoretical_mass > 0 && td_hit.pscore > 0 && td_hit.reported_mass > 0 && td_hit.score > 0
                            && td_hit.ms2ScanNumber > 0 && td_hit.ms2_retention_time > 0)
                            {
                                lock (td_hits) td_hits.Add(td_hit);
                            }
                        }
                    }
                }
            });
            return td_hits;
        }
    }
}