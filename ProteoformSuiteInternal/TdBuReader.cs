using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ProteoformSuiteInternal
{
    public class TdBuReader
    {

        //READING IN BOTTOM-UP MORPHEUS FILE
        public static List<Psm> ReadBUFile(string filename)
        {
            List<Psm> psm_list = new List<Psm>();
            string[] lines = File.ReadAllLines(filename);

            int i = 1;
            bool qLessThan1 = true;
            //only add PSMs with q less than 1. this assumes the tsv is in increasing order of q-value! 
            while (qLessThan1)
            {
                string[] parts = lines[i].Split('\t');
                //only read in with Q-value < 1%
                if (Convert.ToDouble(parts[30]) < 1)
                {
                    if (Convert.ToBoolean(parts[26]))
                    {
                        Psm new_psm = new Psm(parts[11].ToString(), parts[0].ToString(), Convert.ToInt32(parts[14]), Convert.ToInt32(parts[15]),
                            Convert.ToDouble(parts[10]), Convert.ToDouble(parts[6]), Convert.ToDouble(parts[25]), Convert.ToInt32(parts[1]),
                            parts[13].ToString(), Convert.ToDouble(parts[5]), Convert.ToInt32(parts[7]), Convert.ToDouble(parts[18]));
                        psm_list.Add(new_psm);
                    }
                    i++;
                }
                else { qLessThan1 = false; }
            }
            return psm_list;
        }

        //Reading in Top-down excel
        public static List<TopDownHit> ReadTDFile(string filename, TDSoftware td_software)
        {
                List<TopDownHit> td_proteoforms = new List<TopDownHit>();
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filename, false))
                {
                    // Get Data in Sheet1 of Excel file
                    IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>(); // Get all sheets in spread sheet document 
                    WorksheetPart worksheet_1 = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(sheetcollection.First().Id.Value); // Get sheet1 Part of Spread Sheet Document
                    SheetData sheet_1 = worksheet_1.Worksheet.Elements<SheetData>().First();
                    List<Row> rowcollection = worksheet_1.Worksheet.Descendants<Row>().ToList();

                    for (int i = 1; i < rowcollection.Count; i++)   //skip first row (headers)
                    {
                        List<string> cellStrings = new List<string>();
                        for (int k = 0; k < rowcollection[i].Descendants<Cell>().Count(); k++)
                        {
                            if (ComponentReader.GetCellValue(spreadsheetDocument, rowcollection[i].Descendants<Cell>().ElementAt(k)) != null)
                                cellStrings.Add(ComponentReader.GetCellValue(spreadsheetDocument, rowcollection[i].Descendants<Cell>().ElementAt(k)));
                        }

                        //get ptms on proteoform
                        List<Ptm> ptm_list = new List<Ptm>();
                    //N-term modifications
                    if (cellStrings[10].Length > 0)
                    {
                        int position = 0;
                        if (cellStrings[10].Split(':')[1] == "1458")
                        {
                            try
                            {
                                Modification mod = Lollipop.uniprotModificationTable.Values.Where(m => m.ptm_category == "Acetylation" && m.target_aas.Contains(cellStrings[4][0])).ToList().First();
                                ptm_list.Add(new Ptm(position, mod));
                            }
                            //found one case where PTM not in ptmlist.txt (acetylasparagine)
                            catch
                            {
                                ptm_list.Add(new Ptm(position, new Modification("N-acetylation")));
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
                            Modification mod = Lollipop.uniprotModificationTable.Values.Where(m => m.resid == resid).First();
                            ptm_list.Add(new Ptm(position, mod));
                        }
                    }
                   
                        string[] full_filename = cellStrings[14].Split('.');

                        Result_Set result_set = new Result_Set();
                        if (cellStrings[15] == "Tight Absolute Mass") result_set = Result_Set.tight_absolute_mass;
                        else if (cellStrings[15] == "Find Unexpected Modifications") result_set = Result_Set.find_unexpected_mods;
                        else if (cellStrings[15] == "BioMarker") result_set = Result_Set.biomarker;

                        //convert into new td hit
                        TopDownHit td_proteoform = new TopDownHit(cellStrings[2], cellStrings[1], cellStrings[3], cellStrings[4],
                        Convert.ToInt16(cellStrings[5]), Convert.ToInt16(cellStrings[6]), ptm_list, Convert.ToDouble(cellStrings[16]), Convert.ToDouble(cellStrings[12]),
                        Convert.ToInt16(cellStrings[17]), Convert.ToDouble(cellStrings[18]), full_filename[0], Convert.ToDouble(cellStrings[23]), result_set);
                        td_proteoforms.Add(td_proteoform);

                    }
                }

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
                return td_proteoforms;
        }



        //reading in MS1 scan numbers for topdown deconovlution files
        //public static List<string> MS1_scans(string filename)
        //{
        //    if (Lollipop.td_results)

        //    { //find MS1 list corresponding to identification file (will only read in components in MS1 scans)
        //        List<InputFile> td_files = Lollipop.topdownMS1list_files().Where(t => t.filename == filename).ToList(); //checked in GUI for 1 matching file
        //        string[] td_file = File.ReadAllLines(td_files[0].path + "\\" + td_files[0].filename + td_files[0].extension);
        //        return new List<string>(td_file);
        //    }
        //    else return new List<string>();
        //}
    }
}
