using System;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RawExperimentalComponents));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dgv_fileList = new System.Windows.Forms.DataGridView();
            this.bt_recalculate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb_displayQuantificationComponents = new System.Windows.Forms.RadioButton();
            this.rb_displayIdentificationComponents = new System.Windows.Forms.RadioButton();
            this.dgv_rawComponents = new System.Windows.Forms.DataGridView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.label12 = new System.Windows.Forms.Label();
            this.nUD_scans_to_average = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.nUD_min_cs = new System.Windows.Forms.NumericUpDown();
            this.cb_deconvolute = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.nUD_min_num_CS = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.nUD_max_cs = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.nUD_decon_tolerance = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.nUD_agg_tolerance = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.nUD_max_RT = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nUD_min_RT = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.nUD_mass_tolerance = new System.Windows.Forms.NumericUpDown();
            this.rtb_raw_components_counts = new System.Windows.Forms.RichTextBox();
            this.dgv_chargeStates = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_fileList)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_rawComponents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_scans_to_average)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_cs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_CS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_cs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_decon_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_agg_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_RT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_RT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_mass_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_chargeStates)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
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
            this.splitContainer1.Size = new System.Drawing.Size(1125, 654);
            this.splitContainer1.SplitterDistance = 245;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dgv_fileList);
            this.splitContainer2.Panel1.Controls.Add(this.bt_recalculate);
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgv_rawComponents);
            this.splitContainer2.Size = new System.Drawing.Size(1121, 241);
            this.splitContainer2.SplitterDistance = 371;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 0;
            // 
            // dgv_fileList
            // 
            this.dgv_fileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_fileList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_fileList.Location = new System.Drawing.Point(0, 3);
            this.dgv_fileList.Name = "dgv_fileList";
            this.dgv_fileList.Size = new System.Drawing.Size(187, 235);
            this.dgv_fileList.TabIndex = 2;
            // 
            // bt_recalculate
            // 
            this.bt_recalculate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_recalculate.Location = new System.Drawing.Point(191, 109);
            this.bt_recalculate.Name = "bt_recalculate";
            this.bt_recalculate.Size = new System.Drawing.Size(177, 132);
            this.bt_recalculate.TabIndex = 1;
            this.bt_recalculate.Text = "Read Raw Components and\r\nCollapse Deconvolution Artifacts";
            this.bt_recalculate.UseVisualStyleBackColor = true;
            this.bt_recalculate.Click += new System.EventHandler(this.bt_recalculate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.rb_displayQuantificationComponents);
            this.groupBox1.Controls.Add(this.rb_displayIdentificationComponents);
            this.groupBox1.Location = new System.Drawing.Point(193, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(175, 131);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Components Displayed";
            // 
            // rb_displayQuantificationComponents
            // 
            this.rb_displayQuantificationComponents.AutoSize = true;
            this.rb_displayQuantificationComponents.Location = new System.Drawing.Point(10, 56);
            this.rb_displayQuantificationComponents.Name = "rb_displayQuantificationComponents";
            this.rb_displayQuantificationComponents.Size = new System.Drawing.Size(152, 17);
            this.rb_displayQuantificationComponents.TabIndex = 1;
            this.rb_displayQuantificationComponents.Text = "Quantification Components";
            this.rb_displayQuantificationComponents.UseVisualStyleBackColor = true;
            // 
            // rb_displayIdentificationComponents
            // 
            this.rb_displayIdentificationComponents.AutoSize = true;
            this.rb_displayIdentificationComponents.Checked = true;
            this.rb_displayIdentificationComponents.Location = new System.Drawing.Point(10, 33);
            this.rb_displayIdentificationComponents.Name = "rb_displayIdentificationComponents";
            this.rb_displayIdentificationComponents.Size = new System.Drawing.Size(147, 17);
            this.rb_displayIdentificationComponents.TabIndex = 0;
            this.rb_displayIdentificationComponents.TabStop = true;
            this.rb_displayIdentificationComponents.Text = "Identification Components";
            this.rb_displayIdentificationComponents.UseVisualStyleBackColor = true;
            this.rb_displayIdentificationComponents.CheckedChanged += new System.EventHandler(this.rb_displayIdentificationComponents_CheckedChanged);
            // 
            // dgv_rawComponents
            // 
            this.dgv_rawComponents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_rawComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_rawComponents.Location = new System.Drawing.Point(0, 0);
            this.dgv_rawComponents.Margin = new System.Windows.Forms.Padding(2);
            this.dgv_rawComponents.Name = "dgv_rawComponents";
            this.dgv_rawComponents.RowTemplate.Height = 28;
            this.dgv_rawComponents.Size = new System.Drawing.Size(747, 241);
            this.dgv_rawComponents.TabIndex = 0;
            this.dgv_rawComponents.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_RawQuantComp_MI_masses_CellContentClick);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.dgv_chargeStates);
            this.splitContainer3.Size = new System.Drawing.Size(1121, 402);
            this.splitContainer3.SplitterDistance = 381;
            this.splitContainer3.SplitterWidth = 3;
            this.splitContainer3.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.label12);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_scans_to_average);
            this.splitContainer4.Panel1.Controls.Add(this.label10);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_min_cs);
            this.splitContainer4.Panel1.Controls.Add(this.cb_deconvolute);
            this.splitContainer4.Panel1.Controls.Add(this.label8);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_min_num_CS);
            this.splitContainer4.Panel1.Controls.Add(this.label6);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_max_cs);
            this.splitContainer4.Panel1.Controls.Add(this.label5);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_decon_tolerance);
            this.splitContainer4.Panel1.Controls.Add(this.label4);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_agg_tolerance);
            this.splitContainer4.Panel1.Controls.Add(this.label3);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_max_RT);
            this.splitContainer4.Panel1.Controls.Add(this.label2);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_min_RT);
            this.splitContainer4.Panel1.Controls.Add(this.label1);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_mass_tolerance);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.rtb_raw_components_counts);
            this.splitContainer4.Size = new System.Drawing.Size(381, 402);
            this.splitContainer4.SplitterDistance = 369;
            this.splitContainer4.TabIndex = 0;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(136, 235);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(91, 13);
            this.label12.TabIndex = 25;
            this.label12.Text = "Scans to average";
            this.label12.Visible = false;
            // 
            // nUD_scans_to_average
            // 
            this.nUD_scans_to_average.Location = new System.Drawing.Point(11, 233);
            this.nUD_scans_to_average.Name = "nUD_scans_to_average";
            this.nUD_scans_to_average.Size = new System.Drawing.Size(120, 20);
            this.nUD_scans_to_average.TabIndex = 24;
            this.nUD_scans_to_average.Visible = false;
            this.nUD_scans_to_average.ValueChanged += new System.EventHandler(this.nUD_scans_to_average_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(137, 162);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(134, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "Min. assumed charge state";
            this.label10.Visible = false;
            // 
            // nUD_min_cs
            // 
            this.nUD_min_cs.Location = new System.Drawing.Point(11, 158);
            this.nUD_min_cs.Name = "nUD_min_cs";
            this.nUD_min_cs.Size = new System.Drawing.Size(120, 20);
            this.nUD_min_cs.TabIndex = 20;
            this.nUD_min_cs.Visible = false;
            this.nUD_min_cs.ValueChanged += new System.EventHandler(this.nUD_min_cs_ValueChanged);
            // 
            // cb_deconvolute
            // 
            this.cb_deconvolute.AutoSize = true;
            this.cb_deconvolute.Location = new System.Drawing.Point(11, 36);
            this.cb_deconvolute.Name = "cb_deconvolute";
            this.cb_deconvolute.Size = new System.Drawing.Size(87, 17);
            this.cb_deconvolute.TabIndex = 19;
            this.cb_deconvolute.Text = "Deconvolute";
            this.cb_deconvolute.UseVisualStyleBackColor = true;
            this.cb_deconvolute.CheckedChanged += new System.EventHandler(this.cb_deconvolute_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(137, 210);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(241, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Min. number of charge states to make component";
            this.label8.Visible = false;
            // 
            // nUD_min_num_CS
            // 
            this.nUD_min_num_CS.Location = new System.Drawing.Point(10, 208);
            this.nUD_min_num_CS.Name = "nUD_min_num_CS";
            this.nUD_min_num_CS.Size = new System.Drawing.Size(120, 20);
            this.nUD_min_num_CS.TabIndex = 14;
            this.nUD_min_num_CS.Visible = false;
            this.nUD_min_num_CS.ValueChanged += new System.EventHandler(this.nUD_min_num_CS_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(136, 187);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(137, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Max. assumed charge state";
            this.label6.Visible = false;
            // 
            // nUD_max_cs
            // 
            this.nUD_max_cs.Location = new System.Drawing.Point(10, 183);
            this.nUD_max_cs.Name = "nUD_max_cs";
            this.nUD_max_cs.Size = new System.Drawing.Size(120, 20);
            this.nUD_max_cs.TabIndex = 10;
            this.nUD_max_cs.Visible = false;
            this.nUD_max_cs.ValueChanged += new System.EventHandler(this.nUD_max_cs_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(136, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Deconvolution tolerance (ppm)";
            this.label5.Visible = false;
            // 
            // nUD_decon_tolerance
            // 
            this.nUD_decon_tolerance.Location = new System.Drawing.Point(10, 132);
            this.nUD_decon_tolerance.Name = "nUD_decon_tolerance";
            this.nUD_decon_tolerance.Size = new System.Drawing.Size(120, 20);
            this.nUD_decon_tolerance.TabIndex = 8;
            this.nUD_decon_tolerance.Visible = false;
            this.nUD_decon_tolerance.ValueChanged += new System.EventHandler(this.nUD_decon_tolerance_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(136, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(140, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Aggregation tolerance (ppm)";
            this.label4.Visible = false;
            // 
            // nUD_agg_tolerance
            // 
            this.nUD_agg_tolerance.Location = new System.Drawing.Point(10, 106);
            this.nUD_agg_tolerance.Name = "nUD_agg_tolerance";
            this.nUD_agg_tolerance.Size = new System.Drawing.Size(120, 20);
            this.nUD_agg_tolerance.TabIndex = 6;
            this.nUD_agg_tolerance.Visible = false;
            this.nUD_agg_tolerance.ValueChanged += new System.EventHandler(this.nUD_agg_tolerance_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Max. retention time (min)";
            this.label3.Visible = false;
            // 
            // nUD_max_RT
            // 
            this.nUD_max_RT.Location = new System.Drawing.Point(10, 80);
            this.nUD_max_RT.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.nUD_max_RT.Name = "nUD_max_RT";
            this.nUD_max_RT.Size = new System.Drawing.Size(120, 20);
            this.nUD_max_RT.TabIndex = 4;
            this.nUD_max_RT.Visible = false;
            this.nUD_max_RT.ValueChanged += new System.EventHandler(this.nUD_max_RT_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(136, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Min. retention time (min)";
            this.label2.Visible = false;
            // 
            // nUD_min_RT
            // 
            this.nUD_min_RT.Location = new System.Drawing.Point(10, 54);
            this.nUD_min_RT.Name = "nUD_min_RT";
            this.nUD_min_RT.Size = new System.Drawing.Size(120, 20);
            this.nUD_min_RT.TabIndex = 2;
            this.nUD_min_RT.Visible = false;
            this.nUD_min_RT.ValueChanged += new System.EventHandler(this.nUD_min_RT_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(136, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Mass Tolerance for Merging Artifacts (ppm)";
            // 
            // nUD_mass_tolerance
            // 
            this.nUD_mass_tolerance.Location = new System.Drawing.Point(10, 4);
            this.nUD_mass_tolerance.Name = "nUD_mass_tolerance";
            this.nUD_mass_tolerance.Size = new System.Drawing.Size(120, 20);
            this.nUD_mass_tolerance.TabIndex = 0;
            this.nUD_mass_tolerance.ValueChanged += new System.EventHandler(this.nUD_mass_tolerance_ValueChanged);
            // 
            // rtb_raw_components_counts
            // 
            this.rtb_raw_components_counts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_raw_components_counts.Location = new System.Drawing.Point(0, 0);
            this.rtb_raw_components_counts.Name = "rtb_raw_components_counts";
            this.rtb_raw_components_counts.ReadOnly = true;
            this.rtb_raw_components_counts.Size = new System.Drawing.Size(381, 29);
            this.rtb_raw_components_counts.TabIndex = 0;
            this.rtb_raw_components_counts.Text = "";
            // 
            // dgv_chargeStates
            // 
            this.dgv_chargeStates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_chargeStates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_chargeStates.Location = new System.Drawing.Point(0, 0);
            this.dgv_chargeStates.Margin = new System.Windows.Forms.Padding(2);
            this.dgv_chargeStates.Name = "dgv_chargeStates";
            this.dgv_chargeStates.RowTemplate.Height = 28;
            this.dgv_chargeStates.Size = new System.Drawing.Size(737, 402);
            this.dgv_chargeStates.TabIndex = 0;
            // 
            // RawExperimentalComponents
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1125, 654);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "RawExperimentalComponents";
            this.Text = "Raw Experimental Components";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_fileList)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_rawComponents)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nUD_scans_to_average)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_cs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_CS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_cs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_decon_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_agg_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_RT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_RT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_mass_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_chargeStates)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private DataGridView dgv_rawComponents;
        private SplitContainer splitContainer3;
        private DataGridView dgv_chargeStates;
        private Button bt_recalculate;
        private GroupBox groupBox1;
        private RadioButton rb_displayQuantificationComponents;
        private RadioButton rb_displayIdentificationComponents;
        private DataGridView dgv_fileList;
        private SplitContainer splitContainer4;
        private Label label1;
        private NumericUpDown nUD_mass_tolerance;
        private RichTextBox rtb_raw_components_counts;
        private Label label4;
        private NumericUpDown nUD_agg_tolerance;
        private Label label3;
        private NumericUpDown nUD_max_RT;
        private Label label2;
        private NumericUpDown nUD_min_RT;
        private Label label8;
        private NumericUpDown nUD_min_num_CS;
        private Label label6;
        private NumericUpDown nUD_max_cs;
        private Label label5;
        private NumericUpDown nUD_decon_tolerance;
        private CheckBox cb_deconvolute;
        private Label label10;
        private NumericUpDown nUD_min_cs;
        private Label label12;
        private NumericUpDown nUD_scans_to_average;
    }
}