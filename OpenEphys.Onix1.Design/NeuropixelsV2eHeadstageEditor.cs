using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureNeuropixelsV2eHeadstage"/>.
    /// </summary>
    public class NeuropixelsV2eHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV2eHeadstage configureHeadstage)
                {
                    using var editorDialog = new NeuropixelsV2eHeadstageDialog(configureHeadstage.NeuropixelsV2e, configureHeadstage.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureHeadstage.Bno055.Enable = editorDialog.ConfigureBno055.ConfigureNode.Enable;

                        configureHeadstage.NeuropixelsV2e.Enable = editorDialog.DialogNeuropixelsV2e.ConfigureNode.Enable;
                        configureHeadstage.NeuropixelsV2e.ProbeConfigurationA = editorDialog.DialogNeuropixelsV2e.ConfigureNode.ProbeConfigurationA;
                        configureHeadstage.NeuropixelsV2e.ProbeConfigurationB = editorDialog.DialogNeuropixelsV2e.ConfigureNode.ProbeConfigurationB;
                        configureHeadstage.NeuropixelsV2e.GainCalibrationFileA = editorDialog.DialogNeuropixelsV2e.ConfigureNode.GainCalibrationFileA;
                        configureHeadstage.NeuropixelsV2e.GainCalibrationFileB = editorDialog.DialogNeuropixelsV2e.ConfigureNode.GainCalibrationFileB;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
