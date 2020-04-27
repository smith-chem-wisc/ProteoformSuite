using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteomics;

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
                writer.Write(datatable_tostring(experimental_results_dataframe(Sweet.lollipop.target_proteoform_community, analysis)));
            }

            using (StreamWriter writer =
                new StreamWriter(Path.Combine(directory, "decoy_experimental_results_" + timestamp + ".tsv")))
            {
                foreach (var decoy_community in Sweet.lollipop.decoy_proteoform_communities.Values)
                {
                    writer.Write(datatable_tostring(
                        experimental_results_dataframe(decoy_community, analysis)));
                }
            }

            using (StreamWriter writer = new StreamWriter(Path.Combine(directory, "experimental_intensities_by_file_" + timestamp + ".tsv")))
            {
                writer.Write(datatable_tostring(experimental_intensities_dataframe()));
            }
            if (Sweet.lollipop.topdown_proteoforms.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(directory, "topdown_results_" + timestamp + ".tsv")))
                {
                    writer.Write(datatable_tostring(topdown_results_dataframe()));
                }
            }

            if(Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Count > 0)
            {
                Dictionary<string, List<TopDownProteoform>> topdowns_by_accession = new Dictionary<string, List<TopDownProteoform>>();
                using (StreamWriter writer = new StreamWriter(Path.Combine(directory, "bottomup_results_" + timestamp + ".tsv")))
                {
                    writer.Write(datatable_tostring(bottomup_results_dataframe()));
                }

                using (StreamWriter writer = new StreamWriter(Path.Combine(directory, "shared_peptide_bottomup_results_" + timestamp + ".tsv")))
                {
                    writer.Write(datatable_tostring(shared_peptide_results_dataframe()));
                }
                
                using(StreamWriter writer = new StreamWriter(Path.Combine(directory, "proteoform_bottomup_evidence_" + timestamp + ".tsv")))
                {
                    writer.Write(datatable_tostring(putative_proteoforms_bottom_up()));
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
                    Sweet.lollipop.gene_centric_families, Lollipop.preferred_gene_label, false);
            message += Environment.NewLine;

           if (Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Count > 0)
           {
                message += CytoscapeScript.write_cytoscape_script(Sweet.lollipop.target_proteoform_community.families, Sweet.lollipop.target_proteoform_community.families,
                Sweet.lollipop.results_folder, "BottomUp_", timestamp,
                null,
                true, true,
                CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                Sweet.lollipop.gene_centric_families, Lollipop.preferred_gene_label, true);
                        message += Environment.NewLine;
           }

            if (Sweet.lollipop.qVals.Count > 0)
            {
                message += CytoscapeScript.write_cytoscape_script(Sweet.lollipop.target_proteoform_community.families, Sweet.lollipop.target_proteoform_community.families,
                    directory, "AllQuantFamilies_", timestamp,
                    go_analysis as IGoAnalysis,
                    true, true,
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    Sweet.lollipop.gene_centric_families, Lollipop.preferred_gene_label, false);
                message += Environment.NewLine;

                message += CytoscapeScript.write_cytoscape_script(tusher_analysis == null ? Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.Log2FoldChangeValues.significant), Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis) :
                Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms.Where(pf => tusher_analysis as TusherAnalysis1 != null ? pf.quant.TusherValues1.significant : pf.quant.TusherValues2.significant),
                tusher_analysis as TusherAnalysis != null ? tusher_analysis.GoAnalysis : Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis)
                .Distinct().ToList(), Sweet.lollipop.target_proteoform_community.families,
                    directory, "SignificantChanges_", timestamp,
                    go_analysis as IGoAnalysis,
                    true, true,
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    Sweet.lollipop.gene_centric_families, Lollipop.preferred_gene_label, false);
                message += Environment.NewLine;
            }

            foreach (GoTermNumber gtn in go_analysis.GoAnalysis.goTermNumbers.Where(g => g.by < (double)go_analysis.GoAnalysis.maxGoTermFDR).ToList())
            {
                message += CytoscapeScript.write_cytoscape_script(new GoTermNumber[] { gtn }, Sweet.lollipop.target_proteoform_community.families,
                    directory, gtn.Aspect.ToString() + gtn.Description.Replace(" ", "_").Replace(@"\", "_").Replace(@"/", "_") + "_", timestamp,
                    go_analysis as IGoAnalysis, true, true,
                    CytoscapeScript.color_scheme_names[1], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], Lollipop.node_positioning[1], 2,
                    Sweet.lollipop.gene_centric_families, Lollipop.preferred_gene_label);
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
            string report = string.Join(Environment.NewLine, Sweet.save_actions) + Environment.NewLine + Environment.NewLine;
            return header + report;
        }

        public static string loaded_files_report()
        {
            string header = "DECONVOLUTION RESULTS FILES AND PROTEIN DATABASE FILES" + Environment.NewLine;
            StringBuilder report = new StringBuilder();
            foreach (Purpose p in Sweet.lollipop.input_files.Select(f => f.purpose).Distinct())
            {
                report.Append(p.ToString() + ":" + Environment.NewLine + string.Join(Environment.NewLine, Sweet.lollipop.get_files(Sweet.lollipop.input_files, p).Select(f => f.filename + f.extension + "\t" + f.complete_path)) + Environment.NewLine + Environment.NewLine);
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
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count().ToString() + "\tAccepted Experimental Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => !e.topdown_id).ToString() + "\tAccepted Intact-Mass Experimental Proteoforms" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.topdown_id).ToString() + "\tAccepted Top-Down Experimental Proteoforms" + Environment.NewLine;
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

            report += Sweet.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count > 1 || f.experimental_proteoforms.Count(e => e.topdown_id) == 1).ToString() + "\tProteoform Families" + Environment.NewLine;
            List<ProteoformFamily> identified_families = Sweet.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() == 1).ToList();
            List<ProteoformFamily> ambiguous_families = Sweet.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() > 1).ToList();
            List<ProteoformFamily> unidentified_families = Sweet.lollipop.target_proteoform_community.families.Where(f => f.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() == 0 && f.proteoforms.Count > 1).ToList();
            report += identified_families.Count.ToString() + "\tIdentified Families (Correspond to 1 " + (Sweet.lollipop.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += identified_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Identified Families" + Environment.NewLine;
            report += ambiguous_families.Count.ToString() + "\tAmbiguous Families (Correspond to > 1 " + (Sweet.lollipop.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += ambiguous_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Ambiguous Families" + Environment.NewLine;
            report += unidentified_families.Count.ToString() + "\tUnidentified Families (Correspond to no " + (Sweet.lollipop.gene_centric_families ? "gene" : "UniProt accession") + ")" + Environment.NewLine;
            report += unidentified_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Unidentified Families" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.families.Count(f => f.proteoforms.Count == 1 && f.experimental_proteoforms.Count(e => e.topdown_id) == 0).ToString() + "\tOrphaned Experimental Proteoforms (Intact-mass proteoforms not joined with another proteoform)" + Environment.NewLine;
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

             identified_exp_proteoforms = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && e.ambiguous_identifications.Count == 0 && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct));
             avg_identified_decoy_proteoforms = Sweet.lollipop.decoy_proteoform_communities.Count > 0 ?
                Sweet.lollipop.decoy_proteoform_communities.Average(v => v.Value.experimental_proteoforms.Count(e => e.linked_proteoform_references != null && e.ambiguous_identifications.Count == 0 && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct))) :
                -1;
            report += identified_exp_proteoforms.ToString() + "\tIdentified Experimental Proteoforms (no Ambiguous)" + Environment.NewLine;
            report += (avg_identified_decoy_proteoforms > 0 ? Math.Round(avg_identified_decoy_proteoforms, 2).ToString() : "N/A")
                      + "\tAverage Identified Experimental Proteoforms by Decoys (no Ambiguous)" + Environment.NewLine;
            report += Sweet.lollipop.decoy_proteoform_communities.Values.SelectMany(v => v.families).Count() > 0 && identified_exp_proteoforms > 0 ?
                Math.Round(avg_identified_decoy_proteoforms / identified_exp_proteoforms, 4).ToString() + "\tProteoform FDR" + Environment.NewLine :
                "N/A\tProteoform FDR (no Ambiguous)" + Environment.NewLine;
            report += Environment.NewLine;


            int correct_td = Sweet.lollipop.topdown_proteoforms.Count(p => p.linked_proteoform_references != null && p.correct_id && p.ambiguous_identifications.Count == 0);
            int incorrect_td = Sweet.lollipop.topdown_proteoforms.Count(p => p.linked_proteoform_references != null && !p.correct_id && p.ambiguous_identifications.Count == 0);
            int ambiguous_td = Sweet.lollipop.topdown_proteoforms.Count(p => p.linked_proteoform_references != null && p.ambiguous_identifications.Count > 0);

            report += correct_td + "\tTop-Down Proteoforms Assigned Same Identification by Intact-Mass Analysis" + Environment.NewLine;
            report += incorrect_td + "\tTop-Down Proteoforms Assigned Different Identification by Intact-Mass Analysis" + Environment.NewLine;
            report += ambiguous_td + "\tTop-Down Proteoforms Assigned Amibguous Identification by Intact-Mass Analysis" + Environment.NewLine;
            report += Sweet.lollipop.topdown_proteoforms.Count(p => p.linked_proteoform_references == null) + "\tTop-Down Proteoforms Unidentified by Intact-Mass Analysis" + Environment.NewLine;
            report += Environment.NewLine;

            List<string> td_proteins = Sweet.lollipop.topdown_proteoforms.Select(t => t.accession.Split('_')[0].Split('-')[0]).Distinct().ToList();
            report += td_proteins.Count() + "\tUnique Top-Down Protein Identifications (TDPortal)" + Environment.NewLine;
            List<string> intact_mass_proteins = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(e => e.ambiguous_identifications.Count == 0 && !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct)).Select(p => p.linked_proteoform_references.First().accession.Split('_')[0].Split('-')[0]).Distinct().ToList();
            report += td_proteins.Concat(intact_mass_proteins).Distinct().Count() + "\tTotal Unique Protein Identifications" + Environment.NewLine;
            report += Environment.NewLine;

            //get list of experimental accession, sequence, and PTMs
            List<string> experimental_ids = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(e => e.ambiguous_identifications.Count == 0 && !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct))
                .Select(p => string.Join(",", (p.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(e => e.AccessionList.Select(a => a.Split('_')[0])).Distinct()) + "_" + ExperimentalProteoform.get_sequence(p.linked_proteoform_references.First() as TheoreticalProteoform, p.begin, p.end) + "_" + string.Join(", ", p.ptm_set.ptm_combination.Where(m => m.modification.ModificationType != "Deconvolution Error" ).Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).OrderBy(m => m))).ToList();
            report += experimental_ids.Distinct().Count() + "\tUnique Intact-Mass Experimental Proteoform Identifications" + Environment.NewLine;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => !e.topdown_id && e.ambiguous_identifications.Count > 0) + "\tAmbiguous Intact-Mass Experimental Proteoform Identifications" + Environment.NewLine;
            int unique_td = Sweet.lollipop.topdown_proteoforms.Select(p => p.pfr_accession).Distinct().Count();
            report += unique_td + "\tUnique Top-Down Proteoforms Identifications (TDPortal)" + Environment.NewLine;
            List<string> topdown_ids = Sweet.lollipop.topdown_proteoforms
               .Select(p => p.accession.Split('_')[0].Split('-')[0] + "_" + p.sequence + "_" + string.Join(", ", p.topdown_ptm_set.ptm_combination.Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).OrderBy(m => m))).ToList();
            int unique_experimental_ids_not_in_td = experimental_ids.Where(e => !topdown_ids.Any(t => e.Split('_')[0].Split(',').Contains(t.Split('_')[0])
                    && e.Split('_')[1] == t.Split('_')[1] && e.Split('_')[2] == t.Split('_')[2])).Distinct().Count();
            //this # accounts for accessions that were grouped but are the same mass.... (don't  count as an additional ID)
            report += unique_experimental_ids_not_in_td + "\tUnique  Intact-Mass Experimental Proteoforms Identifications Not Identified in Top-Down" + Environment.NewLine;
            int total_unique = unique_td + unique_experimental_ids_not_in_td;
            report += Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Count(e => !e.topdown_id && e.linked_proteoform_references == null) + "\tUnidentified Intact-Mass Experimental Proteoforms" + Environment.NewLine;
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
            report += Sweet.lollipop.satisfactoryProteoforms.Count(p => p.quant.Log2FoldChangeValues.significant).ToString() + "\tExperimental Proteoforms with Significant Change (Threshold for Significance: Benjimini-Hochberg Q-Value < " + Sweet.lollipop.benjiHoch_fdr + ")" + Environment.NewLine;
            report += Math.Round(Sweet.lollipop.benjiHoch_fdr, 4).ToString() + "\tFDR for Significance Conclusion" + Environment.NewLine;
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
                            report += exp_prots_with_these_bioreps + "\tExperimental Proteoforms Observed in " + condition + ", Biological Replicates #" + string.Join(" #", bioreps_of_interest) + Environment.NewLine;
                            report += exp_prots_with_these_bioreps_exclusive + "\tExperimental Proteoforms Observed Exclusively in " + condition + ", Biological Replicates #" + string.Join(" #", bioreps_of_interest) + Environment.NewLine;
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
                + string.Join(Environment.NewLine, Sweet.lollipop.TusherAnalysis1.inducedOrRepressedProteins.Select(p => p.Accession).Distinct().OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            result += "Identified Proteins with Significant Change: (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count + " Permutations)" + Environment.NewLine
                + string.Join(Environment.NewLine, Sweet.lollipop.TusherAnalysis2.inducedOrRepressedProteins.Select(p => p.Accession).Distinct().OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            result += "Identified Proteins with Significant Change: (by log2 fold change analysis)" + Environment.NewLine
                + string.Join(Environment.NewLine, Sweet.lollipop.Log2FoldChangeAnalysis.inducedOrRepressedProteins.Select(p => p.Accession).Distinct().OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            return result;
        }

        public static string go_terms_of_significance()
        {
            string result = "";
            result += "GO Terms of Significance, Tusher Analysis with " + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count + " permutations (Benjimini-Yekeulti p-value < " + Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR.ToString() + "): " + Environment.NewLine
                + string.Join(Environment.NewLine, Sweet.lollipop.TusherAnalysis1.GoAnalysis.goTermNumbers.Where(g => g.by < (double)Sweet.lollipop.TusherAnalysis1.GoAnalysis.maxGoTermFDR).Select(g => g.ToString()).OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            result += "GO Terms of Significance, Tusher Analysis with " + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count + " permutations (Benjimini-Yekeulti p-value < " + Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR.ToString() + "): " + Environment.NewLine
                + string.Join(Environment.NewLine, Sweet.lollipop.TusherAnalysis2.GoAnalysis.goTermNumbers.Where(g => g.by < (double)Sweet.lollipop.TusherAnalysis2.GoAnalysis.maxGoTermFDR).Select(g => g.ToString()).OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
            result += "GO Terms of Significance, Log2 Fold Change Analysis with " + Sweet.lollipop.benjiHoch_fdr.ToString() + " FDR (Benjimini-Yekeulti p-value < " + Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.maxGoTermFDR.ToString() + "): " + Environment.NewLine
                + string.Join(Environment.NewLine, Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.goTermNumbers.Where(g => g.by < (double)Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.maxGoTermFDR).Select(g => g.ToString()).OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
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
                result_string.AppendLine(string.Join("\t", row.ItemArray));
            }
            return result_string.ToString();
        }

        public static DataTable experimental_results_dataframe(ProteoformCommunity community, TusherAnalysis analysis)
        {
            DataTable results = new DataTable();
            results.Columns.Add("Community", typeof(string));
            results.Columns.Add("Aggregated Observation ID", typeof(string));
            results.Columns.Add("Proteoform Mass");
            results.Columns.Add("Retention Time", typeof(double));
            results.Columns.Add("Aggregated Intensity", typeof(double));
            results.Columns.Add("Aggregated Components", typeof(int));
            results.Columns.Add("Top-Down Proteoform", typeof(bool));
            results.Columns.Add("Proteoform Description", typeof(string));
            results.Columns.Add("Gene Name", typeof(string));
            results.Columns.Add("GeneID", typeof(string));
            results.Columns.Add("Accessions", typeof(string));
            results.Columns.Add("PTM Type", typeof(string));
            results.Columns.Add("Begin and End", typeof(string));
            results.Columns.Add("Sequence", typeof(string));
            results.Columns.Add("UniProt-Annotated Modifications");
            results.Columns.Add("Potentially Novel Modifications");
            results.Columns.Add("Bottom-Up PSM Count", typeof(string));

            //results.Columns.Add("Different BU PSM Count for Amibguous IDs", typeof(bool));
            results.Columns.Add("Bottom-Up Modifications from Proteoform", typeof(string));
            //results.Columns.Add("Bottom-Up Modifications from Protein", typeof(string));
            //   results.Columns.Add("Peptide-Specific Bottom-Up Modifications", typeof(string));
            results.Columns.Add("Bottom-Up Evidence for Begin", typeof(string));
            results.Columns.Add("Bottom-Up Evidence for End", typeof(string));
            results.Columns.Add("Bottom-Up Evidence for All PTMs", typeof(string));


            results.Columns.Add("Level Number", typeof(int));
            results.Columns.Add("Level Description", typeof(string));
            results.Columns.Add("New Intact-Mass ID", typeof(string));
            results.Columns.Add("Ambiguous", typeof(bool));
            results.Columns.Add("Adduct", typeof(bool));
            results.Columns.Add("Contaminant", typeof(bool));
            results.Columns.Add("Mass Error", typeof(string));
            results.Columns.Add("Family ID", typeof(string));
            results.Columns.Add("Family", typeof(string));
            results.Columns.Add("Linked Proteoform References", typeof(string));
            results.Columns.Add("Statistically Significant", typeof(bool));
            results.Columns.Add((Sweet.lollipop.numerator_condition == "" ? "Condition #1" : Sweet.lollipop.numerator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add((Sweet.lollipop.denominator_condition == "" ? "Condition #2" : Sweet.lollipop.denominator_condition) + " Quantified Proteoform Intensity", typeof(double));
            results.Columns.Add("M/z values", typeof(string));
            results.Columns.Add("Charges values", typeof(string));
            results.Columns.Add("Abundant Component for Manual Validation of Identification", typeof(string));
            foreach (ExperimentalProteoform e in community.families.SelectMany(f => f.experimental_proteoforms)
                .Where(e => e.linked_proteoform_references != null)
                .OrderByDescending(e => (Sweet.lollipop.significance_by_log2FC ? e.quant.Log2FoldChangeValues.significant : get_tusher_values(e.quant, analysis).significant) ? 1 : 0)
                .ThenBy(e => (e.linked_proteoform_references.First() as TheoreticalProteoform).accession)
                .ThenBy(e => e.ptm_set.ptm_combination.Count))
            {
                results.Rows.Add(
                    community.community_number < 0 ? "Target" : "Decoy_" + community.community_number,
                     e.accession,
                    e.modified_mass,
                    e.agg_rt,
                    e.agg_intensity,
                    e.aggregated.Count,
                    e.topdown_id,
                    (e.linked_proteoform_references.First() as TheoreticalProteoform).description + (e.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => (p.theoretical_base as TheoreticalProteoform).description))
                        : ""),
                    e.linked_proteoform_references.Last().gene_name.primary + (e.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => (p.theoretical_base as TheoreticalProteoform).gene_name.primary))
                        : ""),
                    string.Join("; ", (e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct()) + (e.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(t => string.Join("; ", (t.theoretical_base as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct())))
                        : ""),
                    string.Join(", ", (e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList.Select(a => a.Split('_')[0])).Distinct()) + (e.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(t => string.Join("; ", (t.theoretical_base as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList.Select(a => a.Split('_')[0])).Distinct())))
                        : ""),
                    e.ptm_set.ptm_description + (e.ambiguous_identifications.Count > 0 ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => p.ptm_set.ptm_description)) : ""),
                    e.begin + " to " + e.end + (e.ambiguous_identifications.Count > 0 ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => p.begin + " to " + p.end)) : ""),
                    ExperimentalProteoform.get_sequence(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end)
                    + (e.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(i => ExperimentalProteoform.get_sequence(i.theoretical_base as TheoreticalProteoform, i.begin, i.end)))
                        : ""),
                    e.uniprot_mods,
                    e.novel_mods,
                    (e.linked_proteoform_references != null ? e.bottom_up_PSMs.Count.ToString() : "N/A"
                           + (e.ambiguous_identifications.Count > 0
                               ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(i => i.bottom_up_PSMs.Count.ToString()))
                               : "")),
                   ((e.linked_proteoform_references != null ? e.bottom_up_PSMs.Count(p => p.ptm_list.Count > 0) == 0 ? "N/A" :
                           String.Join(", ", e.bottom_up_PSMs.Where(p => p.ptm_list.Count > 0).Select(p => p.ptm_description).Distinct()) : "N/A")
                       + (e.ambiguous_identifications.Count > 0
                           ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(i => i.bottom_up_PSMs.Count(p => p.ptm_list.Count > 0) == 0 ?
                                 "N/A" : String.Join(", ", i.bottom_up_PSMs.Where(p => p.ptm_list.Count > 0).Select(p => p.ptm_description).Distinct())))
                           : "")),
                  e.bottom_up_PSMs.Any(p => p.begin == e.begin) + (e.ambiguous_identifications.Count > 0 ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(a => a.bottom_up_PSMs.Any(p => p.begin == a.begin))) : ""),
                  e.bottom_up_PSMs.Any(p => p.end == e.end) + (e.ambiguous_identifications.Count > 0 ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(a => a.bottom_up_PSMs.Any(p => p.end == a.end))) : ""),
                   Proteoform.get_bottom_up_evidence_for_all_PTMs(e.bottom_up_PSMs, e.ptm_set, false) + (e.ambiguous_identifications.Count > 0 ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(i => Proteoform.get_bottom_up_evidence_for_all_PTMs(i.bottom_up_PSMs, i.ptm_set, false ))) : ""),
                  e.proteoform_level,
                   e.proteoform_level_description,
                   e.new_intact_mass_id,
                   e.ambiguous_identifications.Count > 0 ? "TRUE" : "FALSE",
                   e.adduct,
                   (e.linked_proteoform_references.First() as TheoreticalProteoform).contaminant,
                   e.calculate_mass_error(e.linked_proteoform_references.First() as TheoreticalProteoform, e.ptm_set, e.begin, e.end).ToString()
                    + (e.ambiguous_identifications.Count > 0
                    ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(i => e.calculate_mass_error(i.theoretical_base as TheoreticalProteoform, i.ptm_set, i.begin, i.end).ToString()))
                    : ""),
                    e.family != null ? e.family.family_id.ToString() : "",
                    e.family != null ? e.family.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() > 1 ? "Ambiguous" : "Identified" : "",
                    string.Join(", ", (e.linked_proteoform_references.Select(p => p.accession))) + (e.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => string.Join(", ", p.linked_proteoform_references.Select(a => a.accession))))
                        : ""),
                    Sweet.lollipop.significance_by_log2FC ? e.quant.Log2FoldChangeValues.significant : get_tusher_values(e.quant, analysis).significant,
                    Sweet.lollipop.significance_by_log2FC ? e.quant.Log2FoldChangeValues.numeratorIntensitySum : get_tusher_values(e.quant, analysis).numeratorIntensitySum,
                    Sweet.lollipop.significance_by_log2FC ? e.quant.Log2FoldChangeValues.denominatorIntensitySum : get_tusher_values(e.quant, analysis).denominatorIntensitySum,
                    e.aggregated.Count > 0 ? string.Join(", ", e.aggregated.OrderByDescending(c => c.intensity_sum).First().charge_states.Select(cs => Math.Round(cs.mz_centroid, 2))) : "",
                    e.aggregated.Count > 0 ? string.Join(", ", e.aggregated.OrderByDescending(c => c.intensity_sum).First().charge_states.Select(cs => cs.charge_count)) : "",
                    e.manual_validation_id
                    );;
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
                result_string.AppendLine(string.Join("\t", row.ItemArray));
            }
            return results;
        }

        public static DataTable experimental_intensities_dataframe()
        {
            DataTable results = new DataTable();

            //determine intensitites and normalized intensitites... (just summing fractions for now...)
            List<string> files = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Select(x => x.lt_condition + "|" + x.biological_replicate + "|" + x.technical_replicate).Distinct().ToList();
            if (files.Count == 0) return results;
            Dictionary<string, Dictionary<ExperimentalProteoform, double>> intensities_by_file = new Dictionary<string, Dictionary<ExperimentalProteoform, double>>();
            for (int f = 0; f < files.Count; f++)
            {
                intensities_by_file.Add(files[f], new Dictionary<ExperimentalProteoform, double>());
                foreach (ExperimentalProteoform p in Sweet.lollipop.target_proteoform_community.families.SelectMany(families => families.experimental_proteoforms))
                {
                    ExperimentalProteoform e = p.topdown_id ? (p as TopDownProteoform).matching_experimental : p;

                    string[] file_info = files[f].Split('|');
                    double intensity = e != null ? e.aggregated.Where(a => a.input_file.lt_condition == file_info[0] && a.input_file.biological_replicate == file_info[1] && a.input_file.technical_replicate == file_info[2]).Sum(a => a.intensity_sum) : 0;
                    intensities_by_file[files[f]].Add(p, intensity);
                }
            }
            
            Dictionary<string, Dictionary<ExperimentalProteoform, double>> normalized_intensities_by_file = new Dictionary<string, Dictionary<ExperimentalProteoform, double>>();

            //normalize by summed intensities for all pforms from a file
            foreach (var f in intensities_by_file)
            {
                normalized_intensities_by_file.Add(f.Key, new Dictionary<ExperimentalProteoform, double>());
                foreach (var p in f.Value)
                {
                    double norm_divisor = f.Value.Sum(v => v.Value) / intensities_by_file.Average(a => a.Value.Sum(v => v.Value));
                    normalized_intensities_by_file[f.Key].Add(p.Key, p.Value / norm_divisor);
                }
            }

                //normalize by median fold change
                //string conditionBiorept_with_least_missing_values = intensities_by_file.OrderBy(p => p.Value.Count(v => v.Value > 0)).First().Key;
                //if (conditionBiorept_with_least_missing_values == null) return results;
                //foreach (var conditionBiorep in intensities_by_file)
                //{
                //    normalized_intensities_by_file.Add(conditionBiorep.Key, new Dictionary<ExperimentalProteoform, double>());
                //    List<double> foldChanges = new List<double>();
                //    foreach (var p in Sweet.lollipop.target_proteoform_community.experimental_proteoforms)
                //    {
                //        double conditionBiorepIntensityThis = conditionBiorep.Value[p];
                //        double conditionBiorepIntensity1 = intensities_by_file[conditionBiorept_with_least_missing_values][p];
                //        if (conditionBiorepIntensity1 > 0 && conditionBiorepIntensityThis > 0)
                //        {
                //            foldChanges.Add(conditionBiorepIntensityThis / conditionBiorepIntensity1);
                //        }
                //    }
                //    double medianFoldChange = foldChanges.Median();
                //    double normalizationFactor = 1.0 / medianFoldChange;


                //    foreach (var proteoform in conditionBiorep.Value)
                //    {
                //        normalized_intensities_by_file[conditionBiorep.Key].Add(proteoform.Key, proteoform.Value * normalizationFactor);
                //    }
                //}

            results.Columns.Add("Proteoform Suite ID", typeof(string));
            results.Columns.Add("Proteoform Description", typeof(string));
            results.Columns.Add("Gene Name", typeof(string));
            results.Columns.Add("Accessions", typeof(string));
            results.Columns.Add("PTM Type", typeof(string));
            results.Columns.Add("Begin and End", typeof(string));
            results.Columns.Add("Proteoform Mass");
            results.Columns.Add("Retention Time", typeof(double));
            foreach (string f in files)
            {
                string[] file_info = f.Split('|');
                string column_name = "Condition " + file_info[0] + ", Biorep " + file_info[1] + ", Techrep " + file_info[2];
                results.Columns.Add(column_name, typeof(double));
            }
            foreach (string f in files)
            {
                string[] file_info = f.Split('|');
                string column_name = "Normalized Condition " + file_info[0] + ", Biorep " + file_info[1] + ", Techrep " + file_info[2];
                results.Columns.Add(column_name, typeof(double));
            }

            //intact_mass_ids
            foreach (ExperimentalProteoform e in Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms).Where(p => !p.topdown_id))
            {
                object[] array = new object[8 + 2 * files.Count];
                array[0] = e.accession;
                array[1] = e.linked_proteoform_references == null ? "N/A" : (e.linked_proteoform_references.First() as TheoreticalProteoform).name;
                array[2] = e.linked_proteoform_references == null ? "N/A" : e.linked_proteoform_references.Last().gene_name.primary;
                array[3] = e.linked_proteoform_references == null ? "N/A" : string.Join(", ", (e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList.Select(a => a.Split('_')[0])).Distinct());
                array[4] = e.linked_proteoform_references == null ? "N/A" : e.ptm_set.ptm_description;
                array[5] = e.linked_proteoform_references == null ? "N/A" : e.begin + " to " + e.end;
                array[6] = e.agg_mass;
                array[7] = e.agg_rt;
                int index = 8;
                for (int f = 0; f < files.Count; f++)
                {
                    array[index] = intensities_by_file[files[f]][e];
                    index++;
                }
                for (int f = 0; f < files.Count; f++)
                {
                    array[index] = normalized_intensities_by_file[files[f]][e];
                    index++;
                }
                results.Rows.Add(array);
            }

            foreach (TopDownProteoform e in Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms).Where(p => p.topdown_id))
            {
                object[] array = new object[8 + 2 * files.Count];
                array[0] = e.accession;
                array[1] = e.name;
                array[2] = e.topdown_geneName;
                array[3] = e.accession.Split('_')[0];
                array[4] = e.topdown_ptm_description;
                array[5] = e.topdown_begin + " to " + e.topdown_end;
                array[6] = e.agg_mass;
                array[7] = e.agg_rt;
                int index = 8;
                for (int f = 0; f < files.Count; f++)
                {
                    array[index] = intensities_by_file[files[f]][e];
                    index++;
                }
                for (int f = 0; f < files.Count; f++)
                {
                    array[index] = normalized_intensities_by_file[files[f]][e];
                    index++;
                }
                results.Rows.Add(array);
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
                result_string.AppendLine(string.Join("\t", row.ItemArray));
            }
            return results;
        }

        public static DataTable topdown_results_dataframe()
        {
            DataTable results = new DataTable();
            results.Columns.Add("Observation ID", typeof(string));
            results.Columns.Add("Proteoform Mass");
            results.Columns.Add("Retention Time", typeof(double));
            results.Columns.Add("Aggregated Top-Down Hits", typeof(int));
            results.Columns.Add("PFR Accession", typeof(string));
            results.Columns.Add("Original PFR Accession/full-sequence", typeof(string));
            results.Columns.Add("Description", typeof(string));
            results.Columns.Add("Gene Name", typeof(string));
            results.Columns.Add("UniProt ID", typeof(string));
            results.Columns.Add("Accession", typeof(string));
            results.Columns.Add("PTM Type", typeof(string));
            results.Columns.Add("Begin and End", typeof(string));
            results.Columns.Add("Sequence", typeof(string));
            results.Columns.Add("UniProt-Annotated Modifications");
            results.Columns.Add("Potentially Novel Modifications");
            results.Columns.Add("Top-Down PTM Type Unlocalized", typeof(string));
            results.Columns.Add("Best C-score", typeof(double));
            results.Columns.Add("Best Delta score", typeof(double));
            results.Columns.Add("Best Q-Value", typeof(double));
            results.Columns.Add("Level Number", typeof(string));
            results.Columns.Add("Level Description", typeof(string));
            results.Columns.Add("Top-Down Mass Error", typeof(string));
            results.Columns.Add("Best Scoring Hit", typeof(string));
            results.Columns.Add("Family ID", typeof(string));
            results.Columns.Add("Family", typeof(string));
            results.Columns.Add("Linked Proteoform References", typeof(string));
            results.Columns.Add("Proteoform Suite Description", typeof(string));
            results.Columns.Add("Proteoform Suite Gene Name", typeof(string));
            results.Columns.Add("Proteoform Suite GeneID", typeof(string));
            results.Columns.Add("Proteoform Suite Accessions", typeof(string));
            results.Columns.Add("Proteoform Suite PTM Type", typeof(string));
            results.Columns.Add("Proteoform Suite Begin and End", typeof(string));
            results.Columns.Add("Proteoform Suite Sequence", typeof(string));
            results.Columns.Add("Proteoform Suite Mass Error", typeof(string));
            results.Columns.Add("Same ID as Top-down ID", typeof(bool));

            results.Columns.Add("Bottom-Up PSM Count", typeof(string));
            results.Columns.Add("Different BU PSM Count for Amibguous IDs", typeof(bool));
            results.Columns.Add("Bottom-Up Modifications from Proteoform", typeof(string));
            results.Columns.Add("Bottom-Up Modifications from Protein", typeof(string));
            results.Columns.Add("Peptide-Specific Bottom-Up Modifications", typeof(string));
            results.Columns.Add("Bottom-Up Evidence for Begin", typeof(string));
            results.Columns.Add("Bottom-Up Evidence for End", typeof(string));
            results.Columns.Add("Bottom-Up Evidence for All PTMs", typeof(string));
            results.Columns.Add("Top-Down and Bottom-Up Modification Overlap Description", typeof(string));
            results.Columns.Add("Top-Down and Bottom-Up Modification Overlap Description from Protein", typeof(string));



            foreach (TopDownProteoform td in Sweet.lollipop.topdown_proteoforms)
            {
                results.Rows.Add(
                    td.accession, 
                    td.modified_mass,
                    td.agg_rt,
                    td.topdown_hits.Count(),
                    td.pfr_accession,
                    td.topdown_hits.First().original_pfr_accession,
                    td.name + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.name)) : ""),
                    td.topdown_geneName.primary + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.gene_name.primary)) : "" ),
                    td.uniprot_id + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.uniprot_id)) : "" ),
                    td.accession.Split('_')[0] + (td.ambiguous_topdown_hits.Count > 0 ? String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.accession.Split('_')[0])) : ""),
                    td.topdown_ptm_description + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.ptm_description)) : ""),
                    td.topdown_begin + " to " + td.topdown_end + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.begin + " to " + h.end)) : ""),
                    td.sequence + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.sequence)) : ""),
                    td.topdown_uniprot_mods,
                    td.topdown_novel_mods,
                    (td.topdown_ptm_set.ptm_combination.Count == 0 ? "Unmodified" : string.Join("; ", td.topdown_ptm_set.ptm_combination.Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).OrderBy(m => m)))
                     + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.ptm_list.Count == 0 ? "Unmodified" : string.Join("; ", h.ptm_list.Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).OrderBy(m => m)))) : ""),
                    td.topdown_hits.Max(h => h.score),
                    td.topdown_hits.Max(h => h.deltaScore),
                    td.topdown_hits.Min(h => h.qValue),
                    td.topdown_level,
                    td.topdown_level_description,
                    Math.Round(td.modified_mass - td.theoretical_mass,4) + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => Math.Round(h.reported_mass - h.theoretical_mass))) : ""),
                    td.manual_validation_id,
                    td.family == null ? "N/A" : td.family.family_id.ToString(),
                    td.family != null ? td.family.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() > 1 ? "Ambiguous" : "Identified" : "",
                    td.family != null && td.linked_proteoform_references != null ? string.Join(", ", (td.linked_proteoform_references.Select(p => p.accession))) + (td.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", td.ambiguous_identifications.Select(p => string.Join(", ", p.linked_proteoform_references.Select(a => a.accession))))
                        : "") : "N/A",
                    td.linked_proteoform_references == null ? "N/A" : (td.linked_proteoform_references.First() as TheoreticalProteoform).description + (td.ambiguous_identifications.Count > 0
                                                                          ? " | " + String.Join(" | ", td.ambiguous_identifications.Select(p => (p.theoretical_base as TheoreticalProteoform).description))
                                                                          : ""),

                    td.linked_proteoform_references == null ? "N/A" : (td.linked_proteoform_references.First() as TheoreticalProteoform).gene_name.primary + (td.ambiguous_identifications.Count > 0
                                                                          ? " | " + String.Join(" | ", td.ambiguous_identifications.Select(p => (p.theoretical_base as TheoreticalProteoform).gene_name.primary))
                                                                          : ""),
                    td.linked_proteoform_references == null ? "N/A" : string.Join("; ", (td.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct()) + (td.ambiguous_identifications.Count > 0
                                                                          ? " | " + String.Join(" | ", td.ambiguous_identifications.Select(t => string.Join("; ", (t.theoretical_base as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct())))
                                                                          : ""),
                   td.linked_proteoform_references == null ? "N/A" :  string.Join(", ", (td.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList.Select(a => a.Split('_')[0])).Distinct()) + (td.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", td.ambiguous_identifications.Select(t => string.Join("; ", (t.theoretical_base as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList.Select(a => a.Split('_')[0])).Distinct())))
                        : ""),
                    td.linked_proteoform_references == null ? "N/A" : td.ptm_set.ptm_description + (td.ambiguous_identifications.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_identifications.Select(p => p.ptm_set.ptm_description)) : ""),
                    td.linked_proteoform_references == null ? "N/A" : td.begin + " to " + td.end + (td.ambiguous_identifications.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_identifications.Select(p => p.begin + " to " + p.end)) : ""),
                    td.linked_proteoform_references == null ? "N/A" : ExperimentalProteoform.get_sequence(td.linked_proteoform_references.First() as TheoreticalProteoform, td.begin, td.end)
                    + (td.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", td.ambiguous_identifications.Select(i => ExperimentalProteoform.get_sequence(i.theoretical_base as TheoreticalProteoform, i.begin, i.end)))
                        : ""),
                    td.linked_proteoform_references == null ? "N/A" : td.calculate_mass_error(td.linked_proteoform_references.First() as TheoreticalProteoform, td.ptm_set, td.begin, td.end).ToString()
                                                                      + (td.ambiguous_identifications.Count > 0
                                                                          ? " | " + String.Join(" | ", td.ambiguous_identifications.Select(i => td.calculate_mass_error(i.theoretical_base as TheoreticalProteoform, i.ptm_set, i.begin, i.end).ToString()))
                                                                          : ""),
                   
                    td.correct_id,

                     td.topdown_bottom_up_PSMs.Count.ToString()
                       + (td.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(i => i.bottom_up_PSMs.Count.ToString()))
                    : ""),
                     td.different_ambiguity,
                     td.bu_PTMs,
                     td.bu_PTMs_all_from_protein,
                     td.bu_PTMs_separatepeptides,
                     td.topdown_bottom_up_PSMs.Any(p => p.begin == td.topdown_begin) + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.bottom_up_PSMs.Any(b => b.begin == h.begin))) : ""),
                     td.topdown_bottom_up_PSMs.Any(p => p.end == td.topdown_end) + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => h.bottom_up_PSMs.Any(b => b.end == h.end))) :""),
                     Proteoform.get_bottom_up_evidence_for_all_PTMs(td.topdown_bottom_up_PSMs, td.topdown_ptm_set, true) + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => Proteoform.get_bottom_up_evidence_for_all_PTMs(h.bottom_up_PSMs, new PtmSet(h.ptm_list), true))) :""),
                     TopDownProteoform.get_description(td.topdown_bottom_up_PSMs, td.accession, true, td.topdown_ptm_set) + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => TopDownProteoform.get_description(h.bottom_up_PSMs, h.accession, true, new PtmSet(h.ptm_list)))) : ""),
                     TopDownProteoform.get_description(td.topdown_bottom_up_PSMs, td.accession, false, td.topdown_ptm_set) + (td.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", td.ambiguous_topdown_hits.Select(h => TopDownProteoform.get_description(h.bottom_up_PSMs, h.accession, false, new PtmSet(h.ptm_list)))) : "")
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
                result_string.AppendLine(string.Join("\t", row.ItemArray));
            }
            return results;
        }

        public static DataTable bottomup_results_dataframe()
        {
            DataTable results = new DataTable();
            results.Columns.Add("Protein Accession", typeof(string));
            results.Columns.Add("Uniprot ID", typeof(string));
            results.Columns.Add("Peptide Sequence", typeof(string));
            results.Columns.Add("Begin", typeof(int));
            results.Columns.Add("End", typeof(int));
            results.Columns.Add("PTM List", typeof(string));
            results.Columns.Add("Biological Interest PTM List", typeof(string));
            results.Columns.Add("Shared", typeof(bool));
            results.Columns.Add("Full Sequence PFR", typeof(string));
            results.Columns.Add("Level1 Top-down proteoform Count", typeof(int));
            results.Columns.Add("Unambiguous Intact-Mass proteoform Count", typeof(int));
            results.Columns.Add("Level1 Top-down proteoform IDs", typeof(string));
            results.Columns.Add("Unambiguous Intact-Mass proteoform IDs", typeof(string));
            results.Columns.Add("Level1 Top-down proteoform PTM description", typeof(string));
            results.Columns.Add("Unambiguous Intact-Mass proteoform PTM description", typeof(string));
            results.Columns.Add("Level1 Top-down proteoform begin and end", typeof(string));
            results.Columns.Add("Unambiguous Intact-Mass proteoform being and end", typeof(string));

            var intact_mass_IDs = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(e => !e.topdown_id && e.linked_proteoform_references != null && e.ambiguous_identifications.Count == 0);
            Parallel.ForEach(Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession, accession =>
                {
                    //right now just unambiguous TD ID's...
                    var topdowns_with_accession = Sweet.lollipop.topdown_proteoforms.Where(t => t.topdown_bottom_up_PSMs.Count > 0 && t.topdown_bottom_up_PSMs.First().accession == accession.Key && t.ambiguous_topdown_hits.Count == 0);
                     var intactMass_with_accession = intact_mass_IDs.Where(e => e.bottom_up_PSMs.Count > 0 && e.bottom_up_PSMs.First().accession == accession.Key);
                    var rows = new List<object[]>();
                    foreach (var peptide in accession.Value)
                    {
                        var ptms_bio_interest = peptide.ptm_list.Where(p => p.modification.ModificationType != "Common Fixed" && UnlocalizedModification.bio_interest(p.modification));
                        string ptm_description_bio_interest = (ptms_bio_interest.Count() == 0 ? "Unmodified" : string.Join("; ", ptms_bio_interest.Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position)));
                        if (peptide.ambiguous_matches.Count > 0) continue;
                        var topdown_with_this_peptide = topdowns_with_accession.Where(t => t.topdown_bottom_up_PSMs.Contains(peptide));
                        var intactMass_with_this_peptide = intactMass_with_accession.Where(e => e.bottom_up_PSMs.Contains(peptide));
                        rows.Add(new object[17]{
                            peptide.accession,
                            peptide.uniprot_id,
                            peptide.sequence,
                            peptide.begin,
                            peptide.end,
                            peptide.ptm_description,
                            ptm_description_bio_interest,
                            peptide.shared_protein,
                            peptide.pfr_accession,
                            topdown_with_this_peptide.Count(),
                            intactMass_with_this_peptide.Count(),
                            String.Join("|", topdown_with_this_peptide.Select(t => t.accession)),
                            String.Join("|", intactMass_with_this_peptide.Select(t => t.accession)),
                            String.Join("|", topdown_with_this_peptide.Select(t => t.topdown_ptm_description)),
                            String.Join("|", intactMass_with_this_peptide.Select(t => t.ptm_set.ptm_description)),
                            String.Join("|", topdown_with_this_peptide.Select(t => t.topdown_begin + " to " + t.topdown_end)),
                            String.Join("|", intactMass_with_this_peptide.Select(t => t.begin + " to " + t.end))
                            });
                    }
                    lock (results)
                    {
                        foreach (var row in rows)
                        {
                            results.Rows.Add(row);
                        }
                    }
                });

            StringBuilder result_string = new StringBuilder();
            string header = "";
            foreach (DataColumn column in results.Columns)
            {
                header += column.ColumnName + "\t";
            }
            result_string.AppendLine(header);
            foreach (DataRow row in results.Rows)
            {
                result_string.AppendLine(string.Join("\t", row.ItemArray));
            }
            return results;
        }

        public static DataTable putative_proteoforms_bottom_up()
        {
            DataTable results = new DataTable();
            results.Columns.Add("Protein Accession", typeof(string));
            results.Columns.Add("Proteoform Sequence", typeof(string));
            results.Columns.Add("Proteoform Begin", typeof(int));
            results.Columns.Add("Proteoform End", typeof(int));
            results.Columns.Add("Fragment", typeof(string));
            results.Columns.Add("PTM List", typeof(string));
            results.Columns.Add("Proteoform Mass", typeof(double));
            results.Columns.Add("All Peptides", typeof(string));
            results.Columns.Add("Unmodified Peptides", typeof(string));
            results.Columns.Add("Level-1 Top-Down Identified", typeof(string));
            results.Columns.Add("Ambiguous Top-Down Identified", typeof(string));
            results.Columns.Add("Closest Intact-Mass", typeof(double));
            results.Columns.Add("All Shared", typeof(bool));

            Parallel.ForEach(Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession, kv =>
            {
                var rows = new List<object[]>();
                Sweet.lollipop.theoretical_database.theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number].TryGetValue(kv.Key, out var theoreticals);
                if (theoreticals != null)
                {
                    theoreticals = theoreticals.OrderByDescending(x => x.fragment == "full-met-cleaved").ThenByDescending(x => x.fragment == "full").ThenBy(x => x.begin).ToList();
                    Dictionary<string, List<SpectrumMatch>> peptides_by_unique_mods = new Dictionary<string, List<SpectrumMatch>>();
                    foreach (var peptide in kv.Value.Where(p => p.ambiguous_matches.Count == 0))
                    {
                        var ptm_description = string.Join("; ", peptide.ptm_list.Where(b => UnlocalizedModification.bio_interest(b.modification)).Select(b => UnlocalizedModification.LookUpId(b.modification) + "@" + b.position));
                        if (ptm_description == "") ptm_description = "Unmodified";
                        if (peptides_by_unique_mods.ContainsKey(ptm_description))
                        {
                            peptides_by_unique_mods[ptm_description].Add(peptide);
                        }
                        else
                        {
                            peptides_by_unique_mods.Add(ptm_description, new List<SpectrumMatch>() { peptide });
                        }
                    }

                    Dictionary<TheoreticalProteoform, List<string>> theoreticals_added = new Dictionary<TheoreticalProteoform, List<string>>();
                    foreach (var unique_mod_set in peptides_by_unique_mods.OrderByDescending(k => k.Key.Count(c => c == '@')))
                    {
                        //see if peptides can already be explained by previously added theoreticals
                        List<SpectrumMatch> unexplained_peptides = new List<SpectrumMatch>();
                        if (theoreticals_added.Count == 0) unexplained_peptides.AddRange(unique_mod_set.Value);
                        foreach (var p in unique_mod_set.Value)
                        {
                            bool unexplained = true;
                            foreach (var theo in theoreticals_added.Where(t => p.begin >= t.Key.begin && p.end <= t.Key.end))
                            {
                                foreach (var mod_set in theo.Value.Where(t => t.Contains('@')))
                                {
                                    var mods = mod_set.Split(';').Select(m => m.Trim()).ToList();
                                    var positions = mod_set.Split(';').Select(t => t.Split('@')[1]).ToList();
                                    if (unique_mod_set.Key == "Unmodified")
                                    {
                                        //unmodified so make sure no PTM positions within peptide...
                                        if (positions.All(position => !(Convert.ToInt32(position) >= p.begin && Convert.ToInt32(position) <= p.end)))
                                        {
                                            unexplained = false;
                                        }
                                    }

                                    //if a more modified proteoform explains this modified peptide, don't add also 
                                    else if (mods.Contains(unique_mod_set.Key))
                                    {
                                        //make sure this peptide isn't unmodified at the position of the proteoforms other PTMs
                                        var other_positions = mods.Where(m => m != unique_mod_set.Key).Select(pos => pos.Split('@')[1]);
                                        if (other_positions.All(position => !(Convert.ToInt32(position) >= p.begin && Convert.ToInt32(position) <= p.end)))
                                        {
                                            unexplained = false;
                                        }
                                    }
                                }
                            }
                            if(unexplained)
                            {
                                unexplained_peptides.Add(p);
                            }
                        }


                        List<TheoreticalProteoform> theoretical_to_add = new List<TheoreticalProteoform>();
                        var begin_or_end_peptides = unexplained_peptides.Where(p => p.begin == 1 || theoreticals.Select(t => t.begin).Contains(p.begin) || theoreticals.Select(t => t.end).Contains(p.end));
                       
                        if (begin_or_end_peptides.Count() > 0)
                        {
                            var begins = begin_or_end_peptides.Select(p => p.begin).Distinct();
                            var ends = begin_or_end_peptides.Select(p => p.end).Distinct();
                            foreach (var begin in begins)
                            {
                                if (begin == 1)
                                {
                                    if (theoreticals.Any(t => t.fragment == "full"))
                                    {
                                        var t = theoreticals.Where(x => x.fragment == "full").First();
                                        if (!theoretical_to_add.Contains(t))
                                        {
                                            theoretical_to_add.Add(t);
                                        }
                                    }
                                    else //take the full cleaved sequence and add M
                                    {
                                        var t = theoreticals.Where(x => x.fragment == "full-met-cleaved").First();
                                        TheoreticalProteoform theoretical_with_M = new TheoreticalProteoform("", "", "M" + t.sequence,
                                        theoreticals.First().ExpandedProteinList, 0, 0, new PtmSet(new List<Ptm>()), false, false, null);
                                        theoretical_with_M.fragment = "full";
                                        theoretical_with_M.begin = 1;
                                        theoretical_with_M.end = t.end;
                                        theoretical_to_add.Add(theoretical_with_M);
                                    }
                                }
                                else if (theoreticals.Any(t => t.begin == begin))
                                {
                                    var t = theoreticals.Where(x => x.begin == begin).First();
                                    if (!theoretical_to_add.Contains(t))
                                    {
                                        theoretical_to_add.Add(t);
                                    }
                                }
                            }
                            foreach (var end in ends)
                            {
                                if (theoretical_to_add.Any(t => t.end == end)) //already explained
                                {
                                    continue;
                                }
                                if (theoreticals.Any(t => t.end == end))
                                {
                                    var t = theoreticals.Where(x => x.end == end).First();
                                    if (!theoretical_to_add.Contains(t))
                                    {
                                        theoretical_to_add.Add(t);
                                    }
                                }
                            }
                        }

                        //any leftover peptides unexlpained?
                        if (unexplained_peptides.Any
                        (p => !theoretical_to_add.Any(t => p.begin >= t.begin && p.end <= t.end)))
                        {
                            theoretical_to_add.Add(theoreticals.First());
                        }

                        foreach (var t in theoretical_to_add)
                        {
                            if (theoreticals_added.ContainsKey(t))
                            {
                                theoreticals_added[t].Add(unique_mod_set.Key);
                            }
                            else
                            {
                                theoreticals_added.Add(t, new List<string>() { unique_mod_set.Key });
                            }
                            var peptides = unique_mod_set.Value.Where(p => p.begin >= t.begin && p.end <= t.end);
                            if (peptides.Count() == 0) continue;
                            var unmodified_peptides = peptides_by_unique_mods.ContainsKey("Unmodified") ? peptides_by_unique_mods["Unmodified"].Where(p => p.begin >= t.begin && p.end <= t.end) : new List<SpectrumMatch>();
                            var topdown = Sweet.lollipop.topdown_proteoforms.Where(td => td.topdown_level == 1 && td.accession.Contains(kv.Key) && td.sequence == t.sequence &&
                                          td.topdown_ptm_set.same_ptmset(new PtmSet(unique_mod_set.Value.First().ptm_list.Where(p => UnlocalizedModification.bio_interest(p.modification)).ToList()), false));
                            var ambiguous_topdown = Sweet.lollipop.topdown_proteoforms.Where(td => td.topdown_level > 1).Where(td => (td.accession.Contains(kv.Key) && td.sequence == t.sequence &&
                                             td.topdown_ptm_set.same_ptmset(new PtmSet(unique_mod_set.Value.First().ptm_list.Where(p => UnlocalizedModification.bio_interest(p.modification)).ToList()), false))
                                             || td.ambiguous_topdown_hits.Any(h => h.accession.Contains(kv.Key) && h.sequence == t.sequence && new PtmSet(h.ptm_list).same_ptmset(new PtmSet(unique_mod_set.Value.First().ptm_list.Where(p => UnlocalizedModification.bio_interest(p.modification)).ToList()), false))
                                             );

                            double theoretical_mass = TheoreticalProteoform.CalculateProteoformMass(t.sequence, unique_mod_set.Value.First().ptm_list.Where(p => UnlocalizedModification.bio_interest(p.modification)).ToList());
                            var intact_mass = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(e => !e.topdown_id && (!Sweet.lollipop.neucode_labeled || e.lysine_count == t.sequence.Count(s => s == 'K'))).OrderBy(e => Math.Abs(e.agg_mass - theoretical_mass)).FirstOrDefault();

                            rows.Add(new object[13]{
                                            kv.Key,
                                            t.sequence,
                                            t.begin,
                                            t.end,
                                            t.fragment,
                                            unique_mod_set.Key,
                                            theoretical_mass,
                                            string.Join("; ", peptides.OrderBy(p => p.begin).ThenBy(p => p.end).Select(p => p.begin + "_to_" + p.end)),
                                            string.Join("; ", unmodified_peptides.OrderBy(p => p.begin).ThenBy(p => p.end).Select(p => p.begin + "_to_" + p.end)),
                                            string.Join("; ", topdown.Select(td => td.accession)),
                                            string.Join("; ", ambiguous_topdown.Select(td => td.accession)),
                                            intact_mass != null ? intact_mass.agg_mass : 0,
                                            unique_mod_set.Value.All(p => p.shared_protein)
                            });
                        }
                    }
                }
                lock (results)
                {
                    foreach (var row in rows)
                    {
                        results.Rows.Add(row);
                    }
                }
            });

            return results;
        }

        public static DataTable shared_peptide_results_dataframe()
        {
            DataTable results = new DataTable();
            results.Columns.Add("Peptide Full Sequence", typeof(string));
            results.Columns.Add("Number Proteins in Protein Group", typeof(string));
            results.Columns.Add("Different TD Matches for Different Proteins", typeof(bool));
            results.Columns.Add("# Level-1 Top-down proteoform matches for protein", typeof(string));
            results.Columns.Add("Level-1 Top-down proteoform matches for protein", typeof(string));
            results.Columns.Add("Shared Protein Group", typeof(bool));

            Dictionary<string, List<SpectrumMatch>> by_pfr = new Dictionary<string, List<SpectrumMatch>>();
            foreach (var p in Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Values.SelectMany(b => b))
            {
                if (!p.shared_protein || p.ambiguous_matches.Count > 0) continue;
                List<SpectrumMatch> list;
                by_pfr.TryGetValue(p.original_pfr_accession, out list);
                if (list != null) list.Add(p);
                else
                {
                    by_pfr.Add(p.original_pfr_accession, new List<SpectrumMatch>() { p });
                }
            }
            foreach (var pfr in by_pfr.Keys)
            {
                var peptides_with_this_pfr = by_pfr[pfr];

                bool shared_protein = true;
                foreach (var peptide in peptides_with_this_pfr)
                {
                    if(Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession[peptide.accession].Any(b => !b.shared_protein))
                    {
                        shared_protein = false;
                        break;
                    }
                }

                List<int> td_counts = peptides_with_this_pfr.Select(b => Sweet.lollipop.topdown_proteoforms.Where(td => td.topdown_level == 1).Count(p => p.accession.Split('_')[0] == b.accession)).ToList();
                results.Rows.Add(
                  pfr,
                  peptides_with_this_pfr.Count(),
                  (td_counts.Any(c => c > 0) && td_counts.Any(c => c == 0)),
                  String.Join("|", td_counts),
                  String.Join("|", peptides_with_this_pfr.Select(b => b.accession)),
                  shared_protein
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
                result_string.AppendLine(string.Join("\t", row.ItemArray));
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