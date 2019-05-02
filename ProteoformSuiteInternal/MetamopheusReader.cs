using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop;
using System.IO;
using Proteomics.ProteolyticDigestion;

namespace ProteoformSuiteInternal
{
    public class MetamopheusReader
    {
        #region Private Fields
        private Dictionary<char, double> aaIsotopeMassList;
        #endregion Private Fields

        //PTMs not in theoretical database added to warning file.
        public List<string> bad_topdown_ptms = new List<string>(); 

        //Reading in metamopheus excel
        public List<TopDownHit> ReadMetamopheusFile(InputFile file)
        {

            //if neucode labeled, calculate neucode light theoretical AND observed mass! --> better for matching up
            //if carbamidomethylated, add 57 to theoretical mass (already in observed mass...)
            aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, Sweet.lollipop.neucode_labeled).AA_Masses;
            List<TopDownHit> td_hits = new List<TopDownHit>();//for one line in excel file

            //creates dictionary to find mods
            Dictionary<string, Modification> mods = Sweet.lollipop.theoretical_database.all_mods_with_mass.ToDictionary(kv => kv.IdWithMotif, kv => kv);


            List<List<string>> cells = ExcelReader.get_cell_strings(file, true);//This returns the entire sheet except for the header. Each row of cells is one List<string>
            //get ptms on proteoform -- check for mods. IF not in database, make new topdown mod, show Warning message.
            Parallel.ForEach(cells, cellStrings =>
            {
                bool add_topdown_hit = true; //if PTM or accession not found, will not add (show warning)
                if (cellStrings.Count == 55)
                {
                    List<Ptm> new_ptm_list = new List<Ptm>();
                    //if bad mod itll catch it to add to bad_topdown_ptms
                    try
                    {
                        PeptideWithSetModifications modsIdentifier = new PeptideWithSetModifications(cellStrings[14].Split('|')[0], mods);

                        var ptm_list = modsIdentifier.AllModsOneIsNterminus;

                        //for each  entry in ptm_list make a new Ptm and add it to the new_ptm_list 
                        foreach (KeyValuePair<int, Proteomics.Modification> entry in ptm_list)
                        {
                            Modification mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).Where(m => m.IdWithMotif == entry.Value.IdWithMotif).FirstOrDefault();
                            var Ptm = new Ptm();

                            if (mod != null)
                            {
                                new_ptm_list.Add(new Ptm(entry.Key, entry.Value));
                                
                            }
                            else
                            {
                                lock (bad_topdown_ptms)
                                {
                                    //error is somewahre in sequece
                                    bad_topdown_ptms.Add("Mod Name:" + entry.Value.IdWithMotif + " at " + entry.Key);
                                    add_topdown_hit = false;
                                }
                                
                            }

                        }
                    }
                    catch (MzLibUtil.MzLibException)
                    {
                        lock (bad_topdown_ptms)
                        {
                            //error is somewahre in sequece
                            bad_topdown_ptms.Add("Bad mod at " + cellStrings[0] + " scan " + cellStrings[1]);
                            add_topdown_hit = false;
                        }
                    }

                    //This is the excel file header:
                    //cellStrings[0]=File Name
                    //cellStrings[1]=Scan Number
                    //cellStrings[2]=Scan Retention Time
                    //cellStrings[3]=Num Experimental Peaks
                    //cellStrings[4]=Total Ion Current
                    //cellStrings[5]=Precursor Scan Number
                    //cellStrings[6]=Precursor Charge
                    //cellStrings[7]=Precursor MZ
                    //cellStrings[8]=Precursor Mass
                    //cellStrings[9]=Score
                    //cellStrings[10]=Delta Score
                    //cellStrings[11]=Notch
                    //cellStrings[12]=Different Peak Matches
                    //cellStrings[13]=Base Sequence
                    //cellStrings[14]=Full Sequence
                    //cellStrings[15]=Essential Sequence
                    //cellStrings[16]=PSM Count
                    //cellStrings[17]=Mods
                    //cellStrings[18]=Mods Chemical Formulas
                    //cellStrings[19]=Mods Combined Chemical Formula
                    //cellStrings[20]=Num Variable Mods
                    //cellStrings[21]=Missed Cleavages
                    //cellStrings[22]=Peptide Monoisotopic Mass
                    //cellStrings[23]=Mass Diff (Da)
                    //cellStrings[24]=Mass Diff (ppm)
                    //cellStrings[25]=Protein Accession
                    //cellStrings[26]=Protein Name
                    //cellStrings[27]=Gene Name
                    //cellStrings[28]=Organism Name
                    //cellStrings[29]=Intersecting Sequence Variations
                    //cellStrings[30]=Identified Sequence Variations
                    //cellStrings[31]=Splice Sites
                    //cellStrings[32]=Contaminant
                    //cellStrings[33]=Decoy
                    //cellStrings[34]=Peptide Description
                    //cellStrings[35]=Start and End Residues In Protein
                    //cellStrings[36]=Previous Amino Acid
                    //cellStrings[37]=Next Amino Acid
                    //cellStrings[38]=All Scores
                    //cellStrings[39]=Theoreticals Searched
                    //cellStrings[40]=Decoy/Contaminant/Target
                    //cellStrings[41]=Matched Ion Series
                    //cellStrings[42]=Matched Ion Mass-To-Charge Ratios
                    //cellStrings[43]=Matched Ion Mass Diff (Da)
                    //cellStrings[44]=Matched Ion Mass Diff (Ppm)
                    //cellStrings[45]=Matched Ion Intensities
                    //cellStrings[46]=Matched Ion Counts
                    //cellStrings[47]=Localized Scores
                    //cellStrings[48]=Improvement Possible
                    //cellStrings[49]=Cumulative Target
                    //cellStrings[50]=Cumulative Decoy
                    //cellStrings[51]=QValue
                    //cellStrings[52]=Cumulative Target Notch
                    //cellStrings[53]=Cumulative Decoy Notch
                    //cellStrings[54]=QValue Notch
                    //cellStrings[55]=eValue
                    //cellStrings[56]=eScore     

                  

                    if (cellStrings[35].Length > 0)
                    {
                            string[] ids = cellStrings[35].Split('|');
                            //splits the string to get the value of starting index
                            string[] index = ids[0].Split(' ');

                            string[] startIndexValue = index[0].Split('[');
                            string startResidues = startIndexValue[1];

                            //splits string to get value of ending index
                            string[] endIndexValue = index[2].Split(']');
                            string endResidues = endIndexValue[0];
                            

                            if (add_topdown_hit)
                            {
                                //if bad mod u want td hit to be false
                                TopDownHit td_hit = new TopDownHit(aaIsotopeMassList, file, TopDownResultType.TightAbsoluteMass, cellStrings[25], cellStrings[14], cellStrings[25], cellStrings[26], cellStrings[13],
                                Int32.TryParse(startResidues, out int j) ? j : 0, Int32.TryParse(endResidues, out int i) ? i : 0, new_ptm_list, Double.TryParse(cellStrings[8], out double d) ? d : 0, Double.TryParse(cellStrings[22], out d) ? d : 0,
                                Int32.TryParse(cellStrings[1], out i) ? i : 0, Double.TryParse(cellStrings[2], out d) ? d : 0, cellStrings[0].Split('.')[0], Double.TryParse(cellStrings[8], out d) ? d : 0, Sweet.lollipop.min_score_td + 1);
                                

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
