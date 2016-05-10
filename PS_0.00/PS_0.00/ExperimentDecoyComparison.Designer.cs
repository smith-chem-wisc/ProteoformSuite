namespace PS_0._00
{
    partial class ExperimentDecoyComparison
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
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.ct_ED_Histogram = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.dgv_ED_Peak_List = new System.Windows.Forms.DataGridView();
            this.nUD_ED_Upper_Bound = new System.Windows.Forms.NumericUpDown();
            this.nUD_ED_Lower_Bound = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nUD_PeakWidthBase = new System.Windows.Forms.NumericUpDown();
            this.ct_ED_peakList = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tb_TotalPeaks = new System.Windows.Forms.TextBox();
            this.nUD_NoManLower = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.nUD_NoManUpper = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.xMinED = new System.Windows.Forms.NumericUpDown();
            this.xMaxED = new System.Windows.Forms.NumericUpDown();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.dgv_ED_Pairs = new System.Windows.Forms.DataGridView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.nud_Decoy_Database = new System.Windows.Forms.NumericUpDown();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.yMaxED = new System.Windows.Forms.NumericUpDown();
            this.yMinED = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ct_ED_Histogram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ED_Peak_List)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_ED_Upper_Bound)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_ED_Lower_Bound)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_PeakWidthBase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_ED_peakList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_NoManLower)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_NoManUpper)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xMinED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xMaxED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ED_Pairs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Decoy_Database)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yMaxED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yMinED)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.SuspendLayout();
            // 
            // ct_ED_Histogram
            // 
            chartArea1.Name = "ChartArea1";
            this.ct_ED_Histogram.ChartAreas.Add(chartArea1);
            this.ct_ED_Histogram.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ct_ED_Histogram.Location = new System.Drawing.Point(0, 0);
            this.ct_ED_Histogram.Name = "ct_ED_Histogram";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Name = "edHistogram";
            this.ct_ED_Histogram.Series.Add(series1);
            this.ct_ED_Histogram.Size = new System.Drawing.Size(336, 269);
            this.ct_ED_Histogram.TabIndex = 0;
            this.ct_ED_Histogram.Text = "chart1";
            // 
            // dgv_ED_Peak_List
            // 
            this.dgv_ED_Peak_List.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_ED_Peak_List.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_ED_Peak_List.Location = new System.Drawing.Point(0, 0);
            this.dgv_ED_Peak_List.Name = "dgv_ED_Peak_List";
            this.dgv_ED_Peak_List.Size = new System.Drawing.Size(184, 272);
            this.dgv_ED_Peak_List.TabIndex = 1;
            // 
            // nUD_ED_Upper_Bound
            // 
            this.nUD_ED_Upper_Bound.Location = new System.Drawing.Point(104, 99);
            this.nUD_ED_Upper_Bound.Name = "nUD_ED_Upper_Bound";
            this.nUD_ED_Upper_Bound.Size = new System.Drawing.Size(120, 20);
            this.nUD_ED_Upper_Bound.TabIndex = 2;
            // 
            // nUD_ED_Lower_Bound
            // 
            this.nUD_ED_Lower_Bound.Location = new System.Drawing.Point(104, 73);
            this.nUD_ED_Lower_Bound.Name = "nUD_ED_Lower_Bound";
            this.nUD_ED_Lower_Bound.Size = new System.Drawing.Size(120, 20);
            this.nUD_ED_Lower_Bound.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Lower (Da)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Upper (Da)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Peak Width Base (Da)";
            // 
            // nUD_PeakWidthBase
            // 
            this.nUD_PeakWidthBase.DecimalPlaces = 3;
            this.nUD_PeakWidthBase.Location = new System.Drawing.Point(96, 37);
            this.nUD_PeakWidthBase.Name = "nUD_PeakWidthBase";
            this.nUD_PeakWidthBase.Size = new System.Drawing.Size(120, 20);
            this.nUD_PeakWidthBase.TabIndex = 7;
            // 
            // ct_ED_peakList
            // 
            chartArea2.Name = "ChartArea1";
            this.ct_ED_peakList.ChartAreas.Add(chartArea2);
            this.ct_ED_peakList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ct_ED_peakList.Location = new System.Drawing.Point(0, 0);
            this.ct_ED_peakList.Name = "ct_ED_peakList";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Name = "edPeakList";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Name = "etPeakList";
            this.ct_ED_peakList.Series.Add(series2);
            this.ct_ED_peakList.Series.Add(series3);
            this.ct_ED_peakList.Size = new System.Drawing.Size(265, 272);
            this.ct_ED_peakList.TabIndex = 8;
            this.ct_ED_peakList.Text = "chart1";
            // 
            // tb_TotalPeaks
            // 
            this.tb_TotalPeaks.Enabled = false;
            this.tb_TotalPeaks.Location = new System.Drawing.Point(110, 8);
            this.tb_TotalPeaks.Name = "tb_TotalPeaks";
            this.tb_TotalPeaks.Size = new System.Drawing.Size(100, 20);
            this.tb_TotalPeaks.TabIndex = 12;
            // 
            // nUD_NoManLower
            // 
            this.nUD_NoManLower.DecimalPlaces = 2;
            this.nUD_NoManLower.Location = new System.Drawing.Point(98, 35);
            this.nUD_NoManLower.Name = "nUD_NoManLower";
            this.nUD_NoManLower.Size = new System.Drawing.Size(120, 20);
            this.nUD_NoManLower.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(-1, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Lower Bound (Da)";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // nUD_NoManUpper
            // 
            this.nUD_NoManUpper.DecimalPlaces = 2;
            this.nUD_NoManUpper.Location = new System.Drawing.Point(98, 61);
            this.nUD_NoManUpper.Name = "nUD_NoManUpper";
            this.nUD_NoManUpper.Size = new System.Drawing.Size(120, 20);
            this.nUD_NoManUpper.TabIndex = 16;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(-1, 63);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(93, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Upper Bound (Da)";
            // 
            // xMinED
            // 
            this.xMinED.DecimalPlaces = 2;
            this.xMinED.Location = new System.Drawing.Point(3, 46);
            this.xMinED.Name = "xMinED";
            this.xMinED.Size = new System.Drawing.Size(120, 20);
            this.xMinED.TabIndex = 17;
            this.xMinED.ValueChanged += new System.EventHandler(this.xMinED_ValueChanged);
            // 
            // xMaxED
            // 
            this.xMaxED.DecimalPlaces = 2;
            this.xMaxED.Location = new System.Drawing.Point(129, 46);
            this.xMaxED.Name = "xMaxED";
            this.xMaxED.Size = new System.Drawing.Size(120, 20);
            this.xMaxED.TabIndex = 18;
            this.xMaxED.ValueChanged += new System.EventHandler(this.xMaxED_ValueChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgv_ED_Peak_List);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ct_ED_peakList);
            this.splitContainer1.Size = new System.Drawing.Size(461, 276);
            this.splitContainer1.SplitterDistance = 188;
            this.splitContainer1.TabIndex = 19;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(834, 553);
            this.splitContainer2.SplitterDistance = 276;
            this.splitContainer2.TabIndex = 20;
            // 
            // splitContainer5
            // 
            this.splitContainer5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.dgv_ED_Pairs);
            this.splitContainer5.Size = new System.Drawing.Size(834, 276);
            this.splitContainer5.SplitterDistance = 461;
            this.splitContainer5.TabIndex = 20;
            // 
            // dgv_ED_Pairs
            // 
            this.dgv_ED_Pairs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_ED_Pairs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_ED_Pairs.Location = new System.Drawing.Point(0, 0);
            this.dgv_ED_Pairs.Name = "dgv_ED_Pairs";
            this.dgv_ED_Pairs.Size = new System.Drawing.Size(365, 272);
            this.dgv_ED_Pairs.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.groupBox4);
            this.splitContainer3.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer3.Panel1.Controls.Add(this.groupBox2);
            this.splitContainer3.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.ct_ED_Histogram);
            this.splitContainer3.Size = new System.Drawing.Size(834, 273);
            this.splitContainer3.SplitterDistance = 490;
            this.splitContainer3.TabIndex = 19;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.nud_Decoy_Database);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.nUD_ED_Upper_Bound);
            this.groupBox4.Controls.Add(this.nUD_ED_Lower_Bound);
            this.groupBox4.Location = new System.Drawing.Point(225, 8);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(263, 133);
            this.groupBox4.TabIndex = 26;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "ED Histogram Boundaries";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Display decoy database:";
            // 
            // nud_Decoy_Database
            // 
            this.nud_Decoy_Database.Location = new System.Drawing.Point(134, 32);
            this.nud_Decoy_Database.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_Decoy_Database.Name = "nud_Decoy_Database";
            this.nud_Decoy_Database.Size = new System.Drawing.Size(120, 20);
            this.nud_Decoy_Database.TabIndex = 21;
            this.nud_Decoy_Database.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_Decoy_Database.ValueChanged += new System.EventHandler(this.nud_Decoy_Database_ValueChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.nUD_PeakWidthBase);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(6, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(218, 64);
            this.groupBox3.TabIndex = 25;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "ED List Parameters";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.xMinED);
            this.groupBox2.Controls.Add(this.xMaxED);
            this.groupBox2.Controls.Add(this.yMaxED);
            this.groupBox2.Controls.Add(this.yMinED);
            this.groupBox2.Location = new System.Drawing.Point(235, 150);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(253, 103);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "ED Histogram Scaling";
            // 
            // yMaxED
            // 
            this.yMaxED.Location = new System.Drawing.Point(69, 20);
            this.yMaxED.Name = "yMaxED";
            this.yMaxED.Size = new System.Drawing.Size(120, 20);
            this.yMaxED.TabIndex = 19;
            this.yMaxED.ValueChanged += new System.EventHandler(this.yMaxED_ValueChanged);
            // 
            // yMinED
            // 
            this.yMinED.Location = new System.Drawing.Point(53, 70);
            this.yMinED.Name = "yMinED";
            this.yMinED.Size = new System.Drawing.Size(120, 20);
            this.yMinED.TabIndex = 20;
            this.yMinED.ValueChanged += new System.EventHandler(this.yMinED_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.nUD_NoManUpper);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.nUD_NoManLower);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(6, 150);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(223, 118);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "No Man\'s Land";
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
            this.splitContainer4.Panel1.Controls.Add(this.label7);
            this.splitContainer4.Panel1.Controls.Add(this.tb_TotalPeaks);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer4.Size = new System.Drawing.Size(834, 588);
            this.splitContainer4.SplitterDistance = 31;
            this.splitContainer4.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Total ED counts";
            // 
            // ExperimentDecoyComparison
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 588);
            this.Controls.Add(this.splitContainer4);
            this.Name = "ExperimentDecoyComparison";
            this.Text = "ExperimentDecoy Comparison";
            this.Load += new System.EventHandler(this.ExperimentDecoyComparison_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ct_ED_Histogram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ED_Peak_List)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_ED_Upper_Bound)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_ED_Lower_Bound)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_PeakWidthBase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ct_ED_peakList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_NoManLower)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_NoManUpper)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xMinED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xMaxED)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ED_Pairs)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Decoy_Database)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.yMaxED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yMinED)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart ct_ED_Histogram;
        private System.Windows.Forms.DataGridView dgv_ED_Peak_List;
        private System.Windows.Forms.NumericUpDown nUD_ED_Upper_Bound;
        private System.Windows.Forms.NumericUpDown nUD_ED_Lower_Bound;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nUD_PeakWidthBase;
        private System.Windows.Forms.DataVisualization.Charting.Chart ct_ED_peakList;
        private System.Windows.Forms.TextBox tb_TotalPeaks;
        private System.Windows.Forms.NumericUpDown nUD_NoManLower;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nUD_NoManUpper;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown xMinED;
        private System.Windows.Forms.NumericUpDown xMaxED;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.NumericUpDown yMinED;
        private System.Windows.Forms.NumericUpDown yMaxED;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nud_Decoy_Database;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.DataGridView dgv_ED_Pairs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
    }
}