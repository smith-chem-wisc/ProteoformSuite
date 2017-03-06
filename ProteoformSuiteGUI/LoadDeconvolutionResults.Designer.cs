namespace ProteoformSuite
{
    partial class LoadDeconvolutionResults
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_td_file = new System.Windows.Forms.CheckBox();
            this.btn_unlabeled = new System.Windows.Forms.RadioButton();
            this.btn_neucode = new System.Windows.Forms.RadioButton();
            this.btn_clearFiles1 = new System.Windows.Forms.Button();
            this.btn_AddFiles1 = new System.Windows.Forms.Button();
            this.btn_addFiles2 = new System.Windows.Forms.Button();
            this.btn_clearFiles2 = new System.Windows.Forms.Button();
            this.btn_addFiles3 = new System.Windows.Forms.Button();
            this.btn_clearFiles3 = new System.Windows.Forms.Button();
            this.dgv_loadFiles1 = new System.Windows.Forms.DataGridView();
            this.dgv_loadFiles2 = new System.Windows.Forms.DataGridView();
            this.dgv_loadFiles3 = new System.Windows.Forms.DataGridView();
            this.btn_fullRun = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_fullRunWithPresets = new System.Windows.Forms.Button();
            this.btn_nextPane = new System.Windows.Forms.Button();
            this.bt_clearResults = new System.Windows.Forms.Button();
            this.cb_run_when_load = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lb_filter3 = new System.Windows.Forms.Label();
            this.tb_filter3 = new System.Windows.Forms.TextBox();
            this.lb_filter2 = new System.Windows.Forms.Label();
            this.lb_filter1 = new System.Windows.Forms.Label();
            this.tb_filter2 = new System.Windows.Forms.TextBox();
            this.tb_filter1 = new System.Windows.Forms.TextBox();
            this.cmb_loadTable1 = new System.Windows.Forms.ComboBox();
            this.cmb_loadTable2 = new System.Windows.Forms.ComboBox();
            this.cmb_loadTable3 = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rb_chemicalCalibration = new System.Windows.Forms.RadioButton();
            this.rb_standardOptions = new System.Windows.Forms.RadioButton();
            this.rb_advanced_user = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_loadFiles1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_loadFiles2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_loadFiles3)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cb_td_file);
            this.groupBox1.Controls.Add(this.btn_unlabeled);
            this.groupBox1.Controls.Add(this.btn_neucode);
            this.groupBox1.Location = new System.Drawing.Point(13, 610);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(213, 114);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Proteoform Identification Results";
            // 
            // cb_td_file
            // 
            this.cb_td_file.AutoSize = true;
            this.cb_td_file.Location = new System.Drawing.Point(23, 91);
            this.cb_td_file.Name = "cb_td_file";
            this.cb_td_file.Size = new System.Drawing.Size(165, 17);
            this.cb_td_file.TabIndex = 2;
            this.cb_td_file.Text = "Top-down Deconvolution File";
            this.cb_td_file.UseVisualStyleBackColor = true;
            this.cb_td_file.Visible = false;
            this.cb_td_file.CheckedChanged += new System.EventHandler(this.cb_td_file_CheckedChanged);
            // 
            // btn_unlabeled
            // 
            this.btn_unlabeled.AutoSize = true;
            this.btn_unlabeled.Location = new System.Drawing.Point(23, 57);
            this.btn_unlabeled.Name = "btn_unlabeled";
            this.btn_unlabeled.Size = new System.Drawing.Size(73, 17);
            this.btn_unlabeled.TabIndex = 1;
            this.btn_unlabeled.Text = "Unlabeled";
            this.btn_unlabeled.UseVisualStyleBackColor = true;
            // 
            // btn_neucode
            // 
            this.btn_neucode.AutoSize = true;
            this.btn_neucode.Checked = true;
            this.btn_neucode.Location = new System.Drawing.Point(23, 26);
            this.btn_neucode.Name = "btn_neucode";
            this.btn_neucode.Size = new System.Drawing.Size(111, 17);
            this.btn_neucode.TabIndex = 0;
            this.btn_neucode.TabStop = true;
            this.btn_neucode.Text = "NeuCode Labeled";
            this.btn_neucode.UseVisualStyleBackColor = true;
            this.btn_neucode.CheckedChanged += new System.EventHandler(this.btn_neucode_CheckedChanged);
            // 
            // btn_clearFiles1
            // 
            this.btn_clearFiles1.Location = new System.Drawing.Point(218, 518);
            this.btn_clearFiles1.Name = "btn_clearFiles1";
            this.btn_clearFiles1.Size = new System.Drawing.Size(122, 36);
            this.btn_clearFiles1.TabIndex = 7;
            this.btn_clearFiles1.Text = "Clear";
            this.btn_clearFiles1.UseVisualStyleBackColor = true;
            this.btn_clearFiles1.Click += new System.EventHandler(this.btn_clearFiles1_Click);
            // 
            // btn_AddFiles1
            // 
            this.btn_AddFiles1.Location = new System.Drawing.Point(90, 518);
            this.btn_AddFiles1.Name = "btn_AddFiles1";
            this.btn_AddFiles1.Size = new System.Drawing.Size(122, 36);
            this.btn_AddFiles1.TabIndex = 8;
            this.btn_AddFiles1.Text = "Add";
            this.btn_AddFiles1.UseVisualStyleBackColor = true;
            this.btn_AddFiles1.Click += new System.EventHandler(this.btn_addFiles1_Click);
            // 
            // btn_addFiles2
            // 
            this.btn_addFiles2.Location = new System.Drawing.Point(552, 518);
            this.btn_addFiles2.Name = "btn_addFiles2";
            this.btn_addFiles2.Size = new System.Drawing.Size(122, 36);
            this.btn_addFiles2.TabIndex = 10;
            this.btn_addFiles2.Text = "Add";
            this.btn_addFiles2.UseVisualStyleBackColor = true;
            this.btn_addFiles2.Click += new System.EventHandler(this.btn_addFiles2_Click);
            // 
            // btn_clearFiles2
            // 
            this.btn_clearFiles2.Location = new System.Drawing.Point(675, 518);
            this.btn_clearFiles2.Name = "btn_clearFiles2";
            this.btn_clearFiles2.Size = new System.Drawing.Size(122, 36);
            this.btn_clearFiles2.TabIndex = 9;
            this.btn_clearFiles2.Text = "Clear";
            this.btn_clearFiles2.UseVisualStyleBackColor = true;
            this.btn_clearFiles2.Click += new System.EventHandler(this.btn_clearFiles2_Click);
            // 
            // btn_addFiles3
            // 
            this.btn_addFiles3.Location = new System.Drawing.Point(991, 518);
            this.btn_addFiles3.Name = "btn_addFiles3";
            this.btn_addFiles3.Size = new System.Drawing.Size(122, 36);
            this.btn_addFiles3.TabIndex = 12;
            this.btn_addFiles3.Text = "Add";
            this.btn_addFiles3.UseVisualStyleBackColor = true;
            this.btn_addFiles3.Click += new System.EventHandler(this.btn_addFiles3_Click);
            // 
            // btn_clearFiles3
            // 
            this.btn_clearFiles3.Location = new System.Drawing.Point(1119, 518);
            this.btn_clearFiles3.Name = "btn_clearFiles3";
            this.btn_clearFiles3.Size = new System.Drawing.Size(122, 36);
            this.btn_clearFiles3.TabIndex = 11;
            this.btn_clearFiles3.Text = "Clear";
            this.btn_clearFiles3.UseVisualStyleBackColor = true;
            this.btn_clearFiles3.Click += new System.EventHandler(this.btn_clearFiles3_Click);
            // 
            // dgv_loadFiles1
            // 
            this.dgv_loadFiles1.AllowDrop = true;
            this.dgv_loadFiles1.AllowUserToOrderColumns = true;
            this.dgv_loadFiles1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_loadFiles1.Location = new System.Drawing.Point(12, 49);
            this.dgv_loadFiles1.Name = "dgv_loadFiles1";
            this.dgv_loadFiles1.RowTemplate.Height = 28;
            this.dgv_loadFiles1.Size = new System.Drawing.Size(430, 463);
            this.dgv_loadFiles1.TabIndex = 13;
            this.dgv_loadFiles1.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgv_loadFiles1_CellFormatting);
            this.dgv_loadFiles1.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgv_deconResults_DragDrop);
            this.dgv_loadFiles1.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgv_deconResults_DragEnter);
            // 
            // dgv_loadFiles2
            // 
            this.dgv_loadFiles2.AllowDrop = true;
            this.dgv_loadFiles2.AllowUserToOrderColumns = true;
            this.dgv_loadFiles2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_loadFiles2.Location = new System.Drawing.Point(460, 49);
            this.dgv_loadFiles2.Name = "dgv_loadFiles2";
            this.dgv_loadFiles2.Size = new System.Drawing.Size(430, 463);
            this.dgv_loadFiles2.TabIndex = 14;
            this.dgv_loadFiles2.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgv_loadFiles2_CellFormatting);
            this.dgv_loadFiles2.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgv_quantResults_DragDrop);
            this.dgv_loadFiles2.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgv_quantResults_DragEnter);
            // 
            // dgv_loadFiles3
            // 
            this.dgv_loadFiles3.AllowDrop = true;
            this.dgv_loadFiles3.AllowUserToOrderColumns = true;
            this.dgv_loadFiles3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_loadFiles3.Location = new System.Drawing.Point(907, 49);
            this.dgv_loadFiles3.Name = "dgv_loadFiles3";
            this.dgv_loadFiles3.Size = new System.Drawing.Size(430, 463);
            this.dgv_loadFiles3.TabIndex = 15;
            this.dgv_loadFiles3.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgv_loadFiles3_CellFormatting);
            this.dgv_loadFiles3.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgv_calibrationResults_DragDrop);
            this.dgv_loadFiles3.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgv_calibrationResults_DragEnter);
            // 
            // btn_fullRun
            // 
            this.btn_fullRun.Location = new System.Drawing.Point(675, 626);
            this.btn_fullRun.Name = "btn_fullRun";
            this.btn_fullRun.Size = new System.Drawing.Size(156, 92);
            this.btn_fullRun.TabIndex = 17;
            this.btn_fullRun.Text = "Full Run With Defaults";
            this.btn_fullRun.UseVisualStyleBackColor = true;
            this.btn_fullRun.Click += new System.EventHandler(this.btn_fullRun_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btn_fullRunWithPresets);
            this.groupBox2.Controls.Add(this.btn_nextPane);
            this.groupBox2.Location = new System.Drawing.Point(999, 604);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(183, 114);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "First Time Users";
            // 
            // btn_fullRunWithPresets
            // 
            this.btn_fullRunWithPresets.Location = new System.Drawing.Point(35, 65);
            this.btn_fullRunWithPresets.Name = "btn_fullRunWithPresets";
            this.btn_fullRunWithPresets.Size = new System.Drawing.Size(122, 43);
            this.btn_fullRunWithPresets.TabIndex = 19;
            this.btn_fullRunWithPresets.Text = "Full Run With Preset Parameters";
            this.btn_fullRunWithPresets.UseVisualStyleBackColor = true;
            this.btn_fullRunWithPresets.Click += new System.EventHandler(this.btn_fullRunWithPresets_Click);
            // 
            // btn_nextPane
            // 
            this.btn_nextPane.Location = new System.Drawing.Point(35, 16);
            this.btn_nextPane.Name = "btn_nextPane";
            this.btn_nextPane.Size = new System.Drawing.Size(122, 36);
            this.btn_nextPane.TabIndex = 17;
            this.btn_nextPane.Text = "Step Through Results Processing";
            this.btn_nextPane.UseVisualStyleBackColor = true;
            this.btn_nextPane.Click += new System.EventHandler(this.btn_nextPane_Click);
            // 
            // bt_clearResults
            // 
            this.bt_clearResults.Location = new System.Drawing.Point(837, 626);
            this.bt_clearResults.Name = "bt_clearResults";
            this.bt_clearResults.Size = new System.Drawing.Size(156, 92);
            this.bt_clearResults.TabIndex = 26;
            this.bt_clearResults.Text = "Clear Results";
            this.bt_clearResults.UseVisualStyleBackColor = true;
            this.bt_clearResults.Click += new System.EventHandler(this.bt_clearResults_Click);
            // 
            // cb_run_when_load
            // 
            this.cb_run_when_load.AutoSize = true;
            this.cb_run_when_load.Location = new System.Drawing.Point(16, 587);
            this.cb_run_when_load.Name = "cb_run_when_load";
            this.cb_run_when_load.Size = new System.Drawing.Size(157, 17);
            this.cb_run_when_load.TabIndex = 27;
            this.cb_run_when_load.Text = "Process when loading page";
            this.cb_run_when_load.UseVisualStyleBackColor = true;
            this.cb_run_when_load.CheckedChanged += new System.EventHandler(this.cb_run_when_load_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lb_filter3);
            this.groupBox3.Controls.Add(this.tb_filter3);
            this.groupBox3.Controls.Add(this.lb_filter2);
            this.groupBox3.Controls.Add(this.lb_filter1);
            this.groupBox3.Controls.Add(this.tb_filter2);
            this.groupBox3.Controls.Add(this.tb_filter1);
            this.groupBox3.Location = new System.Drawing.Point(232, 618);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(442, 100);
            this.groupBox3.TabIndex = 29;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Text Filters";
            // 
            // lb_filter3
            // 
            this.lb_filter3.AutoSize = true;
            this.lb_filter3.Location = new System.Drawing.Point(114, 75);
            this.lb_filter3.Name = "lb_filter3";
            this.lb_filter3.Size = new System.Drawing.Size(32, 13);
            this.lb_filter3.TabIndex = 34;
            this.lb_filter3.Text = "filter3";
            // 
            // tb_filter3
            // 
            this.tb_filter3.Location = new System.Drawing.Point(8, 72);
            this.tb_filter3.Name = "tb_filter3";
            this.tb_filter3.Size = new System.Drawing.Size(100, 20);
            this.tb_filter3.TabIndex = 33;
            this.tb_filter3.TextChanged += new System.EventHandler(this.tb_filter3_TextChanged);
            // 
            // lb_filter2
            // 
            this.lb_filter2.AutoSize = true;
            this.lb_filter2.Location = new System.Drawing.Point(114, 49);
            this.lb_filter2.Name = "lb_filter2";
            this.lb_filter2.Size = new System.Drawing.Size(32, 13);
            this.lb_filter2.TabIndex = 32;
            this.lb_filter2.Text = "filter2";
            // 
            // lb_filter1
            // 
            this.lb_filter1.AutoSize = true;
            this.lb_filter1.Location = new System.Drawing.Point(114, 22);
            this.lb_filter1.Name = "lb_filter1";
            this.lb_filter1.Size = new System.Drawing.Size(32, 13);
            this.lb_filter1.TabIndex = 31;
            this.lb_filter1.Text = "filter1";
            // 
            // tb_filter2
            // 
            this.tb_filter2.Location = new System.Drawing.Point(8, 46);
            this.tb_filter2.Name = "tb_filter2";
            this.tb_filter2.Size = new System.Drawing.Size(100, 20);
            this.tb_filter2.TabIndex = 30;
            this.tb_filter2.TextChanged += new System.EventHandler(this.tb_filter2_TextChanged);
            // 
            // tb_filter1
            // 
            this.tb_filter1.Location = new System.Drawing.Point(8, 19);
            this.tb_filter1.Name = "tb_filter1";
            this.tb_filter1.Size = new System.Drawing.Size(100, 20);
            this.tb_filter1.TabIndex = 29;
            this.tb_filter1.TextChanged += new System.EventHandler(this.tb_filter1_TextChanged);
            // 
            // cmb_loadTable1
            // 
            this.cmb_loadTable1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_loadTable1.FormattingEnabled = true;
            this.cmb_loadTable1.Location = new System.Drawing.Point(13, 15);
            this.cmb_loadTable1.Name = "cmb_loadTable1";
            this.cmb_loadTable1.Size = new System.Drawing.Size(429, 26);
            this.cmb_loadTable1.TabIndex = 31;
            this.cmb_loadTable1.SelectedIndexChanged += new System.EventHandler(this.cmb_loadTable1_SelectedIndexChanged);
            // 
            // cmb_loadTable2
            // 
            this.cmb_loadTable2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_loadTable2.FormattingEnabled = true;
            this.cmb_loadTable2.Location = new System.Drawing.Point(460, 15);
            this.cmb_loadTable2.Name = "cmb_loadTable2";
            this.cmb_loadTable2.Size = new System.Drawing.Size(429, 26);
            this.cmb_loadTable2.TabIndex = 32;
            this.cmb_loadTable2.SelectedIndexChanged += new System.EventHandler(this.cmb_loadTable2_SelectedIndexChanged);
            // 
            // cmb_loadTable3
            // 
            this.cmb_loadTable3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_loadTable3.FormattingEnabled = true;
            this.cmb_loadTable3.Location = new System.Drawing.Point(907, 15);
            this.cmb_loadTable3.Name = "cmb_loadTable3";
            this.cmb_loadTable3.Size = new System.Drawing.Size(429, 26);
            this.cmb_loadTable3.TabIndex = 33;
            this.cmb_loadTable3.SelectedIndexChanged += new System.EventHandler(this.cmb_LoadTable3_SelectedIndexChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rb_chemicalCalibration);
            this.groupBox4.Controls.Add(this.rb_standardOptions);
            this.groupBox4.Controls.Add(this.rb_advanced_user);
            this.groupBox4.Location = new System.Drawing.Point(232, 569);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(442, 43);
            this.groupBox4.TabIndex = 35;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Load Options";
            // 
            // rb_chemicalCalibration
            // 
            this.rb_chemicalCalibration.AutoSize = true;
            this.rb_chemicalCalibration.Location = new System.Drawing.Point(90, 17);
            this.rb_chemicalCalibration.Name = "rb_chemicalCalibration";
            this.rb_chemicalCalibration.Size = new System.Drawing.Size(120, 17);
            this.rb_chemicalCalibration.TabIndex = 37;
            this.rb_chemicalCalibration.Text = "Chemical Calibration";
            this.rb_chemicalCalibration.UseVisualStyleBackColor = true;
            this.rb_chemicalCalibration.CheckedChanged += new System.EventHandler(this.rb_chemicalCalibration_CheckedChanged);
            // 
            // rb_standardOptions
            // 
            this.rb_standardOptions.AutoSize = true;
            this.rb_standardOptions.Checked = true;
            this.rb_standardOptions.Location = new System.Drawing.Point(8, 17);
            this.rb_standardOptions.Name = "rb_standardOptions";
            this.rb_standardOptions.Size = new System.Drawing.Size(68, 17);
            this.rb_standardOptions.TabIndex = 36;
            this.rb_standardOptions.TabStop = true;
            this.rb_standardOptions.Text = "Standard";
            this.rb_standardOptions.UseVisualStyleBackColor = true;
            this.rb_standardOptions.CheckedChanged += new System.EventHandler(this.rb_standardOptions_CheckedChanged);
            // 
            // rb_advanced_user
            // 
            this.rb_advanced_user.AutoSize = true;
            this.rb_advanced_user.Enabled = false;
            this.rb_advanced_user.Location = new System.Drawing.Point(216, 17);
            this.rb_advanced_user.Name = "rb_advanced_user";
            this.rb_advanced_user.Size = new System.Drawing.Size(179, 17);
            this.rb_advanced_user.TabIndex = 35;
            this.rb_advanced_user.Text = "Top Down (Under Development)";
            this.rb_advanced_user.UseVisualStyleBackColor = true;
            this.rb_advanced_user.CheckedChanged += new System.EventHandler(this.rb_advanced_user_CheckedChanged);
            // 
            // LoadDeconvolutionResults
            // 
            this.ClientSize = new System.Drawing.Size(1362, 736);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.cmb_loadTable3);
            this.Controls.Add(this.cmb_loadTable2);
            this.Controls.Add(this.cmb_loadTable1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.cb_run_when_load);
            this.Controls.Add(this.bt_clearResults);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btn_fullRun);
            this.Controls.Add(this.dgv_loadFiles3);
            this.Controls.Add(this.dgv_loadFiles2);
            this.Controls.Add(this.dgv_loadFiles1);
            this.Controls.Add(this.btn_addFiles3);
            this.Controls.Add(this.btn_clearFiles3);
            this.Controls.Add(this.btn_addFiles2);
            this.Controls.Add(this.btn_clearFiles2);
            this.Controls.Add(this.btn_AddFiles1);
            this.Controls.Add(this.btn_clearFiles1);
            this.Controls.Add(this.groupBox1);
            this.Name = "LoadDeconvolutionResults";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_loadFiles1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_loadFiles2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_loadFiles3)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton btn_unlabeled;
        private System.Windows.Forms.RadioButton btn_neucode;
        private System.Windows.Forms.Button btn_clearFiles1;
        private System.Windows.Forms.Button btn_AddFiles1;
        private System.Windows.Forms.Button btn_addFiles2;
        private System.Windows.Forms.Button btn_clearFiles2;
        private System.Windows.Forms.Button btn_addFiles3;
        private System.Windows.Forms.Button btn_clearFiles3;
        private System.Windows.Forms.DataGridView dgv_loadFiles1;
        private System.Windows.Forms.DataGridView dgv_loadFiles2;
        private System.Windows.Forms.DataGridView dgv_loadFiles3;
        private System.Windows.Forms.Button btn_fullRun;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btn_fullRunWithPresets;
        private System.Windows.Forms.Button btn_nextPane;
        private System.Windows.Forms.Button bt_clearResults;
        private System.Windows.Forms.CheckBox cb_run_when_load;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lb_filter2;
        private System.Windows.Forms.Label lb_filter1;
        private System.Windows.Forms.TextBox tb_filter2;
        private System.Windows.Forms.TextBox tb_filter1;
        private System.Windows.Forms.CheckBox cb_td_file;
        private System.Windows.Forms.ComboBox cmb_loadTable1;
        private System.Windows.Forms.ComboBox cmb_loadTable2;
        private System.Windows.Forms.ComboBox cmb_loadTable3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rb_chemicalCalibration;
        private System.Windows.Forms.RadioButton rb_standardOptions;
        private System.Windows.Forms.RadioButton rb_advanced_user;
        private System.Windows.Forms.Label lb_filter3;
        private System.Windows.Forms.TextBox tb_filter3;
    }
}