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
            dgv.AllowUserToAddRows = false;
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
            { }           
        }

        public static void EditInputFileDGVs(DataGridView dgv, Purpose purpose)
        {
            //if (purpose == Purpose.TopDown)
            //{
            //    DataGridViewComboBoxColumn cmCol = new DataGridViewComboBoxColumn();
            //    cmCol.HeaderText = "TD Program";
            //    cmCol.DataSource = Enum.GetValues(typeof(TDProgram));
            //    cmCol.ValueType = typeof(TDProgram);
            //    dgv.Columns.Add(cmCol);
            //}
        }


        public static void GraphRelationsChart(Chart ct, List<ProteoformRelation> relations, string series)
        {
            ct.Series[series].Points.Clear();
            ct.Series[series].XValueMember = "delta_mass";
            ct.Series[series].YValueMembers = "nearby_relations_count";
            List<ProteoformRelation> relations_ordered = relations.OrderByDescending(r => r.delta_mass).ToList();
            ct.DataSource = relations_ordered;
            ct.DataBind();
            ct.ChartAreas[0].AxisX.Title = "Delta Mass (Da)";
            ct.ChartAreas[0].AxisY.Title = "Nearby Count";
        }

        public static void GraphDeltaMassPeaks(Chart ct, List<DeltaMassPeak> peaks, string peak_series, string decoy_series, List<ProteoformRelation> relations, string relations_series)
        {
            ct.Series[peak_series].Points.Clear();
            ct.Series[decoy_series].Points.Clear();
            ct.Series[relations_series].Points.Clear();

            double peak_threshold;
            if (typeof(TheoreticalProteoform).IsAssignableFrom(relations[0].connected_proteoforms[1].GetType())) peak_threshold = Lollipop.min_peak_count_et;
            else peak_threshold = Lollipop.min_peak_count_ee;
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
            double peak_width_base;
            if (typeof(TheoreticalProteoform).IsAssignableFrom(relations[0].connected_proteoforms[1].GetType())) peak_width_base = Lollipop.peak_width_base_et;
            else peak_width_base = Lollipop.peak_width_base_ee;
            ct.ChartAreas[0].AxisX.Minimum = peak.peak_deltaM_average - peak_width_base;
            ct.ChartAreas[0].AxisX.Maximum = peak.peak_deltaM_average + peak_width_base;

            ct.ChartAreas[0].AxisX.StripLines.Clear();
            double stripline_tolerance = peak_width_base * 0.5;
            StripLine lowerPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = peak.peak_deltaM_average - stripline_tolerance };
            StripLine upperPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = peak.peak_deltaM_average + stripline_tolerance };
            ct.ChartAreas[0].AxisX.StripLines.Add(lowerPeakBound_stripline);
            ct.ChartAreas[0].AxisX.StripLines.Add(upperPeakBound_stripline);

            ct.ChartAreas[0].AxisY.Maximum = 1 + Math.Max(
                Convert.ToInt32(peak.peak_relation_group_count * 1.2),
                Convert.ToInt32(relations.Where(r => r.delta_mass >= peak.peak_deltaM_average - peak_width_base 
                    && r.delta_mass <= peak.peak_deltaM_average + peak_width_base).Select(r => r.nearby_relations_count).Max())
            ); //this automatically scales the vertical axis to the peak height plus 20%, also accounting for the nearby trace of unadjusted relation group counts

            ct.ChartAreas[0].AxisX.Title = "Delta Mass (Da)";
            ct.ChartAreas[0].AxisY.Title = "Count";
        }

        public static void FormatAggregatesTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

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
            if (dgv.Columns.Count <= 0) return;

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
            dgv.Columns["ptm_set"].Visible = false;
            dgv.Columns["family"].Visible = false;

            dgv.AllowUserToAddRows = false;
        }

        public static void FormatRelationsGridView(DataGridView dgv, bool mask_experimental, bool mask_theoretical)

        {
            if (dgv.Columns.Count <= 0) return;

            //round table values
            dgv.Columns["delta_mass"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["peak_center_deltaM"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["proteoform_mass_1"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["proteoform_mass_2"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["agg_intensity_1"].DefaultCellStyle.Format = "0.##";
            dgv.Columns["agg_intensity_2"].DefaultCellStyle.Format = "0.##";
            dgv.Columns["agg_RT_1"].DefaultCellStyle.Format = "0.##";
            dgv.Columns["agg_RT_2"].DefaultCellStyle.Format = "0.##";


            //set column header
            dgv.Columns["delta_mass"].HeaderText = "Delta Mass";
            dgv.Columns["nearby_relations_count"].HeaderText = "Nearby Relation Count";
            dgv.Columns["accepted"].HeaderText = "Accepted";
            dgv.Columns["peak_center_deltaM"].HeaderText = "Peak Center Delta Mass";
            dgv.Columns["peak_center_count"].HeaderText = "Peak Center Count";
            dgv.Columns["lysine_count"].HeaderText = "Lysine Count";
            dgv.Columns["outside_no_mans_land"].HeaderText = "Outside No Man's Land";

            //ET formatting
            dgv.Columns["fragment"].HeaderText = "Fragment";
            dgv.Columns["ptm_list"].HeaderText = "PTM Description";
            dgv.Columns["name"].HeaderText = "Name";
            if (mask_experimental)
            {
                dgv.Columns["num_observations_1"].HeaderText = "Number Experimental Observations";
                dgv.Columns["accession_1"].HeaderText = "Experimental Accession";
                dgv.Columns["accession_2"].HeaderText = "Theoretical Accession";
                dgv.Columns["proteoform_mass_2"].HeaderText = "Theoretical Proteoform Mass";
                dgv.Columns["proteoform_mass_1"].HeaderText = "Experimental Aggregated Proteoform Mass";
                dgv.Columns["agg_intensity_1"].HeaderText = "Experimental Aggregated Intensity";
                dgv.Columns["agg_RT_1"].HeaderText = "Experimental Aggregated RT";
                dgv.Columns["agg_intensity_2"].Visible = false;
                dgv.Columns["agg_RT_2"].Visible = false;
                dgv.Columns["num_observations_2"].Visible = false;
                dgv.Columns["relation_type_string"].Visible = false;
            }

            //EE formatting
            if (mask_theoretical)
            {
                dgv.Columns["num_observations_2"].HeaderText = "Number Light Experimental Observations";
                dgv.Columns["num_observations_1"].HeaderText = "Number Heavy Experimental Observations";
                dgv.Columns["agg_intensity_2"].HeaderText = "Light Experimental Aggregated Intensity";
                dgv.Columns["agg_intensity_1"].HeaderText = "Heavy Experimental Aggregated Intensity";
                dgv.Columns["agg_RT_1"].HeaderText = "Aggregated RT Heavy";
                dgv.Columns["agg_RT_2"].HeaderText = "Aggregated RT Light";
                dgv.Columns["accession_1"].HeaderText = "Heavy Experimental Accession";
                dgv.Columns["accession_2"].HeaderText = "Light Experimental Accession";
                dgv.Columns["proteoform_mass_1"].HeaderText = "Heavy Experimental Aggregated Mass";
                dgv.Columns["proteoform_mass_2"].HeaderText = "Light Experimental Aggregated Mass";
                dgv.Columns["name"].Visible = false;
                dgv.Columns["fragment"].Visible = false;
            }

            //ProteoformFamilies formatting
            dgv.Columns["relation_type_string"].HeaderText = "Relation Type";

            //making these columns invisible
            dgv.Columns["peak"].Visible = false;
            if (!Lollipop.neucode_labeled) { dgv.Columns["lysine_count"].Visible = false; }

            dgv.AllowUserToAddRows = false;
        }

        public static void FormatPeakListGridView(DataGridView dgv, bool mask_mass_shifter)
        {
            if (dgv.Columns.Count <= 0) return;

            //making all columns invisible first - faster
            foreach (DataGridViewColumn column in dgv.Columns) { column.Visible = false; }
            if (!mask_mass_shifter)
            {
                dgv.Columns["mass_shifter"].Visible = true;
                dgv.Columns["mass_shifter"].ReadOnly = false; //user can say how much they want to change monoisotopic by for each
                dgv.Columns["possiblePeakAssignments_string"].Visible = true;
            }
            dgv.Columns["peak_deltaM_average"].DefaultCellStyle.Format = "0.####";
            dgv.Columns["peak_group_fdr"].DefaultCellStyle.Format = "0.##";

            dgv.Columns["peak_relation_group_count"].HeaderText = "Peak Center Count";
            dgv.Columns["decoy_relation_count"].HeaderText = "Decoy Count under Peak";
            dgv.Columns["peak_deltaM_average"].HeaderText = "Peak Center Delta Mass";
            dgv.Columns["peak_group_fdr"].HeaderText = "Peak FDR";
            dgv.Columns["peak_accepted"].HeaderText = "Peak Accepted";
            dgv.Columns["possiblePeakAssignments_string"].HeaderText = "Peak Assignment";

            dgv.Columns["peak_relation_group_count"].Visible = true;
            //dgv.Columns["decoy_relation_count"].Visible = true;
            dgv.Columns["peak_deltaM_average"].Visible = true;
            dgv.Columns["peak_group_fdr"].Visible = true;
            dgv.Columns["peak_accepted"].Visible = true;
            dgv.AllowUserToAddRows = false;
        }

        public static void formatDataFileInputGridView(DataGridView dgv, IEnumerable<object> someObject)
        {
            SortableBindingList<object> sbl = new SortableBindingList<object>(someObject);
            
            dgv.ReadOnly = false;
            dgv.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;



            //making all columns invisible first - faster
            //foreach (DataGridViewColumn column in dgv.Columns) { column.Visible = false; }

            //dgv.Columns["matchingCalibrationFile"].HeaderText = "Calibrate";
            //dgv.Columns["filename"].HeaderText = "Filename";
            //dgv.Columns["label"].HeaderText = "NeuCode/Unlabelled";
            //dgv.Columns["sampleCategory.biorep"].HeaderText = "Biorep";
            //dgv.Columns["sampleCategory.techrep"].HeaderText = "Techrep";
            //dgv.Columns["sampleCategory.condition"].HeaderText = "Condition";

            dgv.DataSource = sbl;
        }

        public static object[] get_selected_objects(DataGridView dgv)
        {
            //Check if there are any rows selected
            //int selected_row_sum = 0;
            //for (int i = 0; i < dgv.SelectedCells.Count; i++) selected_row_sum += dgv.SelectedCells[i].RowIndex;

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
            //else
            //    for (int i = 0; i < dgv.SelectedCells.Count; i++)
            //        if (dgv.SelectedCells[i].RowIndex != 0)
            //            items.Add(dgv.Rows[dgv.SelectedCells[i].RowIndex].DataBoundItem);

            return items.ToArray();
        }

        public static void format_families_dgv(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            //set column header
            //dgv_proteoform_families.Columns["family_id"].HeaderText = "Light Monoisotopic Mass";
            dgv.Columns["lysine_count"].HeaderText = "Lysine Count";
            dgv.Columns["experimental_count"].HeaderText = "Experimental Proteoforms";
            dgv.Columns["theoretical_count"].HeaderText = "Theoretical Proteoforms";
            dgv.Columns["relation_count"].HeaderText = "Relation Count";
            dgv.Columns["accession_list"].HeaderText = "Theoretical Accessions";
            dgv.Columns["name_list"].HeaderText = "Theoretical Names";
            dgv.Columns["experimentals_list"].HeaderText = "Experimental Accessions";
            dgv.Columns["agg_mass_list"].HeaderText = "Experimental Aggregated Masses";
            dgv.Columns["relations"].Visible = false;
            dgv.Columns["proteoforms"].Visible = false;
        }
    }
}
