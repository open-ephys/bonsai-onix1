using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureRhs2116Trigger"/>.
    /// </summary>
    public class Rhs2116StimulusSequenceEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureRhs2116Trigger configureNode)
                {
                    using var editorDialog = new Rhs2116StimulusSequenceDialog(configureNode.StimulusSequence, configureNode.ProbeGroup);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNode.StimulusSequence = editorDialog.Sequence;
                        configureNode.ProbeGroup = (Rhs2116ProbeGroup)editorDialog.ChannelDialog.ProbeGroup;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
