using System;
using ProteoformSuiteInternal;
using System.Windows.Forms;


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
            get { return e.gene_name.get_prefered_name(ProteoformCommunity.preferred_gene_label); }
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
            dgv.Columns[nameof(agg_rt)].DefaultCellStyle.Format = "0.##";

            //HEADERS
            dgv.Columns[nameof(Accession)].HeaderText = "Experimental Proteoform ID";
            dgv.Columns[nameof(agg_mass)].HeaderText = "Aggregated Mass";
            dgv.Columns[nameof(agg_intensity)].HeaderText = "Aggregated Intensity";
            dgv.Columns[nameof(agg_rt)].HeaderText = "Aggregated RT";
            dgv.Columns[nameof(observation_count)].HeaderText = "Aggregated Component Count for Identification";
            dgv.Columns[nameof(heavy_observation_count)].HeaderText = "Heavy Quantitative Component Count";
            dgv.Columns[nameof(light_observation_count)].HeaderText = "Light Quantitative Component Count";
            dgv.Columns[nameof(lysine_count)].HeaderText = "Lysine Count";
            dgv.Columns[nameof(mass_shifted)].HeaderText = "Manually Shifted Mass";
            dgv.Columns[nameof(ptm_description)].HeaderText = "PTM Description";
            dgv.Columns[nameof(gene_name)].HeaderText = "Gene Name";

            //VISIBILITY
            dgv.Columns[nameof(lysine_count)].Visible = Lollipop.neucode_labeled; 
        }

        #endregion
    }
}
