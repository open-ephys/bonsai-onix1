namespace OpenEphys.Onix.Design
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
            this.buttonSaveChannelConfiguration = new System.Windows.Forms.Button();
            this.buttonLoadChannelConfiguration = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.zedGraphChannels);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.buttonSaveChannelConfiguration);
            this.splitContainer1.Panel2.Controls.Add(this.buttonLoadChannelConfiguration);
            this.splitContainer1.Panel2.Controls.Add(this.buttonCancel);
            this.splitContainer1.Panel2.Controls.Add(this.buttonOK);
            this.splitContainer1.Size = new System.Drawing.Size(714, 610);
            this.splitContainer1.SplitterDistance = 517;
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
            this.zedGraphChannels.Size = new System.Drawing.Size(714, 517);
            this.zedGraphChannels.TabIndex = 4;
            this.zedGraphChannels.UseExtendedPrintDialog = true;
            // 
            // buttonSaveChannelConfiguration
            // 
            this.buttonSaveChannelConfiguration.Location = new System.Drawing.Point(188, 19);
            this.buttonSaveChannelConfiguration.Name = "buttonSaveChannelConfiguration";
            this.buttonSaveChannelConfiguration.Size = new System.Drawing.Size(162, 49);
            this.buttonSaveChannelConfiguration.TabIndex = 7;
            this.buttonSaveChannelConfiguration.Text = "Save Channel Configuration";
            this.buttonSaveChannelConfiguration.UseVisualStyleBackColor = true;
            this.buttonSaveChannelConfiguration.Click += new System.EventHandler(this.ButtonSaveChannelConfiguration_Click);
            // 
            // buttonLoadChannelConfiguration
            // 
            this.buttonLoadChannelConfiguration.Location = new System.Drawing.Point(12, 19);
            this.buttonLoadChannelConfiguration.Name = "buttonLoadChannelConfiguration";
            this.buttonLoadChannelConfiguration.Size = new System.Drawing.Size(162, 49);
            this.buttonLoadChannelConfiguration.TabIndex = 5;
            this.buttonLoadChannelConfiguration.Text = "Load Channel Configuration";
            this.buttonLoadChannelConfiguration.UseVisualStyleBackColor = true;
            this.buttonLoadChannelConfiguration.Click += new System.EventHandler(this.ButtonNewChannelConfiguration_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(540, 19);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(162, 49);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(364, 19);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(162, 49);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // ChannelConfigurationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(714, 610);
            this.Controls.Add(this.splitContainer1);
            this.Name = "ChannelConfigurationDialog";
            this.Text = "ChannelConfigurationDialog";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private ZedGraph.ZedGraphControl zedGraphChannels;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonLoadChannelConfiguration;
        private System.Windows.Forms.Button buttonSaveChannelConfiguration;
    }
}
