using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV1e"/>.
    /// </summary>
    public class NeuropixelsV1eHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV1e configureHeadstage)
                {
                    var configuration = new ConfigureHeadstageNeuropixelsV1e(configureHeadstage);

                    using var editorDialog = new NeuropixelsV1eHeadstageDialog(configuration.NeuropixelsV1e, configuration.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties((ConfigurePolledBno055)editorDialog.DialogBno055.Device, configureHeadstage.Bno055, DesignHelper.PropertiesToIgnore);

                        DesignHelper.CopyProperties((ConfigureNeuropixelsV1e)editorDialog.DialogNeuropixelsV1e.ConfigureNode, configureHeadstage.NeuropixelsV1e, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
