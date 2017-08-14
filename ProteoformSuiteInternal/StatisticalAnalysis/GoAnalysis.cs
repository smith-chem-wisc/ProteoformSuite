using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class GoAnalysis
    {
        #region GO-TERMS AND GO-TERM SIGNIFICANCE Public Fields

        public List<GoTermNumber> goTermNumbers = new List<GoTermNumber>();//these are the count and enrichment values for TusherAnalysis1
        public bool allTheoreticalProteins = false; // this sets the group used for background. True if all Proteins in the theoretical database are used. False if only proteins observed in the study are used.
        public bool allDetectedProteins = false; // this sets the group used for background. True if all Proteins in the theoretical database are used. False if only proteins observed in the study are used.
        public string backgroundProteinsList = "";
        public decimal maxGoTermFDR = 0.05m;
        public decimal minProteoformIntensity = 0; // 500000m;
        public decimal minProteoformFoldChange = 0; // 1m;

        #endregion GO-TERMS AND GO-TERM SIGNIFICANCE Public Fields

        #region GO-TERMS AND GO-TERM SIGNIFICANCE

        public void GO_analysis(List<ProteinWithGoTerms> inducedOrRepressedProteins)
        {
            List<ProteinWithGoTerms> backgroundProteinsForGoAnalysis;
            if (backgroundProteinsList != null && backgroundProteinsList != "" && File.Exists(backgroundProteinsList))
            {
                string[] protein_accessions = File.ReadAllLines(backgroundProteinsList).Select(acc => acc.Trim()).ToArray();
                backgroundProteinsForGoAnalysis = Sweet.lollipop.theoretical_database.expanded_proteins.Where(p => p.AccessionList.Any(acc => protein_accessions.Contains(acc.Split('_')[0]))).DistinctBy(pwg => pwg.Accession.Split('_')[0]).ToList();
            }
            else
            {
                backgroundProteinsForGoAnalysis = allTheoreticalProteins ?
                    Sweet.lollipop.theoretical_database.expanded_proteins.DistinctBy(pwg => pwg.Accession.Split('_')[0]).ToList() :
                    allDetectedProteins ?
                        Sweet.lollipop.observedProteins :
                        Sweet.lollipop.quantifiedProteins;
            }
            goTermNumbers = getGoTermNumbers(inducedOrRepressedProteins, backgroundProteinsForGoAnalysis);
            calculateGoTermFDR(goTermNumbers);
        }

        public List<GoTermNumber> getGoTermNumbers(List<ProteinWithGoTerms> inducedOrRepressedProteins, List<ProteinWithGoTerms> backgroundProteinSet) //These are only for "interesting proteins", which is the set of proteins induced or repressed beyond a specified fold change, intensity and below FDR.
        {
            Dictionary<string, int> goSignificantCounts = fillGoDictionary(inducedOrRepressedProteins);
            Dictionary<string, int> goBackgroundCounts = fillGoDictionary(backgroundProteinSet);
            return inducedOrRepressedProteins.SelectMany(p => p.GoTerms).DistinctBy(g => g.Id).Select(g =>
                new GoTermNumber(
                    g,
                    goSignificantCounts.ContainsKey(g.Id) ? goSignificantCounts[g.Id] : 0,
                    inducedOrRepressedProteins.Count,
                    goBackgroundCounts.ContainsKey(g.Id) ? goBackgroundCounts[g.Id] : 0,
                    backgroundProteinSet.Count
                )).ToList();
        }

        private Dictionary<string, int> fillGoDictionary(List<ProteinWithGoTerms> proteinSet)
        {
            Dictionary<string, int> goCounts = new Dictionary<string, int>();
            foreach (ProteinWithGoTerms p in proteinSet)
            {
                foreach (string goId in p.GoTerms.Select(g => g.Id).Distinct())
                {
                    if (goCounts.ContainsKey(goId))
                        goCounts[goId]++;
                    else
                        goCounts.Add(goId, 1);
                }
            }
            return goCounts;
        }

        public static void calculateGoTermFDR(List<GoTermNumber> gtns)
        {
            List<double> pvals = gtns.Select(g => g.p_value).ToList();
            pvals.Sort();
            Parallel.ForEach(gtns, g => g.by = GoTermNumber.benjaminiYekutieli(gtns.Count, pvals, g.p_value));
        }

        #endregion GO-TERMS AND GO-TERM SIGNIFICANCE
    }
}
