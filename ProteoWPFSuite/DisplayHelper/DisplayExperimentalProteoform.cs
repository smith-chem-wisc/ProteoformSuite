using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ProteoWPFSuite
{
    class DisplayExperimentalProteoform : DisplayObject
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

        public double agg_intensity
        {
            get { return e.agg_intensity; }
        }

        public double agg_rt
        {
            get { return e.agg_rt; }
        }

        public double lysine_count
        {
            get { return e.lysine_count; }
        }

        public bool Accepted
        {
            get { return e.accepted; }
        }

        public bool mass_shifted
        {
            get { return e.mass_shifted; }
        }

        public int observation_count
        {
            get { return e.aggregated.Count; }
        }

        public int light_verification_count
        {
            get { return e.lt_verification_components.Count; }
        }

        public int heavy_verification_count
        {
            get { return e.hv_verification_components.Count; }
        }

        public int light_observation_count
        {
            get { return e.lt_quant_components.Count; }
        }

        public int heavy_observation_count
        {
            get { return e.hv_quant_components.Count; }
        }

        public string ptm_description
        {
            get { return e.linked_proteoform_references != null ? e.ptm_description : ""; }
        }

        public string uniprot_mods
        {
            get { return e.linked_proteoform_references != null ? e.uniprot_mods : ""; }
        }

        public bool novel_mods
        {
            get { return e.novel_mods; }
        }

        public int Begin
        {
            get { return e.begin; }
        }

        public int End
        {
            get { return e.end; }
        }

        public string gene_name
        {
            get
            {
                return e.gene_name != null ?
                    (e.gene_name.get_prefered_name(Lollipop.preferred_gene_label) != null ?
                    e.gene_name.get_prefered_name(Lollipop.preferred_gene_label) :
                    "")
                    : "";
            }
        }

        public string GeneID
        {
            get
            {
                return e.linked_proteoform_references != null ?
                    String.Join("; ", (e.linked_proteoform_references.First() as TheoreticalProteoform).ExpandedProteinList.SelectMany(p => p.DatabaseReferences.Where(r => r.Type == "GeneID").Select(r => r.Id)).Distinct())
                : "";
            }
        }

        public string theoretical_accession
        {
            get
            {
                return e.linked_proteoform_references != null ?
                   (e.linked_proteoform_references[0] as TheoreticalProteoform).accession :
                    "";
            }
        }

        public string Fragment
        {
            get
            {
                return e.linked_proteoform_references != null ?
                   (e.linked_proteoform_references[0] as TheoreticalProteoform).fragment :
                    "";
            }
        }

        public string manual_validation_id
        {
            get { return e.manual_validation_id; }
        }

        public string manual_validation_verification
        {
            get { return e.manual_validation_verification; }
        }

        public string manual_validation_quant
        {
            get { return e.manual_validation_quant; }
        }

        //needs to be at same time and mass
        public bool topdown_id
        {
            get { return e.topdown_id; }
        }

        public double mass_error
        {
            get { return e.mass_error; }
        }

        public bool Adduct
        {
            get
            {
                return e.adduct;
            }
        }

        public bool Ambiguous
        {
            get
            {
                return e.ambiguous;
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

        public string family_id
        {
            get { return e.family != null ? e.family.family_id.ToString() : ""; }
        }

        public string mz_values
        {
            get
            {
                return e.topdown_id ? "" :
                 String.Join(", ", (e.aggregated.OrderByDescending(c => c.intensity_sum).First()).charge_states.Select(cs => Math.Round(cs.mz_centroid, 2)));
            }
        }

        public string Charges
        {
            get
            {
                return e.topdown_id ? "" :
                    String.Join(", ", (e.aggregated.OrderByDescending(c => c.intensity_sum).First()).charge_states.Select(cs => cs.charge_count));
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatAggregatesTable(System.Windows.Forms.DataGridView dgv)
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
            if (property_name == nameof(heavy_verification_count)) return "Heavy Verification Component Count";
            if (property_name == nameof(light_verification_count)) return "Light Verification Component Count";
            if (property_name == nameof(heavy_observation_count)) return "Heavy Quantitative Component Count";
            if (property_name == nameof(light_observation_count)) return "Light Quantitative Component Count";
            if (property_name == nameof(lysine_count)) return "Lysine Count";
            if (property_name == nameof(mass_shifted)) return "Manually Shifted Mass";
            if (property_name == nameof(ptm_description)) return "PTM Description";
            if (property_name == nameof(gene_name)) return "Gene Name";
            if (property_name == nameof(theoretical_accession)) return "Theoretical Accession";
            if (property_name == nameof(manual_validation_id)) return "Abundant Component for Manual Validation of Identification";
            if (property_name == nameof(manual_validation_verification)) return "Abundant Component for Manual Validation of Identification Verification";
            if (property_name == nameof(manual_validation_quant)) return "Abundant Component for Manual Validation of Quantification";
            if (property_name == nameof(topdown_id)) return "Top-Down Proteoform";
            if (property_name == nameof(family_id)) return "Family ID";
            if (property_name == nameof(mass_error)) return "Mass Error";
            if (property_name == nameof(mz_values)) return "M/z values";
            if (property_name == nameof(uniprot_mods)) return "UniProt-Annotated Modifications";
            if (property_name == nameof(novel_mods)) return "Potentially Novel Mods";
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
