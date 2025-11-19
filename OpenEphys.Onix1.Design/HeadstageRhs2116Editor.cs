using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageRhs2116"/>.
    /// </summary>
    public class HeadstageRhs2116Editor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageRhs2116 configureNode)
                {
                    var configuration = new ConfigureHeadstageRhs2116(configureNode);

                    using var editorDialog = new HeadstageRhs2116Dialog(configuration.StimulusTrigger, configuration.Rhs2116Pair);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties(editorDialog.StimulusSequenceDialog.Trigger, configureNode.StimulusTrigger, DesignHelper.PropertiesToIgnore);

                        DesignHelper.CopyProperties((ConfigureRhs2116Pair)editorDialog.Rhs2116Dialog.Device, configureNode.Rhs2116Pair, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
