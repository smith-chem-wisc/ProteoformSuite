using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using UsefulProteomicsDatabases;
using Proteomics;
using MzIdentML;

namespace ProteoformSuiteInternal
{
    public class TdBuReader
    {

        //READING IN BOTTOM-UP MORPHEUS FILE
        public static List<Psm> ReadBUFile(string filename)
        {
            List<Psm> psm_list = new List<Psm>();
            var identifications = new MzidIdentifications(filename);
            for (int i = 0; i < identifications.Count; i++)
            {
                List<Ptm> modifications = new List<Ptm>();
                for (int p = 0; p < identifications.NumModifications(i); p++)
                {
                    ModificationWithMass mod = Lollipop.uniprotModificationTable.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.id == identifications.ModificationAcession(i, p)).FirstOrDefault();
                    if (mod != null) modifications.Add(new Ptm(identifications.ModificationLocation(i, p), mod));
                    else modifications.Add(new Ptm(identifications.ModificationLocation(i, p), new ModificationWithMass(identifications.ModificationAcession(i, p), null, null, ModificationSites.Any, 0, null, 0, null, null, null)));
                }
                psm_list.Add(new Psm(identifications.PeptideSequenceWithoutModifications(i), identifications.StartResidueInProtein(i), identifications.EndResidueInProtein(i), modifications, identifications.Ms2SpectrumID(i), identifications.ProteinAccession(i), identifications.ProteinFullName(i), identifications.ExperimentalMassToCharge(i), identifications.ChargeState(i), (identifications.ExperimentalMassToCharge(i) - identifications.CalculatedMassToCharge(i))));
            }
           return psm_list;
        }

        //Reading in Top-down excel
        public static List<TopDownHit> ReadTDFile(InputFile file)
        { 
                List<TopDownHit> td_hits = new List<TopDownHit>();
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(file.complete_path, false))
                {
                    // Get Data in Sheet1 of Excel file
                    IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>(); // Get all sheets in spread sheet document 
                    WorksheetPart worksheet_1 = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(sheetcollection.First().Id.Value); // Get sheet1 Part of Spread Sheet Document
                    SheetData sheet_1 = worksheet_1.Worksheet.Elements<SheetData>().First();
                    IEnumerable<Row> rowcollection = worksheet_1.Worksheet.Descendants<Row>().ToList();
                    foreach (Row row in rowcollection)
                    {
                        if (row.RowIndex.Value == 1) continue;
                        IEnumerable<Cell> cells = GetRowCells(row);
                        List<string> cellStrings = new List<string>();
                        foreach (Cell cell in cells)
                        {
                            cellStrings.Add(GetCellValue(spreadsheetDocument, cell));
                        }
                        //get ptms on proteoform
                        List<Ptm> ptm_list = new List<Ptm>();
                        //N-term modifications
                        if (cellStrings[10].Length > 0)
                        {
                            int position = 0;
                            if (cellStrings[10].Split(':')[1] == "1458")
                            {
                                ModificationWithMass mod = Lollipop.uniprotModificationTable.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.id.Contains("acetyl") && m.motif.Motif == cellStrings[4][0].ToString()).FirstOrDefault();
                                if (mod != null)
                                {
                                    ptm_list.Add(new Ptm(position, mod));
                                }
                                //found one case where PTM not in ptmlist.txt (acetylasparagine)
                                else
                                {
                                    ptm_list.Add(new Ptm(position, new ModificationWithMass("N-acetylation", null, null, ModificationSites.NTerminus, 0, null, -1, null, null, null)));
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
                                ModificationWithMass mod = Lollipop.uniprotModificationTable.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains(resid)).FirstOrDefault();
                                if (mod != null) ptm_list.Add(new Ptm(position, mod));
                            }
                        }

                        string[] full_filename = cellStrings[14].Split('.');

                        Result_Set result_set = new Result_Set();
                        if (cellStrings[15] == "Tight Absolute Mass") result_set = Result_Set.tight_absolute_mass;
                        else if (cellStrings[15] == "Find Unexpected Modifications") result_set = Result_Set.find_unexpected_mods;
                        else if (cellStrings[15] == "BioMarker") result_set = Result_Set.biomarker;
                            //convert into new td hit
                            TopDownHit td_hit = new TopDownHit(file, cellStrings[2], cellStrings[1], cellStrings[3], cellStrings[4],
                            Convert.ToInt16(cellStrings[5]), Convert.ToInt16(cellStrings[6]), ptm_list, Convert.ToDouble(cellStrings[16]), Convert.ToDouble(cellStrings[12]),
                            Convert.ToInt16(cellStrings[17]), Convert.ToDouble(cellStrings[18]), full_filename[0], result_set, file.targeted_td_result);
                            td_hits.Add(td_hit);
                    }
                }


                return td_hits;

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
        }

        //TD files have blank spaces in middle of rows/columns -- need this to account for those. 
        public static IEnumerable<Cell> GetRowCells(Row row)
        {
            int currentCount = 0;

            foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in
                row.Descendants<DocumentFormat.OpenXml.Spreadsheet.Cell>())
            {
                string columnName = GetColumnName(cell.CellReference);

                int currentColumnIndex = ConvertColumnNameToNumber(columnName);

                for (; currentCount < currentColumnIndex; currentCount++)
                {
                    yield return new DocumentFormat.OpenXml.Spreadsheet.Cell();
                }

                yield return cell;
                currentCount++;
            }
        }

        public static string GetColumnName(string cellReference)
        {
            // Match the column name portion of the cell name.
            var regex = new System.Text.RegularExpressions.Regex("[A-Za-z]+");
            var match = regex.Match(cellReference);

            return match.Value;
        }

        public static int ConvertColumnNameToNumber(string columnName)
        {
            var alpha = new System.Text.RegularExpressions.Regex("^[A-Z]+$");
            if (!alpha.IsMatch(columnName)) throw new ArgumentException();

            char[] colLetters = columnName.ToCharArray();
            Array.Reverse(colLetters);

            int convertedValue = 0;
            for (int i = 0; i < colLetters.Length; i++)
            {
                char letter = colLetters[i];
                int current = i == 0 ? letter - 65 : letter - 64; // ASCII 'A' = 65
                convertedValue += current * (int)Math.Pow(26, i);
            }

            return convertedValue;
        }

        //TD reader needs own getcellvalue to account for blank spaces in rows/columns
        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            try
            {
                string value = cell.CellValue.InnerXml;
                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && value != null)
                    return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                else
                    return value;
            }
            catch { return ""; }
        }
    }
}
