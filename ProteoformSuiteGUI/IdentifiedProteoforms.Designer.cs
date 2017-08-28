namespace ProteoformSuiteGUI
{
    partial class IdentifiedProteoforms
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IdentifiedProteoforms));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgv_identified_experimentals = new System.Windows.Forms.DataGridView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tb_bottom_up = new System.Windows.Forms.TextBox();
            this.dgv_bottom_up_peptides = new System.Windows.Forms.DataGridView();
            this.tb_topdown = new System.Windows.Forms.TextBox();
            this.tb_not_td = new System.Windows.Forms.TextBox();
            this.dgv_td_proteoforms = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identified_experimentals)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_bottom_up_peptides)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_td_proteoforms)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tb_not_td);
            this.splitContainer1.Panel1.Controls.Add(this.dgv_identified_experimentals);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1287, 659);
            this.splitContainer1.SplitterDistance = 266;
            this.splitContainer1.TabIndex = 0;
            // 
            // dgv_identified_experimentals
            // 
            this.dgv_identified_experimentals.AllowUserToAddRows = false;
            this.dgv_identified_experimentals.AllowUserToDeleteRows = false;
            this.dgv_identified_experimentals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_identified_experimentals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_identified_experimentals.Location = new System.Drawing.Point(0, 0);
            this.dgv_identified_experimentals.Name = "dgv_identified_experimentals";
            this.dgv_identified_experimentals.Size = new System.Drawing.Size(1287, 266);
            this.dgv_identified_experimentals.TabIndex = 0;
            this.dgv_identified_experimentals.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_identified_proteoforms_CellMouseClick);
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
            this.splitContainer2.Panel1.Controls.Add(this.dgv_td_proteoforms);
            this.splitContainer2.Panel1.Controls.Add(this.tb_topdown);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tb_bottom_up);
            this.splitContainer2.Panel2.Controls.Add(this.dgv_bottom_up_peptides);
            this.splitContainer2.Size = new System.Drawing.Size(1287, 389);
            this.splitContainer2.SplitterDistance = 265;
            this.splitContainer2.TabIndex = 0;
            // 
            // tb_bottom_up
            // 
            this.tb_bottom_up.Dock = System.Windows.Forms.DockStyle.Top;
            this.tb_bottom_up.Location = new System.Drawing.Point(0, 0);
            this.tb_bottom_up.Name = "tb_bottom_up";
            this.tb_bottom_up.ReadOnly = true;
            this.tb_bottom_up.Size = new System.Drawing.Size(1287, 20);
            this.tb_bottom_up.TabIndex = 3;
            // 
            // dgv_bottom_up_peptides
            // 
            this.dgv_bottom_up_peptides.AllowUserToAddRows = false;
            this.dgv_bottom_up_peptides.AllowUserToDeleteRows = false;
            this.dgv_bottom_up_peptides.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_bottom_up_peptides.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_bottom_up_peptides.Location = new System.Drawing.Point(0, 0);
            this.dgv_bottom_up_peptides.Name = "dgv_bottom_up_peptides";
            this.dgv_bottom_up_peptides.Size = new System.Drawing.Size(1287, 120);
            this.dgv_bottom_up_peptides.TabIndex = 0;
            // 
            // tb_topdown
            // 
            this.tb_topdown.Dock = System.Windows.Forms.DockStyle.Top;
            this.tb_topdown.Location = new System.Drawing.Point(0, 0);
            this.tb_topdown.Name = "tb_topdown";
            this.tb_topdown.ReadOnly = true;
            this.tb_topdown.Size = new System.Drawing.Size(1287, 20);
            this.tb_topdown.TabIndex = 4;
            // 
            // tb_not_td
            // 
            this.tb_not_td.Dock = System.Windows.Forms.DockStyle.Top;
            this.tb_not_td.Location = new System.Drawing.Point(0, 0);
            this.tb_not_td.Name = "tb_not_td";
            this.tb_not_td.ReadOnly = true;
            this.tb_not_td.Size = new System.Drawing.Size(1287, 20);
            this.tb_not_td.TabIndex = 4;
            // 
            // dgv_td_proteoforms
            // 
            this.dgv_td_proteoforms.AllowUserToAddRows = false;
            this.dgv_td_proteoforms.AllowUserToDeleteRows = false;
            this.dgv_td_proteoforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_td_proteoforms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_td_proteoforms.Location = new System.Drawing.Point(0, 20);
            this.dgv_td_proteoforms.Name = "dgv_td_proteoforms";
            this.dgv_td_proteoforms.Size = new System.Drawing.Size(1287, 245);
            this.dgv_td_proteoforms.TabIndex = 5;
            this.dgv_td_proteoforms.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_topdown_proteoforms_CellMouseClick);
            // 
            // IdentifiedProteoforms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1287, 659);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IdentifiedProteoforms";
            this.Text = "Identified Proteoforms";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identified_experimentals)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_bottom_up_peptides)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_td_proteoforms)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgv_identified_experimentals;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgv_bottom_up_peptides;
        private System.Windows.Forms.TextBox tb_bottom_up;
        private System.Windows.Forms.TextBox tb_not_td;
        private System.Windows.Forms.TextBox tb_topdown;
        private System.Windows.Forms.DataGridView dgv_td_proteoforms;
    }
}