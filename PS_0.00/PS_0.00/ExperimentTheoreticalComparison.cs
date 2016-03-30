using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS_0._00
{
    public partial class ExperimentTheoreticalComparison : Form
    {
        DataTable etPeakList = new DataTable();
        //Zach wuz here
        //still hereeeeee
        public ExperimentTheoreticalComparison()
        {
            InitializeComponent();
        }

        private void ExperimentTheoreticalComparison_Load(object sender, EventArgs e)
        {
            InitializeParameterSet();
            FindAllETPairs();
            CalculateRunningSums();
            FillETGridView();
            GraphETHistogram();
            InitializeETPeakListTable();
            FillETPeakListTable();
            FillETGridView();
            UpdateFiguresOfMerit();
        }

        //private void RunTheGamut()
        //{
        //    FindAllETPairs();
        //    CalculateRunningSums();
        //    FillETGridView();
        //    GraphETHistogram();
        //    InitializeETPeakListTable();
        //    FillETPeakListTable();
        //    FillETGridView();
        //    UpdateFiguresOfMerit();
        //}

        private void UpdateFiguresOfMerit()
        {
            int peakSum = 0;
            int peakCount = 0;
            DataRow[] bigPeaks = etPeakList.Select("[Peak Count] >= " + nUD_PeakCountMinThreshold.Value);
            foreach (DataRow row in bigPeaks)
            {
                peakSum = peakSum + Convert.ToInt32(row["Peak Count"]);
                peakCount++;
            }
            tb_IdentifiedProteoforms.Text = peakSum.ToString();
            tb_TotalPeaks.Text = peakCount.ToString();
        }

        //private void ZeroETPairsTableValues()
        //{
        //    foreach (DataRow row in GlobalData.experimentTheoreticalPairs.Rows)
        //    {
        //        row["Acceptable Peak"] = false;
        //        row["Peak Center Count"] = 0;
        //    }
        //}

        //private void ClearETPeakListTable()
        //{
        //    etPeakList.Clear();
        //    InitializeETPeakListTable();
        //}

        private void FillETPeakListTable()
        {
            string colName = "Running Sum";
            string direction = "DESC";

            DataTable dt = GlobalData.experimentTheoreticalPairs;
            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();
            GlobalData.experimentTheoreticalPairs = dt;

            foreach (DataRow row in GlobalData.experimentTheoreticalPairs.Rows)
            {
                if (Convert.ToBoolean(row["Out of Range Decimal"].ToString())==false && Convert.ToBoolean(row["Acceptable Peak"].ToString()) == false)
                {
                    double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                    double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper + " and [Acceptable Peak] = false";
                    DataRow[] firstSet = GlobalData.experimentTheoreticalPairs.Select(expression);
                    var firstAverage = firstSet.Average(fsRow => fsRow.Field<double>("Delta Mass"));

                    lower = firstAverage - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    upper = firstAverage + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper + " and [Acceptable Peak] = false";
                    DataRow[] secondSet = GlobalData.experimentTheoreticalPairs.Select(expression);
                    var secondAverage = secondSet.Average(ssRow => ssRow.Field<double>("Delta Mass"));

                    foreach (DataRow rutrow in secondSet)
                    {
                        rutrow["Peak Center Count"] = secondSet.Length;
                        rutrow["Peak Center Mass"] = secondAverage;
                        rutrow["Acceptable Peak"] = true;

                        rutrow.EndEdit();
                    }

                    if (secondSet.Length >= nUD_PeakCountMinThreshold.Value)
                    {
                        etPeakList.Rows.Add(secondAverage, secondSet.Length, true);
                    }
                    else
                    {
                        etPeakList.Rows.Add(secondAverage, secondSet.Length, false);
                    }                  

                }
                GlobalData.experimentTheoreticalPairs.AcceptChanges();
            }

            BindingSource dgv_ET_Peak_List_BS = new BindingSource();
            dgv_ET_Peak_List_BS.DataSource = etPeakList;
            dgv_ET_Peak_List.DataSource = dgv_ET_Peak_List_BS;
            dgv_ET_Peak_List.AutoGenerateColumns = true;
            dgv_ET_Peak_List.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_ET_Peak_List.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }



        private void InitializeETPeakListTable()
        {
            etPeakList.Columns.Add("Average Delta Mass", typeof(double));
            etPeakList.Columns.Add("Peak Count", typeof(int));
            etPeakList.Columns.Add("Acceptable", typeof(bool));
        }

        private void GraphETHistogram()
        {
            string colName = "Delta Mass";
            string direction = "DESC";

            DataTable dt = GlobalData.experimentTheoreticalPairs;
            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();
            GlobalData.experimentTheoreticalPairs = dt;

            ct_ET_Histogram.Series["etHistogram"].XValueMember = "Delta Mass";
            ct_ET_Histogram.Series["etHistogram"].YValueMembers = "Running Sum";

            ct_ET_Histogram.DataSource = GlobalData.experimentTheoreticalPairs;
            ct_ET_Histogram.DataBind();

        }

        private void FillETGridView()
        {
            BindingSource dgv_DT_BS = new BindingSource();
            dgv_DT_BS.DataSource = GlobalData.experimentTheoreticalPairs;
            dgv_ET_Pairs.DataSource = dgv_DT_BS;
            dgv_ET_Pairs.AutoGenerateColumns = true;
            dgv_ET_Pairs.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_ET_Pairs.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private DataTable GetNewET_DataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Accession", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Fragment", typeof(string));
            dt.Columns.Add("PTM List", typeof(string));
            dt.Columns.Add("Proteoform Mass", typeof(double));
            dt.Columns.Add("Aggregated Mass", typeof(double));
            dt.Columns.Add("Aggregated Intensity", typeof(double));
            dt.Columns.Add("Aggregated Retention Time", typeof(double));
            dt.Columns.Add("Lysine Count", typeof(int));
            dt.Columns.Add("Delta Mass", typeof(double));
            dt.Columns.Add("Running Sum", typeof(int));
            dt.Columns.Add("Peak Center Count", typeof(int));
            dt.Columns.Add("Peak Center Mass", typeof(double));
            dt.Columns.Add("Out of Range Decimal", typeof(bool));
            dt.Columns.Add("Acceptable Peak", typeof(bool));

            return dt;
        }

        private void FindAllETPairs()
        {
            DataTable eT = new DataTable();
            eT = GetNewET_DataTable();

            foreach (DataRow agRow in GlobalData.aggregatedProteoforms.Rows)
            {
                double lowMass = Convert.ToDouble(agRow["Aggregated Mass"]) + Convert.ToDouble(nUD_ET_Lower_Bound.Value);
                double highMass = Convert.ToDouble(agRow["Aggregated Mass"]) + Convert.ToDouble(nUD_ET_Upper_Bound.Value);

                string expression = "[Proteoform Mass] >= " + lowMass + " and [Proteoform Mass] <= " + highMass;
                expression = expression + "and [Lysine Count] >= " + agRow["Lysine Count"];

                DataRow[] closeTheoreticals = GlobalData.theoreticalAndDecoyDatabases.Tables["Target"].Select(expression);

                foreach (DataRow row in closeTheoreticals)
                {
                    double deltaMass = Convert.ToDouble(agRow["Aggregated Mass"])- Convert.ToDouble(row["Proteoform Mass"]);
                    double afterDecimal = Math.Abs(deltaMass-Math.Truncate(deltaMass));
                    bool oOR = true;
                    if (afterDecimal<= Convert.ToDouble(nUD_NoManLower.Value) || afterDecimal >= Convert.ToDouble(nUD_NoManUpper.Value))
                    {
                        oOR = false;
                    }
                    else
                    {
                        oOR = true;
                    }
                    eT.Rows.Add(row["Accession"], row["Name"], row["Fragment"], row["PTM List"], row["Proteoform Mass"], agRow["Aggregated Mass"], agRow["Aggregated Intensity"], agRow["Aggregated Retention Time"], agRow["Lysine Count"],  deltaMass, 0, 0, deltaMass, oOR, false);
                    //set out of range variable
                }

            }

            GlobalData.experimentTheoreticalPairs = eT;
        }

        private void CalculateRunningSums()
        {
            foreach (DataRow row in GlobalData.experimentTheoreticalPairs.Rows)
            {
                double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper;
                row["Running Sum"] = GlobalData.experimentTheoreticalPairs.Select(expression).Length;
            }
        }

        private void splitContainer3_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void nUD_ET_Lower_Bound_ValueChanged(object sender, EventArgs e)
        {
            //RunTheGamut();
        }

        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e)
        {
            //RunTheGamut();
        }

        private void yMaxET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxET.Value.ToString());
        }

        private void yMinET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisY.Minimum = double.Parse(yMinET.Value.ToString());
        }

        private void xMinET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisX.Minimum = double.Parse(xMinET.Value.ToString());
        }

        private void xMaxET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxET.Value.ToString());
        }

        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e)
        {
            //RunTheGamut();
        }

        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            //RunTheGamut();
        }

        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
            //RunTheGamut();
        }

        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            //ClearETPeakListTable();
            //ZeroETPairsTableValues();
            //FillETPeakListTable();
            //UpdateFiguresOfMerit();
        }

        private void InitializeParameterSet()
        {
            nUD_ET_Lower_Bound.Minimum = -500;
            nUD_ET_Lower_Bound.Maximum = 0;
            nUD_ET_Lower_Bound.Value = -250;

            nUD_ET_Upper_Bound.Minimum = 0;
            nUD_ET_Upper_Bound.Maximum = 500;
            nUD_ET_Upper_Bound.Value = 250;

            yMaxET.Minimum = 0;
            yMaxET.Maximum = 1000;
            yMaxET.Value = 100;

            yMinET.Minimum = -100;
            yMinET.Maximum = yMaxET.Maximum;
            yMinET.Value = 0;

            xMinET.Minimum = nUD_ET_Lower_Bound.Value;
            xMinET.Maximum = xMaxET.Value;
            xMinET.Value = nUD_ET_Lower_Bound.Value;

            xMaxET.Minimum = xMinET.Value;
            xMaxET.Maximum = nUD_ET_Upper_Bound.Value;
            xMaxET.Value = nUD_ET_Upper_Bound.Value;

            nUD_NoManLower.Minimum = 00m;
            nUD_NoManLower.Maximum = 0.49m;
            nUD_NoManLower.Value = 0.22m;//add the m suffix to treat the double as decimal

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            nUD_NoManUpper.Value = 0.88m;

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = 0.0150m;

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = 10;
        }
    }
}
