using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class TopDownProteoform : ExperimentalProteoform
    {
        public string uniprot_id { get; set; }
        public string pfr_accession { get; set; }
        public string name { get; set; }
        public string sequence { get; set; }
        public int topdown_begin { get; set; } //position one based
        public int topdown_end { get; set; } //position one based
        public double theoretical_mass { get; set; }
        public List<SpectrumMatch> topdown_hits;

        public List<SpectrumMatch> topdown_bottom_up_PSMs = new List<SpectrumMatch>();

        private PtmSet _topdown_ptm_set = new PtmSet(new List<Ptm>());

        public PtmSet topdown_ptm_set //the ptmset read in with td data
        {
            get
            {
                return _topdown_ptm_set;
            }

            set
            {
                _topdown_ptm_set = value;
                topdown_ptm_description = _topdown_ptm_set == null || _topdown_ptm_set.ptm_combination == null ?
                    "Unknown" :
                    _topdown_ptm_set.ptm_combination.Count(m => m.modification.ModificationType != "Common Fixed") == 0 ?
                        "Unmodified" :
                    string.Join("; ", _topdown_ptm_set.ptm_combination.Where(ptm => ptm.modification.ModificationType != "Common Fixed").OrderBy(ptm => ptm.position).Select(ptm => ptm.position > 0 ? UnlocalizedModification.LookUpId(ptm.modification) + "@" + ptm.position : UnlocalizedModification.LookUpId(ptm.modification)).ToList());
            }
        }
        public GeneName topdown_geneName { get; set; }
        public string topdown_ptm_description { get; set; }
        public ExperimentalProteoform matching_experimental { get; set; } //corresponding experimental
        public bool correct_id { get; set; } //true if the ID given by ProteoformSuite matches ID from topdown
        public List<SpectrumMatch> ambiguous_topdown_hits = new List<SpectrumMatch>();
        public int topdown_level;
        public string topdown_level_description;
        public string topdown_uniprot_mods;
        public bool topdown_novel_mods;
        public bool different_ambiguity //if different bottom-up PSMs for different amgiuous IDs
        {
            get
            {
                List<int> PSM_counts = ambiguous_topdown_hits.Select(i => i.bottom_up_PSMs.Count).ToList();
                PSM_counts.Add(topdown_bottom_up_PSMs.Count);
                PSM_counts = PSM_counts.Distinct().ToList();
                return PSM_counts.Count > 1;
            }
        }

        public string bu_PTMs
        {
            get
            {
                return (topdown_bottom_up_PSMs.Count(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0) == 0
                    ? "N/A"
                    : String.Join(", ",
                          topdown_bottom_up_PSMs.Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0).SelectMany(p => p.ptm_list).Where(m => UnlocalizedModification.bio_interest(m.modification))
                              .Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(m => m).Distinct()))
                      + (ambiguous_topdown_hits.Count > 0
                          ? " | " + String.Join(" | ",
                                ambiguous_topdown_hits.Select(i =>
                                    i.bottom_up_PSMs.Count(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0) == 0
                                        ? "N/A"
                                        : String.Join(", ",
                                            i.bottom_up_PSMs
                                                .Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0).SelectMany(p => p.ptm_list).Where(m => UnlocalizedModification.bio_interest(m.modification))
                                                 .Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(m => m).Distinct())))
                          : "");

            }
        }

        public string bu_PTMs_all_from_protein { get; set; }
        public string setter_bu_PTMs_all_from_protein()
        {
            //only unambiguous PSMs for now....

            return (get_all_psms_from_sequence(accession.Split('_')[0], sequence).Count(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0) == 0
                ? "N/A"
                : String.Join(", ",
                      (get_all_psms_from_sequence(accession.Split('_')[0], sequence).Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0).SelectMany(p => p.ptm_list).Where(m => UnlocalizedModification.bio_interest(m.modification))
                          .Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(m => m).Distinct()))
                  + (ambiguous_topdown_hits.Count > 0
                      ? " | " + String.Join(" | ",
                            ambiguous_topdown_hits.Select(i =>
                                (get_all_psms_from_sequence(i.accession.Split('_')[0], i.sequence).Count(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0) == 0
                                    ? "N/A"
                                    : String.Join(", ",
                                            get_all_psms_from_sequence(i.accession.Split('_')[0], i.sequence)
                                            .Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0)
                                            .SelectMany(p => p.ptm_list).Where(m => UnlocalizedModification.bio_interest(m.modification)).Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(m => m).Distinct()))))
                      : ""));

        }

        public string bu_PTMs_separatepeptides
        {
            get
            {
               return (topdown_bottom_up_PSMs.Count(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0) == 0
                ? "N/A"
                : String.Join(", ",
                     topdown_bottom_up_PSMs.Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0).Select(p => p.ptm_description).Distinct()))
                  + (ambiguous_topdown_hits.Count > 0
                      ? " | " + String.Join(" | ",
                            ambiguous_topdown_hits.Select(i =>
                                i.bottom_up_PSMs.Count(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0) == 0
                                    ? "N/A"
                                    : String.Join(", ",
                                        i.bottom_up_PSMs
                                            .Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0)
                                            .Select(p => p.ptm_description).Distinct())))
              : "");
            }

        }

        private List<SpectrumMatch> get_all_psms_from_sequence(string accession, string sequence)
        {
            var bottom_up_PSMs = new List<SpectrumMatch>();
            //add BU PSMs
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.TryGetValue(accession.Split('_')[0], out var psms);
            if (psms != null)
            {
                bottom_up_PSMs.AddRange(psms.Where(p => p.ambiguous_matches.Count == 0 && sequence.Contains(p.sequence)));
            }
            return bottom_up_PSMs.ToList();
        }


        public TopDownProteoform(string accession, List<SpectrumMatch> hits) : base(accession, null, true)
        {
            var root = hits[0];
            this.name = root.name;
            this.pfr_accession = root.pfr_accession;
            this.ambiguous_topdown_hits = root.ambiguous_matches;
            this.topdown_ptm_set = new PtmSet(root.ptm_list);
            this.uniprot_id = root.uniprot_id;
            this.sequence = root.sequence;
            this.topdown_begin = root.begin;
            this.theoretical_mass = root.theoretical_mass;
            this.topdown_end = root.end;
            this.topdown_hits = hits;
            this.accession = accession + "_" + topdown_begin + "to" + topdown_end + "_1TD";
            this.calculate_td_properties();
            this.lysine_count = sequence.Count(s => s == 'K');
            this.topdown_id = true;
        }

        public TopDownProteoform(TopDownProteoform t) : base(t.accession, null, true)
        {
            this.root = t.root;
            this.name = t.name;
            this.ptm_set = new PtmSet(t.ptm_set.ptm_combination);
            this.topdown_ptm_set = new PtmSet(t.topdown_ptm_set.ptm_combination);
            this.uniprot_id = t.uniprot_id;
            this.sequence = t.sequence;
            this.begin = t.begin;
            this.theoretical_mass = t.theoretical_mass;
            this.end = t.end;
            this.topdown_hits = t.topdown_hits.ToList();
            this.modified_mass = t.modified_mass;
            this.theoretical_mass = t.theoretical_mass;
            this.matching_experimental = t.matching_experimental;
            this.accession = t.accession;
            this.agg_rt = t.agg_rt;
            this.lysine_count = t.lysine_count;
            this.uniprot_id = t.uniprot_id;
            this.pfr_accession = t.pfr_accession;
            this.agg_intensity = t.agg_intensity;
            this.topdown_id = t.topdown_id;
            this.agg_mass = t.agg_mass;
            this.is_target = t.is_target;
            this.topdown_end = t.topdown_end;
            this.topdown_begin = t.topdown_begin;
            this.topdown_geneName = t.topdown_geneName;
        }

        public void calculate_td_properties()
        {
            //correct here for missed monoisotopic mass...
            this.agg_mass = topdown_hits.Select(h => (h.reported_mass- Math.Round(h.reported_mass - h.theoretical_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS)).Average();
            this.modified_mass = this.agg_mass;
            this.agg_rt = topdown_hits.Select(h => h.ms2_retention_time).Average();
            calculate_topdown_level();
            get_uniprot_mods();
        }

        private void get_uniprot_mods()
        {
            var mods = topdown_ptm_set.ptm_combination.Where(p => !Proteoform.modification_is_adduct(p.modification) && p.modification.ModificationType != "Common Fixed")
                          .Select(ptm => UnlocalizedModification.LookUpId(ptm.modification) + "@" + ptm.position).ToList().Distinct().OrderBy(m => m).ToList();
            topdown_uniprot_mods = "";
            string add = "";
            List<string> to_add = new List<string>();
            if (Sweet.lollipop.theoretical_database.theoreticals_by_accession.ContainsKey(Sweet.lollipop.target_proteoform_community.community_number))
            {
                Sweet.lollipop.theoretical_database.theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number].TryGetValue(accession.Split('_')[0], out var matching_theoretical);
                if (matching_theoretical != null)
                {
                    foreach (string mod in mods)
                    {
                        // positions with mod
                        List<int> theo_ptms = matching_theoretical.First().ExpandedProteinList.SelectMany(p => p
                            .OneBasedPossibleLocalizedModifications)
                            .Where(p => p.Key >= topdown_begin && p.Key <= topdown_end
                                                         && p.Value.Select(m => UnlocalizedModification.LookUpId(m)).Contains(mod.Split('@')[0]))
                            .Select(m => m.Key).Distinct().OrderBy(p => p).ToList();
                        if (theo_ptms.Count > 0)
                        {
                           to_add.Add(mod.Split('@')[0] + "@" + string.Join(", ", theo_ptms) + "; ");
                        }
                        if(!theo_ptms.Select(i => mod.Split('@')[0] + "@" + i).Contains(mod))
                        {
                            topdown_novel_mods = true;
                        }
                    }
                    foreach(var add_string in to_add.Distinct())
                    {
                        add += add_string;
                    }
                    topdown_uniprot_mods += add;
                    if (add.Length == 0) topdown_uniprot_mods += "N/A";

                    foreach (var ambig_id in ambiguous_topdown_hits)
                    {
                        to_add = new List<string>();
                        Sweet.lollipop.theoretical_database.theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number].TryGetValue(accession.Split('_')[0], out var matching_ambig_theoretical);
                        if (matching_ambig_theoretical != null)
                        {
                            var ambig_mods = ambig_id.ptm_list.Where(p => !Proteoform.modification_is_adduct(p.modification) && p.modification.ModificationType != "Common Fixed")
                                       .Select(ptm => UnlocalizedModification.LookUpId(ptm.modification) + "@" + ptm.position).ToList().Distinct().OrderBy(m => m).ToList();

                            topdown_uniprot_mods += " | ";
                            add = "";
                            foreach (var mod in ambig_mods)
                            {
                                // positions with mod
                                List<int> theo_ptms = matching_ambig_theoretical.First().ExpandedProteinList.SelectMany(p => p
                                    .OneBasedPossibleLocalizedModifications)
                                    .Where(p => p.Key >= ambig_id.begin && p.Key <= ambig_id.end
                                                                 && p.Value.Select(m => UnlocalizedModification.LookUpId(m)).Contains(mod.Split('@')[0]))
                                    .Select(m => m.Key).Distinct().OrderBy(p => p).ToList();
                                if (theo_ptms.Count > 0)
                                {
                                    to_add.Add(mod.Split('@')[0] + "@" + string.Join(", ", theo_ptms) + "; ");
                                }
                                if (!theo_ptms.Select(i => mod.Split('@')[0] + "@" + i).Contains(mod))
                                {
                                    topdown_novel_mods = true;
                                }
                            }
                            foreach (var add_string in to_add.Distinct())
                            {
                                add += add_string;
                            }
                        }
                        topdown_uniprot_mods += add;
                        if (add.Length == 0) topdown_uniprot_mods += "N/A";
                    }
                }
            }
        }

        public static string get_description(List<SpectrumMatch> bottom_up_psms, string accession, bool proteoform_specific_PSMs, PtmSet set)
        {
            var modified_psms = new List<SpectrumMatch>();
            var all_psms = new List<SpectrumMatch>();
            if (!proteoform_specific_PSMs)
            {
                var bottom_up_PSMs = new List<SpectrumMatch>();
                //add BU PSMs
                Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.TryGetValue(accession.Split('_')[0], out var psms);
                if (psms != null)
                {
                    bottom_up_PSMs.AddRange(psms.Where(p => p.ambiguous_matches.Count == 0));
                }
                modified_psms = bottom_up_PSMs.Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0).ToList();
                all_psms = bottom_up_PSMs.ToList();
            }
            else
            {
                modified_psms = bottom_up_psms.Where(p => p.ptm_list.Count(m => UnlocalizedModification.bio_interest(m.modification)) > 0 && p.ambiguous_matches.Count == 0).ToList();
                all_psms = bottom_up_psms.Where(p => p.ambiguous_matches.Count == 0).ToList();

            }
            if (all_psms.Count == 0)
            {
                return "no BU PSMs";
            }
            if (modified_psms.Count == 0) return "no modified BU PSMs";
            List<string> td_psm_with_location = set.ptm_combination.Where(m => UnlocalizedModification.bio_interest(m.modification)).Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).ToList();
            List<string> bu_psm_with_location = modified_psms.SelectMany(b => b.ptm_list).Where(m => UnlocalizedModification.bio_interest(m.modification)).Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).Distinct().ToList();
            List<string> td_psm_no_location = set.ptm_combination.Where(m => UnlocalizedModification.bio_interest(m.modification)).Select(p => UnlocalizedModification.LookUpId(p.modification)).ToList();
            List<string> bu_psm_no_location = modified_psms.SelectMany(b => b.ptm_list).Where(m => UnlocalizedModification.bio_interest(m.modification)).Select(p => UnlocalizedModification.LookUpId(p.modification)).Distinct().ToList();
            string description = "";
            bool different_location = false;
            bool same_location = false;
            bool bu_doesnt_contain_this_ptm = false;

            foreach (var td in td_psm_with_location)
            {
                if (bu_psm_with_location.Contains(td))
                {
                    same_location = true;
                }
                else if (bu_psm_no_location.Contains(td.Split('@')[0]))
                {
                    different_location = true;
                }
                else
                {
                    bu_doesnt_contain_this_ptm = true;
                }
            }
            bool different_location_bu = false;
            bool different_ptm_bu = false;

            foreach (var bu in bu_psm_with_location)
            {
                if (!td_psm_with_location.Contains(bu))
                {
                    if (td_psm_no_location.Contains(bu.Split('@')[0]))
                    {
                        different_location_bu = true;
                    }
                    else
                    {
                        different_ptm_bu = true;
                    }
                }
            }
            if (bu_doesnt_contain_this_ptm) description += "Other TD PTM. ";
            if (different_ptm_bu) description += "Other BU PTM. ";
            if (same_location) description += "Same location PTM. ";
            if (different_location && !different_location_bu) description += "Additional TD location. ";
            if (!different_location && different_location_bu) description += "Additional BU locations. ";
            if (different_location && different_location_bu) description += "Different locations";

            return description;
        }
        private void calculate_topdown_level()
        {
            topdown_level_description = "";
            if (ambiguous_topdown_hits.Count == 0)
            {
                topdown_level = 1;
                topdown_level_description = "Unambiguous";
            }
            else
            {

                var unique_accessions = new List<string>() { this.accession.Split('_')[0].Split('-')[0] }.Concat(ambiguous_topdown_hits.Select(a => a.accession.Split('_')[0])).Distinct();
                var unique_sequences = new List<string>() { sequence }.Concat(ambiguous_topdown_hits.Select(a => a.sequence)).Distinct();
                var unique_PTM_locations = new List<string>() { string.Join(",", topdown_ptm_set.ptm_combination.Select(p => p.position).OrderBy(n => n)) }.Concat(ambiguous_topdown_hits.Select(h => string.Join(",", h.ptm_list.Select(p => p.position).OrderBy(n => n)))).Distinct();
                var unique_PTM_IDs = new List<string>() { string.Join(",", topdown_ptm_set.ptm_combination.Select(p => UnlocalizedModification.LookUpId(p.modification)).OrderBy(n => n)) }.Concat(ambiguous_topdown_hits.Select(h => string.Join(",", h.ptm_list.Select(p => UnlocalizedModification.LookUpId(p.modification)).OrderBy(n => n)))).Distinct();

                int gene_ambiguity = unique_accessions.Count() > 1 ? 1 : 0;
                int sequence_ambiguity = unique_sequences.Count() > 1 ? 1 : 0;
                int PTM_ambiguity = unique_PTM_IDs.Count() > 1 ? 1 : 0;
                int PTM_location = unique_PTM_locations.Count() > 1 ? 1 : 0;

                if(gene_ambiguity == 0 && sequence_ambiguity == 0 && PTM_ambiguity == 0 && PTM_location == 0)
                {
                    var unique_PTMs = new List<string>() { string.Join(",", topdown_ptm_set.ptm_combination.Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(n => n)) }.Concat(ambiguous_topdown_hits.Select(h => string.Join(",", h.ptm_list.Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(n => n)))).Distinct();
                    if (unique_PTMs.Count() > 1) PTM_location = 1;
                    else PTM_ambiguity = 1;
                }

                if (gene_ambiguity > 0) topdown_level_description += "Gene ambiguity; ";
                if (sequence_ambiguity > 0) topdown_level_description += "Sequence ambiguity; ";
                if (PTM_ambiguity > 0) topdown_level_description += "PTM identity ambiguity; ";
                if (PTM_location > 0) topdown_level_description += "PTM localization ambiguity; ";
                topdown_level = 1 + gene_ambiguity + sequence_ambiguity + PTM_ambiguity + PTM_location;
            }
        }

        public void set_correct_id()
        {
            if (linked_proteoform_references == null || ambiguous_identifications.Count > 0) correct_id = false;
            else
            {
                TheoreticalProteoform t = linked_proteoform_references.First() as TheoreticalProteoform;
                bool matching_accession = t.ExpandedProteinList.SelectMany(p => p.AccessionList).Select(a => a.Split('_')[0]).Contains(accession.Split('_')[0]);
                bool same_begin_and_end = begin == topdown_begin && end == topdown_end;
                bool same_ptm_set = topdown_ptm_set.same_ptmset(ptm_set, true);
                correct_id = matching_accession && same_ptm_set && same_begin_and_end;
            }
        }
    }
}