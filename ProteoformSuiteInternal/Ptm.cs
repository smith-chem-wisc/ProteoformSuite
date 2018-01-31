using Proteomics;
using System;
using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public class Ptm
    {
        public int position { get; private set; } = -1;
        public ModificationWithMass modification { get; private set; }

        public Ptm() // initializes an "un-Modification"
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("X", out motif);
            modification =  new ModificationWithMass("Unmodified", "Unmodified", motif, TerminusLocalization.Any, 0,
           new Dictionary<string, IList<string>>(), new List<string>(), new List<double>(), new List<double>());
        }

        public Ptm(int position, ModificationWithMass modification)
        {
            this.position = position;
            this.modification = modification;
        }
    }
}
