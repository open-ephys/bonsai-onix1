using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureTS4231V1"/>.
    /// </summary>
    public class TS4231V1Editor : UITypeEditor
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

                ConfigureTS4231V1 configureNode;

                if (context.Instance.GetType() == typeof(ConfigureTS4231V1))
                    configureNode = (ConfigureTS4231V1)context.Instance;
                else if (value.GetType() == typeof(ConfigureTS4231V1))
                    configureNode = (ConfigureTS4231V1)value;
                else
                    throw new Exception("Invalid input given to TS4231V1Editor.");

                if (editorService != null && editorState != null && !editorState.WorkflowRunning)
                {
                    using var editorDialog = new TS4231V1Dialog(configureNode);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties(editorDialog.ConfigureNode, configureNode);
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
