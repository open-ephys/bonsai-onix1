using System;
using System.ComponentModel;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigurePolledBno055"/>.
    /// </summary>
    public partial class PolledBno055Dialog : GenericDeviceDialog
    {
        /// <summary>
        /// Gets or sets the <see cref="ConfigurePolledBno055"/> object attached to
        /// the property grid.
        /// </summary>
        internal InternalPolledBno055 Bno055 { get; set; }

        /// <summary>
        /// Initializes a new <see cref="PolledBno055Dialog"/> instance with the given
        /// <see cref="ConfigurePolledBno055"/> object.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigurePolledBno055"/> object that contains configuration settings.</param>
        /// <param name="allFieldsEditable">
        /// Boolean that is true if this <see cref="ConfigurePolledBno055"/> should be allowed to 
        /// edit the <see cref="ConfigurePolledBno055.AxisMap"/> and  <see cref="ConfigurePolledBno055.AxisSign"/>
        /// properties. It is set to false when called from headstage editors.</param>
        public PolledBno055Dialog(ConfigurePolledBno055 configureNode, bool allFieldsEditable = false)
        {
            InitializeComponent();
            Shown += FormShown;

            Bno055 = new(configureNode);

            if (allFieldsEditable)
            {
                propertyGrid.SelectedObject = Bno055.Bno055;
            }
            else
            {
                propertyGrid.SelectedObject = Bno055;
            }
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                MaximumSize = new System.Drawing.Size(0, 0);
            }
        }
    }

    internal class InternalPolledBno055
    {
        [TypeConverter(typeof(PolledBno055SingleDeviceFactoryConverter))]
        [Category(DeviceFactory.ConfigurationCategory)]
        public ConfigurePolledBno055 Bno055 { get; set; }

        public InternalPolledBno055(ConfigurePolledBno055 polledBno)
        {
            Bno055 = polledBno;
        }

        [Browsable(false)]
        public bool Enable { get => Bno055.Enable; }

        [Browsable(false)]
        public string DeviceName { get => Bno055.DeviceName; }

        [Browsable(false)]
        public uint DeviceAddress { get => Bno055.DeviceAddress; }
    }
}
