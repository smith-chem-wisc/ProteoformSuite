namespace ProteoformSuiteInternal
{
    public class TusherStatistic
    {
        #region Public Properties

        public decimal relative_difference { get; set; }
        public decimal fold_change { get; set; }

        #endregion Public Properties

        #region Public Constructor

        public TusherStatistic(decimal relative_difference, decimal fold_change)
        {
            this.relative_difference = relative_difference;
            this.fold_change = fold_change;
        }

        #endregion

    }
}
