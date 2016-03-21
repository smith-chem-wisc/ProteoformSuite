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
    public partial class neuCodePairs : Form
    {
        public neuCodePairs()
        {
            InitializeComponent();
        }

        private void neuCodePairs_Load(object sender, EventArgs e)
        {
            GlobalData.rawNeuCodePairs = CreateRawNeuCodePairsDataTable();
            Dictionary<string, List<string>> fileNameScanRanges = GetSFileNameScanRangesList();
            FillRawNeuCodePairsDataTable(fileNameScanRanges);
            

            BindingSource dgv_rawNCPairs = new BindingSource();
            dgv_rawNCPairs.DataSource = GlobalData.rawNeuCodePairs;
            dgv_RawExpNeuCodePairs.DataSource = dgv_rawNCPairs;
            dgv_RawExpNeuCodePairs.AutoGenerateColumns = true;
            dgv_RawExpNeuCodePairs.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_RawExpNeuCodePairs.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;

            GraphLysineCount();

            GraphIntensityRatio();
            
        }

        private void GraphIntensityRatio()
        {
            DataTable intensityRatioHistogram = new DataTable();
            intensityRatioHistogram.Columns.Add("intRatio", typeof(double));
            intensityRatioHistogram.Columns.Add("numPairsAtThisIntRatio", typeof(int));

            int ymax = 0;

            for (double i = 0; i <= 20; i=i+0.05)
            {
                string expression = "[Intensity Ratio] >= " + (i-.025) + "AND [Intensity Ratio] < " + (i + .025);
                DataRow[] rows = GlobalData.rawNeuCodePairs.Select(expression);
                int iRCt = rows.Count();
                if (iRCt > ymax) { ymax = iRCt; }
                intensityRatioHistogram.Rows.Add(i, iRCt);
            }

            ct_IntensityRatio.Series["intensityRatio"].XValueMember = "intRatio";
            ct_IntensityRatio.Series["intensityRatio"].YValueMembers = "numPairsAtThisIntRatio";

            yMaxIRat.Maximum = ymax;
            yMaxIRat.Minimum = 0;

            yMinIRat.Maximum = ymax;
            yMinIRat.Minimum = 0;

            xMaxIRat.Maximum = 20;
            xMaxIRat.Minimum = 0;

            xMinIRat.Maximum = 20;
            xMinIRat.Minimum = 0;

            yMaxIRat.Value = ymax;
            yMinIRat.Value = 0;
            xMaxIRat.Value = 20;
            xMinIRat.Value = 0;

            IRatMaxAcceptable.Value = 6;
            IRatMinAcceptable.Maximum = 20;
            IRatMinAcceptable.Minimum = 0;

            IRatMinAcceptable.Value = 1.4m;
            IRatMinAcceptable.Maximum = 20;
            IRatMinAcceptable.Minimum = 0;


            ct_IntensityRatio.DataSource = intensityRatioHistogram;
            ct_IntensityRatio.DataBind();

        }

        private void GraphLysineCount()
        {
            DataTable lysCtHistogram = new DataTable();
            lysCtHistogram.Columns.Add("numLysines", typeof(int));
            lysCtHistogram.Columns.Add("numPairsAtThisLysCt", typeof(int));

            int ymax = 0;

            for (int i = 0; i <= 28; i++)
            {
                string expression = "[Lysine Count] = " + i;
                DataRow[] rows = GlobalData.rawNeuCodePairs.Select(expression);
                int kCt = rows.Count();
                if (kCt > ymax) { ymax = kCt; }
                lysCtHistogram.Rows.Add(i, kCt);
            }

            ct_LysineCount.Series["lysineCount"].XValueMember = "numLysines";
            ct_LysineCount.Series["lysineCount"].YValueMembers = "numPairsAtThisLysCt";

            yMaxKCt.Maximum = ymax;
            yMaxKCt.Minimum = 0;

            yMinKCt.Maximum = ymax;
            yMinKCt.Minimum = 0;

            xMaxKCt.Maximum = 28;
            xMaxKCt.Minimum = 0;

            xMinKCt.Maximum = 28;
            xMinKCt.Minimum = 0;

            yMaxKCt.Value = ymax;
            yMinKCt.Value = 0;
            xMaxKCt.Value = 28;
            xMinKCt.Value = 0;

            KMaxAcceptable.Value = 26.2m;
            KMaxAcceptable.Maximum = 28;
            KMaxAcceptable.Minimum = 0;
            KMinAcceptable.Value = 1.5m;
            KMinAcceptable.Maximum = 28;
            KMinAcceptable.Minimum = 0;

            ct_LysineCount.DataSource = lysCtHistogram;
            ct_LysineCount.DataBind();
            
        }

        private void FillRawNeuCodePairsDataTable(Dictionary<string,List<string>> fNSR)
        {
            foreach (KeyValuePair<string,List<string>> entry in fNSR)
            {
                string fileName = entry.Key;
                foreach (string scanRange in entry.Value)
                {
                    string expression = "[Filename] = '" + fileName + "' AND [Scan Range] = '" + scanRange + "'";// square brackets are key to avoiding missing operand error
                    DataRow[] rows = GlobalData.rawExperimentalProteoforms.Select(expression);
                    double apexRT = 0;
                    if (rows.Length > 0) { apexRT = double.Parse(rows[0]["Apex RT"].ToString()); }
                    List<double> masses = new List<double>();
                    Dictionary<double, int> entryNumber = new Dictionary<double, int>();
                    foreach (DataRow row in rows)
                    {

                        masses.Add(double.Parse(row["Weighted Monoisotopic Mass"].ToString()));
                        //MessageBox.Show("col val: " +row[GlobalData.rawExperimentalProteoforms.Columns[0].ColumnName].ToString());
                        entryNumber.Add(double.Parse(row["Weighted Monoisotopic Mass"].ToString()), int.Parse(row[GlobalData.rawExperimentalProteoforms.Columns[0].ColumnName].ToString()));
                    }
                    masses.Sort();
                    
                    if (masses.Count() > 1)
                    {
                        for (int low = 0; low <= (masses.Count()-2); low++)
                        {
                            for (int high = (low + 1); high <= (masses.Count()-1); high++)
                            {
                                double difference = masses[high] - masses[low];
                                //MessageBox.Show("mass difference" + difference);
                                if (difference < 6)
                                {
                                    List<int> oLC = GetOverLappingChargeStates(fileName, entryNumber[masses[low]],entryNumber[masses[high]]);
                                    double low_int = 0;
                                    double high_int = 0;
                                    if (oLC.Count() > 0)
                                    {
                                        low_int = GetCSIntensitySum(fileName, entryNumber[masses[low]], oLC);
                                        high_int = GetCSIntensitySum(fileName, entryNumber[masses[high]], oLC);
                                    }

                                    if(low_int>0 && high_int > 0)
                                    {
                                        int diff_int = Convert.ToInt32(Math.Round(difference / 1.0015 - 0.5, 0, MidpointRounding.AwayFromZero));
                                        if (low_int > high_int)//lower mass is neucode light
                                        {
                                            double firstCorrection = masses[low] + diff_int * 1.0015;
                                            int lysine_count = Math.Abs(Convert.ToInt32(Math.Round((masses[high]-firstCorrection) / 0.036015372, 0, MidpointRounding.AwayFromZero)));
                                            double intensityRatio = low_int / high_int;
                                            double lt_corrected_mass = masses[low] + Math.Round((lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * 1.0015;
                                            AddOneRawNeuCodePair(fileName, entryNumber[masses[low]], masses[low], lt_corrected_mass, low_int, fileName, entryNumber[masses[high]], masses[high], high_int, oLC, apexRT, intensityRatio, lysine_count, true);
                                        }
                                        else //higher mass is neucode light
                                        {
                                            double firstCorrection = masses[high] - (diff_int + 1) * 1.0015;
                                            int lysine_count = Math.Abs(Convert.ToInt32(Math.Round((masses[low] - firstCorrection) / 0.036015372, 0, MidpointRounding.AwayFromZero)));
                                            double intensityRatio = high_int / low_int;
                                            double lt_corrected_mass = masses[high] + Math.Round((lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * 1.0015;
                                            AddOneRawNeuCodePair(fileName, entryNumber[masses[high]], masses[high], lt_corrected_mass, high_int, fileName, entryNumber[masses[low]], masses[low], low_int, oLC, apexRT, intensityRatio, lysine_count, true);
                                        }                                      
                                        
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddOneRawNeuCodePair(string lt_fn, int lt_ent_num, double lt_mass, double lt_mass_corrected,
                double lt_int, string hv_fn, int hv_ent_num, double hv_mass, double hv_int, List<int> CS, double apexRT,
                double int_ratio, int lys_ct, bool acceptable)
        {
            //MessageBox.Show("pair added");
            GlobalData.rawNeuCodePairs.Rows.Add(lt_fn, lt_ent_num, lt_mass, lt_mass_corrected,
                 lt_int, hv_fn, hv_ent_num, hv_mass, hv_int, CS, apexRT, int_ratio, lys_ct, acceptable);
        }

        private double GetCSIntensitySum(string fileName, int entryNumber, List<int> oLC)
        {
            DataTable dt = GlobalData.rawExperimentalChargeStateData.Tables[fileName + "_" + entryNumber];

            double intensitySum = 0;

            foreach(int cs in oLC)
            {
                string expression = "[Charge State] = " + cs;
                DataRow[] rows = dt.Select(expression);
                foreach (DataRow row in rows)
                {
                    intensitySum = intensitySum + double.Parse(row["Intensity"].ToString());
                }
            }

            return intensitySum;
        }

        private List<int> GetOverLappingChargeStates(string fileName, double low_number, double high_number)
        {
            //MessageBox.Show("trying to get a list of overlapping CS");
            //MessageBox.Show("filename: " + fileName + " low number: " + low_number + " high number: " + high_number);
            List<int> lowMassCS = new List<int>();
            List<int> highMassCS = new List<int>();
            List<int> oCS = new List<int>();

            DataTable dlow = GlobalData.rawExperimentalChargeStateData.Tables[fileName+"_"+low_number];
            DataTable dhigh = GlobalData.rawExperimentalChargeStateData.Tables[fileName + "_" + low_number];

            foreach (DataRow row in dlow.Rows)
            {
                lowMassCS.Add(int.Parse(row["Charge State"].ToString()));
            }
            foreach (DataRow row in dhigh.Rows)
            {
                highMassCS.Add(int.Parse(row["Charge State"].ToString()));
            }

            foreach (int CS in lowMassCS)
            {
                if (highMassCS.Contains(CS))
                {
                    oCS.Add(CS);
                }
            }

            return oCS;
        }

        private Dictionary<string,List<string>> GetSFileNameScanRangesList()
        {
            Dictionary<string, List<string>> FileNameScanRanges = new Dictionary<string, List<string>>();

            foreach (DataRow row in GlobalData.rawExperimentalProteoforms.Rows)
            {
                string fileName = row["Filename"].ToString();
                string scanRange = row["Scan Range"].ToString();
                if (FileNameScanRanges.ContainsKey(fileName))
                {
                    if (!FileNameScanRanges[fileName].Contains(scanRange))
                    {
                        FileNameScanRanges[fileName].Add(scanRange);
                    }
                }
                else
                {
                    List<string> scanRangeList = new List<string>();
                    scanRangeList.Add(scanRange);
                    FileNameScanRanges.Add(fileName, scanRangeList);
                }
            }

            return FileNameScanRanges;
        }

        private DataTable CreateRawNeuCodePairsDataTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Light Filename", typeof(string));
            dt.Columns.Add("Light No#", typeof(int));
            dt.Columns.Add("Light Mass", typeof(double));
            dt.Columns.Add("Light Mass Corrected", typeof(double));
            dt.Columns.Add("Light Intensity", typeof(double));
            dt.Columns.Add("Heavy Filename", typeof(string));
            dt.Columns.Add("Heavy No#", typeof(int));
            dt.Columns.Add("Heavy Mass", typeof(double));
            dt.Columns.Add("Heavy Intensity", typeof(double));
            dt.Columns.Add("Matching Charge States", typeof(List<int>));
            dt.Columns.Add("Apex RT", typeof(double));
            dt.Columns.Add("Intensity Ratio", typeof(double));
            dt.Columns.Add("Lysine Count", typeof(int));
            dt.Columns.Add("Acceptable", typeof(bool));

            return dt;
        }

        private void yMaxKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxKCt.Value.ToString());
        }

        private void yMinKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisY.Minimum = double.Parse(yMinKCt.Value.ToString());
        }

        private void xMinKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisX.Minimum = double.Parse(xMinKCt.Value.ToString());
        }

        private void xMaxKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxKCt.Value.ToString());
        }

        private void yMaxIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxIRat.Value.ToString());
        }

        private void yMinIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisY.Minimum = double.Parse(yMinIRat.Value.ToString());
        }

        private void xMinIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisX.Minimum = double.Parse(xMinIRat.Value.ToString());
        }

        private void xMaxIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxIRat.Value.ToString());
        }

        private void KMinAcceptable_ValueChanged(object sender, EventArgs e)
        {
            string expression = "[Lysine Count] < " + double.Parse(KMinAcceptable.Value.ToString());
            DataRow[] rows = GlobalData.rawNeuCodePairs.Select(expression);
            foreach (DataRow row in rows)
            {
                row["Acceptable"] = false;
            }
            dgv_RawExpNeuCodePairs.Refresh();
        }

        private void KMaxAcceptable_ValueChanged(object sender, EventArgs e)
        {
            string expression = "[Lysine Count] > " + double.Parse(KMaxAcceptable.Value.ToString());
            DataRow[] rows = GlobalData.rawNeuCodePairs.Select(expression);
            foreach (DataRow row in rows)
            {
                row["Acceptable"] = false;
            }
            dgv_RawExpNeuCodePairs.Refresh();
        }
        private void IRatMinAcceptable_ValueChanged(object sender, EventArgs e)
        {
            string expression = "[Intensity Ratio] < " + double.Parse(IRatMinAcceptable.Value.ToString());
            DataRow[] rows = GlobalData.rawNeuCodePairs.Select(expression);
            foreach (DataRow row in rows)
            {
                row["Acceptable"] = false;
            }
            dgv_RawExpNeuCodePairs.Refresh();
        }

        private void IRatMaxAcceptable_ValueChanged(object sender, EventArgs e)
        {
            string expression = "[Intensity Ratio] > " + double.Parse(IRatMaxAcceptable.Value.ToString());
            DataRow[] rows = GlobalData.rawNeuCodePairs.Select(expression);
            foreach (DataRow row in rows)
            {
                row["Acceptable"] = false;
            }
            dgv_RawExpNeuCodePairs.Refresh();
        }
    }
}
