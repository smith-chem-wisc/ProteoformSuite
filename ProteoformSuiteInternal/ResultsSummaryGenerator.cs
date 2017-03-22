using System;
using System.Linq;

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
            foreach (Purpose p in Lollipop.input_files.Select(f => f.purpose).Distinct())
            {
                report += p.ToString() + ":" + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.get_files(Lollipop.input_files, p).Select(f => f.filename + f.extension + "\t" + f.complete_path)) + Environment.NewLine + Environment.NewLine;
            }
            return header + report;
        }

        public static string counts()
        {
            string report = "";

            report += Lollipop.raw_experimental_components.Count.ToString() + "\tRaw Experimental Components" + Environment.NewLine;
            report += Lollipop.raw_quantification_components.Count.ToString() + "\tRaw Quantitative Components" + Environment.NewLine;
            report += Lollipop.raw_neucode_pairs.Count.ToString() + "\tNeuCode Pairs" + Environment.NewLine + Environment.NewLine;

            report += Lollipop.proteoform_community.experimental_proteoforms.Length.ToString() + "\tExperimental Proteoforms" + Environment.NewLine;
            report += Lollipop.theoretical_proteins.Sum(kv => kv.Value.Length).ToString() + "\tTheoretical Proteins" + Environment.NewLine;
            report += Lollipop.expanded_proteins.Length + "\tExpanded Theoretical Proteins" + Environment.NewLine;
            report += Lollipop.proteoform_community.theoretical_proteoforms.Length.ToString() + "\tTheoretical Proteoforms" + Environment.NewLine + Environment.NewLine;

            report += Lollipop.et_relations.Count.ToString() + "\tExperimental-Theoretical Pairs" + Environment.NewLine;
            report += Lollipop.et_peaks.Count.ToString() + "\tExperimental-Theoretical Peaks" + Environment.NewLine;
            report += Lollipop.et_peaks.Count(p => p.peak_accepted).ToString() + "\tAccepted Experimental-Theoretical Peaks" + Environment.NewLine + Environment.NewLine;

            report += Lollipop.ee_relations.Count.ToString() + "\tExperimental-Experimental Pairs" + Environment.NewLine;
            report += Lollipop.ee_peaks.Count.ToString() + "\tExperimental-Experimental Peaks" + Environment.NewLine;
            report += Lollipop.ee_peaks.Count(p => p.peak_accepted).ToString() + "\tAccepted Experimental-Experimental Peaks" + Environment.NewLine + Environment.NewLine;

            report += Lollipop.proteoform_community.families.Count.ToString() + "\tProteoform Families" + Environment.NewLine;
            report += Lollipop.proteoform_community.families.Count(f => f.theoretical_count > 0).ToString() + "\tIdentified Families" + Environment.NewLine;
            report += Lollipop.proteoform_community.families.Count(f => f.proteoforms.Count > 1).ToString() + "\tExperimental Proteoforms in Families" +  Environment.NewLine;
            report += Lollipop.proteoform_community.families.Count(f => f.proteoforms.Count == 1).ToString() + "\tOrphaned Experimental Proteoforms" + Environment.NewLine + Environment.NewLine;

            report += Lollipop.satisfactoryProteoforms.Count.ToString() + "\tQuantified Experimental Proteoforms (Threshold for Quantification: " + Lollipop.minBiorepsWithObservations.ToString() + " = " + Lollipop.observation_requirement + ")" + Environment.NewLine;
            report += Lollipop.satisfactoryProteoforms.Count(p => p.quant.significant).ToString() + "\tExperimental Proteoforms with Significant Change (Threshold for Significance: Log2FoldChange > " + Lollipop.minProteoformFoldChange.ToString() + ", & Total Intensity from Quantification > " + Lollipop.minProteoformIntensity.ToString() + ", & Q-Value < " + Lollipop.minProteoformFDR.ToString() + ")" + Environment.NewLine + Environment.NewLine;

            report += Lollipop.getInterestingFamilies(Lollipop.satisfactoryProteoforms, Lollipop.minProteoformFoldChange, Lollipop.minProteoformFDR, Lollipop.minProteoformIntensity).Count.ToString() + "\tProteoform Families with Significant Change" + Environment.NewLine;
            report += Lollipop.inducedOrRepressedProteins.Count.ToString() + "\tIdentified Proteins with Significant Change" + Environment.NewLine;
            report += Lollipop.goTermNumbers.Count(g => g.by < (double)Lollipop.minProteoformFDR).ToString() + "\tGO Terms of Significance (Benjimini-Yekeulti p-value < " + Lollipop.minProteoformFDR.ToString() + "): " + Environment.NewLine + Environment.NewLine;

            return report;
       }

        public static string proteins_of_significance()
        {
            return "Identified Proteins with Significant Change: " + Environment.NewLine 
                + String.Join(Environment.NewLine, Lollipop.inducedOrRepressedProteins.Select(p => p.Accession).Distinct().OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;
        }

        public static string go_terms_of_significance()
        {
            return "GO Terms of Significance (Benjimini-Yekeulti p-value < " + Lollipop.minProteoformFDR.ToString() + "): " + Environment.NewLine 
                + String.Join(Environment.NewLine, Lollipop.goTermNumbers.Where(g => g.by < (double)Lollipop.minProteoformFDR).Select(g => g.ToString()).OrderBy(x => x)) + Environment.NewLine + Environment.NewLine;

        }
    }
}
