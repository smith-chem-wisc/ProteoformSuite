using System.Windows.Forms;

namespace PS_0._00
{
    partial class RawExperimentalComponents
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgv_RawExpComp_MI_masses = new System.Windows.Forms.DataGridView();
            this.dgv_RawExpComp_IndChgSts = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpComp_MI_masses)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpComp_IndChgSts)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgv_RawExpComp_MI_masses);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgv_RawExpComp_IndChgSts);
            this.splitContainer1.Size = new System.Drawing.Size(1076, 565);
            this.splitContainer1.SplitterDistance = 285;
            this.splitContainer1.TabIndex = 0;
            // 
            // dgv_RawExpProt_MI_masses
            // 
            this.dgv_RawExpComp_MI_masses.AllowUserToOrderColumns = true;
            this.dgv_RawExpComp_MI_masses.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.dgv_RawExpComp_MI_masses.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
            this.dgv_RawExpComp_MI_masses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_RawExpComp_MI_masses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_RawExpComp_MI_masses.Location = new System.Drawing.Point(0, 0);
            this.dgv_RawExpComp_MI_masses.Name = "dgv_RawExpComp_MI_masses";
            this.dgv_RawExpComp_MI_masses.RowTemplate.Height = 28;
            this.dgv_RawExpComp_MI_masses.Size = new System.Drawing.Size(1072, 281);
            this.dgv_RawExpComp_MI_masses.TabIndex = 0;
            this.dgv_RawExpComp_MI_masses.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_RawExpComp_MI_masses_CellContentClick);
            // 
            // dgv_RawExpProt_IndChgSts
            // 
            this.dgv_RawExpComp_IndChgSts.AllowUserToOrderColumns = true;
            this.dgv_RawExpComp_IndChgSts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_RawExpComp_IndChgSts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_RawExpComp_IndChgSts.Location = new System.Drawing.Point(0, 0);
            this.dgv_RawExpComp_IndChgSts.Name = "dgv_RawExpComp_IndChgSts";
            this.dgv_RawExpComp_IndChgSts.RowTemplate.Height = 28;
            this.dgv_RawExpComp_IndChgSts.Size = new System.Drawing.Size(1072, 272);
            this.dgv_RawExpComp_IndChgSts.TabIndex = 0;
            // 
            // RawExperimentalProteoforms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1076, 565);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Name = "RawExperimentalComponents";
            this.Text = "Raw Experimental Components";
            this.Load += new System.EventHandler(this.RawExperimentalComponents_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpComp_MI_masses)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpComp_IndChgSts)).EndInit();
            this.ResumeLayout(false);

            if (GlobalData.repeat==true)
            {
                LoadDeconvolutionResults instance = new LoadDeconvolutionResults();
                instance.Show();
            }
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgv_RawExpComp_IndChgSts;
        private System.Windows.Forms.DataGridView dgv_RawExpComp_MI_masses;
    }
}