using System;
using System.ComponentModel;
using Bonsai.Design;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    internal class PolledBno055Editor : WorkflowComponentEditor
    {
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigurePolledBno055 configureNode)
                {
                    using var editorDialog = new PolledBno055Dialog(configureNode);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties(editorDialog.ConfigureNode, configureNode);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
