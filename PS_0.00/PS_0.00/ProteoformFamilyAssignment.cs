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
using System.Security;


namespace PS_0._00
{
    public partial class ProteoformFamilyAssignment : Form
    {
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private DataGridView dataGridView1;
        private DataGridView dataGridView2;
        public static DataTable ET_Groups = new DataTable(); //4/5/16
        public static DataTable EE_Groups = new DataTable(); //4/5/16
        public static int PF_Group_Num = 0;
        public static double parentmass=0;
        public static int EE_Checkpoint = 0;
        private SplitContainer splitContainer3;
        private DataGridView dataGridView3;
        private Button button2;
        private TextBox textBox1;
        private Button button1;
        private TextBox textBox2;
        private Label label1;
        DataRow[] foundRowsSingle;
        public static string filename="";
        public static string csv ="";
        public static DataTable ExportDataTable = new DataTable();
        public static string folderPath="";
        //OpenFileDialog openFileDialog1 = new OpenFileDialog();
        FolderBrowserDialog FolderBrowserDialog1 = new FolderBrowserDialog();

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
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
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
            this.splitContainer1.Size = new System.Drawing.Size(1010, 456);
            this.splitContainer1.SplitterDistance = 548;
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
            this.splitContainer2.Size = new System.Drawing.Size(548, 456);
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
            this.dataGridView1.Size = new System.Drawing.Size(548, 230);
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
            this.dataGridView2.Size = new System.Drawing.Size(548, 222);
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
            this.splitContainer3.Panel1.Controls.Add(this.textBox2);
            this.splitContainer3.Panel1.Controls.Add(this.label1);
            this.splitContainer3.Panel1.Controls.Add(this.button2);
            this.splitContainer3.Panel1.Controls.Add(this.textBox1);
            this.splitContainer3.Panel1.Controls.Add(this.button1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.dataGridView3);
            this.splitContainer3.Size = new System.Drawing.Size(458, 456);
            this.splitContainer3.SplitterDistance = 230;
            this.splitContainer3.TabIndex = 0;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(152, 64);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(232, 22);
            this.textBox2.TabIndex = 4;
            this.textBox2.TextChanged += new System.EventHandler(this.FileName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "File Name:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(11, 100);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(127, 33);
            this.button2.TabIndex = 2;
            this.button2.Text = "Export CSV";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ExportCSV_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(152, 22);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(232, 22);
            this.textBox1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(11, 17);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(127, 33);
            this.button1.TabIndex = 0;
            this.button1.Text = "Select Directory";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.SelectDirectory_Click);
            // 
            // dataGridView3
            // 
            this.dataGridView3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView3.Location = new System.Drawing.Point(0, 0);
            this.dataGridView3.Name = "dataGridView3";
            this.dataGridView3.RowTemplate.Height = 24;
            this.dataGridView3.Size = new System.Drawing.Size(458, 222);
            this.dataGridView3.TabIndex = 0;
            this.dataGridView3.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView3_CellContentClick_1);
            // 
            // ProteoformFamilyAssignment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 456);
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
            //alrighty, here's the meat of the module, good luck


             // for each unique lysine count
             //put a sweet message box here to track relative progress (state q)
            if (Convert.ToInt32(EE_Groups.Rows.Count).Equals(0)) //if nothing with that lysine count, then ignore it. Only 25 iterations
            {
                    
            }
            else //somethin there!
            {
                double lightmass = Convert.ToDouble(EE_Groups.Rows[0]["Aggregated Mass Light"]);
                double heavymass = Convert.ToDouble(EE_Groups.Rows[0]["Aggregated Mass Heavy"]);
                parentmass = lightmass;
                List<double> ChildrenList = new List<double>();
                Its_A_Parent(q, lightmass, heavymass, ChildrenList, 0);
                for (int i = 1; i < EE_Groups.Rows.Count; i++)//gonna have to go through each one to determine child or not, some more painful than others
                {
                    if (EE_Groups.Rows[i]["Aggregated Mass Light"].Equals(EE_Groups.Rows[(i - 1)]["Aggregated Mass Light"])) //Gotta see if the next one is a parent or child. If it has the same mass1, we know it's in the family
                    {
                        FamilyMember(ChildrenList, q, i);
                    }
                    else //potentially a parent or child
                    {
                        if (ChildrenList.Contains(Convert.ToDouble(EE_Groups.Rows[i]["Aggregated Mass Light"]))==true)//if it's a child //.Equals(EE_Groups.Tables[(q - 2)].Rows[EE_Checkpoint]["Aggregated Mass Heavy"])) //if child
                        {
                            FamilyMember(ChildrenList, q, i);
                        }
                        else //it's a parent. We can save this parent, but we gotta keep moving for right now to find the rest of the current family
                        {
                            if (EE_Checkpoint==0) //if not zero, there's already a checkpoint we don't want to overwrite
                            {
                                EE_Checkpoint = i;
                            }
                        }
                    }

                    double newmass = Convert.ToDouble(EE_Groups.Rows[i]["Aggregated Mass Light"]);
                    double oldmass = ChildrenList.OfType<double>().Max();
                    if (newmass > oldmass) //are we at the end of a family?
                    {
                        Family_Death(q, ChildrenList, parentmass);
                        i = EE_Checkpoint; //jump to the last new family. This is a potential problem
                        ChildrenList.Clear(); //remove old children
                        double newlightmass = Convert.ToDouble(EE_Groups.Rows[i]["Aggregated Mass Light"]);
                        double newheavymass = Convert.ToDouble(EE_Groups.Rows[i]["Aggregated Mass Heavy"]);
                        Its_A_Parent(q, newlightmass, newheavymass, ChildrenList, i);//start first entry in childrenlist
                        EE_Checkpoint = 0;
                    }
                }

                Family_Death(q, ChildrenList, parentmass);
                
            }
        }

        private void AssignColumns()
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
            EE_Groups.Columns.Add("Delta Mass", typeof(double));
            EE_Groups.Columns.Add("Running Sum", typeof(int));
            EE_Groups.Columns.Add("Peak Center Count", typeof(int));
            EE_Groups.Columns.Add("Peak Center Mass", typeof(double));
            EE_Groups.Columns.Add("Out of Range Decimal", typeof(bool));
            EE_Groups.Columns.Add("Acceptable Peak", typeof(bool));
            EE_Groups.Columns.Add("Proteoform Family", typeof(bool));
            EE_Groups.Columns.Add("Group_#", typeof(Int32));

            GlobalData.ProteoformFamilyMetrics.Columns.Add("Group Number", typeof(Int32));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Parent Mass", typeof(double));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Lysine Count", typeof(Int32));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Number of Experimental Masses", typeof(Int32));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Number of IDs", typeof(Int32));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Number of Nodes", typeof(Int32));
        }

        private void Group_ET(int q)
        {
            ET_Groups.Clear();       

            DataRow[] foundRows;
            string expression = "[" + "Lysine Count" +"] = " + q + " AND " + "[Proteoform Family] = true";
            foundRows = GlobalData.experimentTheoreticalPairs.Select(expression); 
            for (int a = 0; a < foundRows.Length; a++)
            {
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
                EE_Groups.Rows.Add(foundRows[a].ItemArray);
                EE_Groups.Rows[a]["Group_#"] = 0;
            }
        }

        private void Its_A_Parent(int lys, double lightmass, double heavymass, List<double> ChildrenList, int EEIndex)//, int checkpoint)
        {
            parentmass = lightmass; //static because of this
            PF_Group_Num++;
            GlobalData.ProteoformFamiliesET.Tables.Add(new DataTable()); //add new table for the fam
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Accession", typeof(string));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Name", typeof(string));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Fragment", typeof(string));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("PTM List", typeof(string));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Proteoform Mass", typeof(double));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Mass", typeof(double));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Intensity", typeof(double));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Retention Time", typeof(double));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Lysine Count", typeof(int));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Delta Mass", typeof(double));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Running Sum", typeof(int));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Peak Center Count", typeof(int));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Peak Center Mass", typeof(double));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Out of Range Decimal", typeof(bool));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Acceptable Peak", typeof(bool));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Proteoform Family", typeof(bool));
            GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Group_#", typeof(Int32));

            foundRowsSingle = ET_Groups.Select("[" + "Aggregated Mass" + "]=" + lightmass, "Proteoform Mass"); //make it org by theo mass later
            foreach (DataRow row in foundRowsSingle)
            {
                row["Group_#"] = PF_Group_Num;
                GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Rows.Add(row.ItemArray);
            }
            ChildrenList.Add(heavymass);

            GlobalData.ProteoformFamiliesEE.Tables.Add(new DataTable()); //add new table for the fam
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Mass Light", typeof(double));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Mass Heavy", typeof(double));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Intensity Light", typeof(double));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Intensity Heavy", typeof(double));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Retention Time Light", typeof(double));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Retention Time Heavy", typeof(double));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Lysine Count", typeof(int));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Delta Mass", typeof(double));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Running Sum", typeof(int));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Peak Center Count", typeof(int));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Peak Center Mass", typeof(double));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Out of Range Decimal", typeof(bool));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Acceptable Peak", typeof(bool));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Proteoform Family", typeof(bool));
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Group_#", typeof(Int32));

            DataRow EErow = EE_Groups.Rows[EEIndex]; //make it org by theo mass later
            EErow["Group_#"] = PF_Group_Num;
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Rows.Add(EErow.ItemArray);
        }

        private void Its_A_Child(int lys, double mass, int EEIndex)//, int checkpoint)
        {

            foundRowsSingle = ET_Groups.Select("[Aggregated Mass]=" + mass, "Proteoform Mass"); //make it org by theo mass later
            foreach (DataRow row in foundRowsSingle)
            {
                row["Group_#"] = PF_Group_Num;
                GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Rows.Add(row.ItemArray);
            }
            DataRow EErow = EE_Groups.Rows[EEIndex];
            EErow["Group_#"] = PF_Group_Num;
            GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Rows.Add(EErow.ItemArray);
        }

        private void Family_Death(int lys, List<double> ChildrenList, double parentmass)
        {

            int Num_Exp_Mass = ChildrenList.Count() + 1;
            int Num_Nodes = GlobalData.ProteoformFamiliesEE.Tables[PF_Group_Num - 1].Rows.Count;
            if ((GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Rows.Count).Equals(0)) //this statement catches families that do not have any theoretical matches
            {
                GlobalData.ProteoformFamilyMetrics.Rows.Add(PF_Group_Num, parentmass, lys, Num_Exp_Mass, 0, Num_Nodes);
            }
            else
            {
                var distinctIds = GlobalData.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].AsEnumerable().Select(s => new { id = s.Field<double>("Proteoform Mass"), }).Distinct().ToList();
                int ID_Count = distinctIds.Count;
                GlobalData.ProteoformFamilyMetrics.Rows.Add(PF_Group_Num, parentmass, lys, Num_Exp_Mass, ID_Count, Num_Nodes); //group#, #exp, lys, #id  ID is the most time consuming, must run through entire list. Most feasible to achieve everytime ET is accessed.
            }
        }

        private void FamilyMember(List<double> ChildrenList, int q, int i)
        {
            
            if (ChildrenList.Contains(Convert.ToDouble(EE_Groups.Rows[i]["Aggregated Mass Heavy"])) == true) //if we've already seen its child, we don't care about it again save for the node
            {
                DataRow EErow = EE_Groups.Rows[i];
                EErow["Group_#"] = PF_Group_Num;
                GlobalData.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Rows.Add(EErow.ItemArray);
            }
            else //it's a new child and we gotta save it
            {
                double childmass = Convert.ToDouble(EE_Groups.Rows[i]["Aggregated Mass Heavy"]);
                Its_A_Child(q, childmass, i);
                ChildrenList.Add(childmass); //another child potentially exists
            }
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

                    ExportDataTable.Rows.Add((row["Aggregated Mass Light"].ToString() //+ "_n=" + row["Running Sum"].ToString() 
                        + "_K=" + row["Lysine Count"].ToString()),
                        (row["Aggregated Mass Heavy"].ToString() //+ "_n=" + row["Running Sum"].ToString() + "_K=" 
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
                    ExportDataTable.Rows.Add((row["Accession"].ToString() + "_K=" + row["Lysine Count"].ToString()),
                        (row["Aggregated Mass"].ToString() + "_n=" //+ row["Running Sum"].ToString() 
                        + "_K=" + row["Lysine Count"].ToString()),
                        row["Delta Mass"],
                        "Theoretical",
                        "Experimental",
                        10000000,
                        row["Aggregated Intensity"]);
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
                dataGridView2.DataSource = GlobalData.ProteoformFamiliesET.Tables[(groupnumber - 1)];
                dataGridView3.DataSource = GlobalData.ProteoformFamiliesEE.Tables[(groupnumber - 1)];
            }
        } //called in DGV

        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        } //called in dataGridView1_CellContentClick_1

        private void dataGridView3_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }//called in dataGridView1_CellContentClick_1

        private void ExportCSV_Click(object sender, EventArgs e)
        {
            FileName_TextChanged(sender, e);

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
                for (int i=0; i<ExportDataTable.Columns.Count; i++)
                {
                    //Add the Data rows.
                    csv += row[i].ToString() + '\t';
                }

                //Add new line.
                csv += "\r\n";
            }

            //Exporting to CSV.
            File.WriteAllText(folderPath +"\\"+ filename, csv);
            MessageBox.Show("Export Successful!");
        }

        private void SelectDirectory_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.FolderBrowserDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = FolderBrowserDialog1.SelectedPath;
            }
                //this.openFileDialog1.Filter = "Folder (*.00)|*.00";
                //folderPath = openFileDialog1.FileName.ToString();
                //DialogResult dr = this.openFileDialog1.ShowDialog();
            //folderPath = dr.ToString();
            textBox1.Text = folderPath;
            //folderPath = textBox1.ToString();
        }

        private void FileName_TextChanged(object sender, EventArgs e)
        {
            filename = this.textBox2.Text.ToString();

        }
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
    }
}