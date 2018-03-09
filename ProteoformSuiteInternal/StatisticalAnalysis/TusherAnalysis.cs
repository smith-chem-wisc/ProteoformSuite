using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public abstract class TusherAnalysis
        : IGoAnalysis
    {
        #region Public Properties

        public List<TusherStatistic> sortedProteoformRelativeDifferences { get; set; } = new List<TusherStatistic>(); // real relative differences (and fold changes) for each selected proteoform; sorted
        public List<List<TusherStatistic>> permutedRelativeDifferences { get; set; } = new List<List<TusherStatistic>>(); // relative differences (and fold changes) for each proteoform for each balanced permutation
        public List<List<TusherStatistic>> sortedPermutedRelativeDifferences { get; set; } = new List<List<TusherStatistic>>(); // sorted relative differences for each balanced permutation
        public List<decimal> avgSortedPermutationRelativeDifferences { get; set; } = new List<decimal>(); // average relative difference across sorted values for each balanced permutation
        public List<TusherStatistic> flattenedPermutedRelativeDifferences { get; set; } = new List<TusherStatistic>(); // all relative differences (and fold changes) from permutations
        public decimal minimumPassingNegativeTestStatistic { get; set; } // the first NEGATIVE relative difference from a selected proteoform that exceeded the offset BELOW the expected relative differences (avg sorted permuted); everything equal to or BELOW this value is considered significant
        public decimal minimumPassingPositiveTestStatisitic { get; set; } // the first POSITIVE relative difference from a selected proteoform that exceeded the offset ABOVE the expected relative differences (avg sorted permuted); everything equal to or ABOVE this value is considered significant
        public double relativeDifferenceFDR { get; set; } // average # of permuted relative differences that pass minimumPassingNegativeTestStatistic & minimumPassingPositiveTestStatisitic, divided by the number of selected proteoforms that passed
        public List<ProteinWithGoTerms> inducedOrRepressedProteins { get; set; } = new List<ProteinWithGoTerms>(); // This is the list of proteins from proteoforms that underwent significant induction or repression
        public GoAnalysis GoAnalysis { get; set; } = new GoAnalysis();
        public QuantitativeDistributions QuantitativeDistributions { get; set; }

        #endregion Public Properties

        #region Public Constructor

        public TusherAnalysis()
        {
            this.QuantitativeDistributions = new QuantitativeDistributions(this);
        }

        #endregion Public Constructor

        #region Public Method

        public abstract void reestablishSignficance(IGoAnalysis analysis);

        #endregion Public Method

    }
}
