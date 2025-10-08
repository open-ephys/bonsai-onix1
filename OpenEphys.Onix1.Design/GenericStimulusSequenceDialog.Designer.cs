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
            this.panelProbe = new System.Windows.Forms.Panel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stimulusWaveformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxDefineStimuli = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panelWaveform = new System.Windows.Forms.Panel();
            this.tabControlVisualization = new System.Windows.Forms.TabControl();
            this.tabPageWaveform = new System.Windows.Forms.TabPage();
            this.zedGraphWaveform = new ZedGraph.ZedGraphControl();
            this.tabPageTable = new System.Windows.Forms.TabPage();
            this.dataGridViewStimulusTable = new System.Windows.Forms.DataGridView();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panelWaveform.SuspendLayout();
            this.tabControlVisualization.SuspendLayout();
            this.tabPageWaveform.SuspendLayout();
            this.tabPageTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStimulusTable)).BeginInit();
            this.flowLayoutPanel2.SuspendLayout();
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
            // panelProbe
            // 
            this.panelProbe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProbe.Location = new System.Drawing.Point(698, 2);
            this.panelProbe.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelProbe.Name = "panelProbe";
            this.panelProbe.Size = new System.Drawing.Size(445, 167);
            this.panelProbe.TabIndex = 0;
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
            this.stimulusWaveformToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.stimulusWaveformToolStripMenuItem.Text = "Stimulus Waveform";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.openFileToolStripMenuItem.Text = "Open File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.MenuItemLoadFile_Click);
            // 
            // saveFileToolStripMenuItem
            // 
            this.saveFileToolStripMenuItem.Name = "saveFileToolStripMenuItem";
            this.saveFileToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.saveFileToolStripMenuItem.Text = "Save File";
            this.saveFileToolStripMenuItem.Click += new System.EventHandler(this.MenuItemSaveFile_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 451F));
            this.tableLayoutPanel1.Controls.Add(this.groupBoxDefineStimuli, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panelProbe, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.panelWaveform, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 308F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1146, 563);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // groupBoxDefineStimuli
            // 
            this.groupBoxDefineStimuli.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDefineStimuli.Location = new System.Drawing.Point(698, 173);
            this.groupBoxDefineStimuli.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxDefineStimuli.Name = "groupBoxDefineStimuli";
            this.groupBoxDefineStimuli.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.SetRowSpan(this.groupBoxDefineStimuli, 2);
            this.groupBoxDefineStimuli.Size = new System.Drawing.Size(445, 346);
            this.groupBoxDefineStimuli.TabIndex = 0;
            this.groupBoxDefineStimuli.TabStop = false;
            this.groupBoxDefineStimuli.Text = "Define Stimuli";
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
            this.panelWaveform.Controls.Add(this.tabControlVisualization);
            this.panelWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWaveform.Location = new System.Drawing.Point(3, 3);
            this.panelWaveform.Name = "panelWaveform";
            this.tableLayoutPanel1.SetRowSpan(this.panelWaveform, 2);
            this.panelWaveform.Size = new System.Drawing.Size(689, 473);
            this.panelWaveform.TabIndex = 8;
            // 
            // tabControlVisualization
            // 
            this.tabControlVisualization.Controls.Add(this.tabPageWaveform);
            this.tabControlVisualization.Controls.Add(this.tabPageTable);
            this.tabControlVisualization.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlVisualization.Location = new System.Drawing.Point(0, 0);
            this.tabControlVisualization.Name = "tabControlVisualization";
            this.tabControlVisualization.SelectedIndex = 0;
            this.tabControlVisualization.Size = new System.Drawing.Size(689, 473);
            this.tabControlVisualization.TabIndex = 6;
            // 
            // tabPageWaveform
            // 
            this.tabPageWaveform.Controls.Add(this.zedGraphWaveform);
            this.tabPageWaveform.Location = new System.Drawing.Point(4, 25);
            this.tabPageWaveform.Name = "tabPageWaveform";
            this.tabPageWaveform.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWaveform.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tabPageWaveform.Size = new System.Drawing.Size(681, 444);
            this.tabPageWaveform.TabIndex = 0;
            this.tabPageWaveform.Text = "Stimulus Waveform";
            this.tabPageWaveform.UseVisualStyleBackColor = true;
            // 
            // zedGraphWaveform
            // 
            this.zedGraphWaveform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zedGraphWaveform.Location = new System.Drawing.Point(3, 3);
            this.zedGraphWaveform.Margin = new System.Windows.Forms.Padding(0);
            this.zedGraphWaveform.Name = "zedGraphWaveform";
            this.zedGraphWaveform.ScrollGrace = 0D;
            this.zedGraphWaveform.ScrollMaxX = 0D;
            this.zedGraphWaveform.ScrollMaxY = 0D;
            this.zedGraphWaveform.ScrollMaxY2 = 0D;
            this.zedGraphWaveform.ScrollMinX = 0D;
            this.zedGraphWaveform.ScrollMinY = 0D;
            this.zedGraphWaveform.ScrollMinY2 = 0D;
            this.zedGraphWaveform.Size = new System.Drawing.Size(675, 438);
            this.zedGraphWaveform.TabIndex = 5;
            this.zedGraphWaveform.UseExtendedPrintDialog = true;
            // 
            // tabPageTable
            // 
            this.tabPageTable.Controls.Add(this.dataGridViewStimulusTable);
            this.tabPageTable.Location = new System.Drawing.Point(4, 25);
            this.tabPageTable.Name = "tabPageTable";
            this.tabPageTable.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTable.Size = new System.Drawing.Size(681, 444);
            this.tabPageTable.TabIndex = 1;
            this.tabPageTable.Text = "Table";
            this.tabPageTable.UseVisualStyleBackColor = true;
            // 
            // dataGridViewStimulusTable
            // 
            this.dataGridViewStimulusTable.AllowUserToAddRows = false;
            this.dataGridViewStimulusTable.AllowUserToDeleteRows = false;
            this.dataGridViewStimulusTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewStimulusTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewStimulusTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewStimulusTable.Location = new System.Drawing.Point(3, 3);
            this.dataGridViewStimulusTable.Name = "dataGridViewStimulusTable";
            this.dataGridViewStimulusTable.ReadOnly = true;
            this.dataGridViewStimulusTable.RowHeadersWidth = 62;
            this.dataGridViewStimulusTable.RowTemplate.Height = 24;
            this.dataGridViewStimulusTable.Size = new System.Drawing.Size(675, 438);
            this.dataGridViewStimulusTable.TabIndex = 0;
            this.dataGridViewStimulusTable.TabStop = false;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.button1);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 482);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(689, 36);
            this.flowLayoutPanel2.TabIndex = 9;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(3, 2);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(144, 32);
            this.button1.TabIndex = 5;
            this.button1.Text = "Reset Zoom";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.ResetZoom_Click);
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
            this.tabControlVisualization.ResumeLayout(false);
            this.tabPageWaveform.ResumeLayout(false);
            this.tabPageTable.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStimulusTable)).EndInit();
            this.flowLayoutPanel2.ResumeLayout(false);
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
        internal System.Windows.Forms.Panel panelProbe;
        internal System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem saveFileToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        internal System.Windows.Forms.GroupBox groupBoxDefineStimuli;
        private System.Windows.Forms.Panel panelWaveform;
        private ZedGraph.ZedGraphControl zedGraphWaveform;
        private System.Windows.Forms.TabControl tabControlVisualization;
        private System.Windows.Forms.TabPage tabPageWaveform;
        private System.Windows.Forms.TabPage tabPageTable;
        internal System.Windows.Forms.DataGridView dataGridViewStimulusTable;
        internal System.Windows.Forms.ToolStripMenuItem stimulusWaveformToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button button1;
    }
}
