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
    public partial class ProteoformFamilyAssignment : Form
    {
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private DataGridView dataGridView1;
        private DataGridView dataGridView2;
        public static int group = 0;
        public ProteoformFamilyAssignment()
        {
            InitializeComponent();
            //MessageBox.Show("Working");
            //ProteoformFamilyAssignment_Load(object sender, EventArgs e);
        }
        //public static DataTable EE = new DataTable();
        //public static DataTable ET = new DataTable();


        private void ee(object sender, EventArgs e)
        {

        }
        //Everything has been made. Now put it in windows.


     //   private void FillETGridView()
   //     {
            //BindingSource dgv_PF_BS = new BindingSource();
          //  dgv_PF_BS.DataSource = GlobalData.ProteoformFamilyMetrics;
        //    dgv_ET_Pairs.DataSource = dgv_PF_BS;
      //      dgv_ET_Pairs.AutoGenerateColumns = true;
    //        dgv_ET_Pairs.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
  //          dgv_ET_Pairs.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
//        }


        //Find Children Function
        private void Find_Children(double mass, int lys, EventArgs e)
        {
            DataTable EE_Children = new DataTable();
            EE_Children.Columns.Add("Aggregated Mass Light", typeof(double));
            EE_Children.Columns.Add("Aggregated Mass Heavy", typeof(double));
            EE_Children.Columns.Add("Aggregated Intensity Light", typeof(double));
            EE_Children.Columns.Add("Aggregated Intensity Heavy", typeof(double));
            EE_Children.Columns.Add("Retention Time Light", typeof(double));
            EE_Children.Columns.Add("Retention Time Heavy", typeof(double));
            EE_Children.Columns.Add("Lysine Count", typeof(int));
            EE_Children.Columns.Add("Delta Mass", typeof(double));
            EE_Children.Columns.Add("Running Sum", typeof(int));
            EE_Children.Columns.Add("Peak Center Count", typeof(int));
            EE_Children.Columns.Add("Peak Center Mass", typeof(double));
            EE_Children.Columns.Add("Out of Range Decimal", typeof(bool));
            EE_Children.Columns.Add("Acceptable Peak", typeof(bool));
            for (int a=0; a<GlobalData.experimentTheoreticalPairs.Rows.Count; a++)
            {
                if (GlobalData.experimentTheoreticalPairs.Rows[a]["Lysine Count"].Equals(lys) && GlobalData.experimentTheoreticalPairs.Rows[a]["Aggregated Mass"].Equals(mass))
                {
                    //MessageBox.Show((group.GetType()).ToString()); Still Int32 here...
                    GlobalData.experimentTheoreticalPairs.Rows[a]["Group_#"] = group;
                    //MessageBox.Show("Named Child!"); This worked!
                }
            }
            //DataTable EE_Children = EE.Rows.Select("[Exp_Mass1] =" + mass + "AND [Lys_Ct] =" + lys);
            //MessageBox.Show("for loop 1 works");
            for (int a = 0; a < GlobalData.experimentExperimentPairs.Rows.Count; a++)
            {
                if (GlobalData.experimentExperimentPairs.Rows[a]["Lysine Count"].Equals(lys) && GlobalData.experimentExperimentPairs.Rows[a]["Aggregated Mass Light"].Equals(mass))
                {
                    EE_Children.Rows.Add(GlobalData.experimentExperimentPairs.Rows[a].ItemArray);
                }
            }
            //MessageBox.Show("for loop 2 works");
            //Never does the foreach loop before it fizzles
            foreach (DataRow Child in EE_Children.Rows)
            {
                MessageBox.Show("Got a child");
                //(ET.Rows.Select("[Lys_Ct] ="+ Child["Lys_Ct"]+ "AND [Exp_Mass] ="+ Child["Exp_Mass1"]))["Group_#"]=group;
                for (int a = 0; a < GlobalData.experimentTheoreticalPairs.Rows.Count; a++)
                {
                    MessageBox.Show("Got an ETrow");
                    if (GlobalData.experimentTheoreticalPairs.Rows[a]["Lysine Count"].Equals(Child["Lysine Count"]) && GlobalData.experimentTheoreticalPairs.Rows[a]["Aggregated Mass"].Equals(Child["Aggregated Mass Light"]))
                    {
                        MessageBox.Show("labeling the child");
                        // Well that didn't work either. MessageBox.Show("Everything works before this point");
                        GlobalData.experimentTheoreticalPairs.Rows[a]["Group_#"] = group;
                        //HAD A BIG O FAT STINKY ERRA HEEA MessageBox.Show("NamedChild?");
                       
                    }
                }
                MessageBox.Show("We're gonna loop");
                Find_Children(Convert.ToDouble(Child["Aggregated Mass Heavy"]), Convert.ToInt32(Child["Lysine Count"]), e);
            }
        }


        //DataGridView3
        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }



        //Click on PF_Contents table, see related Table in dataGridView2
       // private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
       // {

       // }

     //   private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
      //  {
      //
        //}

        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
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
            this.splitContainer1.Size = new System.Drawing.Size(587, 456);
            this.splitContainer1.SplitterDistance = 319;
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
            this.splitContainer2.Size = new System.Drawing.Size(319, 456);
            this.splitContainer2.SplitterDistance = 230;
            this.splitContainer2.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(319, 230);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick_1);
            // 
            // dataGridView2
            // 
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView2.Location = new System.Drawing.Point(0, 0);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.RowTemplate.Height = 24;
            this.dataGridView2.Size = new System.Drawing.Size(319, 222);
            this.dataGridView2.TabIndex = 0;
            this.dataGridView2.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView2_CellContentClick_1);
            // 
            // ProteoformFamilyAssignment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 456);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProteoformFamilyAssignment";
            this.Load += new System.EventHandler(this.ProteoformFamilyAssignment_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.ResumeLayout(false);

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            int groupnumber;
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];
                groupnumber = Convert.ToInt32(row.Cells["Group_#"].Value);
                dataGridView2.DataSource = GlobalData.ProteoformFamilies.Tables[groupnumber - 1];
            }
        }

        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }





        private void ProteoformFamilyAssignment_Load(object sender, EventArgs e)
        {
            //Create Parent Group of EE
            //DataTable dt = new DataTable();
            //MessageBox.Show("Working");
            GlobalData.EE_Parent.Columns.Add("Aggregated Mass Light", typeof(double));
            GlobalData.EE_Parent.Columns.Add("Aggregated Mass Heavy", typeof(double));
            GlobalData.EE_Parent.Columns.Add("Aggregated Intensity Light", typeof(double));
            GlobalData.EE_Parent.Columns.Add("Aggregated Intensity Heavy", typeof(double));
            GlobalData.EE_Parent.Columns.Add("Retention Time Light", typeof(double));
            GlobalData.EE_Parent.Columns.Add("Retention Time Heavy", typeof(double));
            GlobalData.EE_Parent.Columns.Add("Lysine Count", typeof(int));
            GlobalData.EE_Parent.Columns.Add("Delta Mass", typeof(double));
            GlobalData.EE_Parent.Columns.Add("Running Sum", typeof(int));
            GlobalData.EE_Parent.Columns.Add("Peak Center Count", typeof(int));
            GlobalData.EE_Parent.Columns.Add("Peak Center Mass", typeof(double));
            GlobalData.EE_Parent.Columns.Add("Out of Range Decimal", typeof(bool));
            GlobalData.EE_Parent.Columns.Add("Acceptable Peak", typeof(bool));

            //            EE_Parent.Columns.Add("Exp_Mass1", typeof(decimal));
            //          EE_Parent.Columns.Add("Exp_Mass2", typeof(decimal));
            //        EE_Parent.Columns.Add("Lys_Ct", typeof(int));

            for (int i = 0; i < GlobalData.experimentExperimentPairs.Rows.Count; i++) //go through each EE row
            {
                int n = 0; //counter
                for (int j = 0; j < GlobalData.experimentExperimentPairs.Rows.Count; j++) //for each EE row, compare with each EE row
                {
                    if (GlobalData.experimentExperimentPairs.Rows[i]["Aggregated Mass Light"].Equals(GlobalData.experimentExperimentPairs.Rows[j]["Aggregated Mass Heavy"]) && GlobalData.experimentExperimentPairs.Rows[i]["Lysine Count"].Equals(GlobalData.experimentExperimentPairs.Rows[j]["Lysine Count"])) //not a parent if its light mass found in the heavy of another (thus it's a child)
                    {
                        n = n + 1; //add to counter if i is child of a row j
                    }
                }
                if (n.Equals(0)) // && (EE_Parent.Rows.Select("[Lys_Ct] =" EE.Rows[i]["Lys_Ct"])).Equals(0)) //if parent
                {
                    GlobalData.EE_Parent.Rows.Add(GlobalData.experimentExperimentPairs.Rows[i].ItemArray); //add to dt EE_Parent
                }
                //MessageBox.Show(n.ToString());
            }

            //remove parent repeats
            for (int i = 0; i < GlobalData.EE_Parent.Rows.Count; i++) //for each row of parents
            {
                for (int j = i + 1; j < GlobalData.EE_Parent.Rows.Count; j++) //compare against all other rows of parents excluding those already compared against
                {
                    if (GlobalData.EE_Parent.Rows[i]["Lysine Count"].Equals(GlobalData.EE_Parent.Rows[j]["Lysine Count"]) && GlobalData.EE_Parent.Rows[i]["Aggregated Mass Light"].Equals(GlobalData.EE_Parent.Rows[j]["Aggregated Mass Light"])) //if identical parent mass and lys
                    {
                        GlobalData.EE_Parent.Rows[j].Delete(); //delete the repeat, we're only interested in the light Mass to determine the parent.
                    }
                }
            }

            //Use Parent Group to assign group#s to ET
            //add group of zeroes
            GlobalData.experimentTheoreticalPairs.Columns.Add("Group_#", typeof(int)); //Add group id column to ET
            //int group = 0;
            for (int i = 0; i < GlobalData.experimentTheoreticalPairs.Rows.Count; i++) //for each entry in ET
            {
                //MessageBox.Show("pre row of zeroes");
                GlobalData.experimentTheoreticalPairs.Rows[i]["Group_#"] = group; //populate group id column with zeroes
                //MessageBox.Show("post row of zeroes");
            }

            //label parents in ET
            foreach (DataRow parent in GlobalData.EE_Parent.Rows)
            {
                //MessageBox.Show(GlobalData.EE_Parent.Rows.Count.ToString());
                group = group + 1;
                //(ET.Rows.Select("[Lys_Ct] =" + parent["Lys_Ct"]+ "AND [Exp_Mass] =" + parent["Exp_Mass2"]))["Group_#"] = group;
                for (int a = 0; a < GlobalData.experimentTheoreticalPairs.Rows.Count; a++)
                {
                    if (GlobalData.experimentTheoreticalPairs.Rows[a]["Lysine Count"].Equals(parent["Lysine Count"]) && GlobalData.experimentTheoreticalPairs.Rows[a]["Aggregated Mass"].Equals(parent["Aggregated Mass Light"])) //&& ET.Rows[a]["Group_#"].Equals(0))
                    {
                       // MessageBox.Show("Crash imminent");
                        GlobalData.experimentTheoreticalPairs.Rows[a]["Group_#"] = group;
                       // MessageBox.Show("labeled parents");
                    }
                }
                Find_Children(Convert.ToDouble(parent["Aggregated Mass Heavy"]), Convert.ToInt32(parent["Lysine Count"]), e);

            }


            //Create dataTable of contents
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Group_#", typeof(int));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("#_Exp", typeof(int));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("Lysine Count", typeof(int));
            GlobalData.ProteoformFamilyMetrics.Columns.Add("#_IDs", typeof(int));
           // MessageBox.Show(group.ToString());
            for (int i = 1; i < group + 1; i++) //Create datatable for each family
            {
                GlobalData.ProteoformFamilies.Tables.Add(new DataTable());
                //      PF.Tables[i - 1].Columns.Add("Aggregated Mass", typeof(double));
                //    PF.Tables[i - 1].Columns.Add("Lysine Count", typeof(int));
                //  PF.Tables[i - 1].Columns.Add("Accession", typeof(string));
                //DataTable dt = new DataTable();
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Accession", typeof(string));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Name", typeof(string));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Fragment", typeof(string));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("PTM List", typeof(string));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Proteoform Mass", typeof(double));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Aggregated Mass", typeof(double));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Aggregated Intensity", typeof(double));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Aggregated Retention Time", typeof(double));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Lysine Count", typeof(int));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Delta Mass", typeof(double));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Running Sum", typeof(int));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Peak Center Count", typeof(int));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Peak Center Mass", typeof(double));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Out of Range Decimal", typeof(bool));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Acceptable Peak", typeof(bool));
                GlobalData.ProteoformFamilies.Tables[i - 1].Columns.Add("Group_#", typeof(int));
                //MessageBox.Show("1");

                DataRow[] dr = GlobalData.experimentTheoreticalPairs.Select("[Group_#]=" + i.ToString()); //get members of family #i
                //fill datatable of contents
                DataRow content = GlobalData.ProteoformFamilyMetrics.NewRow();
                content["Group_#"] = Convert.ToInt32(i);
                content["#_Exp"] = dr.Count();
                content["Lysine Count"] = dr[0]["Lysine Count"];
                foreach (DataRow dr2 in dr)
                {
                    GlobalData.ProteoformFamilies.Tables[i - 1].Rows.Add(dr2.ItemArray); //fill datatable with all members of family //ARG HERE BE THE SOURCE OF THEE ERRER
                }
                var distinctIds = dr.AsEnumerable()
                    .Select(s => new {
                        id = s.Field<string>("Accession"),
                    })
                    .Distinct().ToList();
                content["#_IDs"] = distinctIds.Count;
                GlobalData.ProteoformFamilyMetrics.Rows.Add(content);
            }
            //MessageBox.Show("HelloWorld");
            dataGridView1.DataSource = GlobalData.ProteoformFamilyMetrics;
            //dataGridView3.DataSource = PF.Tables[3];
        }
    }
}