using ProteoformSuiteInternal;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayNeuCodePair : DisplayComponent
    {

        #region Public Constructors

        public DisplayNeuCodePair(NeuCodePair c)
            : base(c)
        {
            this.c = c;
        }

        #endregion

        #region Private Fields

        private NeuCodePair c;

        #endregion

        #region Public Properties

        public string id_light
        {
            get { return c.neuCodeLight.id; }
        }

        public string id_heavy
        {
            get { return c.neuCodeHeavy.id; }
        }

        public double intensity_ratio
        {
            get { return c.intensity_ratio; }
        }

        public int lysine_count
        {
            get { return c.lysine_count; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatNeuCodeTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            FormatComponentsTable(dgv, false);

            //round table values
            dgv.Columns[nameof(intensity_ratio)].DefaultCellStyle.Format = "0.####";

            //Headers
            dgv.Columns[nameof(id_light)].HeaderText = "Light NeuCode Component ID";
            dgv.Columns[nameof(id_heavy)].HeaderText = "Heavy NeuCode Component ID";
            dgv.Columns[nameof(intensity_ratio)].HeaderText = "Intensity Ratio";
            dgv.Columns[nameof(lysine_count)].HeaderText = "Lysine Count";
            dgv.Columns[nameof(reported_monoisotopic_mass)].HeaderText = "Light " + dgv.Columns[nameof(reported_monoisotopic_mass)].HeaderText;
            dgv.Columns[nameof(weighted_monoisotopic_mass)].HeaderText = "Light " + dgv.Columns[nameof(weighted_monoisotopic_mass)].HeaderText;
            dgv.Columns[nameof(rt_apex)].HeaderText = "Light " + dgv.Columns[nameof(rt_apex)].HeaderText;
            dgv.Columns[nameof(relative_abundance)].HeaderText = "Light " + dgv.Columns[nameof(relative_abundance)].HeaderText;
            dgv.Columns[nameof(fract_abundance)].HeaderText = "Light " + dgv.Columns[nameof(fract_abundance)].HeaderText;
            dgv.Columns[nameof(intensity_sum_olcs)].HeaderText = "Light " + dgv.Columns[nameof(intensity_sum_olcs)].HeaderText;

            //VISIBILITY
            dgv.Columns[nameof(component_id)].Visible = false;
        }

        #endregion

    }
}
