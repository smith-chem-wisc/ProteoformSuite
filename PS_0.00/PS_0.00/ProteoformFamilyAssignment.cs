using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace PS_0._00
{
    public partial class ProteoformFamilyAssignment : Form
    {
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private DataGridView dataGridView1;
        private DataGridView dataGridView2;
        public static DataTable ET_Groups = new DataTable(); 
        public static DataTable EE_Groups = new DataTable(); 
        public static DataTable CompiledEdges = new DataTable();
        public static int PF_Group_Num = 0;
        //public static double parentmass=0;
        //public static int EE_Checkpoint = 0;
        private SplitContainer splitContainer3;
        private DataGridView dataGridView3;
        DataRow[] foundRowsSingle;
        private Button button1;
        private Button button2;
        private TextBox textBox1;
        private Label label1;
        public static string filename = "";
        public static string csv = "";
        public static DataTable ExportDataTable = new DataTable();
        public static string folderPath = "";
        SaveFileDialog PFAFileSave = new SaveFileDialog();
        //public static DataTable Index = new DataTable();
        //public static DataTable Node = new DataTable();
        public static List<List<double>> nodeMasterList = new List<List<double>>();
        public static List<List<int>> indexMasterList = new List<List<int>>();


        public ProteoformFamilyAssignment()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.dataGridView3 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(1063, 456);
            this.splitContainer1.SplitterDistance = 519;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dataGridView2);
            this.splitContainer2.Size = new System.Drawing.Size(519, 456);
            this.splitContainer2.SplitterDistance = 230;
            this.splitContainer2.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(519, 230);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick_1);
            // 
            // dataGridView2
            // 
            this.dataGridView2.AllowUserToAddRows = false;
            this.dataGridView2.AllowUserToDeleteRows = false;
            this.dataGridView2.AllowUserToOrderColumns = true;
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView2.Location = new System.Drawing.Point(0, 0);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.RowTemplate.Height = 24;
            this.dataGridView2.Size = new System.Drawing.Size(519, 222);
            this.dataGridView2.TabIndex = 0;
            this.dataGridView2.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView2_CellContentClick_1);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.label1);
            this.splitContainer3.Panel1.Controls.Add(this.textBox1);
            this.splitContainer3.Panel1.Controls.Add(this.button2);
            this.splitContainer3.Panel1.Controls.Add(this.button1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.dataGridView3);
            this.splitContainer3.Size = new System.Drawing.Size(540, 456);
            this.splitContainer3.SplitterDistance = 230;
            this.splitContainer3.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 17);
            this.label1.TabIndex = 4;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(170, 35);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(287, 22);
            this.textBox1.TabIndex = 2;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(19, 77);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(127, 33);
            this.button2.TabIndex = 1;
            this.button2.Text = "Export CSV\r\n\r\n";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(19, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(127, 49);
            this.button1.TabIndex = 0;
            this.button1.Text = "Select Export Path:\r\n";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // dataGridView3
            // 
            this.dataGridView3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView3.Location = new System.Drawing.Point(0, 0);
            this.dataGridView3.Name = "dataGridView3";
            this.dataGridView3.RowTemplate.Height = 24;
            this.dataGridView3.Size = new System.Drawing.Size(540, 222);
            this.dataGridView3.TabIndex = 0;
            this.dataGridView3.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView3_CellContentClick_1);
            // 
            // ProteoformFamilyAssignment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1063, 456);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ProteoformFamilyAssignment";
            this.Load += new System.EventHandler(this.ProteoformFamilyAssignment_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).EndInit();
            this.ResumeLayout(false);

        }

        public void ProteoformFamilyAssignment_Load(object sender, EventArgs e) //called in initializecomponents
        {
            if (ET_Groups.Columns.Count == 0)
            {
                AssignColumns();
            }
            assign_families();
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.DarkGray;
            dataGridView2.RowsDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView2.AlternatingRowsDefaultCellStyle.BackColor = Color.DarkGray;
            dataGridView3.RowsDefaultCellStyle.BackColor = Color.LightGray;
            dataGridView3.AlternatingRowsDefaultCellStyle.BackColor = Color.DarkGray;
            dataGridView1.DataSource = GlobalData.ProteoformFamilyMetrics;
            ExportProteoformFamilies();
        }

        public void assign_families()
        {
            for (int q = 2; q < 27; q++)
            {
                SingleLysineIteration(q);
            }
        }    

        private void SingleLysineIteration(int q) //called in initializecomponents
        {
            Group_ET(q);
            Group_EE(q);
            CompileEdges();
            if (Convert.ToInt32(CompiledEdges.Rows.Count).Equals(0) == false) //if nothing with that lysine count, then ignore it. Only 25 iterations
            {
                FirstNodeGroupingIteration();
                FinalNodeGroupingIterations();
                FillProteoformFamiliesTables(q);

                //double NewNode = Convert.ToDouble(CompiledEdges.Rows[i]["Mass Heavy"]); //define heavy mass
                //                                                                        // if (Node[parentIndex].Contains(potentialNewNode)==false) //if heavy mass exists
                //                                                                        // {
                //nodeMasterList[masterIndex].Add(NewNode);
                // }
                //first iteration of sorting
                //List<List<int>> Index = new List<List<int>>();

                //  int numberOfRowsOld = 0;
                //int numberOfRowsNew = 1;
                //while(numberOfRowsOld!=numberOfRowsNew) //stop iteration when completed
                //{
                // List<int> newIndexEntry= new List<int> { 0 };



                //catch if unassigned edges
                //ConstructFamilies(q);
            }
        }

        private void FirstNodeGroupingIteration() //is this necessary?
        {
            indexMasterList.Clear();
            nodeMasterList.Clear();
            List<double> newNodeList = new List<double> { Convert.ToDouble(CompiledEdges.Rows[CompiledEdges.Rows.Count - 1]["Mass Light"]), Convert.ToDouble(CompiledEdges.Rows[CompiledEdges.Rows.Count - 1]["Mass Heavy"]) };
            List<int> newIndexList = new List<int> { CompiledEdges.Rows.Count - 1 };
            nodeMasterList.Add(newNodeList);
            indexMasterList.Add(newIndexList);
            int ListIndex = 0; //(i+1) would work?
            for (int i = CompiledEdges.Rows.Count-2; i >= 0; i--)
            {
                if (CompiledEdges.Rows[i]["Mass Light"].Equals(CompiledEdges.Rows[i + 1]["Mass Light"])) //if same Mass Light as previous edge
                {
                    double NewNode = Convert.ToDouble(CompiledEdges.Rows[i]["Mass Heavy"]);
                    int NewIndex = i;
                    nodeMasterList[ListIndex].Add(NewNode);
                    indexMasterList[ListIndex].Add(NewIndex);
                    CompiledEdges.Rows[i].Delete();
                }
                else
                {
                    newNodeList = new List<double> { Convert.ToDouble(CompiledEdges.Rows[i]["Mass Light"]), Convert.ToDouble(CompiledEdges.Rows[i]["Mass Heavy"]) };
                    newIndexList = new List<int> { i };
                    nodeMasterList.Add(newNodeList);
                    indexMasterList.Add(newIndexList);
                    ListIndex++;
                }
            }
        }

        private void FinalNodeGroupingIterations()
        {
            for (int i = 0; i < nodeMasterList.Count; i++)//foreach sourcelist in masterlist
            {
                for (int j = 1; j < nodeMasterList[i].Count; j++)//foreach node in sourcelist (already searched [0] in FirstNodeGroupingIteration)
                {
                    for (int k = i + 1; k < nodeMasterList.Count; k++)//foreach targetlist in masterlist
                    {
                        if (nodeMasterList[k].Contains(nodeMasterList[i][j])) //if targetlist contains node from sourcelist
                        {
                            for (int l = 0; l < nodeMasterList[k].Count; l++)//for each node in targetlist
                            {
                                if (nodeMasterList[i].Contains(nodeMasterList[k][l]) == false)// if sourcelist does not contain a targetlist node
                                {
                                    nodeMasterList[i].Add(nodeMasterList[k][l]); //add new targetlist node to sourcelist
                                }
                            }
                            for (int l = 0; l < indexMasterList[k].Count; l++)
                            {
                                indexMasterList[i].Add(indexMasterList[k][l]);
                            }
                            nodeMasterList.Remove(nodeMasterList[k]);//contents have been merged to sourcelist, targetlist must be removed to prevent replicates
                            indexMasterList.Remove(indexMasterList[k]);//contents have been merged to sourcelist, targetlist must be removed to prevent replicates
                            k--; //removing row shifts other lists over. Without this, lists will be skipped
                        }
                    }
                }
            }
        }

        private void FillProteoformFamiliesTables(int q)
        {
            CompileEdges(); // needs to be recreated after destorying it
            for (int i = 0; i < indexMasterList.Count; i++)
            {
                GlobalData.ProteoformFamiliesET.Tables.Add(new DataTable()); //add new table for the fam
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Accession", typeof(string));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Name", typeof(string));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Fragment", typeof(string));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("PTM List", typeof(string));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Proteoform Mass", typeof(double));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Aggregated Mass", typeof(double));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Aggregated Intensity", typeof(double));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Aggregated Retention Time", typeof(double));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Lysine Count", typeof(int));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Number of Observations", typeof(int));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Delta Mass", typeof(double));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Running Sum", typeof(int));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Peak Center Count", typeof(int));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Peak Center Mass", typeof(double));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Out of Range Decimal", typeof(bool));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Acceptable Peak", typeof(bool));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Proteoform Family", typeof(bool));
                GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Columns.Add("Group_#", typeof(Int32));

                GlobalData.ProteoformFamiliesEE.Tables.Add(new DataTable()); //add new table for the fam
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Aggregated Mass Light", typeof(double));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Aggregated Mass Heavy", typeof(double));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Aggregated Intensity Light", typeof(double));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Aggregated Intensity Heavy", typeof(double));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Retention Time Light", typeof(double));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Retention Time Heavy", typeof(double));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Lysine Count", typeof(int));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Number of Observations Light", typeof(int));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Number of Observations Heavy", typeof(int));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Delta Mass", typeof(double));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Running Sum", typeof(int));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Peak Center Count", typeof(int));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Peak Center Mass", typeof(double));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Out of Range Decimal", typeof(bool));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Acceptable Peak", typeof(bool));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Proteoform Family", typeof(bool));
                GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Columns.Add("Group_#", typeof(Int32));

                int Num_Edges = 0;

                for (int j=0; j<indexMasterList[i].Count;j++) //Fill ET and EE
                {
                    double lightmass = Convert.ToDouble(CompiledEdges.Rows[indexMasterList[i][j]]["Mass Light"]);
                    double heavymass = Convert.ToDouble(CompiledEdges.Rows[indexMasterList[i][j]]["Mass Heavy"]);
                    string type = CompiledEdges.Rows[indexMasterList[i][j]]["Type"].ToString();
                    if (type.Equals("EE"))
                    {
                        foundRowsSingle = EE_Groups.Select("[" + "Aggregated Mass Light" + "]=" + lightmass + "AND" + "[" + "Aggregated Mass Heavy" + "]=" + heavymass);
                        foreach (DataRow row in foundRowsSingle)
                        {
                            row["Group_#"] = PF_Group_Num;
                            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num)].Rows.Add(row.ItemArray);
                            Num_Edges++;
                        }
                    }
                    else if (type.Equals("ET"))
                    {
                        foundRowsSingle = ET_Groups.Select("[" + "Aggregated Mass" + "]=" + lightmass + "AND" + "[" + "Proteoform Mass" + "]=" + heavymass);
                        foreach (DataRow row in foundRowsSingle)
                        {
                            row["Group_#"] = PF_Group_Num;
                            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num)].Rows.Add(row.ItemArray);
                            Num_Edges++;
                        }
                    }
                    else if (type.Equals("TE"))
                    {
                        foundRowsSingle = ET_Groups.Select("[" + "Aggregated Mass" + "]=" + heavymass + "AND" + "[" + "Proteoform Mass" + "]=" + lightmass);
                        foreach (DataRow row in foundRowsSingle)
                        {
                            row["Group_#"] = PF_Group_Num;
                            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num)].Rows.Add(row.ItemArray);
                            Num_Edges++;
                        }
                    }
                }

                //Fill Metrics
                //Find NumberOfUniqueIDs
                DataTable distinctIdsTable = new DataTable();
                distinctIdsTable.Columns.Add("Unique Criteria", typeof(string));
                foreach (DataRow row in GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Rows)
                {
                    distinctIdsTable.Rows.Add(Convert.ToString(row["Accession"]) + Convert.ToString(row["PTM List"]));
                }
                var distinctIds = distinctIdsTable.AsEnumerable().Select(s => new { id = s.Field<string>("Unique Criteria"), }).Distinct().ToArray();
                int ID_Count = distinctIds.Count();

                DataTable distinctExpTable = new DataTable();
                distinctExpTable.Columns.Add("Exp_Masses", typeof(double));
                foreach (DataRow row in GlobalData.ProteoformFamiliesET.Tables[PF_Group_Num].Rows)
                {
                    distinctExpTable.Rows.Add(Convert.ToString(row["Aggregated Mass"]));
                }
                foreach (DataRow row in GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num].Rows)
                {
                    distinctExpTable.Rows.Add(Convert.ToString(row["Aggregated Mass Light"]));
                    distinctExpTable.Rows.Add(Convert.ToString(row["Aggregated Mass Heavy"]));
                }
                var distinctExp = distinctExpTable.AsEnumerable().Select(s => new { id = s.Field<double>("Exp_Masses"), }).Distinct().ToArray();
                int Exp_Count = distinctExp.Count();

                GlobalData.ProteoformFamilyMetrics.Rows.Add(PF_Group_Num, nodeMasterList[i][0], q, Exp_Count, ID_Count, Num_Edges); //group#, expmass, lys, #id  ID is the most time consuming, must run through entire list. Most feasible to achieve everytime ET is accessed.
                PF_Group_Num++;
            }
        }

        public void AssignColumns()
        {
            ET_Groups.Columns.Add("Accession", typeof(string));
            ET_Groups.Columns.Add("Name", typeof(string));
            ET_Groups.Columns.Add("Fragment", typeof(string));
            ET_Groups.Columns.Add("PTM List", typeof(string));
            ET_Groups.Columns.Add("Proteoform Mass", typeof(double));
            ET_Groups.Columns.Add("Aggregated Mass", typeof(double));
            ET_Groups.Columns.Add("Aggregated Intensity", typeof(double));
            ET_Groups.Columns.Add("Aggregated Retention Time", typeof(double));
            ET_Groups.Columns.Add("Lysine Count", typeof(int));
            ET_Groups.Columns.Add("Number of Observations", typeof(int));
            ET_Groups.Columns.Add("Delta Mass", typeof(double));
            ET_Groups.Columns.Add("Running Sum", typeof(int));
            ET_Groups.Columns.Add("Peak Center Count", typeof(int));
            ET_Groups.Columns.Add("Peak Center Mass", typeof(double));
            ET_Groups.Columns.Add("Out of Range Decimal", typeof(bool));
            ET_Groups.Columns.Add("Acceptable Peak", typeof(bool));
            ET_Groups.Columns.Add("Proteoform Family", typeof(bool));
            ET_Groups.Columns.Add("Group_#", typeof(Int32));

            EE_Groups.Columns.Add("Aggregated Mass Light", typeof(double));
            EE_Groups.Columns.Add("Aggregated Mass Heavy", typeof(double));
            EE_Groups.Columns.Add("Aggregated Intensity Light", typeof(double));
            EE_Groups.Columns.Add("Aggregated Intensity Heavy", typeof(double));
            EE_Groups.Columns.Add("Retention Time Light", typeof(double));
            EE_Groups.Columns.Add("Retention Time Heavy", typeof(double));
            EE_Groups.Columns.Add("Lysine Count", typeof(int));
            EE_Groups.Columns.Add("Number of Observations Light", typeof(int));
            EE_Groups.Columns.Add("Number of Observations Heavy", typeof(int));
            EE_Groups.Columns.Add("Delta Mass", typeof(double));
            EE_Groups.Columns.Add("Running Sum", typeof(int));
            EE_Groups.Columns.Add("Peak Center Count", typeof(int));
            EE_Groups.Columns.Add("Peak Center Mass", typeof(double));
            EE_Groups.Columns.Add("Out of Range Decimal", typeof(bool));
            EE_Groups.Columns.Add("Acceptable Peak", typeof(bool));
            EE_Groups.Columns.Add("Proteoform Family", typeof(bool));
            EE_Groups.Columns.Add("Group_#", typeof(Int32));

            CompiledEdges.Columns.Add("Mass Light", typeof(double));
            CompiledEdges.Columns.Add("Mass Heavy", typeof(double));
            CompiledEdges.Columns.Add("Type", typeof(string));

            GlobalData.ProteoformFamilyMetrics.Columns.Add("Group Number", typeof(Int32));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Lowest Mass", typeof(double));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Lysine Count", typeof(Int32));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Number of Experimental Masses", typeof(Int32));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Number of IDs", typeof(Int32));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Number of Edges", typeof(Int32));
        }

        private void Group_ET(int q)
        {
            ET_Groups.Clear();       

            DataRow[] foundRows;
            string expression = "[" + "Lysine Count" +"] = " + q + " AND " + "[Proteoform Family] = true";
            foundRows = GlobalData.experimentTheoreticalPairs.Select(expression);

            for (int a = 0; a < foundRows.Length; a++)
            {
                foundRows[a]["Proteoform Mass"] = Math.Truncate(Math.Round(Convert.ToDouble(foundRows[a]["Proteoform Mass"]), 3) * 1000) / 1000;
                foundRows[a]["Aggregated Mass"] = Math.Truncate(Math.Round(Convert.ToDouble(foundRows[a]["Aggregated Mass"]), 3) * 1000) / 1000;
                //foundRows[a]["Delta Mass"] = Math.Truncate(Math.Round(Convert.ToDouble(foundRows[a]["Delta Mass"]), 0));
                ET_Groups.Rows.Add(foundRows[a].ItemArray);
                ET_Groups.Rows[a]["Group_#"] = 0;
            }
        }   

        private void Group_EE(int q)
        {
            EE_Groups.Clear();

            DataRow[] foundRows;
            foundRows = GlobalData.experimentExperimentPairs.Select("[Lysine Count]=" + q + "AND" + "[Proteoform Family] = " + true, "Aggregated Mass Light ASC, Aggregated Mass Heavy ASC");
            for (int a = 0; a < foundRows.Length; a++)
            {
                foundRows[a]["Aggregated Mass Light"] = Math.Truncate(Math.Round(Convert.ToDouble(foundRows[a]["Aggregated Mass Light"]), 3) * 1000) / 1000;
                foundRows[a]["Aggregated Mass Heavy"] = Math.Truncate(Math.Round(Convert.ToDouble(foundRows[a]["Aggregated Mass Heavy"]), 3) * 1000) / 1000;
                //foundRows[a]["Delta Mass"] = Math.Truncate(Math.Round(Convert.ToDouble(foundRows[a]["Delta Mass"]), 0));
                EE_Groups.Rows.Add(foundRows[a].ItemArray);
                EE_Groups.Rows[a]["Group_#"] = 0;
            }
        }

        private void CompileEdges()
        {
            CompiledEdges.Clear();
            //populate CompiledEdges
            foreach (DataRow row in EE_Groups.Rows)
            {
                CompiledEdges.Rows.Add(row["Aggregated Mass Light"], row["Aggregated Mass Heavy"], "EE");
            }
            foreach (DataRow row in ET_Groups.Rows)
            {
                if (Convert.ToDouble(row["Delta Mass"]) < 0)
                {
                    CompiledEdges.Rows.Add(row["Aggregated Mass"], row["Proteoform Mass"], "ET");
                }
                else
                {
                    CompiledEdges.Rows.Add(row["Proteoform Mass"], row["Aggregated Mass"], "TE");
                }
            }
            //Sort CompiledEdges
            DataView dv = CompiledEdges.DefaultView;
            dv.Sort = "Mass Light DESC, Mass Heavy DESC";
            CompiledEdges = dv.ToTable();
        }

        private void ExportProteoformFamilies()
        {
            if (ExportDataTable.Columns.Count == 0)
            {
                ExportDataTable.Columns.Add("Mass #1", typeof(string));
                ExportDataTable.Columns.Add("Mass #2", typeof(string));
                ExportDataTable.Columns.Add("Delta Mass", typeof(double));
                ExportDataTable.Columns.Add("Type #1", typeof(string));
                ExportDataTable.Columns.Add("Type #2", typeof(string));
                ExportDataTable.Columns.Add("Intensity #1", typeof(double));
                ExportDataTable.Columns.Add("Intensity #2", typeof(double));
            }

            foreach (DataTable dt in GlobalData.ProteoformFamiliesEE.Tables)
            {
                foreach (DataRow row in dt.Rows)
                {
                    ExportDataTable.Rows.Add((row["Aggregated Mass Light"].ToString() + "_n=" + row["Number of Observations Light"].ToString() 
                                    + "_K=" + row["Lysine Count"].ToString()),
                                    (row["Aggregated Mass Heavy"].ToString() + "_n=" + row["Number of Observations Heavy"].ToString() + "_K=" 
                                    + row["Lysine Count"].ToString()),
                                    row["Delta Mass"],
                                    "Experimental",
                                    "Experimental",
                                    row["Aggregated Intensity Light"],
                                    row["Aggregated Intensity Heavy"]);
                }
            }
            
            foreach (DataTable dt in GlobalData.ProteoformFamiliesET.Tables)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["PTM List"].Equals("unmodified"))
                    {
                        ExportDataTable.Rows.Add((row["Accession"].ToString() + "_K=" + row["Lysine Count"].ToString()),
                            (row["Aggregated Mass"].ToString() + "_n=" + row["Number of Observations"].ToString() 
                            + "_K=" + row["Lysine Count"].ToString()),
                            row["Delta Mass"],
                            "Theoretical",
                            "Experimental",
                            1000000000,
                            row["Aggregated Intensity"]);
                    }
                    else
                    {
                        ExportDataTable.Rows.Add((row["Accession"].ToString() +row["PTM List"].ToString()+ "_K=" + row["Lysine Count"].ToString()),
                        (row["Aggregated Mass"].ToString() + "_n=" + row["Number of Observations"].ToString() 
                        + "_K=" + row["Lysine Count"].ToString()),
                        row["Delta Mass"],
                        "TheoreticalWithPTM",
                        "Experimental",
                        1000000000,
                        row["Aggregated Intensity"]);
                    }
                }
            }
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            int groupnumber;
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];
                groupnumber = Convert.ToInt32(row.Cells["Group Number"].Value);
                dataGridView2.DataSource = GlobalData.ProteoformFamiliesET.Tables[(groupnumber)];
                dataGridView3.DataSource = GlobalData.ProteoformFamiliesEE.Tables[(groupnumber)];
            }
        } //called in DGV

        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        } //called in dataGridView1_CellContentClick_1

        private void dataGridView3_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }//called in dataGridView1_CellContentClick_1


        public override string ToString()
        {
            return "ProteoformFamilyAssignment|";
        }

        public void loadSetting(string setting_specs)
        {
            string[] fields = setting_specs.Split('\t');
            switch (fields[0].Split('|')[1])
            {
                case "":
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Build the CSV file data as a Comma separated string.
            csv = "";

            //Add the Header row for CSV file.
            foreach (DataColumn column in ExportDataTable.Columns)
            {
                csv += column.ColumnName + '\t';
            }

            //Add new line.
            csv += "\r\n";

            //Adding the Rows
            foreach (DataRow row in ExportDataTable.Rows)
            {
                for (int i = 0; i < ExportDataTable.Columns.Count; i++)
                {
                    //Add the Data rows.
                    csv += row[i].ToString() + '\t';
                }

                //Add new line.
                csv += "\r\n";
            }

            //Exporting to CSV.
            File.WriteAllText(filename, csv);
            MessageBox.Show("Export Successful!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.PFAFileSave.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                filename = Path.GetFileName(PFAFileSave.FileName);
            }
            textBox1.Text = filename;
        }
    }
}