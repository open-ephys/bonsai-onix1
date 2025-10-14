using Bonsai.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a <see cref="ConfigureHeadstageNeuropixelsV1e"/>.
    /// </summary>
    public class NeuropixelsV1fHeadstageEditor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));

                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstageNeuropixelsV1f configureHeadstage)
                {
                    using var editorDialog = new NeuropixelsV1fHeadstageDialog(configureHeadstage.NeuropixelsV1A, configureHeadstage.NeuropixelsV1B, configureHeadstage.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties((ConfigureBno055)editorDialog.DialogBno055.Device, configureHeadstage.Bno055, DesignHelper.PropertiesToIgnore);

                        configureHeadstage.NeuropixelsV1A.ProbeConfiguration = editorDialog.DialogNeuropixelsV1A.ProbeConfigurationDialog.ProbeConfiguration;
                        configureHeadstage.NeuropixelsV1B.ProbeConfiguration = editorDialog.DialogNeuropixelsV1B.ProbeConfigurationDialog.ProbeConfiguration;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
