using System;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class UnlocalizedModification
    {
        public double mass { get; set; }
        public string id { get; set; }
        public int ptm_count { get; set; }
        public bool require_proteoform_without_mod { get; set; }
        public int ptm_rank { get; set; }
        public Modification original_modification { get; set; }

        public UnlocalizedModification(Modification m)
        {
            original_modification = m;
            mass = Math.Round((double)m.MonoisotopicMass, 5);
            id = m.OriginalId;
            ptm_count = 1;
            require_proteoform_without_mod = false;

            if (m.ModificationType == "Common")
                ptm_rank = 2;
            else if (m.ModificationType == "Deconvolution Error")
                ptm_rank = 1;
            else
                ptm_rank = Sweet.lollipop.modification_ranks[Math.Round((double)m.MonoisotopicMass, 5)];
        }

        public static string LookUpId(Modification m)
        {
            return Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(m, out UnlocalizedModification x) ? x.id : m.OriginalId;
        }
    }
}