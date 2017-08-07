﻿namespace ProteoformSuiteGUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProteoformFamilies));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dgv_main = new System.Windows.Forms.DataGridView();
            this.label11 = new System.Windows.Forms.Label();
            this.cmbx_nodeLabel = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbx_geneLabel = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cmbx_edgeLabel = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tb_tableFilter = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbx_nodeLabelPositioning = new System.Windows.Forms.ComboBox();
            this.cmbx_nodeLayout = new System.Windows.Forms.ComboBox();
            this.cmbx_colorScheme = new System.Windows.Forms.ComboBox();
            this.lb_dgv_selection = new System.Windows.Forms.Label();
            this.cmbx_tableSelector = new System.Windows.Forms.ComboBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.dgv_proteoform_family_members = new System.Windows.Forms.DataGridView();
            this.cb_include_td_nodes = new System.Windows.Forms.CheckBox();
            this.rtb_proteoformFamilyResults = new System.Windows.Forms.RichTextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cb_count_adducts_as_id = new System.Windows.Forms.CheckBox();
            this.tb_likelyCleavages = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.cb_geneCentric = new System.Windows.Forms.CheckBox();
            this.btn_inclusion_list_all_families = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cb_orphans = new System.Windows.Forms.CheckBox();
            this.cb_unidentified_families = new System.Windows.Forms.CheckBox();
            this.cb_identified_families = new System.Windows.Forms.CheckBox();
            this.btn_inclusion_list_selected_families = new System.Windows.Forms.Button();
            this.cb_buildAsQuantitative = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.cb_boldLabel = new System.Windows.Forms.CheckBox();
            this.cb_redBorder = new System.Windows.Forms.CheckBox();
            this.nud_decimalRoundingLabels = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.lb_timeStamp = new System.Windows.Forms.Label();
            this.tb_recentTimeStamp = new System.Windows.Forms.TextBox();
            this.btn_buildSelectedFamilies = new System.Windows.Forms.Button();
            this.btn_buildAllFamilies = new System.Windows.Forms.Button();
            this.label_tempFileFolder = new System.Windows.Forms.Label();
            this.tb_familyBuildFolder = new System.Windows.Forms.TextBox();
            this.btn_browseTempFolder = new System.Windows.Forms.Button();
            this.Families_update = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_main)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_proteoform_family_members)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
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
            this.splitContainer1.Size = new System.Drawing.Size(1362, 741);
            this.splitContainer1.SplitterDistance = 307;
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
            this.splitContainer2.Panel1.Controls.Add(this.dgv_main);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.label11);
            this.splitContainer2.Panel2.Controls.Add(this.cmbx_nodeLabel);
            this.splitContainer2.Panel2.Controls.Add(this.label10);
            this.splitContainer2.Panel2.Controls.Add(this.cmbx_geneLabel);
            this.splitContainer2.Panel2.Controls.Add(this.label9);
            this.splitContainer2.Panel2.Controls.Add(this.cmbx_edgeLabel);
            this.splitContainer2.Panel2.Controls.Add(this.label6);
            this.splitContainer2.Panel2.Controls.Add(this.tb_tableFilter);
            this.splitContainer2.Panel2.Controls.Add(this.label4);
            this.splitContainer2.Panel2.Controls.Add(this.label3);
            this.splitContainer2.Panel2.Controls.Add(this.label2);
            this.splitContainer2.Panel2.Controls.Add(this.cmbx_nodeLabelPositioning);
            this.splitContainer2.Panel2.Controls.Add(this.cmbx_nodeLayout);
            this.splitContainer2.Panel2.Controls.Add(this.cmbx_colorScheme);
            this.splitContainer2.Panel2.Controls.Add(this.lb_dgv_selection);
            this.splitContainer2.Panel2.Controls.Add(this.cmbx_tableSelector);
            this.splitContainer2.Size = new System.Drawing.Size(1362, 307);
            this.splitContainer2.SplitterDistance = 787;
            this.splitContainer2.TabIndex = 5;
            // 
            // dgv_main
            // 
            this.dgv_main.AllowUserToAddRows = false;
            this.dgv_main.AllowUserToDeleteRows = false;
            this.dgv_main.AllowUserToOrderColumns = true;
            this.dgv_main.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_main.Location = new System.Drawing.Point(0, 0);
            this.dgv_main.Name = "dgv_main";
            this.dgv_main.Size = new System.Drawing.Size(783, 303);
            this.dgv_main.TabIndex = 2;
            this.dgv_main.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgv_proteoform_families_CellMouseClick);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.SystemColors.Control;
            this.label11.Location = new System.Drawing.Point(4, 168);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(117, 13);
            this.label11.TabIndex = 52;
            this.label11.Text = "Node Label Information";
            // 
            // cmbx_nodeLabel
            // 
            this.cmbx_nodeLabel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_nodeLabel.FormattingEnabled = true;
            this.cmbx_nodeLabel.Location = new System.Drawing.Point(120, 165);
            this.cmbx_nodeLabel.Name = "cmbx_nodeLabel";
            this.cmbx_nodeLabel.Size = new System.Drawing.Size(350, 21);
            this.cmbx_nodeLabel.TabIndex = 51;
            this.cmbx_nodeLabel.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.SystemColors.Control;
            this.label10.Location = new System.Drawing.Point(4, 248);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(105, 13);
            this.label10.TabIndex = 50;
            this.label10.Text = "Prefered Gene Label";
            // 
            // cmbx_geneLabel
            // 
            this.cmbx_geneLabel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_geneLabel.FormattingEnabled = true;
            this.cmbx_geneLabel.Location = new System.Drawing.Point(119, 244);
            this.cmbx_geneLabel.Name = "cmbx_geneLabel";
            this.cmbx_geneLabel.Size = new System.Drawing.Size(350, 21);
            this.cmbx_geneLabel.TabIndex = 49;
            this.cmbx_geneLabel.SelectedIndexChanged += new System.EventHandler(this.cmbx_geneLabel_SelectedIndexChanged);
            this.cmbx_geneLabel.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.SystemColors.Control;
            this.label9.Location = new System.Drawing.Point(4, 196);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(116, 13);
            this.label9.TabIndex = 48;
            this.label9.Text = "Edge Label Information";
            // 
            // cmbx_edgeLabel
            // 
            this.cmbx_edgeLabel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_edgeLabel.FormattingEnabled = true;
            this.cmbx_edgeLabel.Location = new System.Drawing.Point(120, 193);
            this.cmbx_edgeLabel.Name = "cmbx_edgeLabel";
            this.cmbx_edgeLabel.Size = new System.Drawing.Size(350, 21);
            this.cmbx_edgeLabel.TabIndex = 47;
            this.cmbx_edgeLabel.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 41);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 13);
            this.label6.TabIndex = 46;
            this.label6.Text = "Table Filter";
            // 
            // tb_tableFilter
            // 
            this.tb_tableFilter.Location = new System.Drawing.Point(120, 38);
            this.tb_tableFilter.Name = "tb_tableFilter";
            this.tb_tableFilter.Size = new System.Drawing.Size(350, 20);
            this.tb_tableFilter.TabIndex = 45;
            this.tb_tableFilter.TextChanged += new System.EventHandler(this.tb_tableFilter_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(4, 141);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 13);
            this.label4.TabIndex = 41;
            this.label4.Text = "Node Label Positioning";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(4, 114);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 40;
            this.label3.Text = "Node Layout";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 87);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 13);
            this.label2.TabIndex = 39;
            this.label2.Text = "Node Color Scheme";
            // 
            // cmbx_nodeLabelPositioning
            // 
            this.cmbx_nodeLabelPositioning.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_nodeLabelPositioning.FormattingEnabled = true;
            this.cmbx_nodeLabelPositioning.Location = new System.Drawing.Point(120, 138);
            this.cmbx_nodeLabelPositioning.Name = "cmbx_nodeLabelPositioning";
            this.cmbx_nodeLabelPositioning.Size = new System.Drawing.Size(350, 21);
            this.cmbx_nodeLabelPositioning.TabIndex = 38;
            this.cmbx_nodeLabelPositioning.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // cmbx_nodeLayout
            // 
            this.cmbx_nodeLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_nodeLayout.FormattingEnabled = true;
            this.cmbx_nodeLayout.Location = new System.Drawing.Point(120, 111);
            this.cmbx_nodeLayout.Name = "cmbx_nodeLayout";
            this.cmbx_nodeLayout.Size = new System.Drawing.Size(350, 21);
            this.cmbx_nodeLayout.TabIndex = 37;
            this.cmbx_nodeLayout.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // cmbx_colorScheme
            // 
            this.cmbx_colorScheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_colorScheme.FormattingEnabled = true;
            this.cmbx_colorScheme.Location = new System.Drawing.Point(120, 84);
            this.cmbx_colorScheme.Name = "cmbx_colorScheme";
            this.cmbx_colorScheme.Size = new System.Drawing.Size(350, 21);
            this.cmbx_colorScheme.TabIndex = 36;
            this.cmbx_colorScheme.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
            // 
            // lb_dgv_selection
            // 
            this.lb_dgv_selection.AutoSize = true;
            this.lb_dgv_selection.Location = new System.Drawing.Point(4, 13);
            this.lb_dgv_selection.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lb_dgv_selection.Name = "lb_dgv_selection";
            this.lb_dgv_selection.Size = new System.Drawing.Size(81, 13);
            this.lb_dgv_selection.TabIndex = 35;
            this.lb_dgv_selection.Text = "Table Selection";
            // 
            // cmbx_tableSelector
            // 
            this.cmbx_tableSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbx_tableSelector.FormattingEnabled = true;
            this.cmbx_tableSelector.Location = new System.Drawing.Point(120, 10);
            this.cmbx_tableSelector.Name = "cmbx_tableSelector";
            this.cmbx_tableSelector.Size = new System.Drawing.Size(350, 21);
            this.cmbx_tableSelector.TabIndex = 5;
            this.cmbx_tableSelector.SelectedIndexChanged += new System.EventHandler(this.cmbx_tableSelector_SelectedIndexChanged);
            this.cmbx_tableSelector.TextChanged += new System.EventHandler(this.cmbx_empty_TextChanged);
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
            this.splitContainer3.Panel2.Controls.Add(this.cb_include_td_nodes);
            this.splitContainer3.Panel2.AutoScroll = true;
            this.splitContainer3.Panel2.Controls.Add(this.rtb_proteoformFamilyResults);
            this.splitContainer3.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer3.Panel2.Controls.Add(this.cb_geneCentric);
            this.splitContainer3.Panel2.Controls.Add(this.btn_inclusion_list_all_families);
            this.splitContainer3.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer3.Panel2.Controls.Add(this.btn_inclusion_list_selected_families);
            this.splitContainer3.Panel2.Controls.Add(this.cb_buildAsQuantitative);
            this.splitContainer3.Panel2.Controls.Add(this.groupBox5);
            this.splitContainer3.Panel2.Controls.Add(this.nud_decimalRoundingLabels);
            this.splitContainer3.Panel2.Controls.Add(this.label1);
            this.splitContainer3.Panel2.Controls.Add(this.lb_timeStamp);
            this.splitContainer3.Panel2.Controls.Add(this.tb_recentTimeStamp);
            this.splitContainer3.Panel2.Controls.Add(this.btn_buildSelectedFamilies);
            this.splitContainer3.Panel2.Controls.Add(this.btn_buildAllFamilies);
            this.splitContainer3.Panel2.Controls.Add(this.label_tempFileFolder);
            this.splitContainer3.Panel2.Controls.Add(this.tb_familyBuildFolder);
            this.splitContainer3.Panel2.Controls.Add(this.btn_browseTempFolder);
            this.splitContainer3.Panel2.Controls.Add(this.Families_update);
            this.splitContainer3.Size = new System.Drawing.Size(1358, 426);
            this.splitContainer3.SplitterDistance = 776;
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
            this.dgv_proteoform_family_members.Size = new System.Drawing.Size(776, 426);
            this.dgv_proteoform_family_members.TabIndex = 3;
            // 
            // cb_include_td_nodes
            // 
            this.cb_include_td_nodes.AutoSize = true;
            this.cb_include_td_nodes.Checked = true;
            this.cb_include_td_nodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_include_td_nodes.Location = new System.Drawing.Point(301, 133);
            this.cb_include_td_nodes.Name = "cb_include_td_nodes";
            this.cb_include_td_nodes.Size = new System.Drawing.Size(168, 17);
            this.cb_include_td_nodes.TabIndex = 66;
            this.cb_include_td_nodes.Text = "Include Topdown Proteoforms";
            this.cb_include_td_nodes.UseVisualStyleBackColor = true;
            this.cb_include_td_nodes.CheckedChanged += new System.EventHandler(this.cb_include_td_nodes_CheckedChanged);
            // 
            // rtb_proteoformFamilyResults
            // 
            this.rtb_proteoformFamilyResults.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.rtb_proteoformFamilyResults.Location = new System.Drawing.Point(-3, 303);
            this.rtb_proteoformFamilyResults.Name = "rtb_proteoformFamilyResults";
            this.rtb_proteoformFamilyResults.ReadOnly = true;
            this.rtb_proteoformFamilyResults.Size = new System.Drawing.Size(463, 84);
            this.rtb_proteoformFamilyResults.TabIndex = 63;
            this.rtb_proteoformFamilyResults.Text = "";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cb_count_adducts_as_id);
            this.groupBox2.Controls.Add(this.tb_likelyCleavages);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Location = new System.Drawing.Point(35, 320);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(239, 73);
            this.groupBox2.TabIndex = 62;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Proteoform Identification Options";
            // 
            // cb_count_adducts_as_id
            // 
            this.cb_count_adducts_as_id.AutoSize = true;
            this.cb_count_adducts_as_id.Location = new System.Drawing.Point(7, 46);
            this.cb_count_adducts_as_id.Name = "cb_count_adducts_as_id";
            this.cb_count_adducts_as_id.Size = new System.Drawing.Size(179, 17);
            this.cb_count_adducts_as_id.TabIndex = 59;
            this.cb_count_adducts_as_id.Text = "Count Adducts As Identifications";
            this.cb_count_adducts_as_id.UseVisualStyleBackColor = true;
            this.cb_count_adducts_as_id.CheckedChanged += new System.EventHandler(this.cb_count_adducts_as_id_CheckedChanged);
            // 
            // tb_likelyCleavages
            // 
            this.tb_likelyCleavages.Location = new System.Drawing.Point(7, 20);
            this.tb_likelyCleavages.Name = "tb_likelyCleavages";
            this.tb_likelyCleavages.Size = new System.Drawing.Size(112, 20);
            this.tb_likelyCleavages.TabIndex = 1;
            this.tb_likelyCleavages.TextChanged += new System.EventHandler(this.tb_likelyCleavages_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(125, 23);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(104, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Likely AA Cleavages";
            // 
            // cb_geneCentric
            // 
            this.cb_geneCentric.AutoSize = true;
            this.cb_geneCentric.Checked = true;
            this.cb_geneCentric.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_geneCentric.Location = new System.Drawing.Point(301, 87);
            this.cb_geneCentric.Name = "cb_geneCentric";
            this.cb_geneCentric.Size = new System.Drawing.Size(154, 17);
            this.cb_geneCentric.TabIndex = 61;
            this.cb_geneCentric.Text = "Build Gene-Centric Families";
            this.cb_geneCentric.UseVisualStyleBackColor = true;
            this.cb_geneCentric.CheckedChanged += new System.EventHandler(this.cb_geneCentric_CheckedChanged);
            // 
            // btn_inclusion_list_all_families
            // 
            this.btn_inclusion_list_all_families.Location = new System.Drawing.Point(75, 261);
            this.btn_inclusion_list_all_families.Name = "btn_inclusion_list_all_families";
            this.btn_inclusion_list_all_families.Size = new System.Drawing.Size(195, 23);
            this.btn_inclusion_list_all_families.TabIndex = 60;
            this.btn_inclusion_list_all_families.Text = "Export Inclusion List";
            this.btn_inclusion_list_all_families.UseVisualStyleBackColor = true;
            this.btn_inclusion_list_all_families.Click += new System.EventHandler(this.btn_inclusion_list_all_families_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cb_orphans);
            this.groupBox1.Controls.Add(this.cb_unidentified_families);
            this.groupBox1.Controls.Add(this.cb_identified_families);
            this.groupBox1.Location = new System.Drawing.Point(280, 237);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 96);
            this.groupBox1.TabIndex = 59;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Inclusion List";
            // 
            // cb_orphans
            // 
            this.cb_orphans.AutoSize = true;
            this.cb_orphans.Checked = true;
            this.cb_orphans.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_orphans.Location = new System.Drawing.Point(21, 76);
            this.cb_orphans.Name = "cb_orphans";
            this.cb_orphans.Size = new System.Drawing.Size(66, 17);
            this.cb_orphans.TabIndex = 58;
            this.cb_orphans.Text = "Orphans";
            this.cb_orphans.UseVisualStyleBackColor = true;
            // 
            // cb_unidentified_families
            // 
            this.cb_unidentified_families.AutoSize = true;
            this.cb_unidentified_families.Checked = true;
            this.cb_unidentified_families.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_unidentified_families.Location = new System.Drawing.Point(21, 53);
            this.cb_unidentified_families.Name = "cb_unidentified_families";
            this.cb_unidentified_families.Size = new System.Drawing.Size(141, 17);
            this.cb_unidentified_families.TabIndex = 57;
            this.cb_unidentified_families.Text = "Unidentified Proteoforms";
            this.cb_unidentified_families.UseVisualStyleBackColor = true;
            // 
            // cb_identified_families
            // 
            this.cb_identified_families.AutoSize = true;
            this.cb_identified_families.Checked = true;
            this.cb_identified_families.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_identified_families.Location = new System.Drawing.Point(21, 30);
            this.cb_identified_families.Name = "cb_identified_families";
            this.cb_identified_families.Size = new System.Drawing.Size(128, 17);
            this.cb_identified_families.TabIndex = 56;
            this.cb_identified_families.Text = "Identified Proteoforms";
            this.cb_identified_families.UseVisualStyleBackColor = true;
            // 
            // btn_inclusion_list_selected_families
            // 
            this.btn_inclusion_list_selected_families.Location = new System.Drawing.Point(75, 291);
            this.btn_inclusion_list_selected_families.Name = "btn_inclusion_list_selected_families";
            this.btn_inclusion_list_selected_families.Size = new System.Drawing.Size(195, 23);
            this.btn_inclusion_list_selected_families.TabIndex = 59;
            this.btn_inclusion_list_selected_families.Text = "Export Selected Families Inclusion List";
            this.btn_inclusion_list_selected_families.UseVisualStyleBackColor = true;
            this.btn_inclusion_list_selected_families.Click += new System.EventHandler(this.btn_inclusion_list_selected_families_Click);
            // 
            // cb_buildAsQuantitative
            // 
            this.cb_buildAsQuantitative.AutoSize = true;
            this.cb_buildAsQuantitative.Location = new System.Drawing.Point(301, 110);
            this.cb_buildAsQuantitative.Name = "cb_buildAsQuantitative";
            this.cb_buildAsQuantitative.Size = new System.Drawing.Size(163, 17);
            this.cb_buildAsQuantitative.TabIndex = 58;
            this.cb_buildAsQuantitative.Text = "Build as Quantitative Families";
            this.cb_buildAsQuantitative.UseVisualStyleBackColor = true;
            this.cb_buildAsQuantitative.CheckedChanged += new System.EventHandler(this.cb_buildAsQuantitative_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.cb_boldLabel);
            this.groupBox5.Controls.Add(this.cb_redBorder);
            this.groupBox5.Location = new System.Drawing.Point(280, 151);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(200, 84);
            this.groupBox5.TabIndex = 57;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Highlights for Significant Differences";
            // 
            // cb_boldLabel
            // 
            this.cb_boldLabel.AutoSize = true;
            this.cb_boldLabel.Checked = true;
            this.cb_boldLabel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_boldLabel.Enabled = false;
            this.cb_boldLabel.Location = new System.Drawing.Point(21, 53);
            this.cb_boldLabel.Name = "cb_boldLabel";
            this.cb_boldLabel.Size = new System.Drawing.Size(76, 17);
            this.cb_boldLabel.TabIndex = 57;
            this.cb_boldLabel.Text = "Bold Label";
            this.cb_boldLabel.UseVisualStyleBackColor = true;
            // 
            // cb_redBorder
            // 
            this.cb_redBorder.AutoSize = true;
            this.cb_redBorder.Checked = true;
            this.cb_redBorder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_redBorder.Enabled = false;
            this.cb_redBorder.Location = new System.Drawing.Point(21, 30);
            this.cb_redBorder.Name = "cb_redBorder";
            this.cb_redBorder.Size = new System.Drawing.Size(109, 17);
            this.cb_redBorder.TabIndex = 56;
            this.cb_redBorder.Text = "Red Node Border";
            this.cb_redBorder.UseVisualStyleBackColor = true;
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
            this.btn_buildSelectedFamilies.Location = new System.Drawing.Point(73, 139);
            this.btn_buildSelectedFamilies.Name = "btn_buildSelectedFamilies";
            this.btn_buildSelectedFamilies.Size = new System.Drawing.Size(195, 23);
            this.btn_buildSelectedFamilies.TabIndex = 7;
            this.btn_buildSelectedFamilies.Text = "Build Selected Families in Cytoscape";
            this.btn_buildSelectedFamilies.UseVisualStyleBackColor = true;
            this.btn_buildSelectedFamilies.Click += new System.EventHandler(this.btn_buildSelectedFamilies_Click);
            // 
            // btn_buildAllFamilies
            // 
            this.btn_buildAllFamilies.Location = new System.Drawing.Point(99, 110);
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
            // Families_update
            // 
            this.Families_update.AllowDrop = true;
            this.Families_update.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.Families_update.Location = new System.Drawing.Point(90, 393);
            this.Families_update.Name = "Families_update";
            this.Families_update.Size = new System.Drawing.Size(402, 23);
            this.Families_update.TabIndex = 33;
            this.Families_update.Text = "Construct Families and Identify Proteoforms";
            this.Families_update.UseMnemonic = false;
            this.Families_update.UseVisualStyleBackColor = true;
            this.Families_update.Click += new System.EventHandler(this.Families_update_Click);
            // 
            // ProteoformFamilies
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1362, 741);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProteoformFamilies";
            this.Text = "Proteoform Families";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_main)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_proteoform_family_members)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_decimalRoundingLabels)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgv_main;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView dgv_proteoform_family_members;
        private System.Windows.Forms.Button btn_buildSelectedFamilies;
        private System.Windows.Forms.Button btn_buildAllFamilies;
        private System.Windows.Forms.Label label_tempFileFolder;
        private System.Windows.Forms.TextBox tb_familyBuildFolder;
        private System.Windows.Forms.Button btn_browseTempFolder;
        private System.Windows.Forms.Button Families_update;
        private System.Windows.Forms.Label lb_timeStamp;
        private System.Windows.Forms.TextBox tb_recentTimeStamp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nud_decimalRoundingLabels;
        private System.Windows.Forms.ComboBox cmbx_tableSelector;
        private System.Windows.Forms.Label lb_dgv_selection;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tb_tableFilter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbx_nodeLabelPositioning;
        private System.Windows.Forms.ComboBox cmbx_nodeLayout;
        private System.Windows.Forms.ComboBox cmbx_colorScheme;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmbx_edgeLabel;
        private System.Windows.Forms.CheckBox cb_buildAsQuantitative;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox cb_boldLabel;
        private System.Windows.Forms.CheckBox cb_redBorder;
        private System.Windows.Forms.Button btn_inclusion_list_all_families;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cb_orphans;
        private System.Windows.Forms.CheckBox cb_unidentified_families;
        private System.Windows.Forms.CheckBox cb_identified_families;
        private System.Windows.Forms.Button btn_inclusion_list_selected_families;
        public System.Windows.Forms.CheckBox cb_geneCentric;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.ComboBox cmbx_geneLabel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cmbx_nodeLabel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tb_likelyCleavages;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.RichTextBox rtb_proteoformFamilyResults;
        private System.Windows.Forms.CheckBox cb_count_adducts_as_id;
        public System.Windows.Forms.CheckBox cb_include_td_nodes;
    }
}