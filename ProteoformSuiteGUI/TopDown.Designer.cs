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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgv_TD_proteoforms = new System.Windows.Forms.DataGridView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.rtb_sequence = new System.Windows.Forms.RichTextBox();
            this.dgv_TD_family = new System.Windows.Forms.DataGridView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.bt_check_fragmented_e = new System.Windows.Forms.Button();
            this.bt_targeted_td_relations = new System.Windows.Forms.Button();
            this.cmbx_td_or_e_proteoforms = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_exp_proteoforms = new System.Windows.Forms.TextBox();
            this.bt_load_td = new System.Windows.Forms.Button();
            this.bt_td_relations = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_tdProteoforms = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_proteoforms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_family)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
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
            this.splitContainer1.Panel1.Controls.Add(this.dgv_TD_proteoforms);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(976, 545);
            this.splitContainer1.SplitterDistance = 469;
            this.splitContainer1.TabIndex = 1;
            // 
            // dgv_TD_proteoforms
            // 
            this.dgv_TD_proteoforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_TD_proteoforms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_TD_proteoforms.Location = new System.Drawing.Point(0, 0);
            this.dgv_TD_proteoforms.Name = "dgv_TD_proteoforms";
            this.dgv_TD_proteoforms.Size = new System.Drawing.Size(469, 545);
            this.dgv_TD_proteoforms.TabIndex = 0;
            this.dgv_TD_proteoforms.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_TD_proteoforms_CellContentClick);
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
            this.splitContainer2.Size = new System.Drawing.Size(503, 545);
            this.splitContainer2.SplitterDistance = 183;
            this.splitContainer2.TabIndex = 0;
            // 
            // rtb_sequence
            // 
            this.rtb_sequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_sequence.Location = new System.Drawing.Point(0, 0);
            this.rtb_sequence.Name = "rtb_sequence";
            this.rtb_sequence.ReadOnly = true;
            this.rtb_sequence.Size = new System.Drawing.Size(503, 183);
            this.rtb_sequence.TabIndex = 1;
            this.rtb_sequence.Text = "";
            // 
            // dgv_TD_family
            // 
            this.dgv_TD_family.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_TD_family.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_TD_family.Location = new System.Drawing.Point(0, 0);
            this.dgv_TD_family.Name = "dgv_TD_family";
            this.dgv_TD_family.Size = new System.Drawing.Size(503, 358);
            this.dgv_TD_family.TabIndex = 0;
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
            this.splitContainer3.Panel1.Controls.Add(this.bt_check_fragmented_e);
            this.splitContainer3.Panel1.Controls.Add(this.bt_targeted_td_relations);
            this.splitContainer3.Panel1.Controls.Add(this.cmbx_td_or_e_proteoforms);
            this.splitContainer3.Panel1.Controls.Add(this.label2);
            this.splitContainer3.Panel1.Controls.Add(this.tb_exp_proteoforms);
            this.splitContainer3.Panel1.Controls.Add(this.bt_load_td);
            this.splitContainer3.Panel1.Controls.Add(this.bt_td_relations);
            this.splitContainer3.Panel1.Controls.Add(this.label1);
            this.splitContainer3.Panel1.Controls.Add(this.tb_tdProteoforms);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer3.Size = new System.Drawing.Size(976, 622);
            this.splitContainer3.SplitterDistance = 73;
            this.splitContainer3.TabIndex = 2;
            // 
            // bt_check_fragmented_e
            // 
            this.bt_check_fragmented_e.Enabled = false;
            this.bt_check_fragmented_e.Location = new System.Drawing.Point(498, 1);
            this.bt_check_fragmented_e.Name = "bt_check_fragmented_e";
            this.bt_check_fragmented_e.Size = new System.Drawing.Size(208, 23);
            this.bt_check_fragmented_e.TabIndex = 8;
            this.bt_check_fragmented_e.Text = "Check if Experimental Fragmented";
            this.bt_check_fragmented_e.UseVisualStyleBackColor = true;
            this.bt_check_fragmented_e.Click += new System.EventHandler(this.bt_check_fragmented_e_Click);
            // 
            // bt_targeted_td_relations
            // 
            this.bt_targeted_td_relations.Enabled = false;
            this.bt_targeted_td_relations.Location = new System.Drawing.Point(498, 32);
            this.bt_targeted_td_relations.Name = "bt_targeted_td_relations";
            this.bt_targeted_td_relations.Size = new System.Drawing.Size(208, 23);
            this.bt_targeted_td_relations.TabIndex = 7;
            this.bt_targeted_td_relations.Text = "Check Targeted TopDown Validation";
            this.bt_targeted_td_relations.UseVisualStyleBackColor = true;
            this.bt_targeted_td_relations.Click += new System.EventHandler(this.bt_targeted_td_relations_Click);
            // 
            // cmbx_td_or_e_proteoforms
            // 
            this.cmbx_td_or_e_proteoforms.FormattingEnabled = true;
            this.cmbx_td_or_e_proteoforms.Location = new System.Drawing.Point(12, 50);
            this.cmbx_td_or_e_proteoforms.Name = "cmbx_td_or_e_proteoforms";
            this.cmbx_td_or_e_proteoforms.Size = new System.Drawing.Size(121, 21);
            this.cmbx_td_or_e_proteoforms.TabIndex = 6;
            this.cmbx_td_or_e_proteoforms.SelectedIndexChanged += new System.EventHandler(this.cmbx_td_or_e_proteoforms_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(119, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(126, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Experimental Proteoforms";
            // 
            // tb_exp_proteoforms
            // 
            this.tb_exp_proteoforms.Location = new System.Drawing.Point(13, 26);
            this.tb_exp_proteoforms.Name = "tb_exp_proteoforms";
            this.tb_exp_proteoforms.Size = new System.Drawing.Size(100, 20);
            this.tb_exp_proteoforms.TabIndex = 4;
            // 
            // bt_load_td
            // 
            this.bt_load_td.Location = new System.Drawing.Point(275, 0);
            this.bt_load_td.Name = "bt_load_td";
            this.bt_load_td.Size = new System.Drawing.Size(185, 23);
            this.bt_load_td.TabIndex = 3;
            this.bt_load_td.Text = "Aggregate TopDown Proteoforms";
            this.bt_load_td.UseVisualStyleBackColor = true;
            this.bt_load_td.Click += new System.EventHandler(this.bt_load_td_Click);
            // 
            // bt_td_relations
            // 
            this.bt_td_relations.Location = new System.Drawing.Point(275, 32);
            this.bt_td_relations.Name = "bt_td_relations";
            this.bt_td_relations.Size = new System.Drawing.Size(190, 23);
            this.bt_td_relations.TabIndex = 2;
            this.bt_td_relations.Text = "Make TopDown Comparisons";
            this.bt_td_relations.UseVisualStyleBackColor = true;
            this.bt_td_relations.Click += new System.EventHandler(this.bt_td_relations_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(119, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Topdown Proteoforms";
            // 
            // tb_tdProteoforms
            // 
            this.tb_tdProteoforms.Location = new System.Drawing.Point(13, 3);
            this.tb_tdProteoforms.Name = "tb_tdProteoforms";
            this.tb_tdProteoforms.Size = new System.Drawing.Size(100, 20);
            this.tb_tdProteoforms.TabIndex = 0;
            // 
            // TopDown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(976, 622);
            this.Controls.Add(this.splitContainer3);
            this.Name = "TopDown";
            this.Text = "TopDown";
            this.Load += new System.EventHandler(this.TopDown_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_proteoforms)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_family)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgv_TD_proteoforms;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox rtb_sequence;
        private System.Windows.Forms.DataGridView dgv_TD_family;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_tdProteoforms;
        private System.Windows.Forms.Button bt_td_relations;
        private System.Windows.Forms.Button bt_load_td;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_exp_proteoforms;
        private System.Windows.Forms.ComboBox cmbx_td_or_e_proteoforms;
        private System.Windows.Forms.Button bt_targeted_td_relations;
        private System.Windows.Forms.Button bt_check_fragmented_e;
    }
}