using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace OpenEphys.Onix1.Design
{
    static class DesignHelper
    {
        public static Form AddDialogToTab(this Form form, TabPage tabPage)
        {
            tabPage.Controls.Add(form);
            form.Show();

            return form;
        }

        public static Form AddDialogToPanel(this Form form, Panel panel)
        {
            panel.Controls.Add(form);
            form.Show();

            return form;
        }

        public static Form SetChildFormProperties(this Form child, Form parent)
        {
            child.TopLevel = false;
            child.FormBorderStyle = FormBorderStyle.None;
            child.Dock = DockStyle.Fill;
            child.Parent = parent;

            return child;
        }

        internal static readonly IEnumerable<string> PropertiesToIgnore = new[] { "DeviceName", "DeviceAddress" };

        public static void CopyProperties<T>(T source, T target, IEnumerable<string> propertiesToIgnore = null) where T : class
        {
            if (source == null || target == null)
                throw new NullReferenceException("Null objects cannot have their properties copied from/to.");

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            propertiesToIgnore ??= Array.Empty<string>();

            foreach (var property in properties)
            {
                if (propertiesToIgnore.Contains(property.Name))
                    continue;

                if (property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(source);
                    property.SetValue(target, value);
                }
            }
        }
    }
}
