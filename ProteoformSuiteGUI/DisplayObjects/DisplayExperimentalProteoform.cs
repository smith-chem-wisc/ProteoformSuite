using ProteoformSuiteInternal;
using System.Windows.Forms;
using System;
using System.Linq;

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

        #endregion

        #region Private Fields

        private ExperimentalProteoform e;

        #endregion

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
            get { return e.aggregated_components.Count; }
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
            get { return e.ptm_description; }
        }

        public string gene_name
        {
            get
            {
                return e.gene_name != null ?
                    e.gene_name.get_prefered_name(ProteoformCommunity.preferred_gene_label) :
                    "";
            }
        }

        public string theoretical_accession
        {
            get
            {
                return e.linked_proteoform_references != null ?
                   (e.linked_proteoform_references[0] as TheoreticalProteoform).accession:
                    "";
            }
        }

        public string fragment
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
        public int etd_relations
        {
            get { return e.relationships.Count(r => r.RelationType == ProteoformComparison.ExperimentalTopDown); }
        }

        public int other_topdown
        {
            get
            {
                return e.linked_proteoform_references != null ?
                Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count(t => t.gene_name == e.gene_name && !t.relationships.SelectMany(r => r.connected_proteoforms).Contains(e) &&
                Math.Abs(t.modified_mass - e.modified_mass) < (double)Sweet.lollipop.mass_tolerance) :
                0;
            }
        }

        public int bottomup_PSMs
        {
            get
            {
                return e.linked_proteoform_references != null ?
                           e.family.theoretical_proteoforms.Where(t => t.gene_name == e.gene_name).SelectMany(t => t.psm_list).Count() :
                           0;
            }
        }
        
        #endregion Public Properties

        #region Public Methods

        public static void FormatAggregatesTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            //NUMBER FORMATS
            dgv.Columns[nameof(agg_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(agg_intensity)].DefaultCellStyle.Format = "0.####";

            //HEADERS
            dgv.Columns[nameof(Accession)].HeaderText = "Experimental Proteoform ID";
            dgv.Columns[nameof(agg_mass)].HeaderText = "Aggregated Mass";
            dgv.Columns[nameof(agg_intensity)].HeaderText = "Aggregated Intensity";
            dgv.Columns[nameof(agg_rt)].HeaderText = "Aggregated Retention Time";
            dgv.Columns[nameof(observation_count)].HeaderText = "Aggregated Component Count for Identification";
            dgv.Columns[nameof(heavy_verification_count)].HeaderText = "Heavy Verification Component Count";
            dgv.Columns[nameof(light_verification_count)].HeaderText = "Light Verification Component Count";
            dgv.Columns[nameof(heavy_observation_count)].HeaderText = "Heavy Quantitative Component Count";
            dgv.Columns[nameof(light_observation_count)].HeaderText = "Light Quantitative Component Count";
            dgv.Columns[nameof(lysine_count)].HeaderText = "Lysine Count";
            dgv.Columns[nameof(mass_shifted)].HeaderText = "Manually Shifted Mass";
            dgv.Columns[nameof(ptm_description)].HeaderText = "PTM Description";
            dgv.Columns[nameof(gene_name)].HeaderText = "Gene Name";
            dgv.Columns[nameof(manual_validation_id)].HeaderText = "Abundant Component for Manual Validation of Identification";
            dgv.Columns[nameof(manual_validation_verification)].HeaderText = "Abundant Component for Manual Validation of Identification Verification";
            dgv.Columns[nameof(manual_validation_quant)].HeaderText = "Abundant Component for Manual Validation of Quantification";
            dgv.Columns[nameof(theoretical_accession)].HeaderText = "Theoretical Accession";
            dgv.Columns[nameof(fragment)].HeaderText = "Fragment";

            //VISIBILITY
            dgv.Columns[nameof(lysine_count)].Visible = Sweet.lollipop.neucode_labeled;
            dgv.Columns[nameof(etd_relations)].Visible = false;
            dgv.Columns[nameof(other_topdown)].Visible = false;
            dgv.Columns[nameof(bottomup_PSMs)].Visible = false;
        }

        public static void FormatIdentifiedProteoformTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            //NUMBER FORMATS
            dgv.Columns[nameof(agg_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(agg_intensity)].DefaultCellStyle.Format = "0.####";

            //HEADERS
            dgv.Columns[nameof(Accession)].HeaderText = "Experimental Proteoform ID";
            dgv.Columns[nameof(agg_mass)].HeaderText = "Aggregated Mass";
            dgv.Columns[nameof(agg_intensity)].HeaderText = "Aggregated Intensity";
            dgv.Columns[nameof(agg_rt)].HeaderText = "Aggregated Retention Time";
            dgv.Columns[nameof(observation_count)].HeaderText = "Aggregated Component Count for Identification";
            dgv.Columns[nameof(lysine_count)].HeaderText = "Lysine Count";
            dgv.Columns[nameof(ptm_description)].HeaderText = "PTM Description";
            dgv.Columns[nameof(gene_name)].HeaderText = "Gene Name";
            dgv.Columns[nameof(manual_validation_id)].HeaderText = "Abundant Component for Manual Validation of Identification";
            dgv.Columns[nameof(theoretical_accession)].HeaderText = "Theoretical Accession";
            dgv.Columns[nameof(fragment)].HeaderText = "Fragment";
            dgv.Columns[nameof(etd_relations)].HeaderText = "Experiment-TopDown Relation Count";
            dgv.Columns[nameof(other_topdown)].HeaderText = "Other TopDown Proteoforms With Same Gene Name and Mass";
            dgv.Columns[nameof(bottomup_PSMs)].HeaderText = "BottomUp PSMs Count";

            //VISIBILITY
            dgv.Columns[nameof(lysine_count)].Visible = Sweet.lollipop.neucode_labeled;
            dgv.Columns[nameof(heavy_verification_count)].Visible = false;
            dgv.Columns[nameof(light_verification_count)].Visible = false;
            dgv.Columns[nameof(heavy_observation_count)].Visible = false;
            dgv.Columns[nameof(light_observation_count)].Visible = false;
            dgv.Columns[nameof(manual_validation_verification)].Visible = false;
            dgv.Columns[nameof(manual_validation_quant)].Visible = false;
            dgv.Columns[nameof(mass_shifted)].Visible = false;
            dgv.Columns[nameof(Accepted)].Visible = false;
        }

        #endregion

    }
}
