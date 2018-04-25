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
            this.param_splitcontainer = new System.Windows.Forms.SplitContainer();
            this.dgv_fileList = new System.Windows.Forms.DataGridView();
            this.bt_recalculate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb_displayQuantificationComponents = new System.Windows.Forms.RadioButton();
            this.rb_displayIdentificationComponents = new System.Windows.Forms.RadioButton();
            this.dgv_rawComponents = new System.Windows.Forms.DataGridView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.nUD_max_fit = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nUD_min_liklihood_ratio = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.nUD_mass_tolerance = new System.Windows.Forms.NumericUpDown();
            this.rtb_raw_components_counts = new System.Windows.Forms.RichTextBox();
            this.dgv_chargeStates = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.param_splitcontainer)).BeginInit();
            this.param_splitcontainer.Panel1.SuspendLayout();
            this.param_splitcontainer.Panel2.SuspendLayout();
            this.param_splitcontainer.SuspendLayout();
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
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_fit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_liklihood_ratio)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.param_splitcontainer);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(1125, 654);
            this.splitContainer1.SplitterDistance = 245;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // param_splitcontainer
            // 
            this.param_splitcontainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.param_splitcontainer.Location = new System.Drawing.Point(0, 0);
            this.param_splitcontainer.Margin = new System.Windows.Forms.Padding(2);
            this.param_splitcontainer.Name = "param_splitcontainer";
            // 
            // param_splitcontainer.Panel1
            // 
            this.param_splitcontainer.Panel1.Controls.Add(this.dgv_fileList);
            this.param_splitcontainer.Panel1.Controls.Add(this.bt_recalculate);
            this.param_splitcontainer.Panel1.Controls.Add(this.groupBox1);
            // 
            // param_splitcontainer.Panel2
            // 
            this.param_splitcontainer.Panel2.Controls.Add(this.dgv_rawComponents);
            this.param_splitcontainer.Size = new System.Drawing.Size(1121, 241);
            this.param_splitcontainer.SplitterDistance = 371;
            this.param_splitcontainer.SplitterWidth = 3;
            this.param_splitcontainer.TabIndex = 0;
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
            this.splitContainer4.Panel1.Controls.Add(this.label3);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_max_fit);
            this.splitContainer4.Panel1.Controls.Add(this.label2);
            this.splitContainer4.Panel1.Controls.Add(this.nUD_min_liklihood_ratio);
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(136, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Max Fit";
            // 
            // nUD_max_fit
            // 
            this.nUD_max_fit.DecimalPlaces = 2;
            this.nUD_max_fit.Location = new System.Drawing.Point(10, 56);
            this.nUD_max_fit.Name = "nUD_max_fit";
            this.nUD_max_fit.Size = new System.Drawing.Size(120, 20);
            this.nUD_max_fit.TabIndex = 4;
            this.nUD_max_fit.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.nUD_max_fit.ValueChanged += new System.EventHandler(this.nUD_max_fit_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(136, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Min. Likelihood Ratio";
            // 
            // nUD_min_liklihood_ratio
            // 
            this.nUD_min_liklihood_ratio.Location = new System.Drawing.Point(10, 30);
            this.nUD_min_liklihood_ratio.Name = "nUD_min_liklihood_ratio";
            this.nUD_min_liklihood_ratio.Size = new System.Drawing.Size(120, 20);
            this.nUD_min_liklihood_ratio.TabIndex = 2;
            this.nUD_min_liklihood_ratio.ValueChanged += new System.EventHandler(this.nUD_min_liklihood_ratio_ValueChanged);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
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
            this.param_splitcontainer.Panel1.ResumeLayout(false);
            this.param_splitcontainer.Panel1.PerformLayout();
            this.param_splitcontainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.param_splitcontainer)).EndInit();
            this.param_splitcontainer.ResumeLayout(false);
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
            ((System.ComponentModel.ISupportInitialize)(this.nUD_max_fit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_liklihood_ratio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_mass_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_chargeStates)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private SplitContainer param_splitcontainer;
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
        private Label label3;
        private NumericUpDown nUD_max_fit;
        private Label label2;
        private NumericUpDown nUD_min_liklihood_ratio;
    }
}