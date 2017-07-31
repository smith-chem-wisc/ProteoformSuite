using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class TusherValues2
    {
        #region Tusher Analysis Properties

        public List<BiorepTechrepIntensity> numeratorOriginalBiorepTechrepIntensities { get; set; }
        public List<BiorepTechrepIntensity> denominatorOriginalBiorepTechrepIntensities { get; set; }
        public List<BiorepTechrepIntensity> numeratorImputedBiorepTechrepIntensities { get; set; }
        public List<BiorepTechrepIntensity> denominatorImputedBiorepTechrepIntensities { get; set; }
        public Dictionary<Tuple<string, string, string>, BiorepTechrepIntensity> allBiotechIntensities { get; set; }

        public decimal numeratorIntensitySum { get; set; } = 0;
        public decimal denominatorIntensitySum { get; set; } = 0;
        public decimal scatter { get; set; } = 0;
        public bool significant_tusher { get; set; } = false;
        public decimal relative_difference { get; set; }
        public decimal correspondingAvgSortedRelDiff { get; set; }
        public decimal roughSignificanceFDR { get; set; } = 0;

        #endregion Tusher Analysis Properties

        public static List<List<decimal>> compute_balanced_biorep_permutation_relativeDifferences(Dictionary<string, List<string>> conditionsBioReps, List<InputFile> input_files, string induced_condition, List<ExperimentalProteoform> satisfactoryProteoforms, decimal sKnot_minFoldChange)
        {
            if (!conditionsBioReps.All(x => x.Value.OrderBy(y => y).SequenceEqual(conditionsBioReps.First().Value.OrderBy(z => z))))
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle unbalanced experimental designs.");
            if (conditionsBioReps.Count > 2)
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle experimental designs with more than 2 conditions.");

            List<InputFile> files = Sweet.lollipop.get_files(input_files, Purpose.Quantification).ToList();
            List<string> bioreps = files.Select(f => f.biological_replicate).Distinct().ToList();

            List<Tuple<string, string, string>> all = files.Select(f => new Tuple<string, string, string>(f.lt_condition, f.biological_replicate, f.technical_replicate))
                .Concat(files.Select(f => new Tuple<string, string, string>(f.hv_condition, f.biological_replicate, f.technical_replicate))).ToList();
            List<Tuple<string, string, string>> allInduced = all.Where(x => x.Item1 == induced_condition).ToList();
            List<Tuple<string, string, string>> allUninduced = all.Where(x => x.Item1 != induced_condition).ToList();
            List<IEnumerable<Tuple<string, string, string>>> permutations = ExtensionMethods.Combinations(all, allInduced.Count).ToList();
            List<IEnumerable<Tuple<string, string, string>>> balanced_permutations_induced = permutations.Where(p =>
                !p.SequenceEqual(allInduced) // not the original set
                && bioreps.All(rep => p.Count(x => x.Item2 == rep) == allInduced.Count(x => x.Item2 == rep))) // the number of each biorep should be the same as the original set
                .ToList();

            List<List<decimal>> permutedRelativeDifferences = new List<List<decimal>>(); // each internal list is sorted
            foreach (IEnumerable<Tuple<string, string, string>> induced in balanced_permutations_induced)
            {
                List<decimal> relativeDifferences = new List<decimal>();
                foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
                {
                    List<BiorepIntensity> induced_intensities = induced.Select(x => pf.quant.TusherValues1.allIntensities[x]).ToList();
                    List<BiorepIntensity> uninduced_intensities = pf.quant.TusherValues1.allIntensities.Values.Except(induced_intensities).ToList();
                    relativeDifferences.Add(pf.quant.TusherValues1.getSingleTestStatistic(induced_intensities, uninduced_intensities, pf.quant.TusherValues1.StdDev(induced_intensities, uninduced_intensities), sKnot_minFoldChange));
                }
                permutedRelativeDifferences.Add(relativeDifferences);
            }
            return permutedRelativeDifferences;
        }
    }
}
