namespace ProteoformSuite
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
            this.label = new System.Windows.Forms.Label();
            this.tb_RawExperimentalComponents = new System.Windows.Forms.TextBox();
            this.tb_neucodePairs = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tb_experimentalProteoforms = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_uniprotXmlDatabase = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lb_deconResults = new System.Windows.Forms.ListBox();
            this.tb_ETPairs = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_ETPeaks = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_EEPairs = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tb_EEPeaks = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tb_theoreticalProteoforms = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(21, 90);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(154, 13);
            this.label.TabIndex = 0;
            this.label.Text = "Raw Experimental Components";
            // 
            // tb_RawExperimentalComponents
            // 
            this.tb_RawExperimentalComponents.Location = new System.Drawing.Point(181, 90);
            this.tb_RawExperimentalComponents.Name = "tb_RawExperimentalComponents";
            this.tb_RawExperimentalComponents.ReadOnly = true;
            this.tb_RawExperimentalComponents.Size = new System.Drawing.Size(100, 20);
            this.tb_RawExperimentalComponents.TabIndex = 1;
            // 
            // tb_neucodePairs
            // 
            this.tb_neucodePairs.Location = new System.Drawing.Point(181, 124);
            this.tb_neucodePairs.Name = "tb_neucodePairs";
            this.tb_neucodePairs.ReadOnly = true;
            this.tb_neucodePairs.Size = new System.Drawing.Size(100, 20);
            this.tb_neucodePairs.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Neucode Pairs";
            // 
            // tb_experimentalProteoforms
            // 
            this.tb_experimentalProteoforms.Location = new System.Drawing.Point(181, 167);
            this.tb_experimentalProteoforms.Name = "tb_experimentalProteoforms";
            this.tb_experimentalProteoforms.ReadOnly = true;
            this.tb_experimentalProteoforms.Size = new System.Drawing.Size(100, 20);
            this.tb_experimentalProteoforms.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 167);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(126, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Experimental Proteoforms";
            // 
            // tb_uniprotXmlDatabase
            // 
            this.tb_uniprotXmlDatabase.Location = new System.Drawing.Point(181, 225);
            this.tb_uniprotXmlDatabase.Name = "tb_uniprotXmlDatabase";
            this.tb_uniprotXmlDatabase.ReadOnly = true;
            this.tb_uniprotXmlDatabase.Size = new System.Drawing.Size(444, 20);
            this.tb_uniprotXmlDatabase.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 225);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Theoretical Database";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(164, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Deconvolution Reuslt Filesnames";
            // 
            // lb_deconResults
            // 
            this.lb_deconResults.FormattingEnabled = true;
            this.lb_deconResults.Location = new System.Drawing.Point(182, 9);
            this.lb_deconResults.Name = "lb_deconResults";
            this.lb_deconResults.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lb_deconResults.Size = new System.Drawing.Size(556, 69);
            this.lb_deconResults.TabIndex = 9;
            // 
            // tb_ETPairs
            // 
            this.tb_ETPairs.Location = new System.Drawing.Point(181, 283);
            this.tb_ETPairs.Name = "tb_ETPairs";
            this.tb_ETPairs.ReadOnly = true;
            this.tb_ETPairs.Size = new System.Drawing.Size(100, 20);
            this.tb_ETPairs.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 286);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(141, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Experiment Theoretical Pairs";
            // 
            // tb_ETPeaks
            // 
            this.tb_ETPeaks.Location = new System.Drawing.Point(181, 324);
            this.tb_ETPeaks.Name = "tb_ETPeaks";
            this.tb_ETPeaks.ReadOnly = true;
            this.tb_ETPeaks.Size = new System.Drawing.Size(100, 20);
            this.tb_ETPeaks.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 324);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(148, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Experiment Theoretical Peaks";
            // 
            // tb_EEPairs
            // 
            this.tb_EEPairs.Location = new System.Drawing.Point(181, 389);
            this.tb_EEPairs.Name = "tb_EEPairs";
            this.tb_EEPairs.ReadOnly = true;
            this.tb_EEPairs.Size = new System.Drawing.Size(100, 20);
            this.tb_EEPairs.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(21, 389);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(140, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Experiment Experiment Pairs";
            // 
            // tb_EEPeaks
            // 
            this.tb_EEPeaks.Location = new System.Drawing.Point(181, 442);
            this.tb_EEPeaks.Name = "tb_EEPeaks";
            this.tb_EEPeaks.ReadOnly = true;
            this.tb_EEPeaks.Size = new System.Drawing.Size(100, 20);
            this.tb_EEPeaks.TabIndex = 17;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(21, 442);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(147, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Experiment Experiment Peaks";
            // 
            // tb_theoreticalProteoforms
            // 
            this.tb_theoreticalProteoforms.Location = new System.Drawing.Point(182, 247);
            this.tb_theoreticalProteoforms.Name = "tb_theoreticalProteoforms";
            this.tb_theoreticalProteoforms.ReadOnly = true;
            this.tb_theoreticalProteoforms.Size = new System.Drawing.Size(100, 20);
            this.tb_theoreticalProteoforms.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(21, 250);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(119, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Theoretical Proteoforms";
            // 
            // ResultsSummary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 620);
            this.Controls.Add(this.tb_theoreticalProteoforms);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tb_EEPeaks);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tb_EEPairs);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tb_ETPeaks);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tb_ETPairs);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lb_deconResults);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tb_uniprotXmlDatabase);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tb_experimentalProteoforms);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tb_neucodePairs);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_RawExperimentalComponents);
            this.Controls.Add(this.label);
            this.Name = "ResultsSummary";
            this.Text = "ResultsSummary";
            this.Load += new System.EventHandler(this.ResultsSummary_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label;
        private System.Windows.Forms.TextBox tb_RawExperimentalComponents;
        private System.Windows.Forms.TextBox tb_neucodePairs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tb_experimentalProteoforms;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_uniprotXmlDatabase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lb_deconResults;
        private System.Windows.Forms.TextBox tb_ETPairs;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_ETPeaks;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_EEPairs;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tb_EEPeaks;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tb_theoreticalProteoforms;
        private System.Windows.Forms.Label label9;
    }
}