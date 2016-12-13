namespace ProteoformSuite
{
    partial class LoadResults
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_protIdResultsClear = new System.Windows.Forms.Button();
            this.btn_protIdResultsAdd = new System.Windows.Forms.Button();
            this.btn_protQuantResultsAdd = new System.Windows.Forms.Button();
            this.btn_protQuantResultsClear = new System.Windows.Forms.Button();
            this.btn_protCalibResultsAdd = new System.Windows.Forms.Button();
            this.btn_protCalibResultsClear = new System.Windows.Forms.Button();
            this.dgv_identificationFiles = new System.Windows.Forms.DataGridView();
            this.dgv_quantitationFiles = new System.Windows.Forms.DataGridView();
            this.dgv_calibrationFiles = new System.Windows.Forms.DataGridView();
            this.btn_fullRun = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_fullRunWithPresets = new System.Windows.Forms.Button();
            this.btn_nextPane = new System.Windows.Forms.Button();
            this.dgv_buFiles = new System.Windows.Forms.DataGridView();
            this.dgv_tdFiles = new System.Windows.Forms.DataGridView();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.bt_morpheusBUResultsAdd = new System.Windows.Forms.Button();
            this.bt_morpheusBUResultsClear = new System.Windows.Forms.Button();
            this.bt_tdResultsAdd = new System.Windows.Forms.Button();
            this.bt_tdResultsClear = new System.Windows.Forms.Button();
            this.bt_clearResults = new System.Windows.Forms.Button();
            this.cb_run_when_load = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identificationFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantitationFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_calibrationFiles)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_buFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_tdFiles)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cb_td_file);
            this.groupBox1.Controls.Add(this.btn_unlabeled);
            this.groupBox1.Controls.Add(this.btn_neucode);
            this.groupBox1.Location = new System.Drawing.Point(13, 610);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(357, 114);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Proteoform Identification Results";
            // 
            // cb_td_file
            // 
            this.cb_td_file.AutoSize = true;
            this.cb_td_file.Location = new System.Drawing.Point(6, 91);
            this.cb_td_file.Name = "cb_td_file";
            this.cb_td_file.Size = new System.Drawing.Size(165, 17);
            this.cb_td_file.TabIndex = 2;
            this.cb_td_file.Text = "Top-down Deconvolution File";
            this.cb_td_file.UseVisualStyleBackColor = true;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(284, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Proteoform Identification Results (.xlsx)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(313, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(291, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Proteoform Quantification Results (.xlsx)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(654, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(159, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Calibration Files (.tsv)";
            // 
            // btn_protIdResultsClear
            // 
            this.btn_protIdResultsClear.Location = new System.Drawing.Point(164, 518);
            this.btn_protIdResultsClear.Name = "btn_protIdResultsClear";
            this.btn_protIdResultsClear.Size = new System.Drawing.Size(122, 36);
            this.btn_protIdResultsClear.TabIndex = 7;
            this.btn_protIdResultsClear.Text = "Clear";
            this.btn_protIdResultsClear.UseVisualStyleBackColor = true;
            this.btn_protIdResultsClear.Click += new System.EventHandler(this.btn_protIdResultsClear_Click);
            // 
            // btn_protIdResultsAdd
            // 
            this.btn_protIdResultsAdd.Location = new System.Drawing.Point(36, 518);
            this.btn_protIdResultsAdd.Name = "btn_protIdResultsAdd";
            this.btn_protIdResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.btn_protIdResultsAdd.TabIndex = 8;
            this.btn_protIdResultsAdd.Text = "Add";
            this.btn_protIdResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protIdResultsAdd.Click += new System.EventHandler(this.btn_protIdResultsAdd_Click);
            // 
            // btn_protQuantResultsAdd
            // 
            this.btn_protQuantResultsAdd.Location = new System.Drawing.Point(338, 518);
            this.btn_protQuantResultsAdd.Name = "btn_protQuantResultsAdd";
            this.btn_protQuantResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.btn_protQuantResultsAdd.TabIndex = 10;
            this.btn_protQuantResultsAdd.Text = "Add";
            this.btn_protQuantResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protQuantResultsAdd.Click += new System.EventHandler(this.btn_protQuantResultsAdd_Click);
            // 
            // btn_protQuantResultsClear
            // 
            this.btn_protQuantResultsClear.Location = new System.Drawing.Point(461, 518);
            this.btn_protQuantResultsClear.Name = "btn_protQuantResultsClear";
            this.btn_protQuantResultsClear.Size = new System.Drawing.Size(122, 36);
            this.btn_protQuantResultsClear.TabIndex = 9;
            this.btn_protQuantResultsClear.Text = "Clear";
            this.btn_protQuantResultsClear.UseVisualStyleBackColor = true;
            this.btn_protQuantResultsClear.Click += new System.EventHandler(this.btn_protQuantResultsClear_Click);
            // 
            // btn_protCalibResultsAdd
            // 
            this.btn_protCalibResultsAdd.Location = new System.Drawing.Point(623, 518);
            this.btn_protCalibResultsAdd.Name = "btn_protCalibResultsAdd";
            this.btn_protCalibResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.btn_protCalibResultsAdd.TabIndex = 12;
            this.btn_protCalibResultsAdd.Text = "Add";
            this.btn_protCalibResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protCalibResultsAdd.Click += new System.EventHandler(this.btn_protCalibResultsAdd_Click);
            // 
            // btn_protCalibResultsClear
            // 
            this.btn_protCalibResultsClear.Location = new System.Drawing.Point(751, 518);
            this.btn_protCalibResultsClear.Name = "btn_protCalibResultsClear";
            this.btn_protCalibResultsClear.Size = new System.Drawing.Size(122, 36);
            this.btn_protCalibResultsClear.TabIndex = 11;
            this.btn_protCalibResultsClear.Text = "Clear";
            this.btn_protCalibResultsClear.UseVisualStyleBackColor = true;
            this.btn_protCalibResultsClear.Click += new System.EventHandler(this.btn_protCalibResultsClear_Click);
            // 
            // dgv_identificationFiles
            // 
            this.dgv_identificationFiles.AllowDrop = true;
            this.dgv_identificationFiles.AllowUserToOrderColumns = true;
            this.dgv_identificationFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_identificationFiles.Location = new System.Drawing.Point(12, 49);
            this.dgv_identificationFiles.Name = "dgv_identificationFiles";
            this.dgv_identificationFiles.RowTemplate.Height = 28;
            this.dgv_identificationFiles.Size = new System.Drawing.Size(296, 463);
            this.dgv_identificationFiles.TabIndex = 13;
            this.dgv_identificationFiles.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgv_deconResults_CellFormatting);
            this.dgv_identificationFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgv_deconResults_DragDrop);
            this.dgv_identificationFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgv_deconResults_DragEnter);
            // 
            // dgv_quantitationFiles
            // 
            this.dgv_quantitationFiles.AllowDrop = true;
            this.dgv_quantitationFiles.AllowUserToOrderColumns = true;
            this.dgv_quantitationFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_quantitationFiles.Location = new System.Drawing.Point(314, 49);
            this.dgv_quantitationFiles.Name = "dgv_quantitationFiles";
            this.dgv_quantitationFiles.Size = new System.Drawing.Size(290, 463);
            this.dgv_quantitationFiles.TabIndex = 14;
            this.dgv_quantitationFiles.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgv_quantResults_CellFormatting);
            this.dgv_quantitationFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgv_quantResults_DragDrop);
            this.dgv_quantitationFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgv_quantResults_DragEnter);
            // 
            // dgv_calibrationFiles
            // 
            this.dgv_calibrationFiles.AllowDrop = true;
            this.dgv_calibrationFiles.AllowUserToOrderColumns = true;
            this.dgv_calibrationFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_calibrationFiles.Location = new System.Drawing.Point(610, 49);
            this.dgv_calibrationFiles.Name = "dgv_calibrationFiles";
            this.dgv_calibrationFiles.Size = new System.Drawing.Size(287, 463);
            this.dgv_calibrationFiles.TabIndex = 15;
            this.dgv_calibrationFiles.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgv_calibrationResults_CellFormatting);
            this.dgv_calibrationFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgv_calibrationResults_DragDrop);
            this.dgv_calibrationFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgv_calibrationResults_DragEnter);
            // 
            // btn_fullRun
            // 
            this.btn_fullRun.Location = new System.Drawing.Point(589, 619);
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
            this.groupBox2.Location = new System.Drawing.Point(396, 610);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(187, 114);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "First Time Users";
            // 
            // btn_fullRunWithPresets
            // 
            this.btn_fullRunWithPresets.Location = new System.Drawing.Point(35, 58);
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
            // dgv_buFiles
            // 
            this.dgv_buFiles.AllowDrop = true;
            this.dgv_buFiles.AllowUserToOrderColumns = true;
            this.dgv_buFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_buFiles.Location = new System.Drawing.Point(1183, 49);
            this.dgv_buFiles.Name = "dgv_buFiles";
            this.dgv_buFiles.Size = new System.Drawing.Size(287, 463);
            this.dgv_buFiles.TabIndex = 18;
            // 
            // dgv_tdFiles
            // 
            this.dgv_tdFiles.AllowDrop = true;
            this.dgv_tdFiles.AllowUserToOrderColumns = true;
            this.dgv_tdFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_tdFiles.Location = new System.Drawing.Point(903, 49);
            this.dgv_tdFiles.Name = "dgv_tdFiles";
            this.dgv_tdFiles.Size = new System.Drawing.Size(275, 463);
            this.dgv_tdFiles.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(949, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(183, 20);
            this.label4.TabIndex = 20;
            this.label4.Text = "Top-Down Results (.xlsx)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(1216, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(183, 20);
            this.label5.TabIndex = 21;
            this.label5.Text = "Bottom-Up Results (.tsv)";
            // 
            // bt_morpheusBUResultsAdd
            // 
            this.bt_morpheusBUResultsAdd.Location = new System.Drawing.Point(1216, 518);
            this.bt_morpheusBUResultsAdd.Name = "bt_morpheusBUResultsAdd";
            this.bt_morpheusBUResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.bt_morpheusBUResultsAdd.TabIndex = 23;
            this.bt_morpheusBUResultsAdd.Text = "Add";
            this.bt_morpheusBUResultsAdd.UseVisualStyleBackColor = true;
            this.bt_morpheusBUResultsAdd.Click += new System.EventHandler(this.bt_morpheusBUResultsAdd_Click);
            // 
            // bt_morpheusBUResultsClear
            // 
            this.bt_morpheusBUResultsClear.Location = new System.Drawing.Point(1343, 518);
            this.bt_morpheusBUResultsClear.Name = "bt_morpheusBUResultsClear";
            this.bt_morpheusBUResultsClear.Size = new System.Drawing.Size(122, 36);
            this.bt_morpheusBUResultsClear.TabIndex = 22;
            this.bt_morpheusBUResultsClear.Text = "Clear";
            this.bt_morpheusBUResultsClear.UseVisualStyleBackColor = true;
            this.bt_morpheusBUResultsClear.Click += new System.EventHandler(this.bt_morpheusBUResultsClear_Click);
            // 
            // bt_tdResultsAdd
            // 
            this.bt_tdResultsAdd.Location = new System.Drawing.Point(919, 518);
            this.bt_tdResultsAdd.Name = "bt_tdResultsAdd";
            this.bt_tdResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.bt_tdResultsAdd.TabIndex = 25;
            this.bt_tdResultsAdd.Text = "Add";
            this.bt_tdResultsAdd.UseVisualStyleBackColor = true;
            this.bt_tdResultsAdd.Click += new System.EventHandler(this.bt_tdResultsAdd_Click);
            // 
            // bt_tdResultsClear
            // 
            this.bt_tdResultsClear.Location = new System.Drawing.Point(1047, 518);
            this.bt_tdResultsClear.Name = "bt_tdResultsClear";
            this.bt_tdResultsClear.Size = new System.Drawing.Size(122, 36);
            this.bt_tdResultsClear.TabIndex = 24;
            this.bt_tdResultsClear.Text = "Clear";
            this.bt_tdResultsClear.UseVisualStyleBackColor = true;
            this.bt_tdResultsClear.Click += new System.EventHandler(this.bt_tdResultsClear_Click);
            // 
            // bt_clearResults
            // 
            this.bt_clearResults.Location = new System.Drawing.Point(751, 619);
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
            this.cb_run_when_load.Checked = true;
            this.cb_run_when_load.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_run_when_load.Location = new System.Drawing.Point(16, 587);
            this.cb_run_when_load.Name = "cb_run_when_load";
            this.cb_run_when_load.Size = new System.Drawing.Size(157, 17);
            this.cb_run_when_load.TabIndex = 27;
            this.cb_run_when_load.Text = "Process when loading page";
            this.cb_run_when_load.UseVisualStyleBackColor = true;
            this.cb_run_when_load.CheckedChanged += new System.EventHandler(this.cb_run_when_load_CheckedChanged);
            // 
            // LoadResults
            // 
            this.ClientSize = new System.Drawing.Size(1362, 736);
            this.Controls.Add(this.cb_run_when_load);
            this.Controls.Add(this.bt_clearResults);
            this.Controls.Add(this.bt_tdResultsAdd);
            this.Controls.Add(this.bt_tdResultsClear);
            this.Controls.Add(this.bt_morpheusBUResultsAdd);
            this.Controls.Add(this.bt_morpheusBUResultsClear);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dgv_tdFiles);
            this.Controls.Add(this.dgv_buFiles);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btn_fullRun);
            this.Controls.Add(this.dgv_calibrationFiles);
            this.Controls.Add(this.dgv_quantitationFiles);
            this.Controls.Add(this.dgv_identificationFiles);
            this.Controls.Add(this.btn_protCalibResultsAdd);
            this.Controls.Add(this.btn_protCalibResultsClear);
            this.Controls.Add(this.btn_protQuantResultsAdd);
            this.Controls.Add(this.btn_protQuantResultsClear);
            this.Controls.Add(this.btn_protIdResultsAdd);
            this.Controls.Add(this.btn_protIdResultsClear);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Name = "LoadResults";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identificationFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantitationFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_calibrationFiles)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_buFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_tdFiles)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton btn_unlabeled;
        private System.Windows.Forms.RadioButton btn_neucode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_protIdResultsClear;
        private System.Windows.Forms.Button btn_protIdResultsAdd;
        private System.Windows.Forms.Button btn_protQuantResultsAdd;
        private System.Windows.Forms.Button btn_protQuantResultsClear;
        private System.Windows.Forms.Button btn_protCalibResultsAdd;
        private System.Windows.Forms.Button btn_protCalibResultsClear;
        private System.Windows.Forms.DataGridView dgv_identificationFiles;
        private System.Windows.Forms.DataGridView dgv_quantitationFiles;
        private System.Windows.Forms.DataGridView dgv_calibrationFiles;
        private System.Windows.Forms.Button btn_fullRun;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btn_fullRunWithPresets;
        private System.Windows.Forms.Button btn_nextPane;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button bt_morpheusBUResultsAdd;
        private System.Windows.Forms.Button bt_morpheusBUResultsClear;
        private System.Windows.Forms.Button bt_tdResultsAdd;
        private System.Windows.Forms.Button bt_tdResultsClear;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dgv_tdFiles;
        private System.Windows.Forms.DataGridView dgv_buFiles;
        private System.Windows.Forms.CheckBox cb_td_file;
        private System.Windows.Forms.Button bt_clearResults;
        private System.Windows.Forms.CheckBox cb_run_when_load;
    }
}