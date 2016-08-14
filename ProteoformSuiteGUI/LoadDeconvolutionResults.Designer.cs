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
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identificationFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantitationFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_calibrationFiles)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_unlabeled);
            this.groupBox1.Controls.Add(this.btn_neucode);
            this.groupBox1.Location = new System.Drawing.Point(13, 610);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(357, 114);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Proteoform Identification Results";
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
            this.label2.Location = new System.Drawing.Point(500, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(291, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Proteoform Quantification Results (.xlsx)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(989, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(293, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Deconvolution Calibration Files (.txt, .tsv)";
            // 
            // btn_protIdResultsClear
            // 
            this.btn_protIdResultsClear.Location = new System.Drawing.Point(258, 518);
            this.btn_protIdResultsClear.Name = "btn_protIdResultsClear";
            this.btn_protIdResultsClear.Size = new System.Drawing.Size(122, 36);
            this.btn_protIdResultsClear.TabIndex = 7;
            this.btn_protIdResultsClear.Text = "Clear";
            this.btn_protIdResultsClear.UseVisualStyleBackColor = true;
            this.btn_protIdResultsClear.Click += new System.EventHandler(this.btn_protIdResultsClear_Click);
            // 
            // btn_protIdResultsAdd
            // 
            this.btn_protIdResultsAdd.Location = new System.Drawing.Point(78, 518);
            this.btn_protIdResultsAdd.Name = "btn_protIdResultsAdd";
            this.btn_protIdResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.btn_protIdResultsAdd.TabIndex = 8;
            this.btn_protIdResultsAdd.Text = "Add";
            this.btn_protIdResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protIdResultsAdd.Click += new System.EventHandler(this.btn_protIdResultsAdd_Click);
            // 
            // btn_protQuantResultsAdd
            // 
            this.btn_protQuantResultsAdd.Location = new System.Drawing.Point(555, 518);
            this.btn_protQuantResultsAdd.Name = "btn_protQuantResultsAdd";
            this.btn_protQuantResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.btn_protQuantResultsAdd.TabIndex = 10;
            this.btn_protQuantResultsAdd.Text = "Add";
            this.btn_protQuantResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protQuantResultsAdd.Click += new System.EventHandler(this.btn_protQuantResultsAdd_Click);
            // 
            // btn_protQuantResultsClear
            // 
            this.btn_protQuantResultsClear.Location = new System.Drawing.Point(759, 518);
            this.btn_protQuantResultsClear.Name = "btn_protQuantResultsClear";
            this.btn_protQuantResultsClear.Size = new System.Drawing.Size(122, 36);
            this.btn_protQuantResultsClear.TabIndex = 9;
            this.btn_protQuantResultsClear.Text = "Clear";
            this.btn_protQuantResultsClear.UseVisualStyleBackColor = true;
            this.btn_protQuantResultsClear.Click += new System.EventHandler(this.btn_protQuantResultsClear_Click);
            // 
            // btn_protCalibResultsAdd
            // 
            this.btn_protCalibResultsAdd.Location = new System.Drawing.Point(1062, 518);
            this.btn_protCalibResultsAdd.Name = "btn_protCalibResultsAdd";
            this.btn_protCalibResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.btn_protCalibResultsAdd.TabIndex = 12;
            this.btn_protCalibResultsAdd.Text = "Add";
            this.btn_protCalibResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protCalibResultsAdd.Click += new System.EventHandler(this.btn_protCalibResultsAdd_Click);
            // 
            // btn_protCalibResultsClear
            // 
            this.btn_protCalibResultsClear.Location = new System.Drawing.Point(1235, 518);
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
            this.dgv_identificationFiles.Size = new System.Drawing.Size(453, 463);
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
            this.dgv_quantitationFiles.Location = new System.Drawing.Point(504, 49);
            this.dgv_quantitationFiles.Name = "dgv_quantitationFiles";
            this.dgv_quantitationFiles.Size = new System.Drawing.Size(453, 463);
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
            this.dgv_calibrationFiles.Location = new System.Drawing.Point(993, 49);
            this.dgv_calibrationFiles.Name = "dgv_calibrationFiles";
            this.dgv_calibrationFiles.Size = new System.Drawing.Size(453, 463);
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
            // LoadDeconvolutionResults
            // 
            this.ClientSize = new System.Drawing.Size(1471, 736);
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
            this.Name = "LoadDeconvolutionResults";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identificationFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantitationFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_calibrationFiles)).EndInit();
            this.groupBox2.ResumeLayout(false);
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
    }
}