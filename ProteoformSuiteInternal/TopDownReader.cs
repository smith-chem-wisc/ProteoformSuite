using System;
using System.Collections.Generic;
using System.Linq;

using Proteomics;

namespace ProteoformSuiteInternal
{
    public class TopDownReader
    {

        #region Private Fields

        private Dictionary<char, double> aaIsotopeMassList;

        #endregion Private Fields

        public List<Ptm> topdown_ptms = new List<Ptm>(); //PTMs not in theoretical database added to warning file.
        //Reading in Top-down excel
        public List<TopDownHit> ReadTDFile(InputFile file)
        {
            aaIsotopeMassList = new AminoAcidMasses(SaveState.lollipop.carbamidomethylation, SaveState.lollipop.natural_lysine_isotope_abundance, SaveState.lollipop.neucode_light_lysine, SaveState.lollipop.neucode_heavy_lysine).AA_Masses;
            List<TopDownHit> td_hits = new List<TopDownHit>();

            List<List<string>> cells = ExcelReader.get_cell_strings(file, true);//This returns the entire sheet except for the header. Each row of cells is one List<string>
            //get ptms on proteoform -- check for mods. IF not in database, make new topdown mod, show Warning message. 
            foreach (List<string> cellStrings in cells)
            {
                TopDownResultType tdResultType = (cellStrings[15] == "BioMarker") ? TopDownResultType.Biomarker : TopDownResultType.TightAbsoluteMass;
                List<Ptm> ptm_list = new List<Ptm>();
                //N-term modifications
                if (cellStrings[10].Length > 0) //N Terminal Modification Code
                {
                    int position = 1;
                    if (cellStrings[10].Split(':')[1] == "1458")//PSI-MOD 1458 is supposed to be N-terminal acetylation
                    {
                        ModificationWithMass mod = SaveState.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.id.Contains("acetyl") && m.motif.Motif == cellStrings[4][0].ToString()).FirstOrDefault();
                        if (mod != null)
                        {
                            ptm_list.Add(new Ptm(position, mod));
                        }
                        else
                        {
                            ModificationMotif motif;
                            ModificationMotif.TryGetMotif(cellStrings[4][0].ToString(), out motif);
                            Ptm new_ptm = topdown_ptms.Where(m => m.modification.motif.Motif == motif.Motif && m.modification.id == "N-terminal acetylation").FirstOrDefault();//multiple modifications can be indicated in this cell (e.g. alpha-amino acetylated residue@N, O-phospho-L-serine@95)
                            if (new_ptm == null) //if not in topdown_ptms list, add it (will show in warning)
                            { 
                               topdown_ptms.Add(new Ptm(position, new ModificationWithMass("N-terminal acetylation", null, motif , ModificationSites.NTerminus, 0, null, new List<double>(), new List<double>(), null)));
                            }
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
                        ModificationWithMass mod = SaveState.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains(resid)).FirstOrDefault();
                        if (mod != null) ptm_list.Add(new Ptm(position, mod));
                        else
                        {
                            ModificationMotif motif;
                            ModificationMotif.TryGetMotif(cellStrings[4][position - 1].ToString(), out motif);
                            Ptm new_ptm = topdown_ptms.Where(m => m.modification.motif.Motif == motif.Motif && m.modification.id == resid).FirstOrDefault();
                            if (new_ptm == null) //if not in topdown_ptms list, add it (will show in warning)
                                topdown_ptms.Add(new Ptm(position, new ModificationWithMass(resid, null, motif, ModificationSites.NTerminus, 0, null, new List<double>(), new List<double>(), null)));
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

                TopDownHit td_hit = new TopDownHit(aaIsotopeMassList, file, tdResultType, cellStrings[2], cellStrings[1], cellStrings[3], cellStrings[4],
                Convert.ToInt16(cellStrings[5]), Convert.ToInt16(cellStrings[6]), ptm_list, Convert.ToDouble(cellStrings[16]), Convert.ToDouble(cellStrings[12]),
                Convert.ToInt16(cellStrings[17]), Convert.ToDouble(cellStrings[18]), cellStrings[14].Split('.')[0], file.targeted_td_result, Convert.ToDouble(cellStrings[22]));
                td_hits.Add(td_hit);
            }
            return td_hits;
        }
    }
}

                //PROSIGHT READIN
                //        else if (td_software == TDSoftware.ProSight)
                //        {
                //            string[] description = cellStrings[13].Split(';');
                //            string[] accession = description[0].Split(',');

                //            string file_sequence = cellStrings[6];
                //            file_sequence = file_sequence.Replace(")", "(");
                //            string[] split_sequence = file_sequence.Split('(');
                //            List<int> positions = new List<int>();
                //            string sequence = "";
                //            for (int j = 0; j < split_sequence.Length; j++)
                //            {
                //                try
                //                {
                //                    //if number, add position of PTM to list
                //                    int mod_id = Convert.ToInt16(split_sequence[j]);
                //                    positions.Add(sequence.Length);
                //                }
                //                catch { sequence += split_sequence[j]; }
                //            }

                //            List<Ptm> ptm_list = new List<Ptm>();
                //            string modification_description = cellStrings[8];
                //            modification_description = modification_description.Replace(", ", "; ");
                //            string[] modifications = modification_description.Split(';');
                //            if (modifications.Length > 1)
                //            {
                //                for (int j = 0; j < modifications.Length; j++)
                //                {
                //                    string[] new_modification = modifications[j].Split('(');
                //                    int position = positions[j];
                //                    Ptm ptm = new Ptm(position, new Modification(new_modification[0]));
                //                    ptm_list.Add(ptm);
                //                }
                //            }
                //            string[] full_filename = cellStrings[14].Split('.');

                //            Result_Set result_set = new Result_Set();
                //            if (cellStrings[3] == "absolute_mass") result_set = Result_Set.tight_absolute_mass;
                //            else if (cellStrings[3] == "biomarker") result_set = Result_Set.biomarker;

                //            TopDownHit td_proteoform = new TopDownHit(cellStrings[4], accession[0], description[1], sequence,
                //               0, 0, ptm_list, Convert.ToDouble(cellStrings[10]), Convert.ToDouble(cellStrings[9]), 0, 0, full_filename[0], Convert.ToDouble(cellStrings[20]), result_set);
                //            td_proteoforms.Add(td_proteoform);
                //        }
                //    }
                //}

 