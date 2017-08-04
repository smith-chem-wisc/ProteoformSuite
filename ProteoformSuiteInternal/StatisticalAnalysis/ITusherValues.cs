namespace ProteoformSuiteInternal
{
    public interface ITusherValues
    {
        decimal numeratorIntensitySum { get; set; }
        decimal denominatorIntensitySum { get; set; }
        decimal scatter { get; set; }
        bool significant { get; set; }
        decimal relative_difference { get; set; }
        decimal correspondingAvgSortedRelDiff { get; set; }
        decimal roughSignificanceFDR { get; set; }
    }
}
