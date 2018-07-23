using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class TusherStatistic
    {
        #region Public Properties

        public decimal relative_difference { get; set; }
        public decimal fold_change { get; set; }
        public List<decimal> biorep_fold_changes { get; set; }

        #endregion Public Properties

        #region Public Constructor

        public TusherStatistic(decimal relative_difference, decimal fold_change, List<decimal> biorep_fold_changes)
        {
            this.relative_difference = relative_difference;
            this.fold_change = fold_change;
            this.biorep_fold_changes = biorep_fold_changes;
        }

        #endregion

        #region Public Methods

        public bool is_passing_real(decimal minimumPassingNegativeTestStatistic, decimal minimumPassingPositiveTestStatisitic, string andOrUseFoldChange, bool useFoldChangeCutoff, decimal foldChangeCutoff, bool useAverageFoldChangeCutoff, bool useBiorepFoldChangeCutoff, int minBiorepsWithFoldChange, out bool is_passing_relative_difference, out bool is_passing_fold_change)
        {
            is_passing_relative_difference = relative_difference <= minimumPassingNegativeTestStatistic && relative_difference <= 0 || minimumPassingPositiveTestStatisitic <= relative_difference && relative_difference >= 0;
            return is_passing(is_passing_relative_difference, andOrUseFoldChange, useFoldChangeCutoff, foldChangeCutoff, useAverageFoldChangeCutoff, useBiorepFoldChangeCutoff, minBiorepsWithFoldChange, out is_passing_fold_change);
        }

        public bool is_passing_permutation(decimal minimumPassingNegativeTestStatistic, decimal minimumPassingPositiveTestStatisitic, string andOrUseFoldChange, bool useFoldChangeCutoff, decimal foldChangeCutoff, bool useAverageFoldChangeCutoff, bool useBiorepFoldChangeCutoff, int minBiorepsWithFoldChange, out bool is_passing_relative_difference, out bool is_passing_fold_change)
        {
            is_passing_relative_difference = relative_difference < minimumPassingNegativeTestStatistic && relative_difference <= 0 || minimumPassingPositiveTestStatisitic < relative_difference && relative_difference >= 0;
            return is_passing(is_passing_relative_difference, andOrUseFoldChange, useFoldChangeCutoff, foldChangeCutoff, useAverageFoldChangeCutoff, useBiorepFoldChangeCutoff, minBiorepsWithFoldChange, out is_passing_fold_change);
        }

        #endregion Public Methods

        #region Private Method

        private bool is_passing(bool passing_relative_difference, string andOrUseFoldChange, bool useFoldChangeCutoff, decimal foldChangeCutoff, bool useAverageFoldChangeCutoff, bool useBiorepFoldChangeCutoff, int minBiorepsWithFoldChange, out bool is_passing_fold_change)
        {
            bool passing_average_foldchange = useAverageFoldChangeCutoff && (fold_change >= 1 && fold_change > foldChangeCutoff || fold_change < 1 && (decimal)Math.Pow((double)fold_change, -1) > foldChangeCutoff);
            bool passing_biorep_foldchange = useBiorepFoldChangeCutoff && 
                (biorep_fold_changes.Count(fold_change => fold_change >= 1 && fold_change > foldChangeCutoff) >= minBiorepsWithFoldChange
                || biorep_fold_changes.Count(fold_change => fold_change < 1 && fold_change > 0 && (decimal)Math.Pow((double)fold_change, -1) > foldChangeCutoff) >= minBiorepsWithFoldChange);
            is_passing_fold_change = passing_average_foldchange || passing_biorep_foldchange;
            return
                passing_relative_difference && (!useFoldChangeCutoff || andOrUseFoldChange == "AND" && (passing_average_foldchange || passing_biorep_foldchange))
                || useFoldChangeCutoff && andOrUseFoldChange == "OR" && (passing_relative_difference || passing_average_foldchange || passing_biorep_foldchange);
        }

        #endregion Private Method

    }
}
