namespace OpenEphys.Onix1.Design
{
    partial class ChannelConfigurationDialog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChannelConfigurationDialog));
            this.panel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.zedGraphChannels = new ZedGraph.ZedGraphControl();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonResetZoom = new System.Windows.Forms.Button();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dropDownOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.dropDownSaveFile = new System.Windows.Forms.ToolStripMenuItem();
            this.dropDownSaveFileAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.dropDownImportFile = new System.Windows.Forms.ToolStripMenuItem();
            this.dropDownLoadDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.panel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel
            // 
            this.panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel.Controls.Add(this.tableLayoutPanel1);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 22);
            this.panel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.panel.Name = "panel";
            this.panel.Padding = new System.Windows.Forms.Padding(1);
            this.panel.Size = new System.Drawing.Size(457, 445);
            this.panel.TabIndex = 7;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.zedGraphChannels, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 1);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(453, 441);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // zedGraphChannels
            // 
            this.zedGraphChannels.AutoSize = true;
            this.zedGraphChannels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphChannels.Location = new System.Drawing.Point(4, 5);
            this.zedGraphChannels.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.zedGraphChannels.Name = "zedGraphChannels";
            this.zedGraphChannels.ScrollGrace = 0D;
            this.zedGraphChannels.ScrollMaxX = 0D;
            this.zedGraphChannels.ScrollMaxY = 0D;
            this.zedGraphChannels.ScrollMaxY2 = 0D;
            this.zedGraphChannels.ScrollMinX = 0D;
            this.zedGraphChannels.ScrollMinY = 0D;
            this.zedGraphChannels.ScrollMinY2 = 0D;
            this.zedGraphChannels.Size = new System.Drawing.Size(445, 397);
            this.zedGraphChannels.TabIndex = 4;
            this.zedGraphChannels.TabStop = false;
            this.zedGraphChannels.UseExtendedPrintDialog = true;
            this.zedGraphChannels.ZoomEvent += new ZedGraph.ZedGraphControl.ZoomEventHandler(this.ZoomEvent);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.buttonResetZoom);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(2, 410);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(449, 28);
            this.flowLayoutPanel2.TabIndex = 5;
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Image = global::OpenEphys.Onix1.Design.Properties.Resources.MagnifyingGlass;
            this.buttonResetZoom.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonResetZoom.Location = new System.Drawing.Point(2, 3);
            this.buttonResetZoom.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.buttonResetZoom.Size = new System.Drawing.Size(110, 26);
            this.buttonResetZoom.TabIndex = 0;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.buttonResetZoom.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ButtonResetZoom_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.AutoSize = false;
            this.menuStrip.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip.ShowItemToolTips = true;
            this.menuStrip.Size = new System.Drawing.Size(457, 22);
            this.menuStrip.TabIndex = 5;
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dropDownOpenFile,
            this.toolStripSeparator1,
            this.dropDownSaveFile,
            this.dropDownSaveFileAs,
            this.toolStripSeparator2,
            this.dropDownImportFile,
            this.dropDownLoadDefault});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileMenuItem.Text = "File";
            // 
            // dropDownOpenFile
            // 
            this.dropDownOpenFile.Name = "dropDownOpenFile";
            this.dropDownOpenFile.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.dropDownOpenFile.Size = new System.Drawing.Size(218, 22);
            this.dropDownOpenFile.Text = "Open";
            this.dropDownOpenFile.ToolTipText = "Open a new file and update the ProbeInterface file name to the new file path.";
            this.dropDownOpenFile.Click += new System.EventHandler(this.MenuItemOpenFile);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(215, 6);
            // 
            // dropDownSaveFile
            // 
            this.dropDownSaveFile.Name = "dropDownSaveFile";
            this.dropDownSaveFile.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.dropDownSaveFile.Size = new System.Drawing.Size(218, 22);
            this.dropDownSaveFile.Text = "Save";
            this.dropDownSaveFile.ToolTipText = "Save the current electrode configuration to the current file path.";
            this.dropDownSaveFile.Click += new System.EventHandler(this.MenuItemSaveFile);
            // 
            // dropDownSaveFileAs
            // 
            this.dropDownSaveFileAs.Name = "dropDownSaveFileAs";
            this.dropDownSaveFileAs.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.dropDownSaveFileAs.Size = new System.Drawing.Size(218, 22);
            this.dropDownSaveFileAs.Text = "Save As...";
            this.dropDownSaveFileAs.ToolTipText = "Save the electrode configuration to a new file path.";
            this.dropDownSaveFileAs.Click += new System.EventHandler(this.MenuItemSaveFileAs);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(215, 6);
            // 
            // dropDownImportFile
            // 
            this.dropDownImportFile.Name = "dropDownImportFile";
            this.dropDownImportFile.Size = new System.Drawing.Size(218, 22);
            this.dropDownImportFile.Text = "Import Configuration";
            this.dropDownImportFile.ToolTipText = "Import the contents of another file without overwriting the file path.";
            this.dropDownImportFile.Click += new System.EventHandler(this.MenuItemImportFile);
            // 
            // dropDownLoadDefault
            // 
            this.dropDownLoadDefault.Name = "dropDownLoadDefault";
            this.dropDownLoadDefault.Size = new System.Drawing.Size(218, 22);
            this.dropDownLoadDefault.Text = "Load Default Configuration";
            this.dropDownLoadDefault.ToolTipText = "Load the default configuration without changing the file path.";
            this.dropDownLoadDefault.Click += new System.EventHandler(this.MenuItemLoadDefaultConfig);
            // 
            // ChannelConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 467);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "ChannelConfigurationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Channel Configuration";
            this.panel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        internal ZedGraph.ZedGraphControl zedGraphChannels;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem dropDownImportFile;
        private System.Windows.Forms.ToolStripMenuItem dropDownSaveFileAs;
        private System.Windows.Forms.ToolStripMenuItem dropDownLoadDefault;
        private System.Windows.Forms.Button buttonResetZoom;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.ToolStripMenuItem dropDownOpenFile;
        private System.Windows.Forms.ToolStripMenuItem dropDownSaveFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        internal System.Windows.Forms.Panel panel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
