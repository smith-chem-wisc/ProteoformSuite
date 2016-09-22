namespace ProteoformSuite
{
    partial class Quantification
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.dgv_quantification_results = new System.Windows.Forms.DataGridView();
            this.ckbx_columnNormalize = new System.Windows.Forms.CheckBox();
            this.ckbx_compressTechReps = new System.Windows.Forms.CheckBox();
            this.ckbx_compressFractions = new System.Windows.Forms.CheckBox();
            this.ckbx_compressBioReps = new System.Windows.Forms.CheckBox();
            this.gb_quantDataDisplaySelection = new System.Windows.Forms.GroupBox();
            this.rbtn_pValue = new System.Windows.Forms.RadioButton();
            this.rbtn_variance = new System.Windows.Forms.RadioButton();
            this.rbtn_totalIntensity = new System.Windows.Forms.RadioButton();
            this.rbtn_neucodeRatio = new System.Windows.Forms.RadioButton();
            this.ct_volcano_logFold_logP = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cmbx_quantColumns = new System.Windows.Forms.ComboBox();
            this.dgv_goAnalysis = new System.Windows.Forms.DataGridView();
            this.cmbx_goAspect = new System.Windows.Forms.ComboBox();
            this.gb_goThresholds = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nud_intensity = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nud_ratio = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.nud_pValue = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantification_results)).BeginInit();
            this.gb_quantDataDisplaySelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ct_volcano_logFold_logP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_goAnalysis)).BeginInit();
            this.gb_goThresholds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_intensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_ratio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_pValue)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_quantification_results
            // 
            this.dgv_quantification_results.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_quantification_results.Location = new System.Drawing.Point(82, 50);
            this.dgv_quantification_results.Name = "dgv_quantification_results";
            this.dgv_quantification_results.RowTemplate.Height = 28;
            this.dgv_quantification_results.Size = new System.Drawing.Size(678, 499);
            this.dgv_quantification_results.TabIndex = 0;
            this.dgv_quantification_results.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgv_quantification_results_DataBindingComplete);
            // 
            // ckbx_columnNormalize
            // 
            this.ckbx_columnNormalize.AutoSize = true;
            this.ckbx_columnNormalize.Checked = true;
            this.ckbx_columnNormalize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbx_columnNormalize.Location = new System.Drawing.Point(82, 585);
            this.ckbx_columnNormalize.Name = "ckbx_columnNormalize";
            this.ckbx_columnNormalize.Size = new System.Drawing.Size(163, 24);
            this.ckbx_columnNormalize.TabIndex = 1;
            this.ckbx_columnNormalize.Text = "Column Normalize";
            this.ckbx_columnNormalize.UseVisualStyleBackColor = true;
            this.ckbx_columnNormalize.CheckedChanged += new System.EventHandler(this.ckbx_columnNormalize_CheckedChanged);
            // 
            // ckbx_compressTechReps
            // 
            this.ckbx_compressTechReps.AutoSize = true;
            this.ckbx_compressTechReps.Checked = true;
            this.ckbx_compressTechReps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbx_compressTechReps.Location = new System.Drawing.Point(82, 673);
            this.ckbx_compressTechReps.Name = "ckbx_compressTechReps";
            this.ckbx_compressTechReps.Size = new System.Drawing.Size(257, 24);
            this.ckbx_compressTechReps.TabIndex = 2;
            this.ckbx_compressTechReps.Text = "Compress Technical Replicates";
            this.ckbx_compressTechReps.UseVisualStyleBackColor = true;
            this.ckbx_compressTechReps.CheckedChanged += new System.EventHandler(this.ckbx_compressTechReps_CheckedChanged);
            // 
            // ckbx_compressFractions
            // 
            this.ckbx_compressFractions.AutoSize = true;
            this.ckbx_compressFractions.Checked = true;
            this.ckbx_compressFractions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbx_compressFractions.Location = new System.Drawing.Point(82, 643);
            this.ckbx_compressFractions.Name = "ckbx_compressFractions";
            this.ckbx_compressFractions.Size = new System.Drawing.Size(177, 24);
            this.ckbx_compressFractions.TabIndex = 3;
            this.ckbx_compressFractions.Text = "Compress Fractions";
            this.ckbx_compressFractions.UseVisualStyleBackColor = true;
            this.ckbx_compressFractions.CheckedChanged += new System.EventHandler(this.ckbx_compressFractions_CheckedChanged);
            // 
            // ckbx_compressBioReps
            // 
            this.ckbx_compressBioReps.AutoSize = true;
            this.ckbx_compressBioReps.Checked = true;
            this.ckbx_compressBioReps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbx_compressBioReps.Location = new System.Drawing.Point(82, 613);
            this.ckbx_compressBioReps.Name = "ckbx_compressBioReps";
            this.ckbx_compressBioReps.Size = new System.Drawing.Size(172, 24);
            this.ckbx_compressBioReps.TabIndex = 4;
            this.ckbx_compressBioReps.Text = "Compress BioReps";
            this.ckbx_compressBioReps.UseVisualStyleBackColor = true;
            this.ckbx_compressBioReps.CheckedChanged += new System.EventHandler(this.ckbx_compressBioReps_CheckedChanged);
            // 
            // gb_quantDataDisplaySelection
            // 
            this.gb_quantDataDisplaySelection.Controls.Add(this.rbtn_pValue);
            this.gb_quantDataDisplaySelection.Controls.Add(this.rbtn_variance);
            this.gb_quantDataDisplaySelection.Controls.Add(this.rbtn_totalIntensity);
            this.gb_quantDataDisplaySelection.Controls.Add(this.rbtn_neucodeRatio);
            this.gb_quantDataDisplaySelection.Location = new System.Drawing.Point(561, 573);
            this.gb_quantDataDisplaySelection.Name = "gb_quantDataDisplaySelection";
            this.gb_quantDataDisplaySelection.Size = new System.Drawing.Size(199, 160);
            this.gb_quantDataDisplaySelection.TabIndex = 5;
            this.gb_quantDataDisplaySelection.TabStop = false;
            this.gb_quantDataDisplaySelection.Text = "Data Display Selection";
            // 
            // rbtn_pValue
            // 
            this.rbtn_pValue.AutoSize = true;
            this.rbtn_pValue.Location = new System.Drawing.Point(18, 118);
            this.rbtn_pValue.Name = "rbtn_pValue";
            this.rbtn_pValue.Size = new System.Drawing.Size(89, 24);
            this.rbtn_pValue.TabIndex = 3;
            this.rbtn_pValue.TabStop = true;
            this.rbtn_pValue.Text = "p-Value";
            this.rbtn_pValue.UseVisualStyleBackColor = true;
            // 
            // rbtn_variance
            // 
            this.rbtn_variance.AutoSize = true;
            this.rbtn_variance.Location = new System.Drawing.Point(18, 87);
            this.rbtn_variance.Name = "rbtn_variance";
            this.rbtn_variance.Size = new System.Drawing.Size(97, 24);
            this.rbtn_variance.TabIndex = 2;
            this.rbtn_variance.Text = "Variance";
            this.rbtn_variance.UseVisualStyleBackColor = true;
            // 
            // rbtn_totalIntensity
            // 
            this.rbtn_totalIntensity.AutoSize = true;
            this.rbtn_totalIntensity.Location = new System.Drawing.Point(18, 57);
            this.rbtn_totalIntensity.Name = "rbtn_totalIntensity";
            this.rbtn_totalIntensity.Size = new System.Drawing.Size(133, 24);
            this.rbtn_totalIntensity.TabIndex = 1;
            this.rbtn_totalIntensity.Text = "Total Intensity";
            this.rbtn_totalIntensity.UseVisualStyleBackColor = true;
            // 
            // rbtn_neucodeRatio
            // 
            this.rbtn_neucodeRatio.AutoSize = true;
            this.rbtn_neucodeRatio.Checked = true;
            this.rbtn_neucodeRatio.Location = new System.Drawing.Point(18, 28);
            this.rbtn_neucodeRatio.Name = "rbtn_neucodeRatio";
            this.rbtn_neucodeRatio.Size = new System.Drawing.Size(72, 24);
            this.rbtn_neucodeRatio.TabIndex = 0;
            this.rbtn_neucodeRatio.TabStop = true;
            this.rbtn_neucodeRatio.Text = "Ratio";
            this.rbtn_neucodeRatio.UseVisualStyleBackColor = true;
            // 
            // ct_volcano_logFold_logP
            // 
            chartArea1.Name = "ChartArea1";
            this.ct_volcano_logFold_logP.ChartAreas.Add(chartArea1);
            this.ct_volcano_logFold_logP.Location = new System.Drawing.Point(784, 50);
            this.ct_volcano_logFold_logP.Name = "ct_volcano_logFold_logP";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series1.Name = "Series1";
            this.ct_volcano_logFold_logP.Series.Add(series1);
            this.ct_volcano_logFold_logP.Size = new System.Drawing.Size(714, 499);
            this.ct_volcano_logFold_logP.TabIndex = 6;
            this.ct_volcano_logFold_logP.Text = "Volcano";
            // 
            // cmbx_quantColumns
            // 
            this.cmbx_quantColumns.FormattingEnabled = true;
            this.cmbx_quantColumns.Location = new System.Drawing.Point(1217, 585);
            this.cmbx_quantColumns.Name = "cmbx_quantColumns";
            this.cmbx_quantColumns.Size = new System.Drawing.Size(280, 28);
            this.cmbx_quantColumns.TabIndex = 7;
            this.cmbx_quantColumns.SelectedIndexChanged += new System.EventHandler(this.cmbx_quantColumns_SelectedIndexChanged);
            // 
            // dgv_goAnalysis
            // 
            this.dgv_goAnalysis.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_goAnalysis.Location = new System.Drawing.Point(1515, 50);
            this.dgv_goAnalysis.Name = "dgv_goAnalysis";
            this.dgv_goAnalysis.RowTemplate.Height = 28;
            this.dgv_goAnalysis.Size = new System.Drawing.Size(707, 499);
            this.dgv_goAnalysis.TabIndex = 8;
            // 
            // cmbx_goAspect
            // 
            this.cmbx_goAspect.FormattingEnabled = true;
            this.cmbx_goAspect.Location = new System.Drawing.Point(1942, 585);
            this.cmbx_goAspect.Name = "cmbx_goAspect";
            this.cmbx_goAspect.Size = new System.Drawing.Size(280, 28);
            this.cmbx_goAspect.TabIndex = 9;
            this.cmbx_goAspect.SelectedIndexChanged += new System.EventHandler(this.cmbx_goAspect_SelectedIndexChanged);
            // 
            // gb_goThresholds
            // 
            this.gb_goThresholds.Controls.Add(this.label3);
            this.gb_goThresholds.Controls.Add(this.nud_intensity);
            this.gb_goThresholds.Controls.Add(this.label2);
            this.gb_goThresholds.Controls.Add(this.nud_ratio);
            this.gb_goThresholds.Controls.Add(this.label1);
            this.gb_goThresholds.Controls.Add(this.nud_pValue);
            this.gb_goThresholds.Location = new System.Drawing.Point(1515, 573);
            this.gb_goThresholds.Name = "gb_goThresholds";
            this.gb_goThresholds.Size = new System.Drawing.Size(240, 160);
            this.gb_goThresholds.TabIndex = 10;
            this.gb_goThresholds.TabStop = false;
            this.gb_goThresholds.Text = "GO Term Thresholds";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Intensity >";
            // 
            // nud_intensity
            // 
            this.nud_intensity.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud_intensity.Location = new System.Drawing.Point(106, 118);
            this.nud_intensity.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.nud_intensity.Name = "nud_intensity";
            this.nud_intensity.Size = new System.Drawing.Size(120, 26);
            this.nud_intensity.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "|Ratio| >";
            // 
            // nud_ratio
            // 
            this.nud_ratio.DecimalPlaces = 1;
            this.nud_ratio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nud_ratio.Location = new System.Drawing.Point(106, 78);
            this.nud_ratio.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nud_ratio.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            -2147483648});
            this.nud_ratio.Name = "nud_ratio";
            this.nud_ratio.Size = new System.Drawing.Size(120, 26);
            this.nud_ratio.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "p Value <";
            // 
            // nud_pValue
            // 
            this.nud_pValue.DecimalPlaces = 2;
            this.nud_pValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nud_pValue.Location = new System.Drawing.Point(106, 38);
            this.nud_pValue.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_pValue.Name = "nud_pValue";
            this.nud_pValue.Size = new System.Drawing.Size(120, 26);
            this.nud_pValue.TabIndex = 0;
            // 
            // Quantification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2322, 761);
            this.ControlBox = false;
            this.Controls.Add(this.gb_goThresholds);
            this.Controls.Add(this.cmbx_goAspect);
            this.Controls.Add(this.dgv_goAnalysis);
            this.Controls.Add(this.cmbx_quantColumns);
            this.Controls.Add(this.ct_volcano_logFold_logP);
            this.Controls.Add(this.gb_quantDataDisplaySelection);
            this.Controls.Add(this.ckbx_compressBioReps);
            this.Controls.Add(this.ckbx_compressFractions);
            this.Controls.Add(this.ckbx_compressTechReps);
            this.Controls.Add(this.ckbx_columnNormalize);
            this.Controls.Add(this.dgv_quantification_results);
            this.Name = "Quantification";
            this.Text = "Quantification";
            this.Load += new System.EventHandler(this.Quantification_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantification_results)).EndInit();
            this.gb_quantDataDisplaySelection.ResumeLayout(false);
            this.gb_quantDataDisplaySelection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ct_volcano_logFold_logP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_goAnalysis)).EndInit();
            this.gb_goThresholds.ResumeLayout(false);
            this.gb_goThresholds.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_intensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_ratio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_pValue)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_quantification_results;
        private System.Windows.Forms.CheckBox ckbx_columnNormalize;
        private System.Windows.Forms.CheckBox ckbx_compressTechReps;
        private System.Windows.Forms.CheckBox ckbx_compressFractions;
        private System.Windows.Forms.CheckBox ckbx_compressBioReps;
        private System.Windows.Forms.GroupBox gb_quantDataDisplaySelection;
        private System.Windows.Forms.RadioButton rbtn_variance;
        private System.Windows.Forms.RadioButton rbtn_totalIntensity;
        private System.Windows.Forms.RadioButton rbtn_neucodeRatio;
        private System.Windows.Forms.RadioButton rbtn_pValue;
        private System.Windows.Forms.DataVisualization.Charting.Chart ct_volcano_logFold_logP;
        private System.Windows.Forms.ComboBox cmbx_quantColumns;
        private System.Windows.Forms.DataGridView dgv_goAnalysis;
        private System.Windows.Forms.ComboBox cmbx_goAspect;
        private System.Windows.Forms.GroupBox gb_goThresholds;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nud_intensity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nud_ratio;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nud_pValue;
    }
}