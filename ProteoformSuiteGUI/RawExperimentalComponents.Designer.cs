using System.Windows.Forms;

namespace ProteoformSuite
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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dgv_RawQuantComp_MI_masses = new System.Windows.Forms.DataGridView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.dgv_RawQuantComp_IndChgSts = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpComp_MI_masses)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpComp_IndChgSts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawQuantComp_MI_masses)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawQuantComp_IndChgSts)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(1200, 670);
            this.splitContainer1.SplitterDistance = 253;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // dgv_RawExpComp_MI_masses
            // 
            this.dgv_RawExpComp_MI_masses.AllowUserToOrderColumns = true;
            this.dgv_RawExpComp_MI_masses.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.dgv_RawExpComp_MI_masses.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
            this.dgv_RawExpComp_MI_masses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_RawExpComp_MI_masses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_RawExpComp_MI_masses.Location = new System.Drawing.Point(0, 0);
            this.dgv_RawExpComp_MI_masses.Name = "dgv_RawExpComp_MI_masses";
            this.dgv_RawExpComp_MI_masses.RowTemplate.Height = 28;
            this.dgv_RawExpComp_MI_masses.Size = new System.Drawing.Size(398, 249);
            this.dgv_RawExpComp_MI_masses.TabIndex = 0;
            this.dgv_RawExpComp_MI_masses.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_RawExpComp_MI_masses_CellContentClick);
            // 
            // dgv_RawExpComp_IndChgSts
            // 
            this.dgv_RawExpComp_IndChgSts.AllowUserToOrderColumns = true;
            this.dgv_RawExpComp_IndChgSts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_RawExpComp_IndChgSts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_RawExpComp_IndChgSts.Location = new System.Drawing.Point(0, 0);
            this.dgv_RawExpComp_IndChgSts.Name = "dgv_RawExpComp_IndChgSts";
            this.dgv_RawExpComp_IndChgSts.RowTemplate.Height = 28;
            this.dgv_RawExpComp_IndChgSts.Size = new System.Drawing.Size(398, 408);
            this.dgv_RawExpComp_IndChgSts.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dgv_RawExpComp_MI_masses);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgv_RawQuantComp_MI_masses);
            this.splitContainer2.Size = new System.Drawing.Size(1196, 249);
            this.splitContainer2.SplitterDistance = 398;
            this.splitContainer2.TabIndex = 0;
            // 
            // dgv_RawQuantComp_MI_masses
            // 
            this.dgv_RawQuantComp_MI_masses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_RawQuantComp_MI_masses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_RawQuantComp_MI_masses.Location = new System.Drawing.Point(0, 0);
            this.dgv_RawQuantComp_MI_masses.Name = "dgv_RawQuantComp_MI_masses";
            this.dgv_RawQuantComp_MI_masses.RowTemplate.Height = 28;
            this.dgv_RawQuantComp_MI_masses.Size = new System.Drawing.Size(794, 249);
            this.dgv_RawQuantComp_MI_masses.TabIndex = 0;
            this.dgv_RawQuantComp_MI_masses.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_RawQuantComp_MI_masses_CellContentClick);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.dgv_RawExpComp_IndChgSts);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.dgv_RawQuantComp_IndChgSts);
            this.splitContainer3.Size = new System.Drawing.Size(1196, 408);
            this.splitContainer3.SplitterDistance = 398;
            this.splitContainer3.TabIndex = 0;
            // 
            // dgv_RawQuantComp_IndChgSts
            // 
            this.dgv_RawQuantComp_IndChgSts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_RawQuantComp_IndChgSts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_RawQuantComp_IndChgSts.Location = new System.Drawing.Point(0, 0);
            this.dgv_RawQuantComp_IndChgSts.Name = "dgv_RawQuantComp_IndChgSts";
            this.dgv_RawQuantComp_IndChgSts.RowTemplate.Height = 28;
            this.dgv_RawQuantComp_IndChgSts.Size = new System.Drawing.Size(794, 408);
            this.dgv_RawQuantComp_IndChgSts.TabIndex = 0;
            // 
            // RawExperimentalComponents
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 670);
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
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawQuantComp_MI_masses)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawQuantComp_IndChgSts)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgv_RawExpComp_IndChgSts;
        private System.Windows.Forms.DataGridView dgv_RawExpComp_MI_masses;
        private SplitContainer splitContainer2;
        private DataGridView dgv_RawQuantComp_MI_masses;
        private SplitContainer splitContainer3;
        private DataGridView dgv_RawQuantComp_IndChgSts;
    }
}