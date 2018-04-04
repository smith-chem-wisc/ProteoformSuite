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
            this.rtb_sequence = new System.Windows.Forms.RichTextBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.nUD_td_rt_tolerance = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_unique_PFRs = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tb_td_hits = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_tableFilter = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cb_biomarker = new System.Windows.Forms.CheckBox();
            this.cb_tight_abs_mass = new System.Windows.Forms.CheckBox();
            this.nUD_min_score_td = new System.Windows.Forms.NumericUpDown();
            this.bt_td_relations = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_tdProteoforms = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_proteoforms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_td_rt_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_score_td)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.dgv_TD_proteoforms);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rtb_sequence);
            this.splitContainer1.Size = new System.Drawing.Size(1021, 629);
            this.splitContainer1.SplitterDistance = 491;
            this.splitContainer1.TabIndex = 1;
            // 
            // dgv_TD_proteoforms
            // 
            this.dgv_TD_proteoforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_TD_proteoforms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_TD_proteoforms.Location = new System.Drawing.Point(0, 0);
            this.dgv_TD_proteoforms.Name = "dgv_TD_proteoforms";
            this.dgv_TD_proteoforms.Size = new System.Drawing.Size(1021, 491);
            this.dgv_TD_proteoforms.TabIndex = 0;
            this.dgv_TD_proteoforms.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_TD_proteoforms_CellContentClick);
            // 
            // rtb_sequence
            // 
            this.rtb_sequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_sequence.Location = new System.Drawing.Point(0, 0);
            this.rtb_sequence.Name = "rtb_sequence";
            this.rtb_sequence.ReadOnly = true;
            this.rtb_sequence.Size = new System.Drawing.Size(1021, 134);
            this.rtb_sequence.TabIndex = 2;
            this.rtb_sequence.Text = "";
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
            this.splitContainer3.Panel1.Controls.Add(this.label3);
            this.splitContainer3.Panel1.Controls.Add(this.nUD_td_rt_tolerance);
            this.splitContainer3.Panel1.Controls.Add(this.label6);
            this.splitContainer3.Panel1.Controls.Add(this.tb_unique_PFRs);
            this.splitContainer3.Panel1.Controls.Add(this.label4);
            this.splitContainer3.Panel1.Controls.Add(this.tb_td_hits);
            this.splitContainer3.Panel1.Controls.Add(this.label2);
            this.splitContainer3.Panel1.Controls.Add(this.tb_tableFilter);
            this.splitContainer3.Panel1.Controls.Add(this.label5);
            this.splitContainer3.Panel1.Controls.Add(this.cb_biomarker);
            this.splitContainer3.Panel1.Controls.Add(this.cb_tight_abs_mass);
            this.splitContainer3.Panel1.Controls.Add(this.nUD_min_score_td);
            this.splitContainer3.Panel1.Controls.Add(this.bt_td_relations);
            this.splitContainer3.Panel1.Controls.Add(this.label1);
            this.splitContainer3.Panel1.Controls.Add(this.tb_tdProteoforms);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer1);
            this.splitContainer3.Size = new System.Drawing.Size(1021, 742);
            this.splitContainer3.SplitterDistance = 109;
            this.splitContainer3.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(271, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 13);
            this.label3.TabIndex = 24;
            this.label3.Text = "Ret. Time Tolerance (min)";
            // 
            // nUD_td_rt_tolerance
            // 
            this.nUD_td_rt_tolerance.DecimalPlaces = 2;
            this.nUD_td_rt_tolerance.Location = new System.Drawing.Point(402, 31);
            this.nUD_td_rt_tolerance.Name = "nUD_td_rt_tolerance";
            this.nUD_td_rt_tolerance.Size = new System.Drawing.Size(75, 20);
            this.nUD_td_rt_tolerance.TabIndex = 23;
            this.nUD_td_rt_tolerance.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nUD_td_rt_tolerance.ValueChanged += new System.EventHandler(this.nUD_td_rt_tolerance_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(529, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "Top-Down Hits";
            // 
            // tb_unique_PFRs
            // 
            this.tb_unique_PFRs.Location = new System.Drawing.Point(623, 28);
            this.tb_unique_PFRs.Name = "tb_unique_PFRs";
            this.tb_unique_PFRs.ReadOnly = true;
            this.tb_unique_PFRs.Size = new System.Drawing.Size(100, 20);
            this.tb_unique_PFRs.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(529, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "Unique PFRs";
            // 
            // tb_td_hits
            // 
            this.tb_td_hits.Location = new System.Drawing.Point(623, 4);
            this.tb_td_hits.Name = "tb_td_hits";
            this.tb_td_hits.ReadOnly = true;
            this.tb_td_hits.Size = new System.Drawing.Size(100, 20);
            this.tb_td_hits.TabIndex = 19;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(529, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Table Filter";
            // 
            // tb_tableFilter
            // 
            this.tb_tableFilter.Location = new System.Drawing.Point(594, 77);
            this.tb_tableFilter.Name = "tb_tableFilter";
            this.tb_tableFilter.Size = new System.Drawing.Size(129, 20);
            this.tb_tableFilter.TabIndex = 15;
            this.tb_tableFilter.TextChanged += new System.EventHandler(this.tb_tableFilter_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(271, 11);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Min. C-Score";
            // 
            // cb_biomarker
            // 
            this.cb_biomarker.AutoSize = true;
            this.cb_biomarker.Checked = true;
            this.cb_biomarker.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_biomarker.Location = new System.Drawing.Point(274, 68);
            this.cb_biomarker.Name = "cb_biomarker";
            this.cb_biomarker.Size = new System.Drawing.Size(73, 17);
            this.cb_biomarker.TabIndex = 11;
            this.cb_biomarker.Text = "Biomarker";
            this.cb_biomarker.UseVisualStyleBackColor = true;
            this.cb_biomarker.CheckedChanged += new System.EventHandler(this.cb_biomarker_CheckedChanged);
            // 
            // cb_tight_abs_mass
            // 
            this.cb_tight_abs_mass.AutoSize = true;
            this.cb_tight_abs_mass.Checked = true;
            this.cb_tight_abs_mass.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_tight_abs_mass.Location = new System.Drawing.Point(274, 48);
            this.cb_tight_abs_mass.Name = "cb_tight_abs_mass";
            this.cb_tight_abs_mass.Size = new System.Drawing.Size(122, 17);
            this.cb_tight_abs_mass.TabIndex = 10;
            this.cb_tight_abs_mass.Text = "Tight Absolute Mass";
            this.cb_tight_abs_mass.UseVisualStyleBackColor = true;
            this.cb_tight_abs_mass.CheckedChanged += new System.EventHandler(this.cb_tight_abs_mass_CheckedChanged);
            // 
            // nUD_min_score_td
            // 
            this.nUD_min_score_td.DecimalPlaces = 1;
            this.nUD_min_score_td.Location = new System.Drawing.Point(402, 9);
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
            // bt_td_relations
            // 
            this.bt_td_relations.Location = new System.Drawing.Point(26, 12);
            this.bt_td_relations.Name = "bt_td_relations";
            this.bt_td_relations.Size = new System.Drawing.Size(178, 25);
            this.bt_td_relations.TabIndex = 2;
            this.bt_td_relations.Text = "Aggregate Top-Down Proteoforms";
            this.bt_td_relations.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.bt_td_relations.UseVisualStyleBackColor = true;
            this.bt_td_relations.Click += new System.EventHandler(this.bt_td_relations_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(506, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Top-Down Proteoforms";
            // 
            // tb_tdProteoforms
            // 
            this.tb_tdProteoforms.Location = new System.Drawing.Point(623, 51);
            this.tb_tdProteoforms.Name = "tb_tdProteoforms";
            this.tb_tdProteoforms.ReadOnly = true;
            this.tb_tdProteoforms.Size = new System.Drawing.Size(100, 20);
            this.tb_tdProteoforms.TabIndex = 0;
            // 
            // TopDown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1021, 742);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer3);
            this.Name = "TopDown";
            this.Text = "TopDown";
            this.Load += new System.EventHandler(this.TopDown_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_TD_proteoforms)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nUD_td_rt_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_score_td)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgv_TD_proteoforms;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_tdProteoforms;
        private System.Windows.Forms.Button bt_td_relations;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox cb_biomarker;
        private System.Windows.Forms.CheckBox cb_tight_abs_mass;
        private System.Windows.Forms.NumericUpDown nUD_min_score_td;
        private System.Windows.Forms.TextBox tb_tableFilter;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox rtb_sequence;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_unique_PFRs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tb_td_hits;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nUD_td_rt_tolerance;
    }
}