using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace OpenEphys.Onix.Design
{
    public class Rhs2116Editor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null)
            {
                var instance = (ConfigureRhs2116)context.Instance;

                if (instance == null)
                {
                    return base.EditValue(context, provider, value);
                }

                var headstageEditorDialog = new Rhs2116Dialog(instance);

                if (headstageEditorDialog.ShowDialog() == DialogResult.OK)
                {
                    instance.Enable = headstageEditorDialog.Rhs2116.Enable;
                    instance.DspCutoff = headstageEditorDialog.Rhs2116.DspCutoff;
                    instance.RespectExternalActiveStim = headstageEditorDialog.Rhs2116.RespectExternalActiveStim;
                    instance.AnalogLowCutoffRecovery = headstageEditorDialog.Rhs2116.AnalogLowCutoffRecovery;
                    instance.AnalogLowCutoff = headstageEditorDialog.Rhs2116.AnalogLowCutoff;
                    instance.AnalogHighCutoff = headstageEditorDialog.Rhs2116.AnalogHighCutoff;
                    instance.DeviceAddress = headstageEditorDialog.Rhs2116.DeviceAddress;
                    instance.DeviceName = headstageEditorDialog.Rhs2116.DeviceName;

                    return headstageEditorDialog.Rhs2116.Enable;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
