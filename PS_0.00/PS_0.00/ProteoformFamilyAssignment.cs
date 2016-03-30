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
        public ProteoformFamilyAssignment()
        {
            InitializeComponent();
        }
        //public static DataTable EE = new DataTable();
        //public static DataTable ET = new DataTable();
       
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private DataGridView dataGridView1;
        private DataGridView dataGridView2;
        public static int group = 0;

        private void ProteoformFamilyAssignment_Load(object sender, EventArgs e)
        {
            //Create Parent Group of EE
            //DataTable dt = new DataTable();
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

            for (int i=0; i< GlobalData.experimentExperimentPairs.Rows.Count; i++)
            {
                int n = 0;
                for (int j=0; j< GlobalData.experimentExperimentPairs.Rows.Count; j++)
                {
                    if (GlobalData.experimentExperimentPairs.Rows[i]["Aggregated Mass Light"].Equals(GlobalData.experimentExperimentPairs.Rows[j]["Aggregated Mass Heavy"]) && GlobalData.experimentExperimentPairs.Rows[i]["Lysine Count"].Equals(GlobalData.experimentExperimentPairs.Rows[j]["Lysine Count"]))
                    {
                        n = n + 1;
                    } 
                }
                if (n.Equals(0)) // && (EE_Parent.Rows.Select("[Lys_Ct] =" EE.Rows[i]["Lys_Ct"])).Equals(0))
                {
                    GlobalData.EE_Parent.Rows.Add(GlobalData.experimentExperimentPairs.Rows[i].ItemArray);
                }
                //MessageBox.Show(n.ToString());
            }

            //remove parent repeats
            for (int i=0; i<GlobalData.EE_Parent.Rows.Count; i++)
            {
                for (int j=i+1; j< GlobalData.EE_Parent.Rows.Count; j++)
                {
                    if (GlobalData.EE_Parent.Rows[i]["Lysine Count"].Equals(GlobalData.EE_Parent.Rows[j]["Lysine Count"]) && GlobalData.EE_Parent.Rows[i]["Aggregated Mass Light"].Equals(GlobalData.EE_Parent.Rows[j]["Aggregated Mass Light"]))
                    {
                        GlobalData.EE_Parent.Rows[j].Delete();
                    }
                }
            }

            //Use Parent Group to assign group#s to ET
            //add group of zeroes
            GlobalData.experimentTheoreticalPairs.Columns.Add("Group_#", typeof(Int32));
            //int group = 0;
            for (int i=0; i<GlobalData.experimentTheoreticalPairs.Rows.Count; i++)
            {
                GlobalData.experimentTheoreticalPairs.Rows[i]["Group_#"] = group;
            }
            
            //label parents in ET
            foreach(DataRow parent in GlobalData.EE_Parent.Rows)
            {
                group = group + 1;
                //(ET.Rows.Select("[Lys_Ct] =" + parent["Lys_Ct"]+ "AND [Exp_Mass] =" + parent["Exp_Mass2"]))["Group_#"] = group;
                for (int a=0; a<GlobalData.experimentTheoreticalPairs.Rows.Count;a++)
                {
                    if (GlobalData.experimentTheoreticalPairs.Rows[a]["Lysine Count"].Equals(parent["Lysine Count"]) && GlobalData.experimentTheoreticalPairs.Rows[a]["Aggregated Mass"].Equals(parent["Aggregated Mass Light"])) //&& ET.Rows[a]["Group_#"].Equals(0))
                    {
                        GlobalData.experimentTheoreticalPairs.Rows[a]["Group_#"] = group;
                    }
                }
                Find_Children(Convert.ToDouble(parent["Aggregated Mass Heavy"]), Convert.ToInt32(parent["Lysine Count"]), e);

            }
            


            //Create dataTable of contents
            GlobalData.PF_Contents.Columns.Add("Group_#", typeof(int));
            GlobalData.PF_Contents.Columns.Add("#_Exp", typeof(int));
            GlobalData.PF_Contents.Columns.Add("Lysine Count", typeof(int));
            GlobalData.PF_Contents.Columns.Add("#_IDs", typeof(int));
            for (int i = 1; i < group+1; i++) //Create datatable for each family
            {
                GlobalData.PF.Tables.Add(new DataTable());
                //      PF.Tables[i - 1].Columns.Add("Aggregated Mass", typeof(double));
                //    PF.Tables[i - 1].Columns.Add("Lysine Count", typeof(int));
                //  PF.Tables[i - 1].Columns.Add("Accession", typeof(string));
                GlobalData.PF.Tables[i - 1].Columns.Add("Group_#", typeof(int));
                //DataTable dt = new DataTable();
                GlobalData.PF.Tables[i-1].Columns.Add("Accession", typeof(string));
                GlobalData.PF.Tables[i-1].Columns.Add("Name", typeof(string));
                GlobalData.PF.Tables[i-1].Columns.Add("Fragment", typeof(string));
                GlobalData.PF.Tables[i-1].Columns.Add("PTM List", typeof(string));
                GlobalData.PF.Tables[i-1].Columns.Add("Proteoform Mass", typeof(double));
                GlobalData.PF.Tables[i-1].Columns.Add("Aggregated Mass", typeof(double));
                GlobalData.PF.Tables[i-1].Columns.Add("Aggregated Intensity", typeof(double));
                GlobalData.PF.Tables[i-1].Columns.Add("Aggregated Retention Time", typeof(double));
                GlobalData.PF.Tables[i-1].Columns.Add("Lysine Count", typeof(int));
                GlobalData.PF.Tables[i-1].Columns.Add("Delta Mass", typeof(double));
                GlobalData.PF.Tables[i-1].Columns.Add("Running Sum", typeof(int));
                GlobalData.PF.Tables[i-1].Columns.Add("Peak Center Count", typeof(int));
                GlobalData.PF.Tables[i-1].Columns.Add("Peak Center Mass", typeof(double));
                GlobalData.PF.Tables[i-1].Columns.Add("Out of Range Decimal", typeof(bool));
                GlobalData.PF.Tables[i-1].Columns.Add("Acceptable Peak", typeof(bool));
                DataRow[] dr = GlobalData.experimentTheoreticalPairs.Select("[Group_#]=" + i.ToString()); //get members of family #i
                //fill datatable of contents
                DataRow content = GlobalData.PF_Contents.NewRow();
                content["Group_#"] = i;
                content["#_Exp"] = dr.Count();
                content["Lysine Count"] = dr[0]["Lysine Count"];            
                foreach (DataRow dr2 in dr)
                {
                    GlobalData.PF.Tables[i - 1].Rows.Add(dr2.ItemArray); //fill datatable with all members of family
                }
                var distinctIds = dr.AsEnumerable()
                    .Select(s => new {
                        id = s.Field<string>("Accession"),
                    })
                    .Distinct().ToList();
                content["#_IDs"] = distinctIds.Count;
                GlobalData.PF_Contents.Rows.Add(content);
            }

            dataGridView1.DataSource = GlobalData.PF_Contents;
            //dataGridView3.DataSource = PF.Tables[3];
        }
        //Everything has been made. Now put it in windows.





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
                    GlobalData.experimentTheoreticalPairs.Rows[a]["Group_#"] = group;
                }
            }
            //DataTable EE_Children = EE.Rows.Select("[Exp_Mass1] =" + mass + "AND [Lys_Ct] =" + lys);
            for (int a = 0; a < GlobalData.experimentExperimentPairs.Rows.Count; a++)
            {
                if (GlobalData.experimentExperimentPairs.Rows[a]["Lysine Count"].Equals(lys) && GlobalData.experimentExperimentPairs.Rows[a]["Aggregated Mass Light"].Equals(mass))
                {
                    EE_Children.Rows.Add(GlobalData.experimentExperimentPairs.Rows[a].ItemArray);
                }
            }
            foreach (DataRow Child in EE_Children.Rows)
            {
                //(ET.Rows.Select("[Lys_Ct] ="+ Child["Lys_Ct"]+ "AND [Exp_Mass] ="+ Child["Exp_Mass1"]))["Group_#"]=group;
                for (int a = 0; a < GlobalData.experimentTheoreticalPairs.Rows.Count; a++)
                {
                    if (GlobalData.experimentTheoreticalPairs.Rows[a]["Lysine Count"].Equals(Child["Lysine Count"]) && GlobalData.experimentTheoreticalPairs.Rows[a]["Aggregated Mass"].Equals(Child["Aggregated Mass Light"]))
                    {
                        GlobalData.experimentTheoreticalPairs.Rows[a]["Group_#"] = group;
                    }
                }
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
            this.ClientSize = new System.Drawing.Size(587, 456);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ProteoformFamilyAssignment";
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
                dataGridView2.DataSource = GlobalData.PF.Tables[groupnumber - 1];
            }
        }

        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}