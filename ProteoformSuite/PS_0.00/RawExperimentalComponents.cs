using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel; //Right click Solution/Explorer/References. Then Add  "Reference". Assemblies/Extension/Microsoft.Office.Interop.Excel
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ProteoformSuite
{
    public partial class RawExperimentalComponents : Form
    {
        public RawExperimentalComponents()
        {
            InitializeComponent();
        }

        public void RawExperimentalComponents_Load(object sender, EventArgs e)
        {
            if (Lollipop.raw_experimental_components.Count == 0) Lollipop.process_raw_components();
        }

        public void FillRawExpComponentsTable()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = Lollipop.raw_experimental_components;
            dgv_RawExpComp_MI_masses.DataSource = bs;
            dgv_RawExpComp_MI_masses.ReadOnly = true;
            //dgv_RawExpComp_MI_masses.Columns["Monoisotopic Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Delta Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Weighted Monoisotopic Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Apex RT"].DefaultCellStyle.Format = "0.##";
            //dgv_RawExpComp_MI_masses.Columns["Relative Abundance"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Fractional Abundance"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Sum Intensity"].DefaultCellStyle.Format = "0";
            dgv_RawExpComp_MI_masses.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_RawExpComp_MI_masses.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private void dgv_RawExpComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Component c = (Component)this.dgv_RawExpComp_MI_masses.Rows[e.RowIndex].DataBoundItem;

                //Round doubles before displaying
                BindingSource bs = new BindingSource();
                bs.DataSource = new BindingList<ChargeState>(c.charge_states);
                dgv_RawExpComp_IndChgSts.DataSource = bs;
                dgv_RawExpComp_IndChgSts.ReadOnly = true;
                //dgv_RawExpComp_IndChgSts.Columns["MZ Centroid"].DefaultCellStyle.Format = "0.####";
                //dgv_RawExpComp_IndChgSts.Columns["Calculated Mass"].DefaultCellStyle.Format = "0.####";
                //dgv_RawExpComp_IndChgSts.Columns["Intensity"].DefaultCellStyle.Format = "0";
                dgv_RawExpComp_IndChgSts.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                dgv_RawExpComp_IndChgSts.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
            }
        }
    }
}
