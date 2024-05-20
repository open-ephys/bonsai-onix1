using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using OpenEphys.ProbeInterface;

namespace OpenEphys.Onix.Design
{
    public class ChannelConfigurationEditor : UITypeEditor
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
                var editorDialog = new ChannelConfigurationDialog(value as ProbeGroup);

                if (editorDialog.ShowDialog() == DialogResult.OK)
                {
                    if (editorDialog.ChannelConfiguration != null)
                        return editorDialog.ChannelConfiguration;

                    MessageBox.Show("Warning: Channel configuration was not valid; all settings were discarded.");
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
