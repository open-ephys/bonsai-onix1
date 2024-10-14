using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a NeuropixelsV1 device.
    /// </summary>
    public class NeuropixelsV1Editor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));  
                if (editorState != null && !editorState.WorkflowRunning && component is IConfigureNeuropixelsV1 configureNeuropixelsV1)
                {
                    using var editorDialog = new NeuropixelsV1Dialog(configureNeuropixelsV1);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureNeuropixelsV1.Enable = editorDialog.ConfigureNode.Enable;
                        configureNeuropixelsV1.GainCalibrationFile = editorDialog.ConfigureNode.GainCalibrationFile;
                        configureNeuropixelsV1.AdcCalibrationFile = editorDialog.ConfigureNode.AdcCalibrationFile;
                        configureNeuropixelsV1.ProbeConfiguration = editorDialog.ConfigureNode.ProbeConfiguration;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
