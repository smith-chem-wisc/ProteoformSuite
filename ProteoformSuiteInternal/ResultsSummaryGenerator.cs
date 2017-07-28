using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public static class ResultsSummaryGenerator
    {

        #region Private Methods

        private static void save_summary(string directory, string timestamp)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(Sweet.lollipop.results_folder, "summary_" + timestamp + ".txt")))
                writer.Write(generate_full_report());
        }

        private static void save_dataframe(string directory, string timestamp)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(Sweet.lollipop.results_folder, "results_" + timestamp + ".tsv")))
                writer.Write(results_dataframe());
        }

        private static void save_cytoscripts(string directory, string timestamp)
        {
            string message = "";

            message += CytoscapeScript.write_cytoscape_script(Sweet.lollipop.target_proteoform_community.families, Sweet.lollipop.target_proteoform_community.families,
                    Sweet.lollipop.results_folder, "AllFamilies_", timestamp,
                    false, 
                    true, true,
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
            message += Environment.NewLine;

            if (Sweet.lollipop.qVals.Count > 0)
            {
                message += CytoscapeScript.write_cytoscape_script(Sweet.lollipop.target_proteoform_community.families, Sweet.lollipop.target_proteoform_community.families,
                    Sweet.lollipop.results_folder, "AllQuantFamilies_", timestamp,
                    true,
                    true, true, 
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
                message += Environment.NewLine;

                message += CytoscapeScript.write_cytoscape_script(Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.minProteoformFoldChange, Sweet.lollipop.maxGoTermFDR, Sweet.lollipop.minProteoformIntensity).Distinct().ToList(), Sweet.lollipop.target_proteoform_community.families,
                    Sweet.lollipop.results_folder, "SignificantChanges_", timestamp,
                    true, 
                    true, true, 
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
                message += Environment.NewLine;
            }

            foreach (GoTermNumber gtn in Sweet.lollipop.goTermNumbers.Where(g => g.by < (double)Sweet.lollipop.maxGoTermFDR).ToList())
            {
                message += CytoscapeScript.write_cytoscape_script(new GoTermNumber[] { gtn }, Sweet.lollipop.target_proteoform_community.families,
                    Sweet.lollipop.results_folder, gtn.Aspect.ToString() + gtn.Description.Replace(" ", "_") + "_", timestamp,
                    true, true, true, 
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
                message += Environment.NewLine;
            }
            message += "Remember to install the package \"enhancedGraphics\" under App -> App Manager to view piechart nodes for quantitative data";
        }

        #endregion Private Methods

        #region Public Methods

        public static void save_all(string directory, string timestamp)
        {
            Parallel.Invoke
            (
                () => save_summary(Sweet.lollipop.results_folder, timestamp),
                () => save_dataframe(Sweet.lollipop.results_folder, timestamp),
                () => save_cytoscripts(Sweet.lollipop.results_folder, timestamp)
            );
        }

        public static string generate_full_report()
        {
            return
                counts() +
                proteins_of_significance() +
                go_terms_of_significance() +
                actions() +
                loaded_files_report();
        }

        public static void save_biological_replicate_intensities(string filename, bool include_imputation, bool use_bft, List<ExperimentalProteoform> proteoforms)
        {
            using (StreamWriter writer = new StreamWriter(filename))
                writer.Write(biological_replicate_intensities(proteoforms, Sweet.lollipop.input_files, Sweet.lollipop.conditionsBioReps, include_imputation, use_bft));
        }

        public static string actions()
        {
            string header = "USER ACTIONS" + Environment.NewLine;
            string report = String.Join(Environment.NewLine, Sweet.actions) + Environment.NewLine + Environment.NewLine;
            return header + report;
        }

        public static string loaded_files_report()
        {
            string header = "DECONVOLUTION RESULTS FILES AND PROTEIN DATABASE FILES" + Environment.NewLine;
            string report = "";
            foreach (Purpose p in Sweet.lollipop.input_files.Select(f => f.purpose).Distinct())
            {
                report += p.ToString() + ":" + Environment.NewLine + String.Join(Environment.NewLine, Sweet.lollipop.get_files(Sweet.lollipop.input_files, p).Select(f => f.filename + f.extension + "\t" + f.complete_path)) + Environment.NewLine + Environment.NewLine;
            }
            return header + report;
        }

        public static string counts()
        {
            string report = "";

            report += Sweet.lollipop.raw_experimental_components.Count.ToString() + "\tRaw Experimental Components" + Environment.NewLine;
            report += Sweet.lollipop.raw_quantification_components.Count.ToString() + "\tRaw Quantitative Components" + Environment.NewLine;
            report += Sweet.lollipop.raw_neucode_pairs.Count.ToString() + "\tRaw NeuCode Pairs" + Environment.NewLine;
            report += Sweet.lollipop.raw_neucode_pairs.Count(nc => nc.accepted).ToString() + "\tAccepted NeuCode Pairs" + Environment.NewLine;
            report += Environment.NewLine;

            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Length.ToString() + "\tExperimental Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.accepted).ToString() + "\tAccepted Experimental Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.theoretical_database.theoretical_proteins.Sum(kv => kv.Value.Length).ToString() + "\tTheoretical Proteins" + Environment.NewLine;
            report += Sweet.lollipop.theoretical_database.expanded_proteins.Length + "\tExpanded Theoretical Proteins" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length.ToString() + "\tTheoretical Proteoforms" + Environment.NewLine;
            report += Environment.NewLine;

            report += Sweet.lollipop.et_peaks.Count.ToString() + "\tExperimental-Theoretical Peaks" + Environment.NewLine;
            report += Sweet.lollipop.et_relations.Count.ToString() + "\tExperimental-Theoretical Pairs" + Environment.NewLine;
            report += Sweet.lollipop.et_peaks.Count(p => p.Accepted).ToString() + "\tAccepted Experimental-Theoretical Peaks" + Environment.NewLine;
            report += Sweet.lollipop.et_relations.Count(r => r.Accepted).ToString() + "\tAccepted Experimental-Theoretical Pairs" + Environment.NewLine;
            report += Sweet.lollipop.ed_relations.Count <= 0 ? Environment.NewLine : Sweet.lollipop.ed_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-Decoy Pairs" + Environment.NewLine;
            report += Environment.NewLine;

            report += Sweet.lollipop.ee_peaks.Count.ToString() + "\tExperimental-Experimental Peaks" + Environment.NewLine;
            report += Sweet.lollipop.ee_relations.Count.ToString() + "\tExperimental-Experimental Pairs" + Environment.NewLine;
            report += Sweet.lollipop.ee_peaks.Count(p => p.Accepted).ToString() + "\tAccepted Experimental-Experimental Peaks" + Environment.NewLine;
            report += Sweet.lollipop.ee_relations.Count(r => r.Accepted).ToString() + "\tAccepted Experimental-Experimental Pairs" + Environment.NewLine;
            report += Sweet.lollipop.ef_relations.Count <= 0 ? Environment.NewLine : Sweet.lollipop.ef_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-False Pairs" + Environment.NewLine;
            report += Environment.NewLine;

            report += proteoform_families_report();

            report += quant_report();

            return report;
        }

        public static string proteoform_families_report()
        {
            string report = "";

            report += Sweet.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count > 1).ToString() + "\tProteoform Families" + Environment.NewLine;
            List<ProteoformFamily> identified_families = Sweet.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() == 1).ToList();
            List<ProteoformFamily> ambiguous_families = Sweet.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() > 1).ToList();
            List<ProteoformFamily> unidentified_families = Sweet.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() == 0 && f.proteoforms.Count > 1).ToList();
            report += identified_families.Count.ToString() + "\tIdentified Families (Correspond to 1 " + (ProteoformCommunity.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += identified_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Identified Families" + Environment.NewLine;
            report += ambiguous_families.Count.ToString() + "\tAmbiguous Families (Correspond to > 1 " + (ProteoformCommunity.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += ambiguous_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Ambiguous Families" + Environment.NewLine;
            report += unidentified_families.Count.ToString() + "\tUnidentified Families (Correspond to no " + (ProteoformCommunity.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += unidentified_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Unidentified Families" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count == 1).ToString() + "\tOrphaned Experimental Proteoforms (Not joined with another proteoform)" + Environment.NewLine;
            report += Environment.NewLine;

            int raw_components_in_fams = identified_families.Concat(ambiguous_families).Concat(unidentified_families).Sum(f => f.experimental_proteoforms.Sum(e => e.lt_verification_components.Count + e.hv_verification_components.Count));
            report += raw_components_in_fams + "\tRaw Experimental Components in Families" + Environment.NewLine;
            report += Sweet.lollipop.raw_experimental_components.Count > 0 ?
                Math.Round(100 * ((double)raw_components_in_fams / (double)Sweet.lollipop.raw_experimental_components.Count), 2) + "\t% of Raw Experimental Components in Families" + Environment.NewLine :
                "N/A\t% of Raw Experimental Components in Families" + Environment.NewLine;

            int raw_quant_components_in_fams = identified_families.Concat(ambiguous_families).Concat(unidentified_families).Sum(f => f.experimental_proteoforms.Sum(e => e.lt_quant_components.Count + e.hv_quant_components.Count));
            report += raw_quant_components_in_fams + "\tRaw Quantitative Components in Families" + Environment.NewLine;
            report += Sweet.lollipop.raw_experimental_components.Count > 0 ?
                Math.Round(100 * ((double)raw_quant_components_in_fams / (double)Sweet.lollipop.raw_experimental_components.Count), 2) + "\t% of Raw Quantitative Components in Families" + Environment.NewLine :
                "N/A\t% of Raw Quantitative Components in Families" + Environment.NewLine;
            report += Environment.NewLine;


            int identified_exp_proteoforms = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null);
            double avg_identified_decoy_proteoforms = Sweet.lollipop.decoy_proteoform_communities.Count > 0 ?
                Sweet.lollipop.decoy_proteoform_communities.Average(v => v.Value.experimental_proteoforms.Count(e => e.linked_proteoform_references != null)) : 
                -1;
            report += identified_exp_proteoforms.ToString() + "\tIdentified Experimental Proteoforms" + Environment.NewLine;
            report += (avg_identified_decoy_proteoforms > 0 ? Math.Round(avg_identified_decoy_proteoforms, 2).ToString() : "N/A")
                    + "\tAverage Identified Experimental Proteoforms by Decoys" + Environment.NewLine;
            report += Sweet.lollipop.decoy_proteoform_communities.Values.SelectMany(v => v.families).Count() > 0 && identified_exp_proteoforms > 0 ?
                Math.Round(avg_identified_decoy_proteoforms / identified_exp_proteoforms, 4).ToString() + "\tProteoform FDR" + Environment.NewLine :
                "N/A\tProteoform FDR" + Environment.NewLine;
            report += Environment.NewLine;

            return report;
        }

        public static string quant_report()
        {
            string report = "";

            report += Sweet.lollipop.satisfactoryProteoforms.Count.ToString() + "\tQuantified Experimental Proteoforms (Threshold for Quantification: " + Sweet.lollipop.minBiorepsWithObservations.ToString() + " = " + Sweet.lollipop.observation_requirement + ")" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.significant_tusher && Sweet.lollipop.significance_by_permutation || p.quant.significant_foldchange && Sweet.lollipop.significance_by_log2FC).ToString() + "\tExperimental Proteoforms with Significant Change (Threshold for Significance: Log2FoldChange > " + Sweet.lollipop.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + Sweet.lollipop.minProteoformIntensity.ToString() + ", & Q-Value < " + Sweet.lollipop.maxGoTermFDR.ToString() + ")" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.relativeDifferenceFDR, 4).ToString() + "\tFDR for Significance Conclusion (Offset of " + Math.Round(Sweet.lollipop.offsetTestStatistics, 1).ToString() + " from d(i) = dE(i) line)" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.selectAverageIntensity, 4).ToString() + "\tAverage Log2 Intensity Quantified Experimental Proteoform Observations" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.selectStDev, 2).ToString() + "\tLog2 Intensity Standard Deviation for Quantified Experimental Proteoform" + Environment.NewLine;
            report += Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.minProteoformFoldChange, Sweet.lollipop.maxGoTermFDR, Sweet.lollipop.minProteoformIntensity).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.inducedOrRepressedProteins.Count.ToString() + "\tIdentified Proteins with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.goTermNumbers.Count(g => g.by < (double)Sweet.lollipop.maxGoTermFDR).ToString() + "\tGO Terms of Significance (Benjimini-Yekeulti p-value < " + Sweet.lollipop.maxGoTermFDR.ToString() + "): " + Environment.NewLine;
            report += Environment.NewLine;

            // Venn Diagram of quantifiable proteoforms
            List<string> conditions = Sweet.lollipop.ltConditionsBioReps.Keys.Concat(Sweet.lollipop.hvConditionsBioReps.Keys).Distinct().ToList();
            foreach (string condition in conditions)
            {
                Sweet.lollipop.ltConditionsBioReps.TryGetValue(condition, out List<string> ltbioreps);
                Sweet.lollipop.hvConditionsBioReps.TryGetValue(condition, out List<string> hvbioreps);
                string[] combined_bioreps = ltbioreps == null && hvbioreps == null ?
                    new string[0] :
                    ltbioreps == null ? hvbioreps.Distinct().ToArray() :
                    hvbioreps == null ? ltbioreps.Distinct().ToArray() :
                        ltbioreps.Concat(hvbioreps)
                        .Distinct()
                        .ToArray();

                report += combined_bioreps.Length.ToString() + "\tBiological Replicates for " + condition + Environment.NewLine;

                for (int start = 0; start < combined_bioreps.Length; start++)
                {
                    int exp_prots_with_these_bioreps = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Contains(combined_bioreps[start]));
                    int exp_prots_with_these_bioreps_exclusive = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Contains(combined_bioreps[start]) && e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Distinct().Count() == 1);
                    report += exp_prots_with_these_bioreps + "\tExperimental Proteoforms Observed in " + condition + ", Biological Replicate #" + combined_bioreps[start].ToString() + Environment.NewLine;
                    report += exp_prots_with_these_bioreps_exclusive + "\tExperimental Proteoforms Observed Exclusively in " + condition + ", Biological Replicate #" + combined_bioreps[start].ToString() + Environment.NewLine;

                    for (int end = start; end < combined_bioreps.Length; end++)
                    {
                        for (int between = end; between > start; between--)
                        {
                            List<string> bioreps_of_interest = new List<int> { start }.Concat(Enumerable.Range(between, end - between + 1)).Distinct().Select(idx => combined_bioreps[idx]).ToList();
                            exp_prots_with_these_bioreps = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => bioreps_of_interest.All(x => e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Contains(x)));
                            exp_prots_with_these_bioreps_exclusive = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => bioreps_of_interest.All(x => e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Contains(x)) && e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Distinct().Count() == bioreps_of_interest.Distinct().Count());
                            report += exp_prots_with_these_bioreps + "\tExperimental Proteoforms Observed in " + condition + ", Biological Replicates #" + String.Join(" #", bioreps_of_interest) + Environment.NewLine;
                            report += exp_prots_with_these_bioreps_exclusive + "\tExperimental Proteoforms Observed Exclusively in " + condition + ", Biological Replicates #" + String.Join(" #", bioreps_of_interest) + Environment.NewLine;
                        }
                    }
                }

                int exp_prots_not_in_condition = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.biorepIntensityList.All(br => br.condition != condition));
                report += exp_prots_not_in_condition + "\tExperimental Proteoforms Were Not Observed in " + condition + Environment.NewLine;
                report += Environment.NewLine;
            }
            report += Environment.NewLine;

            return report;
        }

        public static string proteins_of_significance()
        {
            return "Identified Proteins with Significant Change: " + Environment.NewLine
                + String.Join(Environment.NewLine, Sweet.lollipop.inducedOrRepressedProteins.Select(p => p.Accession).Distinct().OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
        }

        public static string go_terms_of_significance()
        {
            return "GO Terms of Significance (Benjimini-Yekeulti p-value < " + Sweet.lollipop.maxGoTermFDR.ToString() + "): " + Environment.NewLine
                + String.Join(Environment.NewLine, Sweet.lollipop.goTermNumbers.Where(g => g.by < (double)Sweet.lollipop.maxGoTermFDR).Select(g => g.ToString()).OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
        }

        public static string results_dataframe()
        {
            DataTable results = new DataTable();
            results.Columns.Add("Proteoform ID", typeof(string));
            results.Columns.Add("Aggregated Observation ID", typeof(string));
            results.Columns.Add("SGD ID", typeof(string));
            results.Columns.Add("Gene Name", typeof(string));
            results.Columns.Add("Protein Fragment Type", typeof(string));
            results.Columns.Add("PTM Type", typeof(string));
            results.Columns.Add("Mass Difference", typeof(double));
            results.Columns.Add("Retention Time", typeof(double));
            results.Columns.Add("Aggregated Intensity", typeof(double));
            results.Columns.Add((Sweet.lollipop.numerator_condition == "" ? "Condition #1" : Sweet.lollipop.numerator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add((Sweet.lollipop.denominator_condition == "" ? "Condition #2" : Sweet.lollipop.denominator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add("Statistically Significant", typeof(bool));

            foreach (ExperimentalProteoform e in Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms)
                .Where(e => e.linked_proteoform_references != null)
                .OrderByDescending(e => (Sweet.lollipop.significance_by_log2FC ? e.quant.significant_foldchange : e.quant.significant_tusher) ? 1 : 0)
                .ThenBy(e => (e.linked_proteoform_references.First() as TheoreticalProteoform).accession)
                .ThenBy(e => e.ptm_set.ptm_combination.Count))
            {

                results.Rows.Add(
                    (e.linked_proteoform_references.First() as TheoreticalProteoform).accession,
                    e.accession,
                    e.linked_proteoform_references.Last().gene_name.ordered_locus,
                    e.linked_proteoform_references.Last().gene_name.primary,
                    (e.linked_proteoform_references.First() as TheoreticalProteoform).fragment,
                    String.Join("; ", e.ptm_set.ptm_combination.Select(ptm => ptm.modification.id)),
                    e.modified_mass - e.linked_proteoform_references.Last().modified_mass,
                    e.agg_rt,
                    e.agg_intensity,
                    e.quant.numeratorIntensitySum,
                    e.quant.denominatorIntensitySum,
                    Sweet.lollipop.significance_by_log2FC ? e.quant.significant_foldchange : e.quant.significant_tusher
                );
            }

            StringBuilder result_string = new StringBuilder();
            string header = "";
            foreach (DataColumn column in results.Columns)
            {
                header += column.ColumnName + "\t";
            }
            result_string.AppendLine(header);
            foreach (DataRow row in results.Rows)
            {
                result_string.AppendLine(String.Join("\t", row.ItemArray));
            }
            return result_string.ToString();
        }

        public static string biological_replicate_intensities(List<ExperimentalProteoform> proteoforms, List<InputFile> input_files, Dictionary<string, List<string>> conditionsBioReps, bool include_imputation, bool use_bft)
        {
            DataTable results = new DataTable();
            results.Columns.Add("Proteoform ID", typeof(string));
            foreach (KeyValuePair<string, List<string>> condition_bioreps in conditionsBioReps)
            {
                foreach (string biorep in condition_bioreps.Value)
                {
                    if (use_bft)
                    {
                        HashSet<Tuple<string, string>> frac_tech = new HashSet<Tuple<string, string>>();
                        frac_tech = new HashSet<Tuple<string, string>>(input_files.Where(f => (f.lt_condition == condition_bioreps.Key || f.hv_condition == condition_bioreps.Key) && f.biological_replicate == biorep).Select(f => new Tuple<string, string>(f.fraction, f.technical_replicate)));
                        foreach (Tuple<string, string> ft in frac_tech)
                        {
                            results.Columns.Add(condition_bioreps.Key + "_" + biorep + "_" + ft.Item1 + "_" + ft.Item2, typeof(double));
                        }
                    }
                    else
                    {
                        results.Columns.Add(condition_bioreps.Key + "_" + biorep, typeof(double));
                    }
                }
            }

            foreach (ExperimentalProteoform pf in proteoforms)
            {
                DataRow row = results.NewRow();
                row["Proteoform ID"] = pf.accession;
                foreach (KeyValuePair<string, List<string>> condition_bioreps in conditionsBioReps)
                {
                    foreach (string biorep in condition_bioreps.Value)
                    {
                        if (use_bft)
                        {
                            foreach (InputFile f in input_files.Where(f => (f.lt_condition == condition_bioreps.Key || f.hv_condition == condition_bioreps.Key) && f.biological_replicate == biorep))
                            {
                                pf.quant.allBftIntensities.TryGetValue(new Tuple<InputFile, string>(f, condition_bioreps.Key), out BiorepFractionTechrepIntensity bft);
                                double value = bft != null ? !bft.imputed || include_imputation ? bft.intensity_sum : double.NaN : double.NaN;
                                row[condition_bioreps.Key + "_" + biorep + "_" + f.fraction + "_" + f.technical_replicate] = value;
                            }
                        }
                        else
                        {
                            pf.quant.allIntensities.TryGetValue(new Tuple<string, string>(condition_bioreps.Key, biorep), out BiorepIntensity br);
                            double value = br != null ? !br.imputed || include_imputation ? br.intensity_sum : double.NaN : double.NaN;
                            row[condition_bioreps.Key + "_" + biorep] = value;
                        }
                    }
                }
                results.Rows.Add(row);
            }

            StringBuilder result_string = new StringBuilder();
            string header = "";
            foreach (DataColumn column in results.Columns)
            {
                header += column.ColumnName + "\t";
            }
            result_string.AppendLine(header);
            foreach (DataRow row in results.Rows)
            {
                result_string.AppendLine(String.Join("\t", row.ItemArray));
            }
            return result_string.ToString();
        }

        #endregion Public Methods

    }
}
