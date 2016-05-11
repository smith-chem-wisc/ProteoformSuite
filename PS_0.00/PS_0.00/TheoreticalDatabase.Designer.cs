namespace PS_0._00
{
    partial class TheoreticalDatabase
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
            this.label4 = new System.Windows.Forms.Label();
            this.nUD_MinPeptideLength = new System.Windows.Forms.NumericUpDown();
            this.btn_Make_Databases = new System.Windows.Forms.Button();
            this.tb_UniProtPtmList_Path = new System.Windows.Forms.TextBox();
            this.btn_UniPtPtmList = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbx_DisplayWhichDB = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nUD_NumDecoyDBs = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.nUD_MaxPTMs = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_NeuCode_Hv = new System.Windows.Forms.RadioButton();
            this.btn_NeuCode_Lt = new System.Windows.Forms.RadioButton();
            this.btn_NaturalIsotopes = new System.Windows.Forms.RadioButton();
            this.ckbx_Meth_Cleaved = new System.Windows.Forms.CheckBox();
            this.ckbx_Carbam = new System.Windows.Forms.CheckBox();
            this.ckbx_OxidMeth = new System.Windows.Forms.CheckBox();
            this.tb_UniProtXML_Path = new System.Windows.Forms.TextBox();
            this.btn_GetUniProtXML = new System.Windows.Forms.Button();
            this.dgv_Database = new System.Windows.Forms.DataGridView();
            this.ckbx_aggregateProteoforms = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_MinPeptideLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_NumDecoyDBs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_MaxPTMs)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Database)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ckbx_aggregateProteoforms);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.nUD_MinPeptideLength);
            this.splitContainer1.Panel1.Controls.Add(this.btn_Make_Databases);
            this.splitContainer1.Panel1.Controls.Add(this.tb_UniProtPtmList_Path);
            this.splitContainer1.Panel1.Controls.Add(this.btn_UniPtPtmList);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.cmbx_DisplayWhichDB);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.nUD_NumDecoyDBs);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.nUD_MaxPTMs);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this.ckbx_Meth_Cleaved);
            this.splitContainer1.Panel1.Controls.Add(this.ckbx_Carbam);
            this.splitContainer1.Panel1.Controls.Add(this.ckbx_OxidMeth);
            this.splitContainer1.Panel1.Controls.Add(this.tb_UniProtXML_Path);
            this.splitContainer1.Panel1.Controls.Add(this.btn_GetUniProtXML);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgv_Database);
            this.splitContainer1.Size = new System.Drawing.Size(1182, 735);
            this.splitContainer1.SplitterDistance = 393;
            this.splitContainer1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(105, 518);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(184, 20);
            this.label4.TabIndex = 16;
            this.label4.Text = "Minimum Peptide Length";
            // 
            // nUD_MinPeptideLength
            // 
            this.nUD_MinPeptideLength.Location = new System.Drawing.Point(26, 514);
            this.nUD_MinPeptideLength.Name = "nUD_MinPeptideLength";
            this.nUD_MinPeptideLength.Size = new System.Drawing.Size(72, 26);
            this.nUD_MinPeptideLength.TabIndex = 15;
            // 
            // btn_Make_Databases
            // 
            this.btn_Make_Databases.Location = new System.Drawing.Point(22, 683);
            this.btn_Make_Databases.Name = "btn_Make_Databases";
            this.btn_Make_Databases.Size = new System.Drawing.Size(310, 34);
            this.btn_Make_Databases.TabIndex = 14;
            this.btn_Make_Databases.Text = "Time to make the databases";
            this.btn_Make_Databases.UseVisualStyleBackColor = true;
            this.btn_Make_Databases.Click += new System.EventHandler(this.btn_Make_Databases_Click);
            // 
            // tb_UniProtPtmList_Path
            // 
            this.tb_UniProtPtmList_Path.Location = new System.Drawing.Point(10, 115);
            this.tb_UniProtPtmList_Path.Name = "tb_UniProtPtmList_Path";
            this.tb_UniProtPtmList_Path.Size = new System.Drawing.Size(322, 26);
            this.tb_UniProtPtmList_Path.TabIndex = 13;
            // 
            // btn_UniPtPtmList
            // 
            this.btn_UniPtPtmList.Location = new System.Drawing.Point(10, 78);
            this.btn_UniPtPtmList.Name = "btn_UniPtPtmList";
            this.btn_UniPtPtmList.Size = new System.Drawing.Size(172, 31);
            this.btn_UniPtPtmList.TabIndex = 12;
            this.btn_UniPtPtmList.Text = "Get UniProt PTM List";
            this.btn_UniPtPtmList.UseVisualStyleBackColor = true;
            this.btn_UniPtPtmList.Click += new System.EventHandler(this.btn_UniPtPtmList_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 574);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 20);
            this.label3.TabIndex = 11;
            this.label3.Text = "Database Displayed";
            // 
            // cmbx_DisplayWhichDB
            // 
            this.cmbx_DisplayWhichDB.FormattingEnabled = true;
            this.cmbx_DisplayWhichDB.Location = new System.Drawing.Point(26, 597);
            this.cmbx_DisplayWhichDB.Name = "cmbx_DisplayWhichDB";
            this.cmbx_DisplayWhichDB.Size = new System.Drawing.Size(306, 28);
            this.cmbx_DisplayWhichDB.TabIndex = 10;
            this.cmbx_DisplayWhichDB.SelectedIndexChanged += new System.EventHandler(this.cmbx_DisplayWhichDB_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(105, 478);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(214, 20);
            this.label2.TabIndex = 9;
            this.label2.Text = "Number of Decoy Databases";
            // 
            // nUD_NumDecoyDBs
            // 
            this.nUD_NumDecoyDBs.Location = new System.Drawing.Point(26, 474);
            this.nUD_NumDecoyDBs.Name = "nUD_NumDecoyDBs";
            this.nUD_NumDecoyDBs.Size = new System.Drawing.Size(72, 26);
            this.nUD_NumDecoyDBs.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(105, 442);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(192, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Max PTMs per Proteoform";
            // 
            // nUD_MaxPTMs
            // 
            this.nUD_MaxPTMs.Location = new System.Drawing.Point(26, 435);
            this.nUD_MaxPTMs.Name = "nUD_MaxPTMs";
            this.nUD_MaxPTMs.Size = new System.Drawing.Size(72, 26);
            this.nUD_MaxPTMs.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_NeuCode_Hv);
            this.groupBox1.Controls.Add(this.btn_NeuCode_Lt);
            this.groupBox1.Controls.Add(this.btn_NaturalIsotopes);
            this.groupBox1.Location = new System.Drawing.Point(10, 258);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(322, 162);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Lysine Isotope Composition";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // btn_NeuCode_Hv
            // 
            this.btn_NeuCode_Hv.AutoSize = true;
            this.btn_NeuCode_Hv.Location = new System.Drawing.Point(16, 115);
            this.btn_NeuCode_Hv.Name = "btn_NeuCode_Hv";
            this.btn_NeuCode_Hv.Size = new System.Drawing.Size(149, 24);
            this.btn_NeuCode_Hv.TabIndex = 7;
            this.btn_NeuCode_Hv.TabStop = true;
            this.btn_NeuCode_Hv.Text = "NeuCode Heavy";
            this.btn_NeuCode_Hv.UseVisualStyleBackColor = true;
            // 
            // btn_NeuCode_Lt
            // 
            this.btn_NeuCode_Lt.AutoSize = true;
            this.btn_NeuCode_Lt.Location = new System.Drawing.Point(16, 75);
            this.btn_NeuCode_Lt.Name = "btn_NeuCode_Lt";
            this.btn_NeuCode_Lt.Size = new System.Drawing.Size(140, 24);
            this.btn_NeuCode_Lt.TabIndex = 6;
            this.btn_NeuCode_Lt.TabStop = true;
            this.btn_NeuCode_Lt.Text = "NeuCode Light";
            this.btn_NeuCode_Lt.UseVisualStyleBackColor = true;
            // 
            // btn_NaturalIsotopes
            // 
            this.btn_NaturalIsotopes.AutoSize = true;
            this.btn_NaturalIsotopes.Location = new System.Drawing.Point(16, 35);
            this.btn_NaturalIsotopes.Name = "btn_NaturalIsotopes";
            this.btn_NaturalIsotopes.Size = new System.Drawing.Size(229, 24);
            this.btn_NaturalIsotopes.TabIndex = 0;
            this.btn_NaturalIsotopes.TabStop = true;
            this.btn_NaturalIsotopes.Text = "Natural Isotope Abundance";
            this.btn_NaturalIsotopes.UseVisualStyleBackColor = true;
            // 
            // ckbx_Meth_Cleaved
            // 
            this.ckbx_Meth_Cleaved.AutoSize = true;
            this.ckbx_Meth_Cleaved.Location = new System.Drawing.Point(26, 228);
            this.ckbx_Meth_Cleaved.Name = "ckbx_Meth_Cleaved";
            this.ckbx_Meth_Cleaved.Size = new System.Drawing.Size(190, 24);
            this.ckbx_Meth_Cleaved.TabIndex = 4;
            this.ckbx_Meth_Cleaved.Text = "Cleaved N-Methionine";
            this.ckbx_Meth_Cleaved.UseVisualStyleBackColor = true;
            // 
            // ckbx_Carbam
            // 
            this.ckbx_Carbam.AutoSize = true;
            this.ckbx_Carbam.Location = new System.Drawing.Point(26, 188);
            this.ckbx_Carbam.Name = "ckbx_Carbam";
            this.ckbx_Carbam.Size = new System.Drawing.Size(193, 24);
            this.ckbx_Carbam.TabIndex = 3;
            this.ckbx_Carbam.Text = "Carbamidomethylation";
            this.ckbx_Carbam.UseVisualStyleBackColor = true;
            // 
            // ckbx_OxidMeth
            // 
            this.ckbx_OxidMeth.AutoSize = true;
            this.ckbx_OxidMeth.Location = new System.Drawing.Point(26, 149);
            this.ckbx_OxidMeth.Name = "ckbx_OxidMeth";
            this.ckbx_OxidMeth.Size = new System.Drawing.Size(177, 24);
            this.ckbx_OxidMeth.TabIndex = 2;
            this.ckbx_OxidMeth.Text = "Oxidized Methionine";
            this.ckbx_OxidMeth.UseVisualStyleBackColor = true;
            // 
            // tb_UniProtXML_Path
            // 
            this.tb_UniProtXML_Path.Location = new System.Drawing.Point(10, 48);
            this.tb_UniProtXML_Path.Name = "tb_UniProtXML_Path";
            this.tb_UniProtXML_Path.Size = new System.Drawing.Size(322, 26);
            this.tb_UniProtXML_Path.TabIndex = 1;
            // 
            // btn_GetUniProtXML
            // 
            this.btn_GetUniProtXML.Location = new System.Drawing.Point(10, 9);
            this.btn_GetUniProtXML.Name = "btn_GetUniProtXML";
            this.btn_GetUniProtXML.Size = new System.Drawing.Size(172, 31);
            this.btn_GetUniProtXML.TabIndex = 0;
            this.btn_GetUniProtXML.Text = "Get UniProt XML";
            this.btn_GetUniProtXML.UseVisualStyleBackColor = true;
            this.btn_GetUniProtXML.Click += new System.EventHandler(this.btn_GetUniProtXML_Click);
            // 
            // dgv_Database
            // 
            this.dgv_Database.AllowUserToAddRows = false;
            this.dgv_Database.AllowUserToDeleteRows = false;
            this.dgv_Database.AllowUserToOrderColumns = true;
            this.dgv_Database.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Database.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Database.Location = new System.Drawing.Point(0, 0);
            this.dgv_Database.Name = "dgv_Database";
            this.dgv_Database.ReadOnly = true;
            this.dgv_Database.RowTemplate.Height = 28;
            this.dgv_Database.Size = new System.Drawing.Size(781, 731);
            this.dgv_Database.TabIndex = 0;
            // 
            // ckbx_aggregateProteoforms
            // 
            this.ckbx_aggregateProteoforms.AutoSize = true;
            this.ckbx_aggregateProteoforms.Location = new System.Drawing.Point(22, 644);
            this.ckbx_aggregateProteoforms.Name = "ckbx_aggregateProteoforms";
            this.ckbx_aggregateProteoforms.Size = new System.Drawing.Size(302, 24);
            this.ckbx_aggregateProteoforms.TabIndex = 17;
            this.ckbx_aggregateProteoforms.Text = "Combine Sequence-Identical Proteins";
            this.ckbx_aggregateProteoforms.UseVisualStyleBackColor = true;
            this.ckbx_aggregateProteoforms.CheckedChanged += new System.EventHandler(this.ckbx_aggregateProteoforms_CheckedChanged);
            // 
            // TheoreticalDatabase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 735);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Name = "TheoreticalDatabase";
            this.Text = "TheoreticalDatabase";
            this.Load += new System.EventHandler(this.TheoreticalDatabase_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nUD_MinPeptideLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_NumDecoyDBs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUD_MaxPTMs)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Database)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox tb_UniProtXML_Path;
        private System.Windows.Forms.Button btn_GetUniProtXML;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbx_DisplayWhichDB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nUD_NumDecoyDBs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nUD_MaxPTMs;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton btn_NeuCode_Hv;
        private System.Windows.Forms.RadioButton btn_NeuCode_Lt;
        private System.Windows.Forms.RadioButton btn_NaturalIsotopes;
        private System.Windows.Forms.CheckBox ckbx_Meth_Cleaved;
        private System.Windows.Forms.CheckBox ckbx_Carbam;
        private System.Windows.Forms.CheckBox ckbx_OxidMeth;
        private System.Windows.Forms.TextBox tb_UniProtPtmList_Path;
        private System.Windows.Forms.Button btn_UniPtPtmList;
        private System.Windows.Forms.Button btn_Make_Databases;
        private System.Windows.Forms.DataGridView dgv_Database;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nUD_MinPeptideLength;
        private System.Windows.Forms.CheckBox ckbx_aggregateProteoforms;
    }
}