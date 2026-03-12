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
                    var configuration = new ConfigureHeadstageNeuropixelsV1f();
                    DesignHelper.DeepCopyProperties(configureHeadstage, configuration);

                    using var editorDialog = new NeuropixelsV1fHeadstageDialog(configuration.NeuropixelsV1A, configuration.NeuropixelsV1B, configuration.Bno055);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties((ConfigureBno055)editorDialog.DialogBno055.Device, configureHeadstage.Bno055, DesignHelper.PropertiesToIgnore);

                        DesignHelper.CopyProperties((ConfigureNeuropixelsV1f)editorDialog.DialogNeuropixelsV1A.ConfigureNode, configureHeadstage.NeuropixelsV1A, DesignHelper.PropertiesToIgnore);

                        DesignHelper.CopyProperties((ConfigureNeuropixelsV1f)editorDialog.DialogNeuropixelsV1B.ConfigureNode, configureHeadstage.NeuropixelsV1B, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
