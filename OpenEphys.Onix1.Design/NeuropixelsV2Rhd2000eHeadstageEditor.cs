using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV2Rhd2000e"/>.
    /// </summary>
    internal class NeuropixelsV2Rhd2000eHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV2Rhd2000e configureV2eHeadstage)
                {
                    using var editorDialog = new NeuropixelsV2Rhd2000eHeadstageDialog(configureV2eHeadstage);
                    editorDialog.ShowDialog();
                    return true;
                }
            }

            return false;
        }
    }
}
