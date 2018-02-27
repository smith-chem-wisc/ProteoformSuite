using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class ProteoformFamily
    {
        #region Private Field

        private static int family_counter = 0;

        #endregion Private Field

        #region Private Property

        private Proteoform seed { get; set; }

        #endregion Private Property

        #region Public Property

        public int family_id { get; set; }
        public string name_list { get { return String.Join("; ", theoretical_proteoforms.Select(p => p.name).Distinct()); } }
        public string accession_list { get { return String.Join("; ", theoretical_proteoforms.Select(p => p.accession)); } }
        public string gene_list { get { return String.Join("; ", gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct()); } }
        public string experimentals_list { get { return String.Join("; ", experimental_proteoforms.Select(p => p.accession)); } }
        public string agg_mass_list { get { return String.Join("; ", experimental_proteoforms.Select(p => Math.Round(p.agg_mass, Sweet.lollipop.deltaM_edge_display_rounding))); } }
        public List<ExperimentalProteoform> experimental_proteoforms { get; private set; }
        public List<TheoreticalProteoform> theoretical_proteoforms { get; private set; }
        public List<GeneName> gene_names { get; private set; }
        public List<ProteoformRelation> relations { get; private set; }
        public List<Proteoform> proteoforms { get; private set; }

        #endregion Public Property

        #region Public Constructor

        public ProteoformFamily(Proteoform seed)
        {
            family_counter++;
            this.family_id = family_counter;
            this.seed = seed;
        }

        #endregion Public Constructor

        #region Public Methods

        public void construct_family()
        {
            proteoforms = new HashSet<Proteoform>(construct_family(new List<Proteoform> { seed })).ToList();
            separate_proteoforms();
        }

        public static void reset_family_counter()
        {
            family_counter = 0;
        }

        public void merge_families(List<ProteoformFamily> families)
        {
            List<ProteoformFamily> gene_family = merge_families(new List<ProteoformFamily> { this }, new List<ProteoformFamily>(families));
            proteoforms = new HashSet<Proteoform>(proteoforms.Concat(gene_family.SelectMany(f => f.proteoforms))).ToList();
            separate_proteoforms();
        }

        public List<ProteoformFamily> merge_families(List<ProteoformFamily> seed, List<ProteoformFamily> families)
        {
            IEnumerable<ProteoformFamily> gene_expansion =
               (from f in families
                from n in seed.SelectMany(s => s.gene_names.Select(g => g.get_prefered_name(Lollipop.preferred_gene_label))).Distinct()
                where f.gene_names.Select(g => g.get_prefered_name(Lollipop.preferred_gene_label)).Contains(n)
                select f
               ).ToList().Except(seed);
            if (gene_expansion.Count() == 0) { return seed; }
            seed.AddRange(gene_expansion);
            return merge_families(seed, families);
        }

        public void identify_experimentals()
        {
            HashSet<ExperimentalProteoform> identified_experimentals = new HashSet<ExperimentalProteoform>();
            foreach (TheoreticalProteoform t in theoretical_proteoforms)
            {
                lock (identified_experimentals)
                    foreach (ExperimentalProteoform e in t.identify_connected_experimentals(Sweet.lollipop.theoretical_database.all_possible_ptmsets, Sweet.lollipop.theoretical_database.all_mods_with_mass))
                    {
                        identified_experimentals.Add(e);
                    }
            }

            //Continue looking for new experimental identifications until no more remain to be identified
            List<ExperimentalProteoform> newly_identified_experimentals = new List<ExperimentalProteoform>(identified_experimentals).OrderBy(p => p.relationships.Count(r => r.candidate_ptmset != null) > 0 ? p.relationships.Where(r => r.candidate_ptmset != null).Min(r => Math.Abs(r.DeltaMass - r.candidate_ptmset.mass)) : 1e6).ThenBy(p => p.modified_mass).ToList();
            int last_identified_count = identified_experimentals.Count - 1;
            while (newly_identified_experimentals.Count > 0 && identified_experimentals.Count > last_identified_count)
            {
                last_identified_count = identified_experimentals.Count;
                HashSet<ExperimentalProteoform> tmp_new_experimentals = new HashSet<ExperimentalProteoform>();
                foreach (ExperimentalProteoform id_experimental in newly_identified_experimentals)
                {
                    lock (identified_experimentals) lock (tmp_new_experimentals)
                            foreach (ExperimentalProteoform new_e in id_experimental.identify_connected_experimentals(Sweet.lollipop.theoretical_database.all_possible_ptmsets, Sweet.lollipop.theoretical_database.all_mods_with_mass))
                            {
                                identified_experimentals.Add(new_e);
                                tmp_new_experimentals.Add(new_e);
                            }
                }
                newly_identified_experimentals = new List<ExperimentalProteoform>(tmp_new_experimentals);
            }

            //determine identified experimentals that are adducts
            //checks if any experimentals have same mods as e's ptmset, except e has additional adduct only mods.
            Parallel.ForEach(experimental_proteoforms, e =>
            {
                e.adduct =
                    e.linked_proteoform_references != null
                    && e.ptm_set.ptm_combination.Any(m =>
                        m.modification.id == "Sulfate Adduct"
                        || m.modification.id == "Acetone Artifact (Unconfirmed)"
                        || m.modification.id == "Hydrogen Dodecyl Sulfate")
                    && experimental_proteoforms.Any(l =>
                        l.linked_proteoform_references != null
                        && l.gene_name.get_prefered_name(Lollipop.preferred_gene_label) == e.gene_name.get_prefered_name(Lollipop.preferred_gene_label)
                        && l.ptm_set.ptm_combination.Count < e.ptm_set.ptm_combination.Count
                        && e.ptm_set.ptm_combination.Where(m => l.ptm_set.ptm_combination.Count(p => p.modification.id == m.modification.id) != e.ptm_set.ptm_combination.Count(p => p.modification.id == m.modification.id))
                            .Count(p =>
                                p.modification.modificationType != "Deconvolution Error"
                                && p.modification.id != "Sulfate Adduct"
                                && p.modification.id != "Acetone Artifact (Unconfirmed)"
                                && p.modification.id != "Hydrogen Dodecyl Sulfate")
                            == 0
                        );
                if (e as TopDownProteoform != null) { (e as TopDownProteoform).set_correct_id(); }
                if (e.linked_proteoform_references != null) { e.mass_error = e.calculate_mass_error(); }
            });
        }

        #endregion Public Methods

        #region Private Methods

        private List<Proteoform> construct_family(List<Proteoform> seed)
        {
            List<Proteoform> seed_expansion = seed.SelectMany(p => p.get_connected_proteoforms()).Except(seed).ToList();
            if (seed_expansion.Count == 0) return seed;
            seed.AddRange(seed_expansion);
            return construct_family(seed);
        }

        private void separate_proteoforms()
        {
            theoretical_proteoforms = proteoforms.OfType<TheoreticalProteoform>().ToList();
            experimental_proteoforms = proteoforms.OfType<ExperimentalProteoform>().ToList();
            gene_names = proteoforms.Where(p => p as TopDownProteoform != null || p as TheoreticalProteoform != null).Select(t => t.gene_name).ToList();
            relations = new HashSet<ProteoformRelation>(proteoforms.SelectMany(p => p.relationships.Where(r => r.Accepted))).ToList();
        }

        #endregion Private Methods
    }
}