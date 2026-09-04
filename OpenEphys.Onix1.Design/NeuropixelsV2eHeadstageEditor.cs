using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV2e"/>.
    /// </summary>
    internal class NeuropixelsV2eHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV2e configureV2eHeadstage)
                {
                    DesignHelper.ShowDialogWithWaitCursor(() => new NeuropixelsV2eHeadstageDialog(configureV2eHeadstage));
                    return true;
                }
                else if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV2eBeta configureV2eBetaHeadstage)
                {
                    DesignHelper.ShowDialogWithWaitCursor(() => new NeuropixelsV2eHeadstageDialog(configureV2eBetaHeadstage));
                    return true;
                }
            }

            return false;
        }
    }
}
