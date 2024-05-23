using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;

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

                var headstageEditorDialog = new HeadstageRhs2116Dialog(instance.ChannelConfiguration, instance.StimulusTrigger.StimulusSequence);

                if (headstageEditorDialog.ShowDialog() == DialogResult.OK)
                {
                    var stimulusSequenceDialog = (Rhs2116StimulusSequenceDialog)headstageEditorDialog.Controls["tabPageStimulusSequence"]
                                                                                                     .Controls[nameof(Rhs2116StimulusSequenceDialog)];
                    instance.StimulusTrigger.StimulusSequence = stimulusSequenceDialog.Sequence;

                    return headstageEditorDialog.ChannelConfiguration;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
