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
            this.clb_deconResults = new System.Windows.Forms.CheckedListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_unlabelled = new System.Windows.Forms.RadioButton();
            this.btn_neucode = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.clb_quantResults = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.clb_calibResults = new System.Windows.Forms.CheckedListBox();
            this.btn_protIdResultsClear = new System.Windows.Forms.Button();
            this.btn_protIdResultsAdd = new System.Windows.Forms.Button();
            this.btn_protQuantResultsAdd = new System.Windows.Forms.Button();
            this.btn_protQuantResultsClear = new System.Windows.Forms.Button();
            this.btn_protCalibResultsAdd = new System.Windows.Forms.Button();
            this.btn_protCalibResultsClear = new System.Windows.Forms.Button();
            this.dgv_deconResults = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_deconResults)).BeginInit();
            this.SuspendLayout();
            // 
            // clb_deconResults
            // 
            this.clb_deconResults.AllowDrop = true;
            this.clb_deconResults.FormattingEnabled = true;
            this.clb_deconResults.Location = new System.Drawing.Point(12, 46);
            this.clb_deconResults.Name = "clb_deconResults";
            this.clb_deconResults.Size = new System.Drawing.Size(453, 88);
            this.clb_deconResults.TabIndex = 0;
            this.clb_deconResults.DragDrop += new System.Windows.Forms.DragEventHandler(this.clb_deconResults_DragDrop);
            this.clb_deconResults.DragEnter += new System.Windows.Forms.DragEventHandler(this.clb_deconResults_DragEnter);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_unlabelled);
            this.groupBox1.Controls.Add(this.btn_neucode);
            this.groupBox1.Location = new System.Drawing.Point(13, 624);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(357, 100);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // btn_unlabelled
            // 
            this.btn_unlabelled.AutoSize = true;
            this.btn_unlabelled.Location = new System.Drawing.Point(23, 57);
            this.btn_unlabelled.Name = "btn_unlabelled";
            this.btn_unlabelled.Size = new System.Drawing.Size(109, 24);
            this.btn_unlabelled.TabIndex = 1;
            this.btn_unlabelled.TabStop = true;
            this.btn_unlabelled.Text = "Unlabelled";
            this.btn_unlabelled.UseVisualStyleBackColor = true;
            // 
            // btn_neucode
            // 
            this.btn_neucode.AutoSize = true;
            this.btn_neucode.Location = new System.Drawing.Point(23, 26);
            this.btn_neucode.Name = "btn_neucode";
            this.btn_neucode.Size = new System.Drawing.Size(165, 24);
            this.btn_neucode.TabIndex = 0;
            this.btn_neucode.TabStop = true;
            this.btn_neucode.Text = "NeuCode Labelled";
            this.btn_neucode.UseVisualStyleBackColor = true;
            this.btn_neucode.CheckedChanged += new System.EventHandler(this.btn_neucode_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(284, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Proteoform Identification Results (.xlsx)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(500, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(291, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Proteoform Quantification Results (.xlsx)";
            // 
            // clb_quantResults
            // 
            this.clb_quantResults.AllowDrop = true;
            this.clb_quantResults.FormattingEnabled = true;
            this.clb_quantResults.Location = new System.Drawing.Point(504, 46);
            this.clb_quantResults.Name = "clb_quantResults";
            this.clb_quantResults.Size = new System.Drawing.Size(453, 466);
            this.clb_quantResults.TabIndex = 3;
            this.clb_quantResults.DragDrop += new System.Windows.Forms.DragEventHandler(this.clb_quantResults_DragDrop);
            this.clb_quantResults.DragEnter += new System.Windows.Forms.DragEventHandler(this.clb_quantResults_DragEnter);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(989, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(289, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Deconvolution Calibration Files (.txt .tsv)";
            // 
            // clb_calibResults
            // 
            this.clb_calibResults.AllowDrop = true;
            this.clb_calibResults.FormattingEnabled = true;
            this.clb_calibResults.Location = new System.Drawing.Point(993, 46);
            this.clb_calibResults.Name = "clb_calibResults";
            this.clb_calibResults.Size = new System.Drawing.Size(453, 466);
            this.clb_calibResults.TabIndex = 5;
            this.clb_calibResults.DragDrop += new System.Windows.Forms.DragEventHandler(this.clb_calibResults_DragDrop);
            this.clb_calibResults.DragEnter += new System.Windows.Forms.DragEventHandler(this.clb_calibResults_DragEnter);
            // 
            // btn_protIdResultsClear
            // 
            this.btn_protIdResultsClear.Location = new System.Drawing.Point(243, 518);
            this.btn_protIdResultsClear.Name = "btn_protIdResultsClear";
            this.btn_protIdResultsClear.Size = new System.Drawing.Size(150, 88);
            this.btn_protIdResultsClear.TabIndex = 7;
            this.btn_protIdResultsClear.Text = "Clear Proteoform Identification Results";
            this.btn_protIdResultsClear.UseVisualStyleBackColor = true;
            this.btn_protIdResultsClear.Click += new System.EventHandler(this.btn_protIdResultsClear_Click);
            // 
            // btn_protIdResultsAdd
            // 
            this.btn_protIdResultsAdd.Location = new System.Drawing.Point(74, 518);
            this.btn_protIdResultsAdd.Name = "btn_protIdResultsAdd";
            this.btn_protIdResultsAdd.Size = new System.Drawing.Size(150, 88);
            this.btn_protIdResultsAdd.TabIndex = 8;
            this.btn_protIdResultsAdd.Text = "Add Proteoform Identification Results";
            this.btn_protIdResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protIdResultsAdd.Click += new System.EventHandler(this.btn_protIdResultsAdd_Click);
            // 
            // btn_protQuantResultsAdd
            // 
            this.btn_protQuantResultsAdd.Location = new System.Drawing.Point(585, 518);
            this.btn_protQuantResultsAdd.Name = "btn_protQuantResultsAdd";
            this.btn_protQuantResultsAdd.Size = new System.Drawing.Size(150, 88);
            this.btn_protQuantResultsAdd.TabIndex = 10;
            this.btn_protQuantResultsAdd.Text = "Add Proteoform Quantification Results";
            this.btn_protQuantResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protQuantResultsAdd.Click += new System.EventHandler(this.btn_protQuantResultsAdd_Click);
            // 
            // btn_protQuantResultsClear
            // 
            this.btn_protQuantResultsClear.Location = new System.Drawing.Point(754, 518);
            this.btn_protQuantResultsClear.Name = "btn_protQuantResultsClear";
            this.btn_protQuantResultsClear.Size = new System.Drawing.Size(150, 88);
            this.btn_protQuantResultsClear.TabIndex = 9;
            this.btn_protQuantResultsClear.Text = "Clear Proteoform Quantification Results";
            this.btn_protQuantResultsClear.UseVisualStyleBackColor = true;
            this.btn_protQuantResultsClear.Click += new System.EventHandler(this.btn_protQuantResultsClear_Click);
            // 
            // btn_protCalibResultsAdd
            // 
            this.btn_protCalibResultsAdd.Location = new System.Drawing.Point(1068, 518);
            this.btn_protCalibResultsAdd.Name = "btn_protCalibResultsAdd";
            this.btn_protCalibResultsAdd.Size = new System.Drawing.Size(150, 88);
            this.btn_protCalibResultsAdd.TabIndex = 12;
            this.btn_protCalibResultsAdd.Text = "Add Proteoform Calibration Results";
            this.btn_protCalibResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protCalibResultsAdd.Click += new System.EventHandler(this.btn_protCalibResultsAdd_Click);
            // 
            // btn_protCalibResultsClear
            // 
            this.btn_protCalibResultsClear.Location = new System.Drawing.Point(1237, 518);
            this.btn_protCalibResultsClear.Name = "btn_protCalibResultsClear";
            this.btn_protCalibResultsClear.Size = new System.Drawing.Size(150, 88);
            this.btn_protCalibResultsClear.TabIndex = 11;
            this.btn_protCalibResultsClear.Text = "Clear Proteoform Calibration Results";
            this.btn_protCalibResultsClear.UseVisualStyleBackColor = true;
            this.btn_protCalibResultsClear.Click += new System.EventHandler(this.btn_protCalibResultsClear_Click);
            // 
            // dgv_deconResults
            // 
            this.dgv_deconResults.AllowDrop = true;
            this.dgv_deconResults.AllowUserToOrderColumns = true;
            this.dgv_deconResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_deconResults.Location = new System.Drawing.Point(12, 151);
            this.dgv_deconResults.Name = "dgv_deconResults";
            this.dgv_deconResults.RowTemplate.Height = 28;
            this.dgv_deconResults.Size = new System.Drawing.Size(453, 361);
            this.dgv_deconResults.TabIndex = 13;
            this.dgv_deconResults.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgv_deconResults_CellFormatting);
            this.dgv_deconResults.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgv_deconResults_DragDrop);
            this.dgv_deconResults.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgv_deconResults_DragEnter);
            // 
            // LoadDeconvolutionResults
            // 
            this.ClientSize = new System.Drawing.Size(1471, 736);
            this.Controls.Add(this.dgv_deconResults);
            this.Controls.Add(this.btn_protCalibResultsAdd);
            this.Controls.Add(this.btn_protCalibResultsClear);
            this.Controls.Add(this.btn_protQuantResultsAdd);
            this.Controls.Add(this.btn_protQuantResultsClear);
            this.Controls.Add(this.btn_protIdResultsAdd);
            this.Controls.Add(this.btn_protIdResultsClear);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.clb_calibResults);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.clb_quantResults);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.clb_deconResults);
            this.Name = "LoadDeconvolutionResults";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_deconResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button btnDeconResultsClear;
        private System.Windows.Forms.Button btn_AddCorrectionFactors;
        private System.Windows.Forms.Button btnDeconResultsRemove;
        private System.Windows.Forms.Button btnDeconResultsAdd;
        private System.Windows.Forms.CheckBox cb_neuCodeLabeled;
        private System.Windows.Forms.CheckedListBox clb_deconResults;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton btn_unlabelled;
        private System.Windows.Forms.RadioButton btn_neucode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox clb_quantResults;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox clb_calibResults;
        private System.Windows.Forms.Button btn_protIdResultsClear;
        private System.Windows.Forms.Button btn_protIdResultsAdd;
        private System.Windows.Forms.Button btn_protQuantResultsAdd;
        private System.Windows.Forms.Button btn_protQuantResultsClear;
        private System.Windows.Forms.Button btn_protCalibResultsAdd;
        private System.Windows.Forms.Button btn_protCalibResultsClear;
        private System.Windows.Forms.DataGridView dgv_deconResults;
    }
}