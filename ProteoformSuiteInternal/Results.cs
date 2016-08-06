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
        //RESULTS INPUT
        static Object lockThread = new object(); //used so that parallel for loops can be used to add to list without addint @ same time
        public static void read_raw_components(string working_directory)
        {
            string[] lines = File.ReadAllLines(working_directory + "\\raw_experimental_components.tsv");
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                Component component = new Component();
                component.id = Convert.ToInt16(line[0]);
                component.monoisotopic_mass = Convert.ToDouble(line[1]);
                component.weighted_monoisotopic_mass = Convert.ToDouble(line[2]);
                component.corrected_mass = Convert.ToDouble(line[3]);
                component.intensity_sum = Convert.ToDouble(line[4]);
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
                    component.file_origin = line[13];
                }
                else { component.file_origin = line[12]; }
                component.accepted = true;
                lock (lockThread) { Lollipop.raw_experimental_components.Add(component); }
            });

        }

        public static void read_raw_neucode_pairs(string working_directory)
        {
            string[] lines = File.ReadAllLines(working_directory + "\\raw_neucode_pairs.tsv");
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                List<Component> neucode_light = Lollipop.raw_experimental_components.Where(c => c.id == Convert.ToInt16(line[0])).ToList();
                List<Component> neucode_heavy = Lollipop.raw_experimental_components.Where(c => c.id == Convert.ToInt16(line[5])).ToList();
                NeuCodePair neucode_pair = new NeuCodePair(neucode_heavy[0], neucode_light[0]);
                neucode_pair.id_light = Convert.ToInt16(line[0]);
                neucode_pair.id_heavy = Convert.ToInt16(line[5]);
                neucode_pair.intensity_ratio = Convert.ToDouble(line[8]);
                neucode_pair.lysine_count = Convert.ToInt32(line[9]);
                neucode_pair.file_origin = line[10];

                if (neucode_pair.lysine_count > Lollipop.min_lysine_ct && neucode_pair.lysine_count < Lollipop.max_lysine_ct
                    && neucode_pair.intensity_ratio > Convert.ToDouble(Lollipop.min_intensity_ratio) && neucode_pair.intensity_ratio < Convert.ToDouble(Lollipop.max_intensity_ratio))
                { neucode_pair.accepted = true; }
                else { neucode_pair.accepted = false; }

                neucode_pair.corrected_mass = neucode_pair.corrected_mass + Math.Round((neucode_pair.lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * 1.0015;

                lock (lockThread) { Lollipop.raw_neucode_pairs.Add(neucode_pair); }
            });
        }

        public static void read_aggregated_proteoforms(string working_directory)
        {
            string[] lines = File.ReadAllLines(working_directory + "\\aggregated_experimental_proteoforms.tsv");
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                ExperimentalProteoform aggregated_proteoform = new ExperimentalProteoform(line[0], Convert.ToDouble(line[1]), Convert.ToInt16(line[2]),
                    Convert.ToBoolean(line[3]));
                aggregated_proteoform.agg_mass = Convert.ToDouble(line[5]);
                aggregated_proteoform.agg_intensity = Convert.ToDouble(line[6]);
                aggregated_proteoform.agg_rt = Convert.ToDouble(line[7]);
                aggregated_proteoform.observation_count = Convert.ToInt16(line[8]);
                lock (lockThread) { Lollipop.proteoform_community.experimental_proteoforms.Add(aggregated_proteoform); }
            });
        }

        public static void read_theoretical_proteoforms(string working_directory)
        {
            string[] lines = File.ReadAllLines(working_directory + "\\theoretical_proteoforms.tsv");
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                List<Ptm> ptm_list = new List<Ptm>();
                TheoreticalProteoform theoretical_proteoform = new TheoreticalProteoform(line[0], line[5], line[6], line[7], Convert.ToInt32(line[8]), Convert.ToInt32(line[9]), Convert.ToDouble(line[10]), Convert.ToInt32(line[2]), ptm_list, Convert.ToDouble(line[12]), Convert.ToDouble(line[1]), true);
                theoretical_proteoform.set_ptm_list(line[11]);
                lock (lockThread) Lollipop.proteoform_community.theoretical_proteoforms.Add(theoretical_proteoform);
            });
        }

        public static void read_decoy_proteoforms(string working_directory)
        {
            string[] lines = File.ReadAllLines(working_directory + "\\decoy_proteoforms.tsv");
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                List<Ptm> ptm_list = new List<Ptm>();
                TheoreticalProteoform decoy_proteoform = new TheoreticalProteoform(line[0], line[5], line[6], line[7], Convert.ToInt32(line[8]), Convert.ToInt32(line[9]), Convert.ToDouble(line[10]), Convert.ToInt32(line[2]), ptm_list, Convert.ToDouble(line[12]), Convert.ToDouble(line[1]), false);
                decoy_proteoform.set_ptm_list(line[11]);
                lock (lockThread)
                {
                    if (Lollipop.proteoform_community.decoy_proteoforms.ContainsKey(line[13])) Lollipop.proteoform_community.add(decoy_proteoform, line[13]);
                    else
                    {
                        Lollipop.proteoform_community.decoy_proteoforms.Add(line[13], new List<TheoreticalProteoform>());
                        Lollipop.ed_relations.Add(line[13], new List<ProteoformRelation>());
                        Lollipop.proteoform_community.add(decoy_proteoform, line[13]);
                    }
                }
            });
        }

        public static void read_experimental_theoretical_relationships(string working_directory)
        {
            string[] lines = File.ReadAllLines(working_directory + "\\experimental_theoretical_relationships.tsv");
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                List<ExperimentalProteoform> experimental = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[0]))).ToList();
                List<TheoreticalProteoform> theoretical = Lollipop.proteoform_community.theoretical_proteoforms.Where(c => c.description.Equals(line[1])).ToList();
                ProteoformRelation relation = new ProteoformRelation(experimental[0], theoretical[0], ProteoformComparison.et, Convert.ToDouble(line[2]));
                relation.nearby_relations_count = Convert.ToInt16(line[3]);
                lock (lockThread) { Lollipop.et_relations.Add(relation); }
            });
        }

        public static void read_experimental_decoy_relationships(string working_directory)
        {
            string[] lines = File.ReadAllLines(working_directory + "\\experimental_decoy_relationships.tsv");
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                List<ExperimentalProteoform> experimental = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[0]))).ToList();
                string[] decoy_accession = line[1].Split('_'); //determine which decoy database
                int decoyDatabaseNum = Convert.ToInt16(decoy_accession[5]);
                List<TheoreticalProteoform> decoy = Lollipop.proteoform_community.decoy_proteoforms["DecoyDatabase_" + decoyDatabaseNum].Where(c => c.description.Equals((line[1]))).ToList();
                ProteoformRelation relation = new ProteoformRelation(experimental[0], decoy[0], ProteoformComparison.ed, Convert.ToDouble(line[2]));
                relation.nearby_relations_count = Convert.ToInt16(line[3]);
                lock (lockThread) { Lollipop.ed_relations["DecoyDatabase_" + decoyDatabaseNum].Add(relation); }
            });
        }

        public static void read_experimental_experimental_relationships(string working_directory)
        {
            string[] lines = File.ReadAllLines(working_directory + "\\experimental_experimental_relationships.tsv");
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                List<ExperimentalProteoform> experimental_1 = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[0]))).ToList();
                List<ExperimentalProteoform> experimental_2 = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[1]))).ToList();
                ProteoformRelation relation = new ProteoformRelation(experimental_1[0], experimental_2[0], ProteoformComparison.ee, Convert.ToDouble(line[2]));
                relation.nearby_relations_count = Convert.ToInt16(line[3]);
                lock (lockThread) { Lollipop.ee_relations.Add(relation); }
            });
        }

        public static void read_experimental_false_relationships(string working_directory)
        {
            string[] lines = File.ReadAllLines(working_directory + "\\experimental_false_relationships.tsv");
            Parallel.For(1, lines.Length, x =>
            {
                string[] line = lines[x].Split('\t');
                List<ExperimentalProteoform> experimental_1 = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[0]))).ToList();
                List<ExperimentalProteoform> experimental_2 = Lollipop.proteoform_community.experimental_proteoforms.Where(c => c.accession.Equals((line[1]))).ToList();
                ProteoformRelation relation = new ProteoformRelation(experimental_1[0], experimental_2[0], ProteoformComparison.ef, Convert.ToDouble(line[2]));
                relation.nearby_relations_count = Convert.ToInt16(line[3]);
                lock (lockThread) { Lollipop.ef_relations.Add(relation); }
            });
        }

        public static void read_peaks(string working_directory, ProteoformComparison relation_type)
        {
            string[] lines = new string[0];
            if (relation_type == ProteoformComparison.et) lines = File.ReadAllLines(working_directory + "\\experimental_theoretical_peaks.tsv");
            else if (relation_type == ProteoformComparison.ee) lines = File.ReadAllLines(working_directory + "\\experimental_experimental_peaks.tsv");
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
                string[] possible_peak_assignments_string = line[6].Split(',');
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


        //RESULTS OUTPUT
        public static string get_results(IEnumerable<object> objects)
        {
            string header = get_tsv_header(objects.GetType().GetGenericArguments()[0]);
            if (objects.Count() == 0)
                return header;
            else
                return header + Environment.NewLine + String.Join(Environment.NewLine, objects.Select(o => as_tsv_row(o)));
        }
        public static string get_results(IEnumerable<object> objects, ProteoformComparison relation_type)
        {
            string header = get_tsv_header(objects.GetType().GetGenericArguments()[0], relation_type);
            if (objects.Count() == 0)
                return header;
            else
                return header + Environment.NewLine + String.Join(Environment.NewLine, objects.Select(o => as_tsv_row(o, relation_type)));
        }

        public static string raw_component_results() { return get_results(Lollipop.raw_experimental_components); }
        public static string raw_neucode_pair_results() { return get_results(Lollipop.raw_neucode_pairs); }
        public static string aggregated_experimental_proteoform_results() { return get_results(Lollipop.proteoform_community.experimental_proteoforms); }
        public static string et_relations_results() { return get_results(Lollipop.et_relations, ProteoformComparison.et); }
        public static string ed_relations_results() { return get_results(Lollipop.ed_relations.Values.ToList()[0], ProteoformComparison.ed); }
        public static string ee_relations_results() { return get_results(Lollipop.ee_relations, ProteoformComparison.ee); }
        public static string ef_relations_results() { return get_results(Lollipop.ef_relations, ProteoformComparison.ef); }
        public static string et_peak_results() { return get_results(Lollipop.et_peaks, ProteoformComparison.et); }
        public static string ee_peak_results() { return get_results(Lollipop.ee_peaks, ProteoformComparison.ee); }
        public static string theoretical_proteoforms_results()
        {
            return get_tsv_header(typeof(TheoreticalProteoform)) + Environment.NewLine +
                String.Join(Environment.NewLine, Lollipop.proteoform_community.theoretical_proteoforms.Select(t => as_tsv_row(t, null)));
        }
        public static string decoy_proteoforms_results()
        {
            //put all decoy databases in one tsv file
            string decoy_results = get_tsv_header(typeof(TheoreticalProteoform), false) + Environment.NewLine;
            for (int decoyDatabaseNum = 0; decoyDatabaseNum < Lollipop.decoy_databases; decoyDatabaseNum++)
                decoy_results += String.Join(Environment.NewLine, Lollipop.proteoform_community.decoy_proteoforms["DecoyDatabase_" + decoyDatabaseNum].Select(p => as_tsv_row(p, "DecoyDatabase_" + decoyDatabaseNum))) + Environment.NewLine;
            return decoy_results;
        }

        // TSV STRINGS
        public static string get_tsv_header(Type type)
        {
            if (type == typeof(Component))
            {
                if (Lollipop.neucode_labeled)
                    return String.Join("\t", new List<string> { "id", "monoisotopic_mass", "weighted_monoisotopic_mass", "corrected_mass", "intensity_sum", "num_charge_states",
                        "delta_mass", "relative_abundance", "fract_abundance", "scan_range", "rt_range", "rt_apex", "intensity_sum_olcs", "file_origin" });
                else
                    return String.Join("\t", new List<string> { "id", "monoisotopic_mass", "weighted_monoisotopic_mass", "corrected_mass", "intensity_sum", "num_charge_states",
                        "delta_mass", "relative_abundance", "fract_abundance", "scan_range", "rt_range", "rt_apex", "file_origin" });
            }
            else if (type == typeof(NeuCodePair))
                return String.Join("\t", new List<string> { "light_id", "light_intensity (overlapping charge states)", "light_weighted_monoisotopic_mass", "light_corrected_mass", "light_apexRt",
                "heavy_id", "heavy_intensity (overlapping charge states)", "heavy_weighted_monoisotopic_mass", "intensity_ratio", "lysine_count", "file_origin" });
            else if (type == typeof(ExperimentalProteoform))
                return String.Join("\t", new List<string> { "accession", "modified_mass", "lysine_count", "is_target", "is_decoy",
                "agg_mass", "agg_intensity", "agg_rt", "observation_count" });
            else return "";
        }
        public static string as_tsv_row(object x)
        {
            if (x.GetType() == typeof(Component))
            {
                Component c = (Component)x;
                if (Lollipop.neucode_labeled)
                    return String.Join("\t", new List<string> { c.id.ToString(), c.monoisotopic_mass.ToString(), c.weighted_monoisotopic_mass.ToString(), c. corrected_mass.ToString(), c.intensity_sum.ToString(), c.num_charge_states.ToString(),
                    c.delta_mass.ToString(), c.relative_abundance.ToString(), c.fract_abundance.ToString(), c.scan_range.ToString(), c.rt_range.ToString(),
                    c.rt_apex.ToString(), c.intensity_sum_olcs.ToString(), c.file_origin.ToString() });
                else
                    return String.Join("\t", new List<string> { c.id.ToString(), c.monoisotopic_mass.ToString(), c.weighted_monoisotopic_mass.ToString(), c.corrected_mass.ToString(), c.intensity_sum.ToString(), c.num_charge_states.ToString(),
                    c.delta_mass.ToString(), c.relative_abundance.ToString(), c.fract_abundance.ToString(), c.scan_range.ToString(), c.rt_range.ToString(),
                    c.rt_apex.ToString(), c.file_origin.ToString() });
            }
            else if (x.GetType() == typeof(NeuCodePair))
            {
                NeuCodePair n = (NeuCodePair)x;
                return String.Join("\t", new List<string> { n.id.ToString(), n.intensity_sum_olcs.ToString(), n.weighted_monoisotopic_mass.ToString(), n.corrected_mass.ToString(), n.rt_apex.ToString(),
                    n.neuCodeHeavy.id.ToString(), n.neuCodeHeavy.intensity_sum_olcs.ToString(), n.neuCodeHeavy.weighted_monoisotopic_mass.ToString(), n.intensity_ratio.ToString(), n.lysine_count.ToString(),
                    n.file_origin.ToString() });
            }
            else if (x.GetType() == typeof(ExperimentalProteoform))
            {
                ExperimentalProteoform e = (ExperimentalProteoform)x;
                return String.Join("\t", new List<string> { e.accession.ToString(), e.modified_mass.ToString(), e.lysine_count.ToString(), e.is_target.ToString(), e.is_decoy.ToString(),
                    e.agg_mass.ToString(), e.agg_intensity.ToString(), e.agg_rt.ToString(), e.observation_count.ToString() });
            }
            else return "";
        }

        public static string get_tsv_header(Type type, bool is_target)
        {
            if (type != typeof(TheoreticalProteoform)) return "";
            if (is_target)
                return String.Join("\t", new List<string> { "accession", "modified_mass", "lysine_count", "is_target", "is_decoy",
                "description", "name", "fragment", "begin", "end", "unmodified_mass", "ptm_list", "ptm_mass" });
            else
                return String.Join("\t", new List<string> { "accession", "modified_mass", "lysine_count", "is_target", "is_decoy",
                "description", "name", "fragment", "begin", "end", "unmodified_mass", "ptm_list", "ptm_mass", "decoy_database" });
        }
        public static string as_tsv_row(object x, string decoy_database)
        {
            if (x.GetType() != typeof(TheoreticalProteoform)) return "";
            TheoreticalProteoform t = (TheoreticalProteoform)x;
            string row = String.Join("\t", new List<string> { t.accession.ToString(), t.modified_mass.ToString(), t.lysine_count.ToString(), t.is_target.ToString(), t.is_decoy.ToString(),
                    t.description.ToString(), t.name.ToString(), t.fragment.ToString(), t.begin.ToString(), t.end.ToString(), t.unmodified_mass.ToString(), t.ptm_descriptions.ToString(), t. ptm_mass.ToString() });
            if (decoy_database == null) return row;
            else return row + "\t" + decoy_database;
        }

        public static string get_tsv_header(Type type, ProteoformComparison relation_type)
        {
            if (type == typeof(ProteoformRelation))
            {
                if (relation_type == ProteoformComparison.ee || relation_type == ProteoformComparison.ef) return String.Join("\t", new List<string> { "proteoform1_accession", "proteoform2_accession", "delta_mass", "nearby_relations" });
                else if (relation_type == ProteoformComparison.et || relation_type == ProteoformComparison.ed) return String.Join("\t", new List<string> { "proteoform1_accession", "proteoform2_description", "delta_mass", "nearby_relations" });
                else return "Error: Did not recognize ProteoformRelation type.";
                //multiple theoreticals have same accession, not same description
            }
            else if (type != typeof(DeltaMassPeak))
            {
                if (relation_type == ProteoformComparison.et)
                    return String.Join("\t", new List<string> { "experimental_accessions", "theoretical_accessions", "peak_deltaM_average", "peak_relation_group_count", "decoy_relation_count", "peak_group_fdr", "peak_assignment" });
                else if (relation_type == ProteoformComparison.ee)
                    return String.Join("\t", new List<string> { "experimental_1_accessions", "experimental_2_accessions", "peak_deltaM_average", "peak_relation_group_count", "decoy_relation_count", "peak_group_fdr", "peak_assignment" });
                else return "Error: Did not recognize DeltaMassPeak type.";
            }
            else return "";
        }
        public static string as_tsv_row(object x, ProteoformComparison relation_type)
        {
            if (x.GetType() == typeof(ProteoformRelation))
            {
                ProteoformRelation r = (ProteoformRelation)x;
                if (relation_type == ProteoformComparison.ee || relation_type == ProteoformComparison.ef)
                    return String.Join("\t", new List<string> { r.connected_proteoforms[0].accession.ToString(), r.connected_proteoforms[1].accession.ToString(), r.delta_mass.ToString(), r.nearby_relations_count.ToString() });
                else if (relation_type == ProteoformComparison.et || relation_type == ProteoformComparison.ed)
                    return String.Join("\t", new List<string> { r.connected_proteoforms[0].accession.ToString(), ((TheoreticalProteoform)r.connected_proteoforms[1]).description.ToString(), r.delta_mass.ToString(), r.nearby_relations_count.ToString() });
                else return "Error: Did not recognize ProteoformRelation type.";
            }
            else if (x.GetType() != typeof(DeltaMassPeak))
            {
                //gives list of proteoform accessions in the peak
                DeltaMassPeak p = (DeltaMassPeak)x;
                string accessions_1_string = String.Join(", ", p.grouped_relations.Select(r => r.connected_proteoforms[0].accession));
                string accessions_2_string = String.Join(", ", p.grouped_relations.Select(r => r.connected_proteoforms[1].accession));
                return String.Join("\t", new List<string> {accessions_1_string, accessions_2_string, p.peak_deltaM_average.ToString(),
                    p.peak_relation_group_count.ToString(), p.decoy_relation_count.ToString(), p.peak_group_fdr.ToString(), p.possiblePeakAssignments_string });
            }
            else return "";
        }
    }
}
