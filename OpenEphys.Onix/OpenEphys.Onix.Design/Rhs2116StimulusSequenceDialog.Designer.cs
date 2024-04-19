namespace OpenEphys.Onix.Design
{
    partial class Rhs2116StimulusSequenceDialog
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
            this.SplitContainerWaveformAndProperties = new System.Windows.Forms.SplitContainer();
            this.ZedGraphWaveform = new ZedGraph.ZedGraphControl();
            this.MenuStrip = new System.Windows.Forms.MenuStrip();
            this.LinkLabelDocumentation = new System.Windows.Forms.LinkLabel();
            this.PropertyGridStimulusSequence = new System.Windows.Forms.PropertyGrid();
            this.PanelButtons = new System.Windows.Forms.Panel();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.ToolStripStatusIsValid = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainerWaveformAndProperties)).BeginInit();
            this.SplitContainerWaveformAndProperties.Panel1.SuspendLayout();
            this.SplitContainerWaveformAndProperties.Panel2.SuspendLayout();
            this.SplitContainerWaveformAndProperties.SuspendLayout();
            this.PanelButtons.SuspendLayout();
            this.StatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // SplitContainerWaveformAndProperties
            // 
            this.SplitContainerWaveformAndProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SplitContainerWaveformAndProperties.Location = new System.Drawing.Point(0, 0);
            this.SplitContainerWaveformAndProperties.Name = "SplitContainerWaveformAndProperties";
            // 
            // SplitContainerWaveformAndProperties.Panel1
            // 
            this.SplitContainerWaveformAndProperties.Panel1.Controls.Add(this.ZedGraphWaveform);
            this.SplitContainerWaveformAndProperties.Panel1.Controls.Add(this.MenuStrip);
            // 
            // SplitContainerWaveformAndProperties.Panel2
            // 
            this.SplitContainerWaveformAndProperties.Panel2.Controls.Add(this.LinkLabelDocumentation);
            this.SplitContainerWaveformAndProperties.Panel2.Controls.Add(this.PropertyGridStimulusSequence);
            this.SplitContainerWaveformAndProperties.Size = new System.Drawing.Size(1440, 508);
            this.SplitContainerWaveformAndProperties.SplitterDistance = 1113;
            this.SplitContainerWaveformAndProperties.TabIndex = 0;
            // 
            // ZedGraphWaveform
            // 
            this.ZedGraphWaveform.AutoScroll = true;
            this.ZedGraphWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ZedGraphWaveform.Location = new System.Drawing.Point(0, 36);
            this.ZedGraphWaveform.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ZedGraphWaveform.Name = "ZedGraphWaveform";
            this.ZedGraphWaveform.ScrollGrace = 0D;
            this.ZedGraphWaveform.ScrollMaxX = 0D;
            this.ZedGraphWaveform.ScrollMaxY = 0D;
            this.ZedGraphWaveform.ScrollMaxY2 = 0D;
            this.ZedGraphWaveform.ScrollMinX = 0D;
            this.ZedGraphWaveform.ScrollMinY = 0D;
            this.ZedGraphWaveform.ScrollMinY2 = 0D;
            this.ZedGraphWaveform.Size = new System.Drawing.Size(1113, 472);
            this.ZedGraphWaveform.TabIndex = 4;
            this.ZedGraphWaveform.UseExtendedPrintDialog = true;
            // 
            // MenuStrip
            // 
            this.MenuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.MenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(1113, 36);
            this.MenuStrip.TabIndex = 3;
            this.MenuStrip.Text = "menuStrip1";
            // 
            // LinkLabelDocumentation
            // 
            this.LinkLabelDocumentation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LinkLabelDocumentation.AutoSize = true;
            this.LinkLabelDocumentation.Location = new System.Drawing.Point(193, 9);
            this.LinkLabelDocumentation.Name = "LinkLabelDocumentation";
            this.LinkLabelDocumentation.Size = new System.Drawing.Size(118, 20);
            this.LinkLabelDocumentation.TabIndex = 4;
            this.LinkLabelDocumentation.TabStop = true;
            this.LinkLabelDocumentation.Text = "Documentation";
            this.LinkLabelDocumentation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelDocumentation_LinkClicked);
            // 
            // PropertyGridStimulusSequence
            // 
            this.PropertyGridStimulusSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyGridStimulusSequence.Location = new System.Drawing.Point(0, 0);
            this.PropertyGridStimulusSequence.Name = "PropertyGridStimulusSequence";
            this.PropertyGridStimulusSequence.Size = new System.Drawing.Size(323, 508);
            this.PropertyGridStimulusSequence.TabIndex = 0;
            this.PropertyGridStimulusSequence.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.PropertyGridStimulusSequence_PropertyValueChanged);
            // 
            // PanelButtons
            // 
            this.PanelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PanelButtons.Controls.Add(this.ButtonOk);
            this.PanelButtons.Controls.Add(this.ButtonCancel);
            this.PanelButtons.Location = new System.Drawing.Point(0, 516);
            this.PanelButtons.Name = "PanelButtons";
            this.PanelButtons.Size = new System.Drawing.Size(1440, 40);
            this.PanelButtons.TabIndex = 2;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonOk.Location = new System.Drawing.Point(1221, 0);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(100, 40);
            this.ButtonOk.TabIndex = 1;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(1329, 0);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(100, 40);
            this.ButtonCancel.TabIndex = 0;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // StatusStrip
            // 
            this.StatusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripStatusIsValid});
            this.StatusStrip.Location = new System.Drawing.Point(0, 556);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(1440, 32);
            this.StatusStrip.TabIndex = 1;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // ToolStripStatusIsValid
            // 
            this.ToolStripStatusIsValid.Name = "ToolStripStatusIsValid";
            this.ToolStripStatusIsValid.Size = new System.Drawing.Size(179, 25);
            this.ToolStripStatusIsValid.Text = "toolStripStatusLabel1";
            // 
            // Rhs2116StimulusSequenceDialog
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(1440, 588);
            this.Controls.Add(this.PanelButtons);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.SplitContainerWaveformAndProperties);
            this.Name = "Rhs2116StimulusSequenceDialog";
            this.Text = "Rhs2116StimulusSequenceDialog";
            this.SplitContainerWaveformAndProperties.Panel1.ResumeLayout(false);
            this.SplitContainerWaveformAndProperties.Panel1.PerformLayout();
            this.SplitContainerWaveformAndProperties.Panel2.ResumeLayout(false);
            this.SplitContainerWaveformAndProperties.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainerWaveformAndProperties)).EndInit();
            this.SplitContainerWaveformAndProperties.ResumeLayout(false);
            this.PanelButtons.ResumeLayout(false);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer SplitContainerWaveformAndProperties;
        private System.Windows.Forms.PropertyGrid PropertyGridStimulusSequence;
        private System.Windows.Forms.Panel PanelButtons;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel ToolStripStatusIsValid;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.MenuStrip MenuStrip;
        private System.Windows.Forms.LinkLabel LinkLabelDocumentation;
        private ZedGraph.ZedGraphControl ZedGraphWaveform;
    }
}
