using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="NeuropixelsV2QuadShankProbeConfiguration"/>.
    /// </summary>
    public class NeuropixelsV2eProbeConfigurationEditor : UITypeEditor
    {
        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <inheritdoc/>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorService != null && editorState != null && !editorState.WorkflowRunning &&
                    value is NeuropixelsV2QuadShankProbeConfiguration configuration)
                {
                    var instance = (ConfigureNeuropixelsV2e)context.Instance;

                    var calibrationFile = configuration.Probe == NeuropixelsV2Probe.ProbeA ? instance.GainCalibrationFileA : instance.GainCalibrationFileB;

                    using var editorDialog = new NeuropixelsV2eProbeConfigurationDialog(configuration, calibrationFile);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (configuration.Probe == NeuropixelsV2Probe.ProbeA)
                        {
                            instance.GainCalibrationFileA = editorDialog.textBoxProbeCalibrationFile.Text;
                        }
                        else
                        {
                            instance.GainCalibrationFileB = editorDialog.textBoxProbeCalibrationFile.Text;
                        }

                        return editorDialog.ProbeConfiguration;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
