using System;
using System.ComponentModel;
using Bonsai.Design;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    internal class PolledBno055Editor : WorkflowComponentEditor
    {
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (provider != null)
            {
                var editorState = (IWorkflowEditorState)provider.GetService(typeof(IWorkflowEditorState));
                if (editorState != null && !editorState.WorkflowRunning && component is ConfigurePolledBno055 configureBno055)
                {
                    using var editorDialog = new PolledBno055Dialog(configureBno055, true);

                    if (editorDialog.ShowDialog() == DialogResult.OK)
                    {
                        configureBno055.Enable = editorDialog.Bno055.Enable;
                        configureBno055.DeviceAddress = editorDialog.Bno055.DeviceAddress;
                        configureBno055.DeviceName = editorDialog.Bno055.DeviceName;
                        configureBno055.AxisMap = editorDialog.Bno055.Bno055.AxisMap;
                        configureBno055.AxisSign = editorDialog.Bno055.Bno055.AxisSign;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
