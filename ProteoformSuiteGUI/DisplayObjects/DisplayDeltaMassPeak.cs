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

        public double DeltaMass
        {
            get { return (display_object as DeltaMassPeak).DeltaMass; }
        }

        public int PeakRelationGroupCount
        {
            get { return (display_object as DeltaMassPeak).peak_relation_group_count; }
        }

        public bool Accepted
        {
            get { return (display_object as DeltaMassPeak).Accepted; }
            set { Sweet.lollipop.change_peak_acceptance(display_object as DeltaMassPeak, value, true); }
        }

        public double DecoyRelationCount
        {
            get { return (display_object as DeltaMassPeak).decoy_relation_count; }
        }

        public double PeakGroupFDR
        {
            get { return (display_object as DeltaMassPeak).peak_group_fdr; }
        }

        public string possiblePeakAssignments_string
        {
            get { return (display_object as DeltaMassPeak).possiblePeakAssignments_string; }
        }

        public string MassShifter
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
                dgv.Columns[nameof(MassShifter)].Visible = true;
                dgv.Columns[nameof(MassShifter)].ReadOnly = false; //user can say how much they want to change monoisotopic by for each
                dgv.Columns[nameof(MassShifter)].HeaderText = "Mass Shifter";
            }
            dgv.Columns[nameof(DeltaMass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(PeakGroupFDR)].DefaultCellStyle.Format = "0.##";

            dgv.Columns[nameof(PeakRelationGroupCount)].HeaderText = "Peak Center Count";
            dgv.Columns[nameof(DecoyRelationCount)].HeaderText = "Decoy Count under Peak";
            dgv.Columns[nameof(DeltaMass)].HeaderText = "Peak Center Delta Mass";
            dgv.Columns[nameof(PeakGroupFDR)].HeaderText = "Peak FDR";
            dgv.Columns[nameof(Accepted)].HeaderText = "Peak Accepted";
            dgv.Columns[nameof(possiblePeakAssignments_string)].HeaderText = "Peak Assignment Possibilites";

            dgv.Columns[nameof(PeakRelationGroupCount)].Visible = true;
            dgv.Columns[nameof(DeltaMass)].Visible = true;
            dgv.Columns[nameof(PeakGroupFDR)].Visible = true;
            dgv.Columns[nameof(Accepted)].Visible = true;
            dgv.Columns[nameof(possiblePeakAssignments_string)].Visible = true;
        }

        #endregion Public Methods

    }
}
