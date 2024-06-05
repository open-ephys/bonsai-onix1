namespace OpenEphys.Onix.Design
{
    partial class HeadstageRhs2116Dialog
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageChannelConfiguration = new System.Windows.Forms.TabPage();
            this.tabPageStimulusSequence = new System.Windows.Forms.TabPage();
            this.tabPageRhs2116A = new System.Windows.Forms.TabPage();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.tabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageChannelConfiguration);
            this.tabControl.Controls.Add(this.tabPageStimulusSequence);
            this.tabControl.Controls.Add(this.tabPageRhs2116A);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1894, 877);
            this.tabControl.TabIndex = 0;
            this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.TabPage_Selected);
            // 
            // tabPageChannelConfiguration
            // 
            this.tabPageChannelConfiguration.Location = new System.Drawing.Point(4, 29);
            this.tabPageChannelConfiguration.Name = "tabPageChannelConfiguration";
            this.tabPageChannelConfiguration.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChannelConfiguration.Size = new System.Drawing.Size(1886, 844);
            this.tabPageChannelConfiguration.TabIndex = 0;
            this.tabPageChannelConfiguration.Text = "Channel Configuration";
            this.tabPageChannelConfiguration.UseVisualStyleBackColor = true;
            // 
            // tabPageStimulusSequence
            // 
            this.tabPageStimulusSequence.Location = new System.Drawing.Point(4, 29);
            this.tabPageStimulusSequence.Name = "tabPageStimulusSequence";
            this.tabPageStimulusSequence.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStimulusSequence.Size = new System.Drawing.Size(1886, 844);
            this.tabPageStimulusSequence.TabIndex = 1;
            this.tabPageStimulusSequence.Text = "Stimulus Sequence";
            this.tabPageStimulusSequence.UseVisualStyleBackColor = true;
            // 
            // tabPageRhs2116A
            // 
            this.tabPageRhs2116A.Location = new System.Drawing.Point(4, 29);
            this.tabPageRhs2116A.Name = "tabPageRhs2116A";
            this.tabPageRhs2116A.Size = new System.Drawing.Size(1886, 844);
            this.tabPageRhs2116A.TabIndex = 2;
            this.tabPageRhs2116A.Text = "Rhs2116";
            this.tabPageRhs2116A.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(1720, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(162, 38);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.OnClickCancel);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(1540, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(162, 38);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.OnClickOK);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.buttonOK);
            this.splitContainer1.Panel2.Controls.Add(this.buttonCancel);
            this.splitContainer1.Size = new System.Drawing.Size(1894, 930);
            this.splitContainer1.SplitterDistance = 877;
            this.splitContainer1.TabIndex = 2;
            // 
            // menuStrip
            // 
            this.menuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1894, 24);
            this.menuStrip.TabIndex = 3;
            this.menuStrip.Text = "menuStrip1";
            // 
            // HeadstageRhs2116Dialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1894, 954);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "HeadstageRhs2116Dialog";
            this.Text = "HeadstageRhs2116Dialog";
            this.tabControl.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageChannelConfiguration;
        private System.Windows.Forms.TabPage tabPageStimulusSequence;
        private System.Windows.Forms.TabPage tabPageRhs2116A;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.MenuStrip menuStrip;
    }
}
