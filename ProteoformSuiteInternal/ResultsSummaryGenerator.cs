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
            using (StreamWriter writer = new StreamWriter(Path.Combine(directory, "summary_" + timestamp + ".txt")))
            {
                writer.Write(generate_full_report());
            }
        }

        private static void save_dataframe(TusherAnalysis analysis, string directory, string timestamp)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(directory, "experimental_results_" + timestamp + ".tsv")))
            {
                writer.Write(datatable_tostring(experimental_results_dataframe(analysis)));
            }
            if (Sweet.lollipop.topdown_proteoforms.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(directory, "topdown_results_" + timestamp + ".tsv")))
                {
                    writer.Write(datatable_tostring(topdown_results_dataframe()));
                }
            }
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
                    directory, "AllQuantFamilies_", timestamp,
                    go_analysis as IGoAnalysis,
                    true, true,
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    Lollipop.gene_centric_families, Lollipop.preferred_gene_label);
                message += Environment.NewLine;

                message += CytoscapeScript.write_cytoscape_script(tusher_analysis == null ? Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.Log2FoldChangeValues.significant), Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis) :
                Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms.Where(pf => tusher_analysis as TusherAnalysis1 != null ? pf.quant.TusherValues1.significant : pf.quant.TusherValues2.significant), 
                tusher_analysis as TusherAnalysis != null ? tusher_analysis.GoAnalysis : Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis)
                .Distinct().ToList(), Sweet.lollipop.target_proteoform_community.families,
                    directory, "SignificantChanges_", timestamp,
                    go_analysis as IGoAnalysis,
                    true, true,
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    Lollipop.gene_centric_families, Lollipop.preferred_gene_label);
                message += Environment.NewLine;
            }

            foreach (GoTermNumber gtn in go_analysis.GoAnalysis.goTermNumbers.Where(g => g.by < (double)go_analysis.GoAnalysis.maxGoTermFDR).ToList())
            {
                message += CytoscapeScript.write_cytoscape_script(new GoTermNumber[] { gtn }, Sweet.lollipop.target_proteoform_community.families,
                    directory, gtn.Aspect.ToString() + gtn.Description.Replace(" ", "_").Replace(@"\", "_").Replace(@"/", "_") + "_", timestamp,
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
            StringBuilder report = new StringBuilder();
            foreach (Purpose p in Sweet.lollipop.input_files.Select(f => f.purpose).Distinct())
            {
                report.Append(p.ToString() + ":" + Environment.NewLine + String.Join(Environment.NewLine, Sweet.lollipop.get_files(Sweet.lollipop.input_files, p).Select(f => f.filename + f.extension + "\t" + f.complete_path)) + Environment.NewLine + Environment.NewLine);
            }
            return header + report;
        }

        public static string raw_components_report()
        {
            string report = "";

            report += Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Sum(f => f.reader.unprocessed_components).ToString() + "\tUnprocessed Raw Experimental Components" + Environment.NewLine;
            report += Sweet.lollipop.raw_experimental_components.Count.ToString() + "\tRaw Experimental Components" + Environment.NewLine;
            report += Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Sum(f => f.reader.missed_mono_merges).ToString() + "\tMissed Monoisotopic Raw Experimental Components Merged" + Environment.NewLine;
            report += Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Sum(f => f.reader.harmonic_merges).ToString() + "\tHarmonic Raw Experimental Components Merged" + Environment.NewLine;
            report += Environment.NewLine;
            report += Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Sum(f => f.reader.unprocessed_components).ToString() + "\tUnprocessed Raw Quantitative Components" + Environment.NewLine;
            report += Sweet.lollipop.raw_quantification_components.Count.ToString() + "\tRaw Quantitative Components" + Environment.NewLine;
            report += Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Sum(f => f.reader.missed_mono_merges).ToString() + "\tMissed Monoisotopic Raw Quantitative Components Merged" + Environment.NewLine;
            report += Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Sum(f => f.reader.harmonic_merges).ToString() + "\tHarmonic Raw Quantitative Components Merged" + Environment.NewLine;
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

            report += Sweet.lollipop.top_down_hits.Count.ToString() + "\tTop-Down Hits" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.topdown_id).ToString() + "\tAccepted Top-Down Proteoforms" + Environment.NewLine;
            report += Environment.NewLine;

            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Length.ToString() + "\tExperimental Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.accepted).ToString() + "\tAccepted Experimental Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.accepted && !e.topdown_id).ToString() + "\tAccepted Intact-Mass Experimental Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.accepted && e.topdown_id).ToString() + "\tAccepted Top-Down Experimental Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.theoretical_database.theoretical_proteins.Sum(kv => kv.Value.Length).ToString() + "\tTheoretical Proteins" + Environment.NewLine;
            report += Sweet.lollipop.theoretical_database.expanded_proteins.Length + "\tExpanded Theoretical Proteins" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length.ToString() + "\tTheoretical Proteoforms" + Environment.NewLine;
            report += Environment.NewLine;

            report += Sweet.lollipop.et_peaks.Count.ToString() + "\tExperimental-Theoretical Peaks" + Environment.NewLine;
            report += Sweet.lollipop.et_relations.Count.ToString() + "\tExperimental-Theoretical Pairs" + Environment.NewLine;
            report += Sweet.lollipop.et_peaks.Count(p => p.Accepted).ToString() + "\tAccepted Experimental-Theoretical Peaks" + Environment.NewLine;
            report += Sweet.lollipop.et_relations.Count(r => r.Accepted).ToString() + "\tAccepted Experimental-Theoretical Pairs" + Environment.NewLine;
            report += Sweet.lollipop.ed_relations.Count <= 0 ? Environment.NewLine : Sweet.lollipop.ed_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-Decoy Pairs" + Environment.NewLine + Environment.NewLine;

            report += Sweet.lollipop.ee_peaks.Count.ToString() + "\tExperimental-Experimental Peaks" + Environment.NewLine;
            report += Sweet.lollipop.ee_relations.Count.ToString() + "\tExperimental-Experimental Pairs" + Environment.NewLine;
            report += Sweet.lollipop.ee_peaks.Count(p => p.Accepted).ToString() + "\tAccepted Experimental-Experimental Peaks" + Environment.NewLine;
            report += Sweet.lollipop.ee_relations.Count(r => r.Accepted).ToString() + "\tAccepted Experimental-Experimental Pairs" + Environment.NewLine;
            report += Sweet.lollipop.ef_relations.Count <= 0 ? Environment.NewLine : Sweet.lollipop.ef_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-False Pairs" + Environment.NewLine + Environment.NewLine;

            report += proteoform_families_report();

            report += quant_report();

            return report;
        }

        public static string proteoform_families_report()
        {
            string report = "";

            report += Sweet.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count > 1).ToString() + "\tProteoform Families" + Environment.NewLine;
            List<ProteoformFamily> identified_families = Sweet.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() == 1).ToList();
            List<ProteoformFamily> ambiguous_families = Sweet.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() > 1).ToList();
            List<ProteoformFamily> unidentified_families = Sweet.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() == 0 && f.proteoforms.Count > 1).ToList();
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
            report += Sweet.lollipop.raw_quantification_components.Count > 0 ?
                Math.Round(100 * ((double)raw_quant_components_in_fams / (double)Sweet.lollipop.raw_quantification_components.Count), 2) + "\t% of Raw Quantitative Components in Families" + Environment.NewLine :
                "N/A\t% of Raw Quantitative Components in Families" + Environment.NewLine;
            report += Environment.NewLine;

            int identified_exp_proteoforms = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct));
            double avg_identified_decoy_proteoforms = Sweet.lollipop.decoy_proteoform_communities.Count > 0 ?
                Sweet.lollipop.decoy_proteoform_communities.Average(v => v.Value.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct))) :
                -1;
            report += identified_exp_proteoforms.ToString() + "\tIdentified Experimental Proteoforms" + Environment.NewLine;
            report += (avg_identified_decoy_proteoforms > 0 ? Math.Round(avg_identified_decoy_proteoforms, 2).ToString() : "N/A")
                    + "\tAverage Identified Experimental Proteoforms by Decoys" + Environment.NewLine;
            report += Sweet.lollipop.decoy_proteoform_communities.Values.SelectMany(v => v.families).Count() > 0 && identified_exp_proteoforms > 0 ?
                Math.Round(avg_identified_decoy_proteoforms / identified_exp_proteoforms, 4).ToString() + "\tProteoform FDR" + Environment.NewLine :
                "N/A\tProteoform FDR" + Environment.NewLine;
            report += Environment.NewLine;

            int correct_td = Sweet.lollipop.topdown_proteoforms.Count(p => p.linked_proteoform_references != null && p.correct_id);
            int incorrect_td = Sweet.lollipop.topdown_proteoforms.Count(p => p.linked_proteoform_references != null && !p.correct_id);
            report += correct_td + "\tTop-Down Proteoforms Assigned Same Identification by Intact-Mass Analysis" + Environment.NewLine;
            report += incorrect_td + "\tTop-Down Proteoforms Assigned Different Identification by Intact-Mass Analysis" + Environment.NewLine;
            report += Sweet.lollipop.topdown_proteoforms.Count(p => p.linked_proteoform_references == null) + "\tTop-Down Proteoforms Unidentified by Intact-Mass Analysis" + Environment.NewLine;
            report += Environment.NewLine;

            int identified_exp_proteoforms_intact = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct));
            report += identified_exp_proteoforms_intact.ToString() + "\tIdentified Intact-Mass Experimental Proteoforms" + Environment.NewLine;
            int ambiguous_exp_proteoforms = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct) && e.ambiguous);
            report += ambiguous_exp_proteoforms.ToString() + "\tIdentified Intact-Mass Experimental Proteoforms That Are Possibly Ambiguous" + Environment.NewLine;
            report += Environment.NewLine;

            List<string> td_proteins = Sweet.lollipop.topdown_proteoforms.Select(t => t.accession.Split('_')[0].Split('-')[0]).Distinct().ToList();
            report += td_proteins.Count() + "\tUnique Top-Down Protein Identifications (TDPortal)" + Environment.NewLine;
            List<string> intact_mass_proteins = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(e => !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct)).Select(p => p.linked_proteoform_references.First().accession.Split('_')[0].Split('-')[0]).Distinct().ToList();
            report += td_proteins.Concat(intact_mass_proteins).Distinct().Count() + "\tTotal Unique Protein Identifications" + Environment.NewLine;
            report += Environment.NewLine;

            //get list of experimental accession, begin, end, and PTMs
            List<string> experimental_ids = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(e => !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct))
                .Select(p => String.Join(",", (p.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(e => e.AccessionList.Select(a => a.Split('_')[0])).Distinct()) + "_" + p.begin + "_" + p.end + "_" + String.Join(", ", p.ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).OrderBy(m => m))).ToList();
            report += experimental_ids.Distinct().Count() + "\tUnique Intact-Mass Experimental Proteoforms Identifications" + Environment.NewLine;
            int unique_td = Sweet.lollipop.topdown_proteoforms.Select(p => p.pfr_accession).Distinct().Count();
            report += unique_td + "\tUnique Top-Down Proteoforms Identifications (TDPortal)" + Environment.NewLine;
            List<string> topdown_ids = Sweet.lollipop.topdown_proteoforms
               .Select(p => p.accession.Split('_')[0].Split('-')[0] + "_" + p.topdown_begin + "_" + p.topdown_end + "_" + String.Join(", ", p.topdown_ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).OrderBy(m => m))).ToList();
            int unique_experimental_ids_not_in_td = experimental_ids.Where(e => !topdown_ids.Any(t => e.Split('_')[0].Split(',').Contains(t.Split('_')[0])
                    && e.Split('_')[1] == t.Split('_')[1] && e.Split('_')[2] == t.Split('_')[2] && e.Split('_')[3] == t.Split('_')[3])).Distinct().Count();
            //this # accounts for accessions that were grouped but are the same mass.... (don't  count as an additional ID)
            report += unique_experimental_ids_not_in_td + "\tUnique  Intact-Mass Experimental Proteoforms Identifications Not Identified in Top-Down" + Environment.NewLine;
            int total_unique = unique_td + unique_experimental_ids_not_in_td;
            report += total_unique + "\tTotal Unique Proteoform Identifications" + Environment.NewLine;
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

            report += "TUSHER ANALYSIS (" + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count + " Permutations)" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis1.QuantitativeDistributions.selectAverageIntensity, 4).ToString() + "\tAverage Log2 Intensity Quantified Experimental Proteoform Observations" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis1.QuantitativeDistributions.selectStDev, 2).ToString() + "\tLog2 Intensity Standard Deviation for Quantified Experimental Proteoform" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant).ToString() + "\tExperimental Proteoforms with Significant Change (Threshold for Significance: Log2FoldChange > " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformIntensity.ToString() + ", & Q-Value < " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR.ToString() + ")" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant_relative_difference).ToString() + "\tExperimental Proteoforms with Significant Change by Relative Difference " + tusher_significance_parenthetical + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues1.significant_fold_change).ToString() + "\tExperimental Proteoforms with Significant Change by Fold Change " + tusher_significance_parenthetical + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis1.relativeDifferenceFDR, 4).ToString() + "\tFDR for Significance Conclusion " + tusher_significance_parenthetical + Environment.NewLine;
            report += Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.TusherValues1.significant), Sweet.lollipop.TusherAnalysis1.GoAnalysis).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.TusherAnalysis1.inducedOrRepressedProteins.Count.ToString() + "\tIdentified Proteins with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.TusherAnalysis1.GoAnalysis.goTermNumbers.Count(g => g.by < (double)Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR).ToString() + "\tGO Terms of Significance (Benjimini-Yekeulti p-value < " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR.ToString() + "; using Experimental Proteoforms that satisified the criteria:  Log2FoldChange > " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.minProteoformIntensity.ToString() + ", & Q-Value < " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR.ToString() + ")" + Environment.NewLine;
            report += Environment.NewLine;

            report += "TUSHER ANALYSIS (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count + " Permutations)" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis2.QuantitativeDistributions.selectAverageIntensity, 4).ToString() + "\tAverage Log2 Intensity Quantified Experimental Proteoform Observations" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis2.QuantitativeDistributions.selectStDev, 2).ToString() + "\tLog2 Intensity Standard Deviation for Quantified Experimental Proteoform" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues2.significant).ToString() + "\tExperimental Proteoforms with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues2.significant_relative_difference).ToString() + "\tExperimental Proteoforms with Significant Change by Relative Difference " + tusher_significance_parenthetical + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.TusherValues2.significant_fold_change).ToString() + "\tExperimental Proteoforms with Significant Change by Fold Change " + tusher_significance_parenthetical + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.TusherAnalysis2.relativeDifferenceFDR, 4).ToString() + "\tFDR for Significance Conclusion " + tusher_significance_parenthetical + Environment.NewLine;
            report += Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.TusherValues2.significant), Sweet.lollipop.TusherAnalysis2.GoAnalysis).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.TusherAnalysis2.inducedOrRepressedProteins.Count.ToString() + "\tIdentified Proteins with Significant Change" + Environment.NewLine;
            report += Sweet.lollipop.TusherAnalysis2.GoAnalysis.goTermNumbers.Count(g => g.by < (double)Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR).ToString() + "\tGO Terms of Significance (Benjimini-Yekeulti p-value < " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR.ToString() + "; using Experimental Proteoforms that satisified the criteria:  Log2FoldChange > " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.minProteoformIntensity.ToString() + ", & Q-Value < " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR.ToString() + ")" + Environment.NewLine;
            report += Environment.NewLine;

            report += "LOG2 FOLD CHANGE ANALYSIS" + Environment.NewLine;
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.Log2FoldChangeValues.significant).ToString() + "\tExperimental Proteoforms with Significant Change (Threshold for Significance: Benjimini-Hochberg Q-Value < " + Sweet.lollipop.Log2FoldChangeAnalysis.benjiHoch_fdr + ")" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.Log2FoldChangeAnalysis.benjiHoch_fdr, 4).ToString() + "\tFDR for Significance Conclusion" + Environment.NewLine;
            report += Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.Log2FoldChangeValues.significant), Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
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

        public static DataTable experimental_results_dataframe(TusherAnalysis analysis)
        {
            DataTable results = new DataTable();
            results.Columns.Add("Proteoform ID", typeof(string));
            results.Columns.Add("Proteoform Description", typeof(string));
            results.Columns.Add("Aggregated Observation ID", typeof(string));
            results.Columns.Add("SGD ID", typeof(string));
            results.Columns.Add("Gene Name", typeof(string));
            results.Columns.Add("Accessions", typeof(string));
            results.Columns.Add("PTM Type", typeof(string));
            results.Columns.Add("Begin and End", typeof(string));
            results.Columns.Add("Mass Error", typeof(double));
            results.Columns.Add("Proteoform Mass");
            results.Columns.Add("Retention Time", typeof(double));
            results.Columns.Add("Aggregated Intensity", typeof(double));
            results.Columns.Add("Top-Down Proteoform", typeof(bool));
            results.Columns.Add("Same ID as Top-Down", typeof(string));
            results.Columns.Add("Ambiguous", typeof(bool));
            results.Columns.Add("Adduct", typeof(bool));
            results.Columns.Add("Contaminant", typeof(bool));
            results.Columns.Add("Family ID", typeof(string));
            results.Columns.Add((Sweet.lollipop.numerator_condition == "" ? "Condition #1" : Sweet.lollipop.numerator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add((Sweet.lollipop.denominator_condition == "" ? "Condition #2" : Sweet.lollipop.denominator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add("M/z values", typeof(string));
            results.Columns.Add("Charges values", typeof(string));
            results.Columns.Add("Statistically Significant", typeof(bool));

            foreach (ExperimentalProteoform e in Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms)
                .Where(e => e.linked_proteoform_references != null)
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
                    String.Join(", ", (e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList.Select(a => a.Split('_')[0])).Distinct()),
                    e.ptm_set.ptm_combination.Count == 0 ?
                        "Unmodified" :
                        String.Join("; ", e.ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).OrderBy(m => m)),
                    e.begin + " to " + e.end,
                    e.mass_error,
                    e.modified_mass,
                    e.agg_rt,
                    e.agg_intensity,
                    e.topdown_id,
                    e.topdown_id ? (e as TopDownProteoform).correct_id.ToString() : "N/A",
                    e.ambiguous,
                    e.adduct,
                    (e.linked_proteoform_references.First() as TheoreticalProteoform).contaminant,
                    e.family != null ? e.family.family_id.ToString() : "",
                    Sweet.lollipop.significance_by_log2FC ? e.quant.Log2FoldChangeValues.numeratorIntensitySum : get_tusher_values(e.quant, analysis).numeratorIntensitySum,
                    Sweet.lollipop.significance_by_log2FC ? e.quant.Log2FoldChangeValues.denominatorIntensitySum : get_tusher_values(e.quant, analysis).denominatorIntensitySum,
                    e.aggregated.Count > 0 ? String.Join(", ", (e.aggregated.OrderByDescending(c => c.intensity_sum).First() as Component).charge_states.Select(cs => Math.Round(cs.mz_centroid, 2))) : "",
                    e.aggregated.Count > 0 ? String.Join(", ", (e.aggregated.OrderByDescending(c => c.intensity_sum).First() as Component).charge_states.Select(cs => cs.charge_count)) : "",
                    Sweet.lollipop.significance_by_log2FC ? e.quant.Log2FoldChangeValues.significant : get_tusher_values(e.quant, analysis).significant
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

        public static DataTable topdown_results_dataframe()
        {
            DataTable results = new DataTable();
            results.Columns.Add("PFR Accession", typeof(string));
            results.Columns.Add("Theoretical Accession", typeof(string));
            results.Columns.Add("Top-Down Full Accession", typeof(string));
            results.Columns.Add("Top-Down Accession", typeof(string));
            results.Columns.Add("Theoretical Description", typeof(string));
            results.Columns.Add("Theoretical Begin and End", typeof(string));
            results.Columns.Add("Top-Down Begin and End", typeof(string));
            results.Columns.Add("Theoretical PTM Type", typeof(string));
            results.Columns.Add("Top-Down PTM Type", typeof(string));
            results.Columns.Add("Top-Down PTM Type Unlocalized", typeof(string));
            results.Columns.Add("Proteoform Suite Mass Error", typeof(string));
            results.Columns.Add("Top-Down Mass Error", typeof(double));
            results.Columns.Add("Proteoform Mass");
            results.Columns.Add("Retention Time", typeof(double));
            results.Columns.Add("Best Scoring Hit", typeof(string));
            results.Columns.Add("Theoretical SGD ID", typeof(string));
            results.Columns.Add("Theoretical Gene Name", typeof(string));
            results.Columns.Add("Top-Down Gene Name", typeof(string));
            results.Columns.Add("Family ID", typeof(string));
            results.Columns.Add("Correct ID", typeof(bool));
            results.Columns.Add("Accepted", typeof(bool));

            foreach (TopDownProteoform td in Sweet.lollipop.topdown_proteoforms)
            {
                results.Rows.Add(
                    td.pfr_accession,
                    td.linked_proteoform_references == null ? "N/A" : (td.linked_proteoform_references.First() as TheoreticalProteoform).accession,
                    td.accession,
                    td.accession.Split('_')[0],
                    td.linked_proteoform_references == null ? "N/A" : (td.linked_proteoform_references.First() as TheoreticalProteoform).description,
                    td.linked_proteoform_references == null ? "N/A" : td.begin + " to " + td.end,
                    td.topdown_begin + " to " + td.topdown_end,
                    td.linked_proteoform_references == null ? "N/A" : td.ptm_set.ptm_combination.Count == 0 ? "Unmodified" : String.Join("; ", td.ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).OrderBy(m => m)),
                    td.topdown_ptm_description,
                    td.topdown_ptm_set.ptm_combination.Count == 0 ?
                        "Unmodified" : String.Join("; ", td.topdown_ptm_set.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).OrderBy(m => m)),
                    td.linked_proteoform_references == null ? "N/A" : td.mass_error.ToString(),
                    td.modified_mass - td.theoretical_mass,
                    td.modified_mass,
                    td.agg_rt,
                    td.manual_validation_id,
                    td.linked_proteoform_references == null ? "N/A" : td.linked_proteoform_references.Last().gene_name.ordered_locus,
                    td.linked_proteoform_references == null ? "N/A" : td.linked_proteoform_references.Last().gene_name.primary,
                    td.gene_name != null ? td.gene_name.primary : "",
                    td.family == null ? "N/A" : td.family.family_id.ToString(),
                    td.correct_id,
                    td.accepted
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

        public static DataTable biological_replicate_intensities(IGoAnalysis analysis, IEnumerable<ExperimentalProteoform> proteoforms, List<InputFile> input_files, Dictionary<string, List<string>> conditionsBioReps, bool include_imputation, bool include_normalized_intensity)
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
                            try
                            {
                                results.Columns.Add(condition_bioreps.Key + "_" + biorep, typeof(double)); // biorep intensities in Log2FoldChangeAnalysis
                            }
                            catch
                            {
                            }
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
                                value = br != null ? !br.imputed || include_imputation ? br.intensity_sum : 0 : 0;
                            }
                            else
                            {
                                double norm_divisor = Sweet.lollipop.TusherAnalysis1.conditionBiorep_sums[new Tuple<string, string>(condition_bioreps.Key, biorep)] /
                                    (Sweet.lollipop.neucode_labeled ? Sweet.lollipop.TusherAnalysis1.conditionBiorep_sums.Where(kv => kv.Key.Item2 == biorep).Average(kv => kv.Value) : Sweet.lollipop.TusherAnalysis1.conditionBiorep_sums.Average(kv => kv.Value));
                                value = pf.biorepIntensityList.Where(x => x.condition == condition_bioreps.Key && x.biorep == biorep).Sum(x => x.intensity_sum)
                                    / norm_divisor;
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
                                    value = br != null ? !br.imputed || include_imputation ? br.intensity_sum : 0 : 0;
                                }
                                else
                                {
                                    double norm_divisor = Sweet.lollipop.TusherAnalysis2.conditionBiorep_sums[new Tuple<string, string>(condition_bioreps.Key, biorep)]
                                        / (Sweet.lollipop.neucode_labeled ? Sweet.lollipop.TusherAnalysis2.conditionBiorep_sums.Where(kv => kv.Key.Item2 == biorep).Average(kv => kv.Value) : Sweet.lollipop.TusherAnalysis2.conditionBiorep_sums.Average(kv => kv.Value));
                                    value = pf.biorepTechrepIntensityList.Where(x => x.condition == condition_bioreps.Key && x.biorep == biorep && x.techrep == techrep).Sum(x => x.intensity_sum)
                                        / norm_divisor;
                                }
                                row[condition_bioreps.Key + "_" + biorep + "_" + techrep] = value;
                            }
                        }
                        else if (analysis as Log2FoldChangeAnalysis != null)
                        {
                            if (include_imputation)
                            {
                                pf.quant.Log2FoldChangeValues.allIntensities.TryGetValue(new Tuple<string, string>(condition_bioreps.Key, biorep), out BiorepIntensity bft);
                                value = bft != null ? !bft.imputed || include_imputation ? bft.intensity_sum : 0 : 0;
                            }
                            else
                            {
                                double norm_divisor = Sweet.lollipop.Log2FoldChangeAnalysis.conditionBiorepIntensitySums[new Tuple<string, string>(condition_bioreps.Key, biorep)]
                                   / (Sweet.lollipop.neucode_labeled ? Sweet.lollipop.Log2FoldChangeAnalysis.conditionBiorepIntensitySums.Where(kv => kv.Key.Item2 == biorep).Average(kv => kv.Value)
                                   : Sweet.lollipop.Log2FoldChangeAnalysis.conditionBiorepIntensitySums.Average(kv => kv.Value));
                                value = pf.biorepIntensityList.Where(x => x.condition == condition_bioreps.Key && x.biorep == biorep).Sum(x => x.intensity_sum)
                                    / (include_normalized_intensity ? norm_divisor : 1);
                            }
                            row[condition_bioreps.Key + "_" + biorep] = value;
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