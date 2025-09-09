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
                        configureNode.Rhd2164 = editorDialog.Rhd2164Dialog.ConfigureNode;
                        configureNode.Bno055 = editorDialog.Bno055Dialog.ConfigureNode;
                        configureNode.TS4231 = editorDialog.TS4231V1Dialog.ConfigureNode;
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
