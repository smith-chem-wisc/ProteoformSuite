using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TheoreticalProteoform = ProteoformSuiteInternal.TheoreticalProteoform;

namespace ProteoformSuiteGUI
{
    public class DisplayExperimentalProteoform : DisplayObject
    {
        #region Public Constructors

        public DisplayExperimentalProteoform(ExperimentalProteoform e)
            : base(e)
        {
            this.e = e;
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly ExperimentalProteoform e;

        #endregion Private Fields

        #region Public Properties

        public string Accession
        {
            get { return e.accession; }
        }

        public double agg_mass
        {
            get { return e.agg_mass; }
        }


        public double agg_rt
        {
            get { return e.agg_rt; }
        }

        public double agg_intensity
        {
            get { return e.agg_intensity; }
        }

        public bool topdown_id
        {
            get { return e.topdown_id; }
        }

        public double lysine_count
        {
            get { return e.lysine_count; }
        }

        public bool mass_shifted
        {
            get { return e.aggregated.Any(c => c as Component != null && (c as Component).manual_mass_shift != 0); }
        }

        public int observation_count
        {
            get { return e.aggregated.Count; }
        }


        public string Description
        {
            get
            {
                return (e.linked_proteoform_references != null
                           ? (e.linked_proteoform_references[0] as TheoreticalProteoform).description
                           : "") + (e.ambiguous_identifications.Count > 0
                           ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => (p.theoretical_base as TheoreticalProteoform).description))
                           : "");
            }
        }

        public string gene_name
        {
            get
            {
                return (e.linked_proteoform_references != null
                           ? (e.linked_proteoform_references[0] as TheoreticalProteoform).gene_name.get_prefered_name(Lollipop.preferred_gene_label)
                           : "") + (e.ambiguous_identifications.Count > 0
                           ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => p.theoretical_base.gene_name.get_prefered_name(Lollipop.preferred_gene_label)))
                           : "");
            }
        }


        public string GeneID
        {
            get
            {
                return (e.linked_proteoform_references != null
                           ? string.Join("; ", (e.linked_proteoform_references[0] as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct())
                           : "") + (e.ambiguous_identifications.Count > 0
                           ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(t => string.Join("; ", (t.theoretical_base as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct())))
                           : "");
            }
        }

        public string grouped_accessions
        {
            get
            {
                return (e.linked_proteoform_references != null
                    ? string.Join(", ", (e.linked_proteoform_references[0] as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.AccessionList).Select(a => a.Split('_')[0]).Distinct())
                    : "") + (e.ambiguous_identifications.Count > 0
                          ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => string.Join(", ", p.theoretical_base.ExpandedProteinList.SelectMany(a => a.AccessionList).Select(a => a.Split('_')[0]).Distinct())))
                          : "");
            }
        }


        public string ptm_description
        {
            get { return (e.linked_proteoform_references != null ? e.ptm_set.ptm_description : "" ) + 
                         (e.ambiguous_identifications.Count > 0 ? " | " +  String.Join(" | ", e.ambiguous_identifications.Select(p => p.ptm_set.ptm_description)) : "" ); }
        }


        public string begin_and_end
        {
            get { return e.linked_proteoform_references != null ? e.begin.ToString() + " to " + e.end + (e.ambiguous_identifications.Count > 0 ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => p.begin + " to " + p.end)) : "")  : "" ; }
        }

        public string Sequence
        {
            get
            {
                return e.linked_proteoform_references != null ? ExperimentalProteoform.get_sequence(e.linked_proteoform_references.First() as TheoreticalProteoform, e.begin, e.end)
                    + (e.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(i => ExperimentalProteoform.get_sequence(i.theoretical_base as TheoreticalProteoform, i.begin, i.end)))
                        : "") : "";
            }
        }

        public string uniprot_mods
        {
            get { return e.linked_proteoform_references != null ? e.uniprot_mods : ""; }
        }

        public bool novel_mods
        {
            get { return e.novel_mods; }
        }

        public string bu_PSMs_count
        {
            get
            {
                return (e.linked_proteoform_references != null ? Proteoform.get_possible_PSMs(e.linked_proteoform_references.First().accession.Split('_')[0], e.ptm_set, e.begin, e.end).Count.ToString() : "N/A")
                       + (e.ambiguous_identifications.Count > 0
                           ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(i => Proteoform.get_possible_PSMs(i.theoretical_base.accession.Split('_')[0], i.ptm_set, i.begin, i.end).Count.ToString()))
                           : "");
            }
        }

        public string bu_PSMs
        {
            get
            {
                return (e.linked_proteoform_references != null ? Proteoform.get_possible_PSMs(e.linked_proteoform_references.First().accession.Split('_')[0], e.ptm_set, e.begin, e.end).Count(p => p.ptm_list.Count > 0) == 0 ? "N/A" :
                           String.Join(", ", Proteoform.get_possible_PSMs(e.linked_proteoform_references.First().accession.Split('_')[0], e.ptm_set, e.begin, e.end).Where(p => p.ptm_list.Count > 0).Select(p => p.ptm_description).Distinct()) : "N/A")
                       + (e.ambiguous_identifications.Count > 0
                           ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(i => Proteoform.get_possible_PSMs(i.theoretical_base.accession.Split('_')[0], i.ptm_set, i.begin, i.end).Count(p => p.ptm_list.Count > 0) == 0 ?
                                 "N/A" : String.Join(", ", Proteoform.get_possible_PSMs(i.theoretical_base.accession.Split('_')[0], i.ptm_set, i.begin, i.end).Where(p => p.ptm_list.Count > 0).Select(p => p.ptm_description).Distinct())))
                           : "");
            }
        }

        public int Level
        {
            get
            {
                return e.proteoform_level;
            }
        }

        public string Ambiguity
        {
            get
            {
                return e.proteoform_level_description;
            }
        }

        public bool new_intact_mass_id
        {
            get
            {
                return e.new_intact_mass_id;
            }
        }

        public bool Ambiguous
        {
            get
            {
                return e.ambiguous_identifications.Count > 0;
            }
        }

        public bool Adduct
        {
            get
            {
                return e.adduct;
            }
        }


        public bool Contaminant
        {
            get
            {
                return e.linked_proteoform_references != null ? (e.linked_proteoform_references.First() as TheoreticalProteoform).contaminant
                    : false;
            }
        }

        public string mass_error
        {
            get
            {
                return (e.linked_proteoform_references != null ? e.calculate_mass_error(e.linked_proteoform_references.First() as TheoreticalProteoform, e.ptm_set, e.begin, e.end).ToString() : "N/A")
                       + (e.ambiguous_identifications.Count > 0
                           ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(i => e.calculate_mass_error(i.theoretical_base as TheoreticalProteoform, i.ptm_set, i.begin, i.end).ToString()))
                           : "");
            }
        }

        public string family_id
        {
            get { return e.family != null ? e.family.family_id.ToString() : ""; }
        }

        public string Family
        {
            get
            {
                return e.family != null ? e.family.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count() > 1 ? "Ambiguous" : "Identified" : "";
                    }
        }


        public string linked_proteoform_references
        {
            get
            {
                return e.linked_proteoform_references != null ? string.Join(", ", (e.linked_proteoform_references.Select(p => p.accession))) + (e.ambiguous_identifications.Count > 0
                        ? " | " + String.Join(" | ", e.ambiguous_identifications.Select(p => string.Join(", ", p.linked_proteoform_references.Select(a => a.accession))))
                        : "") : "";
            }
        }

        public string mz_values
        {
            get
            {
                return e.topdown_id ? "" :
                 string.Join(", ", (e.aggregated.OrderByDescending(c => c.intensity_sum).First()).charge_states.Select(cs => Math.Round(cs.mz_centroid, 2)));
            }
        }

        public string Charges
        {
            get
            {
                return e.topdown_id ? "" :
                    string.Join(", ", (e.aggregated.OrderByDescending(c => c.intensity_sum).First()).charge_states.Select(cs => cs.charge_count));
            }
        }


        public string manual_validation_id
        {
            get { return e.manual_validation_id; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatAggregatesTable(DataGridView dgv)
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
                c.Visible = visible(c.Name, c.Visible);
            }
        }

        public static DataTable FormatAggregatesTable(List<DisplayExperimentalProteoform> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayExperimentalProteoform).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static string header(string property_name)
        {
            if (property_name == nameof(Accession)) return "Experimental Proteoform ID";
            if (property_name == nameof(agg_mass)) return "Aggregated Mass";
            if (property_name == nameof(agg_intensity)) return "Aggregated Deconvolution Intensity";
            if (property_name == nameof(agg_rt)) return "Aggregated RT";
            if (property_name == nameof(observation_count)) return "Aggregated Component Count for Identification";
            if (property_name == nameof(lysine_count)) return "Lysine Count";
            if (property_name == nameof(mass_shifted)) return "Manually Shifted Mass";
            if (property_name == nameof(ptm_description)) return "PTM Description";
            if (property_name == nameof(gene_name)) return "Gene Name";
            if (property_name == nameof(manual_validation_id)) return "Abundant Component for Manual Validation of Identification";
            if (property_name == nameof(topdown_id)) return "Top-Down Proteoform";
            if (property_name == nameof(family_id)) return "Family ID";
            if (property_name == nameof(mass_error)) return "Mass Error";
            if (property_name == nameof(mz_values)) return "M/z values";
            if (property_name == nameof(uniprot_mods)) return "UniProt-Annotated Modifications";
            if (property_name == nameof(novel_mods)) return "Potentially Novel Mods";
            if (property_name == nameof(bu_PSMs)) return "Modified Bottom-Up PSMs";
            if (property_name == nameof(bu_PSMs_count)) return "Bottom-Up PSMs Count";
            if (property_name == nameof(grouped_accessions)) return "Grouped Accessions";
            if (property_name == nameof(new_intact_mass_id)) return "New Intact-Mass ID";
            if (property_name == nameof(linked_proteoform_references)) return "Linked Proteoform References";
            return null;
        }

        private static bool visible(string property_name, bool current)
        {
            if (property_name == nameof(lysine_count)) { return Sweet.lollipop.neucode_labeled; }
            else return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(agg_mass)) { return "0.0000"; }
            if (property_name == nameof(agg_intensity)) { return "0.0000"; }
            if (property_name == nameof(agg_rt)) { return "0.00"; }
            if (property_name == nameof(mass_error)) { return "0.0000"; }
            return null;
        }

        #endregion Private Methods
    }
}