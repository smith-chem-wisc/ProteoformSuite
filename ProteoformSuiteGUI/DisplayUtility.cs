using ProteoformSuiteInternal;
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

namespace ProteoformSuite
{
    public class DisplayUtility
    {
        public static void FillDataGridView(DataGridView dgv, IEnumerable<object> someList)
        {
            SortableBindingList<object> sbl = new SortableBindingList<object>(someList);
            dgv.DataSource = sbl;
            dgv.ReadOnly = false;
            dgv.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        public static void tooltip_graph_display(ToolTip t, MouseEventArgs e, Chart c, Point? p)
        {
            t.RemoveAll();
            Point pos = e.Location;
            if (p.HasValue && pos == p.Value) return;

            HitTestResult[] results = new HitTestResult[4];

            try
            {
                results = c.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);

                foreach (HitTestResult result in results)
                {
                    if (result.ChartElementType == ChartElementType.DataPoint)
                    {
                        DataPoint prop = result.Object as DataPoint;
                        if (prop != null)
                        {
                            double pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                            double pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                            // check if the cursor is really close to the point (2 pixels around the point)
                            if (Math.Abs(pos.X - pointXPixel) < 2) //&& Math.Abs(pos.Y - pointYPixel) < 2)
                                t.Show("X=" + prop.XValue + ", Y=" + prop.YValues[0], c, pos.X, pos.Y - 15);
                        }
                    }
                }
            }
            catch
            {

            }

           
        }

        public static void GraphRelationsChart(Chart ct, List<ProteoformRelation> relations, string series)
        {
            ct.Series[series].Points.Clear();
            ct.Series[series].XValueMember = "delta_mass";
            ct.Series[series].YValueMembers = "nearby_relations_count";
            List<ProteoformRelation> relations_ordered = relations.OrderByDescending(r => r.delta_mass).ToList();
            ct.DataSource = relations_ordered;
            ct.DataBind();

            ct.ChartAreas[0].AxisY.StripLines.Clear();
            StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = Lollipop.min_peak_count };
            ct.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);

            ct.ChartAreas[0].AxisX.Title = "Delta m/z";
            ct.ChartAreas[0].AxisY.Title = "Nearby Count";

        }

        public static void GraphDeltaMassPeaks(Chart ct, List<DeltaMassPeak> peaks, string peak_series, string decoy_series, List<ProteoformRelation> relations, string relations_series)
        {
            ct.Series[peak_series].Points.Clear();
            ct.Series[decoy_series].Points.Clear();
            ct.Series[relations_series].Points.Clear();

            List<DeltaMassPeak> peaks_ordered = peaks.OrderBy(r => r.peak_deltaM_average).ToList();
            foreach (DeltaMassPeak peak in peaks_ordered)
            {
                ct.Series[peak_series].Points.AddXY(peak.peak_deltaM_average, peak.peak_relation_group_count);
                ct.Series[decoy_series].Points.AddXY(peak.peak_deltaM_average, peak.decoy_relation_count);
            }

            List<ProteoformRelation> relations_ordered = relations.OrderBy(r => r.delta_mass).ToList();
            foreach (ProteoformRelation relation in relations_ordered)
            {
                ct.Series[relations_series].Points.AddXY(relation.delta_mass, relation.nearby_relations_count);
            }
            ct.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            if (peaks_ordered.Count > 0) GraphSelectedDeltaMassPeak(ct, peaks_ordered[0], relations);
        }

        public static void GraphSelectedDeltaMassPeak(Chart ct, DeltaMassPeak peak, List<ProteoformRelation> relations)
        {
            ct.ChartAreas[0].AxisY.StripLines.Clear();
            ct.ChartAreas[0].AxisX.Minimum = peak.peak_deltaM_average - Lollipop.peak_width_base;
            ct.ChartAreas[0].AxisX.Maximum = peak.peak_deltaM_average + Lollipop.peak_width_base;

            ct.ChartAreas[0].AxisX.StripLines.Clear();
            double stripline_tolerance = Lollipop.peak_width_base * 0.5;
            StripLine lowerPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = peak.peak_deltaM_average - stripline_tolerance };
            StripLine upperPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = peak.peak_deltaM_average + stripline_tolerance };
            ct.ChartAreas[0].AxisX.StripLines.Add(lowerPeakBound_stripline);
            ct.ChartAreas[0].AxisX.StripLines.Add(upperPeakBound_stripline);

            ct.ChartAreas[0].AxisY.Maximum = 1 + Math.Max(
                Convert.ToInt32(peak.peak_relation_group_count * 1.2), 
                Convert.ToInt32(relations.Where(r => r.delta_mass >= peak.peak_deltaM_average - Lollipop.peak_width_base 
                    && r.delta_mass <= peak.peak_deltaM_average + Lollipop.peak_width_base).Select(r => r.nearby_relations_count).Max())
            ); //this automatically scales the vertical axis to the peak height plus 20%, also accounting for the nearby trace of unadjusted relation group counts

            ct.ChartAreas[0].AxisX.Title = "Delta m/z";
            ct.ChartAreas[0].AxisY.Title = "Count";
        }

        public static void FormatAggregatesTable(DataGridView dgv)
        {
            //round table values
            dgv.Columns["agg_mass"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["agg_intensity"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["agg_rt"].DefaultCellStyle.Format = "0.##";
            dgv.Columns["modified_mass"].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv.Columns["agg_mass"].HeaderText = "Aggregated Mass";
            dgv.Columns["agg_intensity"].HeaderText = "Aggregated Intensity";
            dgv.Columns["agg_rt"].HeaderText = "Aggregated RT";
            dgv.Columns["observation_count"].HeaderText = "Observation Count";
            dgv.Columns["accession"].HeaderText = "Experimental Proteoform ID";
            dgv.Columns["lysine_count"].HeaderText = "Lysine Count";

            //making these columns invisible. (irrelevent for agg proteoforms)
            dgv.Columns["is_target"].Visible = false;
            dgv.Columns["is_decoy"].Visible = false;
            dgv.Columns["modified_mass"].Visible = false;
            if (!Lollipop.neucode_labeled) { dgv.Columns["lysine_count"].Visible = false; }


            dgv.AllowUserToAddRows = false;
        }

        public static void FormatTheoreticalProteoformTable(DataGridView dgv)
        {
            //round table values
            dgv.Columns["unmodified_mass"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["ptm_mass"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["modified_mass"].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv.Columns["name"].HeaderText = "Name";
            dgv.Columns["fragment"].HeaderText = "Fragment";
            dgv.Columns["begin"].HeaderText = "Begin";
            dgv.Columns["end"].HeaderText = "End";
            dgv.Columns["unmodified_mass"].HeaderText = "Unmodified Mass";
            dgv.Columns["ptm_mass"].HeaderText = "PTM Mass";
            dgv.Columns["ptm_descriptions"].HeaderText = "PTM Description";
            dgv.Columns["accession"].HeaderText = "Accession";
            dgv.Columns["description"].HeaderText = "Description";
            dgv.Columns["modified_mass"].HeaderText = "Modified Mass";
            dgv.Columns["lysine_count"].HeaderText = "Lysine Count";

            //making these columns invisible.
            dgv.Columns["is_target"].Visible = false;
            dgv.Columns["is_decoy"].Visible = false;

            dgv.AllowUserToAddRows = false;
        }

        public static void FormatRelationsGridView(DataGridView dgv, bool mask_experimental, bool mask_theoretical)

        {
            //round table values
            dgv.Columns["delta_mass"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["peak_center_deltaM"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["proteoform_mass_1"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["proteoform_mass_2"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["agg_intensity_1"].DefaultCellStyle.Format = "0.##";
            dgv.Columns["agg_RT_1"].DefaultCellStyle.Format = "0.##";

            //set column header
            dgv.Columns["delta_mass"].HeaderText = "Delta Mass";
            dgv.Columns["delta_mass"].DisplayIndex = 18;
            dgv.Columns["nearby_relations_count"].HeaderText = "Nearby Relation Count";
            dgv.Columns["accepted"].HeaderText = "Accepted";
            dgv.Columns["peak_center_deltaM"].HeaderText = "Peak Center Delta Mass";
            dgv.Columns["peak_center_count"].HeaderText = "Peak Center Count";
            dgv.Columns["proteoform_mass_1"].HeaderText = "Experimental Aggregated Proteoform Mass";
            dgv.Columns["agg_intensity_1"].HeaderText = "Experimental Aggregated Intensity";
            dgv.Columns["agg_RT_1"].HeaderText = "Experimental Aggregated RT";
            dgv.Columns["lysine_count"].HeaderText = "Lysine Count";
            dgv.Columns["num_observations_1"].HeaderText = "Number Experimental Observations";
            dgv.Columns["outside_no_mans_land"].HeaderText = "Outside No Man's Land";

            //ET formatting
            dgv.Columns["proteoform_mass_2"].HeaderText = "Theoretical Proteoform Mass";
            dgv.Columns["accession"].HeaderText = "Accession";
            dgv.Columns["fragment"].HeaderText = "Fragment";
            dgv.Columns["ptm_list"].HeaderText = "PTM Description";
            dgv.Columns["name"].HeaderText = "Name";
            if (mask_experimental)
            {
                dgv.Columns["agg_intensity_2"].Visible = false;
                dgv.Columns["agg_RT_2"].Visible = false;
                dgv.Columns["num_observations_2"].Visible = false;
                dgv.Columns["relation_type_string"].Visible = false;
            }

            //EE formatting
            dgv.Columns["agg_RT_2"].HeaderText = "Light Experimental Aggregated RT";
            dgv.Columns["agg_intensity_2"].HeaderText = "Light Experimental Aggregated Intensity";
            dgv.Columns["proteoform_mass_2"].HeaderText = "Light Experimental Aggregated Intensity";
            dgv.Columns["num_observations_2"].HeaderText = "Number Light Experimental Observations";
            if (mask_theoretical)
            {
                dgv.Columns["accession"].Visible = false;
                dgv.Columns["name"].Visible = false;
                dgv.Columns["fragment"].Visible = false;
                dgv.Columns["ptm_list"].Visible = false;
                dgv.Columns["relation_type_string"].Visible = false;
            }

            //ProteoformFamilies formatting
            dgv.Columns["relation_type_string"].HeaderText = "Relation Type";

            //making these columns invisible
            dgv.Columns["peak"].Visible = false;
            if (!Lollipop.neucode_labeled) { dgv.Columns["lysine_count"].Visible = false; }

            dgv.AllowUserToAddRows = false;
        }

        public static void FormatPeakListGridView(DataGridView dgv)
        {
            //making all columns invisible first - faster
            foreach (DataGridViewColumn column in dgv.Columns) { column.Visible = false; }

            dgv.Columns["peak_deltaM_average"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["peak_group_fdr"].DefaultCellStyle.Format = "0.##";

            dgv.Columns["peak_relation_group_count"].HeaderText = "Peak Center Count";
            dgv.Columns["decoy_relation_count"].HeaderText = "Decoy Count under Peak";
            dgv.Columns["peak_deltaM_average"].HeaderText = "Peak Center Delta Mass";
            dgv.Columns["peak_group_fdr"].HeaderText = "Peak FDR";
            dgv.Columns["peak_accepted"].HeaderText = "Peak Accepted";
            dgv.Columns["possiblePeakAssignments_string"].HeaderText = "Peak Assignment";
            dgv.Columns["peak_width"].HeaderText = "Peak Width";

            dgv.Columns["peak_relation_group_count"].Visible = true;
            dgv.Columns["decoy_relation_count"].Visible = true;
            dgv.Columns["peak_deltaM_average"].Visible = true;
            dgv.Columns["peak_group_fdr"].Visible = true;
            dgv.Columns["peak_accepted"].Visible = true;
            dgv.Columns["possiblePeakAssignments_string"].Visible = true;
            dgv.Columns["peak_width"].Visible = false;

            dgv.AllowUserToAddRows = false;
        }
    }
}
