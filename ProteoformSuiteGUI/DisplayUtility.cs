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
            dgv.ReadOnly = true;
            dgv.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        public static void tooltip_graph_display(ToolTip t, MouseEventArgs e, Chart c, Point? p)
        {
            t.RemoveAll();
            Point pos = e.Location;
            if (p.HasValue && pos == p.Value) return;
            HitTestResult[] results = c.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);
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

        public static void GraphRelationsChart(Chart ct, List<ProteoformRelation> relations, string series)
        {
            ct.Series[series].XValueMember = "delta_mass";
            ct.Series[series].YValueMembers = "unadjusted_group_count";
            List<ProteoformRelation> et_relations_ordered = relations.OrderByDescending(r => r.delta_mass).ToList();
                ct.DataSource = et_relations_ordered;
                ct.DataBind();

                ct.ChartAreas[0].AxisY.StripLines.Clear();
                StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = Lollipop.min_peak_count };
                ct.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);

                ct.ChartAreas[0].AxisX.Title = "Delta m/z";
                ct.ChartAreas[0].AxisY.Title = "Nearby Count";
        }

        public static void GraphDeltaMassPeaks(Chart ct, List<DeltaMassPeak> peaks)
        {
            List<DeltaMassPeak> ee_peaks_ordered = peaks.OrderByDescending(r => r.group_adjusted_deltaM).ToList();
            ct.DataSource = ee_peaks_ordered;
            ct.DataBind();
            ct.Series["Peak Count"].XValueMember = "group_adjusted_deltaM";
            ct.Series["Peak Count"].YValueMembers = "group_count";
            ct.Series["Decoy Count"].XValueMember = "group_adjusted_deltaM";
            ct.Series["Decoy Count"].YValueMembers = "decoy_count";
            ct.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            if (ee_peaks_ordered.Count > 0) GraphSelectedDeltaMassPeak(ct, ee_peaks_ordered[0]);
        }

        public static void GraphSelectedDeltaMassPeak(Chart ct, DeltaMassPeak peak)
        {
            ct.ChartAreas[0].AxisY.StripLines.Clear();
            ct.ChartAreas[0].AxisX.Minimum = peak.group_adjusted_deltaM - Lollipop.peak_width_base;
            ct.ChartAreas[0].AxisX.Maximum = peak.group_adjusted_deltaM + Lollipop.peak_width_base;

            ct.ChartAreas[0].AxisX.StripLines.Clear();
            double stripline_tolerance = Lollipop.peak_width_base * 0.5;
            StripLine lowerPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = peak.group_adjusted_deltaM - stripline_tolerance };
            StripLine upperPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = peak.group_adjusted_deltaM + stripline_tolerance };
            ct.ChartAreas[0].AxisX.StripLines.Add(lowerPeakBound_stripline);
            ct.ChartAreas[0].AxisX.StripLines.Add(upperPeakBound_stripline);

            ct.ChartAreas[0].AxisX.Title = "Delta m/z";
            ct.ChartAreas[0].AxisY.Title = "Nearby Count";
        }
    }
}
