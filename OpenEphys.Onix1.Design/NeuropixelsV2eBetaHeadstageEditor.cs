using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureNeuropixelsV2eHeadstage"/>.
    /// </summary>
    public class NeuropixelsV2eBetaHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV2eBetaHeadstage configureHeadstage)
                {
                    using var editorDialog = new NeuropixelsV2eBetaHeadstageDialog(configureHeadstage.NeuropixelsV2eBeta, configureHeadstage.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureHeadstage.Bno055.Enable = editorDialog.DialogBno055.ConfigureNode.Enable;

                        configureHeadstage.NeuropixelsV2eBeta.Enable = editorDialog.DialogNeuropixelsV2e.ConfigureNode.Enable;
                        configureHeadstage.NeuropixelsV2eBeta.EnableLed = editorDialog.DialogNeuropixelsV2e.ConfigureNode.EnableLed;
                        configureHeadstage.NeuropixelsV2eBeta.ProbeConfigurationA = editorDialog.DialogNeuropixelsV2e.ConfigureNode.ProbeConfigurationA;
                        configureHeadstage.NeuropixelsV2eBeta.ProbeConfigurationB = editorDialog.DialogNeuropixelsV2e.ConfigureNode.ProbeConfigurationB;
                        configureHeadstage.NeuropixelsV2eBeta.GainCalibrationFileA = editorDialog.DialogNeuropixelsV2e.ConfigureNode.GainCalibrationFileA;
                        configureHeadstage.NeuropixelsV2eBeta.GainCalibrationFileB = editorDialog.DialogNeuropixelsV2e.ConfigureNode.GainCalibrationFileB;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
