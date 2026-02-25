using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;

namespace OpenEphys.Onix1.Design
{
    internal class Headstage64OpticalStimulatorComponentEditor : WorkflowComponentEditor
    {
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigureHeadstage64OpticalStimulator configureNode)
                {
                    var configuration = new ConfigureHeadstage64OpticalStimulator();
                    DesignHelper.DeepCopyProperties(configureNode, configuration);

                    using var editorDialog = new Headstage64OpticalStimulatorSequenceDialog(configuration);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        DesignHelper.CopyProperties(editorDialog.OpticalStimulator, configureNode);

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
