using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
namespace ProteoWPFSuite
{
    class DisplayUtility
    {
        #region Public Methods

        public static void FillDataGridView(DataGridView dgv, IEnumerable<object> someList)
        {
            SortableBindingList<object> sbl = new SortableBindingList<object>(someList);
            dgv.DataSource = sbl;
            dgv.ReadOnly = false;
            dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dgv.AllowUserToAddRows = false;
            dgv.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
            foreach (DataGridViewColumn col in dgv.Columns) col.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        public static void FillDataGridView(DataGridView dgv, DataTable some_table)
        {
            dgv.DataSource = some_table;
            dgv.ReadOnly = false;
            dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dgv.AllowUserToAddRows = false;
            dgv.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
            foreach (DataGridViewColumn col in dgv.Columns) col.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        public static void tooltip_graph_display(System.Windows.Forms.ToolTip t, System.Windows.Forms.MouseEventArgs e, Chart c, System.Drawing.Point? p)
        {
            t.RemoveAll();
            System.Drawing.Point pos = e.Location;
            if (p.HasValue && pos == p.Value) { return; }

            System.Windows.Forms.DataVisualization.Charting.HitTestResult[] results = new System.Windows.Forms.DataVisualization.Charting.HitTestResult[4];

            try
            {
                results = c.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);

                foreach (System.Windows.Forms.DataVisualization.Charting.HitTestResult result in results)
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
            { }
        }

        public static void GraphRelationsChart(Chart ct, List<ProteoformRelation> relations, string series, bool gaps_zeroed)
        {
            ct.Series[series].Points.Clear();
            ct.Series[series].XValueMember = "delta_mass";
            ct.Series[series].YValueMembers = "nearby_relations_count";
            List<ProteoformRelation> relations_ordered = relations.OrderByDescending(r => r.DeltaMass).ToList();
            for (int i = 0; i < relations_ordered.Count; i++)
            {
                ProteoformRelation relation = relations_ordered[i];
                ProteoformRelation next_relation = relations_ordered[i];
                ct.Series[series].Points.AddXY(relation.DeltaMass, relation.nearby_relations_count);
                if (gaps_zeroed)
                {
                    ct.Series[series].Points.AddXY(relation.DeltaMass + 1e-5, 1e-5);
                    ct.Series[series].Points.AddXY(relation.DeltaMass - 1e-5, 1e-5);
                }
            }
            ct.ChartAreas[0].AxisX.Title = "Delta Mass (Da)";
            ct.ChartAreas[0].AxisY.Title = "Nearby Count";
            ct.ChartAreas[0].AxisX.LabelStyle.Format = "#";
            ct.ChartAreas[0].AxisY.LabelStyle.Format = "#";
            ct.Series[series].Color = System.Drawing.Color.DodgerBlue;
        }

        public static void GraphDeltaMassPeaks(Chart ct, List<DeltaMassPeak> peaks, string peak_series, string decoy_series, List<ProteoformRelation> relations, string relations_series)
        {
            ct.Series[peak_series].Points.Clear();
            ct.Series[decoy_series].Points.Clear();
            ct.Series[relations_series].Points.Clear();

            //if no relations, return, don't crash
            if (relations.Count == 0) return;

            double peak_threshold;
            if (typeof(TheoreticalProteoform).IsAssignableFrom(relations[0].connected_proteoforms[1].GetType())) peak_threshold = Sweet.lollipop.min_peak_count_et;
            else peak_threshold = Sweet.lollipop.min_peak_count_ee;
            List<DeltaMassPeak> peaks_ordered = peaks.OrderBy(r => r.DeltaMass).ToList();
            foreach (DeltaMassPeak peak in peaks_ordered)
            {
                ct.Series[peak_series].Points.AddXY(peak.DeltaMass, peak.peak_relation_group_count);
                ct.Series[decoy_series].Points.AddXY(peak.DeltaMass, peak.decoy_relation_count);
            }

            List<ProteoformRelation> relations_ordered = relations.OrderBy(r => r.DeltaMass).ToList();
            foreach (ProteoformRelation relation in relations_ordered)
            {
                ct.Series[relations_series].Points.AddXY(relation.DeltaMass, relation.nearby_relations_count);
            }
            ct.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            if (peaks_ordered.Count > 0) GraphSelectedDeltaMassPeak(ct, peaks_ordered[0], relations);
        }

        public static void GraphSelectedDeltaMassPeak(Chart ct, DeltaMassPeak peak, List<ProteoformRelation> relations)
        {
            ct.ChartAreas[0].AxisY.StripLines.Clear();
            double peak_width_base;
            if (typeof(TheoreticalProteoform).IsAssignableFrom(relations[0].connected_proteoforms[1].GetType())) peak_width_base = Sweet.lollipop.peak_width_base_et;
            else peak_width_base = Sweet.lollipop.peak_width_base_ee;
            ct.ChartAreas[0].AxisX.Minimum = peak.DeltaMass - peak_width_base;
            ct.ChartAreas[0].AxisX.Maximum = peak.DeltaMass + peak_width_base;

            ct.ChartAreas[0].AxisX.StripLines.Clear();
            double stripline_tolerance = peak_width_base * 0.5;
            StripLine lowerPeakBound_stripline = new StripLine() { BorderColor = System.Drawing.Color.Red, IntervalOffset = peak.DeltaMass - stripline_tolerance };
            StripLine upperPeakBound_stripline = new StripLine() { BorderColor = System.Drawing.Color.Red, IntervalOffset = peak.DeltaMass + stripline_tolerance };
            ct.ChartAreas[0].AxisX.StripLines.Add(lowerPeakBound_stripline);
            ct.ChartAreas[0].AxisX.StripLines.Add(upperPeakBound_stripline);

            ct.ChartAreas[0].AxisY.Maximum = 1 + Math.Max(
                Convert.ToInt32(peak.peak_relation_group_count * 1.2),
                Convert.ToInt32(relations.Where(r => r.DeltaMass >= peak.DeltaMass - peak_width_base
                    && r.DeltaMass <= peak.DeltaMass + peak_width_base).Select(r => r.nearby_relations_count).Max())
            ); //this automatically scales the vertical axis to the peak height plus 20%, also accounting for the nearby trace of unadjusted relation group counts

            ct.ChartAreas[0].AxisX.Title = "Delta Mass (Da)";
            ct.ChartAreas[0].AxisY.Title = "Count";
        }

        public static object[] get_selected_objects(DataGridView dgv)
        {
            List<object> items = new List<object>();
            if (dgv.SelectedRows.Count > 0)
                for (int i = 0; i < dgv.SelectedRows.Count; i++)
                    items.Add(dgv.SelectedRows[i].DataBoundItem);
            else
            {
                List<int> rows = new List<int>();
                for (int i = 0; i < dgv.SelectedCells.Count; i++) rows.Add(dgv.SelectedCells[i].RowIndex);
                rows = rows.Distinct().ToList();
                foreach (int row_index in rows)
                    items.Add(dgv.Rows[row_index].DataBoundItem);
            }

            List<DisplayObject> display_objects = items.OfType<DisplayObject>().ToList();
            return display_objects.Count > 0 ?
                display_objects.Select(d => d.display_object).ToArray() :
                items.ToArray();
        }

        public static DataTable FormatTable(List<DisplayObject> display_objects, IEnumerable<Tuple<PropertyInfo, string, bool>> property1_header2_visible3, string table_name)
        {
            DataTable dt = new DataTable(table_name);
            if (display_objects == null || display_objects.Count <= 0)
                return dt;

            DisplayObject first = display_objects[0];
            foreach (var property_header_visible in property1_header2_visible3)
            {
                if (property_header_visible.Item3) dt.Columns.Add(property_header_visible.Item2 != null ? property_header_visible.Item2 : property_header_visible.Item1.Name);
            }

            foreach (DisplayObject de in display_objects)
            {
                DataRow new_row = dt.NewRow();
                int column_index = 0;
                foreach (var item in property1_header2_visible3)
                {
                    if (item.Item3)
                        new_row[column_index++] = item.Item1.GetValue(de) == null || item.Item1.GetValue(de).ToString() == "NaN" ? "" : item.Item1.GetValue(de);
                }
                dt.Rows.Add(new_row);
            }

            return dt;
        }

        public static bool CheckForProteinFastas(System.Windows.Controls.ComboBox cmb, IEnumerable<string> files)
        {
            if (Lollipop.file_filters[cmb.SelectedIndex].Contains(".fasta") && files.Any(x => x.Contains(".fasta")))
            {
                System.Windows.MessageBox.Show("Usage of protein fasta files is not yet enabled. Please use a protein XML file for now, e.g. from UniProt. (See this site for more information, and let us know there if this is an issue for you: https://github.com/smith-chem-wisc/ProteoformSuite/issues/477.)", "Load Files Message");
                return true;
            }
            return false;
        }

        #endregion Public Methods        
    }
}
