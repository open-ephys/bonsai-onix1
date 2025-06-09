namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a dialog for a <see cref="ConfigureTS4231V1"/>.
    /// </summary>
    public partial class TS4231V1Dialog : GenericDeviceDialog
    {
        internal ConfigureTS4231V1 ConfigureNode
        {
            get => (ConfigureTS4231V1)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TS4231V1Dialog"/>.
        /// </summary>
        /// <param name="configureNode">Existing <see cref="ConfigureTS4231V1"/> device configuration.</param>
        public TS4231V1Dialog(ConfigureTS4231V1 configureNode)
        {
            InitializeComponent();
            ConfigureNode = new(configureNode);
        }
    }
}
