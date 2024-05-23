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
            this.tabPageRhs2116B = new System.Windows.Forms.TabPage();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
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
            this.tabControl.Controls.Add(this.tabPageRhs2116B);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1420, 657);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageChannelConfiguration
            // 
            this.tabPageChannelConfiguration.Location = new System.Drawing.Point(4, 29);
            this.tabPageChannelConfiguration.Name = "tabPageChannelConfiguration";
            this.tabPageChannelConfiguration.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageChannelConfiguration.Size = new System.Drawing.Size(1412, 624);
            this.tabPageChannelConfiguration.TabIndex = 0;
            this.tabPageChannelConfiguration.Text = "Channel Configuration";
            this.tabPageChannelConfiguration.UseVisualStyleBackColor = true;
            // 
            // tabPageStimulusSequence
            // 
            this.tabPageStimulusSequence.Location = new System.Drawing.Point(4, 29);
            this.tabPageStimulusSequence.Name = "tabPageStimulusSequence";
            this.tabPageStimulusSequence.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageStimulusSequence.Size = new System.Drawing.Size(1412, 699);
            this.tabPageStimulusSequence.TabIndex = 1;
            this.tabPageStimulusSequence.Text = "Stimulus Sequence";
            this.tabPageStimulusSequence.UseVisualStyleBackColor = true;
            // 
            // tabPageRhs2116A
            // 
            this.tabPageRhs2116A.Location = new System.Drawing.Point(4, 29);
            this.tabPageRhs2116A.Name = "tabPageRhs2116A";
            this.tabPageRhs2116A.Size = new System.Drawing.Size(1412, 699);
            this.tabPageRhs2116A.TabIndex = 2;
            this.tabPageRhs2116A.Text = "Rhs2116A";
            this.tabPageRhs2116A.UseVisualStyleBackColor = true;
            // 
            // tabPageRhs2116B
            // 
            this.tabPageRhs2116B.Location = new System.Drawing.Point(4, 29);
            this.tabPageRhs2116B.Name = "tabPageRhs2116B";
            this.tabPageRhs2116B.Size = new System.Drawing.Size(1412, 699);
            this.tabPageRhs2116B.TabIndex = 3;
            this.tabPageRhs2116B.Text = "Rhs2116B";
            this.tabPageRhs2116B.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(1246, 10);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(162, 49);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.OnClickCancel);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(1066, 10);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(162, 49);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.OnClickOK);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer1.Size = new System.Drawing.Size(1420, 732);
            this.splitContainer1.SplitterDistance = 657;
            this.splitContainer1.TabIndex = 2;
            // 
            // HeadstageRhs2116Dialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1420, 732);
            this.Controls.Add(this.splitContainer1);
            this.Name = "HeadstageRhs2116Dialog";
            this.Text = "HeadstageRhs2116Dialog";
            this.tabControl.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageChannelConfiguration;
        private System.Windows.Forms.TabPage tabPageStimulusSequence;
        private System.Windows.Forms.TabPage tabPageRhs2116A;
        private System.Windows.Forms.TabPage tabPageRhs2116B;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}
