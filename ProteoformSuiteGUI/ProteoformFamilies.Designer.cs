namespace ProteoformSuite
{
    partial class ProteoformFamilies
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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dgv_proteoform_families = new System.Windows.Forms.DataGridView();
            this.pictureBox_familyDisplay = new System.Windows.Forms.PictureBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.dgv_proteoform_family_members = new System.Windows.Forms.DataGridView();
            this.tb_singleton_count = new System.Windows.Forms.TextBox();
            this.lb_singletons = new System.Windows.Forms.Label();
            this.nud_decimalRoundingLabels = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.lb_timeStamp = new System.Windows.Forms.Label();
            this.tb_recentTimeStamp = new System.Windows.Forms.TextBox();
            this.btn_buildSelectedFamilies = new System.Windows.Forms.Button();
            this.btn_buildAllFamilies = new System.Windows.Forms.Button();
            this.label_tempFileFolder = new System.Windows.Forms.Label();
            this.tb_familyBuildFolder = new System.Windows.Forms.TextBox();
            this.btn_browseTempFolder = new System.Windows.Forms.Button();
            this.tb_IdentifiedFamilies = new System.Windows.Forms.TextBox();
            this.tb_TotalFamilies = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.Families_update = new System.Windows.Forms.Button();
            this.bt_export_families = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_proteoform_families)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_familyDisplay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_proteoform_family_members)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_decimalRoundingLabels)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(953, 677);
            this.splitContainer1.SplitterDistance = 335;
            this.splitContainer1.TabIndex = 3;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dgv_proteoform_families);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.pictureBox_familyDisplay);
            this.splitContainer2.Size = new System.Drawing.Size(953, 335);
            this.splitContainer2.SplitterDistance = 552;
            this.splitContainer2.TabIndex = 5;
            // 
            // dgv_proteoform_families
            // 
            this.dgv_proteoform_families.AllowUserToAddRows = false;
            this.dgv_proteoform_families.AllowUserToDeleteRows = false;
            this.dgv_proteoform_families.AllowUserToOrderColumns = true;
            this.dgv_proteoform_families.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_proteoform_families.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_proteoform_families.Location = new System.Drawing.Point(0, 0);
            this.dgv_proteoform_families.Name = "dgv_proteoform_families";
            this.dgv_proteoform_families.Size = new System.Drawing.Size(548, 331);
            this.dgv_proteoform_families.TabIndex = 2;
            this.dgv_proteoform_families.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_proteoform_families_CellContentClick);
            this.dgv_proteoform_families.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_proteoform_families_CellMouseClick);
            // 
            // pictureBox_familyDisplay
            // 
            this.pictureBox_familyDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_familyDisplay.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_familyDisplay.Name = "pictureBox_familyDisplay";
            this.pictureBox_familyDisplay.Size = new System.Drawing.Size(393, 331);
            this.pictureBox_familyDisplay.TabIndex = 4;
            this.pictureBox_familyDisplay.TabStop = false;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.dgv_proteoform_family_members);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.bt_export_families);
            this.splitContainer3.Panel2.Controls.Add(this.tb_singleton_count);
            this.splitContainer3.Panel2.Controls.Add(this.lb_singletons);
            this.splitContainer3.Panel2.Controls.Add(this.nud_decimalRoundingLabels);
            this.splitContainer3.Panel2.Controls.Add(this.label1);
            this.splitContainer3.Panel2.Controls.Add(this.lb_timeStamp);
            this.splitContainer3.Panel2.Controls.Add(this.tb_recentTimeStamp);
            this.splitContainer3.Panel2.Controls.Add(this.btn_buildSelectedFamilies);
            this.splitContainer3.Panel2.Controls.Add(this.btn_buildAllFamilies);
            this.splitContainer3.Panel2.Controls.Add(this.label_tempFileFolder);
            this.splitContainer3.Panel2.Controls.Add(this.tb_familyBuildFolder);
            this.splitContainer3.Panel2.Controls.Add(this.btn_browseTempFolder);
            this.splitContainer3.Panel2.Controls.Add(this.tb_IdentifiedFamilies);
            this.splitContainer3.Panel2.Controls.Add(this.tb_TotalFamilies);
            this.splitContainer3.Panel2.Controls.Add(this.label8);
            this.splitContainer3.Panel2.Controls.Add(this.label7);
            this.splitContainer3.Panel2.Controls.Add(this.Families_update);
            this.splitContainer3.Size = new System.Drawing.Size(949, 334);
            this.splitContainer3.SplitterDistance = 543;
            this.splitContainer3.TabIndex = 7;
            // 
            // dgv_proteoform_family_members
            // 
            this.dgv_proteoform_family_members.AllowUserToAddRows = false;
            this.dgv_proteoform_family_members.AllowUserToDeleteRows = false;
            this.dgv_proteoform_family_members.AllowUserToOrderColumns = true;
            this.dgv_proteoform_family_members.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_proteoform_family_members.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_proteoform_family_members.Location = new System.Drawing.Point(0, 0);
            this.dgv_proteoform_family_members.Name = "dgv_proteoform_family_members";
            this.dgv_proteoform_family_members.Size = new System.Drawing.Size(543, 334);
            this.dgv_proteoform_family_members.TabIndex = 3;
            // 
            // tb_singleton_count
            // 
            this.tb_singleton_count.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.tb_singleton_count.Location = new System.Drawing.Point(231, 276);
            this.tb_singleton_count.Margin = new System.Windows.Forms.Padding(2);
            this.tb_singleton_count.Name = "tb_singleton_count";
            this.tb_singleton_count.ReadOnly = true;
            this.tb_singleton_count.Size = new System.Drawing.Size(86, 20);
            this.tb_singleton_count.TabIndex = 44;
            // 
            // lb_singletons
            // 
            this.lb_singletons.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.lb_singletons.AutoSize = true;
            this.lb_singletons.Location = new System.Drawing.Point(47, 279);
            this.lb_singletons.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_singletons.Name = "lb_singletons";
            this.lb_singletons.Size = new System.Drawing.Size(144, 13);
            this.lb_singletons.TabIndex = 43;
            this.lb_singletons.Text = "Total Orphans (No Relations)";
            // 
            // nud_decimalRoundingLabels
            // 
            this.nud_decimalRoundingLabels.Location = new System.Drawing.Point(170, 79);
            this.nud_decimalRoundingLabels.Name = "nud_decimalRoundingLabels";
            this.nud_decimalRoundingLabels.Size = new System.Drawing.Size(49, 20);
            this.nud_decimalRoundingLabels.TabIndex = 42;
            this.nud_decimalRoundingLabels.ValueChanged += new System.EventHandler(this.nud_decimalRoundingLabels_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 13);
            this.label1.TabIndex = 41;
            this.label1.Text = "Decimal Rounding for Labels";
            // 
            // lb_timeStamp
            // 
            this.lb_timeStamp.AutoSize = true;
            this.lb_timeStamp.Location = new System.Drawing.Point(28, 49);
            this.lb_timeStamp.Name = "lb_timeStamp";
            this.lb_timeStamp.Size = new System.Drawing.Size(127, 13);
            this.lb_timeStamp.TabIndex = 39;
            this.lb_timeStamp.Text = "Most Recent Time Stamp";
            // 
            // tb_recentTimeStamp
            // 
            this.tb_recentTimeStamp.Location = new System.Drawing.Point(170, 46);
            this.tb_recentTimeStamp.Name = "tb_recentTimeStamp";
            this.tb_recentTimeStamp.ReadOnly = true;
            this.tb_recentTimeStamp.Size = new System.Drawing.Size(100, 20);
            this.tb_recentTimeStamp.TabIndex = 38;
            // 
            // btn_buildSelectedFamilies
            // 
            this.btn_buildSelectedFamilies.Location = new System.Drawing.Point(97, 139);
            this.btn_buildSelectedFamilies.Name = "btn_buildSelectedFamilies";
            this.btn_buildSelectedFamilies.Size = new System.Drawing.Size(195, 23);
            this.btn_buildSelectedFamilies.TabIndex = 7;
            this.btn_buildSelectedFamilies.Text = "Build Selected Families in Cytoscape";
            this.btn_buildSelectedFamilies.UseVisualStyleBackColor = true;
            this.btn_buildSelectedFamilies.Click += new System.EventHandler(this.btn_buildSelectedFamilies_Click);
            // 
            // btn_buildAllFamilies
            // 
            this.btn_buildAllFamilies.Location = new System.Drawing.Point(112, 110);
            this.btn_buildAllFamilies.Name = "btn_buildAllFamilies";
            this.btn_buildAllFamilies.Size = new System.Drawing.Size(169, 23);
            this.btn_buildAllFamilies.TabIndex = 6;
            this.btn_buildAllFamilies.Text = "Build All Families in Cytoscape";
            this.btn_buildAllFamilies.UseVisualStyleBackColor = true;
            this.btn_buildAllFamilies.Click += new System.EventHandler(this.btn_buildAllFamilies_Click);
            // 
            // label_tempFileFolder
            // 
            this.label_tempFileFolder.AutoSize = true;
            this.label_tempFileFolder.Location = new System.Drawing.Point(47, 17);
            this.label_tempFileFolder.Name = "label_tempFileFolder";
            this.label_tempFileFolder.Size = new System.Drawing.Size(109, 13);
            this.label_tempFileFolder.TabIndex = 5;
            this.label_tempFileFolder.Text = "Folder for Family Build";
            // 
            // tb_familyBuildFolder
            // 
            this.tb_familyBuildFolder.Location = new System.Drawing.Point(170, 14);
            this.tb_familyBuildFolder.Name = "tb_familyBuildFolder";
            this.tb_familyBuildFolder.Size = new System.Drawing.Size(100, 20);
            this.tb_familyBuildFolder.TabIndex = 4;
            this.tb_familyBuildFolder.TextChanged += new System.EventHandler(this.tb_tempFileFolderPath_TextChanged);
            // 
            // btn_browseTempFolder
            // 
            this.btn_browseTempFolder.Location = new System.Drawing.Point(286, 11);
            this.btn_browseTempFolder.Name = "btn_browseTempFolder";
            this.btn_browseTempFolder.Size = new System.Drawing.Size(75, 23);
            this.btn_browseTempFolder.TabIndex = 3;
            this.btn_browseTempFolder.Text = "Browse";
            this.btn_browseTempFolder.UseVisualStyleBackColor = true;
            this.btn_browseTempFolder.Click += new System.EventHandler(this.btn_browseTempFolder_Click);
            // 
            // tb_IdentifiedFamilies
            // 
            this.tb_IdentifiedFamilies.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.tb_IdentifiedFamilies.Location = new System.Drawing.Point(231, 253);
            this.tb_IdentifiedFamilies.Margin = new System.Windows.Forms.Padding(2);
            this.tb_IdentifiedFamilies.Name = "tb_IdentifiedFamilies";
            this.tb_IdentifiedFamilies.ReadOnly = true;
            this.tb_IdentifiedFamilies.Size = new System.Drawing.Size(86, 20);
            this.tb_IdentifiedFamilies.TabIndex = 37;
            // 
            // tb_TotalFamilies
            // 
            this.tb_TotalFamilies.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.tb_TotalFamilies.Location = new System.Drawing.Point(231, 229);
            this.tb_TotalFamilies.Margin = new System.Windows.Forms.Padding(2);
            this.tb_TotalFamilies.Name = "tb_TotalFamilies";
            this.tb_TotalFamilies.ReadOnly = true;
            this.tb_TotalFamilies.Size = new System.Drawing.Size(86, 20);
            this.tb_TotalFamilies.TabIndex = 36;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(47, 256);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 13);
            this.label8.TabIndex = 35;
            this.label8.Text = "Identified Families";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(47, 232);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 13);
            this.label7.TabIndex = 34;
            this.label7.Text = "Total Families";
            // 
            // Families_update
            // 
            this.Families_update.AllowDrop = true;
            this.Families_update.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.Families_update.Location = new System.Drawing.Point(2, 301);
            this.Families_update.Name = "Families_update";
            this.Families_update.Size = new System.Drawing.Size(402, 23);
            this.Families_update.TabIndex = 33;
            this.Families_update.Text = "Update";
            this.Families_update.UseMnemonic = false;
            this.Families_update.UseVisualStyleBackColor = true;
            this.Families_update.Click += new System.EventHandler(this.Families_update_Click);
            // 
            // bt_export_families
            // 
            this.bt_export_families.Location = new System.Drawing.Point(97, 168);
            this.bt_export_families.Name = "bt_export_families";
            this.bt_export_families.Size = new System.Drawing.Size(195, 23);
            this.bt_export_families.TabIndex = 45;
            this.bt_export_families.Text = "Export Identified Families List";
            this.bt_export_families.UseVisualStyleBackColor = true;
            this.bt_export_families.Click += new System.EventHandler(this.bt_export_families_Click);
            // 
            // ProteoformFamilies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 677);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ProteoformFamilies";
            this.Text = "ProteoformFamilies";
            this.Load += new System.EventHandler(this.ProteoformFamilies_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_proteoform_families)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_familyDisplay)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_proteoform_family_members)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_decimalRoundingLabels)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgv_proteoform_families;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView dgv_proteoform_family_members;
        private System.Windows.Forms.PictureBox pictureBox_familyDisplay;
        private System.Windows.Forms.Button btn_buildSelectedFamilies;
        private System.Windows.Forms.Button btn_buildAllFamilies;
        private System.Windows.Forms.Label label_tempFileFolder;
        private System.Windows.Forms.TextBox tb_familyBuildFolder;
        private System.Windows.Forms.Button btn_browseTempFolder;
        private System.Windows.Forms.Button Families_update;
        private System.Windows.Forms.TextBox tb_IdentifiedFamilies;
        private System.Windows.Forms.TextBox tb_TotalFamilies;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lb_timeStamp;
        private System.Windows.Forms.TextBox tb_recentTimeStamp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nud_decimalRoundingLabels;
        private System.Windows.Forms.TextBox tb_singleton_count;
        private System.Windows.Forms.Label lb_singletons;
        private System.Windows.Forms.Button bt_export_families;
    }
}