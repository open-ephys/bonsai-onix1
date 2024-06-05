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

                var headstageDialog = new HeadstageRhs2116Dialog(instance.ChannelConfiguration,
                                                                 instance.StimulusTrigger.StimulusSequence,
                                                                 instance.Rhs2116A);

                if (headstageDialog.ShowDialog() == DialogResult.OK)
                {
                    var stimulusSequence = headstageDialog.GetAllChildren()
                                                          .OfType<Rhs2116StimulusSequenceDialog>()
                                                          .Select(dialog => dialog.Sequence)
                                                          .First();

                    if (stimulusSequence != null && stimulusSequence.Valid)
                    {
                        instance.StimulusTrigger.StimulusSequence = stimulusSequence;
                    }

                    var rhs2116 = headstageDialog.GetAllChildren()
                                                 .OfType<Rhs2116Dialog>()
                                                 .Select(dialog => dialog.Rhs2116)
                                                 .First();

                    if (rhs2116 != null)
                    {
                        instance.Rhs2116A = rhs2116;

                        instance.Rhs2116B.AnalogHighCutoff = rhs2116.AnalogHighCutoff;
                        instance.Rhs2116B.AnalogLowCutoff = rhs2116.AnalogLowCutoff;
                        instance.Rhs2116B.AnalogLowCutoffRecovery = rhs2116.AnalogLowCutoffRecovery;
                        instance.Rhs2116B.DspCutoff = rhs2116.DspCutoff;
                        instance.Rhs2116B.Enable = rhs2116.Enable;
                        instance.Rhs2116B.RespectExternalActiveStim = rhs2116.RespectExternalActiveStim;
                    }

                    return headstageDialog.ChannelConfiguration;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
