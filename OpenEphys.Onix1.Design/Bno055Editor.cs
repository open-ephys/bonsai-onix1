using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureBno055"/>.
    /// </summary>
    public class Bno055Editor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureBno055 configureBno055)
                {
                    using var editorDialog = new Bno055Dialog(configureBno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureBno055.Enable = editorDialog.ConfigureNode.Enable;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
