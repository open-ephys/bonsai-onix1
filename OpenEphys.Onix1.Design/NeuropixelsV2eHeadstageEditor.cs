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
                    using var editorDialog = new NeuropixelsV2eHeadstageDialog(configureV2eHeadstage.NeuropixelsV2e, configureV2eHeadstage.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties((ConfigurePolledBno055)editorDialog.DialogBno055.Device, configureV2eHeadstage.Bno055, DesignHelper.PropertiesToIgnore);

                        configureV2eHeadstage.NeuropixelsV2e.ProbeConfigurationA = editorDialog.DialogNeuropixelsV2e.ProbeConfigurationA;
                        configureV2eHeadstage.NeuropixelsV2e.ProbeConfigurationB = editorDialog.DialogNeuropixelsV2e.ProbeConfigurationB;

                        return true;
                    }
                }
                else if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV2eBeta configureV2eBetaHeadstage)
                {
                    using var editorDialog = new NeuropixelsV2eHeadstageDialog(configureV2eBetaHeadstage.NeuropixelsV2eBeta, configureV2eBetaHeadstage.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties((ConfigurePolledBno055)editorDialog.DialogBno055.Device, configureV2eBetaHeadstage.Bno055, DesignHelper.PropertiesToIgnore);

                        configureV2eBetaHeadstage.NeuropixelsV2eBeta.ProbeConfigurationA = editorDialog.DialogNeuropixelsV2e.ProbeConfigurationA;
                        configureV2eBetaHeadstage.NeuropixelsV2eBeta.ProbeConfigurationB = editorDialog.DialogNeuropixelsV2e.ProbeConfigurationB;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
