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
                TheoreticalProteoform theoretical_proteoform = new TheoreticalProteoform(line[0], line[5], line[6], line[7], Convert.ToInt16(line[8]), Convert.ToInt16(line[9]), Convert.ToDouble(line[10]), Convert.ToInt16(line[2]), ptm_list, Convert.ToDouble(line[12]), Convert.ToDouble(line[1]), true);
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
                TheoreticalProteoform decoy_proteoform = new TheoreticalProteoform(line[0], line[5], line[6], line[7], Convert.ToInt16(line[8]), Convert.ToInt16(line[9]), Convert.ToDouble(line[10]), Convert.ToInt16(line[2]), ptm_list, Convert.ToDouble(line[12]), Convert.ToDouble(line[1]), false);
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
                List<TheoreticalProteoform> theoretical = Lollipop.proteoform_community.theoretical_proteoforms.Where(c => c.accession.Equals((line[1]))).ToList();
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
                int decoyDatabaseNum = Convert.ToInt16(decoy_accession[3]);
                List<TheoreticalProteoform> decoy = Lollipop.proteoform_community.decoy_proteoforms["DecoyDatabase_" + decoyDatabaseNum].Where(c => c.accession.Equals((line[1]))).ToList();
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
        public static string raw_component_results()
        {
            return Component.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.raw_experimental_components.Where(c => c != null).Select(c => c.as_tsv_row()));
        }
        public static string raw_neucode_pair_results()
        {
            return NeuCodePair.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.raw_neucode_pairs.Select(p => p.as_tsv_row()));
        }
        public static string aggregated_experimental_proteoform_results()
        {
            return ExperimentalProteoform.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.proteoform_community.experimental_proteoforms.Select(p => p.as_tsv_row()));
        }
        public static string et_relations_results()
        {
            return ProteoformRelation.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.et_relations.Select(r => r.as_tsv_row()));
        }
        public static string et_peak_results()
        {
            return DeltaMassPeak.get_tsv_header(true) + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.et_peaks.Select(r => r.as_tsv_row()));
        }
        public static string ed_relations_results()
        {
            return ProteoformRelation.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.ed_relations.Values.ToList()[0].Select(r => r.as_tsv_row()));
        }
        public static string ee_relations_results()
        {
            return ProteoformRelation.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.ee_relations.Select(r => r.as_tsv_row()));
        }
        public static string ef_relations_results()
        {
            return ProteoformRelation.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.ef_relations.Select(r => r.as_tsv_row()));
        }
        public static string ee_peak_results()
        {
            return DeltaMassPeak.get_tsv_header(true) + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.ee_peaks.Select(r => r.as_tsv_row()));
        }
        public static string theoretical_proteoforms_results()
        {
            return TheoreticalProteoform.get_tsv_header(true) + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.proteoform_community.theoretical_proteoforms.Select(p => p.as_tsv_row("")));
        }
        public static string decoy_proteoforms_results()
        {
            //put all decoy databases in one tsv file
            string decoy_results = TheoreticalProteoform.get_tsv_header(false) + Environment.NewLine;
            for (int decoyDatabaseNum = 0; decoyDatabaseNum < Lollipop.decoy_databases; decoyDatabaseNum++)
                decoy_results += String.Join(Environment.NewLine, Lollipop.proteoform_community.decoy_proteoforms["DecoyDatabase_" + decoyDatabaseNum].Select(p => p.as_tsv_row("DecoyDatabase_" + decoyDatabaseNum))) + Environment.NewLine;
            return decoy_results;
        }
    }
}
