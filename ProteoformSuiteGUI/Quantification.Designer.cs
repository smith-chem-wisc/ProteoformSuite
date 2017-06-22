namespace ProteoformSuiteGUI
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
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Quantification));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.dgv_quantification_results = new System.Windows.Forms.DataGridView();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.nud_minObservations = new System.Windows.Forms.NumericUpDown();
            this.cmbx_observationsTypeRequired = new System.Windows.Forms.ComboBox();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.gb_quantDataDisplaySelection = new System.Windows.Forms.GroupBox();
            this.tb_stdevIntensity = new System.Windows.Forms.TextBox();
            this.tb_avgIntensity = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nud_bkgdWidth = new System.Windows.Forms.NumericUpDown();
            this.nud_bkgdShift = new System.Windows.Forms.NumericUpDown();
            this.ct_proteoformIntensities = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.ct_volcano_logFold_logP = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.btn_refreshCalculation = new System.Windows.Forms.Button();
            this.splitContainer8 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbx_ratioDenominator = new System.Windows.Forms.ComboBox();
            this.cmbx_ratioNumerator = new System.Windows.Forms.ComboBox();
            this.splitContainer9 = new System.Windows.Forms.SplitContainer();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tb_FDR = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.nud_Offset = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.nud_sKnot_minFoldChange = new System.Windows.Forms.NumericUpDown();
            this.ct_relativeDifference = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.splitContainer10 = new System.Windows.Forms.SplitContainer();
            this.dgv_goAnalysis = new System.Windows.Forms.DataGridView();
            this.splitContainer11 = new System.Windows.Forms.SplitContainer();
            this.cmbx_goAspect = new System.Windows.Forms.ComboBox();
            this.splitContainer12 = new System.Windows.Forms.SplitContainer();
            this.splitContainer13 = new System.Windows.Forms.SplitContainer();
            this.gb_goThresholds = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.nud_intensity = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nud_ratio = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.nud_FDR = new System.Windows.Forms.NumericUpDown();
            this.gb_backgroundGoTerms = new System.Windows.Forms.GroupBox();
            this.rb_detectedSampleSet = new System.Windows.Forms.RadioButton();
            this.btn_customBackgroundBrowse = new System.Windows.Forms.Button();
            this.tb_goTermCustomBackground = new System.Windows.Forms.TextBox();
            this.rb_customBackgroundSet = new System.Windows.Forms.RadioButton();
            this.rb_allTheoreticalProteins = new System.Windows.Forms.RadioButton();
            this.rb_quantifiedSampleSet = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.splitContainer14 = new System.Windows.Forms.SplitContainer();
            this.btn_buildFamiliesWithSignificantChange = new System.Windows.Forms.Button();
            this.btn_buildFamiliesAllGO = new System.Windows.Forms.Button();
            this.btn_buildFromSelectedGoTerms = new System.Windows.Forms.Button();
            this.lb_timeStamp = new System.Windows.Forms.Label();
            this.tb_recentTimeStamp = new System.Windows.Forms.TextBox();
            this.btn_buildSelectedQuantFamilies = new System.Windows.Forms.Button();
            this.btn_buildAllFamilies = new System.Windows.Forms.Button();
            this.label_tempFileFolder = new System.Windows.Forms.Label();
            this.tb_familyBuildFolder = new System.Windows.Forms.TextBox();
            this.btn_browseTempFolder = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbx_nodeLabel = new System.Windows.Forms.ComboBox();
            this.cb_geneCentric = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.cmbx_geneLabel = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.cmbx_edgeLabel = new System.Windows.Forms.ComboBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.cb_boldLabel = new System.Windows.Forms.CheckBox();
            this.cb_redBorder = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbx_nodeLabelPositioning = new System.Windows.Forms.ComboBox();
            this.cmbx_nodeLayout = new System.Windows.Forms.ComboBox();
            this.cmbx_colorScheme = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantification_results)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_minObservations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.gb_quantDataDisplaySelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdShift)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_proteoformIntensities)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ct_volcano_logFold_logP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).BeginInit();
            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer8)).BeginInit();
            this.splitContainer8.Panel1.SuspendLayout();
            this.splitContainer8.Panel2.SuspendLayout();
            this.splitContainer8.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer9)).BeginInit();
            this.splitContainer9.Panel1.SuspendLayout();
            this.splitContainer9.Panel2.SuspendLayout();
            this.splitContainer9.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Offset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_sKnot_minFoldChange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_relativeDifference)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer10)).BeginInit();
            this.splitContainer10.Panel1.SuspendLayout();
            this.splitContainer10.Panel2.SuspendLayout();
            this.splitContainer10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_goAnalysis)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer11)).BeginInit();
            this.splitContainer11.Panel1.SuspendLayout();
            this.splitContainer11.Panel2.SuspendLayout();
            this.splitContainer11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer12)).BeginInit();
            this.splitContainer12.Panel1.SuspendLayout();
            this.splitContainer12.Panel2.SuspendLayout();
            this.splitContainer12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer13)).BeginInit();
            this.splitContainer13.Panel1.SuspendLayout();
            this.splitContainer13.Panel2.SuspendLayout();
            this.splitContainer13.SuspendLayout();
            this.gb_goThresholds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_intensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_ratio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_FDR)).BeginInit();
            this.gb_backgroundGoTerms.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer14)).BeginInit();
            this.splitContainer14.Panel1.SuspendLayout();
            this.splitContainer14.Panel2.SuspendLayout();
            this.splitContainer14.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1454, 861);
            this.splitContainer1.SplitterDistance = 484;
            this.splitContainer1.TabIndex = 0;
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
            this.splitContainer3.Panel1.Controls.Add(this.dgv_quantification_results);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(484, 861);
            this.splitContainer3.SplitterDistance = 255;
            this.splitContainer3.TabIndex = 0;
            // 
            // dgv_quantification_results
            // 
            this.dgv_quantification_results.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_quantification_results.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_quantification_results.Location = new System.Drawing.Point(0, 0);
            this.dgv_quantification_results.Margin = new System.Windows.Forms.Padding(2);
            this.dgv_quantification_results.Name = "dgv_quantification_results";
            this.dgv_quantification_results.RowTemplate.Height = 28;
            this.dgv_quantification_results.Size = new System.Drawing.Size(484, 255);
            this.dgv_quantification_results.TabIndex = 19;
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
            this.splitContainer4.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer4.Size = new System.Drawing.Size(484, 602);
            this.splitContainer4.SplitterDistance = 71;
            this.splitContainer4.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.nud_minObservations);
            this.groupBox1.Controls.Add(this.cmbx_observationsTypeRequired);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(484, 71);
            this.groupBox1.TabIndex = 26;
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
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.gb_quantDataDisplaySelection);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.ct_proteoformIntensities);
            this.splitContainer5.Size = new System.Drawing.Size(484, 527);
            this.splitContainer5.SplitterDistance = 78;
            this.splitContainer5.TabIndex = 0;
            // 
            // gb_quantDataDisplaySelection
            // 
            this.gb_quantDataDisplaySelection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gb_quantDataDisplaySelection.Controls.Add(this.tb_stdevIntensity);
            this.gb_quantDataDisplaySelection.Controls.Add(this.tb_avgIntensity);
            this.gb_quantDataDisplaySelection.Controls.Add(this.label16);
            this.gb_quantDataDisplaySelection.Controls.Add(this.label17);
            this.gb_quantDataDisplaySelection.Controls.Add(this.label5);
            this.gb_quantDataDisplaySelection.Controls.Add(this.label4);
            this.gb_quantDataDisplaySelection.Controls.Add(this.nud_bkgdWidth);
            this.gb_quantDataDisplaySelection.Controls.Add(this.nud_bkgdShift);
            this.gb_quantDataDisplaySelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gb_quantDataDisplaySelection.Location = new System.Drawing.Point(0, 0);
            this.gb_quantDataDisplaySelection.Margin = new System.Windows.Forms.Padding(2);
            this.gb_quantDataDisplaySelection.Name = "gb_quantDataDisplaySelection";
            this.gb_quantDataDisplaySelection.Padding = new System.Windows.Forms.Padding(2);
            this.gb_quantDataDisplaySelection.Size = new System.Drawing.Size(484, 78);
            this.gb_quantDataDisplaySelection.TabIndex = 20;
            this.gb_quantDataDisplaySelection.TabStop = false;
            this.gb_quantDataDisplaySelection.Text = "Adjust Background Imputation (Units: Std. Dev. of Selected Pf Intensities)";
            // 
            // tb_stdevIntensity
            // 
            this.tb_stdevIntensity.Location = new System.Drawing.Point(297, 17);
            this.tb_stdevIntensity.Name = "tb_stdevIntensity";
            this.tb_stdevIntensity.ReadOnly = true;
            this.tb_stdevIntensity.Size = new System.Drawing.Size(100, 20);
            this.tb_stdevIntensity.TabIndex = 11;
            // 
            // tb_avgIntensity
            // 
            this.tb_avgIntensity.Location = new System.Drawing.Point(91, 18);
            this.tb_avgIntensity.Name = "tb_avgIntensity";
            this.tb_avgIntensity.ReadOnly = true;
            this.tb_avgIntensity.Size = new System.Drawing.Size(100, 20);
            this.tb_avgIntensity.TabIndex = 10;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(203, 22);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(88, 13);
            this.label16.TabIndex = 9;
            this.label16.Text = "Std Dev Intensity";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(16, 21);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(68, 13);
            this.label17.TabIndex = 8;
            this.label17.Text = "Avg Intensity";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(315, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Bkgd Width";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(185, 55);
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
            this.nud_bkgdWidth.Location = new System.Drawing.Point(382, 51);
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
            5,
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
            this.nud_bkgdShift.Location = new System.Drawing.Point(247, 52);
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
            this.nud_bkgdShift.Value = new decimal(new int[] {
            18,
            0,
            0,
            -2147418112});
            // 
            // ct_proteoformIntensities
            // 
            chartArea1.Name = "ChartArea1";
            this.ct_proteoformIntensities.ChartAreas.Add(chartArea1);
            this.ct_proteoformIntensities.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.ct_proteoformIntensities.Legends.Add(legend1);
            this.ct_proteoformIntensities.Location = new System.Drawing.Point(0, 0);
            this.ct_proteoformIntensities.Name = "ct_proteoformIntensities";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "Series2";
            this.ct_proteoformIntensities.Series.Add(series1);
            this.ct_proteoformIntensities.Series.Add(series2);
            this.ct_proteoformIntensities.Size = new System.Drawing.Size(484, 445);
            this.ct_proteoformIntensities.TabIndex = 25;
            this.ct_proteoformIntensities.Text = "log2_intensity";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer6);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer10);
            this.splitContainer2.Size = new System.Drawing.Size(966, 861);
            this.splitContainer2.SplitterDistance = 485;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Name = "splitContainer6";
            this.splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.ct_volcano_logFold_logP);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.splitContainer7);
            this.splitContainer6.Size = new System.Drawing.Size(485, 861);
            this.splitContainer6.SplitterDistance = 309;
            this.splitContainer6.TabIndex = 0;
            // 
            // ct_volcano_logFold_logP
            // 
            chartArea2.Name = "ChartArea1";
            this.ct_volcano_logFold_logP.ChartAreas.Add(chartArea2);
            this.ct_volcano_logFold_logP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ct_volcano_logFold_logP.Location = new System.Drawing.Point(0, 0);
            this.ct_volcano_logFold_logP.Margin = new System.Windows.Forms.Padding(2);
            this.ct_volcano_logFold_logP.Name = "ct_volcano_logFold_logP";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series3.Name = "Series1";
            this.ct_volcano_logFold_logP.Series.Add(series3);
            this.ct_volcano_logFold_logP.Size = new System.Drawing.Size(485, 309);
            this.ct_volcano_logFold_logP.TabIndex = 21;
            this.ct_volcano_logFold_logP.Text = "Volcano";
            // 
            // splitContainer7
            // 
            this.splitContainer7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer7.Location = new System.Drawing.Point(0, 0);
            this.splitContainer7.Name = "splitContainer7";
            this.splitContainer7.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            this.splitContainer7.Panel1.Controls.Add(this.btn_refreshCalculation);
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.Controls.Add(this.splitContainer8);
            this.splitContainer7.Size = new System.Drawing.Size(485, 548);
            this.splitContainer7.SplitterDistance = 41;
            this.splitContainer7.TabIndex = 0;
            // 
            // btn_refreshCalculation
            // 
            this.btn_refreshCalculation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_refreshCalculation.Location = new System.Drawing.Point(0, 0);
            this.btn_refreshCalculation.Name = "btn_refreshCalculation";
            this.btn_refreshCalculation.Size = new System.Drawing.Size(485, 41);
            this.btn_refreshCalculation.TabIndex = 32;
            this.btn_refreshCalculation.Text = "Refresh Calculations";
            this.btn_refreshCalculation.UseVisualStyleBackColor = true;
            // 
            // splitContainer8
            // 
            this.splitContainer8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer8.Location = new System.Drawing.Point(0, 0);
            this.splitContainer8.Name = "splitContainer8";
            this.splitContainer8.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer8.Panel1
            // 
            this.splitContainer8.Panel1.Controls.Add(this.groupBox2);
            // 
            // splitContainer8.Panel2
            // 
            this.splitContainer8.Panel2.Controls.Add(this.splitContainer9);
            this.splitContainer8.Size = new System.Drawing.Size(485, 503);
            this.splitContainer8.SplitterDistance = 60;
            this.splitContainer8.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.cmbx_ratioDenominator);
            this.groupBox2.Controls.Add(this.cmbx_ratioNumerator);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(485, 60);
            this.groupBox2.TabIndex = 27;
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
            // 
            // cmbx_ratioNumerator
            // 
            this.cmbx_ratioNumerator.Enabled = false;
            this.cmbx_ratioNumerator.FormattingEnabled = true;
            this.cmbx_ratioNumerator.Location = new System.Drawing.Point(17, 17);
            this.cmbx_ratioNumerator.Name = "cmbx_ratioNumerator";
            this.cmbx_ratioNumerator.Size = new System.Drawing.Size(162, 21);
            this.cmbx_ratioNumerator.TabIndex = 0;
            // 
            // splitContainer9
            // 
            this.splitContainer9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer9.Location = new System.Drawing.Point(0, 0);
            this.splitContainer9.Name = "splitContainer9";
            this.splitContainer9.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer9.Panel1
            // 
            this.splitContainer9.Panel1.Controls.Add(this.groupBox6);
            // 
            // splitContainer9.Panel2
            // 
            this.splitContainer9.Panel2.Controls.Add(this.ct_relativeDifference);
            this.splitContainer9.Size = new System.Drawing.Size(485, 439);
            this.splitContainer9.SplitterDistance = 47;
            this.splitContainer9.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.label15);
            this.groupBox6.Controls.Add(this.tb_FDR);
            this.groupBox6.Controls.Add(this.label13);
            this.groupBox6.Controls.Add(this.nud_Offset);
            this.groupBox6.Controls.Add(this.label12);
            this.groupBox6.Controls.Add(this.nud_sKnot_minFoldChange);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Location = new System.Drawing.Point(0, 0);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(485, 47);
            this.groupBox6.TabIndex = 31;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "FDR Determination Via Permutation";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(344, 22);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(29, 13);
            this.label15.TabIndex = 7;
            this.label15.Text = "FDR";
            // 
            // tb_FDR
            // 
            this.tb_FDR.Location = new System.Drawing.Point(379, 19);
            this.tb_FDR.Name = "tb_FDR";
            this.tb_FDR.Size = new System.Drawing.Size(64, 20);
            this.tb_FDR.TabIndex = 6;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(203, 22);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(35, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Offset";
            // 
            // nud_Offset
            // 
            this.nud_Offset.DecimalPlaces = 1;
            this.nud_Offset.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nud_Offset.InterceptArrowKeys = false;
            this.nud_Offset.Location = new System.Drawing.Point(244, 19);
            this.nud_Offset.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nud_Offset.Name = "nud_Offset";
            this.nud_Offset.Size = new System.Drawing.Size(60, 20);
            this.nud_Offset.TabIndex = 2;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(20, 13);
            this.label12.TabIndex = 1;
            this.label12.Text = "S0";
            // 
            // nud_sKnot_minFoldChange
            // 
            this.nud_sKnot_minFoldChange.DecimalPlaces = 1;
            this.nud_sKnot_minFoldChange.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nud_sKnot_minFoldChange.Location = new System.Drawing.Point(33, 20);
            this.nud_sKnot_minFoldChange.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nud_sKnot_minFoldChange.Name = "nud_sKnot_minFoldChange";
            this.nud_sKnot_minFoldChange.Size = new System.Drawing.Size(66, 20);
            this.nud_sKnot_minFoldChange.TabIndex = 0;
            // 
            // ct_relativeDifference
            // 
            chartArea3.Name = "ChartArea1";
            this.ct_relativeDifference.ChartAreas.Add(chartArea3);
            this.ct_relativeDifference.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ct_relativeDifference.Location = new System.Drawing.Point(0, 0);
            this.ct_relativeDifference.Name = "ct_relativeDifference";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series4.Name = "obsVSexp";
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series5.Name = "positiveOffset";
            series6.ChartArea = "ChartArea1";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series6.Name = "negativeOffset";
            this.ct_relativeDifference.Series.Add(series4);
            this.ct_relativeDifference.Series.Add(series5);
            this.ct_relativeDifference.Series.Add(series6);
            this.ct_relativeDifference.Size = new System.Drawing.Size(485, 388);
            this.ct_relativeDifference.TabIndex = 30;
            this.ct_relativeDifference.Text = "Observed vs. Expected Relative Difference";
            // 
            // splitContainer10
            // 
            this.splitContainer10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer10.Location = new System.Drawing.Point(0, 0);
            this.splitContainer10.Name = "splitContainer10";
            this.splitContainer10.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer10.Panel1
            // 
            this.splitContainer10.Panel1.Controls.Add(this.dgv_goAnalysis);
            // 
            // splitContainer10.Panel2
            // 
            this.splitContainer10.Panel2.Controls.Add(this.splitContainer11);
            this.splitContainer10.Size = new System.Drawing.Size(477, 861);
            this.splitContainer10.SplitterDistance = 306;
            this.splitContainer10.TabIndex = 0;
            // 
            // dgv_goAnalysis
            // 
            this.dgv_goAnalysis.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_goAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_goAnalysis.Location = new System.Drawing.Point(0, 0);
            this.dgv_goAnalysis.Margin = new System.Windows.Forms.Padding(2);
            this.dgv_goAnalysis.Name = "dgv_goAnalysis";
            this.dgv_goAnalysis.RowTemplate.Height = 28;
            this.dgv_goAnalysis.Size = new System.Drawing.Size(477, 306);
            this.dgv_goAnalysis.TabIndex = 22;
            // 
            // splitContainer11
            // 
            this.splitContainer11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer11.Location = new System.Drawing.Point(0, 0);
            this.splitContainer11.Name = "splitContainer11";
            this.splitContainer11.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer11.Panel1
            // 
            this.splitContainer11.Panel1.Controls.Add(this.cmbx_goAspect);
            // 
            // splitContainer11.Panel2
            // 
            this.splitContainer11.Panel2.Controls.Add(this.splitContainer12);
            this.splitContainer11.Size = new System.Drawing.Size(477, 551);
            this.splitContainer11.SplitterDistance = 30;
            this.splitContainer11.TabIndex = 0;
            // 
            // cmbx_goAspect
            // 
            this.cmbx_goAspect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbx_goAspect.FormattingEnabled = true;
            this.cmbx_goAspect.Location = new System.Drawing.Point(0, 0);
            this.cmbx_goAspect.Margin = new System.Windows.Forms.Padding(2);
            this.cmbx_goAspect.Name = "cmbx_goAspect";
            this.cmbx_goAspect.Size = new System.Drawing.Size(477, 21);
            this.cmbx_goAspect.TabIndex = 23;
            // 
            // splitContainer12
            // 
            this.splitContainer12.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer12.Location = new System.Drawing.Point(0, 0);
            this.splitContainer12.Name = "splitContainer12";
            this.splitContainer12.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer12.Panel1
            // 
            this.splitContainer12.Panel1.Controls.Add(this.splitContainer13);
            // 
            // splitContainer12.Panel2
            // 
            this.splitContainer12.Panel2.Controls.Add(this.groupBox4);
            this.splitContainer12.Size = new System.Drawing.Size(477, 517);
            this.splitContainer12.SplitterDistance = 153;
            this.splitContainer12.TabIndex = 0;
            // 
            // splitContainer13
            // 
            this.splitContainer13.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer13.Location = new System.Drawing.Point(0, 0);
            this.splitContainer13.Name = "splitContainer13";
            // 
            // splitContainer13.Panel1
            // 
            this.splitContainer13.Panel1.Controls.Add(this.gb_goThresholds);
            // 
            // splitContainer13.Panel2
            // 
            this.splitContainer13.Panel2.Controls.Add(this.gb_backgroundGoTerms);
            this.splitContainer13.Size = new System.Drawing.Size(477, 153);
            this.splitContainer13.SplitterDistance = 175;
            this.splitContainer13.TabIndex = 0;
            // 
            // gb_goThresholds
            // 
            this.gb_goThresholds.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gb_goThresholds.Controls.Add(this.label3);
            this.gb_goThresholds.Controls.Add(this.nud_intensity);
            this.gb_goThresholds.Controls.Add(this.label2);
            this.gb_goThresholds.Controls.Add(this.nud_ratio);
            this.gb_goThresholds.Controls.Add(this.label1);
            this.gb_goThresholds.Controls.Add(this.nud_FDR);
            this.gb_goThresholds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gb_goThresholds.Location = new System.Drawing.Point(0, 0);
            this.gb_goThresholds.Margin = new System.Windows.Forms.Padding(2);
            this.gb_goThresholds.Name = "gb_goThresholds";
            this.gb_goThresholds.Padding = new System.Windows.Forms.Padding(2);
            this.gb_goThresholds.Size = new System.Drawing.Size(175, 153);
            this.gb_goThresholds.TabIndex = 24;
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
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "FDR <";
            // 
            // nud_FDR
            // 
            this.nud_FDR.DecimalPlaces = 2;
            this.nud_FDR.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nud_FDR.Location = new System.Drawing.Point(71, 25);
            this.nud_FDR.Margin = new System.Windows.Forms.Padding(2);
            this.nud_FDR.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_FDR.Name = "nud_FDR";
            this.nud_FDR.Size = new System.Drawing.Size(80, 20);
            this.nud_FDR.TabIndex = 0;
            // 
            // gb_backgroundGoTerms
            // 
            this.gb_backgroundGoTerms.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gb_backgroundGoTerms.Controls.Add(this.rb_detectedSampleSet);
            this.gb_backgroundGoTerms.Controls.Add(this.btn_customBackgroundBrowse);
            this.gb_backgroundGoTerms.Controls.Add(this.tb_goTermCustomBackground);
            this.gb_backgroundGoTerms.Controls.Add(this.rb_customBackgroundSet);
            this.gb_backgroundGoTerms.Controls.Add(this.rb_allTheoreticalProteins);
            this.gb_backgroundGoTerms.Controls.Add(this.rb_quantifiedSampleSet);
            this.gb_backgroundGoTerms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gb_backgroundGoTerms.Location = new System.Drawing.Point(0, 0);
            this.gb_backgroundGoTerms.Name = "gb_backgroundGoTerms";
            this.gb_backgroundGoTerms.Size = new System.Drawing.Size(298, 153);
            this.gb_backgroundGoTerms.TabIndex = 29;
            this.gb_backgroundGoTerms.TabStop = false;
            this.gb_backgroundGoTerms.Text = "Background GO Terms";
            // 
            // rb_detectedSampleSet
            // 
            this.rb_detectedSampleSet.AutoSize = true;
            this.rb_detectedSampleSet.Location = new System.Drawing.Point(23, 42);
            this.rb_detectedSampleSet.Name = "rb_detectedSampleSet";
            this.rb_detectedSampleSet.Size = new System.Drawing.Size(126, 17);
            this.rb_detectedSampleSet.TabIndex = 19;
            this.rb_detectedSampleSet.TabStop = true;
            this.rb_detectedSampleSet.Text = "Detected Sample Set";
            this.rb_detectedSampleSet.UseVisualStyleBackColor = true;
            // 
            // btn_customBackgroundBrowse
            // 
            this.btn_customBackgroundBrowse.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_customBackgroundBrowse.Enabled = false;
            this.btn_customBackgroundBrowse.Location = new System.Drawing.Point(212, 77);
            this.btn_customBackgroundBrowse.Name = "btn_customBackgroundBrowse";
            this.btn_customBackgroundBrowse.Size = new System.Drawing.Size(75, 23);
            this.btn_customBackgroundBrowse.TabIndex = 18;
            this.btn_customBackgroundBrowse.Text = "Browse";
            this.btn_customBackgroundBrowse.UseVisualStyleBackColor = true;
            // 
            // tb_goTermCustomBackground
            // 
            this.tb_goTermCustomBackground.Enabled = false;
            this.tb_goTermCustomBackground.Location = new System.Drawing.Point(23, 80);
            this.tb_goTermCustomBackground.Name = "tb_goTermCustomBackground";
            this.tb_goTermCustomBackground.Size = new System.Drawing.Size(183, 20);
            this.tb_goTermCustomBackground.TabIndex = 17;
            // 
            // rb_customBackgroundSet
            // 
            this.rb_customBackgroundSet.AutoSize = true;
            this.rb_customBackgroundSet.Location = new System.Drawing.Point(23, 62);
            this.rb_customBackgroundSet.Name = "rb_customBackgroundSet";
            this.rb_customBackgroundSet.Size = new System.Drawing.Size(126, 17);
            this.rb_customBackgroundSet.TabIndex = 2;
            this.rb_customBackgroundSet.TabStop = true;
            this.rb_customBackgroundSet.Text = "Protein List (Text File)";
            this.rb_customBackgroundSet.UseVisualStyleBackColor = true;
            // 
            // rb_allTheoreticalProteins
            // 
            this.rb_allTheoreticalProteins.AutoSize = true;
            this.rb_allTheoreticalProteins.Location = new System.Drawing.Point(23, 102);
            this.rb_allTheoreticalProteins.Name = "rb_allTheoreticalProteins";
            this.rb_allTheoreticalProteins.Size = new System.Drawing.Size(97, 17);
            this.rb_allTheoreticalProteins.TabIndex = 1;
            this.rb_allTheoreticalProteins.TabStop = true;
            this.rb_allTheoreticalProteins.Text = "Theoretical Set";
            this.rb_allTheoreticalProteins.UseVisualStyleBackColor = true;
            // 
            // rb_quantifiedSampleSet
            // 
            this.rb_quantifiedSampleSet.AutoSize = true;
            this.rb_quantifiedSampleSet.Location = new System.Drawing.Point(23, 23);
            this.rb_quantifiedSampleSet.Name = "rb_quantifiedSampleSet";
            this.rb_quantifiedSampleSet.Size = new System.Drawing.Size(130, 17);
            this.rb_quantifiedSampleSet.TabIndex = 0;
            this.rb_quantifiedSampleSet.TabStop = true;
            this.rb_quantifiedSampleSet.Text = "Quantified Sample Set";
            this.rb_quantifiedSampleSet.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox4.Controls.Add(this.splitContainer14);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(477, 360);
            this.groupBox4.TabIndex = 28;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Quantified Family Display with Cytoscape";
            // 
            // splitContainer14
            // 
            this.splitContainer14.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer14.Location = new System.Drawing.Point(3, 16);
            this.splitContainer14.Name = "splitContainer14";
            // 
            // splitContainer14.Panel1
            // 
            this.splitContainer14.Panel1.Controls.Add(this.btn_buildFamiliesWithSignificantChange);
            this.splitContainer14.Panel1.Controls.Add(this.btn_buildFamiliesAllGO);
            this.splitContainer14.Panel1.Controls.Add(this.cb_geneCentric);
            this.splitContainer14.Panel1.Controls.Add(this.btn_buildFromSelectedGoTerms);
            this.splitContainer14.Panel1.Controls.Add(this.lb_timeStamp);
            this.splitContainer14.Panel1.Controls.Add(this.tb_recentTimeStamp);
            this.splitContainer14.Panel1.Controls.Add(this.btn_buildSelectedQuantFamilies);
            this.splitContainer14.Panel1.Controls.Add(this.btn_buildAllFamilies);
            this.splitContainer14.Panel1.Controls.Add(this.label_tempFileFolder);
            this.splitContainer14.Panel1.Controls.Add(this.tb_familyBuildFolder);
            this.splitContainer14.Panel1.Controls.Add(this.btn_browseTempFolder);
            // 
            // splitContainer14.Panel2
            // 
            this.splitContainer14.Panel2.Controls.Add(this.label7);
            this.splitContainer14.Panel2.Controls.Add(this.cmbx_nodeLabel);
            this.splitContainer14.Panel2.Controls.Add(this.label14);
            this.splitContainer14.Panel2.Controls.Add(this.cmbx_geneLabel);
            this.splitContainer14.Panel2.Controls.Add(this.label11);
            this.splitContainer14.Panel2.Controls.Add(this.cmbx_edgeLabel);
            this.splitContainer14.Panel2.Controls.Add(this.groupBox5);
            this.splitContainer14.Panel2.Controls.Add(this.label8);
            this.splitContainer14.Panel2.Controls.Add(this.label9);
            this.splitContainer14.Panel2.Controls.Add(this.label10);
            this.splitContainer14.Panel2.Controls.Add(this.cmbx_nodeLabelPositioning);
            this.splitContainer14.Panel2.Controls.Add(this.cmbx_nodeLayout);
            this.splitContainer14.Panel2.Controls.Add(this.cmbx_colorScheme);
            this.splitContainer14.Size = new System.Drawing.Size(471, 341);
            this.splitContainer14.SplitterDistance = 267;
            this.splitContainer14.TabIndex = 0;
            // 
            // btn_buildFamiliesWithSignificantChange
            // 
            this.btn_buildFamiliesWithSignificantChange.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_buildFamiliesWithSignificantChange.Location = new System.Drawing.Point(4, 207);
            this.btn_buildFamiliesWithSignificantChange.Name = "btn_buildFamiliesWithSignificantChange";
            this.btn_buildFamiliesWithSignificantChange.Size = new System.Drawing.Size(255, 23);
            this.btn_buildFamiliesWithSignificantChange.TabIndex = 85;
            this.btn_buildFamiliesWithSignificantChange.Text = "Build All Quantified Families w/ Significant Change";
            this.btn_buildFamiliesWithSignificantChange.UseVisualStyleBackColor = true;
            // 
            // btn_buildFamiliesAllGO
            // 
            this.btn_buildFamiliesAllGO.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_buildFamiliesAllGO.Location = new System.Drawing.Point(4, 274);
            this.btn_buildFamiliesAllGO.Name = "btn_buildFamiliesAllGO";
            this.btn_buildFamiliesAllGO.Size = new System.Drawing.Size(255, 23);
            this.btn_buildFamiliesAllGO.TabIndex = 84;
            this.btn_buildFamiliesAllGO.Text = "Build Families with All GO Terms Above Thresholds";
            this.btn_buildFamiliesAllGO.UseVisualStyleBackColor = true;
            // 
            // btn_buildFromSelectedGoTerms
            // 
            this.btn_buildFromSelectedGoTerms.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_buildFromSelectedGoTerms.Location = new System.Drawing.Point(4, 303);
            this.btn_buildFromSelectedGoTerms.Name = "btn_buildFromSelectedGoTerms";
            this.btn_buildFromSelectedGoTerms.Size = new System.Drawing.Size(255, 23);
            this.btn_buildFromSelectedGoTerms.TabIndex = 81;
            this.btn_buildFromSelectedGoTerms.Text = "Build Families with Selected GO Terms";
            this.btn_buildFromSelectedGoTerms.UseVisualStyleBackColor = true;
            // 
            // lb_timeStamp
            // 
            this.lb_timeStamp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lb_timeStamp.AutoSize = true;
            this.lb_timeStamp.Location = new System.Drawing.Point(22, 83);
            this.lb_timeStamp.Name = "lb_timeStamp";
            this.lb_timeStamp.Size = new System.Drawing.Size(127, 13);
            this.lb_timeStamp.TabIndex = 71;
            this.lb_timeStamp.Text = "Most Recent Time Stamp";
            // 
            // tb_recentTimeStamp
            // 
            this.tb_recentTimeStamp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tb_recentTimeStamp.Location = new System.Drawing.Point(157, 80);
            this.tb_recentTimeStamp.Name = "tb_recentTimeStamp";
            this.tb_recentTimeStamp.ReadOnly = true;
            this.tb_recentTimeStamp.Size = new System.Drawing.Size(100, 20);
            this.tb_recentTimeStamp.TabIndex = 70;
            // 
            // btn_buildSelectedQuantFamilies
            // 
            this.btn_buildSelectedQuantFamilies.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_buildSelectedQuantFamilies.Location = new System.Drawing.Point(4, 235);
            this.btn_buildSelectedQuantFamilies.Name = "btn_buildSelectedQuantFamilies";
            this.btn_buildSelectedQuantFamilies.Size = new System.Drawing.Size(255, 23);
            this.btn_buildSelectedQuantFamilies.TabIndex = 69;
            this.btn_buildSelectedQuantFamilies.Text = "Build Selected Quantified Families";
            this.btn_buildSelectedQuantFamilies.UseVisualStyleBackColor = true;
            // 
            // btn_buildAllFamilies
            // 
            this.btn_buildAllFamilies.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_buildAllFamilies.Location = new System.Drawing.Point(4, 181);
            this.btn_buildAllFamilies.Name = "btn_buildAllFamilies";
            this.btn_buildAllFamilies.Size = new System.Drawing.Size(255, 23);
            this.btn_buildAllFamilies.TabIndex = 68;
            this.btn_buildAllFamilies.Text = "Build All Quantified Families";
            this.btn_buildAllFamilies.UseVisualStyleBackColor = true;
            // 
            // label_tempFileFolder
            // 
            this.label_tempFileFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label_tempFileFolder.AutoSize = true;
            this.label_tempFileFolder.Location = new System.Drawing.Point(15, 24);
            this.label_tempFileFolder.Name = "label_tempFileFolder";
            this.label_tempFileFolder.Size = new System.Drawing.Size(109, 13);
            this.label_tempFileFolder.TabIndex = 67;
            this.label_tempFileFolder.Text = "Folder for Family Build";
            // 
            // tb_familyBuildFolder
            // 
            this.tb_familyBuildFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tb_familyBuildFolder.Location = new System.Drawing.Point(157, 21);
            this.tb_familyBuildFolder.Name = "tb_familyBuildFolder";
            this.tb_familyBuildFolder.Size = new System.Drawing.Size(100, 20);
            this.tb_familyBuildFolder.TabIndex = 66;
            // 
            // btn_browseTempFolder
            // 
            this.btn_browseTempFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_browseTempFolder.Location = new System.Drawing.Point(157, 51);
            this.btn_browseTempFolder.Name = "btn_browseTempFolder";
            this.btn_browseTempFolder.Size = new System.Drawing.Size(100, 23);
            this.btn_browseTempFolder.TabIndex = 65;
            this.btn_browseTempFolder.Text = "Browse";
            this.btn_browseTempFolder.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.SystemColors.Control;
            this.label7.Location = new System.Drawing.Point(81, 101);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(117, 13);
            this.label7.TabIndex = 102;
            this.label7.Text = "Node Label Information";
            // 
            // cmbx_nodeLabel
            // 
            this.cmbx_nodeLabel.FormattingEnabled = true;
            this.cmbx_nodeLabel.Location = new System.Drawing.Point(4, 95);
            this.cmbx_nodeLabel.Name = "cmbx_nodeLabel";
            this.cmbx_nodeLabel.Size = new System.Drawing.Size(75, 21);
            this.cmbx_nodeLabel.TabIndex = 101;
            // 
            // cb_geneCentric
            // 
            this.cb_geneCentric.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cb_geneCentric.AutoSize = true;
            this.cb_geneCentric.Checked = true;
            this.cb_geneCentric.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_geneCentric.Location = new System.Drawing.Point(68, 127);
            this.cb_geneCentric.Name = "cb_geneCentric";
            this.cb_geneCentric.Size = new System.Drawing.Size(154, 17);
            this.cb_geneCentric.TabIndex = 100;
            this.cb_geneCentric.Text = "Build Gene-Centric Families";
            this.cb_geneCentric.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.BackColor = System.Drawing.SystemColors.Control;
            this.label14.Location = new System.Drawing.Point(81, 151);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(62, 13);
            this.label14.TabIndex = 96;
            this.label14.Text = "Gene Label";
            // 
            // cmbx_geneLabel
            // 
            this.cmbx_geneLabel.FormattingEnabled = true;
            this.cmbx_geneLabel.Location = new System.Drawing.Point(4, 148);
            this.cmbx_geneLabel.Name = "cmbx_geneLabel";
            this.cmbx_geneLabel.Size = new System.Drawing.Size(75, 21);
            this.cmbx_geneLabel.TabIndex = 93;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.SystemColors.Control;
            this.label11.Location = new System.Drawing.Point(81, 127);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(116, 13);
            this.label11.TabIndex = 99;
            this.label11.Text = "Edge Label Information";
            // 
            // cmbx_edgeLabel
            // 
            this.cmbx_edgeLabel.FormattingEnabled = true;
            this.cmbx_edgeLabel.Location = new System.Drawing.Point(4, 121);
            this.cmbx_edgeLabel.Name = "cmbx_edgeLabel";
            this.cmbx_edgeLabel.Size = new System.Drawing.Size(75, 21);
            this.cmbx_edgeLabel.TabIndex = 98;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cb_boldLabel);
            this.groupBox5.Controls.Add(this.cb_redBorder);
            this.groupBox5.Location = new System.Drawing.Point(0, 190);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(200, 68);
            this.groupBox5.TabIndex = 97;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Highlights for Significant Differences";
            // 
            // cb_boldLabel
            // 
            this.cb_boldLabel.AutoSize = true;
            this.cb_boldLabel.Location = new System.Drawing.Point(16, 42);
            this.cb_boldLabel.Name = "cb_boldLabel";
            this.cb_boldLabel.Size = new System.Drawing.Size(76, 17);
            this.cb_boldLabel.TabIndex = 57;
            this.cb_boldLabel.Text = "Bold Label";
            this.cb_boldLabel.UseVisualStyleBackColor = true;
            // 
            // cb_redBorder
            // 
            this.cb_redBorder.AutoSize = true;
            this.cb_redBorder.Location = new System.Drawing.Point(16, 19);
            this.cb_redBorder.Name = "cb_redBorder";
            this.cb_redBorder.Size = new System.Drawing.Size(109, 17);
            this.cb_redBorder.TabIndex = 56;
            this.cb_redBorder.Text = "Red Node Border";
            this.cb_redBorder.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.SystemColors.Control;
            this.label8.Location = new System.Drawing.Point(81, 73);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(116, 13);
            this.label8.TabIndex = 95;
            this.label8.Text = "Node Label Positioning";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.SystemColors.Control;
            this.label9.Location = new System.Drawing.Point(83, 51);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(68, 13);
            this.label9.TabIndex = 94;
            this.label9.Text = "Node Layout";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(83, 23);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(102, 13);
            this.label10.TabIndex = 92;
            this.label10.Text = "Node Color Scheme";
            // 
            // cmbx_nodeLabelPositioning
            // 
            this.cmbx_nodeLabelPositioning.FormattingEnabled = true;
            this.cmbx_nodeLabelPositioning.Location = new System.Drawing.Point(4, 67);
            this.cmbx_nodeLabelPositioning.Name = "cmbx_nodeLabelPositioning";
            this.cmbx_nodeLabelPositioning.Size = new System.Drawing.Size(75, 21);
            this.cmbx_nodeLabelPositioning.TabIndex = 91;
            // 
            // cmbx_nodeLayout
            // 
            this.cmbx_nodeLayout.FormattingEnabled = true;
            this.cmbx_nodeLayout.Location = new System.Drawing.Point(4, 40);
            this.cmbx_nodeLayout.Name = "cmbx_nodeLayout";
            this.cmbx_nodeLayout.Size = new System.Drawing.Size(75, 21);
            this.cmbx_nodeLayout.TabIndex = 90;
            // 
            // cmbx_colorScheme
            // 
            this.cmbx_colorScheme.FormattingEnabled = true;
            this.cmbx_colorScheme.Location = new System.Drawing.Point(4, 13);
            this.cmbx_colorScheme.Name = "cmbx_colorScheme";
            this.cmbx_colorScheme.Size = new System.Drawing.Size(75, 21);
            this.cmbx_colorScheme.TabIndex = 89;
            // 
            // Quantification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1454, 861);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Quantification";
            this.Text = "Quantification";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantification_results)).EndInit();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nud_minObservations)).EndInit();
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.gb_quantDataDisplaySelection.ResumeLayout(false);
            this.gb_quantDataDisplaySelection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdShift)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_proteoformIntensities)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ct_volcano_logFold_logP)).EndInit();
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).EndInit();
            this.splitContainer7.ResumeLayout(false);
            this.splitContainer8.Panel1.ResumeLayout(false);
            this.splitContainer8.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer8)).EndInit();
            this.splitContainer8.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.splitContainer9.Panel1.ResumeLayout(false);
            this.splitContainer9.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer9)).EndInit();
            this.splitContainer9.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Offset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_sKnot_minFoldChange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_relativeDifference)).EndInit();
            this.splitContainer10.Panel1.ResumeLayout(false);
            this.splitContainer10.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer10)).EndInit();
            this.splitContainer10.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_goAnalysis)).EndInit();
            this.splitContainer11.Panel1.ResumeLayout(false);
            this.splitContainer11.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer11)).EndInit();
            this.splitContainer11.ResumeLayout(false);
            this.splitContainer12.Panel1.ResumeLayout(false);
            this.splitContainer12.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer12)).EndInit();
            this.splitContainer12.ResumeLayout(false);
            this.splitContainer13.Panel1.ResumeLayout(false);
            this.splitContainer13.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer13)).EndInit();
            this.splitContainer13.ResumeLayout(false);
            this.gb_goThresholds.ResumeLayout(false);
            this.gb_goThresholds.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_intensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_ratio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_FDR)).EndInit();
            this.gb_backgroundGoTerms.ResumeLayout(false);
            this.gb_backgroundGoTerms.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.splitContainer14.Panel1.ResumeLayout(false);
            this.splitContainer14.Panel1.PerformLayout();
            this.splitContainer14.Panel2.ResumeLayout(false);
            this.splitContainer14.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer14)).EndInit();
            this.splitContainer14.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private System.Windows.Forms.SplitContainer splitContainer8;
        private System.Windows.Forms.SplitContainer splitContainer9;
        private System.Windows.Forms.SplitContainer splitContainer10;
        private System.Windows.Forms.SplitContainer splitContainer11;
        private System.Windows.Forms.SplitContainer splitContainer12;
        private System.Windows.Forms.SplitContainer splitContainer13;
        private System.Windows.Forms.DataGridView dgv_quantification_results;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown nud_minObservations;
        private System.Windows.Forms.ComboBox cmbx_observationsTypeRequired;
        private System.Windows.Forms.GroupBox gb_quantDataDisplaySelection;
        private System.Windows.Forms.TextBox tb_stdevIntensity;
        private System.Windows.Forms.TextBox tb_avgIntensity;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nud_bkgdWidth;
        private System.Windows.Forms.NumericUpDown nud_bkgdShift;
        public System.Windows.Forms.DataVisualization.Charting.Chart ct_proteoformIntensities;
        public System.Windows.Forms.DataVisualization.Charting.Chart ct_volcano_logFold_logP;
        private System.Windows.Forms.Button btn_refreshCalculation;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbx_ratioDenominator;
        private System.Windows.Forms.ComboBox cmbx_ratioNumerator;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tb_FDR;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown nud_Offset;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown nud_sKnot_minFoldChange;
        public System.Windows.Forms.DataVisualization.Charting.Chart ct_relativeDifference;
        private System.Windows.Forms.DataGridView dgv_goAnalysis;
        private System.Windows.Forms.ComboBox cmbx_goAspect;
        private System.Windows.Forms.GroupBox gb_goThresholds;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nud_intensity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nud_ratio;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nud_FDR;
        private System.Windows.Forms.GroupBox gb_backgroundGoTerms;
        private System.Windows.Forms.RadioButton rb_detectedSampleSet;
        private System.Windows.Forms.Button btn_customBackgroundBrowse;
        private System.Windows.Forms.TextBox tb_goTermCustomBackground;
        private System.Windows.Forms.RadioButton rb_customBackgroundSet;
        private System.Windows.Forms.RadioButton rb_allTheoreticalProteins;
        private System.Windows.Forms.RadioButton rb_quantifiedSampleSet;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.SplitContainer splitContainer14;
        private System.Windows.Forms.Button btn_buildFamiliesWithSignificantChange;
        private System.Windows.Forms.Button btn_buildFamiliesAllGO;
        private System.Windows.Forms.CheckBox cb_geneCentric;
        private System.Windows.Forms.Button btn_buildFromSelectedGoTerms;
        private System.Windows.Forms.Label lb_timeStamp;
        private System.Windows.Forms.TextBox tb_recentTimeStamp;
        private System.Windows.Forms.Button btn_buildSelectedQuantFamilies;
        private System.Windows.Forms.Button btn_buildAllFamilies;
        private System.Windows.Forms.Label label_tempFileFolder;
        private System.Windows.Forms.TextBox tb_familyBuildFolder;
        private System.Windows.Forms.Button btn_browseTempFolder;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbx_nodeLabel;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox cmbx_geneLabel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmbx_edgeLabel;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox cb_boldLabel;
        private System.Windows.Forms.CheckBox cb_redBorder;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cmbx_nodeLabelPositioning;
        private System.Windows.Forms.ComboBox cmbx_nodeLayout;
        private System.Windows.Forms.ComboBox cmbx_colorScheme;
    }
}