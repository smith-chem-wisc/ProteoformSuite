namespace ProteoformSuiteInternal
{
    public interface IBiorepIntensity
    {
        string condition { get; }
        string biorep { get; }
        double intensity_sum { get; set; } // set is used for normalization, although it might be safer to use another property for normalized intensity
    }
}