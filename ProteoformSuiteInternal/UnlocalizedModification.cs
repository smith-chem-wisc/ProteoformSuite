using Proteomics;

namespace ProteoformSuiteInternal
{
    public class UnlocalizedModification
    {

        #region Public Properties

        public double mass { get; set; }
        public string id { get; set; }
        public int ptm_count { get; set; }
        public int ptm_rank { get; set; }
        public ModificationWithMass original_modification { get; set; }

        #endregion Public Properties

        #region Public Constructor

        public UnlocalizedModification(ModificationWithMass m)
        {
            original_modification = m;
            mass = m.monoisotopicMass;
            id = m.id;
            ptm_count = 1;
            ptm_rank = SaveState.lollipop.modification_ranks[m.monoisotopicMass];

            if (m.modificationType == "FattyAcid" || m.modificationType == "Unlocalized")
                ptm_rank = SaveState.lollipop.mod_rank_first_quartile / 2;
            else if (m.modificationType == "Deconvolution Error")
                ptm_rank = SaveState.lollipop.mod_rank_third_quartile;
        }

        #endregion Public Constructor
    }
}
