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
                    var configureNode = new ConfigureHeadstageNeuropixelsV2Rhd2000e();
                    DesignHelper.DeepCopyProperties(configureV2eHeadstage, configureNode);

                    using var editorDialog = new NeuropixelsV2Rhd2000eHeadstageDialog(configureNode);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties((ConfigurePolledBno055)editorDialog.DialogBno055.Device, configureV2eHeadstage.Bno055, DesignHelper.PropertiesToIgnore);

                        DesignHelper.CopyProperties((ConfigureRhd2000PsbDecoderWithMax41400)editorDialog.DialogRhd2000.Device, configureV2eHeadstage.Rhd2000, DesignHelper.PropertiesToIgnore);

                        DesignHelper.CopyProperties((ConfigureNeuropixelsV2PsbDecoder)editorDialog.DialogNeuropixelsV2.ConfigureNeuropixelsV2, configureV2eHeadstage.NeuropixelsV2, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
