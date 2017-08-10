using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public interface IAggregatable
    {
        string scan_range { get; set; }
        double rt_apex { get; set; }
        double weighted_monoisotopic_mass { get; set; }
        double intensity_sum { get; set; }
        List<ChargeState> charge_states { get; set; }
        InputFile input_file { get; set; }
        bool accepted { get; set; }
    }
}
