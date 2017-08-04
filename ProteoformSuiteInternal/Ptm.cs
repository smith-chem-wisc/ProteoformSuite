using Proteomics;
using System;

namespace ProteoformSuiteInternal
{
    public class Ptm
    {
        public int position { get; private set; } = -1;
        public ModificationWithMass modification { get; private set; } = new ModificationWithMass("Unmodified", new Tuple<string, string>("N/A", "Unmodified"), null, TerminusLocalization.Any, 0, null, null, null, null);

        public Ptm() // initializes an "un-Modification"
        { }

        public Ptm(int position, ModificationWithMass modification)
        {
            this.position = position;
            this.modification = modification;
        }
    }
}
