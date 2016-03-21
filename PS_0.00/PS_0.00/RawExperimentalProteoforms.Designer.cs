namespace PS_0._00
{
    partial class RawExperimentalProteoforms
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
            this.dgv_RawExpProt_MI_masses = new System.Windows.Forms.DataGridView();
            this.dgv_RawExpProt_IndChgSts = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpProt_MI_masses)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpProt_IndChgSts)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.dgv_RawExpProt_MI_masses);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgv_RawExpProt_IndChgSts);
            this.splitContainer1.Size = new System.Drawing.Size(1076, 565);
            this.splitContainer1.SplitterDistance = 285;
            this.splitContainer1.TabIndex = 0;
            // 
            // dgv_RawExpProt_MI_masses
            // 
            this.dgv_RawExpProt_MI_masses.AllowUserToOrderColumns = true;
            this.dgv_RawExpProt_MI_masses.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.dgv_RawExpProt_MI_masses.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
            this.dgv_RawExpProt_MI_masses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_RawExpProt_MI_masses.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_RawExpProt_MI_masses.Location = new System.Drawing.Point(0, 0);
            this.dgv_RawExpProt_MI_masses.Name = "dgv_RawExpProt_MI_masses";
            this.dgv_RawExpProt_MI_masses.RowTemplate.Height = 28;
            this.dgv_RawExpProt_MI_masses.Size = new System.Drawing.Size(1072, 281);
            this.dgv_RawExpProt_MI_masses.TabIndex = 0;
            this.dgv_RawExpProt_MI_masses.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_RawExpProt_MI_masses_CellContentClick);
            // 
            // dgv_RawExpProt_IndChgSts
            // 
            this.dgv_RawExpProt_IndChgSts.AllowUserToOrderColumns = true;
            this.dgv_RawExpProt_IndChgSts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_RawExpProt_IndChgSts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_RawExpProt_IndChgSts.Location = new System.Drawing.Point(0, 0);
            this.dgv_RawExpProt_IndChgSts.Name = "dgv_RawExpProt_IndChgSts";
            this.dgv_RawExpProt_IndChgSts.RowTemplate.Height = 28;
            this.dgv_RawExpProt_IndChgSts.Size = new System.Drawing.Size(1072, 272);
            this.dgv_RawExpProt_IndChgSts.TabIndex = 0;
            // 
            // RawExperimentalProteoforms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1076, 565);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Name = "RawExperimentalProteoforms";
            this.Text = "Raw Experimental Proteoforms";
            this.Load += new System.EventHandler(this.RawExperimentalProteoforms_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpProt_MI_masses)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_RawExpProt_IndChgSts)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgv_RawExpProt_IndChgSts;
        private System.Windows.Forms.DataGridView dgv_RawExpProt_MI_masses;
    }
}