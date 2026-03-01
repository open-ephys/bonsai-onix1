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

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV2e configureV2eHeadstage)
                {
                    var configureNode = new ConfigureHeadstageNeuropixelsV2e();
                    DesignHelper.DeepCopyProperties(configureV2eHeadstage, configureNode);

                    using var editorDialog = new NeuropixelsV2eHeadstageDialog(configureNode.NeuropixelsV2e, configureNode.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties((ConfigurePolledBno055)editorDialog.DialogBno055.Device, configureV2eHeadstage.Bno055, DesignHelper.PropertiesToIgnore);

                        DesignHelper.CopyProperties((ConfigureNeuropixelsV2e)editorDialog.DialogNeuropixelsV2e.ConfigureNode, configureV2eHeadstage.NeuropixelsV2e, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
                else if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV2eBeta configureV2eBetaHeadstage)
                {
                    var configureNode = new ConfigureHeadstageNeuropixelsV2eBeta();
                    DesignHelper.DeepCopyProperties(configureV2eBetaHeadstage, configureNode);

                    using var editorDialog = new NeuropixelsV2eHeadstageDialog(configureNode.NeuropixelsV2eBeta, configureNode.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties((ConfigurePolledBno055)editorDialog.DialogBno055.Device, configureV2eBetaHeadstage.Bno055, DesignHelper.PropertiesToIgnore);

                        DesignHelper.CopyProperties((ConfigureNeuropixelsV2eBeta)editorDialog.DialogNeuropixelsV2e.ConfigureNode, configureV2eBetaHeadstage.NeuropixelsV2eBeta, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
