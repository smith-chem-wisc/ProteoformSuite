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
                SaveState.lollipop.qVals.Count > 0, true, true, false,
                CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
            message += Environment.NewLine;

            if (SaveState.lollipop.qVals.Count > 0)
            {
                message += CytoscapeScript.write_cytoscape_script(SaveState.lollipop.getInterestingFamilies(SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.minProteoformFoldChange, SaveState.lollipop.minProteoformFDR, SaveState.lollipop.minProteoformIntensity).Distinct().ToList(), SaveState.lollipop.target_proteoform_community.families,
                    SaveState.lollipop.results_folder, "SignificantChanges_", timestamp,
                    SaveState.lollipop.qVals.Count > 0, true, true, false,
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
                message += Environment.NewLine;
            }

            foreach (GoTermNumber gtn in SaveState.lollipop.goTermNumbers.Where(g => g.by < (double)SaveState.lollipop.minProteoformFDR).ToList())
            {
                message += CytoscapeScript.write_cytoscape_script(new GoTermNumber[] { gtn }, SaveState.lollipop.target_proteoform_community.families,
                    SaveState.lollipop.results_folder, gtn.Aspect.ToString() + gtn.Description.Replace(" ", "_") + "_", timestamp,
                    true, true, true, false,
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
            report += SaveState.lollipop.raw_neucode_pairs.Count(nc => nc.accepted).ToString() + "\tAccepted NeuCode Pairs" + Environment.NewLine + Environment.NewLine;

            report += SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Length.ToString() + "\tExperimental Proteoforms" + Environment.NewLine;
            report += SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.accepted).ToString() + "\tAccepted Experimental Proteoforms" + Environment.NewLine;
            report += SaveState.lollipop.theoretical_database.theoretical_proteins.Sum(kv => kv.Value.Length).ToString() + "\tTheoretical Proteins" + Environment.NewLine;
            report += SaveState.lollipop.theoretical_database.expanded_proteins.Length + "\tExpanded Theoretical Proteins" + Environment.NewLine;
            report += SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length.ToString() + "\tTheoretical Proteoforms" + Environment.NewLine + Environment.NewLine;

            report += SaveState.lollipop.et_peaks.Count.ToString() + "\tExperimental-Theoretical Peaks" + Environment.NewLine;
            report += SaveState.lollipop.et_relations.Count.ToString() + "\tExperimental-Theoretical Pairs" + Environment.NewLine;
            report += SaveState.lollipop.et_peaks.Count(p => p.Accepted).ToString() + "\tAccepted Experimental-Theoretical Peaks" + Environment.NewLine;
            report += SaveState.lollipop.et_relations.Count(r => r.Accepted).ToString() + "\tAccepted Experimental-Theoretical Pairs" + Environment.NewLine;
            report += SaveState.lollipop.ed_relations.Count <= 0 ? Environment.NewLine : SaveState.lollipop.ed_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-Decoy Pairs" + Environment.NewLine + Environment.NewLine;

            report += SaveState.lollipop.ee_peaks.Count.ToString() + "\tExperimental-Experimental Peaks" + Environment.NewLine;
            report += SaveState.lollipop.ee_relations.Count.ToString() + "\tExperimental-Experimental Pairs" + Environment.NewLine;
            report += SaveState.lollipop.ee_peaks.Count(p => p.Accepted).ToString() + "\tAccepted Experimental-Experimental Peaks" + Environment.NewLine;
            report += SaveState.lollipop.ee_relations.Count(r => r.Accepted).ToString() + "\tAccepted Experimental-Experimental Pairs" + Environment.NewLine;
            report += SaveState.lollipop.ef_relations.Count <= 0 ? Environment.NewLine : SaveState.lollipop.ef_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-False Pairs" + Environment.NewLine + Environment.NewLine;

            report += SaveState.lollipop.target_proteoform_community.families.Count.ToString() + "\tProteoform Families" + Environment.NewLine;
            List<ProteoformFamily> identified_families = SaveState.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() == 1).ToList();
            List<ProteoformFamily> ambiguous_families = SaveState.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() > 1).ToList();
            report += identified_families.Count.ToString() + "\tIdentified Families (Correspond to 1 gene)" + Environment.NewLine;
            report += identified_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Identified Families" + Environment.NewLine;
            report += ambiguous_families.Count.ToString() + "\tAmbiguous Families (Correspond to > 1 gene)" + Environment.NewLine;
            report += ambiguous_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Ambiguous Families" + Environment.NewLine;
            report += SaveState.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count == 1).ToString() + "\tOrphaned Experimental Proteoforms (Not joined with another proteoform)" + Environment.NewLine;
            report += SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown) == 0).ToString() + "\tIdentified Experimental Proteoforms" + Environment.NewLine;
            if (SaveState.lollipop.decoy_proteoform_communities.Values.SelectMany(v => v.families).Count() > 0)
                report += Math.Round(SaveState.lollipop.decoy_proteoform_communities.Average(v => v.Value.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown) == 0)) / SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown) == 0), 2) + "\tProteoform FDR" + Environment.NewLine;
            report += Environment.NewLine;

            report += SaveState.lollipop.satisfactoryProteoforms.Count.ToString() + "\tQuantified Experimental Proteoforms (Threshold for Quantification: " + SaveState.lollipop.minBiorepsWithObservations.ToString() + " = " + SaveState.lollipop.observation_requirement + ")" + Environment.NewLine;
            report += SaveState.lollipop.satisfactoryProteoforms.Count(p => p.quant.significant).ToString() + "\tExperimental Proteoforms with Significant Change (Threshold for Significance: Log2FoldChange > " + SaveState.lollipop.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + SaveState.lollipop.minProteoformIntensity.ToString() + ", & Q-Value < " + SaveState.lollipop.minProteoformFDR.ToString() + ")" + Environment.NewLine + Environment.NewLine;

            report += SaveState.lollipop.getInterestingFamilies(SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.minProteoformFoldChange, SaveState.lollipop.minProteoformFDR, SaveState.lollipop.minProteoformIntensity).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
            report += SaveState.lollipop.inducedOrRepressedProteins.Count.ToString() + "\tIdentified Proteins with Significant Change" + Environment.NewLine;
            report += SaveState.lollipop.goTermNumbers.Count(g => g.by < (double)SaveState.lollipop.minProteoformFDR).ToString() + "\tGO Terms of Significance (Benjimini-Yekeulti p-value < " + SaveState.lollipop.minProteoformFDR.ToString() + "): " + Environment.NewLine + Environment.NewLine;

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
                .Where(e => e.linked_proteoform_references != null)
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
            foreach(TopDownProteoform td in SaveState.lollipop.target_proteoform_community.topdown_proteoforms)
            {
                if (td.linked_proteoform_references != null)
                {
                    results.Rows.Add(
                        td.uniprot_id,
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
