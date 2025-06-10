using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV1e"/>.
    /// </summary>
    public class NeuropixelsV1eHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV1e configureNode)
                {
                    using var editorDialog = new NeuropixelsV1eHeadstageDialog(configureNode.NeuropixelsV1e, configureNode.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNode.Bno055.Enable = editorDialog.DialogBno055.ConfigureNode.Enable;

                        DesignHelper.CopyProperties(editorDialog.DialogNeuropixelsV1e.ConfigureNode, configureNode.NeuropixelsV1e);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
