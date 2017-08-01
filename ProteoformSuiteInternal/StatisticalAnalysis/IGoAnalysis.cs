using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public interface IGoAnalysis
    {
        List<ProteinWithGoTerms> inducedOrRepressedProteins { get; set; }
        GoAnalysis GoAnalysis { get; set; }
    }
}
