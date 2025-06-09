using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureRhd2164"/>.
    /// </summary>
    public class Rhd2164Editor : UITypeEditor
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

                ConfigureRhd2164 configureRhd2164;

                if (context.Instance.GetType() == typeof(ConfigureRhd2164))
                    configureRhd2164 = (ConfigureRhd2164)context.Instance;
                else if (value.GetType() == typeof(ConfigureRhd2164))
                    configureRhd2164 = (ConfigureRhd2164)value;
                else
                    throw new Exception("Invalid input given to Rhd2164Editor.");

                if (editorService != null && editorState != null && !editorState.WorkflowRunning)
                {
                    using var editorDialog = new Rhd2164Dialog(configureRhd2164);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureRhd2164.Enable = editorDialog.ConfigureNode.Enable;
                        configureRhd2164.DspCutoff = editorDialog.ConfigureNode.DspCutoff;
                        configureRhd2164.AnalogHighCutoff = editorDialog.ConfigureNode.AnalogHighCutoff;
                        configureRhd2164.AnalogLowCutoff = editorDialog.ConfigureNode.AnalogLowCutoff;
                        configureRhd2164.DeviceAddress = editorDialog.ConfigureNode.DeviceAddress;
                        configureRhd2164.DeviceName = editorDialog.ConfigureNode.DeviceName;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
