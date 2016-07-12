using ProteoformSuiteInternal;
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


namespace ProteoformSuite
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
        DataRow[] foundRowsSingle;
        private Button button1;
        private Button button2;
        private TextBox textBox1;
        private TextBox textBox2;
        private Label label1;
        public static string filename = "";
        public static string csv = "";
        public static DataTable ExportDataTable = new DataTable();
        public static string folderPath = "";
        //OpenFileDialog openFileDialog1 = new OpenFileDialog();
        FolderBrowserDialog FolderBrowserDialog1 = new FolderBrowserDialog();

        public ProteoformFamilyAssignment()
        {
            //InitializeComponent();
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
            dataGridView1.DataSource = Lollipop.ProteoformFamilyMetrics;
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
            //Group_ET(q);
            //Group_EE(q);
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

            Lollipop.ProteoformFamilyMetrics.Columns.Add("Group Number", typeof(Int32));
            Lollipop.ProteoformFamilyMetrics.Columns.Add("Parent Mass", typeof(double));
            Lollipop.ProteoformFamilyMetrics.Columns.Add("Lysine Count", typeof(Int32));
            Lollipop.ProteoformFamilyMetrics.Columns.Add("Number of Experimental Masses", typeof(Int32));
            Lollipop.ProteoformFamilyMetrics.Columns.Add("Number of IDs", typeof(Int32));
            Lollipop.ProteoformFamilyMetrics.Columns.Add("Number of Edges", typeof(Int32));
        }

        private void Its_A_Parent(int lys, double lightmass, double heavymass, List<double> ChildrenList, int EEIndex)//, int checkpoint)
        {
            parentmass = lightmass; //static because of this
            PF_Group_Num++;
            Lollipop.ProteoformFamiliesET.Tables.Add(new DataTable()); //add new table for the fam
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Accession", typeof(string));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Name", typeof(string));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Fragment", typeof(string));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("PTM List", typeof(string));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Proteoform Mass", typeof(double));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Mass", typeof(double));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Intensity", typeof(double));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Retention Time", typeof(double));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Lysine Count", typeof(int));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Number of Observations", typeof(int));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Delta Mass", typeof(double));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Running Sum", typeof(int));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Peak Center Count", typeof(int));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Peak Center Mass", typeof(double));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Out of Range Decimal", typeof(bool));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Acceptable Peak", typeof(bool));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Proteoform Family", typeof(bool));
            Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Columns.Add("Group_#", typeof(Int32));

            foundRowsSingle = ET_Groups.Select("[" + "Aggregated Mass" + "]=" + lightmass, "Proteoform Mass"); //make it org by theo mass later
            foreach (DataRow row in foundRowsSingle)
            {
                row["Group_#"] = PF_Group_Num;
                Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Rows.Add(row.ItemArray);
            }
            ChildrenList.Add(heavymass);

            Lollipop.ProteoformFamiliesEE.Tables.Add(new DataTable()); //add new table for the fam
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Mass Light", typeof(double));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Mass Heavy", typeof(double));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Intensity Light", typeof(double));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Aggregated Intensity Heavy", typeof(double));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Retention Time Light", typeof(double));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Retention Time Heavy", typeof(double));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Lysine Count", typeof(int));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Number of Observations Light", typeof(int));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Number of Observations Heavy", typeof(int));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Delta Mass", typeof(double));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Running Sum", typeof(int));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Peak Center Count", typeof(int));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Peak Center Mass", typeof(double));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Out of Range Decimal", typeof(bool));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Acceptable Peak", typeof(bool));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Proteoform Family", typeof(bool));
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Columns.Add("Group_#", typeof(Int32));

            DataRow EErow = EE_Groups.Rows[EEIndex]; //make it org by theo mass later
            EErow["Group_#"] = PF_Group_Num;
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Rows.Add(EErow.ItemArray);
        }

        private void Its_A_Child(int lys, double mass, int EEIndex)//, int checkpoint)
        {

            foundRowsSingle = ET_Groups.Select("[Aggregated Mass]=" + mass, "Proteoform Mass"); //make it org by theo mass later
            foreach (DataRow row in foundRowsSingle)
            {
                row["Group_#"] = PF_Group_Num;
                Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Rows.Add(row.ItemArray);
            }
            DataRow EErow = EE_Groups.Rows[EEIndex];
            EErow["Group_#"] = PF_Group_Num;
            Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Rows.Add(EErow.ItemArray);
        }

        private void Family_Death(int lys, List<double> ChildrenList, double parentmass)
        {

            int Num_Exp_Mass = ChildrenList.Count() + 1;
            int Num_Edges = Lollipop.ProteoformFamiliesEE.Tables[PF_Group_Num - 1].Rows.Count;
            if ((Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].Rows.Count).Equals(0)) //this statement catches families that do not have any theoretical matches
            {
                Lollipop.ProteoformFamilyMetrics.Rows.Add(PF_Group_Num, parentmass, lys, Num_Exp_Mass, 0, Num_Edges);
            }
            else
            {
                var distinctIds = Lollipop.ProteoformFamiliesET.Tables[(PF_Group_Num - 1)].AsEnumerable().Select(s => new { id = s.Field<double>("Proteoform Mass"), }).Distinct().ToList();
                int ID_Count = distinctIds.Count;
                Lollipop.ProteoformFamilyMetrics.Rows.Add(PF_Group_Num, parentmass, lys, Num_Exp_Mass, ID_Count, Num_Edges); //group#, #exp, lys, #id  ID is the most time consuming, must run through entire list. Most feasible to achieve everytime ET is accessed.
            }
        }

        private void FamilyMember(List<double> ChildrenList, int q, int i)
        {

            if (ChildrenList.Contains(Convert.ToDouble(EE_Groups.Rows[i]["Aggregated Mass Heavy"])) == true) //if we've already seen its child, we don't care about it again save for the node
            {
                DataRow EErow = EE_Groups.Rows[i];
                EErow["Group_#"] = PF_Group_Num;
                Lollipop.ProteoformFamiliesEE.Tables[(PF_Group_Num - 1)].Rows.Add(EErow.ItemArray);
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

            foreach (DataTable dt in Lollipop.ProteoformFamiliesEE.Tables)
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
            
            foreach (DataTable dt in Lollipop.ProteoformFamiliesET.Tables)
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
                            10000000,
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
                        10000000,
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
                dataGridView2.DataSource = Lollipop.ProteoformFamiliesET.Tables[(groupnumber - 1)];
                dataGridView3.DataSource = Lollipop.ProteoformFamiliesEE.Tables[(groupnumber - 1)];
            }
        } //called in DGV

        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        } //called in dataGridView1_CellContentClick_1

        private void dataGridView3_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }//called in dataGridView1_CellContentClick_1


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2_TextChanged(sender, e);

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
            File.WriteAllText(folderPath + "\\" + filename, csv);
            MessageBox.Show("Export Successful!");
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            filename = this.textBox2.Text.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
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
    }
}