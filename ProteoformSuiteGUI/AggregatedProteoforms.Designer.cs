﻿namespace ProteoformSuiteGUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AggregatedProteoforms));
            this.dgv_AggregatedProteoforms = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label9 = new System.Windows.Forms.Label();
            this.tb_tableFilter = new System.Windows.Forms.TextBox();
            this.cb_validateProteoforms = new System.Windows.Forms.CheckBox();
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb_displayLightQuantificationComponents = new System.Windows.Forms.RadioButton();
            this.rb_displayIdentificationComponents = new System.Windows.Forms.RadioButton();
            this.rb_displayHeavyQuantificationComponents = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.nUD_min_agg_count = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.nUD_min_num_CS = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.nUD_min_num_bioreps = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AggregatedProteoforms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Ks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Monos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_RetTimeToleranace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUP_mass_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AcceptNeuCdLtProteoforms)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_agg_count)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_CS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_bioreps)).BeginInit();
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
            this.dgv_AggregatedProteoforms.Size = new System.Drawing.Size(798, 426);
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
            this.splitContainer1.Size = new System.Drawing.Size(1277, 848);
            this.splitContainer1.SplitterDistance = 430;
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
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_min_num_bioreps);
            this.splitContainer2.Panel1.Controls.Add(this.label6);
            this.splitContainer2.Panel1.Controls.Add(this.label9);
            this.splitContainer2.Panel1.Controls.Add(this.tb_tableFilter);
            this.splitContainer2.Panel1.Controls.Add(this.cb_validateProteoforms);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_min_num_CS);
            this.splitContainer2.Panel1.Controls.Add(this.label8);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_min_agg_count);
            this.splitContainer2.Panel1.Controls.Add(this.label7);
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
            this.splitContainer2.Size = new System.Drawing.Size(1277, 430);
            this.splitContainer2.SplitterDistance = 472;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(25, 261);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(171, 13);
            this.label9.TabIndex = 50;
            this.label9.Text = "Aggregated Proteoform Table Filter";
            // 
            // tb_tableFilter
            // 
            this.tb_tableFilter.Location = new System.Drawing.Point(201, 258);
            this.tb_tableFilter.Name = "tb_tableFilter";
            this.tb_tableFilter.Size = new System.Drawing.Size(184, 20);
            this.tb_tableFilter.TabIndex = 49;
            this.tb_tableFilter.TextChanged += new System.EventHandler(this.tb_tableFilter_TextChanged);
            // 
            // cb_validateProteoforms
            // 
            this.cb_validateProteoforms.AutoSize = true;
            this.cb_validateProteoforms.Checked = true;
            this.cb_validateProteoforms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_validateProteoforms.Location = new System.Drawing.Point(90, 208);
            this.cb_validateProteoforms.Name = "cb_validateProteoforms";
            this.cb_validateProteoforms.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.cb_validateProteoforms.Size = new System.Drawing.Size(123, 17);
            this.cb_validateProteoforms.TabIndex = 17;
            this.cb_validateProteoforms.Text = "Validate Proteoforms";
            this.cb_validateProteoforms.UseVisualStyleBackColor = true;
            this.cb_validateProteoforms.CheckedChanged += new System.EventHandler(this.cb_validateProteoforms_CheckedChanged);
            // 
            // bt_aggregate
            // 
            this.bt_aggregate.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bt_aggregate.Location = new System.Drawing.Point(0, 403);
            this.bt_aggregate.Name = "bt_aggregate";
            this.bt_aggregate.Size = new System.Drawing.Size(468, 23);
            this.bt_aggregate.TabIndex = 2;
            this.bt_aggregate.Text = "Aggregate Proteoform Observations";
            this.bt_aggregate.UseVisualStyleBackColor = true;
            this.bt_aggregate.Click += new System.EventHandler(this.bt_aggregate_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(25, 234);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(197, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Total Accepted Aggregated Proteoforms";
            // 
            // tb_totalAggregatedProteoforms
            // 
            this.tb_totalAggregatedProteoforms.Location = new System.Drawing.Point(235, 227);
            this.tb_totalAggregatedProteoforms.Margin = new System.Windows.Forms.Padding(2);
            this.tb_totalAggregatedProteoforms.Name = "tb_totalAggregatedProteoforms";
            this.tb_totalAggregatedProteoforms.Size = new System.Drawing.Size(81, 20);
            this.tb_totalAggregatedProteoforms.TabIndex = 8;
            // 
            // nUD_Missed_Ks
            // 
            this.nUD_Missed_Ks.Location = new System.Drawing.Point(170, 92);
            this.nUD_Missed_Ks.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_Missed_Ks.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
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
            this.nUD_Missed_Monos.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
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
            this.nUD_RetTimeToleranace.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nUD_RetTimeToleranace.Name = "nUD_RetTimeToleranace";
            this.nUD_RetTimeToleranace.Size = new System.Drawing.Size(80, 20);
            this.nUD_RetTimeToleranace.TabIndex = 4;
            this.nUD_RetTimeToleranace.ValueChanged += new System.EventHandler(this.nUD_RetTimeToleranace_ValueChanged);
            // 
            // nUP_mass_tolerance
            // 
            this.nUP_mass_tolerance.Location = new System.Drawing.Point(170, 20);
            this.nUP_mass_tolerance.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUP_mass_tolerance.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
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
            this.dgv_AcceptNeuCdLtProteoforms.Size = new System.Drawing.Size(1273, 411);
            this.dgv_AcceptNeuCdLtProteoforms.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.rb_displayHeavyQuantificationComponents);
            this.groupBox1.Controls.Add(this.rb_displayLightQuantificationComponents);
            this.groupBox1.Controls.Add(this.rb_displayIdentificationComponents);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 302);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(468, 101);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Components Displayed upon Selecting an Experimental Proteoform";
            // 
            // rb_displayLightQuantificationComponents
            // 
            this.rb_displayLightQuantificationComponents.AutoSize = true;
            this.rb_displayLightQuantificationComponents.Location = new System.Drawing.Point(13, 42);
            this.rb_displayLightQuantificationComponents.Name = "rb_displayLightQuantificationComponents";
            this.rb_displayLightQuantificationComponents.Size = new System.Drawing.Size(178, 17);
            this.rb_displayLightQuantificationComponents.TabIndex = 1;
            this.rb_displayLightQuantificationComponents.Text = "Light Quantification Components";
            this.rb_displayLightQuantificationComponents.UseVisualStyleBackColor = true;
            this.rb_displayLightQuantificationComponents.CheckedChanged += new System.EventHandler(this.rb_displayLightQuantificationComponents_CheckedChanged);
            // 
            // rb_displayIdentificationComponents
            // 
            this.rb_displayIdentificationComponents.AutoSize = true;
            this.rb_displayIdentificationComponents.Checked = true;
            this.rb_displayIdentificationComponents.Location = new System.Drawing.Point(13, 19);
            this.rb_displayIdentificationComponents.Name = "rb_displayIdentificationComponents";
            this.rb_displayIdentificationComponents.Size = new System.Drawing.Size(147, 17);
            this.rb_displayIdentificationComponents.TabIndex = 0;
            this.rb_displayIdentificationComponents.TabStop = true;
            this.rb_displayIdentificationComponents.Text = "Identification Components";
            this.rb_displayIdentificationComponents.UseVisualStyleBackColor = true;
            this.rb_displayIdentificationComponents.CheckedChanged += new System.EventHandler(this.rb_displayIdentificationComponents_CheckedChanged);
            // 
            // rb_displayHeavyQuantificationComponents
            // 
            this.rb_displayHeavyQuantificationComponents.AutoSize = true;
            this.rb_displayHeavyQuantificationComponents.Location = new System.Drawing.Point(13, 65);
            this.rb_displayHeavyQuantificationComponents.Name = "rb_displayHeavyQuantificationComponents";
            this.rb_displayHeavyQuantificationComponents.Size = new System.Drawing.Size(186, 17);
            this.rb_displayHeavyQuantificationComponents.TabIndex = 2;
            this.rb_displayHeavyQuantificationComponents.Text = "Heavy Quantification Components";
            this.rb_displayHeavyQuantificationComponents.UseVisualStyleBackColor = true;
            this.rb_displayHeavyQuantificationComponents.CheckedChanged += new System.EventHandler(this.rb_displayHeavyQuantificationComponents_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(44, 178);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(116, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Min. Aggregated Count";
            // 
            // nUD_min_agg_count
            // 
            this.nUD_min_agg_count.Location = new System.Drawing.Point(176, 176);
            this.nUD_min_agg_count.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_min_agg_count.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nUD_min_agg_count.Name = "nUD_min_agg_count";
            this.nUD_min_agg_count.Size = new System.Drawing.Size(80, 20);
            this.nUD_min_agg_count.TabIndex = 14;
            this.nUD_min_agg_count.ValueChanged += new System.EventHandler(this.nUD_min_agg_count_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(53, 134);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(107, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Min. # Charge States";
            // 
            // nUD_min_num_CS
            // 
            this.nUD_min_num_CS.Location = new System.Drawing.Point(176, 132);
            this.nUD_min_num_CS.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_min_num_CS.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nUD_min_num_CS.Name = "nUD_min_num_CS";
            this.nUD_min_num_CS.Size = new System.Drawing.Size(80, 20);
            this.nUD_min_num_CS.TabIndex = 16;
            this.nUD_min_num_CS.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nUD_min_num_CS.ValueChanged += new System.EventHandler(this.nUD_min_num_CS_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(61, 156);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 13);
            this.label6.TabIndex = 51;
            this.label6.Text = "Min. # BioReps";
            // 
            // nUD_min_num_bioreps
            // 
            this.nUD_min_num_bioreps.Location = new System.Drawing.Point(176, 154);
            this.nUD_min_num_bioreps.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.nUD_min_num_bioreps.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nUD_min_num_bioreps.Name = "nUD_min_num_bioreps";
            this.nUD_min_num_bioreps.Size = new System.Drawing.Size(80, 20);
            this.nUD_min_num_bioreps.TabIndex = 52;
            this.nUD_min_num_bioreps.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nUD_min_num_bioreps.ValueChanged += new System.EventHandler(this.nUD_min_num_bioreps_ValueChanged);
            // 
            // AggregatedProteoforms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1277, 848);
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "AggregatedProteoforms";
            this.Text = "AggregatedProteoforms";
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
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Ks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Monos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_RetTimeToleranace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUP_mass_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AcceptNeuCdLtProteoforms)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_agg_count)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_CS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_bioreps)).EndInit();
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
        private System.Windows.Forms.CheckBox cb_validateProteoforms;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tb_tableFilter;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rb_displayLightQuantificationComponents;
        private System.Windows.Forms.RadioButton rb_displayIdentificationComponents;
        private System.Windows.Forms.RadioButton rb_displayHeavyQuantificationComponents;
        private System.Windows.Forms.NumericUpDown nUD_min_num_bioreps;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nUD_min_num_CS;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nUD_min_agg_count;
        private System.Windows.Forms.Label label7;
    }
}