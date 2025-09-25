using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV1e"/>.
    /// </summary>
    public class NeuropixelsV1fHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV1f configureNode)
                {
                    using var editorDialog = new NeuropixelsV1fHeadstageDialog(configureNode.NeuropixelsV1A, configureNode.NeuropixelsV1B, configureNode.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNode.Bno055.Enable = editorDialog.DialogBno055.ConfigureNode.Enable;

                        DesignHelper.CopyProperties(editorDialog.DialogNeuropixelsV1A.ConfigureNode, configureNode.NeuropixelsV1A);
                        DesignHelper.CopyProperties(editorDialog.DialogNeuropixelsV1B.ConfigureNode, configureNode.NeuropixelsV1B);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
