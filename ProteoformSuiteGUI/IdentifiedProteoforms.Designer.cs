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
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.dgv_same_topdown_id = new System.Windows.Forms.DataGridView();
            this.dgv_other_topdown_ids = new System.Windows.Forms.DataGridView();
            this.dgv_bottom_up_peptides = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identified_experimentals)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_same_topdown_id)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_other_topdown_ids)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_bottom_up_peptides)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.dgv_identified_experimentals);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1287, 659);
            this.splitContainer1.SplitterDistance = 493;
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
            this.dgv_identified_experimentals.Size = new System.Drawing.Size(493, 659);
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
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgv_bottom_up_peptides);
            this.splitContainer2.Size = new System.Drawing.Size(790, 659);
            this.splitContainer2.SplitterDistance = 432;
            this.splitContainer2.TabIndex = 0;
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
            this.splitContainer3.Panel1.Controls.Add(this.dgv_same_topdown_id);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.dgv_other_topdown_ids);
            this.splitContainer3.Size = new System.Drawing.Size(790, 432);
            this.splitContainer3.SplitterDistance = 237;
            this.splitContainer3.TabIndex = 0;
            // 
            // dgv_same_topdown_id
            // 
            this.dgv_same_topdown_id.AllowUserToAddRows = false;
            this.dgv_same_topdown_id.AllowUserToDeleteRows = false;
            this.dgv_same_topdown_id.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_same_topdown_id.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_same_topdown_id.Location = new System.Drawing.Point(0, 0);
            this.dgv_same_topdown_id.Name = "dgv_same_topdown_id";
            this.dgv_same_topdown_id.Size = new System.Drawing.Size(790, 237);
            this.dgv_same_topdown_id.TabIndex = 0;
            // 
            // dgv_other_topdown_ids
            // 
            this.dgv_other_topdown_ids.AllowUserToAddRows = false;
            this.dgv_other_topdown_ids.AllowUserToDeleteRows = false;
            this.dgv_other_topdown_ids.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_other_topdown_ids.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_other_topdown_ids.Location = new System.Drawing.Point(0, 0);
            this.dgv_other_topdown_ids.Name = "dgv_other_topdown_ids";
            this.dgv_other_topdown_ids.Size = new System.Drawing.Size(790, 191);
            this.dgv_other_topdown_ids.TabIndex = 0;
            // 
            // dgv_bottom_up_peptides
            // 
            this.dgv_bottom_up_peptides.AllowUserToAddRows = false;
            this.dgv_bottom_up_peptides.AllowUserToDeleteRows = false;
            this.dgv_bottom_up_peptides.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_bottom_up_peptides.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_bottom_up_peptides.Location = new System.Drawing.Point(0, 0);
            this.dgv_bottom_up_peptides.Name = "dgv_bottom_up_peptides";
            this.dgv_bottom_up_peptides.Size = new System.Drawing.Size(790, 223);
            this.dgv_bottom_up_peptides.TabIndex = 0;
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
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identified_experimentals)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_same_topdown_id)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_other_topdown_ids)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_bottom_up_peptides)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgv_identified_experimentals;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgv_same_topdown_id;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView dgv_other_topdown_ids;
        private System.Windows.Forms.DataGridView dgv_bottom_up_peptides;
    }
}