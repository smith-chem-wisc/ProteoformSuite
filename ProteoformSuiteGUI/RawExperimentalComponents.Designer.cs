using System;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    partial class RawExperimentalComponents
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RawExperimentalComponents));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.dgv_fileList = new System.Windows.Forms.DataGridView();
            this.bt_recalculate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rb_displayQuantificationComponents = new System.Windows.Forms.RadioButton();
            this.rb_displayIdentificationComponents = new System.Windows.Forms.RadioButton();
            this.dgv_rawComponents = new System.Windows.Forms.DataGridView();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.dgv_chargeStates = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_fileList)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_rawComponents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_chargeStates)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
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
            this.splitContainer1.Size = new System.Drawing.Size(1125, 579);
            this.splitContainer1.SplitterDistance = 217;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.dgv_fileList);
            this.splitContainer2.Panel1.Controls.Add(this.bt_recalculate);
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgv_rawComponents);
            this.splitContainer2.Size = new System.Drawing.Size(1121, 213);
            this.splitContainer2.SplitterDistance = 371;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 0;
            // 
            // dgv_fileList
            // 
            this.dgv_fileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv_fileList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_fileList.Location = new System.Drawing.Point(0, 3);
            this.dgv_fileList.Name = "dgv_fileList";
            this.dgv_fileList.Size = new System.Drawing.Size(187, 207);
            this.dgv_fileList.TabIndex = 2;
            // 
            // bt_recalculate
            // 
            this.bt_recalculate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bt_recalculate.Location = new System.Drawing.Point(191, 109);
            this.bt_recalculate.Name = "bt_recalculate";
            this.bt_recalculate.Size = new System.Drawing.Size(177, 104);
            this.bt_recalculate.TabIndex = 1;
            this.bt_recalculate.Text = "Read Raw Components and\r\nCollapse Deconvolution Artifacts";
            this.bt_recalculate.UseVisualStyleBackColor = true;
            this.bt_recalculate.Click += new System.EventHandler(this.bt_recalculate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.rb_displayQuantificationComponents);
            this.groupBox1.Controls.Add(this.rb_displayIdentificationComponents);
            this.groupBox1.Location = new System.Drawing.Point(193, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(175, 103);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Components Displayed";
            // 
            // rb_displayQuantificationComponents
            // 
            this.rb_displayQuantificationComponents.AutoSize = true;
            this.rb_displayQuantificationComponents.Location = new System.Drawing.Point(10, 56);
            this.rb_displayQuantificationComponents.Name = "rb_displayQuantificationComponents";
            this.rb_displayQuantificationComponents.Size = new System.Drawing.Size(152, 17);
            this.rb_displayQuantificationComponents.TabIndex = 1;
            this.rb_displayQuantificationComponents.Text = "Quantification Components";
            this.rb_displayQuantificationComponents.UseVisualStyleBackColor = true;
            // 
            // rb_displayIdentificationComponents
            // 
            this.rb_displayIdentificationComponents.AutoSize = true;
            this.rb_displayIdentificationComponents.Checked = true;
            this.rb_displayIdentificationComponents.Location = new System.Drawing.Point(10, 33);
            this.rb_displayIdentificationComponents.Name = "rb_displayIdentificationComponents";
            this.rb_displayIdentificationComponents.Size = new System.Drawing.Size(147, 17);
            this.rb_displayIdentificationComponents.TabIndex = 0;
            this.rb_displayIdentificationComponents.TabStop = true;
            this.rb_displayIdentificationComponents.Text = "Identification Components";
            this.rb_displayIdentificationComponents.UseVisualStyleBackColor = true;
            this.rb_displayIdentificationComponents.CheckedChanged += new System.EventHandler(this.rb_displayIdentificationComponents_CheckedChanged);
            // 
            // dgv_rawComponents
            // 
            this.dgv_rawComponents.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_rawComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_rawComponents.Location = new System.Drawing.Point(0, 0);
            this.dgv_rawComponents.Margin = new System.Windows.Forms.Padding(2);
            this.dgv_rawComponents.Name = "dgv_rawComponents";
            this.dgv_rawComponents.RowTemplate.Height = 28;
            this.dgv_rawComponents.Size = new System.Drawing.Size(747, 213);
            this.dgv_rawComponents.TabIndex = 0;
            this.dgv_rawComponents.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_RawQuantComp_MI_masses_CellContentClick);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.dgv_chargeStates);
            this.splitContainer3.Size = new System.Drawing.Size(1121, 355);
            this.splitContainer3.SplitterDistance = 371;
            this.splitContainer3.SplitterWidth = 3;
            this.splitContainer3.TabIndex = 0;
            // 
            // dgv_chargeStates
            // 
            this.dgv_chargeStates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_chargeStates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_chargeStates.Location = new System.Drawing.Point(0, 0);
            this.dgv_chargeStates.Margin = new System.Windows.Forms.Padding(2);
            this.dgv_chargeStates.Name = "dgv_chargeStates";
            this.dgv_chargeStates.RowTemplate.Height = 28;
            this.dgv_chargeStates.Size = new System.Drawing.Size(747, 355);
            this.dgv_chargeStates.TabIndex = 0;
            // 
            // RawExperimentalComponents
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(1125, 579);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "RawExperimentalComponents";
            this.Text = "Raw Experimental Components";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_fileList)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_rawComponents)).EndInit();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_chargeStates)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private DataGridView dgv_rawComponents;
        private SplitContainer splitContainer3;
        private DataGridView dgv_chargeStates;
        private Button bt_recalculate;
        private GroupBox groupBox1;
        private RadioButton rb_displayQuantificationComponents;
        private RadioButton rb_displayIdentificationComponents;
        private DataGridView dgv_fileList;
    }
}