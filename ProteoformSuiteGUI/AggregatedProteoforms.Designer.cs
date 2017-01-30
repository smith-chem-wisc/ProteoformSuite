namespace ProteoformSuite
{
    partial class AggregatedProteoforms
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
            this.dgv_AggregatedProteoforms = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.nUD_min_num_bioreps = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.nUD_min_num_CS = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.nUD_min_agg_count = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.nUD_rel_abundance = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.bt_aggregate = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_totalAggregatedProteoforms = new System.Windows.Forms.TextBox();
            this.nUD_Missed_Ks = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.nUD_Missed_Monos = new System.Windows.Forms.NumericUpDown();
            this.nUD_RetTimeToleranace = new System.Windows.Forms.NumericUpDown();
            this.nUP_mass_tolerance = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dgv_AcceptNeuCdLtProteoforms = new System.Windows.Forms.DataGridView();
            this.nUD_min_S_to_N = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AggregatedProteoforms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_bioreps)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_CS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_agg_count)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_rel_abundance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Ks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Monos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_RetTimeToleranace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUP_mass_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AcceptNeuCdLtProteoforms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_S_to_N)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_AggregatedProteoforms
            // 
            this.dgv_AggregatedProteoforms.AllowUserToOrderColumns = true;
            this.dgv_AggregatedProteoforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_AggregatedProteoforms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_AggregatedProteoforms.Location = new System.Drawing.Point(0, 0);
            this.dgv_AggregatedProteoforms.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.dgv_AggregatedProteoforms.Name = "dgv_AggregatedProteoforms";
            this.dgv_AggregatedProteoforms.RowTemplate.Height = 28;
            this.dgv_AggregatedProteoforms.Size = new System.Drawing.Size(633, 341);
            this.dgv_AggregatedProteoforms.TabIndex = 0;
            this.dgv_AggregatedProteoforms.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_AggregatedProteoforms_CellContentClick);
            this.dgv_AggregatedProteoforms.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_AggregatedProteoforms_CellMouseClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgv_AcceptNeuCdLtProteoforms);
            this.splitContainer1.Size = new System.Drawing.Size(1016, 680);
            this.splitContainer1.SplitterDistance = 345;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.label10);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_min_S_to_N);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_min_num_bioreps);
            this.splitContainer2.Panel1.Controls.Add(this.label9);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_min_num_CS);
            this.splitContainer2.Panel1.Controls.Add(this.label8);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_min_agg_count);
            this.splitContainer2.Panel1.Controls.Add(this.label7);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_rel_abundance);
            this.splitContainer2.Panel1.Controls.Add(this.label6);
            this.splitContainer2.Panel1.Controls.Add(this.bt_aggregate);
            this.splitContainer2.Panel1.Controls.Add(this.label5);
            this.splitContainer2.Panel1.Controls.Add(this.tb_totalAggregatedProteoforms);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_Missed_Ks);
            this.splitContainer2.Panel1.Controls.Add(this.label4);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_Missed_Monos);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_RetTimeToleranace);
            this.splitContainer2.Panel1.Controls.Add(this.nUP_mass_tolerance);
            this.splitContainer2.Panel1.Controls.Add(this.label3);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgv_AggregatedProteoforms);
            this.splitContainer2.Size = new System.Drawing.Size(1016, 345);
            this.splitContainer2.SplitterDistance = 376;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 0;
            // 
            // nUD_min_num_bioreps
            // 
            this.nUD_min_num_bioreps.Location = new System.Drawing.Point(170, 190);
            this.nUD_min_num_bioreps.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_min_num_bioreps.Name = "nUD_min_num_bioreps";
            this.nUD_min_num_bioreps.Size = new System.Drawing.Size(80, 20);
            this.nUD_min_num_bioreps.TabIndex = 18;
            this.nUD_min_num_bioreps.ValueChanged += new System.EventHandler(this.nUD_min_num_bioreps_ValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(47, 192);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Min. Number of Bioreps";
            // 
            // nUD_min_num_CS
            // 
            this.nUD_min_num_CS.Location = new System.Drawing.Point(170, 168);
            this.nUD_min_num_CS.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_min_num_CS.Name = "nUD_min_num_CS";
            this.nUD_min_num_CS.Size = new System.Drawing.Size(80, 20);
            this.nUD_min_num_CS.TabIndex = 16;
            this.nUD_min_num_CS.ValueChanged += new System.EventHandler(this.nUD_min_num_CS_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(47, 170);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(107, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Min. # Charge States";
            // 
            // nUD_min_agg_count
            // 
            this.nUD_min_agg_count.Location = new System.Drawing.Point(170, 212);
            this.nUD_min_agg_count.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_min_agg_count.Name = "nUD_min_agg_count";
            this.nUD_min_agg_count.Size = new System.Drawing.Size(80, 20);
            this.nUD_min_agg_count.TabIndex = 14;
            this.nUD_min_agg_count.ValueChanged += new System.EventHandler(this.nUD_min_agg_count_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(39, 214);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(116, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Min. Aggregated Count";
            // 
            // nUD_rel_abundance
            // 
            this.nUD_rel_abundance.DecimalPlaces = 3;
            this.nUD_rel_abundance.Location = new System.Drawing.Point(170, 146);
            this.nUD_rel_abundance.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_rel_abundance.Name = "nUD_rel_abundance";
            this.nUD_rel_abundance.Size = new System.Drawing.Size(80, 20);
            this.nUD_rel_abundance.TabIndex = 12;
            this.nUD_rel_abundance.ValueChanged += new System.EventHandler(this.nUD_rel_abundance_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(38, 148);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(127, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Min. Relative Abundance";
            // 
            // bt_aggregate
            // 
            this.bt_aggregate.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bt_aggregate.Location = new System.Drawing.Point(0, 318);
            this.bt_aggregate.Name = "bt_aggregate";
            this.bt_aggregate.Size = new System.Drawing.Size(372, 23);
            this.bt_aggregate.TabIndex = 2;
            this.bt_aggregate.Text = "Aggregate";
            this.bt_aggregate.UseVisualStyleBackColor = true;
            this.bt_aggregate.Click += new System.EventHandler(this.bt_aggregate_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 272);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(197, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Total Accepted Aggregated Proteoforms";
            // 
            // tb_totalAggregatedProteoforms
            // 
            this.tb_totalAggregatedProteoforms.Location = new System.Drawing.Point(220, 269);
            this.tb_totalAggregatedProteoforms.Margin = new System.Windows.Forms.Padding(2);
            this.tb_totalAggregatedProteoforms.Name = "tb_totalAggregatedProteoforms";
            this.tb_totalAggregatedProteoforms.Size = new System.Drawing.Size(81, 20);
            this.tb_totalAggregatedProteoforms.TabIndex = 8;
            // 
            // nUD_Missed_Ks
            // 
            this.nUD_Missed_Ks.Location = new System.Drawing.Point(170, 92);
            this.nUD_Missed_Ks.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_Missed_Ks.Name = "nUD_Missed_Ks";
            this.nUD_Missed_Ks.Size = new System.Drawing.Size(80, 20);
            this.nUD_Missed_Ks.TabIndex = 7;
            this.nUD_Missed_Ks.ValueChanged += new System.EventHandler(this.nUD_Missed_Ks_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 93);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Missed Lysine Counts (num)";
            // 
            // nUD_Missed_Monos
            // 
            this.nUD_Missed_Monos.Location = new System.Drawing.Point(170, 67);
            this.nUD_Missed_Monos.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_Missed_Monos.Name = "nUD_Missed_Monos";
            this.nUD_Missed_Monos.Size = new System.Drawing.Size(80, 20);
            this.nUD_Missed_Monos.TabIndex = 5;
            this.nUD_Missed_Monos.ValueChanged += new System.EventHandler(this.nUD_Missed_Monos_ValueChanged);
            // 
            // nUD_RetTimeToleranace
            // 
            this.nUD_RetTimeToleranace.DecimalPlaces = 2;
            this.nUD_RetTimeToleranace.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nUD_RetTimeToleranace.Location = new System.Drawing.Point(170, 44);
            this.nUD_RetTimeToleranace.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_RetTimeToleranace.Name = "nUD_RetTimeToleranace";
            this.nUD_RetTimeToleranace.Size = new System.Drawing.Size(80, 20);
            this.nUD_RetTimeToleranace.TabIndex = 4;
            this.nUD_RetTimeToleranace.ValueChanged += new System.EventHandler(this.nUD_RetTimeToleranace_ValueChanged);
            // 
            // nUP_mass_tolerance
            // 
            this.nUP_mass_tolerance.Location = new System.Drawing.Point(170, 20);
            this.nUP_mass_tolerance.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUP_mass_tolerance.Name = "nUP_mass_tolerance";
            this.nUP_mass_tolerance.Size = new System.Drawing.Size(80, 20);
            this.nUP_mass_tolerance.TabIndex = 3;
            this.nUP_mass_tolerance.ValueChanged += new System.EventHandler(this.nUP_mass_tolerance_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 68);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Missed Monoisotopics (num)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 45);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Ret. Time Tolerance (min)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(55, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mass Tolerance (ppm)";
            // 
            // dgv_AcceptNeuCdLtProteoforms
            // 
            this.dgv_AcceptNeuCdLtProteoforms.AllowUserToOrderColumns = true;
            this.dgv_AcceptNeuCdLtProteoforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_AcceptNeuCdLtProteoforms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_AcceptNeuCdLtProteoforms.Location = new System.Drawing.Point(0, 0);
            this.dgv_AcceptNeuCdLtProteoforms.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.dgv_AcceptNeuCdLtProteoforms.Name = "dgv_AcceptNeuCdLtProteoforms";
            this.dgv_AcceptNeuCdLtProteoforms.RowTemplate.Height = 28;
            this.dgv_AcceptNeuCdLtProteoforms.Size = new System.Drawing.Size(1012, 328);
            this.dgv_AcceptNeuCdLtProteoforms.TabIndex = 0;
            // 
            // nUD_min_S_to_N
            // 
            this.nUD_min_S_to_N.DecimalPlaces = 2;
            this.nUD_min_S_to_N.Location = new System.Drawing.Point(170, 234);
            this.nUD_min_S_to_N.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_min_S_to_N.Name = "nUD_min_S_to_N";
            this.nUD_min_S_to_N.Size = new System.Drawing.Size(80, 20);
            this.nUD_min_S_to_N.TabIndex = 19;
            this.nUD_min_S_to_N.ValueChanged += new System.EventHandler(this.nUD_min_S_to_N_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(47, 236);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(101, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "Min. Signal to Noise";
            // 
            // AggregatedProteoforms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 680);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "AggregatedProteoforms";
            this.Text = "AggregatedProteoforms";
            this.Load += new System.EventHandler(this.AggregatedProteoforms_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AggregatedProteoforms)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_bioreps)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_CS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_agg_count)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_rel_abundance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Ks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Monos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_RetTimeToleranace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUP_mass_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AcceptNeuCdLtProteoforms)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_S_to_N)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_AggregatedProteoforms;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.NumericUpDown nUD_Missed_Monos;
        private System.Windows.Forms.NumericUpDown nUD_RetTimeToleranace;
        private System.Windows.Forms.NumericUpDown nUP_mass_tolerance;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgv_AcceptNeuCdLtProteoforms;
        private System.Windows.Forms.NumericUpDown nUD_Missed_Ks;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_totalAggregatedProteoforms;
        private System.Windows.Forms.Button bt_aggregate;
        private System.Windows.Forms.NumericUpDown nUD_rel_abundance;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nUD_min_agg_count;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nUD_min_num_CS;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nUD_min_num_bioreps;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown nUD_min_S_to_N;
    }
}