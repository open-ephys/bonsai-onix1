using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstage64OpticalStimulator"/>.
    /// </summary>
    public class Headstage64OpticalStimulatorSequenceEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstage64OpticalStimulator configureNode)
                {
                    using var editorDialog = new Headstage64OpticalStimulatorSequenceDialog(configureNode.StimulusSequence);

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
