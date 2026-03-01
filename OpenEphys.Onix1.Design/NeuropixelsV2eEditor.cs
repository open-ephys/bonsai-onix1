using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureNeuropixelsV2e"/>.
    /// </summary>
    public class NeuropixelsV2eEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV2e configureNeuropixelsV2e)
                {
                    var configureNode = new ConfigureNeuropixelsV2e();
                    DesignHelper.DeepCopyProperties(configureNeuropixelsV2e, configureNode);

                    using var editorDialog = new NeuropixelsV2eDialog(configureNode);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties(editorDialog.ConfigureNode, configureNeuropixelsV2e, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
                else if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV2eBeta configureNeuropixelsV2eBeta)
                {
                    var configureNode = new ConfigureNeuropixelsV2eBeta();
                    DesignHelper.DeepCopyProperties(configureNeuropixelsV2eBeta, configureNode);

                    using var editorDialog = new NeuropixelsV2eDialog(configureNode);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties(editorDialog.ConfigureNode, configureNeuropixelsV2eBeta, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
