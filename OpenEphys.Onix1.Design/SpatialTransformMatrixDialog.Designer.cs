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
            this.richTextBoxStatus = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanelCoordinates = new System.Windows.Forms.TableLayoutPanel();
            this.labelZ = new System.Windows.Forms.Label();
            this.labelY = new System.Windows.Forms.Label();
            this.textBoxUserCoordinate3Z = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate3Y = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate3X = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate2Z = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate2Y = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate2X = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate1Z = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate1Y = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate0Z = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate0Y = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate1X = new System.Windows.Forms.TextBox();
            this.labelXyz = new System.Windows.Forms.Label();
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
            this.labelTS4231 = new System.Windows.Forms.Label();
            this.labelCoordinate0 = new System.Windows.Forms.Label();
            this.textBoxTS4231Coordinate0 = new System.Windows.Forms.TextBox();
            this.textBoxUserCoordinate0X = new System.Windows.Forms.TextBox();
            this.labelUser = new System.Windows.Forms.Label();
            this.labelX = new System.Windows.Forms.Label();
            this.flowLayoutPanelBottom = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.tableLayoutPanelSpatialMatrix = new System.Windows.Forms.TableLayoutPanel();
            this.labelSpatialMatrix = new System.Windows.Forms.Label();
            this.textBoxSpatialTransformMatrix = new System.Windows.Forms.TextBox();
            this.richTextBoxInstructions = new System.Windows.Forms.RichTextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanelMain.SuspendLayout();
            this.groupBoxStatus.SuspendLayout();
            this.tableLayoutPanelCoordinates.SuspendLayout();
            this.flowLayoutPanelBottom.SuspendLayout();
            this.tableLayoutPanelSpatialMatrix.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 1;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Controls.Add(this.groupBoxStatus, 0, 3);
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelCoordinates, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelBottom, 0, 4);
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelSpatialMatrix, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.richTextBoxInstructions, 0, 0);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 5;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(624, 540);
            this.tableLayoutPanelMain.TabIndex = 0;
            // 
            // groupBoxStatus
            // 
            this.groupBoxStatus.Controls.Add(this.richTextBoxStatus);
            this.groupBoxStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxStatus.Location = new System.Drawing.Point(3, 406);
            this.groupBoxStatus.Name = "groupBoxStatus";
            this.groupBoxStatus.Size = new System.Drawing.Size(618, 95);
            this.groupBoxStatus.TabIndex = 1000;
            this.groupBoxStatus.TabStop = false;
            this.groupBoxStatus.Text = "Status Messages";
            // 
            // richTextBoxStatus
            // 
            this.richTextBoxStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxStatus.Location = new System.Drawing.Point(3, 16);
            this.richTextBoxStatus.Name = "richTextBoxStatus";
            this.richTextBoxStatus.ReadOnly = true;
            this.richTextBoxStatus.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBoxStatus.Size = new System.Drawing.Size(612, 76);
            this.richTextBoxStatus.TabIndex = 1000;
            this.richTextBoxStatus.TabStop = false;
            this.richTextBoxStatus.Text = "";
            // 
            // tableLayoutPanelCoordinates
            // 
            this.tableLayoutPanelCoordinates.ColumnCount = 6;
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanelCoordinates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelZ, 6, 1);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelY, 4, 1);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate3Z, 5, 5);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate3Y, 4, 5);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate3X, 3, 5);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate2Z, 5, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate2Y, 4, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate2X, 3, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate1Z, 5, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate1Y, 4, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate0Z, 5, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate0Y, 4, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate1X, 3, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelXyz, 2, 1);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxTS4231Coordinate3, 2, 5);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxTS4231Coordinate2, 2, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxTS4231Coordinate1, 2, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.buttonMeasure3, 1, 5);
            this.tableLayoutPanelCoordinates.Controls.Add(this.buttonMeasure2, 1, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.buttonMeasure1, 1, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelCoordinate3, 0, 5);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelCoordinate2, 0, 4);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelCoordinate1, 0, 3);
            this.tableLayoutPanelCoordinates.Controls.Add(this.buttonMeasure0, 1, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelTS4231, 1, 0);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelCoordinate0, 0, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxTS4231Coordinate0, 2, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.textBoxUserCoordinate0X, 3, 2);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelUser, 3, 0);
            this.tableLayoutPanelCoordinates.Controls.Add(this.labelX, 3, 1);
            this.tableLayoutPanelCoordinates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelCoordinates.Location = new System.Drawing.Point(3, 154);
            this.tableLayoutPanelCoordinates.Name = "tableLayoutPanelCoordinates";
            this.tableLayoutPanelCoordinates.RowCount = 6;
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelCoordinates.Size = new System.Drawing.Size(618, 180);
            this.tableLayoutPanelCoordinates.TabIndex = 0;
            this.tableLayoutPanelCoordinates.Tag = "0";
            // 
            // labelZ
            // 
            this.labelZ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelZ.Location = new System.Drawing.Point(526, 30);
            this.labelZ.Margin = new System.Windows.Forms.Padding(0);
            this.labelZ.Name = "labelZ";
            this.labelZ.Size = new System.Drawing.Size(92, 30);
            this.labelZ.TabIndex = 1000;
            this.labelZ.Text = "Z";
            this.labelZ.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelY
            // 
            this.labelY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelY.Location = new System.Drawing.Point(436, 30);
            this.labelY.Margin = new System.Windows.Forms.Padding(0);
            this.labelY.Name = "labelY";
            this.labelY.Size = new System.Drawing.Size(90, 30);
            this.labelY.TabIndex = 1000;
            this.labelY.Text = "Y";
            this.labelY.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxUserCoordinate3Z
            // 
            this.textBoxUserCoordinate3Z.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate3Z.Location = new System.Drawing.Point(529, 157);
            this.textBoxUserCoordinate3Z.Name = "textBoxUserCoordinate3Z";
            this.textBoxUserCoordinate3Z.Size = new System.Drawing.Size(86, 20);
            this.textBoxUserCoordinate3Z.TabIndex = 15;
            this.textBoxUserCoordinate3Z.Tag = "11";
            this.textBoxUserCoordinate3Z.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate3Y
            // 
            this.textBoxUserCoordinate3Y.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate3Y.Location = new System.Drawing.Point(439, 157);
            this.textBoxUserCoordinate3Y.Name = "textBoxUserCoordinate3Y";
            this.textBoxUserCoordinate3Y.Size = new System.Drawing.Size(84, 20);
            this.textBoxUserCoordinate3Y.TabIndex = 14;
            this.textBoxUserCoordinate3Y.Tag = "10";
            this.textBoxUserCoordinate3Y.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate3X
            // 
            this.textBoxUserCoordinate3X.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate3X.Location = new System.Drawing.Point(349, 157);
            this.textBoxUserCoordinate3X.Name = "textBoxUserCoordinate3X";
            this.textBoxUserCoordinate3X.Size = new System.Drawing.Size(84, 20);
            this.textBoxUserCoordinate3X.TabIndex = 13;
            this.textBoxUserCoordinate3X.Tag = "9";
            this.textBoxUserCoordinate3X.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate2Z
            // 
            this.textBoxUserCoordinate2Z.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate2Z.Location = new System.Drawing.Point(529, 127);
            this.textBoxUserCoordinate2Z.Name = "textBoxUserCoordinate2Z";
            this.textBoxUserCoordinate2Z.Size = new System.Drawing.Size(86, 20);
            this.textBoxUserCoordinate2Z.TabIndex = 11;
            this.textBoxUserCoordinate2Z.Tag = "8";
            this.textBoxUserCoordinate2Z.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate2Y
            // 
            this.textBoxUserCoordinate2Y.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate2Y.Location = new System.Drawing.Point(439, 127);
            this.textBoxUserCoordinate2Y.Name = "textBoxUserCoordinate2Y";
            this.textBoxUserCoordinate2Y.Size = new System.Drawing.Size(84, 20);
            this.textBoxUserCoordinate2Y.TabIndex = 10;
            this.textBoxUserCoordinate2Y.Tag = "7";
            this.textBoxUserCoordinate2Y.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate2X
            // 
            this.textBoxUserCoordinate2X.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate2X.Location = new System.Drawing.Point(349, 127);
            this.textBoxUserCoordinate2X.Name = "textBoxUserCoordinate2X";
            this.textBoxUserCoordinate2X.Size = new System.Drawing.Size(84, 20);
            this.textBoxUserCoordinate2X.TabIndex = 9;
            this.textBoxUserCoordinate2X.Tag = "6";
            this.textBoxUserCoordinate2X.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate1Z
            // 
            this.textBoxUserCoordinate1Z.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate1Z.Location = new System.Drawing.Point(529, 97);
            this.textBoxUserCoordinate1Z.Name = "textBoxUserCoordinate1Z";
            this.textBoxUserCoordinate1Z.Size = new System.Drawing.Size(86, 20);
            this.textBoxUserCoordinate1Z.TabIndex = 7;
            this.textBoxUserCoordinate1Z.Tag = "5";
            this.textBoxUserCoordinate1Z.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate1Y
            // 
            this.textBoxUserCoordinate1Y.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate1Y.Location = new System.Drawing.Point(439, 97);
            this.textBoxUserCoordinate1Y.Name = "textBoxUserCoordinate1Y";
            this.textBoxUserCoordinate1Y.Size = new System.Drawing.Size(84, 20);
            this.textBoxUserCoordinate1Y.TabIndex = 6;
            this.textBoxUserCoordinate1Y.Tag = "4";
            this.textBoxUserCoordinate1Y.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate0Z
            // 
            this.textBoxUserCoordinate0Z.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate0Z.Location = new System.Drawing.Point(529, 67);
            this.textBoxUserCoordinate0Z.Name = "textBoxUserCoordinate0Z";
            this.textBoxUserCoordinate0Z.Size = new System.Drawing.Size(86, 20);
            this.textBoxUserCoordinate0Z.TabIndex = 3;
            this.textBoxUserCoordinate0Z.Tag = "2";
            this.textBoxUserCoordinate0Z.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate0Y
            // 
            this.textBoxUserCoordinate0Y.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate0Y.Location = new System.Drawing.Point(439, 67);
            this.textBoxUserCoordinate0Y.Name = "textBoxUserCoordinate0Y";
            this.textBoxUserCoordinate0Y.Size = new System.Drawing.Size(84, 20);
            this.textBoxUserCoordinate0Y.TabIndex = 2;
            this.textBoxUserCoordinate0Y.Tag = "1";
            this.textBoxUserCoordinate0Y.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // textBoxUserCoordinate1X
            // 
            this.textBoxUserCoordinate1X.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate1X.Location = new System.Drawing.Point(349, 97);
            this.textBoxUserCoordinate1X.Name = "textBoxUserCoordinate1X";
            this.textBoxUserCoordinate1X.Size = new System.Drawing.Size(84, 20);
            this.textBoxUserCoordinate1X.TabIndex = 5;
            this.textBoxUserCoordinate1X.Tag = "3";
            this.textBoxUserCoordinate1X.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // labelXyz
            // 
            this.labelXyz.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelXyz.Location = new System.Drawing.Point(166, 30);
            this.labelXyz.Margin = new System.Windows.Forms.Padding(0);
            this.labelXyz.Name = "labelXyz";
            this.labelXyz.Size = new System.Drawing.Size(180, 30);
            this.labelXyz.TabIndex = 1000;
            this.labelXyz.Text = "X, Y, Z";
            this.labelXyz.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxTS4231Coordinate3
            // 
            this.textBoxTS4231Coordinate3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxTS4231Coordinate3.Location = new System.Drawing.Point(169, 157);
            this.textBoxTS4231Coordinate3.Name = "textBoxTS4231Coordinate3";
            this.textBoxTS4231Coordinate3.ReadOnly = true;
            this.textBoxTS4231Coordinate3.Size = new System.Drawing.Size(174, 20);
            this.textBoxTS4231Coordinate3.TabIndex = 1000;
            this.textBoxTS4231Coordinate3.TabStop = false;
            this.textBoxTS4231Coordinate3.Tag = "3";
            // 
            // textBoxTS4231Coordinate2
            // 
            this.textBoxTS4231Coordinate2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxTS4231Coordinate2.Location = new System.Drawing.Point(169, 127);
            this.textBoxTS4231Coordinate2.Name = "textBoxTS4231Coordinate2";
            this.textBoxTS4231Coordinate2.ReadOnly = true;
            this.textBoxTS4231Coordinate2.Size = new System.Drawing.Size(174, 20);
            this.textBoxTS4231Coordinate2.TabIndex = 1000;
            this.textBoxTS4231Coordinate2.TabStop = false;
            this.textBoxTS4231Coordinate2.Tag = "2";
            // 
            // textBoxTS4231Coordinate1
            // 
            this.textBoxTS4231Coordinate1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxTS4231Coordinate1.Location = new System.Drawing.Point(169, 97);
            this.textBoxTS4231Coordinate1.Name = "textBoxTS4231Coordinate1";
            this.textBoxTS4231Coordinate1.ReadOnly = true;
            this.textBoxTS4231Coordinate1.Size = new System.Drawing.Size(174, 20);
            this.textBoxTS4231Coordinate1.TabIndex = 1000;
            this.textBoxTS4231Coordinate1.TabStop = false;
            this.textBoxTS4231Coordinate1.Tag = "1";
            // 
            // buttonMeasure3
            // 
            this.buttonMeasure3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonMeasure3.Location = new System.Drawing.Point(83, 153);
            this.buttonMeasure3.Name = "buttonMeasure3";
            this.buttonMeasure3.Size = new System.Drawing.Size(80, 24);
            this.buttonMeasure3.TabIndex = 12;
            this.buttonMeasure3.Tag = "3";
            this.buttonMeasure3.Text = "Measure";
            this.buttonMeasure3.UseVisualStyleBackColor = true;
            this.buttonMeasure3.Click += new System.EventHandler(this.ButtonMeasure_Click);
            // 
            // buttonMeasure2
            // 
            this.buttonMeasure2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonMeasure2.Location = new System.Drawing.Point(83, 123);
            this.buttonMeasure2.Name = "buttonMeasure2";
            this.buttonMeasure2.Size = new System.Drawing.Size(80, 24);
            this.buttonMeasure2.TabIndex = 8;
            this.buttonMeasure2.Tag = "2";
            this.buttonMeasure2.Text = "Measure";
            this.buttonMeasure2.UseVisualStyleBackColor = true;
            this.buttonMeasure2.Click += new System.EventHandler(this.ButtonMeasure_Click);
            // 
            // buttonMeasure1
            // 
            this.buttonMeasure1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonMeasure1.Location = new System.Drawing.Point(83, 93);
            this.buttonMeasure1.Name = "buttonMeasure1";
            this.buttonMeasure1.Size = new System.Drawing.Size(80, 24);
            this.buttonMeasure1.TabIndex = 4;
            this.buttonMeasure1.Tag = "1";
            this.buttonMeasure1.Text = "Measure";
            this.buttonMeasure1.UseVisualStyleBackColor = true;
            this.buttonMeasure1.Click += new System.EventHandler(this.ButtonMeasure_Click);
            // 
            // labelCoordinate3
            // 
            this.labelCoordinate3.Location = new System.Drawing.Point(3, 150);
            this.labelCoordinate3.Name = "labelCoordinate3";
            this.labelCoordinate3.Size = new System.Drawing.Size(74, 30);
            this.labelCoordinate3.TabIndex = 1000;
            this.labelCoordinate3.Text = "Coordinate 3:";
            this.labelCoordinate3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCoordinate2
            // 
            this.labelCoordinate2.Location = new System.Drawing.Point(3, 120);
            this.labelCoordinate2.Name = "labelCoordinate2";
            this.labelCoordinate2.Size = new System.Drawing.Size(74, 30);
            this.labelCoordinate2.TabIndex = 1000;
            this.labelCoordinate2.Text = "Coordinate 2:";
            this.labelCoordinate2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCoordinate1
            // 
            this.labelCoordinate1.Location = new System.Drawing.Point(3, 90);
            this.labelCoordinate1.Name = "labelCoordinate1";
            this.labelCoordinate1.Size = new System.Drawing.Size(74, 30);
            this.labelCoordinate1.TabIndex = 1000;
            this.labelCoordinate1.Text = "Coordinate 1:";
            this.labelCoordinate1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonMeasure0
            // 
            this.buttonMeasure0.Location = new System.Drawing.Point(83, 63);
            this.buttonMeasure0.Name = "buttonMeasure0";
            this.buttonMeasure0.Size = new System.Drawing.Size(80, 24);
            this.buttonMeasure0.TabIndex = 0;
            this.buttonMeasure0.Tag = "0";
            this.buttonMeasure0.Text = "Measure";
            this.buttonMeasure0.UseVisualStyleBackColor = true;
            this.buttonMeasure0.Click += new System.EventHandler(this.ButtonMeasure_Click);
            // 
            // labelTS4231
            // 
            this.labelTS4231.AutoSize = true;
            this.tableLayoutPanelCoordinates.SetColumnSpan(this.labelTS4231, 2);
            this.labelTS4231.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTS4231.Location = new System.Drawing.Point(80, 0);
            this.labelTS4231.Margin = new System.Windows.Forms.Padding(0);
            this.labelTS4231.Name = "labelTS4231";
            this.labelTS4231.Size = new System.Drawing.Size(266, 30);
            this.labelTS4231.TabIndex = 1000;
            this.labelTS4231.Text = "TS4231 Coordinates";
            this.labelTS4231.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCoordinate0
            // 
            this.labelCoordinate0.Location = new System.Drawing.Point(3, 60);
            this.labelCoordinate0.Name = "labelCoordinate0";
            this.labelCoordinate0.Size = new System.Drawing.Size(74, 30);
            this.labelCoordinate0.TabIndex = 1000;
            this.labelCoordinate0.Text = "Coordinate 0:";
            this.labelCoordinate0.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxTS4231Coordinate0
            // 
            this.textBoxTS4231Coordinate0.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxTS4231Coordinate0.Location = new System.Drawing.Point(169, 67);
            this.textBoxTS4231Coordinate0.Name = "textBoxTS4231Coordinate0";
            this.textBoxTS4231Coordinate0.ReadOnly = true;
            this.textBoxTS4231Coordinate0.Size = new System.Drawing.Size(174, 20);
            this.textBoxTS4231Coordinate0.TabIndex = 1000;
            this.textBoxTS4231Coordinate0.TabStop = false;
            this.textBoxTS4231Coordinate0.Tag = "0";
            // 
            // textBoxUserCoordinate0X
            // 
            this.textBoxUserCoordinate0X.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxUserCoordinate0X.Location = new System.Drawing.Point(349, 67);
            this.textBoxUserCoordinate0X.Name = "textBoxUserCoordinate0X";
            this.textBoxUserCoordinate0X.Size = new System.Drawing.Size(84, 20);
            this.textBoxUserCoordinate0X.TabIndex = 1;
            this.textBoxUserCoordinate0X.Tag = "0";
            this.textBoxUserCoordinate0X.TextChanged += new System.EventHandler(this.TextBoxUserCoordinate_TextChanged);
            // 
            // labelUser
            // 
            this.tableLayoutPanelCoordinates.SetColumnSpan(this.labelUser, 3);
            this.labelUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelUser.Location = new System.Drawing.Point(346, 0);
            this.labelUser.Margin = new System.Windows.Forms.Padding(0);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(272, 30);
            this.labelUser.TabIndex = 1000;
            this.labelUser.Text = "User Coordinates";
            this.labelUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelX
            // 
            this.labelX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelX.Location = new System.Drawing.Point(349, 30);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(84, 30);
            this.labelX.TabIndex = 1000;
            this.labelX.Text = "X";
            this.labelX.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanelBottom
            // 
            this.flowLayoutPanelBottom.AutoSize = true;
            this.flowLayoutPanelBottom.Controls.Add(this.buttonCancel);
            this.flowLayoutPanelBottom.Controls.Add(this.buttonOK);
            this.flowLayoutPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelBottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanelBottom.Location = new System.Drawing.Point(3, 507);
            this.flowLayoutPanelBottom.Name = "flowLayoutPanelBottom";
            this.flowLayoutPanelBottom.Size = new System.Drawing.Size(618, 30);
            this.flowLayoutPanelBottom.TabIndex = 1;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(535, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(80, 24);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Tag = "5";
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(449, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(80, 24);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Tag = "4";
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // tableLayoutPanelSpatialMatrix
            // 
            this.tableLayoutPanelSpatialMatrix.AutoSize = true;
            this.tableLayoutPanelSpatialMatrix.ColumnCount = 2;
            this.tableLayoutPanelSpatialMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSpatialMatrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSpatialMatrix.Controls.Add(this.labelSpatialMatrix, 0, 0);
            this.tableLayoutPanelSpatialMatrix.Controls.Add(this.textBoxSpatialTransformMatrix, 1, 0);
            this.tableLayoutPanelSpatialMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSpatialMatrix.Location = new System.Drawing.Point(3, 340);
            this.tableLayoutPanelSpatialMatrix.Name = "tableLayoutPanelSpatialMatrix";
            this.tableLayoutPanelSpatialMatrix.RowCount = 1;
            this.tableLayoutPanelSpatialMatrix.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanelSpatialMatrix.Size = new System.Drawing.Size(618, 60);
            this.tableLayoutPanelSpatialMatrix.TabIndex = 0;
            // 
            // labelSpatialMatrix
            // 
            this.labelSpatialMatrix.AutoSize = true;
            this.labelSpatialMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSpatialMatrix.Location = new System.Drawing.Point(3, 0);
            this.labelSpatialMatrix.Name = "labelSpatialMatrix";
            this.labelSpatialMatrix.Size = new System.Drawing.Size(123, 60);
            this.labelSpatialMatrix.TabIndex = 1000;
            this.labelSpatialMatrix.Text = "Spatial Transform Matrix:";
            // 
            // textBoxSpatialTransformMatrix
            // 
            this.textBoxSpatialTransformMatrix.AcceptsReturn = true;
            this.textBoxSpatialTransformMatrix.AcceptsTab = true;
            this.textBoxSpatialTransformMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSpatialTransformMatrix.Location = new System.Drawing.Point(132, 3);
            this.textBoxSpatialTransformMatrix.Multiline = true;
            this.textBoxSpatialTransformMatrix.Name = "textBoxSpatialTransformMatrix";
            this.textBoxSpatialTransformMatrix.ReadOnly = true;
            this.textBoxSpatialTransformMatrix.Size = new System.Drawing.Size(483, 54);
            this.textBoxSpatialTransformMatrix.TabIndex = 1000;
            this.textBoxSpatialTransformMatrix.TabStop = false;
            // 
            // richTextBoxInstructions
            // 
            this.richTextBoxInstructions.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxInstructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxInstructions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxInstructions.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxInstructions.Name = "richTextBoxInstructions";
            this.richTextBoxInstructions.ReadOnly = true;
            this.richTextBoxInstructions.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBoxInstructions.Size = new System.Drawing.Size(618, 145);
            this.richTextBoxInstructions.TabIndex = 1000;
            this.richTextBoxInstructions.TabStop = false;
            this.richTextBoxInstructions.Text = "Placeholder text";
            this.richTextBoxInstructions.ContentsResized += new System.Windows.Forms.ContentsResizedEventHandler(this.richTextBoxInstructions_ContentsResized);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.statusStrip.Location = new System.Drawing.Point(0, 540);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.ShowItemToolTips = true;
            this.statusStrip.Size = new System.Drawing.Size(624, 21);
            this.statusStrip.TabIndex = 1000;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Image = global::OpenEphys.Onix1.Design.Properties.Resources.StatusWarningImage;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(221, 16);
            this.toolStripStatusLabel.Text = "All fields must be properly populated.";
            // 
            // SpatialTransformMatrixDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 561);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Controls.Add(this.statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(640, 600);
            this.Name = "SpatialTransformMatrixDialog";
            this.Text = "TS4231V1 Calibration GUI";
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            this.groupBoxStatus.ResumeLayout(false);
            this.tableLayoutPanelCoordinates.ResumeLayout(false);
            this.tableLayoutPanelCoordinates.PerformLayout();
            this.flowLayoutPanelBottom.ResumeLayout(false);
            this.tableLayoutPanelSpatialMatrix.ResumeLayout(false);
            this.tableLayoutPanelSpatialMatrix.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.GroupBox groupBoxStatus;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCoordinates;
        private System.Windows.Forms.TextBox textBoxTS4231Coordinate2;
        private System.Windows.Forms.TextBox textBoxTS4231Coordinate1;
        private System.Windows.Forms.Button buttonMeasure2;
        private System.Windows.Forms.Button buttonMeasure1;
        private System.Windows.Forms.Label labelCoordinate2;
        private System.Windows.Forms.Label labelCoordinate1;
        private System.Windows.Forms.Button buttonMeasure0;
        private System.Windows.Forms.Label labelTS4231;
        private System.Windows.Forms.Label labelCoordinate0;
        private System.Windows.Forms.TextBox textBoxTS4231Coordinate0;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelBottom;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.TextBox textBoxTS4231Coordinate3;
        private System.Windows.Forms.Button buttonMeasure3;
        private System.Windows.Forms.Label labelCoordinate3;
        private System.Windows.Forms.Label labelXyz;
        private System.Windows.Forms.TextBox textBoxUserCoordinate0X;
        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.TextBox textBoxUserCoordinate3Z;
        private System.Windows.Forms.TextBox textBoxUserCoordinate3Y;
        private System.Windows.Forms.TextBox textBoxUserCoordinate3X;
        private System.Windows.Forms.TextBox textBoxUserCoordinate2Z;
        private System.Windows.Forms.TextBox textBoxUserCoordinate2Y;
        private System.Windows.Forms.TextBox textBoxUserCoordinate2X;
        private System.Windows.Forms.TextBox textBoxUserCoordinate1Z;
        private System.Windows.Forms.TextBox textBoxUserCoordinate1Y;
        private System.Windows.Forms.TextBox textBoxUserCoordinate0Z;
        private System.Windows.Forms.TextBox textBoxUserCoordinate1X;
        private System.Windows.Forms.Label labelY;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.Label labelZ;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSpatialMatrix;
        private System.Windows.Forms.Label labelSpatialMatrix;
        private System.Windows.Forms.TextBox textBoxSpatialTransformMatrix;
        private System.Windows.Forms.RichTextBox richTextBoxStatus;
        private System.Windows.Forms.TextBox textBoxUserCoordinate0Y;
        private System.Windows.Forms.RichTextBox richTextBoxInstructions;
    }
}
