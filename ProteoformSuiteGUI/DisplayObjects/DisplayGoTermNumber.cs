using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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

        #endregion Public Constructors

        #region Private Fields

        private readonly GoTermNumber gtn;

        #endregion Private Fields

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
            get { return gtn.p_value; }
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
            if (dgv.Columns.Count <= 0) { return; }

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

        public static DataTable FormatGridView(List<DisplayGoTermNumber> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayGoTermNumber).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static string header(string property_name)
        {
            if (property_name == nameof(by)) { return "Benjamini-Yekutieli Corrected p-Value"; }
            if (property_name == nameof(p_value)) { return "p-Value"; }
            if (property_name == nameof(log_odds_ratio)) { return "Log Odds Ratio"; }
            if (property_name == nameof(q_significantProteinsWithThisGoTerm)) { return "Significant Proteins With This Go-Term"; }
            if (property_name == nameof(k_significantProteins)) { return "Total Significant Proteins"; }
            if (property_name == nameof(m_backgroundProteinsWithThisGoTerm)) { return "Background Proteins With This Go-Term"; }
            if (property_name == nameof(t_backgroundProteins)) { return "Total Background Proteins"; }
            return null;
        }

        private static bool visible(string property_name, bool current)
        {
            return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(by)) { return "E2"; }
            if (property_name == nameof(p_value)) { return "E2"; }
            if (property_name == nameof(log_odds_ratio)) { return "0.####"; }
            return null;
        }

        #endregion Private Methods
    }
}