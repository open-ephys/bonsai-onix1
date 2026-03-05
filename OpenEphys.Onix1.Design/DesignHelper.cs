using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

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

        public static IEnumerable<Control> GetTopLevelControls(this Control root)
        {
            var stack = new Stack<Control>();
            stack.Push(root);

            if (stack.Any())
            {
                var next = stack.Pop();
                foreach (Control child in next.Controls)
                {
                    yield return child;
                }
            }
        }

        /// <summary>
        /// Given two forms, take all menu items that are in the "File" MenuItem of the child form, and copy them directly to the 
        /// "File" MenuItem for the parent form
        /// </summary>
        /// <param name="thisForm"></param>
        /// <param name="childForm"></param>
        public static Form AddMenuItemsFromDialogToFileOption(this Form thisForm, Form childForm)
        {
            const string FileString = "File";

            if (childForm != null)
            {
                var childMenuStrip = childForm.GetAllControls()
                                              .OfType<MenuStrip>()
                                              .FirstOrDefault() ?? throw new InvalidOperationException($"There are no menu strips in any child controls of the {childForm.Text} dialog.");

                var thisMenuStrip = thisForm.GetTopLevelControls()
                                            .OfType<MenuStrip>()
                                            .FirstOrDefault() ?? throw new InvalidOperationException($"There are no menu strips at the top level of the {thisForm.Text} dialog to pull out.");

                ToolStripMenuItem existingMenuItem = null;

                foreach (ToolStripMenuItem menuItem in thisMenuStrip.Items)
                {
                    if (menuItem.Text == FileString)
                    {
                        existingMenuItem = menuItem;
                    }
                }

                foreach (ToolStripMenuItem menuItem in childMenuStrip.Items)
                {
                    if (menuItem.Text == FileString)
                    {
                        while (menuItem.DropDownItems.Count > 0)
                        {
                            existingMenuItem.DropDownItems.Add(menuItem.DropDownItems[0]);
                        }
                    }
                }
            }

            return thisForm;
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

        public static Form CopyMenuStripColor(this Form source, Form target)
        {
            var sourceMenuStrip = source.GetAllControls()
                                        .OfType<MenuStrip>()
                                        .FirstOrDefault() ?? throw new InvalidOperationException($"There are no menu strips in any child controls of the {source.Text} dialog.");
            var targetMenuStrip = target.GetTopLevelControls()
                                        .OfType<MenuStrip>()
                                        .FirstOrDefault() ?? throw new InvalidOperationException($"There are no menu strips at the top level of the {target.Text} dialog to pull out.");

            targetMenuStrip.BackColor = sourceMenuStrip.BackColor;
            targetMenuStrip.ForeColor = sourceMenuStrip.ForeColor;

            return source;
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
