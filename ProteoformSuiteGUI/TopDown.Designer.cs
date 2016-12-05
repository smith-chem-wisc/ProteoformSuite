namespace ProteoformSuite
{
    partial class TopDown
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
            this.dgv_TD_proteoforms = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.rtb_sequence = new System.Windows.Forms.RichTextBox();
            this.dgv_TD_family = new System.Windows.Forms.DataGridView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_proteoforms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_family)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgv_TD_proteoforms
            // 
            this.dgv_TD_proteoforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_TD_proteoforms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_TD_proteoforms.Location = new System.Drawing.Point(0, 0);
            this.dgv_TD_proteoforms.Name = "dgv_TD_proteoforms";
            this.dgv_TD_proteoforms.Size = new System.Drawing.Size(323, 338);
            this.dgv_TD_proteoforms.TabIndex = 0;
            this.dgv_TD_proteoforms.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_TD_proteoforms_CellContentClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgv_TD_proteoforms);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(673, 338);
            this.splitContainer1.SplitterDistance = 323;
            this.splitContainer1.TabIndex = 1;
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
            this.splitContainer2.Panel1.Controls.Add(this.rtb_sequence);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgv_TD_family);
            this.splitContainer2.Size = new System.Drawing.Size(346, 338);
            this.splitContainer2.SplitterDistance = 114;
            this.splitContainer2.TabIndex = 0;
            // 
            // rtb_sequence
            // 
            this.rtb_sequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_sequence.Location = new System.Drawing.Point(0, 0);
            this.rtb_sequence.Name = "rtb_sequence";
            this.rtb_sequence.ReadOnly = true;
            this.rtb_sequence.Size = new System.Drawing.Size(346, 114);
            this.rtb_sequence.TabIndex = 1;
            this.rtb_sequence.Text = "";
            // 
            // dgv_TD_family
            // 
            this.dgv_TD_family.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_TD_family.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_TD_family.Location = new System.Drawing.Point(0, 0);
            this.dgv_TD_family.Name = "dgv_TD_family";
            this.dgv_TD_family.Size = new System.Drawing.Size(346, 220);
            this.dgv_TD_family.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer3.Size = new System.Drawing.Size(673, 382);
            this.splitContainer3.SplitterDistance = 40;
            this.splitContainer3.TabIndex = 2;
            // 
            // TopDown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 382);
            this.Controls.Add(this.splitContainer3);
            this.Name = "TopDown";
            this.Text = "TopDown";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_proteoforms)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_family)).EndInit();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_TD_proteoforms;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox rtb_sequence;
        private System.Windows.Forms.DataGridView dgv_TD_family;
        private System.Windows.Forms.SplitContainer splitContainer3;
    }
}