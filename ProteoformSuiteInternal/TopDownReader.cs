using System;
using System.Collections.Generic;
using System.Linq;

using Proteomics;

namespace ProteoformSuiteInternal
{
    public class TopDownReader
    {
        //Reading in Top-down excel
        public static List<TopDownHit> ReadTDFile(InputFile file)
        {
            List<TopDownHit> td_hits = new List<TopDownHit>();

            List<List<string>> cells = ExcelReader.get_cell_strings(file, true);
            //get ptms on proteoform
            foreach (List<string> cellStrings in cells)
            {
                TopDownResultType tdResultType = (cellStrings[15] == "BioMarker") ? TopDownResultType.Biomarker : TopDownResultType.TightAbsoluteMass;
                List<Ptm> ptm_list = new List<Ptm>();
                //N-term modifications
                if (cellStrings[10].Length > 0)
                {
                    int position = 0;
                    if (cellStrings[10].Split(':')[1] == "1458")
                    {
                        ModificationWithMass mod = Lollipop.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.id.Contains("acetyl") && m.motif.Motif == cellStrings[4][0].ToString()).FirstOrDefault();
                        if (mod != null)
                        {
                            ptm_list.Add(new Ptm(position, mod));
                        }
                        //found one case where PTM not in ptmlist.txt (acetylasparagine)
                        else
                        {
                            ptm_list.Add(new Ptm(position, new ModificationWithMass("N-acetylation", null, null, ModificationSites.NTerminus, 0, null, new List<double>(), new List<double>(), null)));
                        }
                    }
                }
                //don't have example of c-term modification to write code
                //other mods
                if (cellStrings[9].Length > 0)
                {
                    string[] res_ids = cellStrings[9].Split('|');
                    foreach (string new_ptm in res_ids)
                    {
                        string resid = new_ptm.Split(':')[1].Split('@')[0];
                        while (resid.Length < 4) resid = "0" + resid;
                        resid = "AA" + resid;
                        int position = Convert.ToInt16(new_ptm.Split(':')[1].Split('@')[1]);
                        ModificationWithMass mod = Lollipop.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains(resid)).FirstOrDefault();
                        if (mod != null) ptm_list.Add(new Ptm(position, mod));
                    }
                }
                //convert into new td hit
                TopDownHit td_hit = new TopDownHit(file, tdResultType, cellStrings[2], cellStrings[1], cellStrings[3], cellStrings[4],
                Convert.ToInt16(cellStrings[5]), Convert.ToInt16(cellStrings[6]), ptm_list, Convert.ToDouble(cellStrings[16]), Convert.ToDouble(cellStrings[12]),
                Convert.ToInt16(cellStrings[17]), Convert.ToDouble(cellStrings[18]), cellStrings[14].Split('.')[0], file.targeted_td_result);
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

 