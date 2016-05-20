using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace PS_0._00
{
    public partial class NeuCodePairs : Form
    {
        public NeuCodePairs()
        {
            InitializeComponent();
            this.ct_IntensityRatio.MouseMove += new MouseEventHandler(ct_IntensityRatio_MouseMove);
            this.ct_LysineCount.MouseMove += new MouseEventHandler(ct_LysineCount_MouseMove);
        }

        public void NeuCodePairs_Load(object sender, EventArgs e)
        {
            if (GlobalData.rawNeuCodePairs.Count == 0)
            {
                Dictionary<string, HashSet<string>> filename_scanRanges = get_scanranges_by_filename();
                FillRawNeuCodePairsDataTable(filename_scanRanges);
                FillNeuCodePairsDGV(); //Filling DGV part of the working logic, now, since it seems to take a while
            }
            GraphLysineCount();
            GraphIntensityRatio();
        }

        private void FillNeuCodePairsDGV()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = GlobalData.rawNeuCodePairs;
            dgv_RawExpNeuCodePairs.DataSource = bs;
            dgv_RawExpNeuCodePairs.ReadOnly = true;
            //dgv_RawExpNeuCodePairs.Columns["Acceptable"].ReadOnly = false;
            //dgv_RawExpNeuCodePairs.Columns["Light Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpNeuCodePairs.Columns["Light Mass Corrected"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpNeuCodePairs.Columns["Heavy Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpNeuCodePairs.Columns["Intensity Ratio"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpNeuCodePairs.Columns["Apex RT"].DefaultCellStyle.Format = "0.##";
            //dgv_RawExpNeuCodePairs.Columns["Light Intensity"].DefaultCellStyle.Format = "0";
            //dgv_RawExpNeuCodePairs.Columns["Heavy Intensity"].DefaultCellStyle.Format = "0";
            dgv_RawExpNeuCodePairs.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_RawExpNeuCodePairs.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }
        
        private void FillRawNeuCodePairsDataTable(Dictionary<string, HashSet<string>> filename_scanRange)
        {
            Parallel.ForEach<KeyValuePair<string, HashSet<string>>>(filename_scanRange, entry =>
            {
                string filename = entry.Key;
                Parallel.ForEach<string>(entry.Value, scanRange =>
               {
                   List<Component> components_in_file_scanrange = new List<Component>();

                   //select all components in file and this particular scanrange
                   Parallel.ForEach<Component>(GlobalData.rawExperimentalComponents, c =>
                   {
                       if (c.file_origin == filename && c.scan_range == scanRange)
                           components_in_file_scanrange.Add(c);
                   });

                   components_in_file_scanrange.OrderBy(c => c.weighted_monoisotopic_mass);

                   //Add putative neucode pairs. Must be in same spectrum, mass must be within 6 Da of each other
                   int lower_mass_index = 0;
                   Parallel.For(lower_mass_index, components_in_file_scanrange.Count - 2, lower_index =>
                   {
                       Component lower_component = components_in_file_scanrange[lower_index];
                       int higher_mass_index = lower_mass_index + 1;
                       double apexRT = lower_component.rt_apex;

                       Parallel.For(higher_mass_index, components_in_file_scanrange.Count - 1, higher_index =>
                       {
                           Component higher_component = components_in_file_scanrange[higher_index];
                           double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass; //changed from decimal; it doesn't seem like that should make a difference
                           if (mass_difference < 6)
                           {
                               Proteoform p = new Proteoform(lower_component, higher_component);
                               if (p.accepted) { GlobalData.rawNeuCodePairs.Add(p); }
                           }
                       });
                   });
               });
            });
        }

        private Dictionary<string, HashSet<string>> get_scanranges_by_filename()
        {
            Dictionary<string, HashSet<string>> filename_scanRanges = new Dictionary<string, HashSet<string>>();
            Parallel.ForEach<string>(GlobalData.deconResultsFileNames, filename =>
            {
                if (!filename_scanRanges.ContainsKey(filename))
                    filename_scanRanges.Add(filename, new HashSet<string>());
            });

            Parallel.ForEach<Component>(GlobalData.rawExperimentalComponents, c =>
            {
                filename_scanRanges[c.file_origin].Add(c.scan_range);
            });
            return filename_scanRanges;
        }

        private DataTable CreateRawNeuCodePairsDataTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Light Filename", typeof(string));
            dt.Columns.Add("Light No.", typeof(int));
            dt.Columns.Add("Light Mass", typeof(double));
            dt.Columns.Add("Light Mass Corrected", typeof(double));
            dt.Columns.Add("Light Intensity", typeof(double));
            dt.Columns.Add("Heavy Filename", typeof(string));
            dt.Columns.Add("Heavy No.", typeof(int));
            dt.Columns.Add("Heavy Mass", typeof(double));
            dt.Columns.Add("Heavy Intensity", typeof(double));
            dt.Columns.Add("Matching Charge States", typeof(List<int>));
            dt.Columns.Add("Apex RT", typeof(double));
            dt.Columns.Add("Intensity Ratio", typeof(double));
            dt.Columns.Add("Lysine Count", typeof(int));
            dt.Columns.Add("Acceptable", typeof(bool));

            return dt;
        }

        private void GraphIntensityRatio()
        {
            DataTable intensityRatioHistogram = new DataTable();
            intensityRatioHistogram.Columns.Add("intRatio", typeof(double));
            intensityRatioHistogram.Columns.Add("numPairsAtThisIntRatio", typeof(int));

            int ymax = 0;
            for (double i = 0; i <= 20; i = i + 0.05)
            {
                string expression = "[Intensity Ratio] >= " + (i - .025) + "AND [Intensity Ratio] < " + (i + .025);
                List<Proteoform> proteoforms_by_intensityRatio = GlobalData.rawNeuCodePairs.Where(p => p.intensity_ratio >= i - 0.025 && p.intensity_ratio < i + 0.025).ToList();
                if (proteoforms_by_intensityRatio.Count > ymax)
                    ymax = proteoforms_by_intensityRatio.Count;
                intensityRatioHistogram.Rows.Add(i, proteoforms_by_intensityRatio.Count);
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

            ct_IntensityRatio.ChartAreas[0].AxisX.Title = "Intensity Ratio of a Pair";
            ct_IntensityRatio.ChartAreas[0].AxisY.Title = "Number of NeuCode Pairs";

            ct_IntensityRatio.DataSource = intensityRatioHistogram;
            ct_IntensityRatio.DataBind();

        }

        private void GraphLysineCount()
        {
            DataTable lysCtHistogram = new DataTable();
            lysCtHistogram.Columns.Add("numLysines", typeof(int));
            lysCtHistogram.Columns.Add("numPairsAtThisLysCt", typeof(int));

            int ymax = 0;
            //double xInt = 0.2;

            for (int i = 0; i <= 28; i++)
            {
                List<Proteoform> pf_by_lysCt = GlobalData.rawNeuCodePairs.Where(p => p.lysine_count == i).ToList();
                if (pf_by_lysCt.Count > ymax)
                    ymax = pf_by_lysCt.Count;
                lysCtHistogram.Rows.Add(i, pf_by_lysCt.Count);
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

            ct_LysineCount.ChartAreas[0].AxisX.Title = "Lysine Count";
            ct_LysineCount.ChartAreas[0].AxisY.Title = "Number of NeuCode Pairs";

            ct_LysineCount.DataSource = lysCtHistogram;
            ct_LysineCount.DataBind();

        }

        Point? ct_intensityRatio_prevPosition = null;
        ToolTip ct_intensityRatio_tt = new ToolTip();

        void ct_IntensityRatio_MouseMove(object sender, MouseEventArgs e)
        {
            tooltip_graph_display(ct_intensityRatio_tt, e, ct_IntensityRatio, ct_intensityRatio_prevPosition);
        }

        Point? ct_LysineCount_prevPosition = null;
        ToolTip ct_LysineCount_tt = new ToolTip();

        void ct_LysineCount_MouseMove(object sender, MouseEventArgs e)
        {
            tooltip_graph_display(ct_LysineCount_tt, e, ct_LysineCount, ct_LysineCount_prevPosition);
        }

        private void tooltip_graph_display(ToolTip t, MouseEventArgs e, Chart c, Point? p)
        {
            var pos = e.Location;
            if (p.HasValue && pos == p.Value) return;
            t.RemoveAll();
            p = pos;
            var results = c.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 2) //&&
                                                               // Math.Abs(pos.Y - pointYPixel) < 2)
                        {
                            t.Show("X=" + prop.XValue + ", Y=" + prop.YValues[0], c, pos.X, pos.Y - 15);
                        }
                    }
                }
            }
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

        private void parse_neucode_param_change(List<Proteoform> selected_pf)
        {
            Parallel.ForEach(selected_pf, p => { p.accepted = false; });
            dgv_RawExpNeuCodePairs.Refresh();
        }

        private void KMinAcceptable_ValueChanged(object sender, EventArgs e)
        {
            List<Proteoform> selected_pf = GlobalData.rawNeuCodePairs.Where(p => p.lysine_count < double.Parse(KMinAcceptable.Value.ToString())).ToList();
            Parallel.ForEach(selected_pf, p => { p.accepted = false; });
            dgv_RawExpNeuCodePairs.Refresh();
        }

        private void KMaxAcceptable_ValueChanged(object sender, EventArgs e)
        {
            List<Proteoform> selected_pf = GlobalData.rawNeuCodePairs.Where(p => p.lysine_count > double.Parse(KMaxAcceptable.Value.ToString())).ToList();
            Parallel.ForEach(selected_pf, p => { p.accepted = false; });
            dgv_RawExpNeuCodePairs.Refresh();
        }

        private void IRatMinAcceptable_ValueChanged(object sender, EventArgs e)
        {
            List<Proteoform> selected_pf = GlobalData.rawNeuCodePairs.Where(p => p.intensity_ratio < double.Parse(IRatMinAcceptable.Value.ToString())).ToList();
            Parallel.ForEach(selected_pf, p => { p.accepted = false; });
            dgv_RawExpNeuCodePairs.Refresh();
        }

        private void IRatMaxAcceptable_ValueChanged(object sender, EventArgs e)
        {
            List<Proteoform> selected_pf = GlobalData.rawNeuCodePairs.Where(p => p.intensity_ratio > double.Parse(IRatMaxAcceptable.Value.ToString())).ToList();
            Parallel.ForEach(selected_pf, p => { p.accepted = false; });
            dgv_RawExpNeuCodePairs.Refresh();
        }

        public override string ToString()
        {
            return String.Join(System.Environment.NewLine, new string[] {
                "NeuCodePairs|KMaxAcceptable.Value\t" + KMaxAcceptable.Value.ToString(),
                "NeuCodePairs|KMinAcceptable.Value\t" + KMinAcceptable.Value.ToString(),
                "NeuCodePairs|IRatMaxAcceptable.Value\t" + IRatMaxAcceptable.Value.ToString(),
                "NeuCodePairs|IRatMinAcceptable.Value\t" + IRatMinAcceptable.Value.ToString()
            });
        }

        public void loadSetting(string setting_specs)
        {
            string[] fields = setting_specs.Split('\t');
            switch (fields[0].Split('|')[1])
            {
                case "KMaxAcceptable.Value":
                    KMaxAcceptable.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "KMinAcceptable.Value":
                    KMinAcceptable.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "IRatMaxAcceptable.Value":
                    IRatMaxAcceptable.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "IRatMinAcceptable.Value":
                    IRatMinAcceptable.Value = Convert.ToDecimal(fields[1]);
                    break;
            }
        }
    }
}
