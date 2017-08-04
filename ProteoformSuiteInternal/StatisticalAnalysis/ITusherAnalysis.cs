using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public interface ITusherAnalysis
    {
        List<decimal> sortedProteoformRelativeDifferences { get; set; } // real relative differences for each selected proteoform; sorted
        List<List<decimal>> permutedRelativeDifferences { get; set; } // relative differences for each proteoform for each balanced permutation
        List<List<decimal>> sortedPermutedRelativeDifferences { get; set; } // sorted relative differences for each balanced permutation
        List<decimal> avgSortedPermutationRelativeDifferences { get; set; } // average relative difference across sorted values for each balanced permutation
        List<decimal> flattenedPermutedRelativeDifferences { get; set; } // all relative differences from permutations
        decimal minimumPassingNegativeTestStatistic { get; set; } // the first NEGATIVE relative difference from a selected proteoform that exceeded the offset BELOW the expected relative differences (avg sorted permuted) { get; set; } everything equal to or BELOW this value is considered significant
        decimal minimumPassingPositiveTestStatisitic { get; set; } // the first POSITIVE relative difference from a selected proteoform that exceeded the offset ABOVE the expected relative differences (avg sorted permuted) { get; set; } everything equal to or ABOVE this value is considered significant
        double relativeDifferenceFDR { get; set; } // average # of permuted relative differences that pass minimumPassingNegativeTestStatistic & minimumPassingPositiveTestStatisitic, divided by the number of selected proteoforms that passed
        List<ProteinWithGoTerms> inducedOrRepressedProteins { get; set; } // This is the list of proteins from proteoforms that underwent significant induction or repression
        QuantitativeDistributions QuantitativeDistributions { get; set; }

        void reestablishSignficance(IGoAnalysis analysis);
    }
}
