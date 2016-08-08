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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lbDeconResults = new System.Windows.Forms.ListBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lbCorrectionFiles = new System.Windows.Forms.ListBox();
            this.btnDeconResultsClear = new System.Windows.Forms.Button();
            this.btn_AddCorrectionFactors = new System.Windows.Forms.Button();
            this.btnDeconResultsRemove = new System.Windows.Forms.Button();
            this.btnDeconResultsAdd = new System.Windows.Forms.Button();
            this.cb_neuCodeLabeled = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lbDeconResults);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1486, 889);
            this.splitContainer1.SplitterDistance = 444;
            this.splitContainer1.TabIndex = 6;
            // 
            // lbDeconResults
            // 
            this.lbDeconResults.BackColor = System.Drawing.Color.White;
            this.lbDeconResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbDeconResults.FormattingEnabled = true;
            this.lbDeconResults.ItemHeight = 20;
            this.lbDeconResults.Location = new System.Drawing.Point(0, 0);

            this.lbDeconResults.Margin = new System.Windows.Forms.Padding(2);
            this.lbDeconResults.Name = "lbDeconResults";
            this.lbDeconResults.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbDeconResults.Size = new System.Drawing.Size(755, 394);

            this.lbDeconResults.Size = new System.Drawing.Size(1486, 444);

            this.lbDeconResults.Name = "lbDeconResults";
            this.lbDeconResults.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbDeconResults.Size = new System.Drawing.Size(1486, 444);

            this.lbDeconResults.Sorted = true;
            this.lbDeconResults.TabIndex = 1;
            // 
            // splitContainer2
            // 

            this.btnDeconResultsAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeconResultsAdd.Location = new System.Drawing.Point(9, 424);
            this.btnDeconResultsAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btnDeconResultsAdd.Name = "btnDeconResultsAdd";
            this.btnDeconResultsAdd.Size = new System.Drawing.Size(69, 29);
            this.btnDeconResultsAdd.TabIndex = 1;
            this.btnDeconResultsAdd.Text = "Add";
            this.btnDeconResultsAdd.UseVisualStyleBackColor = true;

            this.btnDeconResultsAdd.Click += new System.EventHandler(this.btnDeconResultsAdd_Click_1);

            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;

            // 
            // splitContainer2.Panel1
            // 

            // 
            // splitContainer2.Panel1
            // 

            this.btnDeconResultsRemove.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnDeconResultsRemove.Location = new System.Drawing.Point(355, 424);
            this.btnDeconResultsRemove.Margin = new System.Windows.Forms.Padding(2);
            this.btnDeconResultsRemove.Name = "btnDeconResultsRemove";
            this.btnDeconResultsRemove.Size = new System.Drawing.Size(69, 29);
            this.btnDeconResultsRemove.TabIndex = 2;
            this.btnDeconResultsRemove.Text = "Remove";
            this.btnDeconResultsRemove.UseVisualStyleBackColor = true;
            this.btnDeconResultsRemove.Click += new System.EventHandler(this.btnDeconResultsAdd_Click_1);
            // 
            // btnDeconResultsClear
            // 
            this.btnDeconResultsClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeconResultsClear.Location = new System.Drawing.Point(671, 424);
            this.btnDeconResultsClear.Margin = new System.Windows.Forms.Padding(2);

            this.splitContainer2.Panel1.Controls.Add(this.lbCorrectionFiles);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnDeconResultsClear);
            this.splitContainer2.Panel2.Controls.Add(this.btn_AddCorrectionFactors);
            this.splitContainer2.Panel2.Controls.Add(this.btnDeconResultsRemove);
            this.splitContainer2.Panel2.Controls.Add(this.btnDeconResultsAdd);
            this.splitContainer2.Panel2.Controls.Add(this.cb_neuCodeLabeled);
            this.splitContainer2.Size = new System.Drawing.Size(1486, 441);
            this.splitContainer2.SplitterDistance = 144;
            this.splitContainer2.TabIndex = 0;
            // 
            // lbCorrectionFiles
            // 
            this.lbCorrectionFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCorrectionFiles.FormattingEnabled = true;
            this.lbCorrectionFiles.ItemHeight = 20;
            this.lbCorrectionFiles.Location = new System.Drawing.Point(0, 0);
            this.lbCorrectionFiles.Name = "lbCorrectionFiles";
            this.lbCorrectionFiles.Size = new System.Drawing.Size(1486, 144);
            this.lbCorrectionFiles.TabIndex = 0;
            // 
            // btnDeconResultsClear
            // 
            this.splitContainer2.Panel1.Controls.Add(this.lbCorrectionFiles);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnDeconResultsClear);
            this.splitContainer2.Panel2.Controls.Add(this.btn_AddCorrectionFactors);
            this.splitContainer2.Panel2.Controls.Add(this.btnDeconResultsRemove);
            this.splitContainer2.Panel2.Controls.Add(this.btnDeconResultsAdd);
            this.splitContainer2.Panel2.Controls.Add(this.cb_neuCodeLabeled);
            this.splitContainer2.Size = new System.Drawing.Size(1486, 441);
            this.splitContainer2.SplitterDistance = 144;
            this.splitContainer2.TabIndex = 0;
            // 
            // lbCorrectionFiles
            // 
            this.lbCorrectionFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbCorrectionFiles.FormattingEnabled = true;
            this.lbCorrectionFiles.ItemHeight = 20;
            this.lbCorrectionFiles.Location = new System.Drawing.Point(0, 0);
            this.lbCorrectionFiles.Name = "lbCorrectionFiles";
            this.lbCorrectionFiles.Size = new System.Drawing.Size(1486, 144);
            this.lbCorrectionFiles.TabIndex = 0;
            // 
            // btnDeconResultsClear
            // 

            this.btnDeconResultsClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeconResultsClear.Location = new System.Drawing.Point(607, 62);

            this.btnDeconResultsClear.Name = "btnDeconResultsClear";
            this.btnDeconResultsClear.Size = new System.Drawing.Size(168, 58);
            this.btnDeconResultsClear.TabIndex = 9;
            this.btnDeconResultsClear.Text = "Clear";
            this.btnDeconResultsClear.UseVisualStyleBackColor = true;
            this.btnDeconResultsClear.Click += new System.EventHandler(this.btnDeconResultsClear_Click_1);
            // 
            // btn_AddCorrectionFactors
            // 
            this.btn_AddCorrectionFactors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_AddCorrectionFactors.Location = new System.Drawing.Point(414, 62);
            this.btn_AddCorrectionFactors.Name = "btn_AddCorrectionFactors";
            this.btn_AddCorrectionFactors.Size = new System.Drawing.Size(168, 58);
            this.btn_AddCorrectionFactors.TabIndex = 8;
            this.btn_AddCorrectionFactors.Text = "Add Correction Factors";
            this.btn_AddCorrectionFactors.UseVisualStyleBackColor = true;
            this.btn_AddCorrectionFactors.Click += new System.EventHandler(this.btn_AddCorrectionFactors_Click_1);
            // 
            // btnDeconResultsRemove
            // 
            this.btnDeconResultsRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeconResultsRemove.Location = new System.Drawing.Point(217, 62);
            this.btnDeconResultsRemove.Name = "btnDeconResultsRemove";
            this.btnDeconResultsRemove.Size = new System.Drawing.Size(168, 58);
            this.btnDeconResultsRemove.TabIndex = 7;
            this.btnDeconResultsRemove.Text = "Remove";
            this.btnDeconResultsRemove.UseVisualStyleBackColor = true;
            this.btnDeconResultsRemove.Click += new System.EventHandler(this.btnDeconResultsRemove_Click_1);
            // 
            // btnDeconResultsAdd
            // 
            this.btnDeconResultsAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeconResultsAdd.Location = new System.Drawing.Point(16, 62);
            this.btnDeconResultsAdd.Name = "btnDeconResultsAdd";
            this.btnDeconResultsAdd.Size = new System.Drawing.Size(168, 58);
            this.btnDeconResultsAdd.TabIndex = 6;
            this.btnDeconResultsAdd.Text = "Add Deconvolution Results";
            this.btnDeconResultsAdd.UseVisualStyleBackColor = true;
            this.btnDeconResultsAdd.Click += new System.EventHandler(this.btnDeconResultsAdd_Click_1);
            // 
            // cb_neuCodeLabeled
            // 
            this.cb_neuCodeLabeled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cb_neuCodeLabeled.Checked = true;
            this.cb_neuCodeLabeled.CheckState = System.Windows.Forms.CheckState.Checked;


            this.cb_neuCodeLabeled.Location = new System.Drawing.Point(16, 14);
            this.cb_neuCodeLabeled.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);

            this.cb_neuCodeLabeled.Name = "cb_neuCodeLabeled";
            this.cb_neuCodeLabeled.Size = new System.Drawing.Size(168, 26);
            this.cb_neuCodeLabeled.TabIndex = 5;
            this.cb_neuCodeLabeled.Text = "NeuCode Labeled";
            this.cb_neuCodeLabeled.UseVisualStyleBackColor = true;
            this.cb_neuCodeLabeled.CheckedChanged += new System.EventHandler(this.cb_neuCodeLabeled_CheckedChanged_1);
            // 
            // LoadDeconvolutionResults
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1486, 889);
            this.ControlBox = false;
            this.Controls.Add(this.cb_neuCodeLabeled);
            this.Controls.Add(this.btnDeconResultsClear);
            this.Controls.Add(this.btnDeconResultsRemove);
            this.Controls.Add(this.btnDeconResultsAdd);
            this.Controls.Add(this.lbDeconResults);
            this.Margin = new System.Windows.Forms.Padding(2);

            this.Controls.Add(this.splitContainer1);

            this.Name = "LoadDeconvolutionResults";
            this.Text = "LoadDeconvolutionResults";
            this.Load += new System.EventHandler(this.LoadDeconvolutionResults_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lbDeconResults;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox lbCorrectionFiles;
        private System.Windows.Forms.Button btnDeconResultsClear;
        private System.Windows.Forms.Button btn_AddCorrectionFactors;
        private System.Windows.Forms.Button btnDeconResultsRemove;
        private System.Windows.Forms.Button btnDeconResultsAdd;
        private System.Windows.Forms.CheckBox cb_neuCodeLabeled;
    }
}