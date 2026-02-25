using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eDialog : Form
    {
        internal readonly Dictionary<NeuropixelsV2Probe, NeuropixelsV2eProbeConfigurationDialog> ProbeConfigurationDialogs;

        internal NeuropixelsV2ProbeConfiguration ProbeConfigurationA
        {
            get
            {
                return ProbeConfigurationDialogs.TryGetValue(NeuropixelsV2Probe.ProbeA, out var probeConfigurationDialog)
                    ? probeConfigurationDialog.ProbeConfiguration
                    : throw new NullReferenceException("Unable to find the probe configuration dialog for Probe A.");
            }
        }

        internal NeuropixelsV2ProbeConfiguration ProbeConfigurationB
        {
            get
            {
                return ProbeConfigurationDialogs.TryGetValue(NeuropixelsV2Probe.ProbeB, out var probeConfigurationDialog)
                    ? probeConfigurationDialog.ProbeConfiguration
                    : throw new NullReferenceException("Unable to find the probe configuration dialog for Probe B.");
            }
        }

        /// <summary>
        /// Public <see cref="IConfigureNeuropixelsV2"/> interface that is manipulated by
        /// <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        public IConfigureNeuropixelsV2 ConfigureNode
        {
            get => (IConfigureNeuropixelsV2)propertyGrid.SelectedObject;
            set => propertyGrid.SelectedObject = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NeuropixelsV2eDialog"/>.
        /// </summary>
        /// <param name="configureNode">A <see cref="ConfigureNeuropixelsV2e"/> object holding the current configuration settings.</param>
        /// <param name="filterProperties">
        /// <see langword="true"/> if the properties should be filtered by <see cref="DeviceTablePropertyAttribute"/>,
        /// otherwise <see langword="false"/>. Default is <see langword="false"/>.
        /// </param>
        public NeuropixelsV2eDialog(IConfigureNeuropixelsV2 configureNode, bool filterProperties = false)
        {
            InitializeComponent();
            Shown += FormShown;
            FormClosing += DialogClosing;

            if (filterProperties)
            {
                propertyGrid.BrowsableAttributes = new AttributeCollection(
                    new Attribute[]
                    {
                        new BrowsableAttribute(true),
                        new DeviceTablePropertyAttribute (false)
                    });
            }

            propertyGrid.SelectedObject = configureNode;

            bool isBeta = false;

            if (configureNode is ConfigureNeuropixelsV2eBeta configureV2eBeta)
            {
                Text = Text.Replace("NeuropixelsV2e ", "NeuropixelsV2eBeta ");
                isBeta = true;
            }

            ProbeConfigurationDialogs = new()
            {
                { NeuropixelsV2Probe.ProbeA, new(configureNode.ProbeConfigurationA, isBeta) },
                { NeuropixelsV2Probe.ProbeB, new(configureNode.ProbeConfigurationB, isBeta) }
            };

            foreach (var probeConfiguration in ProbeConfigurationDialogs)
            {
                probeConfiguration.Value.SetChildFormProperties(this);
                this.AddMenuItemsFromDialogToFileOption(probeConfiguration.Value, probeConfiguration.Key.ToString());

                probeConfiguration.Value.HidePropertiesTab();
            }
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;

                menuStrip.Visible = false;
            }

            propertyGrid.Focus();
            var rootItem = propertyGrid.SelectedGridItem;
            if (rootItem != null)
            {
                foreach (GridItem item in rootItem.Parent.GridItems)
                {
                    if (item != null && item.Value is NeuropixelsV2ProbeConfiguration probeConfiguration && probeConfiguration.Probe == NeuropixelsV2Probe.ProbeA)
                    {
                        propertyGrid.SelectedGridItem = item;
                        item.Select();
                    }
                }
            }

            const int PropertyGridStartingWidth = 200;

            splitContainer.SplitterDistance = PropertyGridStartingWidth;
        }

        internal void Okay_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        void PropertyGridChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            void ShowProbeConfigurationDialog(NeuropixelsV2Probe probe)
            {
                var dialog = ProbeConfigurationDialogs[probe];

                if (!panelConfigurationDialogs.Controls.Contains(dialog))
                {
                    panelConfigurationDialogs.Controls.Add(dialog);
                    dialog.Show();
                }

                dialog.BringToFront();
            }

            // NB: If the property (or the parent's property) is a probe configuration, show the corresponding dialog.
            //     Otherwise, remove the probe configuration dialogs to show the default text box.
            if (e.NewSelection != null)
            {
                if (e.NewSelection.Value is NeuropixelsV2ProbeConfiguration probeConfiguration)
                {
                    ShowProbeConfigurationDialog(probeConfiguration.Probe);
                }
                else if (e.NewSelection.Parent.Value is NeuropixelsV2ProbeConfiguration parentProbeConfiguration)
                {
                    ShowProbeConfigurationDialog(parentProbeConfiguration.Probe);
                }
                else
                {
                    foreach (var item in panelConfigurationDialogs.Controls.OfType<NeuropixelsV2eProbeConfigurationDialog>().ToList())
                    {
                        if (item != null && e.NewSelection.Parent.Value is not NeuropixelsV2ProbeConfiguration)
                        {
                            panelConfigurationDialogs.Controls.Remove(item);
                        }
                    }
                }
            }
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            CopyConfigurations();
        }

        // NB: This will be removed when the new ProbeInterface FileName changes are implemented, but is necessary to ensure that this commit is not broken
        internal void CopyConfigurations()
        {
            ConfigureNode.ProbeConfigurationA = ProbeConfigurationA;
            ConfigureNode.ProbeConfigurationB = ProbeConfigurationB;
        }
    }
}
