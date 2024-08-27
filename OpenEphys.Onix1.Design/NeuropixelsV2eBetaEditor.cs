using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Opens a new dialog for a <see cref="ConfigureNeuropixelsV2eBeta"/>.
    /// </summary>
    public class NeuropixelsV2eBetaEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV2eBeta configureNeuropixelsV2eBeta)
                {
                    using var editorDialog = new NeuropixelsV2eBetaDialog(configureNeuropixelsV2eBeta);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNeuropixelsV2eBeta.Enable = editorDialog.ConfigureNode.Enable;
                        configureNeuropixelsV2eBeta.EnableLed = editorDialog.ConfigureNode.EnableLed;
                        configureNeuropixelsV2eBeta.GainCalibrationFileA = editorDialog.ConfigureNode.GainCalibrationFileA;
                        configureNeuropixelsV2eBeta.GainCalibrationFileB = editorDialog.ConfigureNode.GainCalibrationFileB;
                        configureNeuropixelsV2eBeta.ProbeConfigurationA = editorDialog.ConfigureNode.ProbeConfigurationA;
                        configureNeuropixelsV2eBeta.ProbeConfigurationB = editorDialog.ConfigureNode.ProbeConfigurationB;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
