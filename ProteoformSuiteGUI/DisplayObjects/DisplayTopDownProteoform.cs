using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayTopDownProteoform : DisplayObject
    {
        #region Public Constructors

        public DisplayTopDownProteoform(TopDownProteoform t)
            : base(t)
        {
            this.t = t;
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly TopDownProteoform t;

        #endregion Private Fields

        #region Public Properties

        public string Accession
        {
            get
            {
                return t.accession;
            }
        }

        public double modified_mass
        {
            get { return t.modified_mass; }
        }


        public double retentionTime
        {
            get { return t.agg_rt; }
        }

        public int Observations
        {
            get { return t.topdown_hits.Count; }
        }


        public string PFR_accession
        {
            get
            {
                return t.pfr_accession + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.pfr_accession))
                           : "");
            }
        }

        public string Description
        {
            get
            {
                return t.name + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => h.name)) : "");

            }
        }

        public string gene_name
        {
            get
            {
                return t.topdown_geneName.primary + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => h.gene_name.primary)) : "");

            }
        }

        public string GeneID
        {
            get
            {
                return string.Join("; ", Sweet.lollipop.theoretical_database.theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number][t.accession.Split('_')[0].Split('-')[0]].First().ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct())
                     + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => string.Join("; ", Sweet.lollipop.theoretical_database.theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number][h.accession.Split('_')[0].Split('-')[0]].First().ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct()))) : "");
            }
        }

        public string grouped_accessions
        {
            get
            {
                return t.accession.Split('_')[0].Split('-')[0] + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => h.accession.Split('_')[0].Split('-')[0])) : "");
            }
        }


        public string ptm_description
        {
            get
            {
                return t.topdown_ptm_description + (t.ambiguous_topdown_hits.Count > 0
                     ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.ptm_description))
                     : "");
            }
        }


        public string begin_and_end
        {
            get
            {
               return t.topdown_begin + " to " + t.topdown_end + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => h.begin + " to " + h.end)) : "");
            }
        }

        public string Sequence
        {
            get
            {
                return t.sequence + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.sequence))
                           : "");
            }
        }

        public string uniprot_mods
        {
            get
            {
                return t.topdown_uniprot_mods;
            }
        }

        public bool potentially_novel
        {
            get
            {
                return t.topdown_novel_mods;
            }
        }

        public string bu_PSMs_count
        {
            get
            {
                return Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin, t.topdown_end).Count.ToString()
                       + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(i => Proteoform.get_possible_PSMs(i.accession.Split('_')[0], new PtmSet(i.ptm_list), i.begin, i.end).Count.ToString()))
                    :"");
            }
        }

        public string bu_PSMs
        {
            //only unambiguous PSMs for now....
            get
            {
                //{
                //    return (Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin,
                //               t.topdown_end).Count(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0) == 0
                //        ? "N/A"
                //        : String.Join(", ",
                //              Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin,
                //                  t.topdown_end).Where(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0).Select(p => p.ptm_description).Distinct()))
                //          + (t.ambiguous_topdown_hits.Count > 0
                //              ? " | " + String.Join(" | ",
                //                    t.ambiguous_topdown_hits.Select(i =>
                //                        Proteoform.get_possible_PSMs(i.accession.Split('_')[0], new PtmSet(i.ptm_list),
                //                            i.begin, i.end).Count(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0) == 0
                //                            ? "N/A"
                //                            : String.Join(", ",
                //                                Proteoform.get_possible_PSMs(i.accession.Split('_')[0],
                //                                        new PtmSet(i.ptm_list), i.begin, i.end)
                //                                    .Where(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0)
                //                                    .Select(p => p.ptm_description).Distinct())))
                //              : "");

                return (Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin,
                           t.topdown_end).Count(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0) == 0
                    ? "N/A"
                    : String.Join(", ",
                          Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin,
                              t.topdown_end).Where(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0).SelectMany(p => p.ptm_list)
                              .Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(m => m).Distinct()))
                      + (t.ambiguous_topdown_hits.Count > 0
                          ? " | " + String.Join(" | ",
                                t.ambiguous_topdown_hits.Select(i =>
                                    Proteoform.get_possible_PSMs(i.accession.Split('_')[0], new PtmSet(i.ptm_list),
                                        i.begin, i.end).Count(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0) == 0
                                        ? "N/A"
                                        : String.Join(", ",
                                            Proteoform.get_possible_PSMs(i.accession.Split('_')[0],
                                                    new PtmSet(i.ptm_list), i.begin, i.end)
                                                .Where(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0).SelectMany(p => p.ptm_list)
                                                 .Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(m => m).Distinct())))
                          : "");

            }
        }



        public string bu_PSMs_all_from_protein
        {
            //only unambiguous PSMs for now....
            get
            {
                
                return (get_psms(t.accession.Split('_')[0]).Count(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0) == 0
                    ? "N/A"
                    : String.Join(", ",
                          (get_psms(t.accession.Split('_')[0]).Where(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0).SelectMany(p => p.ptm_list)
                              .Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(m => m).Distinct()))
                      + (t.ambiguous_topdown_hits.Count > 0
                          ? " | " + String.Join(" | ",
                                t.ambiguous_topdown_hits.Select(i =>
                                    (get_psms(i.accession.Split('_')[0]).Count(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0) == 0
                                        ? "N/A"
                                        : String.Join(", ",
                                                get_psms(i.accession.Split('_')[0])
                                                .Where(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0)
                                                .SelectMany(p => p.ptm_list).Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).OrderBy(m => m).Distinct()))))
                          : ""));

            }
        }

        public string BU_PSMS_separatepeptides
        {
            get
            {

                return (Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin,
                           t.topdown_end).Count(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0) == 0
                    ? "N/A"
                    : String.Join(", ",
                          Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin,
                              t.topdown_end).Where(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0).Select(p => p.ptm_description).Distinct()))
                      + (t.ambiguous_topdown_hits.Count > 0
                          ? " | " + String.Join(" | ",
                                t.ambiguous_topdown_hits.Select(i =>
                                    Proteoform.get_possible_PSMs(i.accession.Split('_')[0], new PtmSet(i.ptm_list),
                                        i.begin, i.end).Count(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0) == 0
                                        ? "N/A"
                                        : String.Join(", ",
                                            Proteoform.get_possible_PSMs(i.accession.Split('_')[0],
                                                    new PtmSet(i.ptm_list), i.begin, i.end)
                                                .Where(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0)
                                                .Select(p => p.ptm_description).Distinct())))
                              : "");
            }
                
        }

        private List<SpectrumMatch> get_psms(string accession)
        {
            var bottom_up_PSMs = new List<SpectrumMatch>();
            //add BU PSMs
            Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.TryGetValue(accession.Split('_')[0].Split('-')[0], out var psms);
            if (psms != null)
            {
                bottom_up_PSMs.AddRange(psms.Where(p => p.ambiguous_matches.Count == 0));
            }
            return bottom_up_PSMs.ToList();
        }

        public string seq_ptm_specific
        {
            //only unambiguous psms for now
            get
            {
                return get_description(t.accession, t.topdown_ptm_set, t.topdown_begin, t.topdown_end, true) + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => get_description(h.accession, new PtmSet(h.ptm_list), h.begin, h.end, true))) : "");

            }
        }

        public string all_peptides_from_protein
        {
            get
            {
                return get_description(t.accession, t.topdown_ptm_set, t.topdown_begin, t.topdown_end, false) + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => get_description(h.accession, new PtmSet(h.ptm_list), h.begin, h.end, false))) : "");

            }
        }


        private string get_description(string accession, PtmSet set, int begin, int end, bool proteoform_specific_PSMs)
        {
            var modified_psms = Proteoform.get_possible_PSMs(accession.Split('_')[0], set, begin,
                           end).Where(p => p.ptm_list.Count > 0 && p.ambiguous_matches.Count == 0).ToList();
            var all_psms = Proteoform.get_possible_PSMs(accession.Split('_')[0], set, begin,
                           end).Where(p => p.ambiguous_matches.Count == 0).ToList();
            if (!proteoform_specific_PSMs)
            {
                var bottom_up_PSMs = new List<SpectrumMatch>();
                //add BU PSMs
                Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.TryGetValue(accession.Split('_')[0].Split('-')[0], out var psms);
                if (psms != null)
                {
                    bottom_up_PSMs.AddRange(psms.Where(p => p.ambiguous_matches.Count == 0));
                }
                modified_psms = bottom_up_PSMs.Where(p => p.ptm_list.Count > 0).ToList();
                all_psms = bottom_up_PSMs.ToList();

            }
            if (all_psms.Count == 0)
            {
                return "no BU PSMs";
            }
            if (modified_psms.Count == 0) return "no modified BU PSMs";
            List<string> td_psm_with_location = set.ptm_combination.Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).ToList();
            List<string> bu_psm_with_location = modified_psms.SelectMany(b => b.ptm_list).Select(p => UnlocalizedModification.LookUpId(p.modification) + "@" + p.position).Distinct().ToList();
            List<string> td_psm_no_location = set.ptm_combination.Select(p => UnlocalizedModification.LookUpId(p.modification)).ToList();
            List<string> bu_psm_no_location = modified_psms.SelectMany(b => b.ptm_list).Select(p => UnlocalizedModification.LookUpId(p.modification)).Distinct().ToList();
            string description = "";
            bool different_location = false;
            bool same_location = false;
            bool bu_doesnt_contain_this_ptm = false;

            foreach (var td in td_psm_with_location)
            {
                if(bu_psm_with_location.Contains(td))
                {
                    same_location = true;
                }
                else if(bu_psm_no_location.Contains(td.Split('@')[0]))
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
                    if(td_psm_no_location.Contains(bu.Split('@')[0]))
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

        public double best_c_score
        {
            get { return t.topdown_hits.Max(h => h.score); }
        }


        public int Level
        {
            get
            {
                return t.topdown_level;
            }
        }

        public string level_description
        {
            get
            {
                return t.topdown_level_description;
            }
        }


        public string mass_error
        {
            get
            {
                return Math.Round(t.modified_mass - t.theoretical_mass, 4) + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h =>Math.Round(h.reported_mass - h.theoretical_mass, 4))) : "");
            }
        }

        public string manual_id
        {
            get { return t.manual_validation_id; }
        }


        public string family_id
        {
            get { return t.family != null ? t.family.family_id.ToString() : "N/A"; }
        }

        public string Family
        {
            get
            {
                return t.family != null ? t.family.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() > 1 ? "Ambiguous" : "Identified" : "N/A";
            }
        }

        public string linked_proteoform_references
        {
            get
            {
                return t.family != null && t.linked_proteoform_references != null ? string.Join(", ", (t.linked_proteoform_references.Select(p => p.accession))) + (t.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", t.ambiguous_identifications.Select(p => string.Join(", ", p.linked_proteoform_references.Select(a => a.accession))))
                        : "") : "N/A";
            }
        }


        #endregion Public Properties

        #region Public Methods

        public static void FormatTopDownTable(DataGridView dgv, bool identified_topdown)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            foreach (DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name);
                string n = number_format(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible, identified_topdown);
            }
        }

        public static DataTable FormatTopDownTable(List<DisplayTopDownProteoform> display, string table_name, bool identified)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayTopDownProteoform).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true, identified)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        public static string header(string name)
        {
            if (name == nameof(modified_mass)) { return "Modified Mass"; }
            if (name == nameof(ptm_description)) { return "PTM Description"; }
            if (name == nameof(retentionTime)) { return "Retention Time"; }
            if (name == nameof(best_c_score)) { return "Best Hit C-Score"; }
            if (name == nameof(manual_id)) { return "Best Hit Info"; }
            if (name == nameof(family_id)) { return "Family ID"; }
            if (name == nameof(PFR_accession)) { return "PFR Accession"; }
            if (name == nameof(bu_PSMs)) return "Modified Bottom-Up PSMs";
            if (name == nameof(bu_PSMs_count)) return "Bottom-Up PSMs Count";
            if (name == nameof(gene_name)) return "Gene Name";
            if (name == nameof(grouped_accessions)) return "Accessions";
            if (name == nameof(begin_and_end)) return "Begin and End";
            if (name == nameof(uniprot_mods)) return "UniProt-Annotated Modifications";
            if (name == nameof(potentially_novel)) return "Potentially Novel Mods";
            if (name == nameof(linked_proteoform_references)) return "Linked Proteoform References";


            return null;
        }

        private static bool visible(string property_name, bool current, bool identified_topdown)
        {
            if (!identified_topdown)
            {
                if (property_name == nameof(family_id)) { return false; }
            }
            return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(modified_mass)) { return "0.0000"; }
            if (property_name == nameof(retentionTime)) { return "0.00"; }
            return null;
        }

        #endregion Public Methods
    }
}