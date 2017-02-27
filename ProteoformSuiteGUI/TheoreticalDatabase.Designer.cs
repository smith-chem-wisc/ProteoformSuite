namespace ProteoformSuite
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
            this.cmb_loadTable = new System.Windows.Forms.ComboBox();
            this.dgv_loadFiles = new System.Windows.Forms.DataGridView();
            this.btn_addFiles = new System.Windows.Forms.Button();
            this.btn_clearFiles = new System.Windows.Forms.Button();
            this.ckbx_combineTheoreticalsByMass = new System.Windows.Forms.CheckBox();
            this.tb_interest_label = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_proteins_of_interest_path = new System.Windows.Forms.TextBox();
            this.bt_proteins_of_interest = new System.Windows.Forms.Button();
            this.ckbx_combineIdenticalSequences = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.nUD_MinPeptideLength = new System.Windows.Forms.NumericUpDown();
            this.btn_Make_Databases = new System.Windows.Forms.Button();
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
            this.dgv_Database = new System.Windows.Forms.DataGridView();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_tableFilter = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_loadFiles)).BeginInit();
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
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.tb_tableFilter);
            this.splitContainer1.Panel1.Controls.Add(this.cmb_loadTable);
            this.splitContainer1.Panel1.Controls.Add(this.dgv_loadFiles);
            this.splitContainer1.Panel1.Controls.Add(this.btn_addFiles);
            this.splitContainer1.Panel1.Controls.Add(this.btn_clearFiles);
            this.splitContainer1.Panel1.Controls.Add(this.ckbx_combineTheoreticalsByMass);
            this.splitContainer1.Panel1.Controls.Add(this.tb_interest_label);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.tb_proteins_of_interest_path);
            this.splitContainer1.Panel1.Controls.Add(this.bt_proteins_of_interest);
            this.splitContainer1.Panel1.Controls.Add(this.ckbx_combineIdenticalSequences);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.nUD_MinPeptideLength);
            this.splitContainer1.Panel1.Controls.Add(this.btn_Make_Databases);
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
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dgv_Database);
            this.splitContainer1.Size = new System.Drawing.Size(1362, 741);
            this.splitContainer1.SplitterDistance = 452;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // cmb_loadTable
            // 
            this.cmb_loadTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_loadTable.FormattingEnabled = true;
            this.cmb_loadTable.Location = new System.Drawing.Point(11, 10);
            this.cmb_loadTable.Name = "cmb_loadTable";
            this.cmb_loadTable.Size = new System.Drawing.Size(429, 28);
            this.cmb_loadTable.TabIndex = 34;
            this.cmb_loadTable.SelectedIndexChanged += new System.EventHandler(this.cmb_loadTable_SelectedIndexChanged);
            // 
            // dgv_loadFiles
            // 
            this.dgv_loadFiles.AllowDrop = true;
            this.dgv_loadFiles.AllowUserToOrderColumns = true;
            this.dgv_loadFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_loadFiles.Location = new System.Drawing.Point(11, 44);
            this.dgv_loadFiles.Name = "dgv_loadFiles";
            this.dgv_loadFiles.Size = new System.Drawing.Size(430, 186);
            this.dgv_loadFiles.TabIndex = 25;
            this.dgv_loadFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgv_loadFiles_DragDrop);
            // 
            // btn_addFiles
            // 
            this.btn_addFiles.Location = new System.Drawing.Point(53, 236);
            this.btn_addFiles.Name = "btn_addFiles";
            this.btn_addFiles.Size = new System.Drawing.Size(122, 36);
            this.btn_addFiles.TabIndex = 24;
            this.btn_addFiles.Text = "Add";
            this.btn_addFiles.UseVisualStyleBackColor = true;
            this.btn_addFiles.Click += new System.EventHandler(this.btn_addFiles_Click);
            // 
            // btn_clearFiles
            // 
            this.btn_clearFiles.Location = new System.Drawing.Point(230, 236);
            this.btn_clearFiles.Name = "btn_clearFiles";
            this.btn_clearFiles.Size = new System.Drawing.Size(122, 36);
            this.btn_clearFiles.TabIndex = 23;
            this.btn_clearFiles.Text = "Clear";
            this.btn_clearFiles.UseVisualStyleBackColor = true;
            this.btn_clearFiles.Click += new System.EventHandler(this.btn_clearFiles_Click);
            // 
            // ckbx_combineTheoreticalsByMass
            // 
            this.ckbx_combineTheoreticalsByMass.AutoSize = true;
            this.ckbx_combineTheoreticalsByMass.Location = new System.Drawing.Point(7, 668);
            this.ckbx_combineTheoreticalsByMass.Margin = new System.Windows.Forms.Padding(2);
            this.ckbx_combineTheoreticalsByMass.Name = "ckbx_combineTheoreticalsByMass";
            this.ckbx_combineTheoreticalsByMass.Size = new System.Drawing.Size(225, 17);
            this.ckbx_combineTheoreticalsByMass.TabIndex = 22;
            this.ckbx_combineTheoreticalsByMass.Text = "Combine Theoretical Proteoforms By Mass";
            this.ckbx_combineTheoreticalsByMass.UseVisualStyleBackColor = true;
            this.ckbx_combineTheoreticalsByMass.CheckedChanged += new System.EventHandler(this.ckbx_combineTheoreticalsByMass_CheckedChanged);
            // 
            // tb_interest_label
            // 
            this.tb_interest_label.Enabled = false;
            this.tb_interest_label.Location = new System.Drawing.Point(239, 301);
            this.tb_interest_label.Margin = new System.Windows.Forms.Padding(2);
            this.tb_interest_label.Name = "tb_interest_label";
            this.tb_interest_label.Size = new System.Drawing.Size(202, 20);
            this.tb_interest_label.TabIndex = 21;
            this.tb_interest_label.Visible = false;
            this.tb_interest_label.TextChanged += new System.EventHandler(this.tb_interest_label_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(155, 281);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "(optional)";
            // 
            // tb_proteins_of_interest_path
            // 
            this.tb_proteins_of_interest_path.Enabled = false;
            this.tb_proteins_of_interest_path.Location = new System.Drawing.Point(7, 301);
            this.tb_proteins_of_interest_path.Margin = new System.Windows.Forms.Padding(2);
            this.tb_proteins_of_interest_path.Name = "tb_proteins_of_interest_path";
            this.tb_proteins_of_interest_path.Size = new System.Drawing.Size(228, 20);
            this.tb_proteins_of_interest_path.TabIndex = 19;
            // 
            // bt_proteins_of_interest
            // 
            this.bt_proteins_of_interest.Enabled = false;
            this.bt_proteins_of_interest.Location = new System.Drawing.Point(7, 277);
            this.bt_proteins_of_interest.Margin = new System.Windows.Forms.Padding(2);
            this.bt_proteins_of_interest.Name = "bt_proteins_of_interest";
            this.bt_proteins_of_interest.Size = new System.Drawing.Size(144, 20);
            this.bt_proteins_of_interest.TabIndex = 18;
            this.bt_proteins_of_interest.Text = "Get Proteins of Interest List";
            this.bt_proteins_of_interest.UseVisualStyleBackColor = true;
            this.bt_proteins_of_interest.Click += new System.EventHandler(this.bt_genes_of_interest_Click);
            // 
            // ckbx_combineIdenticalSequences
            // 
            this.ckbx_combineIdenticalSequences.AutoSize = true;
            this.ckbx_combineIdenticalSequences.Location = new System.Drawing.Point(7, 647);
            this.ckbx_combineIdenticalSequences.Margin = new System.Windows.Forms.Padding(2);
            this.ckbx_combineIdenticalSequences.Name = "ckbx_combineIdenticalSequences";
            this.ckbx_combineIdenticalSequences.Size = new System.Drawing.Size(203, 17);
            this.ckbx_combineIdenticalSequences.TabIndex = 17;
            this.ckbx_combineIdenticalSequences.Text = "Combine Sequence-Identical Proteins";
            this.ckbx_combineIdenticalSequences.UseVisualStyleBackColor = true;
            this.ckbx_combineIdenticalSequences.CheckedChanged += new System.EventHandler(this.ckbx_combineIdenticalSequences_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(62, 565);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Minimum Peptide Length";
            // 
            // nUD_MinPeptideLength
            // 
            this.nUD_MinPeptideLength.Location = new System.Drawing.Point(9, 562);
            this.nUD_MinPeptideLength.Margin = new System.Windows.Forms.Padding(2);
            this.nUD_MinPeptideLength.Name = "nUD_MinPeptideLength";
            this.nUD_MinPeptideLength.Size = new System.Drawing.Size(48, 20);
            this.nUD_MinPeptideLength.TabIndex = 15;
            this.nUD_MinPeptideLength.ValueChanged += new System.EventHandler(this.nUD_MinPeptideLength_ValueChanged);
            // 
            // btn_Make_Databases
            // 
            this.btn_Make_Databases.Location = new System.Drawing.Point(7, 694);
            this.btn_Make_Databases.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Make_Databases.Name = "btn_Make_Databases";
            this.btn_Make_Databases.Size = new System.Drawing.Size(207, 22);
            this.btn_Make_Databases.TabIndex = 14;
            this.btn_Make_Databases.Text = "Time to make the databases";
            this.btn_Make_Databases.UseVisualStyleBackColor = true;
            this.btn_Make_Databases.Click += new System.EventHandler(this.btn_Make_Databases_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 601);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Database Displayed";
            // 
            // cmbx_DisplayWhichDB
            // 
            this.cmbx_DisplayWhichDB.FormattingEnabled = true;
            this.cmbx_DisplayWhichDB.Location = new System.Drawing.Point(9, 616);
            this.cmbx_DisplayWhichDB.Margin = new System.Windows.Forms.Padding(2);
            this.cmbx_DisplayWhichDB.Name = "cmbx_DisplayWhichDB";
            this.cmbx_DisplayWhichDB.Size = new System.Drawing.Size(205, 21);
            this.cmbx_DisplayWhichDB.TabIndex = 10;
            this.cmbx_DisplayWhichDB.SelectedIndexChanged += new System.EventHandler(this.cmbx_DisplayWhichDB_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(62, 539);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Number of Decoy Databases";
            // 
            // nUD_NumDecoyDBs
            // 
            this.nUD_NumDecoyDBs.Location = new System.Drawing.Point(9, 536);
            this.nUD_NumDecoyDBs.Margin = new System.Windows.Forms.Padding(2);
            this.nUD_NumDecoyDBs.Name = "nUD_NumDecoyDBs";
            this.nUD_NumDecoyDBs.Size = new System.Drawing.Size(48, 20);
            this.nUD_NumDecoyDBs.TabIndex = 8;
            this.nUD_NumDecoyDBs.ValueChanged += new System.EventHandler(this.nUD_NumDecoyDBs_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(62, 515);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Max PTMs per Proteoform";
            // 
            // nUD_MaxPTMs
            // 
            this.nUD_MaxPTMs.Location = new System.Drawing.Point(9, 511);
            this.nUD_MaxPTMs.Margin = new System.Windows.Forms.Padding(2);
            this.nUD_MaxPTMs.Name = "nUD_MaxPTMs";
            this.nUD_MaxPTMs.Size = new System.Drawing.Size(48, 20);
            this.nUD_MaxPTMs.TabIndex = 6;
            this.nUD_MaxPTMs.ValueChanged += new System.EventHandler(this.nUD_MaxPTMs_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_NeuCode_Hv);
            this.groupBox1.Controls.Add(this.btn_NeuCode_Lt);
            this.groupBox1.Controls.Add(this.btn_NaturalIsotopes);
            this.groupBox1.Location = new System.Drawing.Point(-1, 396);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(215, 105);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Lysine Isotope Composition";
            // 
            // btn_NeuCode_Hv
            // 
            this.btn_NeuCode_Hv.AutoSize = true;
            this.btn_NeuCode_Hv.Location = new System.Drawing.Point(11, 75);
            this.btn_NeuCode_Hv.Margin = new System.Windows.Forms.Padding(2);
            this.btn_NeuCode_Hv.Name = "btn_NeuCode_Hv";
            this.btn_NeuCode_Hv.Size = new System.Drawing.Size(104, 17);
            this.btn_NeuCode_Hv.TabIndex = 7;
            this.btn_NeuCode_Hv.TabStop = true;
            this.btn_NeuCode_Hv.Text = "NeuCode Heavy";
            this.btn_NeuCode_Hv.UseVisualStyleBackColor = true;
            this.btn_NeuCode_Hv.CheckedChanged += new System.EventHandler(this.btn_NeuCode_Hv_CheckedChanged);
            // 
            // btn_NeuCode_Lt
            // 
            this.btn_NeuCode_Lt.AutoSize = true;
            this.btn_NeuCode_Lt.Location = new System.Drawing.Point(11, 49);
            this.btn_NeuCode_Lt.Margin = new System.Windows.Forms.Padding(2);
            this.btn_NeuCode_Lt.Name = "btn_NeuCode_Lt";
            this.btn_NeuCode_Lt.Size = new System.Drawing.Size(96, 17);
            this.btn_NeuCode_Lt.TabIndex = 6;
            this.btn_NeuCode_Lt.TabStop = true;
            this.btn_NeuCode_Lt.Text = "NeuCode Light";
            this.btn_NeuCode_Lt.UseVisualStyleBackColor = true;
            this.btn_NeuCode_Lt.CheckedChanged += new System.EventHandler(this.btn_NeuCode_Lt_CheckedChanged);
            // 
            // btn_NaturalIsotopes
            // 
            this.btn_NaturalIsotopes.AutoSize = true;
            this.btn_NaturalIsotopes.Location = new System.Drawing.Point(11, 23);
            this.btn_NaturalIsotopes.Margin = new System.Windows.Forms.Padding(2);
            this.btn_NaturalIsotopes.Name = "btn_NaturalIsotopes";
            this.btn_NaturalIsotopes.Size = new System.Drawing.Size(155, 17);
            this.btn_NaturalIsotopes.TabIndex = 0;
            this.btn_NaturalIsotopes.TabStop = true;
            this.btn_NaturalIsotopes.Text = "Natural Isotope Abundance";
            this.btn_NaturalIsotopes.UseVisualStyleBackColor = true;
            this.btn_NaturalIsotopes.CheckedChanged += new System.EventHandler(this.btn_NaturalIsotopes_CheckedChanged);
            // 
            // ckbx_Meth_Cleaved
            // 
            this.ckbx_Meth_Cleaved.AutoSize = true;
            this.ckbx_Meth_Cleaved.Location = new System.Drawing.Point(9, 376);
            this.ckbx_Meth_Cleaved.Margin = new System.Windows.Forms.Padding(2);
            this.ckbx_Meth_Cleaved.Name = "ckbx_Meth_Cleaved";
            this.ckbx_Meth_Cleaved.Size = new System.Drawing.Size(131, 17);
            this.ckbx_Meth_Cleaved.TabIndex = 4;
            this.ckbx_Meth_Cleaved.Text = "Cleaved N-Methionine";
            this.ckbx_Meth_Cleaved.UseVisualStyleBackColor = true;
            this.ckbx_Meth_Cleaved.CheckedChanged += new System.EventHandler(this.ckbx_Meth_Cleaved_CheckedChanged);
            // 
            // ckbx_Carbam
            // 
            this.ckbx_Carbam.AutoSize = true;
            this.ckbx_Carbam.Location = new System.Drawing.Point(9, 350);
            this.ckbx_Carbam.Margin = new System.Windows.Forms.Padding(2);
            this.ckbx_Carbam.Name = "ckbx_Carbam";
            this.ckbx_Carbam.Size = new System.Drawing.Size(129, 17);
            this.ckbx_Carbam.TabIndex = 3;
            this.ckbx_Carbam.Text = "Carbamidomethylation";
            this.ckbx_Carbam.UseVisualStyleBackColor = true;
            this.ckbx_Carbam.CheckedChanged += new System.EventHandler(this.ckbx_Carbam_CheckedChanged);
            // 
            // ckbx_OxidMeth
            // 
            this.ckbx_OxidMeth.AutoSize = true;
            this.ckbx_OxidMeth.Location = new System.Drawing.Point(9, 325);
            this.ckbx_OxidMeth.Margin = new System.Windows.Forms.Padding(2);
            this.ckbx_OxidMeth.Name = "ckbx_OxidMeth";
            this.ckbx_OxidMeth.Size = new System.Drawing.Size(121, 17);
            this.ckbx_OxidMeth.TabIndex = 2;
            this.ckbx_OxidMeth.Text = "Oxidized Methionine";
            this.ckbx_OxidMeth.UseVisualStyleBackColor = true;
            this.ckbx_OxidMeth.CheckedChanged += new System.EventHandler(this.ckbx_OxidMeth_CheckedChanged);
            // 
            // dgv_Database
            // 
            this.dgv_Database.AllowUserToAddRows = false;
            this.dgv_Database.AllowUserToDeleteRows = false;
            this.dgv_Database.AllowUserToOrderColumns = true;
            this.dgv_Database.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Database.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Database.Location = new System.Drawing.Point(0, 0);
            this.dgv_Database.Margin = new System.Windows.Forms.Padding(2);
            this.dgv_Database.Name = "dgv_Database";
            this.dgv_Database.ReadOnly = true;
            this.dgv_Database.RowTemplate.Height = 28;
            this.dgv_Database.Size = new System.Drawing.Size(903, 737);
            this.dgv_Database.TabIndex = 0;
            //
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(236, 600);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 13);
            this.label6.TabIndex = 48;
            this.label6.Text = "Table Filter";
            // 
            // tb_tableFilter
            // 
            this.tb_tableFilter.Location = new System.Drawing.Point(239, 616);
            this.tb_tableFilter.Name = "tb_tableFilter";
            this.tb_tableFilter.Size = new System.Drawing.Size(202, 20);
            this.tb_tableFilter.TabIndex = 47;
            this.tb_tableFilter.TextChanged += new System.EventHandler(this.tb_tableFilter_TextChanged);
            // 
            // TheoreticalDatabase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1362, 741);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "TheoreticalDatabase";
            this.Text = "TheoreticalDatabase";
            this.Load += new System.EventHandler(this.TheoreticalDatabase_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_loadFiles)).EndInit();
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
        private System.Windows.Forms.Button btn_Make_Databases;
        private System.Windows.Forms.DataGridView dgv_Database;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nUD_MinPeptideLength;
        private System.Windows.Forms.CheckBox ckbx_combineIdenticalSequences;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tb_proteins_of_interest_path;
        private System.Windows.Forms.Button bt_proteins_of_interest;
        private System.Windows.Forms.TextBox tb_interest_label;
        private System.Windows.Forms.CheckBox ckbx_combineTheoreticalsByMass;
        private System.Windows.Forms.DataGridView dgv_loadFiles;
        private System.Windows.Forms.Button btn_addFiles;
        private System.Windows.Forms.Button btn_clearFiles;
        private System.Windows.Forms.ComboBox cmb_loadTable;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_tableFilter;
    }
}