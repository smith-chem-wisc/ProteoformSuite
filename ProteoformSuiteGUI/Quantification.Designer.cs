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
            this.label6 = new System.Windows.Forms.Label();
            this.cmbx_ratioDenominator = new System.Windows.Forms.ComboBox();
            this.cmbx_ratioNumerator = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rb_allTheoreticalGOTerms = new System.Windows.Forms.RadioButton();
            this.rb_allSampleGOTerms = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btn_buildFamiliesAllGO = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.btn_buildFromSelectedGoTerms = new System.Windows.Forms.Button();
            this.cmbx_edgeLabel = new System.Windows.Forms.ComboBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.cb_moreOpacity = new System.Windows.Forms.CheckBox();
            this.cb_boldLabel = new System.Windows.Forms.CheckBox();
            this.cb_redBorder = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbx_nodeLabelPositioning = new System.Windows.Forms.ComboBox();
            this.cmbx_nodeLayout = new System.Windows.Forms.ComboBox();
            this.cmbx_colorScheme = new System.Windows.Forms.ComboBox();
            this.lb_timeStamp = new System.Windows.Forms.Label();
            this.tb_recentTimeStamp = new System.Windows.Forms.TextBox();
            this.btn_buildSelectedQuantFamilies = new System.Windows.Forms.Button();
            this.btn_buildAllFamilies = new System.Windows.Forms.Button();
            this.label_tempFileFolder = new System.Windows.Forms.Label();
            this.tb_familyBuildFolder = new System.Windows.Forms.TextBox();
            this.btn_browseTempFolder = new System.Windows.Forms.Button();
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
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
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
            this.cmbx_quantColumns.Location = new System.Drawing.Point(811, 370);
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
            this.gb_goThresholds.Location = new System.Drawing.Point(1010, 353);
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
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(200, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "divide by";
            // 
            // cmbx_ratioDenominator
            // 
            this.cmbx_ratioDenominator.Enabled = false;
            this.cmbx_ratioDenominator.FormattingEnabled = true;
            this.cmbx_ratioDenominator.Location = new System.Drawing.Point(281, 17);
            this.cmbx_ratioDenominator.Name = "cmbx_ratioDenominator";
            this.cmbx_ratioDenominator.Size = new System.Drawing.Size(162, 21);
            this.cmbx_ratioDenominator.TabIndex = 1;
            this.cmbx_ratioDenominator.SelectedIndexChanged += new System.EventHandler(this.cmbx_ratioDenominator_SelectedIndexChanged);
            // 
            // cmbx_ratioNumerator
            // 
            this.cmbx_ratioNumerator.Enabled = false;
            this.cmbx_ratioNumerator.FormattingEnabled = true;
            this.cmbx_ratioNumerator.Location = new System.Drawing.Point(17, 17);
            this.cmbx_ratioNumerator.Name = "cmbx_ratioNumerator";
            this.cmbx_ratioNumerator.Size = new System.Drawing.Size(162, 21);
            this.cmbx_ratioNumerator.TabIndex = 0;
            this.cmbx_ratioNumerator.SelectedIndexChanged += new System.EventHandler(this.cmbx_ratioNumerator_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rb_allTheoreticalGOTerms);
            this.groupBox3.Controls.Add(this.rb_allSampleGOTerms);
            this.groupBox3.Location = new System.Drawing.Point(1295, 406);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(188, 70);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Background GO Terms";
            // 
            // rb_allTheoreticalGOTerms
            // 
            this.rb_allTheoreticalGOTerms.AutoSize = true;
            this.rb_allTheoreticalGOTerms.Location = new System.Drawing.Point(23, 39);
            this.rb_allTheoreticalGOTerms.Name = "rb_allTheoreticalGOTerms";
            this.rb_allTheoreticalGOTerms.Size = new System.Drawing.Size(97, 17);
            this.rb_allTheoreticalGOTerms.TabIndex = 1;
            this.rb_allTheoreticalGOTerms.TabStop = true;
            this.rb_allTheoreticalGOTerms.Text = "Theoretical Set";
            this.rb_allTheoreticalGOTerms.UseVisualStyleBackColor = true;
            // 
            // rb_allSampleGOTerms
            // 
            this.rb_allSampleGOTerms.AutoSize = true;
            this.rb_allSampleGOTerms.Location = new System.Drawing.Point(23, 19);
            this.rb_allSampleGOTerms.Name = "rb_allSampleGOTerms";
            this.rb_allSampleGOTerms.Size = new System.Drawing.Size(79, 17);
            this.rb_allSampleGOTerms.TabIndex = 0;
            this.rb_allSampleGOTerms.TabStop = true;
            this.rb_allSampleGOTerms.Text = "Sample Set";
            this.rb_allSampleGOTerms.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btn_buildFamiliesAllGO);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.btn_buildFromSelectedGoTerms);
            this.groupBox4.Controls.Add(this.cmbx_edgeLabel);
            this.groupBox4.Controls.Add(this.groupBox5);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.textBox1);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.cmbx_nodeLabelPositioning);
            this.groupBox4.Controls.Add(this.cmbx_nodeLayout);
            this.groupBox4.Controls.Add(this.cmbx_colorScheme);
            this.groupBox4.Controls.Add(this.lb_timeStamp);
            this.groupBox4.Controls.Add(this.tb_recentTimeStamp);
            this.groupBox4.Controls.Add(this.btn_buildSelectedQuantFamilies);
            this.groupBox4.Controls.Add(this.btn_buildAllFamilies);
            this.groupBox4.Controls.Add(this.label_tempFileFolder);
            this.groupBox4.Controls.Add(this.tb_familyBuildFolder);
            this.groupBox4.Controls.Add(this.btn_browseTempFolder);
            this.groupBox4.Location = new System.Drawing.Point(523, 518);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(960, 357);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Quantified Family Display with Cytoscape";
            // 
            // btn_buildFamiliesAllGO
            // 
            this.btn_buildFamiliesAllGO.Location = new System.Drawing.Point(51, 182);
            this.btn_buildFamiliesAllGO.Name = "btn_buildFamiliesAllGO";
            this.btn_buildFamiliesAllGO.Size = new System.Drawing.Size(255, 23);
            this.btn_buildFamiliesAllGO.TabIndex = 61;
            this.btn_buildFamiliesAllGO.Text = "Build Families with All GO Terms Above Thresholds";
            this.btn_buildFamiliesAllGO.UseVisualStyleBackColor = true;
            this.btn_buildFamiliesAllGO.Click += new System.EventHandler(this.btn_buildFamiliesAllGO_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.SystemColors.Control;
            this.label11.Location = new System.Drawing.Point(555, 142);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(116, 13);
            this.label11.TabIndex = 60;
            this.label11.Text = "Edge Label Information";
            // 
            // btn_buildFromSelectedGoTerms
            // 
            this.btn_buildFromSelectedGoTerms.Location = new System.Drawing.Point(51, 211);
            this.btn_buildFromSelectedGoTerms.Name = "btn_buildFromSelectedGoTerms";
            this.btn_buildFromSelectedGoTerms.Size = new System.Drawing.Size(255, 23);
            this.btn_buildFromSelectedGoTerms.TabIndex = 57;
            this.btn_buildFromSelectedGoTerms.Text = "Build Families with Selected GO Terms";
            this.btn_buildFromSelectedGoTerms.UseVisualStyleBackColor = true;
            this.btn_buildFromSelectedGoTerms.Click += new System.EventHandler(this.btn_buildFromSelectedGoTerms_Click);
            // 
            // cmbx_edgeLabel
            // 
            this.cmbx_edgeLabel.Enabled = false;
            this.cmbx_edgeLabel.FormattingEnabled = true;
            this.cmbx_edgeLabel.Location = new System.Drawing.Point(428, 140);
            this.cmbx_edgeLabel.Name = "cmbx_edgeLabel";
            this.cmbx_edgeLabel.Size = new System.Drawing.Size(121, 21);
            this.cmbx_edgeLabel.TabIndex = 59;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cb_moreOpacity);
            this.groupBox5.Controls.Add(this.cb_boldLabel);
            this.groupBox5.Controls.Add(this.cb_redBorder);
            this.groupBox5.Location = new System.Drawing.Point(428, 181);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(200, 113);
            this.groupBox5.TabIndex = 56;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Highlights for Significant Differences";
            // 
            // cb_moreOpacity
            // 
            this.cb_moreOpacity.AutoSize = true;
            this.cb_moreOpacity.Enabled = false;
            this.cb_moreOpacity.Location = new System.Drawing.Point(21, 76);
            this.cb_moreOpacity.Name = "cb_moreOpacity";
            this.cb_moreOpacity.Size = new System.Drawing.Size(96, 17);
            this.cb_moreOpacity.TabIndex = 58;
            this.cb_moreOpacity.Text = "Higher Opacity";
            this.cb_moreOpacity.UseVisualStyleBackColor = true;
            // 
            // cb_boldLabel
            // 
            this.cb_boldLabel.AutoSize = true;
            this.cb_boldLabel.Location = new System.Drawing.Point(21, 53);
            this.cb_boldLabel.Name = "cb_boldLabel";
            this.cb_boldLabel.Size = new System.Drawing.Size(76, 17);
            this.cb_boldLabel.TabIndex = 57;
            this.cb_boldLabel.Text = "Bold Label";
            this.cb_boldLabel.UseVisualStyleBackColor = true;
            // 
            // cb_redBorder
            // 
            this.cb_redBorder.AutoSize = true;
            this.cb_redBorder.Location = new System.Drawing.Point(21, 30);
            this.cb_redBorder.Name = "cb_redBorder";
            this.cb_redBorder.Size = new System.Drawing.Size(109, 17);
            this.cb_redBorder.TabIndex = 56;
            this.cb_redBorder.Text = "Red Node Border";
            this.cb_redBorder.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.SystemColors.Control;
            this.label7.Location = new System.Drawing.Point(554, 117);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 13);
            this.label7.TabIndex = 54;
            this.label7.Text = "Node Label Example";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(428, 114);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(121, 20);
            this.textBox1.TabIndex = 53;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.SystemColors.Control;
            this.label8.Location = new System.Drawing.Point(555, 89);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(116, 13);
            this.label8.TabIndex = 52;
            this.label8.Text = "Node Label Positioning";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.SystemColors.Control;
            this.label9.Location = new System.Drawing.Point(555, 62);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(68, 13);
            this.label9.TabIndex = 51;
            this.label9.Text = "Node Layout";
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(554, 35);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(102, 13);
            this.label10.TabIndex = 50;
            this.label10.Text = "Node Color Scheme";
            // 
            // cmbx_nodeLabelPositioning
            // 
            this.cmbx_nodeLabelPositioning.FormattingEnabled = true;
            this.cmbx_nodeLabelPositioning.Location = new System.Drawing.Point(428, 86);
            this.cmbx_nodeLabelPositioning.Name = "cmbx_nodeLabelPositioning";
            this.cmbx_nodeLabelPositioning.Size = new System.Drawing.Size(121, 21);
            this.cmbx_nodeLabelPositioning.TabIndex = 49;
            // 
            // cmbx_nodeLayout
            // 
            this.cmbx_nodeLayout.Enabled = false;
            this.cmbx_nodeLayout.FormattingEnabled = true;
            this.cmbx_nodeLayout.Location = new System.Drawing.Point(428, 59);
            this.cmbx_nodeLayout.Name = "cmbx_nodeLayout";
            this.cmbx_nodeLayout.Size = new System.Drawing.Size(121, 21);
            this.cmbx_nodeLayout.TabIndex = 48;
            // 
            // cmbx_colorScheme
            // 
            this.cmbx_colorScheme.FormattingEnabled = true;
            this.cmbx_colorScheme.Location = new System.Drawing.Point(428, 32);
            this.cmbx_colorScheme.Name = "cmbx_colorScheme";
            this.cmbx_colorScheme.Size = new System.Drawing.Size(121, 21);
            this.cmbx_colorScheme.TabIndex = 47;
            // 
            // lb_timeStamp
            // 
            this.lb_timeStamp.AutoSize = true;
            this.lb_timeStamp.Location = new System.Drawing.Point(29, 70);
            this.lb_timeStamp.Name = "lb_timeStamp";
            this.lb_timeStamp.Size = new System.Drawing.Size(127, 13);
            this.lb_timeStamp.TabIndex = 46;
            this.lb_timeStamp.Text = "Most Recent Time Stamp";
            // 
            // tb_recentTimeStamp
            // 
            this.tb_recentTimeStamp.Location = new System.Drawing.Point(171, 67);
            this.tb_recentTimeStamp.Name = "tb_recentTimeStamp";
            this.tb_recentTimeStamp.ReadOnly = true;
            this.tb_recentTimeStamp.Size = new System.Drawing.Size(100, 20);
            this.tb_recentTimeStamp.TabIndex = 45;
            // 
            // btn_buildSelectedQuantFamilies
            // 
            this.btn_buildSelectedQuantFamilies.Location = new System.Drawing.Point(51, 143);
            this.btn_buildSelectedQuantFamilies.Name = "btn_buildSelectedQuantFamilies";
            this.btn_buildSelectedQuantFamilies.Size = new System.Drawing.Size(255, 23);
            this.btn_buildSelectedQuantFamilies.TabIndex = 44;
            this.btn_buildSelectedQuantFamilies.Text = "Build Selected Quantified Families";
            this.btn_buildSelectedQuantFamilies.UseVisualStyleBackColor = true;
            this.btn_buildSelectedQuantFamilies.Click += new System.EventHandler(this.btn_buildSelectedQuantFamilies_Click);
            // 
            // btn_buildAllFamilies
            // 
            this.btn_buildAllFamilies.Location = new System.Drawing.Point(51, 114);
            this.btn_buildAllFamilies.Name = "btn_buildAllFamilies";
            this.btn_buildAllFamilies.Size = new System.Drawing.Size(255, 23);
            this.btn_buildAllFamilies.TabIndex = 43;
            this.btn_buildAllFamilies.Text = "Build All Quantified Families";
            this.btn_buildAllFamilies.UseVisualStyleBackColor = true;
            this.btn_buildAllFamilies.Click += new System.EventHandler(this.btn_buildAllQuantifiedFamilies_Click);
            // 
            // label_tempFileFolder
            // 
            this.label_tempFileFolder.AutoSize = true;
            this.label_tempFileFolder.Location = new System.Drawing.Point(48, 38);
            this.label_tempFileFolder.Name = "label_tempFileFolder";
            this.label_tempFileFolder.Size = new System.Drawing.Size(109, 13);
            this.label_tempFileFolder.TabIndex = 42;
            this.label_tempFileFolder.Text = "Folder for Family Build";
            // 
            // tb_familyBuildFolder
            // 
            this.tb_familyBuildFolder.Location = new System.Drawing.Point(171, 35);
            this.tb_familyBuildFolder.Name = "tb_familyBuildFolder";
            this.tb_familyBuildFolder.Size = new System.Drawing.Size(100, 20);
            this.tb_familyBuildFolder.TabIndex = 41;
            this.tb_familyBuildFolder.TextChanged += new System.EventHandler(this.tb_familyBuildFolder_TextChanged);
            // 
            // btn_browseTempFolder
            // 
            this.btn_browseTempFolder.Location = new System.Drawing.Point(287, 32);
            this.btn_browseTempFolder.Name = "btn_browseTempFolder";
            this.btn_browseTempFolder.Size = new System.Drawing.Size(75, 23);
            this.btn_browseTempFolder.TabIndex = 40;
            this.btn_browseTempFolder.Text = "Browse";
            this.btn_browseTempFolder.UseVisualStyleBackColor = true;
            this.btn_browseTempFolder.Click += new System.EventHandler(this.btn_browseTempFolder_Click);
            // 
            // Quantification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2099, 892);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
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
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton rb_allTheoreticalGOTerms;
        private System.Windows.Forms.RadioButton rb_allSampleGOTerms;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label lb_timeStamp;
        private System.Windows.Forms.TextBox tb_recentTimeStamp;
        private System.Windows.Forms.Button btn_buildSelectedQuantFamilies;
        private System.Windows.Forms.Button btn_buildAllFamilies;
        private System.Windows.Forms.Label label_tempFileFolder;
        private System.Windows.Forms.TextBox tb_familyBuildFolder;
        private System.Windows.Forms.Button btn_browseTempFolder;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cmbx_nodeLabelPositioning;
        private System.Windows.Forms.ComboBox cmbx_nodeLayout;
        private System.Windows.Forms.ComboBox cmbx_colorScheme;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox cb_moreOpacity;
        private System.Windows.Forms.CheckBox cb_boldLabel;
        private System.Windows.Forms.CheckBox cb_redBorder;
        private System.Windows.Forms.Button btn_buildFromSelectedGoTerms;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmbx_edgeLabel;
        private System.Windows.Forms.Button btn_buildFamiliesAllGO;
    }
}