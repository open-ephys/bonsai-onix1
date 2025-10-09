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
            this.zedGraphChannels = new ZedGraph.ZedGraphControl();
            this.buttonResetZoom = new System.Windows.Forms.Button();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dropDownOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.dropDownSaveFile = new System.Windows.Forms.ToolStripMenuItem();
            this.dropDownLoadDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.menuStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // zedGraphChannels
            // 
            this.zedGraphChannels.AutoSize = true;
            this.zedGraphChannels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphChannels.Location = new System.Drawing.Point(5, 6);
            this.zedGraphChannels.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.zedGraphChannels.Name = "zedGraphChannels";
            this.zedGraphChannels.ScrollGrace = 0D;
            this.zedGraphChannels.ScrollMaxX = 0D;
            this.zedGraphChannels.ScrollMaxY = 0D;
            this.zedGraphChannels.ScrollMaxY2 = 0D;
            this.zedGraphChannels.ScrollMinX = 0D;
            this.zedGraphChannels.ScrollMinY = 0D;
            this.zedGraphChannels.ScrollMinY2 = 0D;
            this.zedGraphChannels.Size = new System.Drawing.Size(599, 496);
            this.zedGraphChannels.TabIndex = 4;
            this.zedGraphChannels.TabStop = false;
            this.zedGraphChannels.UseExtendedPrintDialog = true;
            this.zedGraphChannels.ZoomEvent += new ZedGraph.ZedGraphControl.ZoomEventHandler(this.ZoomEvent);
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Image = global::OpenEphys.Onix1.Design.Properties.Resources.MagnifyingGlass;
            this.buttonResetZoom.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonResetZoom.Location = new System.Drawing.Point(3, 3);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(132, 32);
            this.buttonResetZoom.TabIndex = 0;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.buttonResetZoom.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ButtonResetZoom_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(5, 1, 0, 1);
            this.menuStrip.Size = new System.Drawing.Size(609, 24);
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
            this.fileMenuItem.Size = new System.Drawing.Size(37, 22);
            this.fileMenuItem.Text = "File";
            // 
            // dropDownOpenFile
            // 
            this.dropDownOpenFile.Name = "dropDownOpenFile";
            this.dropDownOpenFile.Size = new System.Drawing.Size(218, 22);
            this.dropDownOpenFile.Text = "Open ProbeInterface File";
            this.dropDownOpenFile.Click += new System.EventHandler(this.MenuItemOpenFile);
            // 
            // dropDownSaveFile
            // 
            this.dropDownSaveFile.Name = "dropDownSaveFile";
            this.dropDownSaveFile.Size = new System.Drawing.Size(218, 22);
            this.dropDownSaveFile.Text = "Save ProbeInterface File";
            this.dropDownSaveFile.Click += new System.EventHandler(this.MenuItemSaveFile);
            // 
            // dropDownLoadDefault
            // 
            this.dropDownLoadDefault.Name = "dropDownLoadDefault";
            this.dropDownLoadDefault.Size = new System.Drawing.Size(218, 22);
            this.dropDownLoadDefault.Text = "Load Default Configuration";
            this.dropDownLoadDefault.Click += new System.EventHandler(this.MenuItemLoadDefaultConfig);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.zedGraphChannels, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(609, 550);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.buttonResetZoom);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 511);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(603, 36);
            this.flowLayoutPanel2.TabIndex = 5;
            // 
            // ChannelConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 574);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "ChannelConfigurationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Channel Configuration";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal ZedGraph.ZedGraphControl zedGraphChannels;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dropDownOpenFile;
        private System.Windows.Forms.ToolStripMenuItem dropDownSaveFile;
        private System.Windows.Forms.ToolStripMenuItem dropDownLoadDefault;
        private System.Windows.Forms.Button buttonResetZoom;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
    }
}
