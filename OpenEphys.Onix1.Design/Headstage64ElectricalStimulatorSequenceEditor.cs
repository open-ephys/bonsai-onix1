using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstage64ElectricalStimulator"/>.
    /// </summary>
    public class Headstage64ElectricalStimulatorSequenceEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstage64ElectricalStimulator configureNode)
                {
                    using var editorDialog = new Headstage64ElectricalStimulatorSequenceDialog(configureNode.StimulusSequence);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNode.StimulusSequence = editorDialog.Sequence;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
