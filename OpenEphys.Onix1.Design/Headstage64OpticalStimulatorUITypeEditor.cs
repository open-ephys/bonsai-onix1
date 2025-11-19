using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstage64OpticalStimulator"/>.
    /// </summary>
    public class Headstage64OpticalStimulatorUITypeEditor : UITypeEditor
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

                if (editorService != null && editorState != null && !editorState.WorkflowRunning && value is ConfigureHeadstage64OpticalStimulator configureNode)
                {
                    var configuration = new ConfigureHeadstage64OpticalStimulator(configureNode);

                    using var editorDialog = new Headstage64OpticalStimulatorSequenceDialog(configuration);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        return editorDialog.OpticalStimulator;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}

