namespace ProteoformSuiteInternal
{
    public interface IBiorepIntensity
    {
        string condition { get; }
        string biorep { get; }
        double intensity_sum { get; }
    }
}
