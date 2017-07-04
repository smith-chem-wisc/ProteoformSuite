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
            using (StreamWriter writer = new StreamWriter(Path.Combine(SaveState.lollipop.results_folder, "summary_" + timestamp + ".txt")))
                writer.Write(generate_full_report());
        }

        private static void save_dataframe(string directory, string timestamp)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(SaveState.lollipop.results_folder, "results_" + timestamp + ".tsv")))
                writer.Write(results_dataframe());
        }

        private static void save_cytoscripts(string directory, string timestamp)
        {
            string message = "";

            message += CytoscapeScript.write_cytoscape_script(SaveState.lollipop.target_proteoform_community.families, SaveState.lollipop.target_proteoform_community.families,
                    SaveState.lollipop.results_folder, "AllFamilies_", timestamp,
                    false, 
                    true, true,
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
            message += Environment.NewLine;

            if (SaveState.lollipop.qVals.Count > 0)
            {
                message += CytoscapeScript.write_cytoscape_script(SaveState.lollipop.target_proteoform_community.families, SaveState.lollipop.target_proteoform_community.families,
                    SaveState.lollipop.results_folder, "AllQuantFamilies_", timestamp,
                    true,
                    true, true, 
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
                message += Environment.NewLine;

                message += CytoscapeScript.write_cytoscape_script(SaveState.lollipop.getInterestingFamilies(SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.minProteoformFoldChange, SaveState.lollipop.minProteoformFDR, SaveState.lollipop.minProteoformIntensity).Distinct().ToList(), SaveState.lollipop.target_proteoform_community.families,
                    SaveState.lollipop.results_folder, "SignificantChanges_", timestamp,
                    true, 
                    true, true, 
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
                message += Environment.NewLine;
            }

            foreach (GoTermNumber gtn in SaveState.lollipop.goTermNumbers.Where(g => g.by < (double)SaveState.lollipop.minProteoformFDR).ToList())
            {
                message += CytoscapeScript.write_cytoscape_script(new GoTermNumber[] { gtn }, SaveState.lollipop.target_proteoform_community.families,
                    SaveState.lollipop.results_folder, gtn.Aspect.ToString() + gtn.Description.Replace(" ", "_") + "_", timestamp,
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
                () => save_summary(SaveState.lollipop.results_folder, timestamp),
                () => save_dataframe(SaveState.lollipop.results_folder, timestamp),
                () => save_cytoscripts(SaveState.lollipop.results_folder, timestamp)
            );
        }

        public static string generate_full_report()
        {
            return
                counts() +
                proteins_of_significance() +
                go_terms_of_significance() +
                loaded_files_report();
        }

        public static string loaded_files_report()
        {
            string header = "DECONVOLUTION RESULTS FILES AND PROTEIN DATABASE FILES" + Environment.NewLine;
            string report = "";
            foreach (Purpose p in SaveState.lollipop.input_files.Select(f => f.purpose).Distinct())
            {
                report += p.ToString() + ":" + Environment.NewLine + String.Join(Environment.NewLine, SaveState.lollipop.get_files(SaveState.lollipop.input_files, p).Select(f => f.filename + f.extension + "\t" + f.complete_path)) + Environment.NewLine + Environment.NewLine;
            }
            return header + report;
        }

        public static string counts()
        {
            string report = "";

            report += SaveState.lollipop.raw_experimental_components.Count.ToString() + "\tRaw Experimental Components" + Environment.NewLine;
            report += SaveState.lollipop.raw_quantification_components.Count.ToString() + "\tRaw Quantitative Components" + Environment.NewLine;
            report += SaveState.lollipop.raw_neucode_pairs.Count.ToString() + "\tRaw NeuCode Pairs" + Environment.NewLine;
            report += SaveState.lollipop.raw_neucode_pairs.Count(nc => nc.accepted).ToString() + "\tAccepted NeuCode Pairs" + Environment.NewLine;
            report += Environment.NewLine;

            report += SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Length.ToString() + "\tExperimental Proteoforms" + Environment.NewLine;
            report += SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.accepted).ToString() + "\tAccepted Experimental Proteoforms" + Environment.NewLine;
            report += SaveState.lollipop.theoretical_database.theoretical_proteins.Sum(kv => kv.Value.Length).ToString() + "\tTheoretical Proteins" + Environment.NewLine;
            report += SaveState.lollipop.theoretical_database.expanded_proteins.Length + "\tExpanded Theoretical Proteins" + Environment.NewLine;
            report += SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length.ToString() + "\tTheoretical Proteoforms" + Environment.NewLine;
            report += Environment.NewLine;

            report += SaveState.lollipop.et_peaks.Count.ToString() + "\tExperimental-Theoretical Peaks" + Environment.NewLine;
            report += SaveState.lollipop.et_relations.Count.ToString() + "\tExperimental-Theoretical Pairs" + Environment.NewLine;
            report += SaveState.lollipop.et_peaks.Count(p => p.Accepted).ToString() + "\tAccepted Experimental-Theoretical Peaks" + Environment.NewLine;
            report += SaveState.lollipop.et_relations.Count(r => r.Accepted).ToString() + "\tAccepted Experimental-Theoretical Pairs" + Environment.NewLine;
            report += SaveState.lollipop.ed_relations.Count <= 0 ? Environment.NewLine : SaveState.lollipop.ed_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-Decoy Pairs" + Environment.NewLine;
            report += Environment.NewLine;

            report += SaveState.lollipop.ee_peaks.Count.ToString() + "\tExperimental-Experimental Peaks" + Environment.NewLine;
            report += SaveState.lollipop.ee_relations.Count.ToString() + "\tExperimental-Experimental Pairs" + Environment.NewLine;
            report += SaveState.lollipop.ee_peaks.Count(p => p.Accepted).ToString() + "\tAccepted Experimental-Experimental Peaks" + Environment.NewLine;
            report += SaveState.lollipop.ee_relations.Count(r => r.Accepted).ToString() + "\tAccepted Experimental-Experimental Pairs" + Environment.NewLine;
            report += SaveState.lollipop.ef_relations.Count <= 0 ? Environment.NewLine : SaveState.lollipop.ef_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-False Pairs" + Environment.NewLine;
            report += Environment.NewLine;

            report += proteoform_families_report();

            report += quant_report();

            return report;
        }

        public static string proteoform_families_report()
        {
            string report = "";

            report += SaveState.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count > 1).ToString() + "\tProteoform Families" + Environment.NewLine;
            List<ProteoformFamily> identified_families = SaveState.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() == 1).ToList();
            List<ProteoformFamily> ambiguous_families = SaveState.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() > 1).ToList();
            List<ProteoformFamily> unidentified_families = SaveState.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() == 0 && f.proteoforms.Count > 1).ToList();
            report += identified_families.Count.ToString() + "\tIdentified Families (Correspond to 1 " + (ProteoformCommunity.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += identified_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Identified Families" + Environment.NewLine;
            report += ambiguous_families.Count.ToString() + "\tAmbiguous Families (Correspond to > 1 " + (ProteoformCommunity.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += ambiguous_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Ambiguous Families" + Environment.NewLine;
            report += unidentified_families.Count.ToString() + "\tUnidentified Families (Correspond to no " + (ProteoformCommunity.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += unidentified_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Unidentified Families" + Environment.NewLine;
            report += SaveState.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count == 1).ToString() + "\tOrphaned Experimental Proteoforms (Not joined with another proteoform)" + Environment.NewLine;
            report += Environment.NewLine;

            int raw_components_in_fams = identified_families.Concat(ambiguous_families).Concat(unidentified_families).Sum(f => f.experimental_proteoforms.Sum(e => e.lt_verification_components.Count + e.hv_verification_components.Count));
            report += raw_components_in_fams + "\tRaw Experimental Components in Families" + Environment.NewLine;
            report += SaveState.lollipop.raw_experimental_components.Count > 0 ?
                Math.Round(100 * ((double)raw_components_in_fams / (double)SaveState.lollipop.raw_experimental_components.Count), 2) + "\t% of Raw Experimental Components in Families" + Environment.NewLine :
                "N/A\t% of Raw Experimental Components in Families" + Environment.NewLine;

            int raw_quant_components_in_fams = identified_families.Concat(ambiguous_families).Concat(unidentified_families).Sum(f => f.experimental_proteoforms.Sum(e => e.lt_quant_components.Count + e.hv_quant_components.Count));
            report += raw_quant_components_in_fams + "\tRaw Quantitative Components in Families" + Environment.NewLine;
            report += SaveState.lollipop.raw_experimental_components.Count > 0 ?
                Math.Round(100 * ((double)raw_quant_components_in_fams / (double)SaveState.lollipop.raw_experimental_components.Count), 2) + "\t% of Raw Quantitative Components in Families" + Environment.NewLine :
                "N/A\t% of Raw Quantitative Components in Families" + Environment.NewLine;
            report += Environment.NewLine;


            int identified_exp_proteoforms = SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && (SaveState.lollipop.count_adducts_as_identifications || !e.adduct) && e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown) == 0);
            double avg_identified_decoy_proteoforms = SaveState.lollipop.decoy_proteoform_communities.Count > 0 ?
                SaveState.lollipop.decoy_proteoform_communities.Average(v => v.Value.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && (SaveState.lollipop.count_adducts_as_identifications || !e.adduct) && e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown) == 0)) : 
                -1;
            report += identified_exp_proteoforms.ToString() + "\tIdentified Experimental Proteoforms" + Environment.NewLine;
            report += SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.fragmented && e.linked_proteoform_references != null && (SaveState.lollipop.count_adducts_as_identifications || !e.adduct) && e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown) == 0) + "\tIdentified Experimentals Fragmented" + Environment.NewLine;
            report += SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.topdown_identified && e.linked_proteoform_references != null && (SaveState.lollipop.count_adducts_as_identifications || !e.adduct) && e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown) == 0) + "\tIdentified Experimentals Also Identified By Top-Down" + Environment.NewLine;

            report += (avg_identified_decoy_proteoforms > 0 ? Math.Round(avg_identified_decoy_proteoforms, 2).ToString() : "N/A")
                    + "\tAverage Identified Experimental Proteoforms by Decoys" + Environment.NewLine;
            report += SaveState.lollipop.decoy_proteoform_communities.Values.SelectMany(v => v.families).Count() > 0 && identified_exp_proteoforms > 0 ?
                Math.Round(avg_identified_decoy_proteoforms / identified_exp_proteoforms, 4).ToString() + "\tProteoform FDR" + Environment.NewLine :
                "N/A\tProteoform FDR" + Environment.NewLine;
            report += Environment.NewLine;

            return report;
        }

        public static string quant_report()
        {
            string report = "";

            report += SaveState.lollipop.satisfactoryProteoforms.Count.ToString() + "\tQuantified Experimental Proteoforms (Threshold for Quantification: " + SaveState.lollipop.minBiorepsWithObservations.ToString() + " = " + SaveState.lollipop.observation_requirement + ")" + Environment.NewLine;
            report += SaveState.lollipop.satisfactoryProteoforms.Count(p => p.quant.significant).ToString() + "\tExperimental Proteoforms with Significant Change (Threshold for Significance: Log2FoldChange > " + SaveState.lollipop.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + SaveState.lollipop.minProteoformIntensity.ToString() + ", & Q-Value < " + SaveState.lollipop.minProteoformFDR.ToString() + ")" + Environment.NewLine;
            report += SaveState.lollipop.selectAverageIntensity.ToString() + "\tAverage Intensity Quantified Experimental Proteoform Observations" + Environment.NewLine;
            report += SaveState.lollipop.selectStDev.ToString() + "\tIntensity Standard Deviation for Quantified Experimental Proteoform" + Environment.NewLine;
            report += SaveState.lollipop.getInterestingFamilies(SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.minProteoformFoldChange, SaveState.lollipop.minProteoformFDR, SaveState.lollipop.minProteoformIntensity).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
            report += SaveState.lollipop.inducedOrRepressedProteins.Count.ToString() + "\tIdentified Proteins with Significant Change" + Environment.NewLine;
            report += SaveState.lollipop.goTermNumbers.Count(g => g.by < (double)SaveState.lollipop.minProteoformFDR).ToString() + "\tGO Terms of Significance (Benjimini-Yekeulti p-value < " + SaveState.lollipop.minProteoformFDR.ToString() + "): " + Environment.NewLine;
            report += Environment.NewLine;

            // Venn Diagram of quantifiable proteoforms
            List<string> conditions = SaveState.lollipop.ltConditionsBioReps.Keys.Concat(SaveState.lollipop.hvConditionsBioReps.Keys).ToList();
            foreach (string condition in conditions)
            {
                SaveState.lollipop.ltConditionsBioReps.TryGetValue(condition, out List<int> ltbioreps);
                SaveState.lollipop.hvConditionsBioReps.TryGetValue(condition, out List<int> hvbioreps);
                List<int> combined_bioreps = ltbioreps == null && hvbioreps == null ?
                    new List<int>() :
                    ltbioreps == null ? hvbioreps.Distinct().ToList() :
                    hvbioreps == null ? ltbioreps.Distinct().ToList() :
                        ltbioreps.Concat(hvbioreps).Distinct().ToList();

                report += combined_bioreps.Count.ToString() + "\tBiological Replicates for " + condition + Environment.NewLine;

                for (int start = 0; start < combined_bioreps.Count; start++)
                {
                    int exp_prots_with_these_bioreps = SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Contains(combined_bioreps[start]));
                    int exp_prots_with_these_bioreps_exclusive = SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Contains(combined_bioreps[start]) && e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Distinct().Count() == 1);
                    report += exp_prots_with_these_bioreps + "\tExperimental Proteoforms Observed in " + condition + ", Biological Replicate #" + combined_bioreps[start].ToString() + Environment.NewLine;
                    report += exp_prots_with_these_bioreps_exclusive + "\tExperimental Proteoforms Observed Exclusively in " + condition + ", Biological Replicate #" + combined_bioreps[start].ToString() + Environment.NewLine;

                    for (int end = start; end < combined_bioreps.Count; end++)
                    {
                        for (int between = end; between > start; between--)
                        {
                            List<int> bioreps_of_interest = new List<int> { start }.Concat(Enumerable.Range(between, end - between + 1)).Distinct().Select(idx => combined_bioreps[idx]).ToList();
                            exp_prots_with_these_bioreps = SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => bioreps_of_interest.All(x => e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Contains(x)));
                            exp_prots_with_these_bioreps_exclusive = SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => bioreps_of_interest.All(x => e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Contains(x)) && e.biorepIntensityList.Where(br => br.condition == condition).Select(br => br.biorep).Distinct().Count() == bioreps_of_interest.Distinct().Count());
                            report += exp_prots_with_these_bioreps + "\tExperimental Proteoforms Observed in " + condition + ", Biological Replicates #" + String.Join(" #", bioreps_of_interest) + Environment.NewLine;
                            report += exp_prots_with_these_bioreps_exclusive + "\tExperimental Proteoforms Observed Exclusively in " + condition + ", Biological Replicates #" + String.Join(" #", bioreps_of_interest) + Environment.NewLine;
                        }
                    }
                }

                int exp_prots_not_in_condition = SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.biorepIntensityList.All(br => br.condition != condition));
                report += exp_prots_not_in_condition + "\tExperimental Proteoforms Were Not Observed in " + condition + Environment.NewLine;
                report += Environment.NewLine;
            }
            report += Environment.NewLine;

            return report;
        }

        public static string proteins_of_significance()
        {
            return "Identified Proteins with Significant Change: " + Environment.NewLine
                + String.Join(Environment.NewLine, SaveState.lollipop.inducedOrRepressedProteins.Select(p => p.Accession).Distinct().OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
        }

        public static string go_terms_of_significance()
        {
            return "GO Terms of Significance (Benjimini-Yekeulti p-value < " + SaveState.lollipop.minProteoformFDR.ToString() + "): " + Environment.NewLine
                + String.Join(Environment.NewLine, SaveState.lollipop.goTermNumbers.Where(g => g.by < (double)SaveState.lollipop.minProteoformFDR).Select(g => g.ToString()).OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
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
            results.Columns.Add((SaveState.lollipop.numerator_condition == "" ? "Condition #1" : SaveState.lollipop.numerator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add((SaveState.lollipop.denominator_condition == "" ? "Condition #2" : SaveState.lollipop.denominator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add("Statistically Significant", typeof(bool));

            foreach (ExperimentalProteoform e in SaveState.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms).Where(e => e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown) == 0)
                .Where(e => e.linked_proteoform_references != null && (SaveState.lollipop.count_adducts_as_identifications || !e.adduct))
                .OrderByDescending(e => e.quant.significant ? 1 : 0)
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
                    e.quant.lightIntensitySum,
                    e.quant.heavyIntensitySum,
                    e.quant.significant
                );
            }
            foreach (TopDownProteoform td in SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Where(t => t.linked_proteoform_references != null))
            {

                results.Rows.Add(
                   (td.linked_proteoform_references.First() as TheoreticalProteoform).accession,
                    td.accession,
                    td.linked_proteoform_references.Last().gene_name.ordered_locus,
                    td.linked_proteoform_references.Last().gene_name.primary,
                    td.start_index + " to " + td.stop_index,
                    String.Join("; ", td.ptm_set.ptm_combination.Select(ptm => ptm.modification.id)),
                    td.modified_mass - td.linked_proteoform_references.Last().modified_mass,
                    td.agg_RT,
                    0,
                    0,
                    0,
                    0
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

        #endregion Public Methods

    }
}
