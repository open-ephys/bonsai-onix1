using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureNeuropixelsV2PsbDecoder"/>.
    /// </summary>
    internal class NeuropixelsV2eEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV2PsbDecoder configureNeuropixelsV2e)
                {
                    using var shell = new ImGuiShellDialog("NeuropixelsV2 Configuration") { StartPosition = FormStartPosition.CenterScreen };
                    var editorDialog = new NeuropixelsV2eImGuiDialog(configureNeuropixelsV2e, nameof(NeuropixelsV2), shell.Log);
                    shell.AddTab(nameof(NeuropixelsV2), editorDialog);
                    shell.ShowDialog();
                    return true;
                }
                else if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV2BetaPsbDecoder configureNeuropixelsV2eBeta)
                {
                    using var shell = new ImGuiShellDialog("NeuropixelsV2-Beta Configuration") { StartPosition = FormStartPosition.CenterScreen };
                    var editorDialog = new NeuropixelsV2eImGuiDialog(configureNeuropixelsV2eBeta, nameof(NeuropixelsV2), shell.Log);
                    shell.AddTab(nameof(NeuropixelsV2), editorDialog);
                    shell.ShowDialog();
                    return true;
                }
            }

            return false;
        }
    }
}
