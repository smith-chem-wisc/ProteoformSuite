using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ProteoWPFSuite
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
                return t.pfr_accession;
            }
        }

        public string original_PFR_accession
        {
            get { return t.topdown_hits.First().original_pfr_accession; }
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

        //public string GeneID
        //{
        //    get
        //    {
        //        return string.Join("; ", Sweet.lollipop.theoretical_database.theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number][t.accession.Split('_')[0].Split('-')[0]].First().ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct())
        //             + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => string.Join("; ", Sweet.lollipop.theoretical_database.theoreticals_by_accession[Sweet.lollipop.target_proteoform_community.community_number][h.accession.Split('_')[0].Split('-')[0]].First().ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct()))) : "");
        //    }
        //}

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

        public double best_c_score
        {
            get { return t.topdown_hits.Max(h => h.score); }
        }

        public double best_q_value
        {
            get { return t.topdown_hits.Min(h => h.qValue); }
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
                return Math.Round(t.modified_mass - t.theoretical_mass, 4) + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => Math.Round(h.reported_mass - h.theoretical_mass, 4))) : "");
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

        public string bu_PSMs_count
        {
            get
            {
                return t.topdown_bottom_up_PSMs.Count.ToString()
                       + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(i => i.bottom_up_PSMs.Count.ToString()))
                    : "");
            }
        }

        public bool different_ambiguity
        {
            get
            {
                return t.different_ambiguity;
            }
        }

        public string bu_PTMs
        {
            get
            {
                return t.bu_PTMs;
            }
        }



        public string bu_PTMs_all_from_protein
        {
            //only unambiguous PSMs for now....
            get
            {
                return t.bu_PTMs_all_from_protein;
            }
        }

        public string BU_PTMS_separatepeptides
        {
            get
            {
                return t.bu_PTMs_separatepeptides;

            }
        }

        public bool begin_peptide
        {
            get
            {
                return t.topdown_bottom_up_PSMs.Any(p => p.begin == t.topdown_begin);
            }
        }

        public bool end_peptide
        {
            get
            {
                return t.topdown_bottom_up_PSMs.Any(p => p.end == t.topdown_end);
            }
        }


        public bool bottom_up_evidence_for_all_PTMs
        {
            get
            {
                return Proteoform.get_bottom_up_evidence_for_all_PTMs(t.topdown_bottom_up_PSMs, t.topdown_ptm_set, true);
            }
        }


        public string seq_ptm_specific
        {
            //only unambiguous psms for now
            get
            {
                return TopDownProteoform.get_description(t.topdown_bottom_up_PSMs, t.accession, true, t.topdown_ptm_set) + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => TopDownProteoform.get_description(h.bottom_up_PSMs, h.accession, true, new PtmSet(h.ptm_list)))) : "");

            }
        }

        public string all_peptides_from_protein
        {
            get
            {
                return TopDownProteoform.get_description(t.topdown_bottom_up_PSMs, t.accession, false, t.topdown_ptm_set) + (t.ambiguous_topdown_hits.Count > 0 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(h => TopDownProteoform.get_description(h.bottom_up_PSMs, h.accession, false, new PtmSet(h.ptm_list)))) : "");

            }
        }

        public string Fragments
        {
            get
            {
                return String.Join(", ", t.topdown_hits.SelectMany(h => h.matched_fragment_ions)
                    .OrderBy(i => i.NeutralTheoreticalProduct.ProductType.ToString()).ThenBy(i => i.NeutralTheoreticalProduct.TerminusFragment.FragmentNumber)
                    .Select(i => i.NeutralTheoreticalProduct.ProductType.ToString() + i.NeutralTheoreticalProduct.TerminusFragment.FragmentNumber).Distinct());
            }
        }

        //public string hits_index
        //{
        //    get { return String.Join(",", t.topdown_hits.Select(h => "H" + h.hit_ID + "hit")); }
        //}

        //public double percent_target
        //{
        //    get
        //    {
        //        int total_hits = 1 + t.ambiguous_topdown_hits.Count;
        //        int total_targets = (t.topdown_hits.Any(h => h.target) ? 1 : 0) + t.ambiguous_topdown_hits.Count(h => h.target);
        //        double ratio = (double)total_targets / (double)total_hits;
        //        return ratio;
        //    }
        //}



        #endregion Public Properties

        #region Public Methods

        public static void FormatTopDownTable(System.Windows.Forms.DataGridView dgv, bool identified_topdown)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            foreach (System.Windows.Forms.DataGridViewColumn c in dgv.Columns)
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
            if (name == nameof(bu_PTMs)) return "Modified Bottom-Up PSMs";
            if (name == nameof(bu_PSMs_count)) return "Bottom-Up PSMs Count";
            if (name == nameof(different_ambiguity)) return "Different Ambiguity in Bottom-Up PSMs";
            if (name == nameof(bu_PTMs_all_from_protein)) return "All Modified Bottom-Up PSMs from Protein";
            if (name == nameof(BU_PTMS_separatepeptides)) return "Bottom-Up PSMs Separate Peptides";
            if (name == nameof(begin_peptide)) return "Bottom-Up Evidence for Begin";
            if (name == nameof(end_peptide)) return "Bottom-Up Evidence for End";
            if (name == nameof(bottom_up_evidence_for_all_PTMs)) return "Bottom-Up Evidence for All PTMs";
            if (name == nameof(seq_ptm_specific)) return "Sequence Specific";
            if (name == nameof(all_peptides_from_protein)) return "All Peptides From Protein";
            if (name == nameof(gene_name)) return "Gene Name";
            if (name == nameof(grouped_accessions)) return "Accessions";
            if (name == nameof(begin_and_end)) return "Begin and End";
            if (name == nameof(uniprot_mods)) return "UniProt-Annotated Modifications";
            if (name == nameof(potentially_novel)) return "Potentially Novel Mods";
            if (name == nameof(linked_proteoform_references)) return "Linked Proteoform References";
            if (name == nameof(original_PFR_accession)) return "Original PFR/full-sequence";
            if (name == nameof(level_description)) return "Level Description";
            if (name == nameof(mass_error)) return "Mass Error";
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