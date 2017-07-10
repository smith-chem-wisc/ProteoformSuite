using ProteoformSuiteInternal;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayDeltaMassPeak : DisplayObject
    {

        #region Public Constructor

        public DisplayDeltaMassPeak(DeltaMassPeak peak)
            : base(peak)
        { }

        #endregion Public Constructor

        #region Public Properties

        public string mass_shifter
        {
            get
            {
                return (display_object as DeltaMassPeak).mass_shifter;
            }
            set
            {
                (display_object as DeltaMassPeak).mass_shifter = value;
                Sweet.shift_peak_action(display_object as DeltaMassPeak);
            }
        }

        public double DeltaMass
        {
            get { return (display_object as DeltaMassPeak).DeltaMass; }
        }

        public double decoy_relation_count
        {
            get { return (display_object as DeltaMassPeak).decoy_relation_count; }
        }

        public double peak_group_fdr
        {
            get { return (display_object as DeltaMassPeak).peak_group_fdr; }
        }

        public bool Accepted
        {
            get { return (display_object as DeltaMassPeak).Accepted; }
            set { Sweet.lollipop.change_peak_acceptance(display_object as DeltaMassPeak, value); }
        }

        public string possiblePeakAssignments_string
        {
            get { return (display_object as DeltaMassPeak).possiblePeakAssignments_string; }
        }

        public int peak_relation_group_count
        {
            get { return (display_object as DeltaMassPeak).peak_relation_group_count; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatPeakListGridView(DataGridView dgv, bool mask_mass_shifter)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            //making all columns invisible first - faster
            foreach (DataGridViewColumn column in dgv.Columns) { column.Visible = false; }
            if (!mask_mass_shifter)
            {
                dgv.Columns[nameof(mass_shifter)].Visible = true;
                dgv.Columns[nameof(mass_shifter)].ReadOnly = false; //user can say how much they want to change monoisotopic by for each
            }
            dgv.Columns[nameof(DeltaMass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(peak_group_fdr)].DefaultCellStyle.Format = "0.##";

            dgv.Columns[nameof(peak_relation_group_count)].HeaderText = "Peak Center Count";
            dgv.Columns[nameof(decoy_relation_count)].HeaderText = "Decoy Count under Peak";
            dgv.Columns[nameof(DeltaMass)].HeaderText = "Peak Center Delta Mass";
            dgv.Columns[nameof(peak_group_fdr)].HeaderText = "Peak FDR";
            dgv.Columns[nameof(Accepted)].HeaderText = "Peak Accepted";
            dgv.Columns[nameof(possiblePeakAssignments_string)].HeaderText = "Peak Assignment Possibilites";

            dgv.Columns[nameof(peak_relation_group_count)].Visible = true;
            dgv.Columns[nameof(DeltaMass)].Visible = true;
            dgv.Columns[nameof(peak_group_fdr)].Visible = true;
            dgv.Columns[nameof(Accepted)].Visible = true;
            dgv.Columns[nameof(possiblePeakAssignments_string)].Visible = true;
        }

        #endregion Public Methods
    }
}
