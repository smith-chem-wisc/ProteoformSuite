using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
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
        public string name_list { get { return String.Join("; ", theoretical_proteoforms.Select(p => p.name)); } }
        public string accession_list { get { return String.Join("; ", theoretical_proteoforms.Select(p => p.accession)); } }
        public string gene_list { get { return String.Join("; ", gene_names.Select(p => p.get_prefered_name(ProteoformCommunity.preferred_gene_label)).Where(n => n != null).Distinct()); } }
        public string experimentals_list { get { return String.Join("; ", experimental_proteoforms.Select(p => p.accession)); } }
        public string agg_mass_list { get { return String.Join("; ", experimental_proteoforms.Select(p => Math.Round(p.agg_mass, Lollipop.deltaM_edge_display_rounding))); } }
        public int lysine_count { get; set; } = -1;
        public List<ExperimentalProteoform> experimental_proteoforms { get; private set; }
        public List<TheoreticalProteoform> theoretical_proteoforms { get; private set; }
        public List<TopDownProteoform> topdown_proteoforms { get; set; }
        public List<GeneName> gene_names { get; private set; }
        public HashSet<ProteoformRelation> relations { get; private set; }
        public HashSet<Proteoform> proteoforms { get; private set; }
        public double range_RT_in_family { get { try { return (experimental_proteoforms.Max(e => e.agg_rt) - experimental_proteoforms.Min(e => e.agg_rt)); } catch { return 0; } } }
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
            this.proteoforms = new HashSet<Proteoform>(construct_family(new List<Proteoform> { seed }));
            separate_proteoforms();
        }

        public void merge_families()
        {
            IEnumerable<ProteoformFamily> gene_family =
                    from f in Lollipop.proteoform_community.families
                    from n in this.gene_names.Select(g => g.get_prefered_name(ProteoformCommunity.preferred_gene_label)).Distinct()
                    where f.gene_names.Select(g => g.get_prefered_name(ProteoformCommunity.preferred_gene_label)).Contains(n)
                    select f;
            proteoforms = new HashSet<Proteoform>(proteoforms.Concat(gene_family.SelectMany(f => f.proteoforms)));
            separate_proteoforms();
        }

        public void identify_experimentals()
        {
            HashSet<ExperimentalProteoform> identified_experimentals = new HashSet<ExperimentalProteoform>();
            Parallel.ForEach(topdown_proteoforms, t =>
            {
                lock (identified_experimentals)
                    foreach (ExperimentalProteoform e in t.identify_connected_experimentals(Lollipop.all_possible_ptmsets, Lollipop.all_mods_with_mass, true))
                    {
                        identified_experimentals.Add(e);
                    }
            });

            Parallel.ForEach(theoretical_proteoforms, t =>
            {
                lock (identified_experimentals)
                    foreach (ExperimentalProteoform e in t.identify_connected_experimentals(Lollipop.all_possible_ptmsets, Lollipop.all_mods_with_mass, false))
                    {
                        identified_experimentals.Add(e);
                    }
            });

            //Continue looking for new experimental identifications until no more remain to be identified
            List<ExperimentalProteoform> newly_identified_experimentals = new List<ExperimentalProteoform>(identified_experimentals);
            int last_identified_count = identified_experimentals.Count - 1;
            while (newly_identified_experimentals.Count > 0 && identified_experimentals.Count > last_identified_count)
            {
                last_identified_count = identified_experimentals.Count;
                HashSet<ExperimentalProteoform> tmp_new_experimentals = new HashSet<ExperimentalProteoform>();
                Parallel.ForEach(newly_identified_experimentals, id_experimental =>
                {
                    lock (identified_experimentals) lock (tmp_new_experimentals)
                            foreach (ExperimentalProteoform new_e in id_experimental.identify_connected_experimentals(Lollipop.all_possible_ptmsets, Lollipop.all_mods_with_mass, false))
                            {
                                identified_experimentals.Add(new_e);
                                tmp_new_experimentals.Add(new_e);
                            }
                });
                newly_identified_experimentals = new List<ExperimentalProteoform>(tmp_new_experimentals);
            }
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
            this.theoretical_proteoforms = proteoforms.OfType<TheoreticalProteoform>().ToList();
            this.gene_names = theoretical_proteoforms.Select(t => t.gene_name).ToList();
            HashSet<int> lysine_counts = new HashSet<int>(proteoforms.Select(p => p.lysine_count));
            if (lysine_counts.Count == 1) this.lysine_count = lysine_counts.FirstOrDefault();
            this.experimental_proteoforms = proteoforms.OfType<ExperimentalProteoform>().ToList();
            this.topdown_proteoforms = proteoforms.OfType<TopDownProteoform>().ToList();
            this.relations = new HashSet<ProteoformRelation>(proteoforms.SelectMany(p => p.relationships.Where(r => r.accepted)));
        }

        #endregion Private Methods

    }
}
