namespace ProteoformSuiteGUI
{
    partial class IdentifiedProteoforms
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
            this.param_splitcontainer = new System.Windows.Forms.SplitContainer();
            this.label12 = new System.Windows.Forms.Label();
            this.tb_tableFilter = new System.Windows.Forms.TextBox();
            this.btn_compare_with_td = new System.Windows.Forms.Button();
            this.tb_not_td = new System.Windows.Forms.TextBox();
            this.dgv_identified_experimentals = new System.Windows.Forms.DataGridView();
            this.dgv_td_proteoforms = new System.Windows.Forms.DataGridView();
            this.tb_topdown = new System.Windows.Forms.TextBox();
            this.dgv_bu_peptides = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.param_splitcontainer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identified_experimentals)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_td_proteoforms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_bu_peptides)).BeginInit();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // 
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // 
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgv_bu_peptides);
            this.splitContainer1.Size = new System.Drawing.Size(1287, 655);
            this.splitContainer1.SplitterDistance = 1134;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // 
            // 
            this.splitContainer2.Panel1.Controls.Add(this.param_splitcontainer);
            // 
            // 
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgv_td_proteoforms);
            this.splitContainer2.Panel2.Controls.Add(this.tb_topdown);
            this.splitContainer2.Size = new System.Drawing.Size(1134, 655);
            this.splitContainer2.SplitterDistance = 315;
            this.splitContainer2.TabIndex = 1;
            // 
            // param_splitcontainer
            // 
            this.param_splitcontainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.param_splitcontainer.Location = new System.Drawing.Point(0, 0);
            this.param_splitcontainer.Name = "param_splitcontainer";
            this.param_splitcontainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // 
            // 
            this.param_splitcontainer.Panel1.Controls.Add(this.label12);
            this.param_splitcontainer.Panel1.Controls.Add(this.tb_tableFilter);
            this.param_splitcontainer.Panel1.Controls.Add(this.btn_compare_with_td);
            // 
            // 
            // 
            this.param_splitcontainer.Panel2.Controls.Add(this.tb_not_td);
            this.param_splitcontainer.Panel2.Controls.Add(this.dgv_identified_experimentals);
            this.param_splitcontainer.Size = new System.Drawing.Size(1134, 315);
            this.param_splitcontainer.SplitterDistance = 25;
            this.param_splitcontainer.TabIndex = 0;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(397, 5);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(69, 15);
            this.label12.TabIndex = 5;
            this.label12.Text = "Table Filters";
            // 
            // tb_tableFilter
            // 
            this.tb_tableFilter.Location = new System.Drawing.Point(246, 2);
            this.tb_tableFilter.Name = "tb_tableFilter";
            this.tb_tableFilter.Size = new System.Drawing.Size(145, 23);
            this.tb_tableFilter.TabIndex = 4;
            // 
            // btn_compare_with_td
            // 
            this.btn_compare_with_td.Location = new System.Drawing.Point(0, 0);
            this.btn_compare_with_td.Name = "btn_compare_with_td";
            this.btn_compare_with_td.Size = new System.Drawing.Size(207, 23);
            this.btn_compare_with_td.TabIndex = 0;
            this.btn_compare_with_td.Text = "Compare with Other Top-Down Results";
            this.btn_compare_with_td.UseVisualStyleBackColor = true;
            // 
            // tb_not_td
            // 
            this.tb_not_td.Dock = System.Windows.Forms.DockStyle.Top;
            this.tb_not_td.Location = new System.Drawing.Point(0, 0);
            this.tb_not_td.Name = "tb_not_td";
            this.tb_not_td.ReadOnly = true;
            this.tb_not_td.Size = new System.Drawing.Size(1134, 23);
            this.tb_not_td.TabIndex = 6;
            // 
            // dgv_identified_experimentals
            // 
            this.dgv_identified_experimentals.AllowUserToAddRows = false;
            this.dgv_identified_experimentals.AllowUserToDeleteRows = false;
            this.dgv_identified_experimentals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_identified_experimentals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_identified_experimentals.Location = new System.Drawing.Point(0, 0);
            this.dgv_identified_experimentals.Name = "dgv_identified_experimentals";
            this.dgv_identified_experimentals.Size = new System.Drawing.Size(1134, 286);
            this.dgv_identified_experimentals.TabIndex = 5;
            this.dgv_identified_experimentals.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_identified_experimentals_CellMouseClick);
            // 
            // dgv_td_proteoforms
            // 
            this.dgv_td_proteoforms.AllowUserToAddRows = false;
            this.dgv_td_proteoforms.AllowUserToDeleteRows = false;
            this.dgv_td_proteoforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_td_proteoforms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_td_proteoforms.Location = new System.Drawing.Point(0, 23);
            this.dgv_td_proteoforms.Name = "dgv_td_proteoforms";
            this.dgv_td_proteoforms.Size = new System.Drawing.Size(1134, 313);
            this.dgv_td_proteoforms.TabIndex = 7;
            this.dgv_td_proteoforms.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_td_proteoforms_experimentals_CellMouseClick);
            // 
            // tb_topdown
            // 
            this.tb_topdown.Dock = System.Windows.Forms.DockStyle.Top;
            this.tb_topdown.Location = new System.Drawing.Point(0, 0);
            this.tb_topdown.Name = "tb_topdown";
            this.tb_topdown.ReadOnly = true;
            this.tb_topdown.Size = new System.Drawing.Size(1134, 23);
            this.tb_topdown.TabIndex = 6;
            // 
            // dgv_bu_peptides
            // 
            this.dgv_bu_peptides.AllowUserToAddRows = false;
            this.dgv_bu_peptides.AllowUserToDeleteRows = false;
            this.dgv_bu_peptides.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_bu_peptides.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_bu_peptides.Location = new System.Drawing.Point(0, 0);
            this.dgv_bu_peptides.Name = "dgv_bu_peptides";
            this.dgv_bu_peptides.Size = new System.Drawing.Size(149, 655);
            this.dgv_bu_peptides.TabIndex = 6;
            // 
            // IdentifiedProteoforms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1287, 655);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Name = "IdentifiedProteoforms";
            this.Text = "Identified Proteoforms";
            this.Load += new System.EventHandler(this.IdentifiedProteoforms_Load);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.param_splitcontainer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_identified_experimentals)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_td_proteoforms)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_bu_peptides)).EndInit();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer param_splitcontainer;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tb_tableFilter;
        private System.Windows.Forms.Button btn_compare_with_td;
        private System.Windows.Forms.TextBox tb_not_td;
        private System.Windows.Forms.DataGridView dgv_identified_experimentals;
        private System.Windows.Forms.DataGridView dgv_td_proteoforms;
        private System.Windows.Forms.TextBox tb_topdown;
        private System.Windows.Forms.DataGridView dgv_bu_peptides;
    }
}