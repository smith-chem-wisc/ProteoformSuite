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
        public string name_list { get { return string.Join("; ", theoretical_proteoforms.Select(p => p.name).Distinct()); } }
        public string accession_list { get { return string.Join("; ", theoretical_proteoforms.Select(p => p.accession)); } }
        public string gene_list { get { return string.Join("; ", gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct()); } }
        public string experimentals_list { get { return string.Join("; ", experimental_proteoforms.Select(p => p.accession)); } }
        public string agg_mass_list { get { return string.Join("; ", experimental_proteoforms.Select(p => Math.Round(p.agg_mass, Sweet.lollipop.deltaM_edge_display_rounding))); } }
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

            if (Sweet.lollipop.identify_from_td_nodes)
            {
                foreach (TopDownProteoform topdown in experimental_proteoforms.Where(e => e.topdown_id))
                {
                    Sweet.lollipop.theoretical_database
                        .theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number]
                        .TryGetValue(topdown.accession.Split('_')[0], out var t);
                    if (t != null && t.Count > 0)
                    {
                        TheoreticalProteoform theoretical =
                            new TheoreticalProteoform(topdown.accession, topdown.name, topdown.sequence,
                                t.First().ExpandedProteinList, topdown.modified_mass, topdown.lysine_count,
                                topdown.topdown_ptm_set, true, false, null);
                        theoretical.topdown_theoretical = true;
                        theoretical.new_topdown_proteoform = true;
                        theoretical.begin = topdown.topdown_begin;
                        theoretical.end = topdown.topdown_end;
                        foreach (ExperimentalProteoform e in topdown.identify_connected_experimentals(theoretical, topdown.topdown_begin, topdown.topdown_end,
                             new PtmSet(topdown.topdown_ptm_set.ptm_combination), null))
                        {
                            identified_experimentals.Add(e);
                        }
                    }
                }
            }
            foreach (TheoreticalProteoform t in theoretical_proteoforms.OrderBy(t => t.topdown_theoretical))
            {
                lock (identified_experimentals)
                    foreach (ExperimentalProteoform e in t.identify_connected_experimentals(t, t.begin, t.end, t.ptm_set, t.linked_proteoform_references))
                    {
                        identified_experimentals.Add(e);
                    }
            }

            //Continue looking for new experimental identifications until no more remain to be identified
            List<ExperimentalProteoform> newly_identified_experimentals = new List<ExperimentalProteoform>(identified_experimentals).OrderBy(p => p.relationships.Count(r => r.candidate_ptmset != null) > 0 ? p.relationships.Where(r => r.candidate_ptmset != null).Min(r => Math.Abs(r.DeltaMass - r.candidate_ptmset.mass)) : 1e6).ThenBy(p => p.modified_mass).ToList();
            int last_identified_count = identified_experimentals.Count - 1;
            while (newly_identified_experimentals.Count > 0) //&& identified_experimentals.Count > last_identified_count)
            {
                last_identified_count = identified_experimentals.Count;
                HashSet<ExperimentalProteoform> tmp_new_experimentals = new HashSet<ExperimentalProteoform>();
                foreach (ExperimentalProteoform id_experimental in newly_identified_experimentals)
                {
                    {
                        lock (identified_experimentals) lock (tmp_new_experimentals)
                                foreach (ExperimentalProteoform new_e in id_experimental.identify_connected_experimentals(id_experimental.linked_proteoform_references.First() as TheoreticalProteoform, id_experimental.begin,
                                    id_experimental.end, id_experimental.ptm_set, id_experimental.linked_proteoform_references))
                                {
                                    identified_experimentals.Add(new_e);
                                    tmp_new_experimentals.Add(new_e);
                                }
                    }
                }
                newly_identified_experimentals = new List<ExperimentalProteoform>(tmp_new_experimentals);
            }

            List<string> topdown_ids = Sweet.lollipop.topdown_proteoforms
               .Select(p => p.accession.Split('_')[0].Split('-')[0] + "_" + p.sequence + "_" + string.Join(", ", p.topdown_ptm_set.ptm_combination.Where(m => m.modification.ModificationType != "Deconvolution Error").Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).OrderBy(m => m))).ToList();


            //determine identified experimentals that are adducts
            //checks if any experimentals have same mods as e's ptmset, except e has additional adduct only mods.
            Parallel.ForEach(experimental_proteoforms, e =>
            {
                e.adduct =
                    e.linked_proteoform_references != null
                    && e.ptm_set.ptm_combination.Any(m => Proteoform.modification_is_adduct(m.modification))
                    && experimental_proteoforms.Any(l =>
                        l.linked_proteoform_references != null
                        && l.begin == e.begin && l.end == e.end
                        && l.gene_name.get_prefered_name(Lollipop.preferred_gene_label) == e.gene_name.get_prefered_name(Lollipop.preferred_gene_label)
                        && l.ptm_set.ptm_combination.Count < e.ptm_set.ptm_combination.Count
                        && e.ptm_set.ptm_combination.Where(m => l.ptm_set.ptm_combination.Count(p => UnlocalizedModification.LookUpId(p.modification) == UnlocalizedModification.LookUpId(m.modification)) != e.ptm_set.ptm_combination.Count(p => UnlocalizedModification.LookUpId(p.modification) == UnlocalizedModification.LookUpId(m.modification)))
                            .Count(p => !Proteoform.modification_is_adduct(p.modification))
                            == 0
                        );

                if (e as TopDownProteoform != null) { (e as TopDownProteoform).set_correct_id(); }

                if (e.linked_proteoform_references != null)
                {
                    e.bottom_up_PSMs = Proteoform.get_possible_PSMs(e.linked_proteoform_references.First().accession, e.ptm_set, ExperimentalProteoform.get_sequence(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end), false);
                    foreach(var a in e.ambiguous_identifications)
                    {
                        a.bottom_up_PSMs = Proteoform.get_possible_PSMs(a.theoretical_base.accession, a.ptm_set, ExperimentalProteoform.get_sequence(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end),  false);
                    }

                    var mods = e.ptm_set.ptm_combination.Where(p => !Proteoform.modification_is_adduct(p.modification))
                           .Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).ToList().Distinct().OrderBy(m => m).ToList();
                    e.uniprot_mods = "";
                    string add = "";
                    foreach (string mod in mods)
                    {
                        // positions with mod
                        List<int> theo_ptms = (e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p
                            .OneBasedPossibleLocalizedModifications)
                            .Where(p => p.Key >= e.begin && p.Key <= e.end
                                                         && p.Value.Select(m => UnlocalizedModification.LookUpId(m)).Contains(mod))
                            .Select(m => m.Key).ToList();
                        if (theo_ptms.Count > 0)
                        {
                            add += mod + " @ " + string.Join(", ", theo_ptms) + "; ";
                        }
                        if (e.ptm_set.ptm_combination.Where(ptm => ptm.modification.ModificationType != "Deconvolution Error" && !Proteoform.modification_is_adduct(ptm.modification)).Select(ptm => UnlocalizedModification.LookUpId(ptm.modification))
                                .Count(m => m == mod) > theo_ptms.Count)
                        {
                            e.novel_mods = true;
                        }
                    }
                    e.uniprot_mods += add;
                    if (add.Length == 0) e.uniprot_mods += "N/A";

                    foreach (var ambig_id in e.ambiguous_identifications)
                    {
                        var ambig_mods = ambig_id.ptm_set.ptm_combination.Where(p => !Proteoform.modification_is_adduct(p.modification))
                                   .Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).ToList().Distinct().OrderBy(m => m).ToList();

                        e.uniprot_mods += " | ";
                        add = "";
                        foreach (var mod in ambig_mods)
                        {
                            // positions with mod
                            List<int> theo_ptms = ambig_id.theoretical_base.ExpandedProteinList.SelectMany(p => p
                                .OneBasedPossibleLocalizedModifications)
                                .Where(p => p.Key >= ambig_id.begin && p.Key <= ambig_id.end
                                                             && p.Value.Select(m => UnlocalizedModification.LookUpId(m)).Contains(mod))
                                .Select(m => m.Key).ToList();
                            if (theo_ptms.Count > 0)
                            {
                                add += mod + " @ " + string.Join(", ", theo_ptms) + "; ";
                            }
                            if(ambig_id.ptm_set.ptm_combination.Where(ptm => ptm.modification.ModificationType != "Deconvolution Error" && !Proteoform.modification_is_adduct(ptm.modification)).Select(ptm => UnlocalizedModification.LookUpId(ptm.modification))
                                                                        .Count(m => m == mod) > theo_ptms.Count)
                            {
                                e.novel_mods = true;
                            }
                        }
                        e.uniprot_mods += add;
                        if (add.Length == 0) e.uniprot_mods += "N/A";
                    }
                }

                //determine level #
                e.proteoform_level_description = "";
                if(e.linked_proteoform_references == null)
                {
                    e.proteoform_level = 5;
                    e.proteoform_level_description = "Unidentified";
                }
                else if(e.ambiguous_identifications.Count == 0)
                {
                    if(e.ptm_set.ptm_combination.Count == 0)
                    {
                        e.proteoform_level = 1;

                    }
                    else
                    {
                        e.proteoform_level = 2;
                        e.proteoform_level_description += "PTM localization ambiguity; ";
                    }

                    //check if accessions had been grouped in constructing the theoretical database
                    if ((e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList).Select(a => a.Split('_')[0]).Distinct().Count() > 1)
                    {
                        e.proteoform_level += 1;
                        e.proteoform_level_description += "Gene ambiguity; ";
                    }
                }
                else
                {
                    var unique_accessions = new List<string>() { e.linked_proteoform_references.First().accession.Split('_')[0].Split('-')[0] }.Concat(e.ambiguous_identifications.Select(a => a.theoretical_base.accession.Split('_')[0].Split('-')[0])).Distinct();
                    var unique_sequences = new List<string>() { ExperimentalProteoform.get_sequence(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end) }.
                    Concat(e.ambiguous_identifications.Select(a => ExperimentalProteoform.get_sequence(a.theoretical_base, a.begin, a.end))).Distinct();
                    var unique_PTMs = new List<string>() { string.Join(", ", e.ptm_set.ptm_combination.Where(m => m.modification.ModificationType != "Deconvolution Error").Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).OrderBy(m => m)) }.Concat(e.ambiguous_identifications.Select(a => string.Join(", ", a.ptm_set.ptm_combination.Where(m => m.modification.ModificationType != "Deconvolution Error").Select(ptm => UnlocalizedModification.LookUpId(ptm.modification))))).Distinct();

                    int gene_ambiguity = unique_accessions.Count() > 1 ? 1 : 0;

                    //check if accessions had been grouped in constructing the theoretical database
                    if ((e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList).Select(a => a.Split('_')[0]).Distinct().Count() > 1)
                    {
                        gene_ambiguity = 1;
                    }

                    int sequence_ambiguity = unique_sequences.Count() > 1 ? 1 : 0;
                    int PTM_ambiguity = unique_PTMs.Count() > 1 ? 1 : 0;
                    int PTM_location = e.ptm_set.ptm_combination.Count(m => m.modification.ModificationType != "Deconvolution Error") > 0 || e.ambiguous_identifications.Any(a => a.ptm_set.ptm_combination.Count(m => m.modification.ModificationType != "Deconvolution Error") > 0) ? 1 : 0;

                    e.proteoform_level = 1 + gene_ambiguity + sequence_ambiguity + PTM_ambiguity + PTM_location;
                    if (gene_ambiguity > 0) e.proteoform_level_description += "Gene ambiguity; ";
                    if (sequence_ambiguity > 0)  e.proteoform_level_description += "Sequence ambiguity; ";
                    if (PTM_ambiguity > 0) e.proteoform_level_description += "PTM identity ambiguity; ";
                    if (PTM_location > 0) e.proteoform_level_description += "PTM localization ambiguity; ";

                }
                if (e.proteoform_level == 1)
                {
                    e.proteoform_level_description = "Unambiguous";
                }

                //determine if new intact-mass ID
                e.new_intact_mass_id = false;
                if (!e.topdown_id && e.linked_proteoform_references != null && e.ambiguous_identifications.Count == 0)
                {
                    string this_id = string.Join(",", (e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList.Select(a => a.Split('_')[0])).Distinct()) + "_" + ExperimentalProteoform.get_sequence(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end) + "_" + string.Join(", ", e.ptm_set.ptm_combination.Where(m => m.modification.ModificationType != "Deconvolution Error").Select(ptm => UnlocalizedModification.LookUpId(ptm.modification)).OrderBy(m => m));
                    if (!topdown_ids.Any(t => this_id.Split('_')[0].Split(',').Contains(t.Split('_')[0])
                        && this_id.Split('_')[1] == t.Split('_')[1] && this_id.Split('_')[2] == t.Split('_')[2]))
                    {
                        e.new_intact_mass_id = true;
                    }
                }
            });

            if (Sweet.lollipop.remove_bad_connections)
            {
                if (theoretical_proteoforms.Count > 0 || (Sweet.lollipop.identify_from_td_nodes && experimental_proteoforms.Count(e => e.topdown_id) > 0))
                {
                    Parallel.ForEach(relations, r =>
                    {
                        r.Accepted = r.Identification;
                    });
                }
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
            theoretical_proteoforms = proteoforms.OfType<TheoreticalProteoform>().ToList();
            experimental_proteoforms = proteoforms.OfType<ExperimentalProteoform>().ToList();
            gene_names = proteoforms.Where(p => p as TheoreticalProteoform != null).Select(t => t.gene_name)
                .Concat(proteoforms.Where(p => p as TopDownProteoform != null).Select(t => (t as TopDownProteoform).topdown_geneName)).ToList();
            relations = new HashSet<ProteoformRelation>(proteoforms.SelectMany(p => p.relationships.Where(r => r.Accepted))).ToList();
        }

        #endregion Private Methods
    }
}