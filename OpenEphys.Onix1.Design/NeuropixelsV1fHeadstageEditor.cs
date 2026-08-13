using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV1f"/>.
    /// </summary>
    internal class NeuropixelsV1fHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV1f configureHeadstage)
                {
                    DesignHelper.ShowDialogWithWaitCursor(() =>
                        new NeuropixelsV1fHeadstageDialog(configureHeadstage.NeuropixelsV1A, configureHeadstage.NeuropixelsV1B, configureHeadstage.Bno055));
                    return true;
                }
            }

            return false;
        }
    }
}
