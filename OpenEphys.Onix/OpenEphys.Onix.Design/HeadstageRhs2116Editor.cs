using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Linq;

namespace OpenEphys.Onix.Design
{
    public class HeadstageRhs2116Editor : UITypeEditor
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
                var instance = (ConfigureHeadstageRhs2116)context.Instance;

                if (instance == null)
                {
                    return base.EditValue(context, provider, value);
                }

                var headstageEditorDialog = new HeadstageRhs2116Dialog(instance.ChannelConfiguration,
                                                                       instance.StimulusTrigger.StimulusSequence,
                                                                       instance.Rhs2116A);

                if (headstageEditorDialog.ShowDialog() == DialogResult.OK)
                {
                    instance.StimulusTrigger.StimulusSequence = DesignHelper.GetAllChildren(headstageEditorDialog)
                                                                            .OfType<Rhs2116StimulusSequenceDialog>()
                                                                            .Select(dialog => dialog.Sequence)
                                                                            .First();

                    instance.Rhs2116A = DesignHelper.GetAllChildren(headstageEditorDialog)
                                                    .OfType<Rhs2116Dialog>()
                                                    .Where(dialog => dialog.Rhs2116.DeviceName.Contains(nameof(instance.Rhs2116A)))
                                                    .Select(dialog => dialog.Rhs2116)
                                                    .First();

                    return headstageEditorDialog.ChannelConfiguration;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
