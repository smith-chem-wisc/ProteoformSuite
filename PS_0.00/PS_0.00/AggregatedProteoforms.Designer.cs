namespace PS_0._00
{
    partial class AggregatedProteoforms
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
            this.dgv_AggregatedProteoforms = new System.Windows.Forms.DataGridView();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.nUD_Missed_Monos = new System.Windows.Forms.NumericUpDown();
            this.nUD_RetTimeToleranace = new System.Windows.Forms.NumericUpDown();
            this.nUP_mass_tolerance = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dgv_AcceptNeuCdLtProteoforms = new System.Windows.Forms.DataGridView();
            this.nUD_Missed_Ks = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AggregatedProteoforms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Monos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_RetTimeToleranace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUP_mass_tolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AcceptNeuCdLtProteoforms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Ks)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_AggregatedProteoforms
            // 
            this.dgv_AggregatedProteoforms.AllowUserToOrderColumns = true;
            this.dgv_AggregatedProteoforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_AggregatedProteoforms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_AggregatedProteoforms.Location = new System.Drawing.Point(0, 0);
            this.dgv_AggregatedProteoforms.Name = "dgv_AggregatedProteoforms";
            this.dgv_AggregatedProteoforms.RowTemplate.Height = 28;
            this.dgv_AggregatedProteoforms.Size = new System.Drawing.Size(736, 295);
            this.dgv_AggregatedProteoforms.TabIndex = 0;
            this.dgv_AggregatedProteoforms.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_AggregatedProteoforms_CellContentClick);
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
            this.splitContainer1.Panel2.Controls.Add(this.dgv_AcceptNeuCdLtProteoforms);
            this.splitContainer1.Size = new System.Drawing.Size(1116, 587);
            this.splitContainer1.SplitterDistance = 299;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.nUD_Missed_Ks);
            this.splitContainer2.Panel1.Controls.Add(this.label4);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_Missed_Monos);
            this.splitContainer2.Panel1.Controls.Add(this.nUD_RetTimeToleranace);
            this.splitContainer2.Panel1.Controls.Add(this.nUP_mass_tolerance);
            this.splitContainer2.Panel1.Controls.Add(this.label3);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgv_AggregatedProteoforms);
            this.splitContainer2.Size = new System.Drawing.Size(1116, 299);
            this.splitContainer2.SplitterDistance = 372;
            this.splitContainer2.TabIndex = 0;
            // 
            // nUD_Missed_Monos
            // 
            this.nUD_Missed_Monos.Location = new System.Drawing.Point(231, 158);
            this.nUD_Missed_Monos.Name = "nUD_Missed_Monos";
            this.nUD_Missed_Monos.Size = new System.Drawing.Size(120, 26);
            this.nUD_Missed_Monos.TabIndex = 5;
            this.nUD_Missed_Monos.ValueChanged += new System.EventHandler(this.nUD_Missed_Monos_ValueChanged);
            // 
            // nUD_RetTimeToleranace
            // 
            this.nUD_RetTimeToleranace.DecimalPlaces = 2;
            this.nUD_RetTimeToleranace.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nUD_RetTimeToleranace.Location = new System.Drawing.Point(231, 122);
            this.nUD_RetTimeToleranace.Name = "nUD_RetTimeToleranace";
            this.nUD_RetTimeToleranace.Size = new System.Drawing.Size(120, 26);
            this.nUD_RetTimeToleranace.TabIndex = 4;
            this.nUD_RetTimeToleranace.ValueChanged += new System.EventHandler(this.nUD_RetTimeToleranace_ValueChanged);
            // 
            // nUP_mass_tolerance
            // 
            this.nUP_mass_tolerance.Location = new System.Drawing.Point(231, 86);
            this.nUP_mass_tolerance.Name = "nUP_mass_tolerance";
            this.nUP_mass_tolerance.Size = new System.Drawing.Size(120, 26);
            this.nUP_mass_tolerance.TabIndex = 3;
            this.nUP_mass_tolerance.ValueChanged += new System.EventHandler(this.nUP_mass_tolerance_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 160);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(210, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Missed Monoisotopics (num)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 124);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(190, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Ret. Time Tolerance (min)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(58, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mass Tolerance (ppm)";
            // 
            // dgv_AcceptNeuCdLtProteoforms
            // 
            this.dgv_AcceptNeuCdLtProteoforms.AllowUserToOrderColumns = true;
            this.dgv_AcceptNeuCdLtProteoforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_AcceptNeuCdLtProteoforms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_AcceptNeuCdLtProteoforms.Location = new System.Drawing.Point(0, 0);
            this.dgv_AcceptNeuCdLtProteoforms.Name = "dgv_AcceptNeuCdLtProteoforms";
            this.dgv_AcceptNeuCdLtProteoforms.RowTemplate.Height = 28;
            this.dgv_AcceptNeuCdLtProteoforms.Size = new System.Drawing.Size(1112, 280);
            this.dgv_AcceptNeuCdLtProteoforms.TabIndex = 0;
            // 
            // nUD_Missed_Ks
            // 
            this.nUD_Missed_Ks.Location = new System.Drawing.Point(231, 196);
            this.nUD_Missed_Ks.Name = "nUD_Missed_Ks";
            this.nUD_Missed_Ks.Size = new System.Drawing.Size(120, 26);
            this.nUD_Missed_Ks.TabIndex = 7;
            this.nUD_Missed_Ks.ValueChanged += new System.EventHandler(this.nUD_Missed_Ks_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 198);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(208, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Missed Lysine Counts (num)";
            // 
            // AggregatedProteoforms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1116, 587);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Name = "AggregatedProteoforms";
            this.Text = "AggregatedProteoforms";
            this.Load += new System.EventHandler(this.AggregatedProteoforms_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AggregatedProteoforms)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Monos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_RetTimeToleranace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUP_mass_tolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_AcceptNeuCdLtProteoforms)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_Missed_Ks)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_AggregatedProteoforms;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.NumericUpDown nUD_Missed_Monos;
        private System.Windows.Forms.NumericUpDown nUD_RetTimeToleranace;
        private System.Windows.Forms.NumericUpDown nUP_mass_tolerance;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgv_AcceptNeuCdLtProteoforms;
        private System.Windows.Forms.NumericUpDown nUD_Missed_Ks;
        private System.Windows.Forms.Label label4;
    }
}