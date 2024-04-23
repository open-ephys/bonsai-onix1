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
            this.ZedGraphWaveform = new ZedGraph.ZedGraphControl();
            this.LinkLabelDocumentation = new System.Windows.Forms.LinkLabel();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.ToolStripStatusIsValid = new System.Windows.Forms.ToolStripStatusLabel();
            this.ToolStripStatusSlotsUsed = new System.Windows.Forms.ToolStripStatusLabel();
            this.TableLayoutPanelGraphAndProperties = new System.Windows.Forms.TableLayoutPanel();
            this.TableLayoutPanelProperties = new System.Windows.Forms.TableLayoutPanel();
            this.PanelButtons = new System.Windows.Forms.Panel();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.TabPageAddSinglePulse = new System.Windows.Forms.TabPage();
            this.TabPageAddPulseTrain = new System.Windows.Forms.TabPage();
            this.TabPageEditorDialog = new System.Windows.Forms.TabPage();
            this.PropertyGridStimulusSequence = new System.Windows.Forms.PropertyGrid();
            this.ZedGraphControlChannels = new ZedGraph.ZedGraphControl();
            this.StatusStrip.SuspendLayout();
            this.TableLayoutPanelGraphAndProperties.SuspendLayout();
            this.TableLayoutPanelProperties.SuspendLayout();
            this.PanelButtons.SuspendLayout();
            this.TabControl.SuspendLayout();
            this.TabPageEditorDialog.SuspendLayout();
            this.SuspendLayout();
            // 
            // ZedGraphWaveform
            // 
            this.ZedGraphWaveform.AutoScroll = true;
            this.ZedGraphWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ZedGraphWaveform.Location = new System.Drawing.Point(4, 5);
            this.ZedGraphWaveform.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ZedGraphWaveform.Name = "ZedGraphWaveform";
            this.ZedGraphWaveform.ScrollGrace = 0D;
            this.ZedGraphWaveform.ScrollMaxX = 0D;
            this.ZedGraphWaveform.ScrollMaxY = 0D;
            this.ZedGraphWaveform.ScrollMaxY2 = 0D;
            this.ZedGraphWaveform.ScrollMinX = 0D;
            this.ZedGraphWaveform.ScrollMinY = 0D;
            this.ZedGraphWaveform.ScrollMinY2 = 0D;
            this.ZedGraphWaveform.Size = new System.Drawing.Size(1359, 716);
            this.ZedGraphWaveform.TabIndex = 4;
            this.ZedGraphWaveform.UseExtendedPrintDialog = true;
            // 
            // LinkLabelDocumentation
            // 
            this.LinkLabelDocumentation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LinkLabelDocumentation.AutoSize = true;
            this.LinkLabelDocumentation.Location = new System.Drawing.Point(1078, 25);
            this.LinkLabelDocumentation.Name = "LinkLabelDocumentation";
            this.LinkLabelDocumentation.Size = new System.Drawing.Size(118, 20);
            this.LinkLabelDocumentation.TabIndex = 4;
            this.LinkLabelDocumentation.TabStop = true;
            this.LinkLabelDocumentation.Text = "Documentation";
            this.LinkLabelDocumentation.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelDocumentation_LinkClicked);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonCancel.Location = new System.Drawing.Point(166, 51);
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
            this.ToolStripStatusIsValid,
            this.ToolStripStatusSlotsUsed});
            this.StatusStrip.Location = new System.Drawing.Point(0, 726);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(1708, 32);
            this.StatusStrip.TabIndex = 1;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // ToolStripStatusIsValid
            // 
            this.ToolStripStatusIsValid.AutoSize = false;
            this.ToolStripStatusIsValid.Image = global::OpenEphys.Onix.Design.Properties.Resources.StatusReadyImage;
            this.ToolStripStatusIsValid.Name = "ToolStripStatusIsValid";
            this.ToolStripStatusIsValid.Size = new System.Drawing.Size(300, 25);
            this.ToolStripStatusIsValid.Text = "Valid stimulus sequence";
            // 
            // ToolStripStatusSlotsUsed
            // 
            this.ToolStripStatusSlotsUsed.AutoSize = false;
            this.ToolStripStatusSlotsUsed.Name = "ToolStripStatusSlotsUsed";
            this.ToolStripStatusSlotsUsed.Size = new System.Drawing.Size(180, 25);
            this.ToolStripStatusSlotsUsed.Text = "100% of slots used";
            this.ToolStripStatusSlotsUsed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TableLayoutPanelGraphAndProperties
            // 
            this.TableLayoutPanelGraphAndProperties.ColumnCount = 2;
            this.TableLayoutPanelGraphAndProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.05372F));
            this.TableLayoutPanelGraphAndProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.94628F));
            this.TableLayoutPanelGraphAndProperties.Controls.Add(this.ZedGraphWaveform, 0, 0);
            this.TableLayoutPanelGraphAndProperties.Controls.Add(this.TableLayoutPanelProperties, 1, 0);
            this.TableLayoutPanelGraphAndProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanelGraphAndProperties.Location = new System.Drawing.Point(0, 0);
            this.TableLayoutPanelGraphAndProperties.Name = "TableLayoutPanelGraphAndProperties";
            this.TableLayoutPanelGraphAndProperties.RowCount = 1;
            this.TableLayoutPanelGraphAndProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanelGraphAndProperties.Size = new System.Drawing.Size(1708, 726);
            this.TableLayoutPanelGraphAndProperties.TabIndex = 5;
            // 
            // TableLayoutPanelProperties
            // 
            this.TableLayoutPanelProperties.AutoSize = true;
            this.TableLayoutPanelProperties.ColumnCount = 1;
            this.TableLayoutPanelProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanelProperties.Controls.Add(this.PanelButtons, 0, 2);
            this.TableLayoutPanelProperties.Controls.Add(this.TabControl, 0, 1);
            this.TableLayoutPanelProperties.Controls.Add(this.ZedGraphControlChannels, 0, 0);
            this.TableLayoutPanelProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanelProperties.Location = new System.Drawing.Point(1370, 3);
            this.TableLayoutPanelProperties.Name = "TableLayoutPanelProperties";
            this.TableLayoutPanelProperties.RowCount = 3;
            this.TableLayoutPanelProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.TableLayoutPanelProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanelProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.TableLayoutPanelProperties.Size = new System.Drawing.Size(335, 720);
            this.TableLayoutPanelProperties.TabIndex = 5;
            // 
            // PanelButtons
            // 
            this.PanelButtons.AutoSize = true;
            this.PanelButtons.Controls.Add(this.ButtonOk);
            this.PanelButtons.Controls.Add(this.ButtonCancel);
            this.PanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelButtons.Location = new System.Drawing.Point(3, 623);
            this.PanelButtons.Name = "PanelButtons";
            this.PanelButtons.Size = new System.Drawing.Size(329, 94);
            this.PanelButtons.TabIndex = 0;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ButtonOk.Location = new System.Drawing.Point(31, 51);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(100, 40);
            this.ButtonOk.TabIndex = 4;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            // 
            // TabControl
            // 
            this.TabControl.Controls.Add(this.TabPageAddSinglePulse);
            this.TabControl.Controls.Add(this.TabPageAddPulseTrain);
            this.TabControl.Controls.Add(this.TabPageEditorDialog);
            this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControl.Location = new System.Drawing.Point(3, 253);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(329, 364);
            this.TabControl.TabIndex = 1;
            // 
            // TabPageAddSinglePulse
            // 
            this.TabPageAddSinglePulse.Location = new System.Drawing.Point(4, 29);
            this.TabPageAddSinglePulse.Name = "TabPageAddSinglePulse";
            this.TabPageAddSinglePulse.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageAddSinglePulse.Size = new System.Drawing.Size(321, 331);
            this.TabPageAddSinglePulse.TabIndex = 0;
            this.TabPageAddSinglePulse.Text = "Add Single Pulse";
            this.TabPageAddSinglePulse.UseVisualStyleBackColor = true;
            // 
            // TabPageAddPulseTrain
            // 
            this.TabPageAddPulseTrain.Location = new System.Drawing.Point(4, 29);
            this.TabPageAddPulseTrain.Name = "TabPageAddPulseTrain";
            this.TabPageAddPulseTrain.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageAddPulseTrain.Size = new System.Drawing.Size(321, 331);
            this.TabPageAddPulseTrain.TabIndex = 1;
            this.TabPageAddPulseTrain.Text = "Add Pulse Train";
            this.TabPageAddPulseTrain.UseVisualStyleBackColor = true;
            // 
            // TabPageEditorDialog
            // 
            this.TabPageEditorDialog.Controls.Add(this.PropertyGridStimulusSequence);
            this.TabPageEditorDialog.Location = new System.Drawing.Point(4, 29);
            this.TabPageEditorDialog.Name = "TabPageEditorDialog";
            this.TabPageEditorDialog.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageEditorDialog.Size = new System.Drawing.Size(321, 331);
            this.TabPageEditorDialog.TabIndex = 2;
            this.TabPageEditorDialog.Text = "EditorDialog";
            this.TabPageEditorDialog.UseVisualStyleBackColor = true;
            // 
            // PropertyGridStimulusSequence
            // 
            this.PropertyGridStimulusSequence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyGridStimulusSequence.Location = new System.Drawing.Point(3, 3);
            this.PropertyGridStimulusSequence.Name = "PropertyGridStimulusSequence";
            this.PropertyGridStimulusSequence.Size = new System.Drawing.Size(315, 325);
            this.PropertyGridStimulusSequence.TabIndex = 4;
            this.PropertyGridStimulusSequence.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.PropertyGridStimulusSequence_PropertyValueChanged);
            // 
            // ZedGraphControlChannels
            // 
            this.ZedGraphControlChannels.AutoSize = true;
            this.ZedGraphControlChannels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ZedGraphControlChannels.Location = new System.Drawing.Point(4, 5);
            this.ZedGraphControlChannels.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ZedGraphControlChannels.Name = "ZedGraphControlChannels";
            this.ZedGraphControlChannels.ScrollGrace = 0D;
            this.ZedGraphControlChannels.ScrollMaxX = 0D;
            this.ZedGraphControlChannels.ScrollMaxY = 0D;
            this.ZedGraphControlChannels.ScrollMaxY2 = 0D;
            this.ZedGraphControlChannels.ScrollMinX = 0D;
            this.ZedGraphControlChannels.ScrollMinY = 0D;
            this.ZedGraphControlChannels.ScrollMinY2 = 0D;
            this.ZedGraphControlChannels.Size = new System.Drawing.Size(327, 240);
            this.ZedGraphControlChannels.TabIndex = 2;
            this.ZedGraphControlChannels.UseExtendedPrintDialog = true;
            // 
            // Rhs2116StimulusSequenceDialog
            // 
            this.AccessibleDescription = "a";
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(1708, 758);
            this.Controls.Add(this.TableLayoutPanelGraphAndProperties);
            this.Controls.Add(this.LinkLabelDocumentation);
            this.Controls.Add(this.StatusStrip);
            this.Name = "Rhs2116StimulusSequenceDialog";
            this.Text = "Rhs2116StimulusSequenceDialog";
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.TableLayoutPanelGraphAndProperties.ResumeLayout(false);
            this.TableLayoutPanelGraphAndProperties.PerformLayout();
            this.TableLayoutPanelProperties.ResumeLayout(false);
            this.TableLayoutPanelProperties.PerformLayout();
            this.PanelButtons.ResumeLayout(false);
            this.TabControl.ResumeLayout(false);
            this.TabPageEditorDialog.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel ToolStripStatusIsValid;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.LinkLabel LinkLabelDocumentation;
        private ZedGraph.ZedGraphControl ZedGraphWaveform;
        private System.Windows.Forms.ToolStripStatusLabel ToolStripStatusSlotsUsed;
        private System.Windows.Forms.TableLayoutPanel TableLayoutPanelGraphAndProperties;
        private System.Windows.Forms.TableLayoutPanel TableLayoutPanelProperties;
        private System.Windows.Forms.Panel PanelButtons;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage TabPageAddSinglePulse;
        private System.Windows.Forms.TabPage TabPageAddPulseTrain;
        private System.Windows.Forms.TabPage TabPageEditorDialog;
        private System.Windows.Forms.PropertyGrid PropertyGridStimulusSequence;
        private ZedGraph.ZedGraphControl ZedGraphControlChannels;
    }
}
