namespace ProteoformSuiteInternal
{
    public interface IFileIntensity
    {
        InputFile input_file { get; }
        double intensity_sum { get; }
    }
}