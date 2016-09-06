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
            this.dgv_quantification_results = new System.Windows.Forms.DataGridView();
            this.ckbx_columnNormalize = new System.Windows.Forms.CheckBox();
            this.ckbx_compressTechReps = new System.Windows.Forms.CheckBox();
            this.ckbx_compressFractions = new System.Windows.Forms.CheckBox();
            this.ckbx_compressBioReps = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantification_results)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_quantification_results
            // 
            this.dgv_quantification_results.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_quantification_results.Location = new System.Drawing.Point(82, 50);
            this.dgv_quantification_results.Name = "dgv_quantification_results";
            this.dgv_quantification_results.RowTemplate.Height = 28;
            this.dgv_quantification_results.Size = new System.Drawing.Size(678, 499);
            this.dgv_quantification_results.TabIndex = 0;
            this.dgv_quantification_results.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgv_quantification_results_DataBindingComplete);
            // 
            // ckbx_columnNormalize
            // 
            this.ckbx_columnNormalize.AutoSize = true;
            this.ckbx_columnNormalize.Checked = true;
            this.ckbx_columnNormalize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbx_columnNormalize.Location = new System.Drawing.Point(82, 585);
            this.ckbx_columnNormalize.Name = "ckbx_columnNormalize";
            this.ckbx_columnNormalize.Size = new System.Drawing.Size(163, 24);
            this.ckbx_columnNormalize.TabIndex = 1;
            this.ckbx_columnNormalize.Text = "Column Normalize";
            this.ckbx_columnNormalize.UseVisualStyleBackColor = true;
            this.ckbx_columnNormalize.CheckedChanged += new System.EventHandler(this.ckbx_columnNormalize_CheckedChanged);
            // 
            // ckbx_compressTechReps
            // 
            this.ckbx_compressTechReps.AutoSize = true;
            this.ckbx_compressTechReps.Checked = true;
            this.ckbx_compressTechReps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbx_compressTechReps.Location = new System.Drawing.Point(82, 673);
            this.ckbx_compressTechReps.Name = "ckbx_compressTechReps";
            this.ckbx_compressTechReps.Size = new System.Drawing.Size(257, 24);
            this.ckbx_compressTechReps.TabIndex = 2;
            this.ckbx_compressTechReps.Text = "Compress Technical Replicates";
            this.ckbx_compressTechReps.UseVisualStyleBackColor = true;
            this.ckbx_compressTechReps.CheckedChanged += new System.EventHandler(this.ckbx_compressTechReps_CheckedChanged);
            // 
            // ckbx_compressFractions
            // 
            this.ckbx_compressFractions.AutoSize = true;
            this.ckbx_compressFractions.Checked = true;
            this.ckbx_compressFractions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbx_compressFractions.Location = new System.Drawing.Point(82, 643);
            this.ckbx_compressFractions.Name = "ckbx_compressFractions";
            this.ckbx_compressFractions.Size = new System.Drawing.Size(177, 24);
            this.ckbx_compressFractions.TabIndex = 3;
            this.ckbx_compressFractions.Text = "Compress Fractions";
            this.ckbx_compressFractions.UseVisualStyleBackColor = true;
            this.ckbx_compressFractions.CheckedChanged += new System.EventHandler(this.ckbx_compressFractions_CheckedChanged);
            // 
            // ckbx_compressBioReps
            // 
            this.ckbx_compressBioReps.AutoSize = true;
            this.ckbx_compressBioReps.Checked = true;
            this.ckbx_compressBioReps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckbx_compressBioReps.Location = new System.Drawing.Point(82, 613);
            this.ckbx_compressBioReps.Name = "ckbx_compressBioReps";
            this.ckbx_compressBioReps.Size = new System.Drawing.Size(172, 24);
            this.ckbx_compressBioReps.TabIndex = 4;
            this.ckbx_compressBioReps.Text = "Compress BioReps";
            this.ckbx_compressBioReps.UseVisualStyleBackColor = true;
            this.ckbx_compressBioReps.CheckedChanged += new System.EventHandler(this.ckbx_compressBioReps_CheckedChanged);
            // 
            // Quantification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1106, 761);
            this.ControlBox = false;
            this.Controls.Add(this.ckbx_compressBioReps);
            this.Controls.Add(this.ckbx_compressFractions);
            this.Controls.Add(this.ckbx_compressTechReps);
            this.Controls.Add(this.ckbx_columnNormalize);
            this.Controls.Add(this.dgv_quantification_results);
            this.Name = "Quantification";
            this.Text = "Quantification";
            this.Load += new System.EventHandler(this.Quantification_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quantification_results)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_quantification_results;
        private System.Windows.Forms.CheckBox ckbx_columnNormalize;
        private System.Windows.Forms.CheckBox ckbx_compressTechReps;
        private System.Windows.Forms.CheckBox ckbx_compressFractions;
        private System.Windows.Forms.CheckBox ckbx_compressBioReps;
    }
}