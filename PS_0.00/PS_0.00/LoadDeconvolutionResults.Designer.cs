namespace PS_0._00
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
            this.lbDeconResults = new System.Windows.Forms.ListBox();
            this.btnDeconResultsAdd = new System.Windows.Forms.Button();
            this.btnDeconResultsRemove = new System.Windows.Forms.Button();
            this.btnDeconResultsClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbDeconResults
            // 
            this.lbDeconResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbDeconResults.FormattingEnabled = true;
            this.lbDeconResults.ItemHeight = 20;
            this.lbDeconResults.Location = new System.Drawing.Point(0, 0);
            this.lbDeconResults.Name = "lbDeconResults";
            this.lbDeconResults.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbDeconResults.Size = new System.Drawing.Size(1130, 584);
            this.lbDeconResults.Sorted = true;
            this.lbDeconResults.TabIndex = 0;
            this.lbDeconResults.SelectedIndexChanged += new System.EventHandler(this.lbDeconResults_SelectedIndexChanged);
            // 
            // btnDeconResultsAdd
            // 
            this.btnDeconResultsAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeconResultsAdd.Location = new System.Drawing.Point(13, 653);
            this.btnDeconResultsAdd.Name = "btnDeconResultsAdd";
            this.btnDeconResultsAdd.Size = new System.Drawing.Size(103, 44);
            this.btnDeconResultsAdd.TabIndex = 1;
            this.btnDeconResultsAdd.Text = "Add";
            this.btnDeconResultsAdd.UseVisualStyleBackColor = true;
            this.btnDeconResultsAdd.Click += new System.EventHandler(this.btnDeconResultsAdd_Click);
            // 
            // btnDeconResultsRemove
            // 
            this.btnDeconResultsRemove.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnDeconResultsRemove.Location = new System.Drawing.Point(533, 653);
            this.btnDeconResultsRemove.Name = "btnDeconResultsRemove";
            this.btnDeconResultsRemove.Size = new System.Drawing.Size(103, 44);
            this.btnDeconResultsRemove.TabIndex = 2;
            this.btnDeconResultsRemove.Text = "Remove";
            this.btnDeconResultsRemove.UseVisualStyleBackColor = true;
            this.btnDeconResultsRemove.Click += new System.EventHandler(this.btnDeconResultsRemove_Click);
            // 
            // btnDeconResultsClear
            // 
            this.btnDeconResultsClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeconResultsClear.Location = new System.Drawing.Point(1006, 653);
            this.btnDeconResultsClear.Name = "btnDeconResultsClear";
            this.btnDeconResultsClear.Size = new System.Drawing.Size(103, 44);
            this.btnDeconResultsClear.TabIndex = 3;
            this.btnDeconResultsClear.Text = "Clear";
            this.btnDeconResultsClear.UseVisualStyleBackColor = true;
            this.btnDeconResultsClear.Click += new System.EventHandler(this.btnDeconResultsClear_Click);
            // 
            // LoadDeconvolutionResults
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1130, 722);
            this.ControlBox = false;
            this.Controls.Add(this.btnDeconResultsClear);
            this.Controls.Add(this.btnDeconResultsRemove);
            this.Controls.Add(this.btnDeconResultsAdd);
            this.Controls.Add(this.lbDeconResults);
            this.Name = "LoadDeconvolutionResults";
            this.Text = "LoadDeconvolutionResults";
            this.Load += new System.EventHandler(this.LoadDeconvolutionResults_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbDeconResults;
        private System.Windows.Forms.Button btnDeconResultsAdd;
        private System.Windows.Forms.Button btnDeconResultsRemove;
        private System.Windows.Forms.Button btnDeconResultsClear;
    }
}