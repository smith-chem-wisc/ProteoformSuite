using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Threading;

namespace ProteoformSuiteInternal
{
    //to read in or save results for each module
    public class Results
    {
        static Object lockThread = new object(); //used so that parallel for loops can be used to add to list without addint @ same time

        // RAW COMPONENT I/O
        // Note: the parallel for loops do not preserve ordering
        public static void read_raw_components(string[] lines)
        {
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                Component component = new Component();
                component.id = (line[0]).ToString();
                component.id = line[0].ToString();
                component.monoisotopic_mass = Convert.ToDouble(line[1]);
                component.weighted_monoisotopic_mass = Convert.ToDouble(line[2]);
                component.corrected_mass = Convert.ToDouble(line[3]);
                component.intensity_sum = Convert.ToDouble(line[4]);
                component.intensity_reported = Convert.ToDouble(line[4]);
                component.num_charge_states_fromFile = Convert.ToInt16(line[5]);
                component.delta_mass = Convert.ToDouble(line[6]);
                component.relative_abundance = Convert.ToDouble(line[7]);
                component.fract_abundance = Convert.ToDouble(line[8]);
                component.scan_range = line[9];
                component.rt_range = line[10];
                component.rt_apex = Convert.ToDouble(line[11]);
                if (Lollipop.neucode_labeled)
                {
                    component.intensity_sum_olcs = Convert.ToDouble(line[12]);
                    component.input_file = Lollipop.input_files.Where(s => s.UniqueId == Convert.ToInt32(line[13])).ToList().First();
                    component.accepted = Convert.ToBoolean(line[14]);
                }
                else
                {
                    component.intensity_sum_olcs = Convert.ToDouble(line[4]);
                    component.input_file = Lollipop.input_files.Where(s => s.UniqueId == Convert.ToInt32(line[12])).ToList().First();
                    component.accepted = Convert.ToBoolean(line[13]);
                }


                lock (lockThread) { Lollipop.raw_experimental_components.Add(component); }
            });
        }

        public static string raw_component_results()
        {
            string tsv_header;
            if (Lollipop.neucode_labeled)
                tsv_header = String.Join("\t", new List<string> { "id", "monoisotopic_mass", "weighted_monoisotopic_mass", "corrected_mass", "intensity_sum", "num_charge_states",
                        "delta_mass", "relative_abundance", "fract_abundance", "scan_range", "rt_range", "rt_apex", "intensity_sum_olcs", "input_file_ID", "accepted" });
            else
                tsv_header = String.Join("\t", new List<string> { "id", "monoisotopic_mass", "weighted_monoisotopic_mass", "corrected_mass", "intensity_sum", "num_charge_states",
                        "delta_mass", "relative_abundance", "fract_abundance", "scan_range", "rt_range", "rt_apex", "input_file_ID", "accepted" });
            string results_rows = String.Join(Environment.NewLine, Lollipop.raw_experimental_components.Select(c => component_as_tsv_row(c)));
            return tsv_header + Environment.NewLine + results_rows;
        }

        private static string component_as_tsv_row(Component c)
        {
            if (Lollipop.neucode_labeled)
                return String.Join("\t", new List<string> { c.id.ToString(), c.monoisotopic_mass.ToString(), c.weighted_monoisotopic_mass.ToString(), c. corrected_mass.ToString(),
                    c.intensity_sum.ToString(), c.num_charge_states.ToString(),
                    c.delta_mass.ToString(), c.relative_abundance.ToString(), c.fract_abundance.ToString(), c.scan_range.ToString(), c.rt_range.ToString(),
                    c.rt_apex.ToString(), c.intensity_sum_olcs.ToString(), c.input_file.UniqueId.ToString(), c.accepted.ToString() });
            else
                return String.Join("\t", new List<string> { c.id.ToString(), c.monoisotopic_mass.ToString(), c.weighted_monoisotopic_mass.ToString(), c.corrected_mass.ToString(),
                    c.intensity_sum.ToString(), c.num_charge_states.ToString(),
                    c.delta_mass.ToString(), c.relative_abundance.ToString(), c.fract_abundance.ToString(), c.scan_range.ToString(), c.rt_range.ToString(),
                    c.rt_apex.ToString(), c.input_file.UniqueId.ToString(), c.accepted.ToString() });
        }

        //INPUT_FILE I/O
        public static void read_input_files(string[] lines)
        {
            Parallel.For(1, lines.Length, x =>
             {
                 string[] line = lines[x].Split('\t');
                 
                 int uniqueId = Convert.ToInt32(line[0]);
                 bool matchingCalibrationFile = Convert.ToBoolean(line[1]);
                 int biological_replicate = Convert.ToInt32(line[2]);
                 int fraction = Convert.ToInt32(line[3]);
                 int technical_replicate = Convert.ToInt32(line[4]);
                 string lt_condition = line[5];
                 string hv_condition = line[6];
                 string completePath = line[7] + line[8] + line[9];
                 Purpose purpose = (Purpose)Enum.Parse(typeof(Purpose), line[10]);
                 Labeling label = (Labeling)Enum.Parse(typeof(Labeling), line[11]);
                 InputFile inpfile = new InputFile(uniqueId, matchingCalibrationFile, biological_replicate, fraction, technical_replicate, lt_condition, hv_condition, completePath, label, purpose);

                 lock (lockThread) { Lollipop.input_files.Add(inpfile); }
             });
        }

        public static string input_file_results()
        {
            string tsv_header = String.Join("\t", new List<string> { "UniqueId", "matching_calibration_file", "biological_replicate", "fraction", "technical_replicate", "lt_condition",
                "hv_condition", "path", "filename", "extension", "purpose", "label" });
            string results_rows = String.Join(Environment.NewLine, Lollipop.input_files.Select(n => input_file_as_tsv_row(n)));
            return tsv_header + Environment.NewLine + results_rows;
        }

        private static string input_file_as_tsv_row(InputFile n)
        {
            return String.Join("\t", new List<string> { n.UniqueId.ToString(), n.matchingCalibrationFile.ToString(), n.biological_replicate.ToString(), n.fraction.ToString(), n.technical_replicate.ToString(), n.lt_condition.ToString(),
                    n.hv_condition.ToString(), n.path.ToString(), n.filename.ToString(), n.extension.ToString(), n.purpose.ToString(),
                    n.label.ToString() });
        }


        // RAW NEUCODE PAIR I/O
        public static void read_raw_neucode_pairs(string[] lines)
        {
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                Component neucode_light = Lollipop.raw_experimental_components.Where(c => c.id == (line[0]).ToString()).First();
                Component neucode_heavy = Lollipop.raw_experimental_components.Where(c => c.id == (line[5]).ToString()).First();
                NeuCodePair neucode_pair = new NeuCodePair(neucode_light, neucode_heavy);
                neucode_pair.intensity_ratio = Convert.ToDouble(line[8]);
                neucode_pair.lysine_count = Convert.ToInt32(line[9]);
                neucode_pair.input_file = Lollipop.input_files.Where(s => s.UniqueId == Convert.ToInt32(line[10])).ToList().First();

                if (neucode_pair.lysine_count > Lollipop.min_lysine_ct && neucode_pair.lysine_count < Lollipop.max_lysine_ct
                    && neucode_pair.intensity_ratio > Convert.ToDouble(Lollipop.min_intensity_ratio) && neucode_pair.intensity_ratio < Convert.ToDouble(Lollipop.max_intensity_ratio))
                { neucode_pair.accepted = true; }
                else { neucode_pair.accepted = false; }

                neucode_pair.corrected_mass = neucode_pair.corrected_mass + Math.Round((neucode_pair.lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * 1.0015;

                lock (lockThread) { Lollipop.raw_neucode_pairs.Add(neucode_pair); }
            });
        }

        public static string raw_neucode_pair_results()
        {
            string tsv_header = String.Join("\t", new List<string> { "light_id", "light_intensity_(overlapping_charge_states)", "light_weighted_monoisotopic_mass", "light_corrected_mass", "light_apexRt",
                "heavy_id", "heavy_intensity_(overlapping_charge_states)", "heavy_weighted_monoisotopic_mass", "intensity_ratio", "lysine_count", "inputFile_ID" });
            string results_rows = String.Join(Environment.NewLine, Lollipop.raw_neucode_pairs.Select(n => neucode_pair_as_tsv_row(n)));
            return tsv_header + Environment.NewLine + results_rows;
        }

        private static string neucode_pair_as_tsv_row(NeuCodePair n)
        {
            return String.Join("\t", new List<string> { n.id.ToString(), n.intensity_sum_olcs.ToString(), n.weighted_monoisotopic_mass.ToString(), n.corrected_mass.ToString(), n.rt_apex.ToString(),
                    n.neuCodeHeavy.id.ToString(), n.neuCodeHeavy.intensity_sum_olcs.ToString(), n.neuCodeHeavy.weighted_monoisotopic_mass.ToString(), n.intensity_ratio.ToString(), n.lysine_count.ToString(),
                    n.input_file.UniqueId.ToString() });
        }

        // AGGREGATED PROTEOFORM I/O
        public static void read_aggregated_proteoforms(string[] lines)
        {
            List<ExperimentalProteoform> experimental_proteoforms = new List<ExperimentalProteoform>();
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                ExperimentalProteoform aggregated_proteoform = new ExperimentalProteoform(line[0], Convert.ToDouble(line[1]), Convert.ToInt16(line[2]),
                    Convert.ToBoolean(line[3]));
                aggregated_proteoform.agg_mass = Convert.ToDouble(line[5]);
                aggregated_proteoform.agg_intensity = Convert.ToDouble(line[6]);
                aggregated_proteoform.agg_rt = Convert.ToDouble(line[7]);
                aggregated_proteoform.aggregated_components = (from id in line[9].Split(',') from c in Lollipop.raw_experimental_components where c.id == (id).ToString() select c).ToList();
                lock (lockThread) { experimental_proteoforms.Add(aggregated_proteoform); }
            });
            Lollipop.proteoform_community.experimental_proteoforms = experimental_proteoforms.ToArray();
        }

        public static string aggregated_experimental_proteoform_results()
        {
            string tsv_header = String.Join("\t", new List<string> { "accession", "modified_mass", "lysine_count", "is_target", "is_decoy",
                "agg_mass", "agg_intensity", "agg_rt", "observation_count", "component_list" });
            string results_rows = String.Join(Environment.NewLine, Lollipop.proteoform_community.experimental_proteoforms.Select(e => aggregated_experimental_proteoform_as_tsv_row(e)));
            return tsv_header + Environment.NewLine + results_rows;
        }

        private static string aggregated_experimental_proteoform_as_tsv_row(ExperimentalProteoform e)
        {
            return String.Join("\t", new List<string> { e.accession.ToString(), e.modified_mass.ToString(), e.lysine_count.ToString(), e.is_target.ToString(), e.is_decoy.ToString(),
                    e.agg_mass.ToString(), e.agg_intensity.ToString(), e.agg_rt.ToString(), e.observation_count.ToString(), String.Join(",", e.aggregated_components.Select(c => c.id)) });
        }

        // THEORETICAL PROTEOFORM I/O
        public static void read_theoretical_proteoforms(string[] lines, bool target)
        {
            string[] header = lines[0].Split('\t');
            List<TheoreticalProteoform> theoretical_proteoforms = new List<TheoreticalProteoform>();
            Dictionary<string, List<TheoreticalProteoform>> decoy_proteoforms = new Dictionary<string, List<TheoreticalProteoform>>();
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                if (line.Length == header.Length)
                {
                    PtmSet ptm_set;
                    List<GoTerm> goTerms = new List<GoTerm>();
                    List<Ptm> unmodified = new List<Ptm>();
                    if (line[11] != "unmodified") { ptm_set = new PtmSet(new List<Ptm>(from ptm_description in line[11].Split(';') select new Ptm(-1, Lollipop.uniprotModificationTable[ptm_description.Trim().TrimEnd(';')]))); }
                    else ptm_set = new PtmSet(unmodified);
                    TheoreticalProteoform theoretical_proteoform = new TheoreticalProteoform(line[0], line[5], line[6], line[7], Convert.ToInt32(line[8]), Convert.ToInt32(line[9]), Convert.ToDouble(line[10]), Convert.ToInt32(line[2]), goTerms, ptm_set, Convert.ToDouble(line[1]), Convert.ToBoolean(line[3]));
                    theoretical_proteoform.psm_count_BU = Convert.ToInt32(line[14]);
                    theoretical_proteoform.psm_count_TD = Convert.ToInt32(line[15]);
                    string database = line[13];
                    if (database == "Target") lock (lockThread)
                        theoretical_proteoforms.Add(theoretical_proteoform);
                    else lock (lockThread)
                    {
                        if (!decoy_proteoforms.ContainsKey(line[13])) decoy_proteoforms.Add(line[13], new List<TheoreticalProteoform>());
                        decoy_proteoforms[line[13]].Add(theoretical_proteoform);
                    }
                }
            });
            if (target) Lollipop.proteoform_community.theoretical_proteoforms = theoretical_proteoforms.ToArray();
            else Lollipop.proteoform_community.decoy_proteoforms = decoy_proteoforms.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
        }

        public static string theoretical_proteoforms_results(bool target)
        {
            string tsv_header = String.Join("\t", new List<string> { "accession", "modified_mass", "lysine_count", "is_target", "is_decoy",
                "description", "name", "fragment", "begin", "end", "unmodified_mass", "ptm_list", "ptm_mass", "database_name", "BU_psm_count", "TD_psm_count" });
            if (target)
            {
                return tsv_header + Environment.NewLine +
                    String.Join(Environment.NewLine, Lollipop.proteoform_community.theoretical_proteoforms.Select(t => theoretical_proteoform_as_tsv_row(t, "Target")));
            }
            else
            {
                string decoy_results = tsv_header;
                for (int decoyDatabaseNum = 0; decoyDatabaseNum < Lollipop.proteoform_community.decoy_proteoforms.Keys.Count; decoyDatabaseNum++)
                    decoy_results += Environment.NewLine + String.Join(Environment.NewLine, Lollipop.proteoform_community.decoy_proteoforms[Lollipop.decoy_database_name_prefix + decoyDatabaseNum].Select(p => theoretical_proteoform_as_tsv_row(p, Lollipop.decoy_database_name_prefix + decoyDatabaseNum)));
                return decoy_results;
            }
        }

        private static string theoretical_proteoform_as_tsv_row(TheoreticalProteoform t, string database)
        {
            return String.Join("\t", new List<string> { t.accession.ToString(), t.modified_mass.ToString(), t.lysine_count.ToString(), t.is_target.ToString(), t.is_decoy.ToString(),
                t.description.ToString(), t.name.ToString(), t.fragment.ToString(), t.begin.ToString(), t.end.ToString(), t.unmodified_mass.ToString(), t.ptm_descriptions.ToString(), t.ptm_mass.ToString(), database, t.psm_count_BU.ToString(), t.psm_count_TD.ToString() });
        }

        // PROTEOFORM RELATION I/O
        public static void read_relationships(string[] lines, ProteoformComparison relation_type)
        {
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                switch(relation_type)
                {
                    case ProteoformComparison.et:
                    {
                        ExperimentalProteoform experimental = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[0]))).First();
                        TheoreticalProteoform theoretical = Lollipop.proteoform_community.theoretical_proteoforms.Where(c => c.description.Equals(line[1])).First();
                        ProteoformRelation relation = new ProteoformRelation(experimental, theoretical, ProteoformComparison.et, Convert.ToDouble(line[2]));
                        relation.nearby_relations_count = Convert.ToInt16(line[3]);
                        lock (lockThread) { Lollipop.et_relations.Add(relation); }
                        break;
                    }
                    case ProteoformComparison.ed:
                    {
                        ExperimentalProteoform experimental = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[0]))).First();
                        string[] decoy_accession = line[1].Split('_'); //determine which decoy database
                        int decoyDatabaseNum = Convert.ToInt16(decoy_accession[5]);
                        TheoreticalProteoform decoy = Lollipop.proteoform_community.decoy_proteoforms[Lollipop.decoy_database_name_prefix + decoyDatabaseNum].Where(c => c.description.Equals((line[1]))).First();
                        ProteoformRelation relation = new ProteoformRelation(experimental, decoy, ProteoformComparison.ed, Convert.ToDouble(line[2]));
                        relation.nearby_relations_count = Convert.ToInt16(line[3]);
                        lock (lockThread)
                        {
                            if (!Lollipop.ed_relations.ContainsKey(Lollipop.decoy_database_name_prefix + decoyDatabaseNum))
                                Lollipop.ed_relations.Add(Lollipop.decoy_database_name_prefix + decoyDatabaseNum, new List<ProteoformRelation>());
                            Lollipop.ed_relations[Lollipop.decoy_database_name_prefix + decoyDatabaseNum].Add(relation);
                        }
                        break;
                    }
                    case ProteoformComparison.ee:
                    case ProteoformComparison.ef:
                    {
                        ExperimentalProteoform experimental_1 = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[0]))).First();
                        ExperimentalProteoform experimental_2 = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[1]))).First();
                        ProteoformRelation relation = new ProteoformRelation(experimental_1, experimental_2, relation_type, Convert.ToDouble(line[2]));
                        relation.nearby_relations_count = Convert.ToInt16(line[3]);
                        if (relation_type == ProteoformComparison.ef)
                            lock (lockThread) { Lollipop.ef_relations.Add(relation); }
                        else
                            lock (lockThread) { Lollipop.ee_relations.Add(relation); }
                        break;
                    }
                }
            });
        }

        public static string relation_results(ProteoformComparison relation_type)
        {
            string tsv_header = "";
            string results_rows = "";
            switch (relation_type)
            {
                case ProteoformComparison.et:
                    tsv_header = String.Join("\t", new List<string> { "proteoform1_accession", "proteoform2_description", "delta_mass", "nearby_relations" });
                    results_rows = String.Join(Environment.NewLine, Lollipop.et_relations.Select(r => relation_as_tsv_row(r, relation_type)));
                    break;
                case ProteoformComparison.ed:
                    tsv_header = String.Join("\t", new List<string> { "proteoform1_accession", "proteoform2_description", "delta_mass", "nearby_relations" });
                    for (int decoyDatabaseNum = 0; decoyDatabaseNum < Lollipop.decoy_databases; decoyDatabaseNum++)
                        results_rows += String.Join(Environment.NewLine, Lollipop.ed_relations[Lollipop.decoy_database_name_prefix + decoyDatabaseNum].Select(r => relation_as_tsv_row(r, relation_type)));
                    break;
                case ProteoformComparison.ee:
                    tsv_header = String.Join("\t", new List<string> { "proteoform1_accession", "proteoform2_accession", "delta_mass", "nearby_relations" });
                    results_rows = String.Join(Environment.NewLine, Lollipop.ee_relations.Select(r => relation_as_tsv_row(r, relation_type)));
                    break;
                case ProteoformComparison.ef:
                    tsv_header = String.Join("\t", new List<string> { "proteoform1_accession", "proteoform2_accession", "delta_mass", "nearby_relations" });
                    results_rows = String.Join(Environment.NewLine, Lollipop.ef_relations.Select(r => relation_as_tsv_row(r, relation_type)));
                    break;
            }
            return tsv_header + Environment.NewLine + results_rows;
        }

        private static string relation_as_tsv_row(ProteoformRelation r, ProteoformComparison relation_type)
        {
            string tsv_row = "";
            switch (relation_type)
            {
                case ProteoformComparison.et:
                case ProteoformComparison.ed:
                    tsv_row = String.Join("\t", new List<string> { r.connected_proteoforms[0].accession.ToString(), ((TheoreticalProteoform)r.connected_proteoforms[1]).description.ToString(),
                        r.delta_mass.ToString(), r.nearby_relations_count.ToString() });
                    break;
                case ProteoformComparison.ee:
                case ProteoformComparison.ef:
                    tsv_row = String.Join("\t", new List<string> { r.connected_proteoforms[0].accession.ToString(), r.connected_proteoforms[1].accession.ToString(), r.delta_mass.ToString(),
                        r.nearby_relations_count.ToString() });
                    break;
            }
            return tsv_row;
        }

        // DELTA MASS PEAK I/O
        public static void read_peaks(string[] lines, ProteoformComparison relation_type)
        {
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                string[] p1 = line[0].Split(',');
                string[] p2 = line[1].Split(',');
                List<ProteoformRelation> relations_in_peak = new List<ProteoformRelation>();
                List<ProteoformRelation> relation = new List<ProteoformRelation>();

                for (int i = 0; i < p1.Length; i++)
                {
                    if (relation_type == ProteoformComparison.et) relation = Lollipop.et_relations.Where(p => p.connected_proteoforms[0].accession == p1[i].Trim().TrimEnd(',') && p.connected_proteoforms[1].accession == p2[i].Trim().TrimEnd(',')).ToList();
                    else if (relation_type == ProteoformComparison.ee) relation = Lollipop.ee_relations.Where(p => p.connected_proteoforms[0].accession == p1[i].Trim().TrimEnd(',') && p.connected_proteoforms[1].accession == p2[i].Trim().TrimEnd(',')).ToList();
                    relation[0].connected_proteoforms[0].relationships.Add(relation[0]);
                    relation[0].connected_proteoforms[1].relationships.Add(relation[0]);
                    relations_in_peak.Add(relation[0]);
                }

                DeltaMassPeak peak = new DeltaMassPeak(relations_in_peak[0], relations_in_peak);
                peak.peak_relation_group_count = Convert.ToInt16(line[3]);
                peak.peak_deltaM_average = Convert.ToDouble(line[2]);
                string[] possible_peak_assignments_string = line[5].Split(',');
                List<Modification> possible_peak_assignments = new List<Modification>();
                foreach (string mod in possible_peak_assignments_string)
                {
                    possible_peak_assignments.Add(new Modification(mod.Trim().TrimEnd(',')));
                }
                peak.possiblePeakAssignments = possible_peak_assignments;
                lock (lockThread)
                {
                    if (relation_type == ProteoformComparison.et) Lollipop.et_peaks.Add(peak);
                    else if (relation_type == ProteoformComparison.ee) Lollipop.ee_peaks.Add(peak);
                }
            });
        }

        public static string peak_results(ProteoformComparison relation_type)
        {
            string tsv_header = "";
            string results_rows = "";
            switch (relation_type)
            {
                case ProteoformComparison.et:
                    tsv_header = String.Join("\t", new List<string> { "experimental_accessions", "theoretical_accessions", "peak_deltaM_average", "peak_relation_group_count",
                        "decoy_relation_count", "peak_group_fdr", "peak_assignment" });
                    results_rows = String.Join(Environment.NewLine, Lollipop.et_peaks.Select(p => peak_as_tsv_row(p, relation_type)));
                    break;
                case ProteoformComparison.ee:
                    tsv_header = String.Join("\t", new List<string> { "experimental_1_accessions", "experimental_2_accessions", "peak_deltaM_average", "peak_relation_group_count",
                         "decoy_relation_count", "peak_group_fdr", "peak_assignment" });
                    results_rows = String.Join(Environment.NewLine, Lollipop.ee_peaks.Select(p => peak_as_tsv_row(p, relation_type)));
                    break;
            }
            return tsv_header + Environment.NewLine + results_rows;
        }

        private static string peak_as_tsv_row(DeltaMassPeak p, ProteoformComparison relation_type)
        {
            string accessions_1_string = String.Join(", ", p.grouped_relations.Select(r => r.connected_proteoforms[0].accession));
            string accessions_2_string = String.Join(", ", p.grouped_relations.Select(r => r.connected_proteoforms[1].accession));
            return String.Join("\t", new List<string> {accessions_1_string, accessions_2_string, p.peak_deltaM_average.ToString(), p.peak_relation_group_count.ToString(), p.decoy_relation_count.ToString(), p.peak_group_fdr.ToString(), p.possiblePeakAssignments_string });
        }

        // PROTEOFORM FAMILY I/O
        public static void read_families(string[] lines)
        {
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                lock (lockThread) { }
            });
        }

        public static string family_results()
        {
            string tsv_header = "family_id\tproteoform1_accession\tpeak_center_delta_mass\tproteoform2_accession";
            string results_rows = String.Join(Environment.NewLine, Lollipop.proteoform_community.families.Select(f => family_as_tsv_row(f)));
            return tsv_header + Environment.NewLine + results_rows;
        }

        private static string family_as_tsv_row(ProteoformFamily f)
        {
            //Probably {family_id, proteoform1_id, delta_mass, proteoform2_id} for each relation
            //This could be placed directly into Cytoscape
            return String.Join(Environment.NewLine, new List<string>(
                f.relations.Select(r => String.Join("\t", new List<string> { f.family_id.ToString(), r.accession_1, r.peak_center_deltaM.ToString(), r.accession_2 }))));
        }
    }
}
