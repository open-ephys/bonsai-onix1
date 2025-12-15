using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="NeuropixelsV2ProbeConfiguration"/>.
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

                if (editorService != null && editorState != null && !editorState.WorkflowRunning && value is NeuropixelsV2ProbeConfiguration configuration)
                {
                    bool isBeta = (IConfigureNeuropixelsV2)context.Instance is ConfigureNeuropixelsV2eBeta;

                    var configurationCopy = configuration.Clone();

                    using var editorDialog = new NeuropixelsV2eProbeConfigurationDialog(configurationCopy);

                    if (isBeta)
                    {
                        editorDialog.Text = editorDialog.Text.Replace("NeuropixelsV2e ", "NeuropixelsV2eBeta ");
                    }

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        return editorDialog.ProbeConfiguration;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
