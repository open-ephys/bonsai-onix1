using System;
using System.Linq;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Reactive.Linq;
using System.Numerics;
using Bonsai.Design;


namespace OpenEphys.Onix1.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog for selecting
    /// members of a workflow expression type.
    /// </summary>
    public class SpatialTransformMatrixEditor : DataSourceTypeEditor
    {
        public SpatialTransformMatrixEditor()
            : base(DataSource.Input, typeof(void))
        {
        }

        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        protected virtual IObservable<Tuple<int, Vector3>> GetData(IObservable<IObservable<object>> source)
        {
            return source.Merge().Select(coordinate => (Tuple<int, Vector3>)coordinate);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                var source = GetDataSource(context, provider);
                var dataFrames = GetData(source.Output);
                using (var visualizerDialog = new SpatialTransformMatrixDialog(dataFrames))
                {
                    if (editorService.ShowDialog(visualizerDialog) == DialogResult.OK && visualizerDialog.ApplySpatialTransform)
                    {
                        return visualizerDialog.SpatialTransform;
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }
    }
}
