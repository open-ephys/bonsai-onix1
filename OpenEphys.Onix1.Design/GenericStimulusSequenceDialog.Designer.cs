namespace OpenEphys.Onix1.Design
{
    partial class GenericStimulusSequenceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenericStimulusSequenceDialog));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusIsValid = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonOk = new System.Windows.Forms.Button();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stimulusWaveformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panelWaveform = new System.Windows.Forms.Panel();
            this.zedGraphWaveform = new ZedGraph.ZedGraphControl();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonResetZoom = new System.Windows.Forms.Button();
            this.tabControlProperties = new System.Windows.Forms.TabControl();
            this.tabPageDefineStimuli = new System.Windows.Forms.TabPage();
            this.tabPageProperties = new System.Windows.Forms.TabPage();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.bindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panelWaveform.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.tabControlProperties.SuspendLayout();
            this.tabPageProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(993, 2);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(144, 32);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.AutoSize = false;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusIsValid,
            this.toolStripStatusText});
            this.statusStrip.Location = new System.Drawing.Point(0, 587);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(3, 0, 12, 0);
            this.statusStrip.Size = new System.Drawing.Size(1146, 26);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusIsValid
            // 
            this.toolStripStatusIsValid.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.toolStripStatusIsValid.BorderStyle = System.Windows.Forms.Border3DStyle.Raised;
            this.toolStripStatusIsValid.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusReadyImage;
            this.toolStripStatusIsValid.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripStatusIsValid.Name = "toolStripStatusIsValid";
            this.toolStripStatusIsValid.Size = new System.Drawing.Size(153, 21);
            this.toolStripStatusIsValid.Text = "Valid stimulus sequence";
            this.toolStripStatusIsValid.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusText
            // 
            this.toolStripStatusText.Name = "toolStripStatusText";
            this.toolStripStatusText.Size = new System.Drawing.Size(0, 21);
            this.toolStripStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonOk
            // 
            this.buttonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOk.Location = new System.Drawing.Point(843, 2);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(144, 32);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(1146, 24);
            this.menuStrip.TabIndex = 7;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stimulusWaveformToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // stimulusWaveformToolStripMenuItem
            // 
            this.stimulusWaveformToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.saveFileToolStripMenuItem});
            this.stimulusWaveformToolStripMenuItem.Name = "stimulusWaveformToolStripMenuItem";
            this.stimulusWaveformToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.stimulusWaveformToolStripMenuItem.Text = "Stimulus Waveform";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openFileToolStripMenuItem.Text = "Import File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.MenuItemLoadFile_Click);
            // 
            // saveFileToolStripMenuItem
            // 
            this.saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
            this.saveFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveFileToolStripMenuItem.Text = "Export File";
            this.saveFileToolStripMenuItem.Click += new System.EventHandler(this.MenuItemSaveFile_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 451F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panelWaveform, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tabControlProperties, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1146, 563);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // flowLayoutPanel1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel1, 2);
            this.flowLayoutPanel1.Controls.Add(this.buttonCancel);
            this.flowLayoutPanel1.Controls.Add(this.buttonOk);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 523);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1140, 38);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // panelWaveform
            // 
            this.panelWaveform.Controls.Add(this.zedGraphWaveform);
            this.panelWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWaveform.Location = new System.Drawing.Point(3, 3);
            this.panelWaveform.Name = "panelWaveform";
            this.panelWaveform.Size = new System.Drawing.Size(689, 473);
            this.panelWaveform.TabIndex = 8;
            // 
            // zedGraphWaveform
            // 
            this.zedGraphWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphWaveform.Location = new System.Drawing.Point(0, 0);
            this.zedGraphWaveform.Margin = new System.Windows.Forms.Padding(0);
            this.zedGraphWaveform.Name = "zedGraphWaveform";
            this.zedGraphWaveform.ScrollGrace = 0D;
            this.zedGraphWaveform.ScrollMaxX = 0D;
            this.zedGraphWaveform.ScrollMaxY = 0D;
            this.zedGraphWaveform.ScrollMaxY2 = 0D;
            this.zedGraphWaveform.ScrollMinX = 0D;
            this.zedGraphWaveform.ScrollMinY = 0D;
            this.zedGraphWaveform.ScrollMinY2 = 0D;
            this.zedGraphWaveform.Size = new System.Drawing.Size(689, 473);
            this.zedGraphWaveform.TabIndex = 6;
            this.zedGraphWaveform.UseExtendedPrintDialog = true;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.buttonResetZoom);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 482);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(689, 36);
            this.flowLayoutPanel2.TabIndex = 9;
            // 
            // buttonResetZoom
            // 
            this.buttonResetZoom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonResetZoom.Location = new System.Drawing.Point(3, 2);
            this.buttonResetZoom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonResetZoom.Name = "buttonResetZoom";
            this.buttonResetZoom.Size = new System.Drawing.Size(144, 32);
            this.buttonResetZoom.TabIndex = 5;
            this.buttonResetZoom.Text = "Reset Zoom";
            this.buttonResetZoom.UseVisualStyleBackColor = true;
            this.buttonResetZoom.Click += new System.EventHandler(this.ResetZoom_Click);
            // 
            // tabControlProperties
            // 
            this.tabControlProperties.Controls.Add(this.tabPageDefineStimuli);
            this.tabControlProperties.Controls.Add(this.tabPageProperties);
            this.tabControlProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlProperties.Location = new System.Drawing.Point(698, 3);
            this.tabControlProperties.Name = "tabControlProperties";
            this.tableLayoutPanel1.SetRowSpan(this.tabControlProperties, 2);
            this.tabControlProperties.SelectedIndex = 0;
            this.tabControlProperties.Size = new System.Drawing.Size(445, 515);
            this.tabControlProperties.TabIndex = 10;
            // 
            // tabPageDefineStimuli
            // 
            this.tabPageDefineStimuli.Location = new System.Drawing.Point(4, 25);
            this.tabPageDefineStimuli.Name = "tabPageDefineStimuli";
            this.tabPageDefineStimuli.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDefineStimuli.Size = new System.Drawing.Size(437, 486);
            this.tabPageDefineStimuli.TabIndex = 0;
            this.tabPageDefineStimuli.Text = "Define Stimuli";
            this.tabPageDefineStimuli.UseVisualStyleBackColor = true;
            // 
            // tabPageProperties
            // 
            this.tabPageProperties.Controls.Add(this.propertyGrid);
            this.tabPageProperties.Location = new System.Drawing.Point(4, 25);
            this.tabPageProperties.Name = "tabPageProperties";
            this.tabPageProperties.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageProperties.Size = new System.Drawing.Size(437, 486);
            this.tabPageProperties.TabIndex = 1;
            this.tabPageProperties.Text = "Properties";
            this.tabPageProperties.UseVisualStyleBackColor = true;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(3, 3);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(431, 480);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.PropertyValueChanged);
            // 
            // GenericStimulusSequenceDialog
            // 
            this.AccessibleDescription = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(1146, 613);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(131, 52);
            this.Name = "GenericStimulusSequenceDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "StimulusSequenceDialog";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panelWaveform.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.tabControlProperties.ResumeLayout(false);
            this.tabPageProperties.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip;
        internal System.Windows.Forms.ToolStripStatusLabel toolStripStatusIsValid;
        private System.Windows.Forms.Button buttonCancel;
        internal System.Windows.Forms.ToolStripStatusLabel toolStripStatusText;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem saveFileToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        internal System.Windows.Forms.ToolStripMenuItem stimulusWaveformToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button buttonResetZoom;
        internal System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        internal System.Windows.Forms.Panel panelWaveform;
        private ZedGraph.ZedGraphControl zedGraphWaveform;
        internal System.Windows.Forms.TabPage tabPageDefineStimuli;
        internal System.Windows.Forms.TabPage tabPageProperties;
        internal System.Windows.Forms.PropertyGrid propertyGrid;
        internal System.Windows.Forms.BindingSource bindingSource;
        internal System.Windows.Forms.TabControl tabControlProperties;
    }
}
