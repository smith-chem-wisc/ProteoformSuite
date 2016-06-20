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
    public partial class DecoyDecoyComparison : Form
    {
        DataTable ddList;
        Boolean formLoadEvent;

        public DecoyDecoyComparison()
        {
            InitializeComponent();
            this.dgv_DD_Peak_List.MouseClick += new MouseEventHandler(dgv_DD_Peak_List_CellClick);

        }

        public void DecoyDecoyComparison_Load(object sender, EventArgs e)
        {
            if (GlobalData.decoyDecoyPairs.Tables.Count == 0)
            {
                run_comparison();

            }
            GraphDDHistogram();
            FillDDPairsGridView("DecoyDatabase_0");
            FillDecoyDatabaseGridView("DecoyDatabase_0");
            FillDDListTable();
            GraphEEPeakList();
            GraphDDList();

       }
    

        public void run_comparison()
        {
            this.Cursor = Cursors.WaitCursor;
            formLoadEvent = true;
            ddList = new DataTable();
            InitializeParameterSet();
            createDD_Databases();
            FindAllDDPairs();
            CalculateRunningSums();
            InitializeDDListTable();
            formLoadEvent = false;
          //  UpdateFiguresOfMerit();
            this.Cursor = Cursors.Default;

        }


        private void InitializeDDListTable()
        {
            ddList.Columns.Add("Delta Mass", typeof(double));
            ddList.Columns.Add("EE Peak Count", typeof(int)); //comparing EE peaks to DD counts for each delta mass 
            ddList.Columns.Add("DD count", typeof(int));
        }

        public void createDD_Databases()
        {
            Random randGen = new Random();

            for (int i = 0; i < GlobalData.numDecoyDatabases; i++)
            {
                string tableName = "DecoyDatabase_" + i;
                DataTable dt = GenerateDecoyDataTable(tableName);
                DataTable decoyDatabase = new DataTable();
                decoyDatabase  = GlobalData.theoreticalAndDecoyDatabases.Tables[tableName].Copy();

                foreach (DataRow agRow in GlobalData.aggregatedProteoforms.Rows)
                {
                    int kCount = Convert.ToInt32(agRow["Lysine Count"]);
                    double agMass = Convert.ToDouble(agRow["Aggregated Mass"]);

                    string expression = "[Lysine Count] = " + kCount;
                    DataRow[] decoysWithSameKCount = decoyDatabase.Select(expression);

                    ////chooses random decoy with same K count. 
                    //int index = randGen.Next(decoysWithSameKCount.Length);
                    //dt.ImportRow(decoysWithSameKCount[index]);
                    //string accessionNumber = decoysWithSameKCount[index]["Accession"].ToString();


                    //chooses decoy that's closest to mass of aggregate proteoform
                    int closestMassProteoform = 0;
                   
                    for (int m = 0; m < decoysWithSameKCount.Length; m++)
                    {
                        double massDiff1 = Math.Abs(agMass - Convert.ToDouble(decoysWithSameKCount[m]["Proteoform Mass"]));
                        double massDiffCurrentClosest = Math.Abs(agMass - Convert.ToDouble(decoysWithSameKCount[closestMassProteoform]["Proteoform Mass"]));

                        if (massDiff1 < massDiffCurrentClosest)
                        {
                            closestMassProteoform = m;
                        }
                    }
                    string accessionNumber = decoysWithSameKCount[closestMassProteoform]["Accession"].ToString();
                    dt.ImportRow(decoysWithSameKCount[closestMassProteoform]);

                    foreach (DataRow row in decoysWithSameKCount)
                    {
                        if (row["Accession"].ToString().Equals(accessionNumber))
                        {
                            row.Delete();
                        }
                    }

                }

                    GlobalData.decoyDecoyDatabases.Tables.Add(dt);
            }
        }

        


        static DataTable GenerateDecoyDataTable(string title)
        {
            DataTable dt = new DataTable(title);//datatable name goes in parentheses.
            dt.Columns.Add("Accession", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Fragment", typeof(string));
            dt.Columns.Add("Begin", typeof(int));
            dt.Columns.Add("End", typeof(int));
            dt.Columns.Add("Mass", typeof(double));
            dt.Columns.Add("Lysine Count", typeof(int));
            dt.Columns.Add("PTM List", typeof(string));
            dt.Columns.Add("PTM Group Mass", typeof(double));
            dt.Columns.Add("Proteoform Mass", typeof(double));
            return dt;
        }



        private void GraphDDHistogram()
        {
            int i = (int)nud_Decoy_Database.Value - 1;
            string tableName = "DecoyDatabase_" + i;
            string colName = "Delta Mass";
            string direction = "DESC";
            DataTable dt = GlobalData.decoyDecoyPairs.Tables[tableName];

            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();


            ct_DD_Histogram.Series["ddHistogram"].XValueMember = "Delta Mass";
            ct_DD_Histogram.Series["ddHistogram"].YValueMembers = "Running Sum";

            ct_DD_Histogram.DataSource = dt;
            ct_DD_Histogram.DataBind();

            ct_DD_Histogram.Series["ddHistogram"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";

        }

        private void GraphEEPeakList()
        {
            string colNameEE = "Delta Mass";
            string directionEE = "DESC";
            DataTable ee = GlobalData.experimentExperimentPairs;
            ee.DefaultView.Sort = colNameEE + " " + directionEE;
            ee = ee.DefaultView.ToTable();
            GlobalData.experimentExperimentPairs = ee;

            foreach (DataRow row in GlobalData.experimentExperimentPairs.Rows)
            {
                ct_DD_Peak_list.Series["eePeakList"].Points.AddXY(row["Delta Mass"], row["Running Sum"]);
            }
        }

        private void GraphDDList()
        {
            int i = (int)nud_Decoy_Database.Value - 1;
            string colNameDD = "Delta Mass";
            string directionDD = "DESC";
            string tableName = "DecoyDatabase_" + i;

            DataTable dd = GlobalData.decoyDecoyPairs.Tables[tableName];
            dd.DefaultView.Sort = colNameDD + " " + directionDD;
            dd = dd.DefaultView.ToTable();

            foreach (DataRow row in dd.Rows)
            {
                ct_DD_Peak_list.Series["ddPeakList"].Points.AddXY(row["Delta Mass"], row["Running Sum"]);
            }

            ct_DD_Peak_list.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            ct_DD_Peak_list.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(dgv_DD_Peak_List.Rows[0].Cells["Delta Mass"].Value.ToString()) - Convert.ToDouble(nUD_PeakWidthBase.Value);
            ct_DD_Peak_list.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(dgv_DD_Peak_List.Rows[0].Cells["Delta Mass"].Value.ToString()) + Convert.ToDouble(nUD_PeakWidthBase.Value);
            ct_DD_Peak_list.Series["ddPeakList"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
            ct_DD_Peak_list.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_DD_Peak_List.Rows[0].Cells["Delta Mass"].Value.ToString()) + 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });

            ct_DD_Peak_list.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_DD_Peak_List.Rows[0].Cells["Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });
        }

        private void dgv_DD_Peak_List_CellClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                int clickedRow = dgv_DD_Peak_List.HitTest(e.X, e.Y).RowIndex;
                if (clickedRow >= 0 && clickedRow < GlobalData.ddList.Rows.Count)
                {
                    DDListGraphParameters(clickedRow);
                }
            }
        }

        private void DDListGraphParameters(int clickedRow)
        {
            ct_DD_Peak_list.ChartAreas[0].AxisX.StripLines.Clear();
            double graphMax = Convert.ToDouble(dgv_DD_Peak_List.Rows[clickedRow].Cells["Delta Mass"].Value.ToString()) + Convert.ToDouble((nUD_PeakWidthBase.Value));
            double graphMin = Convert.ToDouble(dgv_DD_Peak_List.Rows[clickedRow].Cells["Delta Mass"].Value.ToString()) - Convert.ToDouble((nUD_PeakWidthBase.Value));

            if (graphMin < graphMax)
            {
                ct_DD_Peak_list.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(graphMin);
                ct_DD_Peak_list.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(graphMax);
            }

            ct_DD_Peak_list.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_DD_Peak_List.Rows[clickedRow].Cells["Delta Mass"].Value.ToString()) + 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });

            ct_DD_Peak_list.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_DD_Peak_List.Rows[clickedRow].Cells["Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });
        }



        private void FillDDPairsGridView(string table)
        {

            DataTable displayTable = GlobalData.decoyDecoyPairs.Tables[table];

            dgv_DD_Pairs.DataSource = displayTable;
            dgv_DD_Pairs.ReadOnly = true;
            //dgv_ED_Pairs.Columns["Acceptable Peak"].ReadOnly = false;
            dgv_DD_Pairs.Columns["Mass Light"].DefaultCellStyle.Format = "0.#####";
            dgv_DD_Pairs.Columns["Mass Heavy"].DefaultCellStyle.Format = "0.#####";
            dgv_DD_Pairs.Columns["Lysine Count"].DefaultCellStyle.Format = "0.#####";
            dgv_DD_Pairs.Columns["Delta Mass"].DefaultCellStyle.Format = "0.#####";
            //dgv_ED_Pairs.Columns["Peak Center Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_DD_Pairs.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_DD_Pairs.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private void FillDecoyDatabaseGridView(string table)
        {
            DataTable displayTable = GlobalData.decoyDecoyDatabases.Tables[table];

            dgv_decoy_database.DataSource = displayTable;
            dgv_decoy_database.ReadOnly = true;
            //dgv_ED_Pairs.Columns["Acceptable Peak"].ReadOnly = false;
            dgv_decoy_database.Columns["Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_decoy_database.Columns["PTM Group Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_decoy_database.Columns["Proteoform Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_decoy_database.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_decoy_database.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }


        private DataTable CreateDDPairsDataTable(string tableName)
        {
            DataTable dt = new DataTable(tableName);
            dt.Columns.Add("Mass Light", typeof(double));
            dt.Columns.Add("Mass Heavy", typeof(double));
           // dt.Columns.Add("Aggregated Intensity Light", typeof(double));
           // dt.Columns.Add("Aggregated Intensity Heavy", typeof(double));
           // dt.Columns.Add("Retention Time Light", typeof(double));
            //dt.Columns.Add("Retention Time Heavy", typeof(double));
            dt.Columns.Add("Lysine Count", typeof(int));
            //dt.Columns.Add("Number of Observations Light", typeof(int));
            //dt.Columns.Add("Number of Observations Heavy", typeof(int));
            dt.Columns.Add("Delta Mass", typeof(double));
            dt.Columns.Add("Running Sum", typeof(int));
            dt.Columns.Add("Decoy Hits Count");
           // dt.Columns.Add("Peak Center Count", typeof(int));
            dt.Columns.Add("Out of Range Decimal", typeof(bool));  
            return dt;
        }


        private void FindAllDDPairs()
        {
            for (int i = 0; i < GlobalData.numDecoyDatabases; i++)
            {
                string tableName = "DecoyDatabase_" + i;
                DataTable dt = GlobalData.decoyDecoyDatabases.Tables[tableName];
                DataTable ddPairs = CreateDDPairsDataTable(tableName);

                int numRows = dt.Rows.Count;

                for (int index1 = 0; index1 < numRows; index1++)
                {
                    for (int index2 = 0; index2 < numRows; index2++)
                    {

                        double massLight = Convert.ToDouble(dt.Rows[index1]["Proteoform Mass"]);
                        double massHeavy = Convert.ToDouble(dt.Rows[index2]["Proteoform Mass"]);
                        int lysineLight = Convert.ToInt16(dt.Rows[index1]["Lysine Count"]);
                        int lysineHeavy = Convert.ToInt16(dt.Rows[index2]["Lysine Count"]);

                        if (massHeavy > massLight)
                        {
                            if (lysineLight == lysineHeavy)
                            {
                                double deltaMass = massHeavy - massLight;

                                if (deltaMass < Convert.ToDouble(nUD_DD_Upper_Bound.Value))
                                {
                                    double afterDecimal = Math.Abs(deltaMass - Math.Truncate(deltaMass));
                                    bool oOR = true;
                                    if (afterDecimal <= Convert.ToDouble(nUD_NoManLower.Value) ||
                                        afterDecimal >= Convert.ToDouble(nUD_NoManUpper.Value))
                                    {
                                        oOR = false;
                                    }

                                    ddPairs.Rows.Add(massLight, massHeavy, lysineLight, deltaMass, 0, 0, oOR);
                                }
                            }
                        }
                    }
                }

                GlobalData.decoyDecoyPairs.Tables.Add(ddPairs);
            }
        }

        private void CalculateRunningSums()
        {
            for (int i = 0; i < GlobalData.numDecoyDatabases; i++)
            {
                string tableName = "DecoyDatabase_" + i;
                foreach (DataRow row in GlobalData.decoyDecoyPairs.Tables[tableName].Rows)
                {
                    double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                    double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    string expression = "[Delta Mass] >= " + lower + "and [Delta Mass] <= " + upper;
                    row["Running Sum"] = GlobalData.decoyDecoyPairs.Tables[tableName].Select(expression).Length;
                }
            }

        }

        private void FillDDListTable()
        {

            foreach (DataRow row in GlobalData.eePeakList.Rows)
            {
                DataTable decoyTotals = new DataTable();
                decoyTotals.Columns.Add("Decoy Hits", typeof(int));

                double deltaMass = Convert.ToDouble(row["Average Delta Mass"].ToString());
                int peakCount = Convert.ToInt16(row["Peak Count"].ToString());

                double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;

                string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper;

                for (int i = 0; i < GlobalData.numDecoyDatabases; i++)
                {
                    string colName = "Running Sum";
                    string direction = "DESC";
                    string tableName = "DecoyDatabase_" + i;
                    DataTable dt = new DataTable(tableName);
                    dt = GlobalData.decoyDecoyPairs.Tables[tableName];
                    dt.DefaultView.Sort = colName + " " + direction;
                    dt = dt.DefaultView.ToTable();

                    //if (Convert.ToBoolean(row["Out of Range Decimal"].ToString()) == false && Convert.ToBoolean(row["Acceptable Peak"].ToString()) == false)

                    DataRow[] decoyHits = dt.Select(expression);
                    decoyTotals.Rows.Add(decoyHits.Length);

                    foreach (DataRow rutrow in decoyHits)
                    {
                        rutrow["Decoy Hits Count"] = decoyHits.Length;

                    }

                    GlobalData.decoyDecoyPairs.Tables.Remove(tableName);
                    GlobalData.decoyDecoyPairs.Tables.Add(dt);
                }


                //calculate average of decoy hits for given protein
                int sum = 0;
                for (int i = 0; i < decoyTotals.Rows.Count; i++)
                {
                    sum = sum + Convert.ToInt16(decoyTotals.Rows[i]["Decoy Hits"]);
                }

                decimal average = sum / (decoyTotals.Rows.Count);
                ddList.Rows.Add(deltaMass, peakCount, average);

                //calculate median of decoy hits for given protein... not sure which we want to do.
                //string colName2 = "Decoy Hits";
                //decoyTotals.DefaultView.Sort = colName2 + " " + "ASC";
                //decoyTotals = decoyTotals.DefaultView.ToTable(); 
                //int indexMedian = (decoyTotals.Rows.Count)/ 2;
                //int median = Convert.ToInt16(decoyTotals.Rows[indexMedian][0]);
                //edList.Rows.Add(deltaMass, peakCount, median);
            }

            GlobalData.ddList = ddList;

            //Round before displaying ED peak list
            dgv_DD_Peak_List.DataSource = ddList;
            dgv_DD_Peak_List.ReadOnly = true;
            dgv_DD_Peak_List.Columns["Delta Mass"].DefaultCellStyle.Format = "0.####";
            dgv_DD_Peak_List.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_DD_Peak_List.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private void InitializeParameterSet()
        {
            nUD_DD_Upper_Bound.Minimum = 0;
            nUD_DD_Upper_Bound.Maximum = 500;
            //nUD_EE_Upper_Bound.Value = 500; // maximum mass difference in Da allowed between experimental pairs

            yMaxDD.Minimum = 0;
            yMaxDD.Maximum = 1000;
            yMaxDD.Value = 100; // scaling for y-axis maximum in the histogram of all EE pairs

            yMinDD.Minimum = -100;
            yMinDD.Maximum = yMaxDD.Maximum;
            yMinDD.Value = 0; // scaling for y-axis minimum in the histogram of all EE pairs

            xMaxDD.Minimum = xMinDD.Value;
            xMaxDD.Maximum = nUD_DD_Upper_Bound.Value;
            xMaxDD.Value = nUD_DD_Upper_Bound.Value; // scaling for x-axis maximum in the histogram of all EE pairs

            xMinDD.Minimum = 0;
            xMinDD.Maximum = xMaxDD.Value;
            xMinDD.Value = 0; // scaling for x-axis minimum in the histogram of all EE pairs

            nUD_NoManLower.Minimum = 00m;
            nUD_NoManLower.Maximum = 0.49m;
            //  nUD_NoManLower.Value = 0.22m;

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            //   nUD_NoManUpper.Value = 0.88m;

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            //   nUD_PeakWidthBase.Value = 0.0150m;

            nud_Decoy_Database.Minimum = 1;
            nud_Decoy_Database.Maximum = GlobalData.numDecoyDatabases;
            nud_Decoy_Database.Value = 1;

        }

       
        private void xMaxDD_ValueChanged(object sender, EventArgs e) // scaling for x-axis maximum in the histogram of all EE pairs
        {
            if (!formLoadEvent)
            {
                double newXMaxEE = double.Parse(xMaxDD.Value.ToString());
                if (newXMaxEE > double.Parse(xMinDD.Value.ToString()))
                {
                    ct_DD_Histogram.ChartAreas[0].AxisX.Maximum = newXMaxEE;
                }
            }
        }

        private void yMaxDD_ValueChanged(object sender, EventArgs e) // scaling for y-axis maximum in the histogram of all EE pairs
        {
            if (!formLoadEvent)
            {
                double newYMaxDD = double.Parse(yMaxDD.Value.ToString());
                if (newYMaxDD > double.Parse(yMinDD.Value.ToString()))
                {
                    ct_DD_Histogram.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxDD.Value.ToString());
                }
            }
        }

        private void yMinDD_ValueChanged(object sender, EventArgs e) // scaling for y-axis minimum in the histogram of all EE pairs
        {
            if (!formLoadEvent)
            {
                double newYMinDD = double.Parse(yMinDD.Value.ToString());
                if (newYMinDD < double.Parse(yMaxDD.Value.ToString()))
                {
                    ct_DD_Histogram.ChartAreas[0].AxisY.Minimum = double.Parse(yMinDD.Value.ToString());
                }
            }
        }

        private void xMinDD_ValueChanged(object sender, EventArgs e) // scaling for x-axis maximum in the histogram of all EE pairs
        {
            if (!formLoadEvent)
            {
                double newXMinDD = double.Parse(xMinDD.Value.ToString());
                if (newXMinDD < double.Parse(xMaxDD.Value.ToString()))
                {
                    ct_DD_Histogram.ChartAreas[0].AxisX.Minimum = newXMinDD;
                }
            }
        }


        public override string ToString()
        {
            return String.Join(System.Environment.NewLine, new string[] {
                "DecoyDecoyComparison|nUD_NoManLower.Value\t" + nUD_NoManLower.Value.ToString(),
                "DecoyDecoyComparison|nUD_NoManUpper.Value\t" + nUD_NoManUpper.Value.ToString(),
                "DecoyDecoyComparison|nUD_PeakWidthBase.Value\t" + nUD_PeakWidthBase.Value.ToString(),
               
            });
        }

        public void loadSetting(string setting_specs)
        {
            string[] fields = setting_specs.Split('\t');
            switch (fields[0].Split('|')[1])
            {
                case "nUD_NoManLower.Value":
                    nUD_NoManLower.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nUD_NoManUpper.Value":
                    nUD_NoManUpper.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nUD_PeakWidthBase.Value":
                    nUD_PeakWidthBase.Value = Convert.ToDecimal(fields[1]);
                    break;
            }
        }

        private void nud_Decoy_Database_ValueChanged(object sender, EventArgs e)
        {
            int i = (int)nud_Decoy_Database.Value - 1;
            string table = "DecoyDatabase_" + i;
            FillDDPairsGridView(table);
            GraphDDHistogram();
            FillDecoyDatabaseGridView(table);
        }
    }
}
