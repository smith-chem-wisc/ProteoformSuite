using System;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class Ptm
    {
        public int position { get; private set; } = -1;
        public ModificationWithMass modification { get; private set; } = new ModificationWithMass("Unmodified", new Tuple<string, string>("N/A", "Unmodified"), null, ModificationSites.Any, 0, null, -1, null, null, null);

        public Ptm() // initializes an "un-Modification"
        { }

        public Ptm(int position, ModificationWithMass modification)
        {
            this.position = position;
            this.modification = modification;
        }
    }
}
