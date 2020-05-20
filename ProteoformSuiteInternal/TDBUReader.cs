﻿using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Accord.Math;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;
using Proteomics.ProteolyticDigestion;
using UsefulProteomicsDatabases;

namespace ProteoformSuiteInternal
{
    public class TDBUReader
    {
        #region Private Fields

        private Dictionary<char, double> aaIsotopeMassList;

        #endregion Private Fields

        public List<string>
            bad_ptms = new List<string>(); //PTMs not in theoretical database added to warning file.

        //Reading in Top-down excel
        public List<SpectrumMatch> ReadTDFile(InputFile file)
        {
            if (file.extension == ".xlsx")
            {
                return TDPortalReader(file);
            }
            else if (file.extension == ".psmtsv")
            {
                return MetaMorpheusReader(file);

            }

            return new List<SpectrumMatch>();
        }

        private List<SpectrumMatch> TDPortalReader(InputFile file)
        {
            //if neucode labeled, calculate neucode light theoretical AND observed mass! --> better for matching up
            //if carbamidomethylated, add 57 to theoretical mass (already in observed mass...)
            aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, Sweet.lollipop.neucode_labeled)
                .AA_Masses;
            List<SpectrumMatch> td_hits = new List<SpectrumMatch>();
            List<List<string>>
                cells = ExcelReader.get_cell_strings(file,
                    true); //This returns the entire sheet except for the header. Each row of cells is one List<string>

            //get ptms on proteoform -- check for mods. IF not in database, make new topdown mod, show Warning message.
            Parallel.ForEach(cells, cellStrings =>
            {
                bool add_topdown_hit = true; //if PTM or accession not found, will not add (show warning)
                TopDownResultType tdResultType = (cellStrings[15] == "BioMarker")
                    ? TopDownResultType.Biomarker
                    : ((cellStrings[15] == "Tight Absolute Mass")
                        ? TopDownResultType.TightAbsoluteMass
                        : TopDownResultType.Unknown);
                if (tdResultType != TopDownResultType.Unknown) //uknown result type!
                {
                    List<Ptm>
                        ptm_list =
                            new List<Ptm>(); // if nothing gets added, an empty ptmlist is passed to the topdownhit constructor.
                    //N-term modifications
                    if (cellStrings[10].Length > 0) //N Terminal Modification Code
                    {
                        string[] ptms = cellStrings[10].Split('|');

                        //for bottom-up, don't read in ambiguous PSMs
                        if (file.purpose == Purpose.BottomUp && ptms.Length > 1)
                        {
                            add_topdown_hit = false;
                        }


                        foreach (string ptm in ptms)
                        {
                            int position = Int32.TryParse(cellStrings[5], out int i) ? i : 0;
                            if (position == 0)
                            {
                                add_topdown_hit = false;
                                continue;
                            }

                            if (cellStrings[10].Split(':')[1] == "1458"
                            ) //PSI-MOD 1458 is supposed to be N-terminal acetylation
                            {
                                ptm_list.Add(new Ptm(position,
                                    Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m)
                                        .Where(m => m.OriginalId == "N-terminal Acetyl").FirstOrDefault()));
                            }
                            else
                            {
                                string psimod =
                                    ptm.Split(':')[1]
                                        .Split('@')[0]; //The number after the @ is the position in the protein
                                while (psimod.Length < 5)
                                    psimod =
                                        "0" + psimod; //short part should be the accession number, which is an integer
                                Modification mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values
                                    .SelectMany(m => m).Where(m =>
                                        m.DatabaseReference != null && m.DatabaseReference.ContainsKey("PSI-MOD") &&
                                        m.DatabaseReference["PSI-MOD"].Contains(psimod)).FirstOrDefault();
                                if (mod == null)
                                {
                                    psimod = "MOD:" + psimod;
                                    mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values
                                        .SelectMany(m => m).Where(m =>
                                            m.DatabaseReference != null && m.DatabaseReference.ContainsKey("PSI-MOD") &&
                                            m.DatabaseReference["PSI-MOD"].Contains(psimod)).FirstOrDefault();
                                }

                                if (mod != null) ptm_list.Add(new Ptm(position, mod));
                                else
                                {
                                    lock (bad_ptms)
                                    {
                                        bad_ptms.Add("PSI-MOD:" + psimod + " at " + position);
                                    }

                                    add_topdown_hit = false;
                                }
                            }
                        }
                    }

                    //don't have example of c-term modification to write code
                    //other mods
                    if (cellStrings[9].Length > 0) //Modification Codes
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

                            int position_after_begin =
                                (Int32.TryParse(ptm.Split(':')[1].Split('@')[1], out int j) ? j : -1) +
                                1; //one based sequence
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
                                string resid =
                                    ptm.Split(':')[1]
                                        .Split('@')[0]; //The number after the @ is the position in the protein
                                while (resid.Length < 4)
                                    resid =
                                        "0" + resid; //short part should be the accession number, which is an integer
                                resid = "AA" + resid;
                                id = "RESID:" + resid;
                                mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m)
                                    .Where(m => m.DatabaseReference != null &&
                                                m.DatabaseReference.ContainsKey("RESID") &&
                                                m.DatabaseReference["RESID"].Contains(resid)).FirstOrDefault();
                            }
                            else if (ptm.Split(':')[0] == "PSI-MOD")
                            {
                                string psimod =
                                    ptm.Split(':')[1]
                                        .Split('@')[0]; //The number after the @ is the position in the protein
                                while (psimod.Length < 5)
                                    psimod =
                                        "0" + psimod; //short part should be the accession number, which is an integer
                                mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m)
                                    .Where(m => m.DatabaseReference != null &&
                                                m.DatabaseReference.ContainsKey("PSI-MOD") &&
                                                m.DatabaseReference["PSI-MOD"].Contains(psimod)).FirstOrDefault();
                                if (mod == null)
                                {
                                    psimod = "MOD:" + psimod;
                                    mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values
                                        .SelectMany(m => m).Where(m =>
                                            m.DatabaseReference != null && m.DatabaseReference.ContainsKey("PSI-MOD") &&
                                            m.DatabaseReference["PSI-MOD"].Contains(psimod)).FirstOrDefault();
                                }

                                id = "PSI-MOD:" + psimod;
                            }

                            if (mod != null)
                            {
                                ptm_list.Add(new Ptm(position, mod));
                            }
                            else
                            {
                                lock (bad_ptms)
                                {
                                    bad_ptms.Add(id + " at " + cellStrings[4][position_after_begin - 1]);
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
                        SpectrumMatch td_hit = new SpectrumMatch(aaIsotopeMassList, file, tdResultType, cellStrings[2],
                            cellStrings[0], cellStrings[1], cellStrings[3], cellStrings[4],
                            Int32.TryParse(cellStrings[5], out int i) ? i : 0,
                            Int32.TryParse(cellStrings[6], out i) ? i : 0, ptm_list,
                            Double.TryParse(cellStrings[16], out double d) ? d : 0,
                            Double.TryParse(cellStrings[12], out d) ? d : 0,
                            Int32.TryParse(cellStrings[17], out i) ? i : 0,
                            Double.TryParse(cellStrings[18], out d) ? d : 0, cellStrings[14].Split('.')[0],
                            Double.TryParse(cellStrings[20], out d) ? d : 0,
                            Double.TryParse(cellStrings[22], out d) ? d : 0);

                        if (td_hit.begin > 0 && td_hit.end > 0 && td_hit.theoretical_mass > 0 && td_hit.pscore > 0 &&
                            td_hit.reported_mass > 0 && td_hit.score > 0
                            && td_hit.ms2ScanNumber > 0 && td_hit.ms2_retention_time > 0 && (td_hit.end - td_hit.begin + 1 == td_hit.sequence.Length))
                        {
                            lock (td_hits) td_hits.Add(td_hit);
                        }
                    }
                }
            });
            return td_hits;
        }

        private List<SpectrumMatch> MetaMorpheusReader(InputFile file)
        {

            //if neucode labeled, calculate neucode light theoretical AND observed mass! --> better for matching up
            //if carbamidomethylated, add 57 to theoretical mass (already in observed mass...)
            aaIsotopeMassList = new AminoAcidMasses(false, Sweet.lollipop.neucode_labeled)
                .AA_Masses;
            List<SpectrumMatch> td_hits = new List<SpectrumMatch>(); //for one line in excel file

 
            string[] cells = Enumerable.ToArray(System.IO.File.ReadAllLines(file.complete_path));
            string[] header = cells[0].Split('\t');

            int index_q_value = header.IndexOf("QValue");
            int index_decoy = header.IndexOf("Decoy");
            int index_full_sequence = header.IndexOf("Full Sequence");
            int index_filename = header.IndexOf("File Name");
            int index_scan_number = header.IndexOf("Scan Number");
            int index_begin_end = header.IndexOf("Start and End Residues In Protein");
            int index_protein_accession = header.IndexOf("Protein Accession");
            int index_protein_name = header.IndexOf("Protein Name");
            int index_base_sequence = header.IndexOf("Base Sequence");
            int index_retention_time = header.IndexOf("Scan Retention Time");
            int index_precursor_mass = header.IndexOf("Precursor Mass");
            int index_peptide_monoisotopic_mass = header.IndexOf("Peptide Monoisotopic Mass");
            bool glycan = header.Contains("GlycanIDs");

            //creates dictionary to find mods
            Dictionary<string, Modification> mods = new Dictionary<string, Modification>();
            foreach (var mod in Sweet.lollipop.theoretical_database.all_mods_with_mass)
            {
                if (!mods.ContainsKey(mod.IdWithMotif))
                {
                    mods.Add(mod.IdWithMotif, mod);
                }
            }

            if (glycan)
            {
                foreach (var mod in Sweet.lollipop.theoretical_database.glycan_mods)
                {
                    if (!mods.ContainsKey(mod.IdWithMotif))
                    {
                        mods.Add(mod.IdWithMotif, mod);
                    }
                }
            }

            Parallel.ForEach(cells.Skip(1), row =>
            {
                var cellStrings = row.Split('\t').ToList();
                bool add_topdown_hit = true; //if PTM or accession not found, will not add (show warning)
                double qValue = Convert.ToDouble(cellStrings[index_q_value].Split('|')[0]);
                string decoy = cellStrings[index_decoy].Split('|')[0];
                if (qValue < 0.01 && decoy == "N")
                {
                    List<int> begin = new List<int>();
                    List<int> end = new List<int>();
                    var start_and_end_residue_array = cellStrings[index_begin_end].Split('|');
                    //splits the string to get the value of starting index
                    foreach (var x in start_and_end_residue_array)
                    {
                        string[] index = x.Trim(new char[] {'[', ']'}).Split(' ');
                        begin.Add(Int32.TryParse(index[0], out int j) ? j : 0);
                        end.Add(Int32.TryParse(index[2], out int i) ? i : 0);
                    }

                    List<List<Ptm>> new_ptm_list = new List<List<Ptm>>();
                    var full_sequences = cellStrings[index_full_sequence].Split('|');
                    for(int i = 0; i < full_sequences.Length; i++)
                    {
                        //if bad mod itll catch it to add to bad_topdown_ptms
                        try
                        {
                            List<Ptm> list = new List<Ptm>();
                            PeptideWithSetModifications modsIdentifier =
                                new PeptideWithSetModifications(full_sequences[i], mods);

                            var ptm_list = modsIdentifier.AllModsOneIsNterminus;

                            //for each  entry in ptm_list make a new Ptm and add it to the new_ptm_list 
                            foreach (KeyValuePair<int, Proteomics.Modification> entry in ptm_list)
                            {
                                if (glycan && entry.Value.ModificationType == "N-Glycosylation")
                                {
                                    string glycanFormula = entry.Value.OriginalId;
                                    int H = Convert.ToInt32(glycanFormula.Substring(glycanFormula.IndexOf('H') + 1,
                                        glycanFormula.IndexOf('N') - glycanFormula.IndexOf('H') - 1));
                                    int N = Convert.ToInt32(glycanFormula.Substring(glycanFormula.IndexOf('N') + 1,
                                        glycanFormula.IndexOf('A') - glycanFormula.IndexOf('N') - 1));
                                    int A = Convert.ToInt32(glycanFormula.Substring(glycanFormula.IndexOf('A') + 1,
                                        glycanFormula.IndexOf('G') - glycanFormula.IndexOf('A') - 1));
                                    int G = Convert.ToInt32(glycanFormula.Substring(glycanFormula.IndexOf('G') + 1,
                                        glycanFormula.IndexOf('F') - glycanFormula.IndexOf('G') - 1));
                                    int F = Convert.ToInt32(glycanFormula.Substring(glycanFormula.IndexOf('F') + 1,
                                        glycanFormula.Length - glycanFormula.IndexOf('F') - 1));

                                    int new_ptm_list_index = new_ptm_list.Count > i ? i : 0; 
                                    var H_added = add_glycans(H, "H",
                                        entry.Key + begin.Count > i ? begin[i] : begin[0] - (entry.Key == 1 ? 1 : 2), list);
                                    var N_added = add_glycans(N, "N",
                                        entry.Key + begin.Count > i ? begin[i] : begin[0] - (entry.Key == 1 ? 1 : 2), list);
                                    var A_added = add_glycans(A, "A",
                                        entry.Key + begin.Count > i ? begin[i] : begin[0] - (entry.Key == 1 ? 1 : 2), list);
                                    var G_added = add_glycans(G, "G",
                                        entry.Key + begin.Count > i ? begin[i] : begin[0] - (entry.Key == 1 ? 1 : 2), list);
                                    var F_added = add_glycans(F, "F",
                                        entry.Key + begin.Count > i ? begin[i] : begin[0] - (entry.Key == 1 ? 1 : 2), list);

                                    add_topdown_hit = H_added && N_added && A_added && G_added && F_added;
                                }
                                else
                                {
                                    Modification mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values
                                        .SelectMany(m => m).Where(m => m.IdWithMotif == entry.Value.IdWithMotif)
                                        .FirstOrDefault();
                                    if (mod != null)
                                    {

                                        list.Add(new Ptm(entry.Key + (begin.Count > i ? begin[i] : begin[0]) - (entry.Key == 1 ? 1 : 2), entry.Value));
                                    }
                                    else
                                    {
                                        lock (bad_ptms)
                                        {
                                            //error is somewahre in sequece
                                            bad_ptms.Add(
                                                "Mod Name:" + entry.Value.IdWithMotif + " at " + entry.Key);
                                            add_topdown_hit = false;
                                        }
                                    }
                                }
                            }

                            new_ptm_list.Add(list);
                        }
                        catch (MzLibUtil.MzLibException)
                        {
                            lock (bad_ptms)
                            {
                                //error is somewahre in sequece
                                bad_ptms.Add("Bad mod at " + cellStrings[index_filename] + " scan " +
                                             cellStrings[index_scan_number]);
                                add_topdown_hit = false;
                            }
                        }
                    }
                        if (add_topdown_hit)
                        {
                            List<SpectrumMatch> ambiguious_hits = new List<SpectrumMatch>();
                            SpectrumMatch hit_to_add = null;
                            List<string> accessions = cellStrings[index_protein_accession].Split('|').ToList();
                            List<string> names = cellStrings[index_protein_name].Split('|').ToList();
                            List<string> base_sequences = cellStrings[index_base_sequence].Split('|').ToList();
                            List<double> theoretical_masses = cellStrings[index_peptide_monoisotopic_mass].Split('|')
                                .Select(a => Double.TryParse(a, out var d) ? d : 0).ToList();
                            List<int> counts = new List<int>()
                            {
                                accessions.Count, names.Count, base_sequences.Count, theoretical_masses.Count,
                                new_ptm_list.Count, begin.Count, end.Count
                            };

                            for (int hit = 0; hit < counts.Max(); hit++)
                            {
                                SpectrumMatch td_hit = new SpectrumMatch(aaIsotopeMassList, file,
                                    TopDownResultType.TightAbsoluteMass,
                                    accessions.Count > hit ? accessions[hit] : accessions[0],
                                    full_sequences.Length > hit ? full_sequences[hit] : full_sequences[0],
                                    accessions.Count > hit ? accessions[hit] : accessions[0],
                                    names.Count > hit ? names[hit] : names[0],
                                    base_sequences.Count > hit ? base_sequences[hit] : base_sequences[0],
                                    begin.Count > hit ? begin[0] : begin[0], end.Count > hit ? end[0] : end[0],
                                    new_ptm_list.Count > hit ? new_ptm_list[hit] : new_ptm_list.Count > 0 ?  new_ptm_list[0] : new List<Ptm>(),
                                    Double.TryParse(cellStrings[index_precursor_mass], out double m) ? m : 0,
                                    theoretical_masses.Count > hit ? theoretical_masses[hit] : theoretical_masses[0],
                                    Int32.TryParse(cellStrings[index_scan_number], out int i) ? i : 0,
                                    Double.TryParse(cellStrings[index_retention_time], out m) ? m : 0,
                                    cellStrings[index_filename].Split('.')[0],
                                    Double.TryParse(cellStrings[index_precursor_mass], out m) ? m : 0,
                                    Sweet.lollipop.min_score_td + 1);

                                if (td_hit.begin > 0 && td_hit.end > 0 && td_hit.theoretical_mass > 0 &&
                                    td_hit.pscore > 0 && td_hit.reported_mass > 0 && td_hit.score > 0
                                    && td_hit.ms2ScanNumber > 0 && td_hit.ms2_retention_time > 0)
                                {
                                    if (hit == 0)
                                    {
                                        hit_to_add = td_hit;
                                    }
                                    else
                                    {
                                        ambiguious_hits.Add(td_hit);
                                    }
                                }
                            }

                            if (hit_to_add != null)
                            {
                                hit_to_add.ambiguous_matches = ambiguious_hits;
                                lock (td_hits) td_hits.Add(hit_to_add);
                            }
                        }
                    
                }
            });
            return td_hits;
        }

        private bool add_glycans(int num_to_add, string glycan_id, int location, List<Ptm> ptm_list)
        {
            var mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values
                .SelectMany(m => m).Where(m => m.OriginalId == glycan_id)
                .FirstOrDefault();
            if (mod == null)
            {
                bad_ptms.Add(glycan_id);
                return false;
            }
            else
            {
                for (int count = 0; count < num_to_add; count++)
                {
                    ptm_list.Add(new Ptm(location, mod));
                }
            }

            return true;
        }
    }
}
