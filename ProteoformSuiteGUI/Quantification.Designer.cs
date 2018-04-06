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
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series6 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Quantification));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.dgv_quantification_results = new System.Windows.Forms.DataGridView();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label22 = new System.Windows.Forms.Label();
            this.cmbx_quantitativeValuesTableSelection = new System.Windows.Forms.ComboBox();
            this.nud_minObservations = new System.Windows.Forms.NumericUpDown();
            this.cmbx_observationsTypeRequired = new System.Windows.Forms.ComboBox();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.gb_quantDataDisplaySelection = new System.Windows.Forms.GroupBox();
            this.nud_randomSeed = new System.Windows.Forms.NumericUpDown();
            this.label18 = new System.Windows.Forms.Label();
            this.cb_useRandomSeed = new System.Windows.Forms.CheckBox();
            this.cmbx_intensityDistributionChartSelection = new System.Windows.Forms.ComboBox();
            this.tb_stdevIntensity = new System.Windows.Forms.TextBox();
            this.tb_avgIntensity = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nud_bkgdWidth = new System.Windows.Forms.NumericUpDown();
            this.nud_bkgdShift = new System.Windows.Forms.NumericUpDown();
            this.ct_proteoformIntensities = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.param_splitcontainer = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.ct_volcano_logFold_logP = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.splitContainer7 = new System.Windows.Forms.SplitContainer();
            this.splitContainer15 = new System.Windows.Forms.SplitContainer();
            this.btn_refreshCalculation = new System.Windows.Forms.Button();
            this.rb_significanceByFoldChange = new System.Windows.Forms.RadioButton();
            this.rb_signficanceByPermutation = new System.Windows.Forms.RadioButton();
            this.splitContainer8 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label20 = new System.Windows.Forms.Label();
            this.nUD_min_fold_change = new System.Windows.Forms.NumericUpDown();
            this.label21 = new System.Windows.Forms.Label();
            this.nud_benjiHochFDR = new System.Windows.Forms.NumericUpDown();
            this.cmbx_ratioDenominator = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbx_ratioNumerator = new System.Windows.Forms.ComboBox();
            this.splitContainer9 = new System.Windows.Forms.SplitContainer();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.cb_useAveragePermutationFoldChange = new System.Windows.Forms.CheckBox();
            this.cb_useBiorepPermutationFoldChange = new System.Windows.Forms.CheckBox();
            this.label23 = new System.Windows.Forms.Label();
            this.nud_foldChangeObservations = new System.Windows.Forms.NumericUpDown();
            this.cmbx_foldChangeConjunction = new System.Windows.Forms.ComboBox();
            this.nud_foldChangeCutoff = new System.Windows.Forms.NumericUpDown();
            this.cb_useFoldChangeCutoff = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.cmbx_inducedCondition = new System.Windows.Forms.ComboBox();
            this.label19 = new System.Windows.Forms.Label();
            this.cmbx_relativeDifferenceChartSelection = new System.Windows.Forms.ComboBox();
            this.nud_localFdrCutoff = new System.Windows.Forms.NumericUpDown();
            this.cb_useLocalFdrCutoff = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.tb_FDR = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.nud_Offset = new System.Windows.Forms.NumericUpDown();
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
            this.cb_geneCentric = new System.Windows.Forms.CheckBox();
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
            ((System.ComponentModel.ISupportInitialize)(this.nud_randomSeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdShift)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_proteoformIntensities)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.param_splitcontainer)).BeginInit();
            this.param_splitcontainer.Panel1.SuspendLayout();
            this.param_splitcontainer.Panel2.SuspendLayout();
            this.param_splitcontainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ct_volcano_logFold_logP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).BeginInit();
            this.splitContainer7.Panel1.SuspendLayout();
            this.splitContainer7.Panel2.SuspendLayout();
            this.splitContainer7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer15)).BeginInit();
            this.splitContainer15.Panel1.SuspendLayout();
            this.splitContainer15.Panel2.SuspendLayout();
            this.splitContainer15.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer8)).BeginInit();
            this.splitContainer8.Panel1.SuspendLayout();
            this.splitContainer8.Panel2.SuspendLayout();
            this.splitContainer8.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_fold_change)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_benjiHochFDR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer9)).BeginInit();
            this.splitContainer9.Panel1.SuspendLayout();
            this.splitContainer9.Panel2.SuspendLayout();
            this.splitContainer9.SuspendLayout();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_foldChangeObservations)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_foldChangeCutoff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_localFdrCutoff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Offset)).BeginInit();
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
            this.splitContainer1.Panel2.Controls.Add(this.param_splitcontainer);
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
            this.splitContainer4.Panel1.AutoScroll = true;
            this.splitContainer4.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer4.Size = new System.Drawing.Size(484, 602);
            this.splitContainer4.SplitterDistance = 89;
            this.splitContainer4.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.label22);
            this.groupBox1.Controls.Add(this.cmbx_quantitativeValuesTableSelection);
            this.groupBox1.Controls.Add(this.nud_minObservations);
            this.groupBox1.Controls.Add(this.cmbx_observationsTypeRequired);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(484, 89);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Minimum Required Observations";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(15, 43);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(176, 13);
            this.label22.TabIndex = 16;
            this.label22.Text = "Quantitative Values Table Selection";
            // 
            // cmbx_quantitativeValuesTableSelection
            // 
            this.cmbx_quantitativeValuesTableSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_quantitativeValuesTableSelection.FormattingEnabled = true;
            this.cmbx_quantitativeValuesTableSelection.Location = new System.Drawing.Point(18, 61);
            this.cmbx_quantitativeValuesTableSelection.Name = "cmbx_quantitativeValuesTableSelection";
            this.cmbx_quantitativeValuesTableSelection.Size = new System.Drawing.Size(460, 21);
            this.cmbx_quantitativeValuesTableSelection.TabIndex = 15;
            this.cmbx_quantitativeValuesTableSelection.SelectedIndexChanged += new System.EventHandler(this.cmbx_quantitativeValuesTableSelection_SelectedIndexChanged);
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
            this.nud_minObservations.ValueChanged += new System.EventHandler(this.nud_minObservations_ValueChanged);
            // 
            // cmbx_observationsTypeRequired
            // 
            this.cmbx_observationsTypeRequired.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_observationsTypeRequired.FormattingEnabled = true;
            this.cmbx_observationsTypeRequired.Location = new System.Drawing.Point(77, 19);
            this.cmbx_observationsTypeRequired.Name = "cmbx_observationsTypeRequired";
            this.cmbx_observationsTypeRequired.Size = new System.Drawing.Size(401, 21);
            this.cmbx_observationsTypeRequired.TabIndex = 0;
            this.cmbx_observationsTypeRequired.SelectedIndexChanged += new System.EventHandler(this.cmbx_observationsTypeRequired_SelectedIndexChanged);
            this.cmbx_observationsTypeRequired.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
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
            this.splitContainer5.Panel1.AutoScroll = true;
            this.splitContainer5.Panel1.Controls.Add(this.gb_quantDataDisplaySelection);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.ct_proteoformIntensities);
            this.splitContainer5.Size = new System.Drawing.Size(484, 509);
            this.splitContainer5.SplitterDistance = 124;
            this.splitContainer5.TabIndex = 0;
            // 
            // gb_quantDataDisplaySelection
            // 
            this.gb_quantDataDisplaySelection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gb_quantDataDisplaySelection.Controls.Add(this.nud_randomSeed);
            this.gb_quantDataDisplaySelection.Controls.Add(this.label18);
            this.gb_quantDataDisplaySelection.Controls.Add(this.cb_useRandomSeed);
            this.gb_quantDataDisplaySelection.Controls.Add(this.cmbx_intensityDistributionChartSelection);
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
            this.gb_quantDataDisplaySelection.Size = new System.Drawing.Size(484, 124);
            this.gb_quantDataDisplaySelection.TabIndex = 20;
            this.gb_quantDataDisplaySelection.TabStop = false;
            this.gb_quantDataDisplaySelection.Text = "Imputation from Background. (Shift and width are multiples of observed log2 inten" +
    "sity std. dev.)";
            // 
            // nud_randomSeed
            // 
            this.nud_randomSeed.Location = new System.Drawing.Point(351, 78);
            this.nud_randomSeed.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud_randomSeed.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_randomSeed.Name = "nud_randomSeed";
            this.nud_randomSeed.Size = new System.Drawing.Size(54, 20);
            this.nud_randomSeed.TabIndex = 19;
            this.nud_randomSeed.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_randomSeed.ValueChanged += new System.EventHandler(this.nud_randomSeed_ValueChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(229, 23);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(176, 13);
            this.label18.TabIndex = 14;
            this.label18.Text = "Intensity Distribution Chart Selection";
            // 
            // cb_useRandomSeed
            // 
            this.cb_useRandomSeed.AutoSize = true;
            this.cb_useRandomSeed.Location = new System.Drawing.Point(232, 79);
            this.cb_useRandomSeed.Name = "cb_useRandomSeed";
            this.cb_useRandomSeed.Size = new System.Drawing.Size(119, 17);
            this.cb_useRandomSeed.TabIndex = 18;
            this.cb_useRandomSeed.Text = "Use Random Seed:";
            this.cb_useRandomSeed.UseVisualStyleBackColor = true;
            this.cb_useRandomSeed.CheckedChanged += new System.EventHandler(this.cb_useRandomSeed_CheckedChanged);
            // 
            // cmbx_intensityDistributionChartSelection
            // 
            this.cmbx_intensityDistributionChartSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_intensityDistributionChartSelection.FormattingEnabled = true;
            this.cmbx_intensityDistributionChartSelection.Location = new System.Drawing.Point(232, 41);
            this.cmbx_intensityDistributionChartSelection.Name = "cmbx_intensityDistributionChartSelection";
            this.cmbx_intensityDistributionChartSelection.Size = new System.Drawing.Size(246, 21);
            this.cmbx_intensityDistributionChartSelection.TabIndex = 13;
            this.cmbx_intensityDistributionChartSelection.SelectedIndexChanged += new System.EventHandler(this.cmbx_intensityDistributionChartSelection_SelectedIndexChanged);
            this.cmbx_intensityDistributionChartSelection.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // tb_stdevIntensity
            // 
            this.tb_stdevIntensity.Location = new System.Drawing.Point(127, 44);
            this.tb_stdevIntensity.Name = "tb_stdevIntensity";
            this.tb_stdevIntensity.ReadOnly = true;
            this.tb_stdevIntensity.Size = new System.Drawing.Size(82, 20);
            this.tb_stdevIntensity.TabIndex = 11;
            // 
            // tb_avgIntensity
            // 
            this.tb_avgIntensity.Location = new System.Drawing.Point(127, 19);
            this.tb_avgIntensity.Name = "tb_avgIntensity";
            this.tb_avgIntensity.ReadOnly = true;
            this.tb_avgIntensity.Size = new System.Drawing.Size(82, 20);
            this.tb_avgIntensity.TabIndex = 10;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(5, 51);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(121, 13);
            this.label16.TabIndex = 9;
            this.label16.Text = "Std. Dev. Log2 Intensity";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(5, 26);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(116, 13);
            this.label17.TabIndex = 8;
            this.label17.Text = "Average Log2 Intensity";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(58, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Bkgd Width";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(65, 98);
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
            this.nud_bkgdWidth.Location = new System.Drawing.Point(127, 70);
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
            this.nud_bkgdWidth.ValueChanged += new System.EventHandler(this.nud_bkgdWidth_ValueChanged);
            // 
            // nud_bkgdShift
            // 
            this.nud_bkgdShift.DecimalPlaces = 1;
            this.nud_bkgdShift.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nud_bkgdShift.Location = new System.Drawing.Point(127, 96);
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
            this.nud_bkgdShift.ValueChanged += new System.EventHandler(this.nud_bkgdShift_ValueChanged);
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
            this.ct_proteoformIntensities.Size = new System.Drawing.Size(484, 381);
            this.ct_proteoformIntensities.TabIndex = 25;
            this.ct_proteoformIntensities.Text = "log2_intensity";
            this.ct_proteoformIntensities.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ct_proteoformIntensities_MouseClick);
            // 
            // param_splitcontainer
            // 
            this.param_splitcontainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.param_splitcontainer.Location = new System.Drawing.Point(0, 0);
            this.param_splitcontainer.Name = "param_splitcontainer";
            // 
            // param_splitcontainer.Panel1
            // 
            this.param_splitcontainer.Panel1.Controls.Add(this.splitContainer6);
            // 
            // param_splitcontainer.Panel2
            // 
            this.param_splitcontainer.Panel2.Controls.Add(this.splitContainer10);
            this.param_splitcontainer.Size = new System.Drawing.Size(966, 861);
            this.param_splitcontainer.SplitterDistance = 485;
            this.param_splitcontainer.TabIndex = 0;
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
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.ct_volcano_logFold_logP.Series.Add(series3);
            this.ct_volcano_logFold_logP.Size = new System.Drawing.Size(485, 309);
            this.ct_volcano_logFold_logP.TabIndex = 21;
            this.ct_volcano_logFold_logP.Text = "Volcano";
            this.ct_volcano_logFold_logP.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ct_volcano_logFold_logP_MouseClick);
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
            this.splitContainer7.Panel1.Controls.Add(this.splitContainer15);
            // 
            // splitContainer7.Panel2
            // 
            this.splitContainer7.Panel2.Controls.Add(this.splitContainer8);
            this.splitContainer7.Size = new System.Drawing.Size(485, 548);
            this.splitContainer7.SplitterDistance = 61;
            this.splitContainer7.TabIndex = 0;
            // 
            // splitContainer15
            // 
            this.splitContainer15.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer15.Location = new System.Drawing.Point(0, 0);
            this.splitContainer15.Name = "splitContainer15";
            this.splitContainer15.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer15.Panel1
            // 
            this.splitContainer15.Panel1.Controls.Add(this.btn_refreshCalculation);
            // 
            // splitContainer15.Panel2
            // 
            this.splitContainer15.Panel2.Controls.Add(this.rb_significanceByFoldChange);
            this.splitContainer15.Panel2.Controls.Add(this.rb_signficanceByPermutation);
            this.splitContainer15.Size = new System.Drawing.Size(485, 61);
            this.splitContainer15.SplitterDistance = 25;
            this.splitContainer15.TabIndex = 0;
            // 
            // btn_refreshCalculation
            // 
            this.btn_refreshCalculation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_refreshCalculation.Location = new System.Drawing.Point(0, 0);
            this.btn_refreshCalculation.Name = "btn_refreshCalculation";
            this.btn_refreshCalculation.Size = new System.Drawing.Size(485, 25);
            this.btn_refreshCalculation.TabIndex = 32;
            this.btn_refreshCalculation.Text = "Refresh Calculations";
            this.btn_refreshCalculation.UseVisualStyleBackColor = true;
            this.btn_refreshCalculation.Click += new System.EventHandler(this.btn_refreshCalculation_Click);
            // 
            // rb_significanceByFoldChange
            // 
            this.rb_significanceByFoldChange.AutoSize = true;
            this.rb_significanceByFoldChange.Checked = true;
            this.rb_significanceByFoldChange.Location = new System.Drawing.Point(8, 10);
            this.rb_significanceByFoldChange.Name = "rb_significanceByFoldChange";
            this.rb_significanceByFoldChange.Size = new System.Drawing.Size(201, 17);
            this.rb_significanceByFoldChange.TabIndex = 58;
            this.rb_significanceByFoldChange.TabStop = true;
            this.rb_significanceByFoldChange.Text = "Significance by Fold Change Analysis";
            this.rb_significanceByFoldChange.UseVisualStyleBackColor = true;
            this.rb_significanceByFoldChange.CheckedChanged += new System.EventHandler(this.rb_significanceByFoldChange_CheckedChanged);
            // 
            // rb_signficanceByPermutation
            // 
            this.rb_signficanceByPermutation.AutoSize = true;
            this.rb_signficanceByPermutation.Location = new System.Drawing.Point(216, 10);
            this.rb_signficanceByPermutation.Name = "rb_signficanceByPermutation";
            this.rb_signficanceByPermutation.Size = new System.Drawing.Size(197, 17);
            this.rb_signficanceByPermutation.TabIndex = 59;
            this.rb_signficanceByPermutation.Text = "Significance by Permutation Analysis";
            this.rb_signficanceByPermutation.UseVisualStyleBackColor = true;
            this.rb_signficanceByPermutation.CheckedChanged += new System.EventHandler(this.rb_signficanceByPermutation_CheckedChanged);
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
            this.splitContainer8.Panel1.AutoScroll = true;
            this.splitContainer8.Panel1.Controls.Add(this.groupBox2);
            // 
            // splitContainer8.Panel2
            // 
            this.splitContainer8.Panel2.Controls.Add(this.splitContainer9);
            this.splitContainer8.Size = new System.Drawing.Size(485, 483);
            this.splitContainer8.SplitterDistance = 90;
            this.splitContainer8.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label20);
            this.groupBox2.Controls.Add(this.nUD_min_fold_change);
            this.groupBox2.Controls.Add(this.label21);
            this.groupBox2.Controls.Add(this.nud_benjiHochFDR);
            this.groupBox2.Controls.Add(this.cmbx_ratioDenominator);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.cmbx_ratioNumerator);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(485, 90);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Volcano Plot";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(323, 49);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(111, 13);
            this.label20.TabIndex = 18;
            this.label20.Text = "Minimum Fold Change";
            // 
            // nUD_min_fold_change
            // 
            this.nUD_min_fold_change.DecimalPlaces = 2;
            this.nUD_min_fold_change.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nUD_min_fold_change.Location = new System.Drawing.Point(267, 44);
            this.nUD_min_fold_change.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nUD_min_fold_change.Name = "nUD_min_fold_change";
            this.nUD_min_fold_change.Size = new System.Drawing.Size(50, 20);
            this.nUD_min_fold_change.TabIndex = 19;
            this.nUD_min_fold_change.ValueChanged += new System.EventHandler(this.nUD_min_fold_change_ValueChanged);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(323, 74);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(90, 13);
            this.label21.TabIndex = 16;
            this.label21.Text = "Significance FDR";
            // 
            // nud_benjiHochFDR
            // 
            this.nud_benjiHochFDR.DecimalPlaces = 2;
            this.nud_benjiHochFDR.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nud_benjiHochFDR.Location = new System.Drawing.Point(267, 69);
            this.nud_benjiHochFDR.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_benjiHochFDR.Name = "nud_benjiHochFDR";
            this.nud_benjiHochFDR.Size = new System.Drawing.Size(50, 20);
            this.nud_benjiHochFDR.TabIndex = 17;
            this.nud_benjiHochFDR.ValueChanged += new System.EventHandler(this.nud_benjiHochFDR_ValueChanged);
            // 
            // cmbx_ratioDenominator
            // 
            this.cmbx_ratioDenominator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_ratioDenominator.FormattingEnabled = true;
            this.cmbx_ratioDenominator.Location = new System.Drawing.Point(267, 15);
            this.cmbx_ratioDenominator.Name = "cmbx_ratioDenominator";
            this.cmbx_ratioDenominator.Size = new System.Drawing.Size(212, 21);
            this.cmbx_ratioDenominator.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(209, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "divide by";
            // 
            // cmbx_ratioNumerator
            // 
            this.cmbx_ratioNumerator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_ratioNumerator.FormattingEnabled = true;
            this.cmbx_ratioNumerator.Location = new System.Drawing.Point(9, 15);
            this.cmbx_ratioNumerator.Name = "cmbx_ratioNumerator";
            this.cmbx_ratioNumerator.Size = new System.Drawing.Size(194, 21);
            this.cmbx_ratioNumerator.TabIndex = 7;
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
            this.splitContainer9.Panel1.AutoScroll = true;
            this.splitContainer9.Panel1.Controls.Add(this.groupBox6);
            // 
            // splitContainer9.Panel2
            // 
            this.splitContainer9.Panel2.Controls.Add(this.ct_relativeDifference);
            this.splitContainer9.Size = new System.Drawing.Size(485, 389);
            this.splitContainer9.SplitterDistance = 147;
            this.splitContainer9.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.cb_useAveragePermutationFoldChange);
            this.groupBox6.Controls.Add(this.cb_useBiorepPermutationFoldChange);
            this.groupBox6.Controls.Add(this.label23);
            this.groupBox6.Controls.Add(this.nud_foldChangeObservations);
            this.groupBox6.Controls.Add(this.cmbx_foldChangeConjunction);
            this.groupBox6.Controls.Add(this.nud_foldChangeCutoff);
            this.groupBox6.Controls.Add(this.cb_useFoldChangeCutoff);
            this.groupBox6.Controls.Add(this.label12);
            this.groupBox6.Controls.Add(this.cmbx_inducedCondition);
            this.groupBox6.Controls.Add(this.label19);
            this.groupBox6.Controls.Add(this.cmbx_relativeDifferenceChartSelection);
            this.groupBox6.Controls.Add(this.nud_localFdrCutoff);
            this.groupBox6.Controls.Add(this.cb_useLocalFdrCutoff);
            this.groupBox6.Controls.Add(this.label15);
            this.groupBox6.Controls.Add(this.tb_FDR);
            this.groupBox6.Controls.Add(this.label13);
            this.groupBox6.Controls.Add(this.nud_Offset);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Location = new System.Drawing.Point(0, 0);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(485, 147);
            this.groupBox6.TabIndex = 31;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Significance FDR Via Permutation";
            // 
            // cb_useAveragePermutationFoldChange
            // 
            this.cb_useAveragePermutationFoldChange.AutoSize = true;
            this.cb_useAveragePermutationFoldChange.Location = new System.Drawing.Point(236, 86);
            this.cb_useAveragePermutationFoldChange.Name = "cb_useAveragePermutationFoldChange";
            this.cb_useAveragePermutationFoldChange.Size = new System.Drawing.Size(65, 17);
            this.cb_useAveragePermutationFoldChange.TabIndex = 24;
            this.cb_useAveragePermutationFoldChange.Text = "average";
            this.cb_useAveragePermutationFoldChange.UseVisualStyleBackColor = true;
            this.cb_useAveragePermutationFoldChange.CheckedChanged += new System.EventHandler(this.cb_useAveragePermutationFoldChange_CheckedChanged);
            // 
            // cb_useBiorepPermutationFoldChange
            // 
            this.cb_useBiorepPermutationFoldChange.AutoSize = true;
            this.cb_useBiorepPermutationFoldChange.Location = new System.Drawing.Point(236, 106);
            this.cb_useBiorepPermutationFoldChange.Name = "cb_useBiorepPermutationFoldChange";
            this.cb_useBiorepPermutationFoldChange.Size = new System.Drawing.Size(34, 17);
            this.cb_useBiorepPermutationFoldChange.TabIndex = 23;
            this.cb_useBiorepPermutationFoldChange.Text = "in";
            this.cb_useBiorepPermutationFoldChange.UseVisualStyleBackColor = true;
            this.cb_useBiorepPermutationFoldChange.CheckedChanged += new System.EventHandler(this.cb_useBiorepPermutationFoldChange_CheckedChanged);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(336, 105);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(99, 13);
            this.label23.TabIndex = 22;
            this.label23.Text = "biological replicates";
            // 
            // nud_foldChangeObservations
            // 
            this.nud_foldChangeObservations.Location = new System.Drawing.Point(276, 103);
            this.nud_foldChangeObservations.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud_foldChangeObservations.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_foldChangeObservations.Name = "nud_foldChangeObservations";
            this.nud_foldChangeObservations.Size = new System.Drawing.Size(54, 20);
            this.nud_foldChangeObservations.TabIndex = 20;
            this.nud_foldChangeObservations.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_foldChangeObservations.ValueChanged += new System.EventHandler(this.nud_foldChangeObservations_ValueChanged);
            // 
            // cmbx_foldChangeConjunction
            // 
            this.cmbx_foldChangeConjunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_foldChangeConjunction.FormattingEnabled = true;
            this.cmbx_foldChangeConjunction.Location = new System.Drawing.Point(8, 82);
            this.cmbx_foldChangeConjunction.Name = "cmbx_foldChangeConjunction";
            this.cmbx_foldChangeConjunction.Size = new System.Drawing.Size(57, 21);
            this.cmbx_foldChangeConjunction.TabIndex = 18;
            this.cmbx_foldChangeConjunction.SelectedIndexChanged += new System.EventHandler(this.cmbx_foldChangeConjunction_SelectedIndexChanged);
            // 
            // nud_foldChangeCutoff
            // 
            this.nud_foldChangeCutoff.DecimalPlaces = 1;
            this.nud_foldChangeCutoff.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.nud_foldChangeCutoff.Location = new System.Drawing.Point(161, 84);
            this.nud_foldChangeCutoff.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nud_foldChangeCutoff.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_foldChangeCutoff.Name = "nud_foldChangeCutoff";
            this.nud_foldChangeCutoff.Size = new System.Drawing.Size(69, 20);
            this.nud_foldChangeCutoff.TabIndex = 17;
            this.nud_foldChangeCutoff.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_foldChangeCutoff.ValueChanged += new System.EventHandler(this.nud_permutationFoldChangeCutoff_ValueChanged);
            // 
            // cb_useFoldChangeCutoff
            // 
            this.cb_useFoldChangeCutoff.AutoSize = true;
            this.cb_useFoldChangeCutoff.Location = new System.Drawing.Point(71, 85);
            this.cb_useFoldChangeCutoff.Name = "cb_useFoldChangeCutoff";
            this.cb_useFoldChangeCutoff.Size = new System.Drawing.Size(89, 17);
            this.cb_useFoldChangeCutoff.TabIndex = 16;
            this.cb_useFoldChangeCutoff.Text = "Fold Change:";
            this.cb_useFoldChangeCutoff.UseVisualStyleBackColor = true;
            this.cb_useFoldChangeCutoff.CheckedChanged += new System.EventHandler(this.cb_useFoldChangeCutoff_CheckedChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(240, 15);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(93, 13);
            this.label12.TabIndex = 14;
            this.label12.Text = "Induced Condition";
            // 
            // cmbx_inducedCondition
            // 
            this.cmbx_inducedCondition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_inducedCondition.FormattingEnabled = true;
            this.cmbx_inducedCondition.Location = new System.Drawing.Point(242, 31);
            this.cmbx_inducedCondition.Name = "cmbx_inducedCondition";
            this.cmbx_inducedCondition.Size = new System.Drawing.Size(215, 21);
            this.cmbx_inducedCondition.TabIndex = 13;
            this.cmbx_inducedCondition.SelectedIndexChanged += new System.EventHandler(this.cmbx_inducedCondition_SelectedIndexChanged);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(6, 16);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(173, 13);
            this.label19.TabIndex = 12;
            this.label19.Text = "Relative Difference Chart Selection";
            // 
            // cmbx_relativeDifferenceChartSelection
            // 
            this.cmbx_relativeDifferenceChartSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_relativeDifferenceChartSelection.FormattingEnabled = true;
            this.cmbx_relativeDifferenceChartSelection.Location = new System.Drawing.Point(9, 31);
            this.cmbx_relativeDifferenceChartSelection.Name = "cmbx_relativeDifferenceChartSelection";
            this.cmbx_relativeDifferenceChartSelection.Size = new System.Drawing.Size(219, 21);
            this.cmbx_relativeDifferenceChartSelection.TabIndex = 11;
            this.cmbx_relativeDifferenceChartSelection.SelectedIndexChanged += new System.EventHandler(this.cmbx_relativeDifferenceChartSelection_SelectedIndexChanged);
            this.cmbx_relativeDifferenceChartSelection.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // nud_localFdrCutoff
            // 
            this.nud_localFdrCutoff.DecimalPlaces = 3;
            this.nud_localFdrCutoff.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nud_localFdrCutoff.Location = new System.Drawing.Point(410, 129);
            this.nud_localFdrCutoff.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nud_localFdrCutoff.Name = "nud_localFdrCutoff";
            this.nud_localFdrCutoff.Size = new System.Drawing.Size(69, 20);
            this.nud_localFdrCutoff.TabIndex = 10;
            this.nud_localFdrCutoff.ValueChanged += new System.EventHandler(this.nud_localFdrCutoff_ValueChanged);
            // 
            // cb_useLocalFdrCutoff
            // 
            this.cb_useLocalFdrCutoff.AutoSize = true;
            this.cb_useLocalFdrCutoff.Location = new System.Drawing.Point(242, 132);
            this.cb_useLocalFdrCutoff.Name = "cb_useLocalFdrCutoff";
            this.cb_useLocalFdrCutoff.Size = new System.Drawing.Size(168, 17);
            this.cb_useLocalFdrCutoff.TabIndex = 9;
            this.cb_useLocalFdrCutoff.Text = "Use Rough Local FDR Cutoff:";
            this.cb_useLocalFdrCutoff.UseVisualStyleBackColor = true;
            this.cb_useLocalFdrCutoff.CheckedChanged += new System.EventHandler(this.cb_useLocalFdrCutoff_CheckedChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(8, 133);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(90, 13);
            this.label15.TabIndex = 7;
            this.label15.Text = "Significance FDR";
            // 
            // tb_FDR
            // 
            this.tb_FDR.Location = new System.Drawing.Point(104, 130);
            this.tb_FDR.Name = "tb_FDR";
            this.tb_FDR.ReadOnly = true;
            this.tb_FDR.Size = new System.Drawing.Size(124, 20);
            this.tb_FDR.TabIndex = 6;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 61);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(129, 13);
            this.label13.TabIndex = 3;
            this.label13.Text = "Relative Difference Offset";
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
            this.nud_Offset.Location = new System.Drawing.Point(159, 58);
            this.nud_Offset.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nud_Offset.Name = "nud_Offset";
            this.nud_Offset.Size = new System.Drawing.Size(69, 20);
            this.nud_Offset.TabIndex = 2;
            this.nud_Offset.ValueChanged += new System.EventHandler(this.nud_Offset_ValueChanged);
            // 
            // ct_relativeDifference
            // 
            chartArea3.Name = "ChartArea1";
            this.ct_relativeDifference.ChartAreas.Add(chartArea3);
            this.ct_relativeDifference.Dock = System.Windows.Forms.DockStyle.Fill;
            legend2.Name = "Legend1";
            this.ct_relativeDifference.Legends.Add(legend2);
            this.ct_relativeDifference.Location = new System.Drawing.Point(0, 0);
            this.ct_relativeDifference.Name = "ct_relativeDifference";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;
            series4.Legend = "Legend1";
            series4.Name = "obsVSexp";
            series5.ChartArea = "ChartArea1";
            series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series5.Legend = "Legend1";
            series5.Name = "positiveOffset";
            series6.ChartArea = "ChartArea1";
            series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series6.Legend = "Legend1";
            series6.Name = "negativeOffset";
            this.ct_relativeDifference.Series.Add(series4);
            this.ct_relativeDifference.Series.Add(series5);
            this.ct_relativeDifference.Series.Add(series6);
            this.ct_relativeDifference.Size = new System.Drawing.Size(485, 238);
            this.ct_relativeDifference.TabIndex = 30;
            this.ct_relativeDifference.Text = "Observed vs. Expected Relative Difference";
            this.ct_relativeDifference.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ct_relativeDifference_MouseClick);
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
            this.cmbx_goAspect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_goAspect.FormattingEnabled = true;
            this.cmbx_goAspect.Location = new System.Drawing.Point(0, 0);
            this.cmbx_goAspect.Margin = new System.Windows.Forms.Padding(2);
            this.cmbx_goAspect.Name = "cmbx_goAspect";
            this.cmbx_goAspect.Size = new System.Drawing.Size(477, 21);
            this.cmbx_goAspect.TabIndex = 23;
            this.cmbx_goAspect.SelectedIndexChanged += new System.EventHandler(this.cmbx_goAspect_SelectedIndexChanged);
            this.cmbx_goAspect.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
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
            this.splitContainer13.Panel1.AutoScroll = true;
            this.splitContainer13.Panel1.Controls.Add(this.gb_goThresholds);
            // 
            // splitContainer13.Panel2
            // 
            this.splitContainer13.Panel2.AutoScroll = true;
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
            this.label3.Location = new System.Drawing.Point(18, 79);
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
            this.nud_intensity.Location = new System.Drawing.Point(91, 77);
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
            this.label2.Location = new System.Drawing.Point(18, 52);
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
            this.nud_ratio.Location = new System.Drawing.Point(91, 48);
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
            this.label1.Location = new System.Drawing.Point(4, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Corrected p-value <";
            // 
            // nud_FDR
            // 
            this.nud_FDR.DecimalPlaces = 2;
            this.nud_FDR.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nud_FDR.Location = new System.Drawing.Point(108, 24);
            this.nud_FDR.Margin = new System.Windows.Forms.Padding(2);
            this.nud_FDR.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_FDR.Name = "nud_FDR";
            this.nud_FDR.Size = new System.Drawing.Size(63, 20);
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
            this.rb_detectedSampleSet.CheckedChanged += new System.EventHandler(this.rb_detectedSampleSet_CheckedChanged);
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
            this.btn_customBackgroundBrowse.Click += new System.EventHandler(this.btn_customBackgroundBrowse_Click);
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
            this.rb_customBackgroundSet.CheckedChanged += new System.EventHandler(this.rb_customBackgroundSet_CheckedChanged);
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
            this.rb_allTheoreticalProteins.CheckedChanged += new System.EventHandler(this.rb_allTheoreticalProteins_CheckedChanged);
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
            this.rb_quantifiedSampleSet.CheckedChanged += new System.EventHandler(this.rb_quantifiedSampleSet_CheckedChanged);
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
            this.splitContainer14.Panel1.AutoScroll = true;
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
            this.splitContainer14.Panel2.AutoScroll = true;
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
            this.btn_buildFamiliesWithSignificantChange.Location = new System.Drawing.Point(4, 148);
            this.btn_buildFamiliesWithSignificantChange.Name = "btn_buildFamiliesWithSignificantChange";
            this.btn_buildFamiliesWithSignificantChange.Size = new System.Drawing.Size(255, 23);
            this.btn_buildFamiliesWithSignificantChange.TabIndex = 85;
            this.btn_buildFamiliesWithSignificantChange.Text = "Build All Quantified Families w/ Significant Change";
            this.btn_buildFamiliesWithSignificantChange.UseVisualStyleBackColor = true;
            this.btn_buildFamiliesWithSignificantChange.Click += new System.EventHandler(this.btn_buildFamiliesWithSignificantChange_Click);
            // 
            // btn_buildFamiliesAllGO
            // 
            this.btn_buildFamiliesAllGO.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_buildFamiliesAllGO.Location = new System.Drawing.Point(4, 219);
            this.btn_buildFamiliesAllGO.Name = "btn_buildFamiliesAllGO";
            this.btn_buildFamiliesAllGO.Size = new System.Drawing.Size(255, 23);
            this.btn_buildFamiliesAllGO.TabIndex = 84;
            this.btn_buildFamiliesAllGO.Text = "Build Families with All GO Terms Above Thresholds";
            this.btn_buildFamiliesAllGO.UseVisualStyleBackColor = true;
            this.btn_buildFamiliesAllGO.Click += new System.EventHandler(this.btn_buildFamiliesAllGO_Click);
            // 
            // cb_geneCentric
            // 
            this.cb_geneCentric.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.cb_geneCentric.AutoSize = true;
            this.cb_geneCentric.Checked = true;
            this.cb_geneCentric.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_geneCentric.Location = new System.Drawing.Point(65, 84);
            this.cb_geneCentric.Name = "cb_geneCentric";
            this.cb_geneCentric.Size = new System.Drawing.Size(154, 17);
            this.cb_geneCentric.TabIndex = 100;
            this.cb_geneCentric.Text = "Build Gene-Centric Families";
            this.cb_geneCentric.UseVisualStyleBackColor = true;
            this.cb_geneCentric.CheckedChanged += new System.EventHandler(this.cb_geneCentric_CheckedChanged);
            // 
            // btn_buildFromSelectedGoTerms
            // 
            this.btn_buildFromSelectedGoTerms.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_buildFromSelectedGoTerms.Location = new System.Drawing.Point(4, 248);
            this.btn_buildFromSelectedGoTerms.Name = "btn_buildFromSelectedGoTerms";
            this.btn_buildFromSelectedGoTerms.Size = new System.Drawing.Size(255, 23);
            this.btn_buildFromSelectedGoTerms.TabIndex = 81;
            this.btn_buildFromSelectedGoTerms.Text = "Build Families with Selected GO Terms";
            this.btn_buildFromSelectedGoTerms.UseVisualStyleBackColor = true;
            this.btn_buildFromSelectedGoTerms.Click += new System.EventHandler(this.btn_buildFromSelectedGoTerms_Click);
            // 
            // lb_timeStamp
            // 
            this.lb_timeStamp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lb_timeStamp.AutoSize = true;
            this.lb_timeStamp.Location = new System.Drawing.Point(24, 61);
            this.lb_timeStamp.Name = "lb_timeStamp";
            this.lb_timeStamp.Size = new System.Drawing.Size(127, 13);
            this.lb_timeStamp.TabIndex = 71;
            this.lb_timeStamp.Text = "Most Recent Time Stamp";
            // 
            // tb_recentTimeStamp
            // 
            this.tb_recentTimeStamp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tb_recentTimeStamp.Location = new System.Drawing.Point(159, 58);
            this.tb_recentTimeStamp.Name = "tb_recentTimeStamp";
            this.tb_recentTimeStamp.ReadOnly = true;
            this.tb_recentTimeStamp.Size = new System.Drawing.Size(100, 20);
            this.tb_recentTimeStamp.TabIndex = 70;
            // 
            // btn_buildSelectedQuantFamilies
            // 
            this.btn_buildSelectedQuantFamilies.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_buildSelectedQuantFamilies.Location = new System.Drawing.Point(4, 176);
            this.btn_buildSelectedQuantFamilies.Name = "btn_buildSelectedQuantFamilies";
            this.btn_buildSelectedQuantFamilies.Size = new System.Drawing.Size(255, 23);
            this.btn_buildSelectedQuantFamilies.TabIndex = 69;
            this.btn_buildSelectedQuantFamilies.Text = "Build Selected Quantified Families";
            this.btn_buildSelectedQuantFamilies.UseVisualStyleBackColor = true;
            this.btn_buildSelectedQuantFamilies.Click += new System.EventHandler(this.btn_buildSelectedQuantFamilies_Click);
            // 
            // btn_buildAllFamilies
            // 
            this.btn_buildAllFamilies.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_buildAllFamilies.Location = new System.Drawing.Point(4, 122);
            this.btn_buildAllFamilies.Name = "btn_buildAllFamilies";
            this.btn_buildAllFamilies.Size = new System.Drawing.Size(255, 23);
            this.btn_buildAllFamilies.TabIndex = 68;
            this.btn_buildAllFamilies.Text = "Build All Quantified Families";
            this.btn_buildAllFamilies.UseVisualStyleBackColor = true;
            this.btn_buildAllFamilies.Click += new System.EventHandler(this.btn_buildAllQuantifiedFamilies_Click);
            // 
            // label_tempFileFolder
            // 
            this.label_tempFileFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label_tempFileFolder.AutoSize = true;
            this.label_tempFileFolder.Location = new System.Drawing.Point(15, 6);
            this.label_tempFileFolder.Name = "label_tempFileFolder";
            this.label_tempFileFolder.Size = new System.Drawing.Size(109, 13);
            this.label_tempFileFolder.TabIndex = 67;
            this.label_tempFileFolder.Text = "Folder for Family Build";
            // 
            // tb_familyBuildFolder
            // 
            this.tb_familyBuildFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tb_familyBuildFolder.Location = new System.Drawing.Point(157, 3);
            this.tb_familyBuildFolder.Name = "tb_familyBuildFolder";
            this.tb_familyBuildFolder.Size = new System.Drawing.Size(100, 20);
            this.tb_familyBuildFolder.TabIndex = 66;
            this.tb_familyBuildFolder.TextChanged += new System.EventHandler(this.tb_familyBuildFolder_TextChanged);
            // 
            // btn_browseTempFolder
            // 
            this.btn_browseTempFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btn_browseTempFolder.Location = new System.Drawing.Point(157, 29);
            this.btn_browseTempFolder.Name = "btn_browseTempFolder";
            this.btn_browseTempFolder.Size = new System.Drawing.Size(100, 23);
            this.btn_browseTempFolder.TabIndex = 65;
            this.btn_browseTempFolder.Text = "Browse";
            this.btn_browseTempFolder.UseVisualStyleBackColor = true;
            this.btn_browseTempFolder.Click += new System.EventHandler(this.btn_browseTempFolder_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.SystemColors.Control;
            this.label7.Location = new System.Drawing.Point(-3, 121);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(117, 13);
            this.label7.TabIndex = 102;
            this.label7.Text = "Node Label Information";
            // 
            // cmbx_nodeLabel
            // 
            this.cmbx_nodeLabel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_nodeLabel.FormattingEnabled = true;
            this.cmbx_nodeLabel.Location = new System.Drawing.Point(0, 137);
            this.cmbx_nodeLabel.Name = "cmbx_nodeLabel";
            this.cmbx_nodeLabel.Size = new System.Drawing.Size(200, 21);
            this.cmbx_nodeLabel.TabIndex = 101;
            this.cmbx_nodeLabel.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.BackColor = System.Drawing.SystemColors.Control;
            this.label14.Location = new System.Drawing.Point(0, 201);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(62, 13);
            this.label14.TabIndex = 96;
            this.label14.Text = "Gene Label";
            // 
            // cmbx_geneLabel
            // 
            this.cmbx_geneLabel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_geneLabel.FormattingEnabled = true;
            this.cmbx_geneLabel.Location = new System.Drawing.Point(0, 217);
            this.cmbx_geneLabel.Name = "cmbx_geneLabel";
            this.cmbx_geneLabel.Size = new System.Drawing.Size(200, 21);
            this.cmbx_geneLabel.TabIndex = 93;
            this.cmbx_geneLabel.SelectedIndexChanged += new System.EventHandler(this.cmbx_geneLabel_SelectedIndexChanged);
            this.cmbx_geneLabel.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.SystemColors.Control;
            this.label11.Location = new System.Drawing.Point(-2, 161);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(116, 13);
            this.label11.TabIndex = 99;
            this.label11.Text = "Edge Label Information";
            // 
            // cmbx_edgeLabel
            // 
            this.cmbx_edgeLabel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_edgeLabel.FormattingEnabled = true;
            this.cmbx_edgeLabel.Location = new System.Drawing.Point(0, 177);
            this.cmbx_edgeLabel.Name = "cmbx_edgeLabel";
            this.cmbx_edgeLabel.Size = new System.Drawing.Size(200, 21);
            this.cmbx_edgeLabel.TabIndex = 98;
            this.cmbx_edgeLabel.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cb_boldLabel);
            this.groupBox5.Controls.Add(this.cb_redBorder);
            this.groupBox5.Location = new System.Drawing.Point(1, 244);
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
            this.label8.Location = new System.Drawing.Point(-3, 81);
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
            this.label9.Location = new System.Drawing.Point(2, 41);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(68, 13);
            this.label9.TabIndex = 94;
            this.label9.Text = "Node Layout";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(2, 1);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(102, 13);
            this.label10.TabIndex = 92;
            this.label10.Text = "Node Color Scheme";
            // 
            // cmbx_nodeLabelPositioning
            // 
            this.cmbx_nodeLabelPositioning.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_nodeLabelPositioning.FormattingEnabled = true;
            this.cmbx_nodeLabelPositioning.Location = new System.Drawing.Point(0, 97);
            this.cmbx_nodeLabelPositioning.Name = "cmbx_nodeLabelPositioning";
            this.cmbx_nodeLabelPositioning.Size = new System.Drawing.Size(200, 21);
            this.cmbx_nodeLabelPositioning.TabIndex = 91;
            this.cmbx_nodeLabelPositioning.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // cmbx_nodeLayout
            // 
            this.cmbx_nodeLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_nodeLayout.FormattingEnabled = true;
            this.cmbx_nodeLayout.Location = new System.Drawing.Point(0, 57);
            this.cmbx_nodeLayout.Name = "cmbx_nodeLayout";
            this.cmbx_nodeLayout.Size = new System.Drawing.Size(200, 21);
            this.cmbx_nodeLayout.TabIndex = 90;
            this.cmbx_nodeLayout.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // cmbx_colorScheme
            // 
            this.cmbx_colorScheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_colorScheme.FormattingEnabled = true;
            this.cmbx_colorScheme.Location = new System.Drawing.Point(0, 19);
            this.cmbx_colorScheme.Name = "cmbx_colorScheme";
            this.cmbx_colorScheme.Size = new System.Drawing.Size(200, 21);
            this.cmbx_colorScheme.TabIndex = 89;
            this.cmbx_colorScheme.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // Quantification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
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
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_minObservations)).EndInit();
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.gb_quantDataDisplaySelection.ResumeLayout(false);
            this.gb_quantDataDisplaySelection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_randomSeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_bkgdShift)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_proteoformIntensities)).EndInit();
            this.param_splitcontainer.Panel1.ResumeLayout(false);
            this.param_splitcontainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.param_splitcontainer)).EndInit();
            this.param_splitcontainer.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ct_volcano_logFold_logP)).EndInit();
            this.splitContainer7.Panel1.ResumeLayout(false);
            this.splitContainer7.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer7)).EndInit();
            this.splitContainer7.ResumeLayout(false);
            this.splitContainer15.Panel1.ResumeLayout(false);
            this.splitContainer15.Panel2.ResumeLayout(false);
            this.splitContainer15.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer15)).EndInit();
            this.splitContainer15.ResumeLayout(false);
            this.splitContainer8.Panel1.ResumeLayout(false);
            this.splitContainer8.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer8)).EndInit();
            this.splitContainer8.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_min_fold_change)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_benjiHochFDR)).EndInit();
            this.splitContainer9.Panel1.ResumeLayout(false);
            this.splitContainer9.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer9)).EndInit();
            this.splitContainer9.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_foldChangeObservations)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_foldChangeCutoff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_localFdrCutoff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Offset)).EndInit();
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
        private System.Windows.Forms.SplitContainer param_splitcontainer;
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
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox tb_FDR;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown nud_Offset;
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
        private System.Windows.Forms.NumericUpDown nud_localFdrCutoff;
        private System.Windows.Forms.CheckBox cb_useLocalFdrCutoff;
        private System.Windows.Forms.ComboBox cmbx_intensityDistributionChartSelection;
        private System.Windows.Forms.ComboBox cmbx_relativeDifferenceChartSelection;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cmbx_inducedCondition;
        private System.Windows.Forms.ComboBox cmbx_ratioDenominator;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbx_ratioNumerator;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.NumericUpDown nud_benjiHochFDR;
        private System.Windows.Forms.NumericUpDown nud_foldChangeCutoff;
        private System.Windows.Forms.CheckBox cb_useFoldChangeCutoff;
        private System.Windows.Forms.NumericUpDown nud_randomSeed;
        private System.Windows.Forms.CheckBox cb_useRandomSeed;
        private System.Windows.Forms.NumericUpDown nud_foldChangeObservations;
        private System.Windows.Forms.ComboBox cmbx_foldChangeConjunction;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.CheckBox cb_useAveragePermutationFoldChange;
        private System.Windows.Forms.CheckBox cb_useBiorepPermutationFoldChange;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.ComboBox cmbx_quantitativeValuesTableSelection;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.NumericUpDown nUD_min_fold_change;
        private System.Windows.Forms.SplitContainer splitContainer15;
        private System.Windows.Forms.RadioButton rb_significanceByFoldChange;
        private System.Windows.Forms.RadioButton rb_signficanceByPermutation;
    }
}