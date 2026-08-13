using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a NeuropixelsV1 device.
    /// </summary>
    internal class NeuropixelsV1Editor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV1PsbDecoder configureNeuropixelsV1e)
                {
                    using var shell = new ImGuiShellDialog("NeuropixelsV1 Configuration") { StartPosition = FormStartPosition.CenterScreen };
                    var editorDialog = new NeuropixelsV1ImGuiDialog(configureNeuropixelsV1e, nameof(NeuropixelsV1), shell.Log);
                    shell.AddTab(nameof(NeuropixelsV1), editorDialog);
                    shell.ShowDialog();
                    return true;
                }
                else if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV1f configureNeuropixelsV1f)
                {
                    using var shell = new ImGuiShellDialog("NeuropixelsV1 Configuration") { StartPosition = FormStartPosition.CenterScreen };
                    var editorDialog = new NeuropixelsV1ImGuiDialog(configureNeuropixelsV1f, nameof(NeuropixelsV1), shell.Log);
                    shell.AddTab(nameof(NeuropixelsV1), editorDialog);
                    shell.ShowDialog();
                    return true;
                }
            }

            return false;
        }
    }
}
