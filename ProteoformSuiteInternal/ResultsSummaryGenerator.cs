using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ProteoformSuiteInternal
{
    public static class ResultsSummaryGenerator
    {
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

            report += SaveState.lollipop.proteoform_community.experimental_proteoforms.Length.ToString() + "\tExperimental Proteoforms" + Environment.NewLine;
            report += SaveState.lollipop.proteoform_community.experimental_proteoforms.Count(e => e.accepted).ToString() + "\tAccepted Experimental Proteoforms" + Environment.NewLine;
            report += SaveState.lollipop.theoretical_database.theoretical_proteins.Sum(kv => kv.Value.Length).ToString() + "\tTheoretical Proteins" + Environment.NewLine;
            report += SaveState.lollipop.theoretical_database.expanded_proteins.Length + "\tExpanded Theoretical Proteins" + Environment.NewLine;
            report += SaveState.lollipop.proteoform_community.theoretical_proteoforms.Length.ToString() + "\tTheoretical Proteoforms" + Environment.NewLine + Environment.NewLine;

            report += SaveState.lollipop.et_peaks.Count.ToString() + "\tExperimental-Theoretical Peaks" + Environment.NewLine;
            report += SaveState.lollipop.et_relations.Count.ToString() + "\tExperimental-Theoretical Pairs" + Environment.NewLine;
            report += SaveState.lollipop.et_peaks.Count(p => p.peak_accepted).ToString() + "\tAccepted Experimental-Theoretical Peaks" + Environment.NewLine;
            report += SaveState.lollipop.et_relations.Count(r => r.accepted).ToString() + "\tAccepted Experimental-Theoretical Pairs" + Environment.NewLine;
            report += SaveState.lollipop.ed_relations.Count <= 0 ? Environment.NewLine : SaveState.lollipop.ed_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-Decoy Pairs" + Environment.NewLine + Environment.NewLine;

            report += SaveState.lollipop.ee_peaks.Count.ToString() + "\tExperimental-Experimental Peaks" + Environment.NewLine;
            report += SaveState.lollipop.ee_relations.Count.ToString() + "\tExperimental-Experimental Pairs" + Environment.NewLine;
            report += SaveState.lollipop.ee_peaks.Count(p => p.peak_accepted).ToString() + "\tAccepted Experimental-Experimental Peaks" + Environment.NewLine;
            report += SaveState.lollipop.ee_relations.Count(r => r.accepted).ToString() + "\tAccepted Experimental-Experimental Pairs" + Environment.NewLine;
            report += SaveState.lollipop.ef_relations.Count <= 0 ? Environment.NewLine : SaveState.lollipop.ef_relations.Average(d => d.Value.Count).ToString() + "\tAverage Experimental-False Pairs" + Environment.NewLine + Environment.NewLine;

            report += SaveState.lollipop.proteoform_community.families.Count.ToString() + "\tProteoform Families" + Environment.NewLine;
            List<ProteoformFamily> identified_families = SaveState.lollipop.proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() == 1).ToList();
            List<ProteoformFamily> ambiguous_families = SaveState.lollipop.proteoform_community.families.Where(f => f.gene_names.Select(g => g.ordered_locus).Distinct().Count() > 1).ToList();
            report += identified_families.Count.ToString() + "\tIdentified Families (Correspond to 1 gene)" + Environment.NewLine;
            report += identified_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Identified Families" + Environment.NewLine;
            report += ambiguous_families.Count.ToString() + "\tAmbiguous Families (Correspond to > 1 gene)" + Environment.NewLine;
            report += ambiguous_families.Sum(f => f.experimental_proteoforms.Count).ToString() + "\tExperimental Proteoforms in Ambiguous Families" + Environment.NewLine;
            report += SaveState.lollipop.proteoform_community.families.Count(f => f.proteoforms.Count == 1).ToString() + "\tOrphaned Experimental Proteoforms (Not joined with another proteoform)" + Environment.NewLine + Environment.NewLine;

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

            foreach (ExperimentalProteoform e in SaveState.lollipop.proteoform_community.families.SelectMany(f => f.experimental_proteoforms)
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
    }
}
