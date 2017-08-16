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

        private static void save_dataframe(TusherAnalysis analysis, string directory, string timestamp)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(Sweet.lollipop.results_folder, "results_" + timestamp + ".tsv")))
                writer.Write(datatable_tostring(results_dataframe(analysis)));
        }

        private static void save_cytoscripts(string directory, string timestamp, IGoAnalysis go_analysis, TusherAnalysis tusher_analysis)
        {
            string message = "";

            message += CytoscapeScript.write_cytoscape_script(Sweet.lollipop.target_proteoform_community.families, Sweet.lollipop.target_proteoform_community.families,
                    Sweet.lollipop.results_folder, "AllFamilies_", timestamp,
                    null, 
                    true, true,
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    Lollipop.gene_centric_families, Lollipop.preferred_gene_label);
            message += Environment.NewLine;

            if (Sweet.lollipop.qVals.Count > 0)
            {
                message += CytoscapeScript.write_cytoscape_script(Sweet.lollipop.target_proteoform_community.families, Sweet.lollipop.target_proteoform_community.families,
                    Sweet.lollipop.results_folder, "AllQuantFamilies_", timestamp,
                    go_analysis as IGoAnalysis,
                    true, true, 
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    Lollipop.gene_centric_families, Lollipop.preferred_gene_label);
                message += Environment.NewLine;

                message += CytoscapeScript.write_cytoscape_script(Sweet.lollipop.getInterestingFamilies(tusher_analysis, Sweet.lollipop.satisfactoryProteoforms, go_analysis.GoAnalysis.minProteoformFoldChange, go_analysis.GoAnalysis.maxGoTermFDR, go_analysis.GoAnalysis.minProteoformIntensity).Distinct().ToList(), Sweet.lollipop.target_proteoform_community.families,
                    Sweet.lollipop.results_folder, "SignificantChanges_", timestamp,
                    go_analysis as IGoAnalysis, 
                    true, true, 
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    Lollipop.gene_centric_families, Lollipop.preferred_gene_label);
                message += Environment.NewLine;
            }

            foreach (GoTermNumber gtn in go_analysis.GoAnalysis.goTermNumbers.Where(g => g.by < (double)go_analysis.GoAnalysis.maxGoTermFDR).ToList())
            {
                message += CytoscapeScript.write_cytoscape_script(new GoTermNumber[] { gtn }, Sweet.lollipop.target_proteoform_community.families,
                    Sweet.lollipop.results_folder, gtn.Aspect.ToString() + gtn.Description.Replace(" ", "_").Replace(@"\", "_").Replace(@"/", "_") + "_", timestamp,
                    go_analysis as IGoAnalysis, true, true, 
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    Lollipop.gene_centric_families, Lollipop.preferred_gene_label);
                message += Environment.NewLine;
            }
            message += "Remember to install the package \"enhancedGraphics\" under App -> App Manager to view piechart nodes for quantitative data";
        }

        #endregion Private Methods

        #region Public Methods

        public static void save_all(string directory, string timestamp, IGoAnalysis go_analysis, TusherAnalysis tusher_analysis)
        {
            Parallel.Invoke
            (
                () => save_summary(Sweet.lollipop.results_folder, timestamp),
                () => save_dataframe(tusher_analysis, Sweet.lollipop.results_folder, timestamp),
                () => save_cytoscripts(Sweet.lollipop.results_folder, timestamp, go_analysis, tusher_analysis)
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

        public static string actions()
        {
            string header = "USER ACTIONS" + Environment.NewLine;
            string report = String.Join(Environment.NewLine, Sweet.save_actions) + Environment.NewLine + Environment.NewLine;
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

        public static string raw_components_report()
        {
            string report = "";

            report += Sweet.lollipop.unprocessed_exp_components.ToString() + "\tUnprocessed Raw Experimental Components" + Environment.NewLine;
            report += Sweet.lollipop.raw_experimental_components.Count.ToString() + "\tRaw Experimental Components" + Environment.NewLine;
            report += Sweet.lollipop.missed_mono_merges_exp.ToString() + "\tMissed Monoisotopic Raw Experimental Components Merged" + Environment.NewLine;
            report += Sweet.lollipop.harmonic_merges_exp.ToString() + "\tHarmonic Raw Experimental Components Merged" + Environment.NewLine;
            report += Environment.NewLine;
            report += Sweet.lollipop.unprocessed_quant_components.ToString() + "\tUnprocessed Raw Quantitative Components" + Environment.NewLine;
            report += Sweet.lollipop.raw_quantification_components.Count.ToString() + "\tRaw Quantitative Components" + Environment.NewLine;
            report += Sweet.lollipop.missed_mono_merges_quant.ToString() + "\tMissed Monoisotopic Raw Quantitative Components Merged" + Environment.NewLine;
            report += Sweet.lollipop.harmonic_merges_quant.ToString() + "\tHarmonic Raw Quantitative Components Merged" + Environment.NewLine;
            report += Environment.NewLine;

            return report;

        }

        public static string counts()
        {
            string report = "";

            report += raw_components_report();

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

            report += Sweet.lollipop.top_down_hits.Count.ToString() + "\tTop-Down Hits" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Length.ToString() + "\tTop-Down Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.relationships.Any(r => r.RelationType == ProteoformComparison.TopdownExperimental)).ToString() + "\tExperimental Proteoforms with Top-Down Pairs" + Environment.NewLine;
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
            report += identified_families.Count.ToString() + "\tIdentified Families (Correspond to 1 " + (Lollipop.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += identified_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Identified Families" + Environment.NewLine;
            report += ambiguous_families.Count.ToString() + "\tAmbiguous Families (Correspond to > 1 " + (Lollipop.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += ambiguous_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Ambiguous Families" + Environment.NewLine;
            report += unidentified_families.Count.ToString() + "\tUnidentified Families (Correspond to no " + (Lollipop.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
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

            int identified_exp_proteoforms = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct) && e.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental) == 0);
            double avg_identified_decoy_proteoforms = Sweet.lollipop.decoy_proteoform_communities.Count > 0 ?
                Sweet.lollipop.decoy_proteoform_communities.Average(v => v.Value.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct) && e.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental) == 0)) : 
                -1;
            report += identified_exp_proteoforms.ToString() + "\tIdentified Experimental Proteoforms" + Environment.NewLine;
            report += (avg_identified_decoy_proteoforms > 0 ? Math.Round(avg_identified_decoy_proteoforms, 2).ToString() : "N/A")
                    + "\tAverage Identified Experimental Proteoforms by Decoys" + Environment.NewLine;
            if (avg_identified_decoy_proteoforms > 0)
                report += String.Join(", ", Sweet.lollipop.decoy_proteoform_communities.Select(v => v.Value.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct) && e.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental) == 0)))
                    + "\tIndividual Decoy Community Identified Experimental Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.decoy_proteoform_communities.Values.SelectMany(v => v.families).Count() > 0 && identified_exp_proteoforms > 0 ?
                Math.Round(avg_identified_decoy_proteoforms / identified_exp_proteoforms, 4).ToString() + "\tProteoform FDR" + Environment.NewLine :
                "N/A\tProteoform FDR" + Environment.NewLine;
            report += identified_exp_proteoforms + Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Length
            + "\tTotal Identified Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count(t => t.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental) == 0).ToString()
                + "\tTop-Dop Proteoforms Without Experimental Match" + Environment.NewLine;
            report += Environment.NewLine;

            return report;
        }

        public static string quant_report()
        {
            string report = "";
            string tusher_significance_parenthetical = "(Offset of " + Math.Round(Sweet.lollipop.offsetTestStatistics, 1).ToString() + " from d(i) = dE(i) line";
            if (Sweet.lollipop.useFoldChangeCutoff)
            {
                tusher_significance_parenthetical += ", " + Sweet.lollipop.fold_change_conjunction + " > " + Sweet.lollipop.foldChangeCutoff.ToString() + " fold-change" + (Sweet.lollipop.useAveragePermutationFoldChange ? ", on average among biological replicates" : Sweet.lollipop.useBiorepPermutationFoldChange ? " in " + Sweet.lollipop.minBiorepsWithFoldChange + " biological replicates at minimum" : "");
            }
            tusher_significance_parenthetical += ")";

            report += "QUANTIFICATION" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count.ToString() + "\tQuantified Experimental Proteoforms (Threshold for Quantification: " + Sweet.lollipop.minBiorepsWithObservations.ToString() + " = " + Sweet.lollipop.observation_requirement + ")" + Environment.NewLine;
            report += Environment.NewLine;

            report += "TUSHER ANALYSIS (" + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count + " Permutations)" + Environment.NewLine; ;
            report += Math.Round(Sweet.lollipop.TusherAnalysis1.QuantitativeDistributions.selectAverageIntensity, 4).ToString() + "\tAverage Log2 Intensity Quantified Experimental Proteoform Observations" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis1.QuantitativeDistributions.selectStDev, 2).ToString() + "\tLog2 Intensity Standard Deviation for Quantified Experimental Proteoform" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant).ToString() + "\tExperimental Proteoforms with Significant Change (Threshold for Significance: Log2FoldChange > " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformIntensity.ToString() + ", & Q-Value < " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR.ToString() + ")" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant_relative_difference).ToString() + "\tExperimental Proteoforms with Significant Change by Relative Difference " + tusher_significance_parenthetical + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant_fold_change).ToString() + "\tExperimental Proteoforms with Significant Change by Fold Change " + tusher_significance_parenthetical + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis1.relativeDifferenceFDR, 4).ToString() + "\tFDR for Significance Conclusion " + tusher_significance_parenthetical + Environment.NewLine;
            report += Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.TusherAnalysis1 as TusherAnalysis, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformFoldChange, Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR, Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformIntensity).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.TusherAnalysis1.inducedOrRepressedProteins.Count.ToString() + "\tIdentified Proteins with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.TusherAnalysis1.GoAnalysis.goTermNumbers.Count(g => g.by < (double)Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR).ToString() + "\tGO Terms of Significance (Benjimini-Yekeulti p-value < " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR.ToString() + "; using Experimental Proteoforms that satisified the criteria:  Log2FoldChange > " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformIntensity.ToString() + ", & Q-Value < " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR.ToString() + ")" + Environment.NewLine;
            report += Environment.NewLine;

            report += "TUSHER ANALYSIS (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count + " Permutations)" + Environment.NewLine; ;
            report += Math.Round(Sweet.lollipop.TusherAnalysis2.QuantitativeDistributions.selectAverageIntensity, 4).ToString() + "\tAverage Log2 Intensity Quantified Experimental Proteoform Observations" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis2.QuantitativeDistributions.selectStDev, 2).ToString() + "\tLog2 Intensity Standard Deviation for Quantified Experimental Proteoform" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues2.significant).ToString() + "\tExperimental Proteoforms with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues2.significant_relative_difference).ToString() + "\tExperimental Proteoforms with Significant Change by Relative Difference " + tusher_significance_parenthetical + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues2.significant_fold_change).ToString() + "\tExperimental Proteoforms with Significant Change by Fold Change " + tusher_significance_parenthetical + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis2.relativeDifferenceFDR, 4).ToString() + "\tFDR for Significance Conclusion " + tusher_significance_parenthetical + Environment.NewLine;
            report += Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.TusherAnalysis2 as TusherAnalysis, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.TusherAnalysis2.GoAnalysis.minProteoformFoldChange, Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR, Sweet.lollipop.TusherAnalysis2.GoAnalysis.minProteoformIntensity).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.TusherAnalysis2.inducedOrRepressedProteins.Count.ToString() + "\tIdentified Proteins with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.TusherAnalysis2.GoAnalysis.goTermNumbers.Count(g => g.by < (double)Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR).ToString() + "\tGO Terms of Significance (Benjimini-Yekeulti p-value < " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR.ToString() + "; using Experimental Proteoforms that satisified the criteria:  Log2FoldChange > " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.minProteoformIntensity.ToString() + ", & Q-Value < " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR.ToString() + ")" + Environment.NewLine;
            report += Environment.NewLine;

            report += "LOG2 FOLD CHANGE ANALYSIS" + Environment.NewLine; ;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.Log2FoldChangeValues.significant).ToString() + "\tExperimental Proteoforms with Significant Change (Threshold for Significance: Benjimini-Hochberg Q-Value < " + Sweet.lollipop.Log2FoldChangeAnalysis.benjiHoch_fdr + ")" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.Log2FoldChangeAnalysis.benjiHoch_fdr, 4).ToString() + "\tFDR for Significance Conclusion (Offset of " + Math.Round(Sweet.lollipop.offsetTestStatistics, 1).ToString() + " from d(i) = dE(i) line)" + Environment.NewLine;
            report += Sweet.lollipop.getInterestingFamilies(null, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.minProteoformFoldChange, Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.maxGoTermFDR, Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.minProteoformIntensity).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.Log2FoldChangeAnalysis.inducedOrRepressedProteins.Count.ToString() + "\tIdentified Proteins with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.goTermNumbers.Count(g => g.by < (double)Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.maxGoTermFDR).ToString() + "\tGO Terms of Significance (Benjimini-Yekeulti p-value < " + Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.maxGoTermFDR.ToString() + "; using Experimental Proteoforms that satisified the criteria:  Log2FoldChange > " + Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.minProteoformIntensity.ToString() + ", & Q-Value < " + Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.maxGoTermFDR.ToString() + ")" + Environment.NewLine;
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
            string result = "";
            result += "Identified Proteins with Significant Change: (" + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count + " Permutations)" + Environment.NewLine
                + String.Join(Environment.NewLine, Sweet.lollipop.TusherAnalysis1.inducedOrRepressedProteins.Select(p => p.Accession).Distinct().OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            result += "Identified Proteins with Significant Change: (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count + " Permutations)" + Environment.NewLine
                + String.Join(Environment.NewLine, Sweet.lollipop.TusherAnalysis2.inducedOrRepressedProteins.Select(p => p.Accession).Distinct().OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            result += "Identified Proteins with Significant Change: (by log2 fold change analysis)" + Environment.NewLine
                + String.Join(Environment.NewLine, Sweet.lollipop.Log2FoldChangeAnalysis.inducedOrRepressedProteins.Select(p => p.Accession).Distinct().OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            return result;
        }

        public static string go_terms_of_significance()
        {
            string result = "";
            result += "GO Terms of Significance, Tusher Analysis with " + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count + " permutations (Benjimini-Yekeulti p-value < " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR.ToString() + "): " + Environment.NewLine
                + String.Join(Environment.NewLine, Sweet.lollipop.TusherAnalysis1.GoAnalysis.goTermNumbers.Where(g => g.by < (double)Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR).Select(g => g.ToString()).OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            result += "GO Terms of Significance, Tusher Analysis with " + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count + " permutations (Benjimini-Yekeulti p-value < " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR.ToString() + "): " + Environment.NewLine
                + String.Join(Environment.NewLine, Sweet.lollipop.TusherAnalysis2.GoAnalysis.goTermNumbers.Where(g => g.by < (double)Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR).Select(g => g.ToString()).OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            result += "GO Terms of Significance, Log2 Fold Change Analysis with " + Sweet.lollipop.Log2FoldChangeAnalysis.benjiHoch_fdr.ToString() + " FDR (Benjimini-Yekeulti p-value < " + Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.maxGoTermFDR.ToString() + "): " + Environment.NewLine
                + String.Join(Environment.NewLine, Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.goTermNumbers.Where(g => g.by < (double)Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.maxGoTermFDR).Select(g => g.ToString()).OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            return result;
        }

        private static TusherValues get_tusher_values(QuantitativeProteoformValues q, TusherAnalysis analysis)
        {
            return analysis as TusherAnalysis1 != null ? q.TusherValues1 as TusherValues : q.TusherValues2 as TusherValues;
        }

        public static string datatable_tostring(DataTable results)
        {
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

        public static DataTable results_dataframe(TusherAnalysis analysis)
        {
            DataTable results = new DataTable();
            results.Columns.Add("Proteoform ID", typeof(string));
            results.Columns.Add("Proteoform Description", typeof(string));
            results.Columns.Add("Aggregated Observation ID", typeof(string));
            results.Columns.Add("SGD ID", typeof(string));
            results.Columns.Add("Gene Name", typeof(string));
            results.Columns.Add("Protein Fragment Type", typeof(string));
            results.Columns.Add("PTM Type", typeof(string));
            results.Columns.Add("Mass Difference", typeof(double));
            results.Columns.Add("Proteoform Mass");
            results.Columns.Add("Retention Time", typeof(double));
            results.Columns.Add("Aggregated Intensity", typeof(double));
            results.Columns.Add((Sweet.lollipop.numerator_condition == "" ? "Condition #1" : Sweet.lollipop.numerator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add((Sweet.lollipop.denominator_condition == "" ? "Condition #2" : Sweet.lollipop.denominator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add("Statistically Significant", typeof(bool));

            foreach (ExperimentalProteoform e in Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms)
                .Where(e => e.linked_proteoform_references != null && e.relationships.Count(r => r.RelationType == ProteoformComparison.TopdownExperimental) == 0 && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct))
                .OrderByDescending(e => (Sweet.lollipop.significance_by_log2FC ? e.quant.Log2FoldChangeValues.significant : get_tusher_values(e.quant, analysis).significant) ? 1 : 0)
                .ThenBy(e => (e.linked_proteoform_references.First() as TheoreticalProteoform).accession)
                .ThenBy(e => e.ptm_set.ptm_combination.Count))
            {
                results.Rows.Add(
                    (e.linked_proteoform_references.First() as TheoreticalProteoform).accession,
                    (e.linked_proteoform_references.First() as TheoreticalProteoform).description,
                    e.accession,
                    e.linked_proteoform_references.Last().gene_name.ordered_locus,
                    e.linked_proteoform_references.Last().gene_name.primary,
                    (e.linked_proteoform_references.First() as TheoreticalProteoform).fragment,
                    String.Join("; ", e.ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id)),
                    e.modified_mass - e.linked_proteoform_references.Last().modified_mass,
                    e.modified_mass,
                    e.agg_rt,
                    e.agg_intensity,
                    get_tusher_values(e.quant, analysis).numeratorIntensitySum,
                    get_tusher_values(e.quant, analysis).denominatorIntensitySum,
                    Sweet.lollipop.significance_by_log2FC ? e.quant.Log2FoldChangeValues.significant : get_tusher_values(e.quant, analysis).significant
                );
            }
            foreach (TopDownProteoform td in Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Where(t => t.linked_proteoform_references != null))
            {

                results.Rows.Add(
                   (td.linked_proteoform_references.First() as TheoreticalProteoform).accession,
                   td.name,
                    td.accession,
                    td.linked_proteoform_references.Last().gene_name.ordered_locus,
                    td.linked_proteoform_references.Last().gene_name.primary,
                    td.begin + " to " + td.end,
                    td.ptm_description,
                    td.modified_mass - td.linked_proteoform_references.Last().modified_mass,
                    td.modified_mass,
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
            return results;
        }

        public static DataTable biological_replicate_intensities(IGoAnalysis analysis, IEnumerable<ExperimentalProteoform> proteoforms, List<InputFile> input_files, Dictionary<string, List<string>> conditionsBioReps, bool include_imputation)
        {
            DataTable results = new DataTable();
            List<Tuple<string, string>> biorep_techreps = Sweet.lollipop.get_files(input_files, Purpose.Quantification).Select(x => new Tuple<string, string>(x.biological_replicate, x.technical_replicate)).Distinct().ToList();

            // Add columns
            results.Columns.Add("Proteoform ID", typeof(string));
            foreach (KeyValuePair<string, List<string>> condition_bioreps in conditionsBioReps)
            {
                foreach (string biorep in condition_bioreps.Value)
                {
                    if (analysis as TusherAnalysis1 != null)
                    {
                        results.Columns.Add(condition_bioreps.Key + "_" + biorep, typeof(double)); // biorep intensities in TusherAnalysis1
                    }
                    else if (analysis as TusherAnalysis2 != null)
                    {
                        foreach (string techrep in biorep_techreps.Where(x => x.Item1 == biorep).Select(x => x.Item2).Distinct().ToList())
                        {
                            results.Columns.Add(condition_bioreps.Key + "_" + biorep + "_" + techrep, typeof(double)); // biorep-techrep intensities in TusherAnalysis2
                        }
                    }
                    else if (analysis as Log2FoldChangeAnalysis != null)
                    {
                        foreach (InputFile f in input_files.Where(f => (f.lt_condition == condition_bioreps.Key || f.hv_condition == condition_bioreps.Key) && f.biological_replicate == biorep))
                        {
                            results.Columns.Add(condition_bioreps.Key + "_" + biorep + "_" + f.fraction + "_" + f.technical_replicate, typeof(double)); // biorep-fraction-techrep intensities in Log2FoldChangeAnalysis
                        }
                    }
                }
            }
            results.Columns.Add("Subtracted Average", typeof(double));

            // Add data for each proteoform
            foreach (ExperimentalProteoform pf in proteoforms.ToList())
            {
                DataRow row = results.NewRow();
                row["Proteoform ID"] = pf.accession;
                foreach (KeyValuePair<string, List<string>> condition_bioreps in conditionsBioReps)
                {
                    foreach (string biorep in condition_bioreps.Value)
                    {
                        double value;
                        if (analysis as TusherAnalysis1 != null)  // biorep intensities in TusherAnalysis1
                        {
                            if (include_imputation)
                            {
                                pf.quant.TusherValues1.allIntensities.TryGetValue(new Tuple<string, string>(condition_bioreps.Key, biorep), out BiorepIntensity br);
                                value = br != null ? !br.imputed || include_imputation ? br.intensity_sum : double.NaN : double.NaN;
                            }
                            else
                            {
                                double norm_divisor = Sweet.lollipop.TusherAnalysis1.conditionBiorep_sums[new Tuple<string, string>(condition_bioreps.Key, biorep)] / Sweet.lollipop.TusherAnalysis1.conditionBiorep_sums.Where(kv => kv.Key.Item2 == biorep).Average(kv => kv.Value);
                                value = pf.biorepIntensityList.Where(x => x.condition == condition_bioreps.Key && x.biorep == biorep).Sum(x => x.intensity_sum) / norm_divisor;
                            }
                            row[condition_bioreps.Key + "_" + biorep] = value;
                        }
                        else if (analysis as TusherAnalysis2 != null)  // biorep-techrep intensities in TusherAnalysis1
                        {
                            foreach (string techrep in biorep_techreps.Where(x => x.Item1 == biorep).Select(x => x.Item2).Distinct().ToList())
                            {
                                if (include_imputation)
                                {
                                    pf.quant.TusherValues2.allIntensities.TryGetValue(new Tuple<string, string, string>(condition_bioreps.Key, biorep, techrep), out BiorepTechrepIntensity br);
                                    value = br != null ? !br.imputed || include_imputation ? br.intensity_sum : double.NaN : double.NaN;
                                }
                                else
                                {
                                    double norm_divisor = Sweet.lollipop.TusherAnalysis2.conditionBiorep_sums[new Tuple<string, string>(condition_bioreps.Key, biorep)] / Sweet.lollipop.TusherAnalysis2.conditionBiorep_sums.Where(kv => kv.Key.Item2 == biorep).Average(kv => kv.Value);
                                    value = pf.biorepTechrepIntensityList.Where(x => x.condition == condition_bioreps.Key && x.biorep == biorep && x.techrep == techrep).Sum(x => x.intensity_sum) / norm_divisor;
                                }
                                row[condition_bioreps.Key + "_" + biorep + "_" + techrep] = value;
                            }
                        }
                        else if (analysis as Log2FoldChangeAnalysis != null)  // biorep-fraction-techrep intensities in TusherAnalysis1
                        {
                            foreach (InputFile f in input_files.Where(f => (f.lt_condition == condition_bioreps.Key || f.hv_condition == condition_bioreps.Key) && f.biological_replicate == biorep))
                            {
                                if (include_imputation)
                                {
                                    pf.quant.Log2FoldChangeValues.allBftIntensities.TryGetValue(new Tuple<InputFile, string>(f, condition_bioreps.Key), out BiorepFractionTechrepIntensity bft);
                                    value = bft != null ? !bft.imputed || include_imputation ? bft.intensity_sum : double.NaN : double.NaN;
                                }
                                else
                                {
                                    value = pf.bftIntensityList.Where(x => x.condition == condition_bioreps.Key && x.biorep == biorep && x.input_file.technical_replicate == f.technical_replicate && x.input_file.fraction == f.fraction).Sum(x => x.intensity_sum)
                                        / Sweet.lollipop.Log2FoldChangeAnalysis.conditionBiorepNormalizationDivisors[new Tuple<string, string>(condition_bioreps.Key, f.biological_replicate)];
                                }
                                row[condition_bioreps.Key + "_" + biorep + "_" + f.fraction + "_" + f.technical_replicate] = value;
                            }
                        }
                    }
                }

                double subtracted = analysis as TusherAnalysis1 != null ? pf.quant.TusherValues1.normalization_subtractand : analysis as TusherAnalysis2 != null ? pf.quant.TusherValues2.normalization_subtractand : 0;
                if (subtracted != 0)
                {
                    row["Subtracted Average"] = subtracted;
                }

                results.Rows.Add(row);
            }

            return results;
        }

        #endregion Public Methods

    }
}
