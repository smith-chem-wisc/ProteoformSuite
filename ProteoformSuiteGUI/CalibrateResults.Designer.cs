namespace ProteoformSuite
{
    partial class CalibrateResults
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
            this.bt_tdResultsAdd = new System.Windows.Forms.Button();
            this.bt_tdResultsClear = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.dgv_tdFiles = new System.Windows.Forms.DataGridView();
            this.dgv_rawFiles = new System.Windows.Forms.DataGridView();
            this.dgv_identificationFiles = new System.Windows.Forms.DataGridView();
            this.btn_protIdResultsAdd = new System.Windows.Forms.Button();
            this.btn_protIdResultsClear = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_calibrate_lock_mass_peptide = new System.Windows.Forms.CheckBox();
            this.cb_calibrate_with_td_results = new System.Windows.Forms.CheckBox();
            this.bt_calibrate = new System.Windows.Forms.Button();
            this.bt_rawFilesAdd = new System.Windows.Forms.Button();
            this.bt_rawFilesClear = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_tdFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_rawFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identificationFiles)).BeginInit();
            this.SuspendLayout();
            // 
            // bt_tdResultsAdd
            // 
            this.bt_tdResultsAdd.Location = new System.Drawing.Point(625, 501);
            this.bt_tdResultsAdd.Name = "bt_tdResultsAdd";
            this.bt_tdResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.bt_tdResultsAdd.TabIndex = 37;
            this.bt_tdResultsAdd.Text = "Add";
            this.bt_tdResultsAdd.UseVisualStyleBackColor = true;
            this.bt_tdResultsAdd.Click += new System.EventHandler(this.bt_tdResultsAdd_Click);
            // 
            // bt_tdResultsClear
            // 
            this.bt_tdResultsClear.Location = new System.Drawing.Point(753, 501);
            this.bt_tdResultsClear.Name = "bt_tdResultsClear";
            this.bt_tdResultsClear.Size = new System.Drawing.Size(122, 36);
            this.bt_tdResultsClear.TabIndex = 36;
            this.bt_tdResultsClear.Text = "Clear";
            this.bt_tdResultsClear.UseVisualStyleBackColor = true;
            this.bt_tdResultsClear.Click += new System.EventHandler(this.bt_tdResultsClear_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(665, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(183, 20);
            this.label4.TabIndex = 35;
            this.label4.Text = "Top-Down Results (.xlsx)";
            // 
            // dgv_tdFiles
            // 
            this.dgv_tdFiles.AllowDrop = true;
            this.dgv_tdFiles.AllowUserToOrderColumns = true;
            this.dgv_tdFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_tdFiles.Location = new System.Drawing.Point(625, 32);
            this.dgv_tdFiles.Name = "dgv_tdFiles";
            this.dgv_tdFiles.Size = new System.Drawing.Size(275, 463);
            this.dgv_tdFiles.TabIndex = 34;
            // 
            // dgv_rawFiles
            // 
            this.dgv_rawFiles.AllowDrop = true;
            this.dgv_rawFiles.AllowUserToOrderColumns = true;
            this.dgv_rawFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_rawFiles.Location = new System.Drawing.Point(325, 32);
            this.dgv_rawFiles.Name = "dgv_rawFiles";
            this.dgv_rawFiles.Size = new System.Drawing.Size(287, 463);
            this.dgv_rawFiles.TabIndex = 33;
            // 
            // dgv_identificationFiles
            // 
            this.dgv_identificationFiles.AllowDrop = true;
            this.dgv_identificationFiles.AllowUserToOrderColumns = true;
            this.dgv_identificationFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_identificationFiles.Location = new System.Drawing.Point(12, 32);
            this.dgv_identificationFiles.Name = "dgv_identificationFiles";
            this.dgv_identificationFiles.RowTemplate.Height = 28;
            this.dgv_identificationFiles.Size = new System.Drawing.Size(296, 463);
            this.dgv_identificationFiles.TabIndex = 32;
            // 
            // btn_protIdResultsAdd
            // 
            this.btn_protIdResultsAdd.Location = new System.Drawing.Point(35, 501);
            this.btn_protIdResultsAdd.Name = "btn_protIdResultsAdd";
            this.btn_protIdResultsAdd.Size = new System.Drawing.Size(122, 36);
            this.btn_protIdResultsAdd.TabIndex = 29;
            this.btn_protIdResultsAdd.Text = "Add";
            this.btn_protIdResultsAdd.UseVisualStyleBackColor = true;
            this.btn_protIdResultsAdd.Click += new System.EventHandler(this.btn_protIdResultsAdd_Click);
            // 
            // btn_protIdResultsClear
            // 
            this.btn_protIdResultsClear.Location = new System.Drawing.Point(163, 501);
            this.btn_protIdResultsClear.Name = "btn_protIdResultsClear";
            this.btn_protIdResultsClear.Size = new System.Drawing.Size(122, 36);
            this.btn_protIdResultsClear.TabIndex = 28;
            this.btn_protIdResultsClear.Text = "Clear";
            this.btn_protIdResultsClear.UseVisualStyleBackColor = true;
            this.btn_protIdResultsClear.Click += new System.EventHandler(this.btn_protIdResultsClear_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(403, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 20);
            this.label3.TabIndex = 27;
            this.label3.Text = "Raw Files (.raw)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(284, 20);
            this.label1.TabIndex = 26;
            this.label1.Text = "Proteoform Identification Results (.xlsx)";
            // 
            // cb_calibrate_lock_mass_peptide
            // 
            this.cb_calibrate_lock_mass_peptide.AutoSize = true;
            this.cb_calibrate_lock_mass_peptide.Location = new System.Drawing.Point(12, 564);
            this.cb_calibrate_lock_mass_peptide.Name = "cb_calibrate_lock_mass_peptide";
            this.cb_calibrate_lock_mass_peptide.Size = new System.Drawing.Size(183, 17);
            this.cb_calibrate_lock_mass_peptide.TabIndex = 41;
            this.cb_calibrate_lock_mass_peptide.Text = "Calibrate with Lock Mass Peptide";
            this.cb_calibrate_lock_mass_peptide.UseVisualStyleBackColor = true;
            this.cb_calibrate_lock_mass_peptide.CheckedChanged += new System.EventHandler(this.cb_calibrate_lock_mass_peptide_CheckedChanged);
            // 
            // cb_calibrate_with_td_results
            // 
            this.cb_calibrate_with_td_results.AutoSize = true;
            this.cb_calibrate_with_td_results.Checked = true;
            this.cb_calibrate_with_td_results.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_calibrate_with_td_results.Location = new System.Drawing.Point(12, 587);
            this.cb_calibrate_with_td_results.Name = "cb_calibrate_with_td_results";
            this.cb_calibrate_with_td_results.Size = new System.Drawing.Size(178, 17);
            this.cb_calibrate_with_td_results.TabIndex = 42;
            this.cb_calibrate_with_td_results.Text = "Calibrate with Top-down Results";
            this.cb_calibrate_with_td_results.UseVisualStyleBackColor = true;
            this.cb_calibrate_with_td_results.CheckedChanged += new System.EventHandler(this.cb_calibrate_with_td_results_CheckedChanged);
            // 
            // bt_calibrate
            // 
            this.bt_calibrate.Location = new System.Drawing.Point(12, 611);
            this.bt_calibrate.Name = "bt_calibrate";
            this.bt_calibrate.Size = new System.Drawing.Size(208, 23);
            this.bt_calibrate.TabIndex = 43;
            this.bt_calibrate.Text = "Calibrate";
            this.bt_calibrate.UseVisualStyleBackColor = true;
            this.bt_calibrate.Click += new System.EventHandler(this.bt_calibrate_Click);
            // 
            // bt_rawFilesAdd
            // 
            this.bt_rawFilesAdd.Location = new System.Drawing.Point(345, 501);
            this.bt_rawFilesAdd.Name = "bt_rawFilesAdd";
            this.bt_rawFilesAdd.Size = new System.Drawing.Size(122, 36);
            this.bt_rawFilesAdd.TabIndex = 47;
            this.bt_rawFilesAdd.Text = "Add";
            this.bt_rawFilesAdd.UseVisualStyleBackColor = true;
            this.bt_rawFilesAdd.Click += new System.EventHandler(this.bt_rawFilesAdd_Click);
            // 
            // bt_rawFilesClear
            // 
            this.bt_rawFilesClear.Location = new System.Drawing.Point(473, 501);
            this.bt_rawFilesClear.Name = "bt_rawFilesClear";
            this.bt_rawFilesClear.Size = new System.Drawing.Size(122, 36);
            this.bt_rawFilesClear.TabIndex = 46;
            this.bt_rawFilesClear.Text = "Clear";
            this.bt_rawFilesClear.UseVisualStyleBackColor = true;
            this.bt_rawFilesClear.Click += new System.EventHandler(this.bt_rawFilesClear_Click);
            // 
            // CalibrateResults
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1110, 653);
            this.Controls.Add(this.bt_rawFilesAdd);
            this.Controls.Add(this.bt_rawFilesClear);
            this.Controls.Add(this.bt_calibrate);
            this.Controls.Add(this.cb_calibrate_with_td_results);
            this.Controls.Add(this.cb_calibrate_lock_mass_peptide);
            this.Controls.Add(this.bt_tdResultsAdd);
            this.Controls.Add(this.bt_tdResultsClear);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.dgv_tdFiles);
            this.Controls.Add(this.dgv_rawFiles);
            this.Controls.Add(this.dgv_identificationFiles);
            this.Controls.Add(this.btn_protIdResultsAdd);
            this.Controls.Add(this.btn_protIdResultsClear);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Name = "CalibrateResults";
            this.Text = "CalibrateResults";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_tdFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_rawFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identificationFiles)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bt_tdResultsAdd;
        private System.Windows.Forms.Button bt_tdResultsClear;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dgv_tdFiles;
        private System.Windows.Forms.DataGridView dgv_rawFiles;
        private System.Windows.Forms.DataGridView dgv_identificationFiles;
        private System.Windows.Forms.Button btn_protIdResultsAdd;
        private System.Windows.Forms.Button btn_protIdResultsClear;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cb_calibrate_lock_mass_peptide;
        private System.Windows.Forms.CheckBox cb_calibrate_with_td_results;
        private System.Windows.Forms.Button bt_calibrate;
        private System.Windows.Forms.Button bt_rawFilesAdd;
        private System.Windows.Forms.Button bt_rawFilesClear;
    }
}