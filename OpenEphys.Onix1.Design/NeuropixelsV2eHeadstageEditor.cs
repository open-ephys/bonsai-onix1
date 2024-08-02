using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    public class NeuropixelsV2eHeadstageEditor : WorkflowComponentEditor
    {
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

                        configureHeadstage.NeuropixelsV2e.Enable = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.Enable;
                        configureHeadstage.NeuropixelsV2e.ProbeConfigurationA = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.ProbeConfigurationA;
                        configureHeadstage.NeuropixelsV2e.ProbeConfigurationB = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.ProbeConfigurationB;
                        configureHeadstage.NeuropixelsV2e.GainCalibrationFileA = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.GainCalibrationFileA;
                        configureHeadstage.NeuropixelsV2e.GainCalibrationFileB = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.GainCalibrationFileB;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
