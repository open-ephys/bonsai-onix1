using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstage64"/>.
    /// </summary>
    internal class Headstage64Editor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstage64 configureNode)
                {
                    using var editorDialog = new Headstage64Dialog(configureNode);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties((ConfigureRhd2164)editorDialog.Rhd2164Dialog.Device, configureNode.Rhd2164, DesignHelper.PropertiesToIgnore);
                        DesignHelper.CopyProperties((ConfigureBno055)editorDialog.Bno055Dialog.Device, configureNode.Bno055, DesignHelper.PropertiesToIgnore);
                        DesignHelper.CopyProperties((ConfigureTS4231V1)editorDialog.TS4231V1Dialog.Device, configureNode.TS4231, DesignHelper.PropertiesToIgnore);
                        configureNode.ElectricalStimulator = editorDialog.ElectricalStimulatorSequenceDialog.ElectricalStimulator;
                        configureNode.OpticalStimulator = editorDialog.OpticalStimulatorSequenceDialog.OpticalStimulator;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
