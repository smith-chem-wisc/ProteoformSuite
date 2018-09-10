using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public interface IAggregatable
    {
        int min_scan { get; set; }
        int max_scan { get; set; }
        double rt_apex { get; set; }
        double weighted_monoisotopic_mass { get; set; }
        double intensity_sum { get; set; }
        List<ChargeState> charge_states { get; set; }
        InputFile input_file { get; set; }
        bool accepted { get; set; }
    }
}