using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class UnlocalizedModification
    {

        #region Public Properties

        public double mass { get; set; }
        public string id { get; set; }
        public int ptm_count { get; set; }
        public ModificationWithMass original_modification { get; set; }

        #endregion Public Properties

        #region Public Constructor

        public UnlocalizedModification(ModificationWithMass m)
        {
            original_modification = m;
            mass = m.monoisotopicMass;
            id = m.id;
            ptm_count = 1;
        }

        #endregion Public Constructor
    }
}
