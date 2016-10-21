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
            this.Families_update = new System.Windows.Forms.Button();
            this.tb_IdentifiedFamilies = new System.Windows.Forms.TextBox();
            this.tb_TotalFamilies = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
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
            // Families_update
            // 
            this.Families_update.AllowDrop = true;
            this.Families_update.Location = new System.Drawing.Point(2, 301);
            this.Families_update.Name = "Families_update";
            this.Families_update.Size = new System.Drawing.Size(402, 23);
            this.Families_update.TabIndex = 33;
            this.Families_update.Text = "Update";
            this.Families_update.UseMnemonic = false;
            this.Families_update.UseVisualStyleBackColor = true;
            this.Families_update.Click += new System.EventHandler(this.Families_update_Click);
            // 
            // tb_IdentifiedFamilies
            // 
            this.tb_IdentifiedFamilies.Location = new System.Drawing.Point(190, 260);
            this.tb_IdentifiedFamilies.Margin = new System.Windows.Forms.Padding(2);
            this.tb_IdentifiedFamilies.Name = "tb_IdentifiedFamilies";
            this.tb_IdentifiedFamilies.ReadOnly = true;
            this.tb_IdentifiedFamilies.Size = new System.Drawing.Size(86, 20);
            this.tb_IdentifiedFamilies.TabIndex = 37;
            // 
            // tb_TotalFamilies
            // 
            this.tb_TotalFamilies.Location = new System.Drawing.Point(190, 228);
            this.tb_TotalFamilies.Margin = new System.Windows.Forms.Padding(2);
            this.tb_TotalFamilies.Name = "tb_TotalFamilies";
            this.tb_TotalFamilies.ReadOnly = true;
            this.tb_TotalFamilies.Size = new System.Drawing.Size(86, 20);
            this.tb_TotalFamilies.TabIndex = 36;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(80, 267);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 13);
            this.label8.TabIndex = 35;
            this.label8.Text = "Identified Families";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(80, 235);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 13);
            this.label7.TabIndex = 34;
            this.label7.Text = "Total Families";
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgv_proteoform_families;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView dgv_proteoform_family_members;
        private System.Windows.Forms.PictureBox pictureBox_familyDisplay;
        private System.Windows.Forms.Button Families_update;
        private System.Windows.Forms.TextBox tb_IdentifiedFamilies;
        private System.Windows.Forms.TextBox tb_TotalFamilies;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
    }
}