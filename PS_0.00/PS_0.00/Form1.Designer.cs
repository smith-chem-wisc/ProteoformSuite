namespace PS_0._00
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadMethodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMethodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.processingPhaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadDeconvolutionResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rawExperimentalProteoformsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.neuCodeProteoformPairsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aggregatedProteoformsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.theoreticalProteoformDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.experimentTheoreticalComparisonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.experimentDecoyComparisonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.experimentExperimentComparisonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.proteoformFamilyAssignmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runMethodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateMethodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.processingPhaseToolStripMenuItem,
            this.runMethodToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(795, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.printToolStripMenuItem,
            this.loadMethodToolStripMenuItem,
            this.saveMethodToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.printToolStripMenuItem.Text = "Print";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // loadMethodToolStripMenuItem
            // 
            this.loadMethodToolStripMenuItem.Name = "loadMethodToolStripMenuItem";
            this.loadMethodToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.loadMethodToolStripMenuItem.Text = "Load Method";
            this.loadMethodToolStripMenuItem.Click += new System.EventHandler(this.loadMethodToolStripMenuItem_Click);
            // 
            // saveMethodToolStripMenuItem
            // 
            this.saveMethodToolStripMenuItem.Name = "saveMethodToolStripMenuItem";
            this.saveMethodToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveMethodToolStripMenuItem.Text = "Save Method";
            this.saveMethodToolStripMenuItem.Click += new System.EventHandler(this.saveMethodToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // processingPhaseToolStripMenuItem
            // 
            this.processingPhaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadDeconvolutionResultsToolStripMenuItem,
            this.rawExperimentalProteoformsToolStripMenuItem,
            this.neuCodeProteoformPairsToolStripMenuItem,
            this.aggregatedProteoformsToolStripMenuItem,
            this.theoreticalProteoformDatabaseToolStripMenuItem,
            this.experimentTheoreticalComparisonToolStripMenuItem,
            this.experimentDecoyComparisonToolStripMenuItem,
            this.experimentExperimentComparisonToolStripMenuItem,
            this.proteoformFamilyAssignmentToolStripMenuItem});
            this.processingPhaseToolStripMenuItem.Name = "processingPhaseToolStripMenuItem";
            this.processingPhaseToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.processingPhaseToolStripMenuItem.Text = "Processing Phase";
            // 
            // loadDeconvolutionResultsToolStripMenuItem
            // 
            this.loadDeconvolutionResultsToolStripMenuItem.Name = "loadDeconvolutionResultsToolStripMenuItem";
            this.loadDeconvolutionResultsToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.loadDeconvolutionResultsToolStripMenuItem.Text = "Load Deconvolution Results";
            this.loadDeconvolutionResultsToolStripMenuItem.Click += new System.EventHandler(this.loadDeconvolutionResultsToolStripMenuItem_Click);
            // 
            // rawExperimentalProteoformsToolStripMenuItem
            // 
            this.rawExperimentalProteoformsToolStripMenuItem.Name = "rawExperimentalProteoformsToolStripMenuItem";
            this.rawExperimentalProteoformsToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.rawExperimentalProteoformsToolStripMenuItem.Text = "Raw Experimental Components";
            this.rawExperimentalProteoformsToolStripMenuItem.Click += new System.EventHandler(this.rawExperimentalProteoformsToolStripMenuItem_Click);
            // 
            // neuCodeProteoformPairsToolStripMenuItem
            // 
            this.neuCodeProteoformPairsToolStripMenuItem.Name = "neuCodeProteoformPairsToolStripMenuItem";
            this.neuCodeProteoformPairsToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.neuCodeProteoformPairsToolStripMenuItem.Text = "NeuCode Proteoform Pairs";
            this.neuCodeProteoformPairsToolStripMenuItem.Click += new System.EventHandler(this.neuCodeProteoformPairsToolStripMenuItem_Click);
            // 
            // aggregatedProteoformsToolStripMenuItem
            // 
            this.aggregatedProteoformsToolStripMenuItem.Name = "aggregatedProteoformsToolStripMenuItem";
            this.aggregatedProteoformsToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.aggregatedProteoformsToolStripMenuItem.Text = "Aggregated Proteoforms";
            this.aggregatedProteoformsToolStripMenuItem.Click += new System.EventHandler(this.aggregatedProteoformsToolStripMenuItem_Click);
            // 
            // theoreticalProteoformDatabaseToolStripMenuItem
            // 
            this.theoreticalProteoformDatabaseToolStripMenuItem.Name = "theoreticalProteoformDatabaseToolStripMenuItem";
            this.theoreticalProteoformDatabaseToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.theoreticalProteoformDatabaseToolStripMenuItem.Text = "Theoretical Proteoform Database";
            this.theoreticalProteoformDatabaseToolStripMenuItem.Click += new System.EventHandler(this.theoreticalProteoformDatabaseToolStripMenuItem_Click);
            // 
            // experimentTheoreticalComparisonToolStripMenuItem
            // 
            this.experimentTheoreticalComparisonToolStripMenuItem.Name = "experimentTheoreticalComparisonToolStripMenuItem";
            this.experimentTheoreticalComparisonToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.experimentTheoreticalComparisonToolStripMenuItem.Text = "Experiment - Theoretical Comparison";
            this.experimentTheoreticalComparisonToolStripMenuItem.Click += new System.EventHandler(this.experimentTheoreticalComparisonToolStripMenuItem_Click);
            // 
            // experimentDecoyComparisonToolStripMenuItem
            // 
            this.experimentDecoyComparisonToolStripMenuItem.Name = "experimentDecoyComparisonToolStripMenuItem";
            this.experimentDecoyComparisonToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.experimentDecoyComparisonToolStripMenuItem.Text = "Experiment - Decoy Comparison";
            this.experimentDecoyComparisonToolStripMenuItem.Click += new System.EventHandler(this.experimentDecoyComparisonToolStripMenuItem_Click);
            // 
            // experimentExperimentComparisonToolStripMenuItem
            // 
            this.experimentExperimentComparisonToolStripMenuItem.Name = "experimentExperimentComparisonToolStripMenuItem";
            this.experimentExperimentComparisonToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.experimentExperimentComparisonToolStripMenuItem.Text = "Experiment - Experiment Comparison";
            this.experimentExperimentComparisonToolStripMenuItem.Click += new System.EventHandler(this.experimentExperimentComparisonToolStripMenuItem_Click);
            // 
            // proteoformFamilyAssignmentToolStripMenuItem
            // 
            this.proteoformFamilyAssignmentToolStripMenuItem.Name = "proteoformFamilyAssignmentToolStripMenuItem";
            this.proteoformFamilyAssignmentToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.proteoformFamilyAssignmentToolStripMenuItem.Text = "Proteoform Family Assignment";
            this.proteoformFamilyAssignmentToolStripMenuItem.Click += new System.EventHandler(this.proteoformFamilyAssignmentToolStripMenuItem_Click);
            // 
            // runMethodToolStripMenuItem
            // 
            this.runMethodToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generateMethodToolStripMenuItem,
            this.loadRunToolStripMenuItem});
            this.runMethodToolStripMenuItem.Name = "runMethodToolStripMenuItem";
            this.runMethodToolStripMenuItem.Size = new System.Drawing.Size(85, 22);
            this.runMethodToolStripMenuItem.Text = "Run Method";
            // 
            // generateMethodToolStripMenuItem
            // 
            this.generateMethodToolStripMenuItem.Name = "generateMethodToolStripMenuItem";
            this.generateMethodToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.generateMethodToolStripMenuItem.Text = "Generate Method";
            this.generateMethodToolStripMenuItem.Click += new System.EventHandler(this.generateMethodToolStripMenuItem_Click);
            // 
            // loadRunToolStripMenuItem
            // 
            this.loadRunToolStripMenuItem.Name = "loadRunToolStripMenuItem";
            this.loadRunToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.loadRunToolStripMenuItem.Text = "Load && Run";
            this.loadRunToolStripMenuItem.Click += new System.EventHandler(this.loadRunToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(795, 425);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Proteoform Suite 0.00";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem processingPhaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadDeconvolutionResultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rawExperimentalProteoformsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem neuCodeProteoformPairsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aggregatedProteoformsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem theoreticalProteoformDatabaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem experimentTheoreticalComparisonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem experimentDecoyComparisonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem experimentExperimentComparisonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem proteoformFamilyAssignmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadMethodToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMethodToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runMethodToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateMethodToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadRunToolStripMenuItem;
    }
}

