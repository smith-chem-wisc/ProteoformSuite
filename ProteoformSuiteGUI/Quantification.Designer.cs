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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.dgv_quantification_results = new System.Windows.Forms.DataGridView();
            this.gb_quantDataDisplaySelection = new System.Windows.Forms.GroupBox();
            this.btn_refreshCalculation = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nud_bkgdWidth = new System.Windows.Forms.NumericUpDown();
            this.nud_bkgdShift = new System.Windows.Forms.NumericUpDown();
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
            this.ct_proteoformIntensities = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.nud_minObservations = new System.Windows.Forms.NumericUpDown();
            this.cmbx_observationsTypeRequired = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmbx_ratioNumerator = new System.Windows.Forms.ComboBox();
            this.cmbx_ratioDenominator = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantification_results)).BeginInit();
            this.gb_quantDataDisplaySelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdShift)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_volcano_logFold_logP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_goAnalysis)).BeginInit();
            this.gb_goThresholds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_intensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_ratio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_pValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_proteoformIntensities)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_minObservations)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgv_quantification_results
            // 
            this.dgv_quantification_results.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_quantification_results.Location = new System.Drawing.Point(55, 32);
            this.dgv_quantification_results.Margin = new System.Windows.Forms.Padding(2);
            this.dgv_quantification_results.Name = "dgv_quantification_results";
            this.dgv_quantification_results.RowTemplate.Height = 28;
            this.dgv_quantification_results.Size = new System.Drawing.Size(452, 298);
            this.dgv_quantification_results.TabIndex = 0;
            // 
            // gb_quantDataDisplaySelection
            // 
            this.gb_quantDataDisplaySelection.Controls.Add(this.btn_refreshCalculation);
            this.gb_quantDataDisplaySelection.Controls.Add(this.label5);
            this.gb_quantDataDisplaySelection.Controls.Add(this.label4);
            this.gb_quantDataDisplaySelection.Controls.Add(this.nud_bkgdWidth);
            this.gb_quantDataDisplaySelection.Controls.Add(this.nud_bkgdShift);
            this.gb_quantDataDisplaySelection.Location = new System.Drawing.Point(55, 462);
            this.gb_quantDataDisplaySelection.Margin = new System.Windows.Forms.Padding(2);
            this.gb_quantDataDisplaySelection.Name = "gb_quantDataDisplaySelection";
            this.gb_quantDataDisplaySelection.Padding = new System.Windows.Forms.Padding(2);
            this.gb_quantDataDisplaySelection.Size = new System.Drawing.Size(452, 48);
            this.gb_quantDataDisplaySelection.TabIndex = 5;
            this.gb_quantDataDisplaySelection.TabStop = false;
            this.gb_quantDataDisplaySelection.Text = "Adjust Background Imputation";
            // 
            // btn_refreshCalculation
            // 
            this.btn_refreshCalculation.Location = new System.Drawing.Point(289, 19);
            this.btn_refreshCalculation.Name = "btn_refreshCalculation";
            this.btn_refreshCalculation.Size = new System.Drawing.Size(138, 23);
            this.btn_refreshCalculation.TabIndex = 8;
            this.btn_refreshCalculation.Text = "Refresh Calculation";
            this.btn_refreshCalculation.UseVisualStyleBackColor = true;
            this.btn_refreshCalculation.Click += new System.EventHandler(this.btn_refreshCalculation_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(154, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Bkgd Width";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Bkgd Shift";
            // 
            // nud_bkgdWidth
            // 
            this.nud_bkgdWidth.DecimalPlaces = 1;
            this.nud_bkgdWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nud_bkgdWidth.Location = new System.Drawing.Point(216, 20);
            this.nud_bkgdWidth.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_bkgdWidth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nud_bkgdWidth.Name = "nud_bkgdWidth";
            this.nud_bkgdWidth.Size = new System.Drawing.Size(48, 20);
            this.nud_bkgdWidth.TabIndex = 5;
            this.nud_bkgdWidth.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            // 
            // nud_bkgdShift
            // 
            this.nud_bkgdShift.DecimalPlaces = 1;
            this.nud_bkgdShift.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nud_bkgdShift.Location = new System.Drawing.Point(77, 20);
            this.nud_bkgdShift.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.nud_bkgdShift.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            -2147483648});
            this.nud_bkgdShift.Name = "nud_bkgdShift";
            this.nud_bkgdShift.Size = new System.Drawing.Size(48, 20);
            this.nud_bkgdShift.TabIndex = 4;
            // 
            // ct_volcano_logFold_logP
            // 
            chartArea1.Name = "ChartArea1";
            this.ct_volcano_logFold_logP.ChartAreas.Add(chartArea1);
            this.ct_volcano_logFold_logP.Location = new System.Drawing.Point(523, 32);
            this.ct_volcano_logFold_logP.Margin = new System.Windows.Forms.Padding(2);
            this.ct_volcano_logFold_logP.Name = "ct_volcano_logFold_logP";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series1.Name = "Series1";
            this.ct_volcano_logFold_logP.Series.Add(series1);
            this.ct_volcano_logFold_logP.Size = new System.Drawing.Size(476, 298);
            this.ct_volcano_logFold_logP.TabIndex = 6;
            this.ct_volcano_logFold_logP.Text = "Volcano";
            // 
            // cmbx_quantColumns
            // 
            this.cmbx_quantColumns.FormattingEnabled = true;
            this.cmbx_quantColumns.Location = new System.Drawing.Point(811, 380);
            this.cmbx_quantColumns.Margin = new System.Windows.Forms.Padding(2);
            this.cmbx_quantColumns.Name = "cmbx_quantColumns";
            this.cmbx_quantColumns.Size = new System.Drawing.Size(188, 21);
            this.cmbx_quantColumns.TabIndex = 7;
            // 
            // dgv_goAnalysis
            // 
            this.dgv_goAnalysis.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_goAnalysis.Location = new System.Drawing.Point(1010, 32);
            this.dgv_goAnalysis.Margin = new System.Windows.Forms.Padding(2);
            this.dgv_goAnalysis.Name = "dgv_goAnalysis";
            this.dgv_goAnalysis.RowTemplate.Height = 28;
            this.dgv_goAnalysis.Size = new System.Drawing.Size(471, 298);
            this.dgv_goAnalysis.TabIndex = 8;
            // 
            // cmbx_goAspect
            // 
            this.cmbx_goAspect.FormattingEnabled = true;
            this.cmbx_goAspect.Location = new System.Drawing.Point(1295, 380);
            this.cmbx_goAspect.Margin = new System.Windows.Forms.Padding(2);
            this.cmbx_goAspect.Name = "cmbx_goAspect";
            this.cmbx_goAspect.Size = new System.Drawing.Size(188, 21);
            this.cmbx_goAspect.TabIndex = 9;
            // 
            // gb_goThresholds
            // 
            this.gb_goThresholds.Controls.Add(this.label3);
            this.gb_goThresholds.Controls.Add(this.nud_intensity);
            this.gb_goThresholds.Controls.Add(this.label2);
            this.gb_goThresholds.Controls.Add(this.nud_ratio);
            this.gb_goThresholds.Controls.Add(this.label1);
            this.gb_goThresholds.Controls.Add(this.nud_pValue);
            this.gb_goThresholds.Location = new System.Drawing.Point(1010, 372);
            this.gb_goThresholds.Margin = new System.Windows.Forms.Padding(2);
            this.gb_goThresholds.Name = "gb_goThresholds";
            this.gb_goThresholds.Padding = new System.Windows.Forms.Padding(2);
            this.gb_goThresholds.Size = new System.Drawing.Size(160, 104);
            this.gb_goThresholds.TabIndex = 10;
            this.gb_goThresholds.TabStop = false;
            this.gb_goThresholds.Text = "GO Term Thresholds";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 78);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
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
            this.nud_intensity.Location = new System.Drawing.Point(71, 77);
            this.nud_intensity.Margin = new System.Windows.Forms.Padding(2);
            this.nud_intensity.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.nud_intensity.Name = "nud_intensity";
            this.nud_intensity.Size = new System.Drawing.Size(80, 20);
            this.nud_intensity.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 52);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
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
            this.nud_ratio.Location = new System.Drawing.Point(71, 51);
            this.nud_ratio.Margin = new System.Windows.Forms.Padding(2);
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
            this.nud_ratio.Size = new System.Drawing.Size(80, 20);
            this.nud_ratio.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
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
            this.nud_pValue.Location = new System.Drawing.Point(71, 25);
            this.nud_pValue.Margin = new System.Windows.Forms.Padding(2);
            this.nud_pValue.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_pValue.Name = "nud_pValue";
            this.nud_pValue.Size = new System.Drawing.Size(80, 20);
            this.nud_pValue.TabIndex = 0;
            // 
            // ct_proteoformIntensities
            // 
            chartArea2.Name = "ChartArea1";
            this.ct_proteoformIntensities.ChartAreas.Add(chartArea2);
            legend1.Name = "Legend1";
            this.ct_proteoformIntensities.Legends.Add(legend1);
            this.ct_proteoformIntensities.Location = new System.Drawing.Point(55, 518);
            this.ct_proteoformIntensities.Name = "ct_proteoformIntensities";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Legend = "Legend1";
            series3.Name = "Series2";
            this.ct_proteoformIntensities.Series.Add(series2);
            this.ct_proteoformIntensities.Series.Add(series3);
            this.ct_proteoformIntensities.Size = new System.Drawing.Size(452, 357);
            this.ct_proteoformIntensities.TabIndex = 11;
            this.ct_proteoformIntensities.Text = "log2_intensity";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.nud_minObservations);
            this.groupBox1.Controls.Add(this.cmbx_observationsTypeRequired);
            this.groupBox1.Location = new System.Drawing.Point(57, 335);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(452, 56);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Minimum Required Observations";
            // 
            // nud_minObservations
            // 
            this.nud_minObservations.Location = new System.Drawing.Point(18, 19);
            this.nud_minObservations.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_minObservations.Name = "nud_minObservations";
            this.nud_minObservations.Size = new System.Drawing.Size(52, 20);
            this.nud_minObservations.TabIndex = 1;
            this.nud_minObservations.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cmbx_observationsTypeRequired
            // 
            this.cmbx_observationsTypeRequired.FormattingEnabled = true;
            this.cmbx_observationsTypeRequired.Location = new System.Drawing.Point(77, 19);
            this.cmbx_observationsTypeRequired.Name = "cmbx_observationsTypeRequired";
            this.cmbx_observationsTypeRequired.Size = new System.Drawing.Size(369, 21);
            this.cmbx_observationsTypeRequired.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.cmbx_ratioDenominator);
            this.groupBox2.Controls.Add(this.cmbx_ratioNumerator);
            this.groupBox2.Location = new System.Drawing.Point(58, 398);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(449, 59);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Ratio";
            // 
            // cmbx_ratioNumerator
            // 
            this.cmbx_ratioNumerator.FormattingEnabled = true;
            this.cmbx_ratioNumerator.Location = new System.Drawing.Point(17, 17);
            this.cmbx_ratioNumerator.Name = "cmbx_ratioNumerator";
            this.cmbx_ratioNumerator.Size = new System.Drawing.Size(162, 21);
            this.cmbx_ratioNumerator.TabIndex = 0;
            // 
            // cmbx_ratioDenominator
            // 
            this.cmbx_ratioDenominator.FormattingEnabled = true;
            this.cmbx_ratioDenominator.Location = new System.Drawing.Point(281, 17);
            this.cmbx_ratioDenominator.Name = "cmbx_ratioDenominator";
            this.cmbx_ratioDenominator.Size = new System.Drawing.Size(162, 21);
            this.cmbx_ratioDenominator.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(200, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "divide by";
            // 
            // Quantification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2099, 892);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ct_proteoformIntensities);
            this.Controls.Add(this.gb_goThresholds);
            this.Controls.Add(this.cmbx_goAspect);
            this.Controls.Add(this.dgv_goAnalysis);
            this.Controls.Add(this.cmbx_quantColumns);
            this.Controls.Add(this.ct_volcano_logFold_logP);
            this.Controls.Add(this.gb_quantDataDisplaySelection);
            this.Controls.Add(this.dgv_quantification_results);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Quantification";
            this.Text = "Quantification";
            this.Load += new System.EventHandler(this.Quantification_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantification_results)).EndInit();
            this.gb_quantDataDisplaySelection.ResumeLayout(false);
            this.gb_quantDataDisplaySelection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdShift)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_volcano_logFold_logP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_goAnalysis)).EndInit();
            this.gb_goThresholds.ResumeLayout(false);
            this.gb_goThresholds.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_intensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_ratio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_pValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_proteoformIntensities)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nud_minObservations)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_quantification_results;
        private System.Windows.Forms.GroupBox gb_quantDataDisplaySelection;
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
        private System.Windows.Forms.DataVisualization.Charting.Chart ct_proteoformIntensities;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nud_bkgdWidth;
        private System.Windows.Forms.NumericUpDown nud_bkgdShift;
        private System.Windows.Forms.Button btn_refreshCalculation;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown nud_minObservations;
        private System.Windows.Forms.ComboBox cmbx_observationsTypeRequired;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbx_ratioDenominator;
        private System.Windows.Forms.ComboBox cmbx_ratioNumerator;
    }
}