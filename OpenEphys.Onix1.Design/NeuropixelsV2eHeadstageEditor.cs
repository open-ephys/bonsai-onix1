using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV2e"/>.
    /// </summary>
    public class NeuropixelsV2eHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV2e configureNode)
                {
                    using var editorDialog = new NeuropixelsV2eHeadstageDialog(configureNode.NeuropixelsV2e, configureNode.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNode.Bno055.Enable = editorDialog.DialogBno055.ConfigureNode.Enable;

                        DesignHelper.CopyProperties(editorDialog.DialogNeuropixelsV2e.ConfigureNode, configureNode.NeuropixelsV2e);

                        return true;
                    }
                }
                else if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV2eBeta configureNodeBeta)
                {
                    using var editorDialog = new NeuropixelsV2eHeadstageDialog(configureNodeBeta.NeuropixelsV2eBeta, configureNodeBeta.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNodeBeta.Bno055.Enable = editorDialog.DialogBno055.ConfigureNode.Enable;

                        DesignHelper.CopyProperties(editorDialog.DialogNeuropixelsV2e.ConfigureNode, configureNodeBeta.NeuropixelsV2eBeta);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
