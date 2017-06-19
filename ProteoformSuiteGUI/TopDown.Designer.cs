namespace ProteoformSuiteGUI
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
            this.bt_targeted_td_relations = new System.Windows.Forms.Button();
            this.cmbx_td_or_e_proteoforms = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_exp_proteoforms = new System.Windows.Forms.TextBox();
            this.bt_td_relations = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_tdProteoforms = new System.Windows.Forms.TextBox();
            this.nUD_min_RT_td = new System.Windows.Forms.NumericUpDown();
            this.nUD_max_RT_td = new System.Windows.Forms.NumericUpDown();
            this.nUD_min_score_td = new System.Windows.Forms.NumericUpDown();
            this.cb_tight_abs_mass = new System.Windows.Forms.CheckBox();
            this.cb_biomarker = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_RT_td)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_RT_td)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_score_td)).BeginInit();
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
            this.splitContainer3.Panel1.Controls.Add(this.label5);
            this.splitContainer3.Panel1.Controls.Add(this.label4);
            this.splitContainer3.Panel1.Controls.Add(this.label3);
            this.splitContainer3.Panel1.Controls.Add(this.cb_biomarker);
            this.splitContainer3.Panel1.Controls.Add(this.cb_tight_abs_mass);
            this.splitContainer3.Panel1.Controls.Add(this.nUD_min_score_td);
            this.splitContainer3.Panel1.Controls.Add(this.nUD_max_RT_td);
            this.splitContainer3.Panel1.Controls.Add(this.nUD_min_RT_td);
            this.splitContainer3.Panel1.Controls.Add(this.bt_targeted_td_relations);
            this.splitContainer3.Panel1.Controls.Add(this.cmbx_td_or_e_proteoforms);
            this.splitContainer3.Panel1.Controls.Add(this.label2);
            this.splitContainer3.Panel1.Controls.Add(this.tb_exp_proteoforms);
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
            // bt_targeted_td_relations
            // 
            this.bt_targeted_td_relations.Enabled = false;
            this.bt_targeted_td_relations.Location = new System.Drawing.Point(314, 35);
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
            this.label2.Size = new System.Drawing.Size(156, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Intact Experimental Proteoforms";
            // 
            // tb_exp_proteoforms
            // 
            this.tb_exp_proteoforms.Location = new System.Drawing.Point(13, 26);
            this.tb_exp_proteoforms.Name = "tb_exp_proteoforms";
            this.tb_exp_proteoforms.Size = new System.Drawing.Size(100, 20);
            this.tb_exp_proteoforms.TabIndex = 4;
            // 
            // bt_td_relations
            // 
            this.bt_td_relations.Location = new System.Drawing.Point(314, 6);
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
            // nUD_min_RT_td
            // 
            this.nUD_min_RT_td.DecimalPlaces = 1;
            this.nUD_min_RT_td.Location = new System.Drawing.Point(565, 5);
            this.nUD_min_RT_td.Name = "nUD_min_RT_td";
            this.nUD_min_RT_td.Size = new System.Drawing.Size(75, 20);
            this.nUD_min_RT_td.TabIndex = 3;
            this.nUD_min_RT_td.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.nUD_min_RT_td.ValueChanged += new System.EventHandler(this.nUD_min_RT_td_ValueChanged);
            // 
            // nUD_max_RT_td
            // 
            this.nUD_max_RT_td.DecimalPlaces = 1;
            this.nUD_max_RT_td.Location = new System.Drawing.Point(565, 26);
            this.nUD_max_RT_td.Name = "nUD_max_RT_td";
            this.nUD_max_RT_td.Size = new System.Drawing.Size(75, 20);
            this.nUD_max_RT_td.TabIndex = 8;
            this.nUD_max_RT_td.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.nUD_max_RT_td.ValueChanged += new System.EventHandler(this.nUD_max_RT_td_ValueChanged);
            // 
            // nUD_min_score_td
            // 
            this.nUD_min_score_td.DecimalPlaces = 1;
            this.nUD_min_score_td.Location = new System.Drawing.Point(565, 50);
            this.nUD_min_score_td.Name = "nUD_min_score_td";
            this.nUD_min_score_td.Size = new System.Drawing.Size(75, 20);
            this.nUD_min_score_td.TabIndex = 9;
            this.nUD_min_score_td.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nUD_min_score_td.ValueChanged += new System.EventHandler(this.nUD_min_score_td_ValueChanged);
            // 
            // cb_tight_abs_mass
            // 
            this.cb_tight_abs_mass.AutoSize = true;
            this.cb_tight_abs_mass.Checked = true;
            this.cb_tight_abs_mass.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_tight_abs_mass.Location = new System.Drawing.Point(757, 7);
            this.cb_tight_abs_mass.Name = "cb_tight_abs_mass";
            this.cb_tight_abs_mass.Size = new System.Drawing.Size(122, 17);
            this.cb_tight_abs_mass.TabIndex = 10;
            this.cb_tight_abs_mass.Text = "Tight Absolute Mass";
            this.cb_tight_abs_mass.UseVisualStyleBackColor = true;
            this.cb_tight_abs_mass.CheckedChanged += new System.EventHandler(this.cb_tight_abs_mass_CheckedChanged);
            // 
            // cb_biomarker
            // 
            this.cb_biomarker.AutoSize = true;
            this.cb_biomarker.Checked = true;
            this.cb_biomarker.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_biomarker.Location = new System.Drawing.Point(757, 28);
            this.cb_biomarker.Name = "cb_biomarker";
            this.cb_biomarker.Size = new System.Drawing.Size(73, 17);
            this.cb_biomarker.TabIndex = 11;
            this.cb_biomarker.Text = "Biomarker";
            this.cb_biomarker.UseVisualStyleBackColor = true;
            this.cb_biomarker.CheckedChanged += new System.EventHandler(this.cb_biomarker_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(646, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Min. Retention Time";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(646, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Max. Retention Time";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(646, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Min. C-Score";
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
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_RT_td)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_RT_td)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_score_td)).EndInit();
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_exp_proteoforms;
        private System.Windows.Forms.ComboBox cmbx_td_or_e_proteoforms;
        private System.Windows.Forms.Button bt_targeted_td_relations;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cb_biomarker;
        private System.Windows.Forms.CheckBox cb_tight_abs_mass;
        private System.Windows.Forms.NumericUpDown nUD_min_score_td;
        private System.Windows.Forms.NumericUpDown nUD_max_RT_td;
        private System.Windows.Forms.NumericUpDown nUD_min_RT_td;
    }
}