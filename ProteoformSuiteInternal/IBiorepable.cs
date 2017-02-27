namespace ProteoformSuiteInternal
{
    public interface IBiorepable
    {
        InputFile input_file { get; }
        double intensity_sum { get; }
    }
}