using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace OpenEphys.Onix1.Design
{
    static class DesignHelper
    {
        public static IEnumerable<Control> GetAllControls(this Control root)
        {
            var stack = new Stack<Control>();
            stack.Push(root);

            while (stack.Any())
            {
                var next = stack.Pop();
                foreach (Control child in next.Controls)
                    stack.Push(child);
                yield return next;
            }
        }

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

        static void CopyPropertiesCore<T>(T source, T target, IEnumerable<string> propertiesToIgnore, bool shallowCopy) where T : class
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

                    if (value == null)
                        continue;

                    var type = property.PropertyType;

                    if (shallowCopy || type.IsPrimitive || type.IsEnum || type == typeof(Enum) || type == typeof(string))
                    {
                        property.SetValue(target, value);
                    }
                    else
                    {
                        var settings = new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.All
                        };

                        var concreteType = value.GetType();
                        var json = JsonConvert.SerializeObject(value, settings);
                        var deepCopy = JsonConvert.DeserializeObject(json, concreteType, settings);
                        property.SetValue(target, deepCopy);
                    }
                }
            }
        }

        public static void CopyProperties<T>(T source, T target, IEnumerable<string> propertiesToIgnore = null) where T : class
        {
            CopyPropertiesCore(source, target, propertiesToIgnore, shallowCopy: true);
        }

        public static void DeepCopyProperties<T>(T source, T target, IEnumerable<string> propertiesToIgnore = null) where T : class
        {
            CopyPropertiesCore(source, target, propertiesToIgnore, shallowCopy: false);
        }
    }
}
