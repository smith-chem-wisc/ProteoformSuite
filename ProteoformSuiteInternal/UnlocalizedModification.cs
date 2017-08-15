using Proteomics;

namespace ProteoformSuiteInternal
{
    public class UnlocalizedModification
    {

        #region Public Properties

        public double mass { get; set; }
        public string id { get; set; }
        public int ptm_count { get; set; }
        public bool require_proteoform_without_mod { get; set; }
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
            require_proteoform_without_mod = false;

            if (m.modificationType == "Unlocalized")
                ptm_rank = Sweet.lollipop.mod_rank_first_quartile / 2;
            else if (m.modificationType == "Deconvolution Error")
                ptm_rank = Sweet.lollipop.mod_rank_first_quartile;
            else
                ptm_rank = Sweet.lollipop.modification_ranks[m.monoisotopicMass];
        }

        #endregion Public Constructor

    }
}
