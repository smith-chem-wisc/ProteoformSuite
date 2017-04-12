namespace ProteoformSuiteGUI
{
    partial class ResultsSummary
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResultsSummary));
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.tb_summarySaveFolder = new System.Windows.Forms.TextBox();
            this.btn_browseSummarySaveFolder = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_save = new System.Windows.Forms.Button();
            this.cb_savePlots = new System.Windows.Forms.CheckBox();
            this.rtb_summary = new System.Windows.Forms.RichTextBox();
            this.cb_saveCytoScripts = new System.Windows.Forms.CheckBox();
            this.cb_saveDataframe = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1049, 60);
            this.splitter1.TabIndex = 0;
            this.splitter1.TabStop = false;
            // 
            // tb_summarySaveFolder
            // 
            this.tb_summarySaveFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_summarySaveFolder.Location = new System.Drawing.Point(3, 26);
            this.tb_summarySaveFolder.Name = "tb_summarySaveFolder";
            this.tb_summarySaveFolder.Size = new System.Drawing.Size(216, 22);
            this.tb_summarySaveFolder.TabIndex = 2;
            // 
            // btn_browseSummarySaveFolder
            // 
            this.btn_browseSummarySaveFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_browseSummarySaveFolder.Location = new System.Drawing.Point(225, 26);
            this.btn_browseSummarySaveFolder.Name = "btn_browseSummarySaveFolder";
            this.btn_browseSummarySaveFolder.Size = new System.Drawing.Size(75, 23);
            this.btn_browseSummarySaveFolder.TabIndex = 3;
            this.btn_browseSummarySaveFolder.Text = "Browse";
            this.btn_browseSummarySaveFolder.UseVisualStyleBackColor = true;
            this.btn_browseSummarySaveFolder.Click += new System.EventHandler(this.btn_browseSummarySaveFolder_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Save Results Summary";
            // 
            // btn_save
            // 
            this.btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save.Location = new System.Drawing.Point(306, 26);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(75, 23);
            this.btn_save.TabIndex = 5;
            this.btn_save.Text = "Save";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // cb_savePlots
            // 
            this.cb_savePlots.AutoSize = true;
            this.cb_savePlots.Checked = true;
            this.cb_savePlots.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_savePlots.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_savePlots.Location = new System.Drawing.Point(387, 28);
            this.cb_savePlots.Name = "cb_savePlots";
            this.cb_savePlots.Size = new System.Drawing.Size(159, 20);
            this.cb_savePlots.TabIndex = 6;
            this.cb_savePlots.Text = "Save Plots As Images";
            this.cb_savePlots.UseVisualStyleBackColor = true;
            // 
            // rtb_summary
            // 
            this.rtb_summary.BackColor = System.Drawing.SystemColors.Window;
            this.rtb_summary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_summary.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb_summary.Location = new System.Drawing.Point(0, 60);
            this.rtb_summary.Name = "rtb_summary";
            this.rtb_summary.ReadOnly = true;
            this.rtb_summary.Size = new System.Drawing.Size(1049, 787);
            this.rtb_summary.TabIndex = 9;
            this.rtb_summary.Text = "";
            // 
            // cb_saveCytoScripts
            // 
            this.cb_saveCytoScripts.AutoSize = true;
            this.cb_saveCytoScripts.Checked = true;
            this.cb_saveCytoScripts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_saveCytoScripts.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_saveCytoScripts.Location = new System.Drawing.Point(552, 28);
            this.cb_saveCytoScripts.Name = "cb_saveCytoScripts";
            this.cb_saveCytoScripts.Size = new System.Drawing.Size(171, 20);
            this.cb_saveCytoScripts.TabIndex = 10;
            this.cb_saveCytoScripts.Text = "Save Cytoscape Scripts";
            this.cb_saveCytoScripts.UseVisualStyleBackColor = true;
            // 
            // cb_saveDataframe
            // 
            this.cb_saveDataframe.AutoSize = true;
            this.cb_saveDataframe.Checked = true;
            this.cb_saveDataframe.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_saveDataframe.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_saveDataframe.Location = new System.Drawing.Point(729, 29);
            this.cb_saveDataframe.Name = "cb_saveDataframe";
            this.cb_saveDataframe.Size = new System.Drawing.Size(210, 20);
            this.cb_saveDataframe.TabIndex = 11;
            this.cb_saveDataframe.Text = "Save Proteoform ID Dataframe";
            this.cb_saveDataframe.UseVisualStyleBackColor = true;
            // 
            // ResultsSummary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1049, 847);
            this.ControlBox = false;
            this.Controls.Add(this.cb_saveDataframe);
            this.Controls.Add(this.cb_saveCytoScripts);
            this.Controls.Add(this.rtb_summary);
            this.Controls.Add(this.cb_savePlots);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_browseSummarySaveFolder);
            this.Controls.Add(this.tb_summarySaveFolder);
            this.Controls.Add(this.splitter1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ResultsSummary";
            this.Text = "Results Summary";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.TextBox tb_summarySaveFolder;
        private System.Windows.Forms.Button btn_browseSummarySaveFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.CheckBox cb_savePlots;
        private System.Windows.Forms.RichTextBox rtb_summary;
        private System.Windows.Forms.CheckBox cb_saveCytoScripts;
        private System.Windows.Forms.CheckBox cb_saveDataframe;
    }
}