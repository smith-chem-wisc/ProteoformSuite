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
            this.cb_deconvolute = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.nUD_min_num_scans = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.nUD_min_num_CS = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.nUD_intensity_ratio_limit = new System.Windows.Forms.NumericUpDown();
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
            this.label10 = new System.Windows.Forms.Label();
            this.nUD_min_cs = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.nUD_min_rel_ab = new System.Windows.Forms.NumericUpDown();
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
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_scans)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_CS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_intensity_ratio_limit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_cs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_decon_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_agg_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_RT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_RT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_mass_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_chargeStates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_cs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_rel_ab)).BeginInit();
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
            this.splitContainer1.Size = new System.Drawing.Size(1125, 579);
            this.splitContainer1.SplitterDistance = 217;
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
            this.splitContainer2.Size = new System.Drawing.Size(1121, 213);
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
            this.dgv_fileList.Size = new System.Drawing.Size(187, 207);
            this.dgv_fileList.TabIndex = 2;
            // 
            // bt_recalculate
            // 
            this.bt_recalculate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_recalculate.Location = new System.Drawing.Point(191, 109);
            this.bt_recalculate.Name = "bt_recalculate";
            this.bt_recalculate.Size = new System.Drawing.Size(177, 104);
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
            this.groupBox1.Size = new System.Drawing.Size(175, 103);
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
            this.dgv_rawComponents.Size = new System.Drawing.Size(747, 213);
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
            this.splitContainer3.Size = new System.Drawing.Size(1121, 355);
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
            this.splitContainer4.Panel1.Controls.Add(this.label11);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_min_rel_ab);
            this.splitContainer4.Panel1.Controls.Add(this.label10);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_min_cs);
            this.splitContainer4.Panel1.Controls.Add(this.cb_deconvolute);
            this.splitContainer4.Panel1.Controls.Add(this.label9);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_min_num_scans);
            this.splitContainer4.Panel1.Controls.Add(this.label8);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_min_num_CS);
            this.splitContainer4.Panel1.Controls.Add(this.label7);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_intensity_ratio_limit);
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
            this.splitContainer4.Size = new System.Drawing.Size(381, 355);
            this.splitContainer4.SplitterDistance = 325;
            this.splitContainer4.TabIndex = 0;
            // 
            // cb_deconvolute
            // 
            this.cb_deconvolute.AutoSize = true;
            this.cb_deconvolute.Location = new System.Drawing.Point(11, 47);
            this.cb_deconvolute.Name = "cb_deconvolute";
            this.cb_deconvolute.Size = new System.Drawing.Size(87, 17);
            this.cb_deconvolute.TabIndex = 19;
            this.cb_deconvolute.Text = "Deconvolute";
            this.cb_deconvolute.UseVisualStyleBackColor = true;
            this.cb_deconvolute.CheckedChanged += new System.EventHandler(this.cb_deconvolute_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(136, 303);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(205, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Min. number of scans to make component";
            this.label9.Visible = false;
            // 
            // nUD_min_num_scans
            // 
            this.nUD_min_num_scans.Location = new System.Drawing.Point(10, 299);
            this.nUD_min_num_scans.Name = "nUD_min_num_scans";
            this.nUD_min_num_scans.Size = new System.Drawing.Size(120, 20);
            this.nUD_min_num_scans.TabIndex = 16;
            this.nUD_min_num_scans.Visible = false;
            this.nUD_min_num_scans.ValueChanged += new System.EventHandler(this.nUD_min_num_scans_ValueChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(136, 277);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(241, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Min. number of charge states to make component";
            this.label8.Visible = false;
            // 
            // nUD_min_num_CS
            // 
            this.nUD_min_num_CS.Location = new System.Drawing.Point(10, 275);
            this.nUD_min_num_CS.Name = "nUD_min_num_CS";
            this.nUD_min_num_CS.Size = new System.Drawing.Size(120, 20);
            this.nUD_min_num_CS.TabIndex = 14;
            this.nUD_min_num_CS.Visible = false;
            this.nUD_min_num_CS.ValueChanged += new System.EventHandler(this.nUD_min_num_CS_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(136, 253);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Intensity ratio limit";
            this.label7.Visible = false;
            // 
            // nUD_intensity_ratio_limit
            // 
            this.nUD_intensity_ratio_limit.Location = new System.Drawing.Point(10, 249);
            this.nUD_intensity_ratio_limit.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.nUD_intensity_ratio_limit.Name = "nUD_intensity_ratio_limit";
            this.nUD_intensity_ratio_limit.Size = new System.Drawing.Size(120, 20);
            this.nUD_intensity_ratio_limit.TabIndex = 12;
            this.nUD_intensity_ratio_limit.Visible = false;
            this.nUD_intensity_ratio_limit.ValueChanged += new System.EventHandler(this.nUD_intensity_ratio_limit_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(136, 203);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(137, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Max. assumed charge state";
            this.label6.Visible = false;
            // 
            // nUD_max_cs
            // 
            this.nUD_max_cs.Location = new System.Drawing.Point(10, 199);
            this.nUD_max_cs.Name = "nUD_max_cs";
            this.nUD_max_cs.Size = new System.Drawing.Size(120, 20);
            this.nUD_max_cs.TabIndex = 10;
            this.nUD_max_cs.Visible = false;
            this.nUD_max_cs.ValueChanged += new System.EventHandler(this.nUD_max_cs_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(136, 152);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Deconvolution tolerance (ppm)";
            this.label5.Visible = false;
            // 
            // nUD_decon_tolerance
            // 
            this.nUD_decon_tolerance.Location = new System.Drawing.Point(10, 148);
            this.nUD_decon_tolerance.Name = "nUD_decon_tolerance";
            this.nUD_decon_tolerance.Size = new System.Drawing.Size(120, 20);
            this.nUD_decon_tolerance.TabIndex = 8;
            this.nUD_decon_tolerance.Visible = false;
            this.nUD_decon_tolerance.ValueChanged += new System.EventHandler(this.nUD_decon_tolerance_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(136, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(140, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Aggregation tolerance (ppm)";
            this.label4.Visible = false;
            // 
            // nUD_agg_tolerance
            // 
            this.nUD_agg_tolerance.Location = new System.Drawing.Point(10, 122);
            this.nUD_agg_tolerance.Name = "nUD_agg_tolerance";
            this.nUD_agg_tolerance.Size = new System.Drawing.Size(120, 20);
            this.nUD_agg_tolerance.TabIndex = 6;
            this.nUD_agg_tolerance.Visible = false;
            this.nUD_agg_tolerance.ValueChanged += new System.EventHandler(this.nUD_agg_tolerance_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Max. retention time (min)";
            this.label3.Visible = false;
            // 
            // nUD_max_RT
            // 
            this.nUD_max_RT.Location = new System.Drawing.Point(10, 96);
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
            this.label2.Location = new System.Drawing.Point(136, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Min. retention time (min)";
            this.label2.Visible = false;
            // 
            // nUD_min_RT
            // 
            this.nUD_min_RT.Location = new System.Drawing.Point(10, 70);
            this.nUD_min_RT.Name = "nUD_min_RT";
            this.nUD_min_RT.Size = new System.Drawing.Size(120, 20);
            this.nUD_min_RT.TabIndex = 2;
            this.nUD_min_RT.Visible = false;
            this.nUD_min_RT.ValueChanged += new System.EventHandler(this.nUD_min_RT_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(136, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(209, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Mass Tolerance for Merging Artifacts (ppm)";
            // 
            // nUD_mass_tolerance
            // 
            this.nUD_mass_tolerance.Location = new System.Drawing.Point(10, 14);
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
            this.rtb_raw_components_counts.Size = new System.Drawing.Size(381, 26);
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
            this.dgv_chargeStates.Size = new System.Drawing.Size(737, 355);
            this.dgv_chargeStates.TabIndex = 0;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(137, 178);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(134, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "Min. assumed charge state";
            this.label10.Visible = false;
            // 
            // nUD_min_cs
            // 
            this.nUD_min_cs.Location = new System.Drawing.Point(11, 174);
            this.nUD_min_cs.Name = "nUD_min_cs";
            this.nUD_min_cs.Size = new System.Drawing.Size(120, 20);
            this.nUD_min_cs.TabIndex = 20;
            this.nUD_min_cs.Visible = false;
            this.nUD_min_cs.ValueChanged += new System.EventHandler(this.nUD_min_cs_ValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(136, 229);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(138, 13);
            this.label11.TabIndex = 23;
            this.label11.Text = "Min. relative abundance (%)";
            this.label11.Visible = false;
            // 
            // nUD_min_rel_ab
            // 
            this.nUD_min_rel_ab.DecimalPlaces = 2;
            this.nUD_min_rel_ab.Location = new System.Drawing.Point(10, 225);
            this.nUD_min_rel_ab.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.nUD_min_rel_ab.Name = "nUD_min_rel_ab";
            this.nUD_min_rel_ab.Size = new System.Drawing.Size(120, 20);
            this.nUD_min_rel_ab.TabIndex = 22;
            this.nUD_min_rel_ab.Visible = false;
            this.nUD_min_rel_ab.ValueChanged += new System.EventHandler(this.nUD_min_rel_ab_ValueChanged);
            // 
            // RawExperimentalComponents
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1125, 579);
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
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_scans)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_num_CS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_intensity_ratio_limit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_cs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_decon_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_agg_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_RT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_RT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_mass_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_chargeStates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_cs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_rel_ab)).EndInit();
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
        private Label label9;
        private NumericUpDown nUD_min_num_scans;
        private Label label8;
        private NumericUpDown nUD_min_num_CS;
        private Label label7;
        private NumericUpDown nUD_intensity_ratio_limit;
        private Label label6;
        private NumericUpDown nUD_max_cs;
        private Label label5;
        private NumericUpDown nUD_decon_tolerance;
        private CheckBox cb_deconvolute;
        private Label label11;
        private NumericUpDown nUD_min_rel_ab;
        private Label label10;
        private NumericUpDown nUD_min_cs;
    }
}