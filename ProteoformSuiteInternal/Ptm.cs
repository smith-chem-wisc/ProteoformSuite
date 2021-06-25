using Proteomics;
using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public class Ptm
    {
        public int position { get; private set; } = -1;
        public Modification modification { get; private set; }

        public Ptm() // initializes an "un-Modification"
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("X", out motif);
            modification = new Modification("Unmodified", null, "Unmodified", null, motif,
                _monoisotopicMass: 0, _databaseReference: new Dictionary<string, IList<string>>(), _keywords: new List<string>());
        }

        public Ptm(int position, Modification modification)
        {
            this.position = position;
            this.modification = modification;
        }
    }
}