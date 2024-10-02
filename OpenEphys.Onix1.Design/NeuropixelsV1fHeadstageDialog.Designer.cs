namespace OpenEphys.Onix1.Design
{
    partial class NeuropixelsV1fHeadstageDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuropixelsV1fHeadstageDialog));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageNeuropixelsV1A = new System.Windows.Forms.TabPage();
            this.panelNeuropixelsV1A = new System.Windows.Forms.Panel();
            this.tabPageBno055 = new System.Windows.Forms.TabPage();
            this.panelBno055 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tabPageNeuropixelsV1B = new System.Windows.Forms.TabPage();
            this.panelNeuropixelsV1B = new System.Windows.Forms.Panel();
            this.tabControl1.SuspendLayout();
            this.tabPageNeuropixelsV1A.SuspendLayout();
            this.tabPageBno055.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tabPageNeuropixelsV1B.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageNeuropixelsV1A);
            this.tabControl1.Controls.Add(this.tabPageNeuropixelsV1B);
            this.tabControl1.Controls.Add(this.tabPageBno055);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 2);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1350, 732);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageNeuropixelsV1A
            // 
            this.tabPageNeuropixelsV1A.Controls.Add(this.panelNeuropixelsV1A);
            this.tabPageNeuropixelsV1A.Location = new System.Drawing.Point(4, 25);
            this.tabPageNeuropixelsV1A.Name = "tabPageNeuropixelsV1A";
            this.tabPageNeuropixelsV1A.Size = new System.Drawing.Size(1342, 703);
            this.tabPageNeuropixelsV1A.TabIndex = 0;
            this.tabPageNeuropixelsV1A.Text = "NeuropixelsV1A";
            this.tabPageNeuropixelsV1A.UseVisualStyleBackColor = true;
            // 
            // panelNeuropixelsV1A
            // 
            this.panelNeuropixelsV1A.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelNeuropixelsV1A.Location = new System.Drawing.Point(0, 0);
            this.panelNeuropixelsV1A.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelNeuropixelsV1A.Name = "panelNeuropixelsV1A";
            this.panelNeuropixelsV1A.Size = new System.Drawing.Size(1342, 703);
            this.panelNeuropixelsV1A.TabIndex = 0;
            // 
            // tabPageBno055
            // 
            this.tabPageBno055.Controls.Add(this.panelBno055);
            this.tabPageBno055.Location = new System.Drawing.Point(4, 25);
            this.tabPageBno055.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageBno055.Name = "tabPageBno055";
            this.tabPageBno055.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageBno055.Size = new System.Drawing.Size(1342, 703);
            this.tabPageBno055.TabIndex = 1;
            this.tabPageBno055.Text = "Bno055";
            this.tabPageBno055.UseVisualStyleBackColor = true;
            // 
            // panelBno055
            // 
            this.panelBno055.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBno055.Location = new System.Drawing.Point(3, 2);
            this.panelBno055.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelBno055.Name = "panelBno055";
            this.panelBno055.Size = new System.Drawing.Size(1336, 699);
            this.panelBno055.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(1238, 2);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(111, 34);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(1121, 2);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(111, 34);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.Okay_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(1356, 26);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 26);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1356, 778);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel1.Controls.Add(this.buttonOK);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 738);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1352, 38);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // tabPageNeuropixelsV1B
            // 
            this.tabPageNeuropixelsV1B.Controls.Add(this.panelNeuropixelsV1B);
            this.tabPageNeuropixelsV1B.Location = new System.Drawing.Point(4, 25);
            this.tabPageNeuropixelsV1B.Name = "tabPageNeuropixelsV1B";
            this.tabPageNeuropixelsV1B.Size = new System.Drawing.Size(1342, 703);
            this.tabPageNeuropixelsV1B.TabIndex = 2;
            this.tabPageNeuropixelsV1B.Text = "NeuropixelsV1B";
            this.tabPageNeuropixelsV1B.UseVisualStyleBackColor = true;
            // 
            // panelNeuropixelsV1B
            // 
            this.panelNeuropixelsV1B.AutoSize = true;
            this.panelNeuropixelsV1B.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelNeuropixelsV1B.Location = new System.Drawing.Point(0, 0);
            this.panelNeuropixelsV1B.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelNeuropixelsV1B.Name = "panelNeuropixelsV1B";
            this.panelNeuropixelsV1B.Size = new System.Drawing.Size(1342, 703);
            this.panelNeuropixelsV1B.TabIndex = 1;
            // 
            // NeuropixelsV1fHeadstageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1356, 804);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "NeuropixelsV1fHeadstageDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NeuropixelsV1f Headstage Configuration";
            this.tabControl1.ResumeLayout(false);
            this.tabPageNeuropixelsV1A.ResumeLayout(false);
            this.tabPageBno055.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tabPageNeuropixelsV1B.ResumeLayout(false);
            this.tabPageNeuropixelsV1B.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageNeuropixelsV1A;
        private System.Windows.Forms.TabPage tabPageBno055;
        private System.Windows.Forms.Panel panelNeuropixelsV1A;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Panel panelBno055;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TabPage tabPageNeuropixelsV1B;
        private System.Windows.Forms.Panel panelNeuropixelsV1B;
    }
}
