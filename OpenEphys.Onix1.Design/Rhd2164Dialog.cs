namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a dialog for a <see cref="ConfigureRhd2164"/>.
    /// </summary>
    public partial class Rhd2164Dialog : GenericDeviceDialog
    {
        internal ConfigureRhd2164 ConfigureNode
        {
            get => (ConfigureRhd2164)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rhd2164Dialog"/>.
        /// </summary>
        /// <param name="configureRhd2164">Existing <see cref="ConfigureRhd2164"/> device configuration.</param>
        public Rhd2164Dialog(ConfigureRhd2164 configureRhd2164)
        {
            InitializeComponent();

            ConfigureNode = new(configureRhd2164);
        }
    }
}
