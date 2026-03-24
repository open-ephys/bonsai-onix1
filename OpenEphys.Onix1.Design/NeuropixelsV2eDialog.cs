using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Partial class to create a GUI for <see cref="ConfigureNeuropixelsV2e"/>.
    /// </summary>
    public partial class NeuropixelsV2eDialog : Form
    {
        internal event EventHandler OnStateChange;

        internal readonly Dictionary<NeuropixelsV2Probe, NeuropixelsV2eProbeConfigurationDialog> ProbeConfigurationDialogs;

        internal bool HasChanges => ProbeConfigurationDialogs.Values.Any(dialog => dialog.HasChanges);

        internal NeuropixelsV2ProbeConfiguration ProbeConfigurationA
        {
            get
            {
                return ProbeConfigurationDialogs.TryGetValue(NeuropixelsV2Probe.ProbeA, out var probeConfigurationDialog)
                    ? probeConfigurationDialog.ProbeConfiguration
                    : throw new NullReferenceException("Unable to find the probe configuration for Probe A.");
            }
        }

        internal NeuropixelsV2ProbeConfiguration ProbeConfigurationB
        {
            get
            {
                return ProbeConfigurationDialogs.TryGetValue(NeuropixelsV2Probe.ProbeB, out var probeConfigurationDialog)
                    ? probeConfigurationDialog.ProbeConfiguration
                    : throw new NullReferenceException("Unable to find the probe configuration for Probe B.");
            }
        }

        IConfigureNeuropixelsV2 configureNeuropixelsV2;

        internal IConfigureNeuropixelsV2 ConfigureNeuropixelsV2
        {
            get => configureNeuropixelsV2;
            set
            {
                propertyGrid.SelectedObject = value;
                configureNeuropixelsV2 = value;
            }
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

            ConfigureNeuropixelsV2 = configureNode;
            propertyGrid.PropertyValueChanged += (sender, e) =>
            {
                if (e.ChangedItem.Label == nameof(NeuropixelsV2ProbeConfiguration.ProbeInterfaceFileName))
                {
                    static void RevertFileNameValue(PropertyGrid propertyGrid, PropertyValueChangedEventArgs e)
                    {
                        var gridItem = propertyGrid.SelectedGridItem;
                        var parent = gridItem.Parent;
                        var parentType = parent.Value.GetType();
                        var propertyInfo = parentType.GetProperty(e.ChangedItem.PropertyDescriptor.Name);
                        if (propertyInfo != null && propertyInfo.CanWrite)
                        {
                            propertyInfo.SetValue(parent.Value, e.OldValue);
                            propertyGrid.Refresh();
                        }
                    }

                    var probeConfigDialog = 
                        e.ChangedItem.Parent.Label == nameof(ConfigureNeuropixelsV2e.ProbeConfigurationA)
                        ? ProbeConfigurationDialogs[NeuropixelsV2Probe.ProbeA]
                        : e.ChangedItem.Parent.Label == nameof(ConfigureNeuropixelsV2e.ProbeConfigurationB) 
                          ? ProbeConfigurationDialogs[NeuropixelsV2Probe.ProbeB]
                          : throw new NullReferenceException("Unable to find the probe configuration.");

                    if (probeConfigDialog.HasChanges && !string.IsNullOrEmpty(e.OldValue as string))
                    {
                        var result = MessageBox.Show(
                            $"Warning: Changing the filename will discard the unsaved {probeConfigDialog.ProbeName} configuration changes for \"{e.OldValue}\". Do you want to continue?",
                            "Change File Name?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.No)
                        {
                            RevertFileNameValue(sender as PropertyGrid, e);
                            return;
                        }
                    }

                    string fileName = e.ChangedItem.Value as string;
                    if (File.Exists(fileName))
                    {
                        if (!probeConfigDialog.OpenProbeInterfaceFile(fileName))
                        {
                            RevertFileNameValue(sender as PropertyGrid, e);
                        }
                    }
                    else
                    {
                        probeConfigDialog.HasChanges = true;
                    }
                }

                foreach (var dialog in ProbeConfigurationDialogs.Values)
                {
                    dialog.CheckStatus();
                }
            };

            bool isBeta = false;

            if (configureNode is ConfigureNeuropixelsV2eBeta configureV2eBeta)
            {
                Text = Text.Replace("NeuropixelsV2e ", "NeuropixelsV2eBeta ");
                isBeta = true;
            }

            ProbeConfigurationDialogs = new()
            {
                { NeuropixelsV2Probe.ProbeA, new(configureNode.ProbeConfigurationA, isBeta, nameof(NeuropixelsV2Probe.ProbeA)) },
                { NeuropixelsV2Probe.ProbeB, new(configureNode.ProbeConfigurationB, isBeta, nameof(NeuropixelsV2Probe.ProbeB)) }
            };

            foreach (var probeConfigurationDialog in ProbeConfigurationDialogs)
            {
                string probeName = probeConfigurationDialog.Key.ToString();

                probeConfigurationDialog.Value.SetChildFormProperties(this);
                probeConfigurationDialog.Value.OnStateChange += (sender, args) =>
                {
                    if (HasChanges)
                    {
                        if (!Text.EndsWith("*"))
                        {
                            Text += '*';
                        }
                    }
                    else
                    {
                        Text = Text.TrimEnd('*');
                    }

                    OnStateChange?.Invoke(this, EventArgs.Empty);
                };

                probeConfigurationDialog.Value.OnProbeConfigurationChange += (sender, args) =>
                {
                    if (sender is NeuropixelsV2eProbeConfigurationDialog dialog)
                    {
                        if (probeConfigurationDialog.Key == NeuropixelsV2Probe.ProbeA)
                        {
                            configureNeuropixelsV2.ProbeConfigurationA = dialog.ProbeConfiguration;
                        }
                        else if (probeConfigurationDialog.Key == NeuropixelsV2Probe.ProbeB)
                        {
                            configureNeuropixelsV2.ProbeConfigurationB = dialog.ProbeConfiguration;
                        }
                    }

                    propertyGrid.Refresh();
                };

                probeConfigurationDialog.Value.OnPropertyValueChanged += (sender, args) => propertyGrid.Refresh();

                probeConfigurationDialog.Value.HidePropertiesTab();

                probeConfigurationDialog.Value.PanelBorderColor = propertyGrid.ViewBorderColor;
            }
        }

        private void FormShown(object sender, EventArgs e)
        {
            if (!TopLevel)
            {
                tableLayoutPanel1.Controls.Remove(flowLayoutPanel1);
                tableLayoutPanel1.RowCount = 1;
            }

            propertyGrid.Focus();
            var rootItem = propertyGrid.SelectedGridItem;
            if (rootItem != null)
            {
                foreach (GridItem item in rootItem.Parent.GridItems)
                {
                    if (item != null && item.Value is NeuropixelsV2ProbeConfiguration)
                    {
                        propertyGrid.SelectedGridItem = item;
                        item.Select();
                        break;
                    }
                }
            }

            const int PropertyGridStartingWidth = 200;

            splitContainer.SplitterDistance = PropertyGridStartingWidth;
        }

        internal bool ProcessMenuShortcut(Keys keyData)
        {
            foreach (var probeConfigurationDialog in ProbeConfigurationDialogs)
            {
                var gridItem = propertyGrid.SelectedGridItem;

                while (gridItem != null)
                {
                    if (gridItem.Value as NeuropixelsV2ProbeConfiguration == probeConfigurationDialog.Value.ProbeConfiguration && probeConfigurationDialog.Value.ProcessMenuShortcut(keyData))
                    {
                        return true;
                    }

                    gridItem = gridItem.Parent;
                }
            }

            return false;
        }

        internal void Okay_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        void PropertyGridChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            void ShowProbeConfigurationDialog(NeuropixelsV2ProbeConfiguration probeConfiguration)
            {
                var probe = ConfigureNeuropixelsV2.ProbeConfigurationA == probeConfiguration ? NeuropixelsV2Probe.ProbeA :
                            ConfigureNeuropixelsV2.ProbeConfigurationB == probeConfiguration ? NeuropixelsV2Probe.ProbeB :
                            throw new NullReferenceException("Unable to find the probe corresponding to the selected probe configuration.");

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
                SuspendLayout();
                if (e.NewSelection.Value is NeuropixelsV2ProbeConfiguration probeConfiguration)
                {
                    ShowProbeConfigurationDialog(probeConfiguration);
                }
                else if (e.NewSelection.Parent.Value is NeuropixelsV2ProbeConfiguration parentProbeConfiguration)
                {
                    ShowProbeConfigurationDialog(parentProbeConfiguration);
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
                ResumeLayout();
            }
        }

        void DialogClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
                return;

            bool cancel = false;

            foreach (var dialog in ProbeConfigurationDialogs.Values)
            {
                dialog.Close();

                if (!dialog.IsDisposed)
                {
                    cancel = true;
                    break;
                }
            }

            e.Cancel = cancel;
        }
    }
}
