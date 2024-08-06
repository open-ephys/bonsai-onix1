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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.zedGraphChannels = new ZedGraph.ZedGraphControl();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dropDownOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.dropDownSaveFile = new System.Windows.Forms.ToolStripMenuItem();
            this.dropDownLoadDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 36);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.zedGraphChannels);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.buttonCancel);
            this.splitContainer1.Panel2.Controls.Add(this.buttonOK);
            this.splitContainer1.Size = new System.Drawing.Size(686, 681);
            this.splitContainer1.SplitterDistance = 585;
            this.splitContainer1.TabIndex = 0;
            // 
            // zedGraphChannels
            // 
            this.zedGraphChannels.AutoSize = true;
            this.zedGraphChannels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphChannels.Location = new System.Drawing.Point(0, 0);
            this.zedGraphChannels.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.zedGraphChannels.Name = "zedGraphChannels";
            this.zedGraphChannels.ScrollGrace = 0D;
            this.zedGraphChannels.ScrollMaxX = 0D;
            this.zedGraphChannels.ScrollMaxY = 0D;
            this.zedGraphChannels.ScrollMaxY2 = 0D;
            this.zedGraphChannels.ScrollMinX = 0D;
            this.zedGraphChannels.ScrollMinY = 0D;
            this.zedGraphChannels.ScrollMinY2 = 0D;
            this.zedGraphChannels.Size = new System.Drawing.Size(686, 585);
            this.zedGraphChannels.TabIndex = 4;
            this.zedGraphChannels.UseExtendedPrintDialog = true;
            this.zedGraphChannels.ZoomEvent += new ZedGraph.ZedGraphControl.ZoomEventHandler(this.ZoomEvent);
            // 
            // menuStrip
            // 
            this.menuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(686, 36);
            this.menuStrip.TabIndex = 5;
            this.menuStrip.Text = "menuStripChannelConfiguration";
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dropDownOpenFile,
            this.dropDownSaveFile,
            this.dropDownLoadDefault});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(54, 30);
            this.fileMenuItem.Text = "File";
            // 
            // dropDownOpenFile
            // 
            this.dropDownOpenFile.Name = "dropDownOpenFile";
            this.dropDownOpenFile.Size = new System.Drawing.Size(215, 34);
            this.dropDownOpenFile.Text = "Open File";
            this.dropDownOpenFile.Click += new System.EventHandler(this.MenuItemOpenFile);
            // 
            // dropDownSaveFile
            // 
            this.dropDownSaveFile.Name = "dropDownSaveFile";
            this.dropDownSaveFile.Size = new System.Drawing.Size(215, 34);
            this.dropDownSaveFile.Text = "Save File";
            this.dropDownSaveFile.Click += new System.EventHandler(this.MenuItemSaveFile);
            // 
            // dropDownLoadDefault
            // 
            this.dropDownLoadDefault.Name = "dropDownLoadDefault";
            this.dropDownLoadDefault.Size = new System.Drawing.Size(215, 34);
            this.dropDownLoadDefault.Text = "Load Default";
            this.dropDownLoadDefault.Click += new System.EventHandler(this.MenuItemLoadDefaultConfig);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(512, 24);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(162, 40);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(329, 24);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(162, 40);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK);
            // 
            // ChannelConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 717);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "ChannelConfigurationDialog";
            this.Text = "ChannelConfigurationDialog";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        internal ZedGraph.ZedGraphControl zedGraphChannels;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dropDownOpenFile;
        private System.Windows.Forms.ToolStripMenuItem dropDownSaveFile;
        private System.Windows.Forms.ToolStripMenuItem dropDownLoadDefault;
    }
}
