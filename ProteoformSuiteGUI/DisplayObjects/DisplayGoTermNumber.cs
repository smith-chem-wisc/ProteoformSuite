using ProteoformSuiteInternal;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayGoTermNumber : DisplayObject
    {
        #region Public Constructors

        public DisplayGoTermNumber(GoTermNumber g)
            : base(g)
        {
            gtn = g;
        }

        #endregion

        #region Private Fields

        private GoTermNumber gtn;

        #endregion

        #region Public Properties

        public string Aspect
        {
            get { return gtn.Aspect.ToString(); }
        }

        public string Description
        {
            get
            {
                return gtn.Description;
            }
        }

        public string ID
        {
            get { return "GO: " + gtn.Id; }
        }

        public double p_value
        {
            get { return gtn.by; }
        }

        public double by //benjamini yekutieli calculated after all p-values are calculated
        {
            get { return gtn.by; }
        }

        public decimal log_odds_ratio
        {
            get { return gtn.log_odds_ratio; }
        }

        public int q_significantProteinsWithThisGoTerm
        {
            get { return gtn.q_significantProteinsWithThisGoTerm; }
        }

        public int k_significantProteins
        {
            get { return gtn.k_significantProteins; }
        }

        public int m_backgroundProteinsWithThisGoTerm
        {
            get { return gtn.m_backgroundProteinsWithThisGoTerm; }
        }

        public int t_backgroundProteins
        {
            get { return gtn.t_backgroundProteins; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatGridView(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            //round table values
            dgv.Columns[nameof(by)].DefaultCellStyle.Format = "E2";
            dgv.Columns[nameof(p_value)].DefaultCellStyle.Format = "E2";
            dgv.Columns[nameof(log_odds_ratio)].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv.Columns[nameof(by)].HeaderText = "Benjamini-Yekutieli Corrected p-Value";
            dgv.Columns[nameof(p_value)].HeaderText = "p-Value";
            dgv.Columns[nameof(log_odds_ratio)].HeaderText = "Log Odds Ratio";
            dgv.Columns[nameof(q_significantProteinsWithThisGoTerm)].HeaderText = "Significant Proteins With This Go-Term";
            dgv.Columns[nameof(k_significantProteins)].HeaderText = "Total Significant Proteins";
            dgv.Columns[nameof(m_backgroundProteinsWithThisGoTerm)].HeaderText = "Background Proteins With This Go-Term";
            dgv.Columns[nameof(t_backgroundProteins)].HeaderText = "Total Background Proteins";
        } 

        #endregion
    }
}
