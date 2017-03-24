using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class Proteoform
    {
        public string accession { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; } = -1;
        public bool is_target { get; set; } = true;
        public bool is_decoy { get; set; } = false;
        public List<Proteoform> candidate_relatives { get; set; }
        public List<ProteoformRelation> relationships { get; set; } = new List<ProteoformRelation>();
        public ProteoformFamily family { get; set; }
        public PtmSet ptm_set { get; set; } = new PtmSet(new List<Ptm>());
        public string theoretical_reference_accession { get; set; }
        public string theoretical_reference_fragment { get; set; }
        public Proteoform theoretical_reference { get; set; }
        public GeneName gene_name { get; set; }

        public Proteoform(string accession, double modified_mass, int lysine_count, bool is_target)
        {
            this.accession = accession;
            this.modified_mass = modified_mass;
            this.lysine_count = lysine_count;
            this.is_target = is_target;
            this.is_decoy = !is_target;
        }

        public Proteoform(string accession)
        {
            this.accession = accession;
        }

        public List<Proteoform> get_connected_proteoforms()
        {
            return relationships.Where(r => r.peak.peak_accepted).SelectMany(r => r.connected_proteoforms).ToList();
        }

        public List<ExperimentalProteoform> identify_connected_experimentals()
        {
            List<ExperimentalProteoform> identified = new List<ExperimentalProteoform>();
            foreach (ProteoformRelation r in relationships.Where(r => r.peak.peak_accepted).Distinct().ToList())
            {
                ExperimentalProteoform e = r.connected_proteoforms.OfType<ExperimentalProteoform>().FirstOrDefault(p => p != this);
                if (e == null) continue; // Looking at an ET pair, expecting an EE pair

                double mass_tolerance = this.modified_mass / 1000000 * (double)Lollipop.mass_tolerance;
                int sign = Math.Sign(e.modified_mass - modified_mass);
                double deltaM = Math.Sign(r.peak_center_deltaM) < 0 ? r.peak_center_deltaM : sign * r.peak_center_deltaM; // give EE relations the correct sign, but don't switch negative ET relation deltaM's
                ModificationWithMass best_addition = null;
                ModificationWithMass best_loss = null;
                foreach (ModificationWithMass m in Lollipop.uniprotModificationTable.SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList())
                {
                    if (deltaM >= m.monoisotopicMass - mass_tolerance && deltaM <= m.monoisotopicMass + mass_tolerance 
                        && (best_addition == null || Math.Abs(deltaM - m.monoisotopicMass) < Math.Abs(deltaM - best_addition.monoisotopicMass))
                        || best_addition != null && Math.Abs(deltaM - m.monoisotopicMass) == Math.Abs(deltaM - best_addition.monoisotopicMass) && m.modificationType == "Unlocalized")
                    {
                        best_addition = m;
                    }

                    if (this.ptm_set.ptm_combination.Select(ptm => ptm.modification).Contains(m) 
                        && deltaM >= -m.monoisotopicMass - mass_tolerance && deltaM <= -m.monoisotopicMass + mass_tolerance
                        && (best_loss == null || Math.Abs(deltaM - (-m.monoisotopicMass)) < Math.Abs(deltaM - (-best_loss.monoisotopicMass))))
                    {
                        best_loss = m;
                    }
                }

                // If they're the same and someone hasn't labeled 0 difference with a "ModificationWithMass", then label it null
                if (best_addition == null && best_loss == null && Math.Abs(r.peak_center_deltaM) <= mass_tolerance)
                {
                    lock (r) lock (e) assign_pf_identity(e, this, ptm_set, r, null);
                    identified.Add(e);
                }

                if (best_addition == null && best_loss == null)
                    continue;

                PtmSet with_mod_change = best_loss != null ?
                    new PtmSet(new List<Ptm>(this.ptm_set.ptm_combination.Except(this.ptm_set.ptm_combination.Where(ptm => ptm.modification.Equals(best_loss))))) :
                    new PtmSet(new List<Ptm>(this.ptm_set.ptm_combination).Concat(new Ptm[] { new Ptm(-1, best_addition) }).ToList());
                lock (r) lock (e) assign_pf_identity(e, this, with_mod_change, r, best_loss != null ? best_loss : best_addition);
                identified.Add(e);
            }
            return identified;
        }

        private void assign_pf_identity(ExperimentalProteoform e, Proteoform theoretical_reference, PtmSet set, ProteoformRelation r, ModificationWithMass m)
        {
            if (r.represented_modification == null)
            {
                r.represented_modification = m;
            }
            if (e.theoretical_reference == null)
            {
                e.theoretical_reference_accession = this.theoretical_reference_accession;
                e.theoretical_reference_fragment = this.theoretical_reference_fragment;
                e.theoretical_reference = this;
                e.ptm_set = set;
            }
            if (e.gene_name == null)
                e.gene_name = this.gene_name;
            else
                e.gene_name.gene_names.Concat(this.gene_name.gene_names);
        }
    }
}
