using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstage64ElectricalStimulator"/>.
    /// </summary>
    public class Headstage64ElectricalStimulatorUITypeEditor : UITypeEditor
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

                if (editorService != null && editorState != null && !editorState.WorkflowRunning && value is ConfigureHeadstage64ElectricalStimulator configureNode)
                {
                    using var editorDialog = new Headstage64ElectricalStimulatorSequenceDialog(configureNode);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        return editorDialog.ElectricalStimulator;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
