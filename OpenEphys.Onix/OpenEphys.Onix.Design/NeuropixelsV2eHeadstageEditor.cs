using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix.Design
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
                    using var editorDialog = new NeuropixelsV2eHeadstageDialog(configureHeadstage.NeuropixelsV2, configureHeadstage.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureHeadstage.Bno055.Enable = editorDialog.ConfigureBno055.ConfigureNode.Enable;

                        configureHeadstage.NeuropixelsV2.Enable = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.Enable;
                        configureHeadstage.NeuropixelsV2.ProbeConfigurationA = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.ProbeConfigurationA;
                        configureHeadstage.NeuropixelsV2.ProbeConfigurationB = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.ProbeConfigurationB;
                        configureHeadstage.NeuropixelsV2.GainCalibrationFileA = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.GainCalibrationFileA;
                        configureHeadstage.NeuropixelsV2.GainCalibrationFileB = editorDialog.ConfigureNeuropixelsV2e.ConfigureNode.GainCalibrationFileB;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
