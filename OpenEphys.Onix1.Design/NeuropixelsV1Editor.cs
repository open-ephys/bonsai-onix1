using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Class that opens a new dialog for a NeuropixelsV1 device.
    /// </summary>
    public class NeuropixelsV1Editor : WorkflowComponentEditor
    {
        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));  
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV1e configureNeuropixelsV1e)
                {
                    var configuration = new ConfigureNeuropixelsV1e();
                    DesignHelper.DeepCopyProperties(configureNeuropixelsV1e, configuration);

                    using var editorDialog = new NeuropixelsV1Dialog(configuration);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties(editorDialog.ConfigureNode, configureNeuropixelsV1e, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
                else if (editorState != null && !editorState.WorkflowRunning && component is ConfigureNeuropixelsV1f configureNeuropixelsV1f)
                {
                    var configuration = new ConfigureNeuropixelsV1f();
                    DesignHelper.DeepCopyProperties(configureNeuropixelsV1f, configuration);

                    using var editorDialog = new NeuropixelsV1Dialog(configuration);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties(editorDialog.ConfigureNode, configureNeuropixelsV1f, DesignHelper.PropertiesToIgnore);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
