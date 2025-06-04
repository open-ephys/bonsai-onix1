using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    partial class SpatialTransformMatrixDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpatialTransformMatrixDialog));
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxStatus = new System.Windows.Forms.GroupBox();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.labelInstructions = new System.Windows.Forms.Label();
            this.tableLayoutPanelCoordinates = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxUserCoordinate3 = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate2 = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate1 = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate0 = new System.Windows.Forms.TextBox();
            this.textBoxTS4231Coordinate3 = new System.Windows.Forms.TextBox();
            this.textBoxTS4231Coordinate2 = new System.Windows.Forms.TextBox();
            this.textBoxTS4231Coordinate1 = new System.Windows.Forms.TextBox();
            this.buttonMeasure3 = new System.Windows.Forms.Button();
            this.buttonMeasure2 = new System.Windows.Forms.Button();
            this.buttonMeasure1 = new System.Windows.Forms.Button();
            this.labelCoordinate3 = new System.Windows.Forms.Label();
            this.labelCoordinate2 = new System.Windows.Forms.Label();
            this.labelCoordinate1 = new System.Windows.Forms.Label();
            this.buttonMeasure0 = new System.Windows.Forms.Button();
            this.labelHeaderTS4231 = new System.Windows.Forms.Label();
            this.labelCoordinate0 = new System.Windows.Forms.Label();
            this.textBoxTS4231Coordinate0 = new System.Windows.Forms.TextBox();
            this.labelHeaderUser = new System.Windows.Forms.Label();
            this.buttonCalculate = new System.Windows.Forms.Button();
            this.flowLayoutPanelBottom = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelTS4231 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelUser = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanelMain.SuspendLayout();
            this.groupBoxStatus.SuspendLayout();
            this.tableLayoutPanelCoordinates.SuspendLayout();
            this.flowLayoutPanelBottom.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.groupBoxStatus, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.labelInstructions, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelCoordinates, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.buttonCalculate, 0, 3);
            this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelBottom, 0, 4);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 5;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(604, 639);
            this.tableLayoutPanelMain.TabIndex = 7;
            // 
            // groupBoxStatus
            // 
            this.groupBoxStatus.Controls.Add(this.textBoxStatus);
            this.groupBoxStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxStatus.Location = new System.Drawing.Point(3, 263);
            this.groupBoxStatus.Name = "groupBoxStatus";
            this.groupBoxStatus.Size = new System.Drawing.Size(598, 308);
            this.groupBoxStatus.TabIndex = 6;
            this.groupBoxStatus.TabStop = false;
            this.groupBoxStatus.Text = "Status Messages";
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.AcceptsReturn = true;
            this.textBoxStatus.AcceptsTab = true;
            this.textBoxStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxStatus.Location = new System.Drawing.Point(3, 16);
            this.textBoxStatus.Multiline = true;
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxStatus.Size = new System.Drawing.Size(592, 289);
            this.textBoxStatus.TabIndex = 3;
            this.textBoxStatus.Text = "Awaiting user input...\r\n";
            // 
            // labelInstructions
            // 
            this.labelInstructions.AutoSize = true;
            this.labelInstructions.Location = new System.Drawing.Point(3, 0);
            this.labelInstructions.Name = "labelInstructions";
            this.labelInstructions.Size = new System.Drawing.Size(596, 104);
            this.labelInstructions.TabIndex = 4;
            this.labelInstructions.Text = resources.GetString("labelInstructions.Text");
            // 
            // tableLayoutPanelCoordinates
            // 
            this.tableLayoutPanelCoordinates.AutoSize = true;
            this.tableLayoutPanelCoordinates.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelCoordinates.ColumnCount = 4;
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate3, 3, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate2, 3, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate1, 3, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate0, 3, 1);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxTS4231Coordinate3, 2, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxTS4231Coordinate2, 2, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxTS4231Coordinate1, 2, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.buttonMeasure3, 1, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.buttonMeasure2, 1, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.buttonMeasure1, 1, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelCoordinate3, 0, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelCoordinate2, 0, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelCoordinate1, 0, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.buttonMeasure0, 1, 1);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelHeaderTS4231, 1, 0);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelCoordinate0, 0, 1);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxTS4231Coordinate0, 2, 1);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelHeaderUser, 3, 0);
            this.tableLayoutPanelCoordinates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelCoordinates.Location = new System.Drawing.Point(3, 107);
            this.tableLayoutPanelCoordinates.Name = "tableLayoutPanelCoordinates";
            this.tableLayoutPanelCoordinates.RowCount = 5;
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.Size = new System.Drawing.Size(598, 150);
            this.tableLayoutPanelCoordinates.TabIndex = 5;
            this.tableLayoutPanelCoordinates.Tag = "6";
            // 
            // textBoxUserCoordinate3
            // 
            this.textBoxUserCoordinate3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxUserCoordinate3.Location = new System.Drawing.Point(385, 123);
            this.textBoxUserCoordinate3.MinimumSize = new System.Drawing.Size(150, 4);
            this.textBoxUserCoordinate3.Name = "textBoxUserCoordinate3";
            this.textBoxUserCoordinate3.Size = new System.Drawing.Size(210, 20);
            this.textBoxUserCoordinate3.TabIndex = 39;
            this.textBoxUserCoordinate3.Tag = "7";
            this.textBoxUserCoordinate3.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate2
            // 
            this.textBoxUserCoordinate2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxUserCoordinate2.Location = new System.Drawing.Point(385, 93);
            this.textBoxUserCoordinate2.MinimumSize = new System.Drawing.Size(150, 4);
            this.textBoxUserCoordinate2.Name = "textBoxUserCoordinate2";
            this.textBoxUserCoordinate2.Size = new System.Drawing.Size(210, 20);
            this.textBoxUserCoordinate2.TabIndex = 38;
            this.textBoxUserCoordinate2.Tag = "6";
            this.textBoxUserCoordinate2.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate1
            // 
            this.textBoxUserCoordinate1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxUserCoordinate1.Location = new System.Drawing.Point(385, 63);
            this.textBoxUserCoordinate1.MinimumSize = new System.Drawing.Size(150, 4);
            this.textBoxUserCoordinate1.Name = "textBoxUserCoordinate1";
            this.textBoxUserCoordinate1.Size = new System.Drawing.Size(210, 20);
            this.textBoxUserCoordinate1.TabIndex = 37;
            this.textBoxUserCoordinate1.Tag = "5";
            this.textBoxUserCoordinate1.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate0
            // 
            this.textBoxUserCoordinate0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxUserCoordinate0.Location = new System.Drawing.Point(385, 33);
            this.textBoxUserCoordinate0.MinimumSize = new System.Drawing.Size(150, 4);
            this.textBoxUserCoordinate0.Name = "textBoxUserCoordinate0";
            this.textBoxUserCoordinate0.Size = new System.Drawing.Size(210, 20);
            this.textBoxUserCoordinate0.TabIndex = 36;
            this.textBoxUserCoordinate0.Tag = "4";
            this.textBoxUserCoordinate0.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxTS4231Coordinate3
            // 
            this.textBoxTS4231Coordinate3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTS4231Coordinate3.Location = new System.Drawing.Point(169, 123);
            this.textBoxTS4231Coordinate3.MinimumSize = new System.Drawing.Size(150, 4);
            this.textBoxTS4231Coordinate3.Name = "textBoxTS4231Coordinate3";
            this.textBoxTS4231Coordinate3.ReadOnly = true;
            this.textBoxTS4231Coordinate3.Size = new System.Drawing.Size(210, 20);
            this.textBoxTS4231Coordinate3.TabIndex = 33;
            this.textBoxTS4231Coordinate3.TabStop = false;
            this.textBoxTS4231Coordinate3.Tag = "3";
            // 
            // textBoxTS4231Coordinate2
            // 
            this.textBoxTS4231Coordinate2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTS4231Coordinate2.Location = new System.Drawing.Point(169, 93);
            this.textBoxTS4231Coordinate2.MinimumSize = new System.Drawing.Size(150, 4);
            this.textBoxTS4231Coordinate2.Name = "textBoxTS4231Coordinate2";
            this.textBoxTS4231Coordinate2.ReadOnly = true;
            this.textBoxTS4231Coordinate2.Size = new System.Drawing.Size(210, 20);
            this.textBoxTS4231Coordinate2.TabIndex = 32;
            this.textBoxTS4231Coordinate2.TabStop = false;
            this.textBoxTS4231Coordinate2.Tag = "2";
            // 
            // textBoxTS4231Coordinate1
            // 
            this.textBoxTS4231Coordinate1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTS4231Coordinate1.Location = new System.Drawing.Point(169, 63);
            this.textBoxTS4231Coordinate1.MinimumSize = new System.Drawing.Size(150, 4);
            this.textBoxTS4231Coordinate1.Name = "textBoxTS4231Coordinate1";
            this.textBoxTS4231Coordinate1.ReadOnly = true;
            this.textBoxTS4231Coordinate1.Size = new System.Drawing.Size(210, 20);
            this.textBoxTS4231Coordinate1.TabIndex = 31;
            this.textBoxTS4231Coordinate1.TabStop = false;
            this.textBoxTS4231Coordinate1.Tag = "1";
            // 
            // buttonMeasure3
            // 
            this.buttonMeasure3.Location = new System.Drawing.Point(83, 123);
            this.buttonMeasure3.Name = "buttonMeasure3";
            this.buttonMeasure3.Size = new System.Drawing.Size(80, 24);
            this.buttonMeasure3.TabIndex = 29;
            this.buttonMeasure3.Tag = "3";
            this.buttonMeasure3.Text = "Measure";
            this.buttonMeasure3.UseVisualStyleBackColor = true;
            this.buttonMeasure3.Click += new System.EventHandler(this.ButtonMeasure_Click);
            // 
            // buttonMeasure2
            // 
            this.buttonMeasure2.Location = new System.Drawing.Point(83, 93);
            this.buttonMeasure2.Name = "buttonMeasure2";
            this.buttonMeasure2.Size = new System.Drawing.Size(80, 24);
            this.buttonMeasure2.TabIndex = 26;
            this.buttonMeasure2.Tag = "2";
            this.buttonMeasure2.Text = "Measure";
            this.buttonMeasure2.UseVisualStyleBackColor = true;
            this.buttonMeasure2.Click += new System.EventHandler(this.ButtonMeasure_Click);
            // 
            // buttonMeasure1
            // 
            this.buttonMeasure1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonMeasure1.Location = new System.Drawing.Point(83, 63);
            this.buttonMeasure1.Name = "buttonMeasure1";
            this.buttonMeasure1.Size = new System.Drawing.Size(80, 24);
            this.buttonMeasure1.TabIndex = 23;
            this.buttonMeasure1.Tag = "1";
            this.buttonMeasure1.Text = "Measure";
            this.buttonMeasure1.UseVisualStyleBackColor = true;
            this.buttonMeasure1.Click += new System.EventHandler(this.ButtonMeasure_Click);
            // 
            // labelCoordinate3
            // 
            this.labelCoordinate3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCoordinate3.Location = new System.Drawing.Point(3, 120);
            this.labelCoordinate3.Name = "labelCoordinate3";
            this.labelCoordinate3.Size = new System.Drawing.Size(74, 30);
            this.labelCoordinate3.TabIndex = 18;
            this.labelCoordinate3.Text = "Coordinate 3:";
            this.labelCoordinate3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCoordinate2
            // 
            this.labelCoordinate2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCoordinate2.Location = new System.Drawing.Point(3, 90);
            this.labelCoordinate2.Name = "labelCoordinate2";
            this.labelCoordinate2.Size = new System.Drawing.Size(74, 30);
            this.labelCoordinate2.TabIndex = 16;
            this.labelCoordinate2.Text = "Coordinate 2:";
            this.labelCoordinate2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCoordinate1
            // 
            this.labelCoordinate1.Location = new System.Drawing.Point(3, 60);
            this.labelCoordinate1.Name = "labelCoordinate1";
            this.labelCoordinate1.Size = new System.Drawing.Size(74, 30);
            this.labelCoordinate1.TabIndex = 10;
            this.labelCoordinate1.Text = "Coordinate 1:";
            this.labelCoordinate1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonMeasure0
            // 
            this.buttonMeasure0.Location = new System.Drawing.Point(83, 33);
            this.buttonMeasure0.Name = "buttonMeasure0";
            this.buttonMeasure0.Size = new System.Drawing.Size(80, 24);
            this.buttonMeasure0.TabIndex = 1;
            this.buttonMeasure0.Tag = "0";
            this.buttonMeasure0.Text = "Measure";
            this.buttonMeasure0.UseVisualStyleBackColor = true;
            this.buttonMeasure0.Click += new System.EventHandler(this.ButtonMeasure_Click);
            // 
            // labelHeaderTS4231
            // 
            this.tableLayoutPanelCoordinates.SetColumnSpan(this.labelHeaderTS4231, 2);
            this.labelHeaderTS4231.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHeaderTS4231.Location = new System.Drawing.Point(80, 0);
            this.labelHeaderTS4231.Margin = new System.Windows.Forms.Padding(0);
            this.labelHeaderTS4231.Name = "labelHeaderTS4231";
            this.labelHeaderTS4231.Size = new System.Drawing.Size(302, 30);
            this.labelHeaderTS4231.TabIndex = 0;
            this.labelHeaderTS4231.Text = "Naive TS4231 Coordinates";
            this.labelHeaderTS4231.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCoordinate0
            // 
            this.labelCoordinate0.Location = new System.Drawing.Point(3, 30);
            this.labelCoordinate0.Name = "labelCoordinate0";
            this.labelCoordinate0.Size = new System.Drawing.Size(74, 30);
            this.labelCoordinate0.TabIndex = 2;
            this.labelCoordinate0.Text = "Coordinate 0:";
            this.labelCoordinate0.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxTS4231Coordinate0
            // 
            this.textBoxTS4231Coordinate0.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTS4231Coordinate0.Location = new System.Drawing.Point(169, 33);
            this.textBoxTS4231Coordinate0.MinimumSize = new System.Drawing.Size(150, 4);
            this.textBoxTS4231Coordinate0.Name = "textBoxTS4231Coordinate0";
            this.textBoxTS4231Coordinate0.ReadOnly = true;
            this.textBoxTS4231Coordinate0.Size = new System.Drawing.Size(210, 20);
            this.textBoxTS4231Coordinate0.TabIndex = 30;
            this.textBoxTS4231Coordinate0.TabStop = false;
            this.textBoxTS4231Coordinate0.Tag = "0";
            // 
            // labelHeaderUser
            // 
            this.labelHeaderUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHeaderUser.Location = new System.Drawing.Point(385, 0);
            this.labelHeaderUser.MinimumSize = new System.Drawing.Size(150, 0);
            this.labelHeaderUser.Name = "labelHeaderUser";
            this.labelHeaderUser.Size = new System.Drawing.Size(210, 30);
            this.labelHeaderUser.TabIndex = 34;
            this.labelHeaderUser.Text = "User-Defined Coordinates";
            this.labelHeaderUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonCalculate
            // 
            this.buttonCalculate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonCalculate.Enabled = false;
            this.buttonCalculate.Location = new System.Drawing.Point(3, 577);
            this.buttonCalculate.Name = "buttonCalculate";
            this.buttonCalculate.Size = new System.Drawing.Size(598, 23);
            this.buttonCalculate.TabIndex = 7;
            this.buttonCalculate.Text = "Calculate Spatial Transform";
            this.buttonCalculate.UseVisualStyleBackColor = true;
            this.buttonCalculate.Click += new System.EventHandler(this.ButtonCalculate_Click);
            // 
            // flowLayoutPanelBottom
            // 
            this.flowLayoutPanelBottom.AutoSize = true;
            this.flowLayoutPanelBottom.Controls.Add(this.buttonCancel);
            this.flowLayoutPanelBottom.Controls.Add(this.buttonOK);
            this.flowLayoutPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelBottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanelBottom.Location = new System.Drawing.Point(3, 606);
            this.flowLayoutPanelBottom.Name = "flowLayoutPanelBottom";
            this.flowLayoutPanelBottom.Size = new System.Drawing.Size(598, 30);
            this.flowLayoutPanelBottom.TabIndex = 8;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(515, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(80, 24);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonOKOrCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(429, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(80, 24);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOKOrCancel_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelTS4231,
            this.toolStripStatusLabelUser});
            this.statusStrip.Location = new System.Drawing.Point(0, 639);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.ShowItemToolTips = true;
            this.statusStrip.Size = new System.Drawing.Size(604, 22);
            this.statusStrip.TabIndex = 8;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabelTS4231
            // 
            this.toolStripStatusLabelTS4231.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusBlockedImage;
            this.toolStripStatusLabelTS4231.Name = "toolStripStatusLabelTS4231";
            this.toolStripStatusLabelTS4231.Size = new System.Drawing.Size(237, 17);
            this.toolStripStatusLabelTS4231.Text = "At least one TS4231 coordinate is invalid.";
            this.toolStripStatusLabelTS4231.ToolTipText = "All four TS4231 coordinates must be measured before the spatial transform matrix " +
    "can be calculated.";
            // 
            // toolStripStatusLabelUser
            // 
            this.toolStripStatusLabelUser.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusBlockedImage;
            this.toolStripStatusLabelUser.Name = "toolStripStatusLabelUser";
            this.toolStripStatusLabelUser.Size = new System.Drawing.Size(267, 17);
            this.toolStripStatusLabelUser.Text = "At least one user-defined coordinate is invalid.";
            this.toolStripStatusLabelUser.ToolTipText = resources.GetString("toolStripStatusLabelUser.ToolTipText");
            // 
            // SpatialTransformMatrixDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 661);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(620, 700);
            this.Name = "SpatialTransformMatrixDialog";
            this.Text = "TS4231V1 Calibration GUI";
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            this.groupBoxStatus.ResumeLayout(false);
            this.groupBoxStatus.PerformLayout();
            this.tableLayoutPanelCoordinates.ResumeLayout(false);
            this.tableLayoutPanelCoordinates.PerformLayout();
            this.flowLayoutPanelBottom.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.GroupBox groupBoxStatus;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCoordinates;
        private System.Windows.Forms.TextBox textBoxUserCoordinate3;
        private System.Windows.Forms.TextBox textBoxUserCoordinate2;
        private System.Windows.Forms.TextBox textBoxUserCoordinate1;
        private System.Windows.Forms.TextBox textBoxUserCoordinate0;
        private System.Windows.Forms.TextBox textBoxTS4231Coordinate3;
        private System.Windows.Forms.TextBox textBoxTS4231Coordinate2;
        private System.Windows.Forms.TextBox textBoxTS4231Coordinate1;
        private System.Windows.Forms.Button buttonMeasure3;
        private System.Windows.Forms.Button buttonMeasure2;
        private System.Windows.Forms.Button buttonMeasure1;
        private System.Windows.Forms.Label labelCoordinate3;
        private System.Windows.Forms.Label labelCoordinate2;
        private System.Windows.Forms.Label labelCoordinate1;
        private System.Windows.Forms.Button buttonMeasure0;
        private System.Windows.Forms.Label labelHeaderTS4231;
        private System.Windows.Forms.Label labelCoordinate0;
        private System.Windows.Forms.TextBox textBoxTS4231Coordinate0;
        private System.Windows.Forms.Label labelHeaderUser;
        private System.Windows.Forms.Button buttonCalculate;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelBottom;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelInstructions;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelUser;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTS4231;
    }
}
