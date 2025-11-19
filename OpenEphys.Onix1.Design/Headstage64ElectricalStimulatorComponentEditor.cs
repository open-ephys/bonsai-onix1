using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <inheritdoc/>
    internal class Headstage64ElectricalStimulatorComponentEditor : WorkflowComponentEditor
    {
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstage64ElectricalStimulator configureNode)
                {
                    var configuration = new ConfigureHeadstage64ElectricalStimulator(configureNode);

                    using var editorDialog = new Headstage64ElectricalStimulatorSequenceDialog(configuration);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties(editorDialog.ElectricalStimulator, configureNode);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
