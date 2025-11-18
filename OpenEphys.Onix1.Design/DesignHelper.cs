using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
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
                var childMenuStrip = childForm.GetTopLevelControls()
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

        /// <summary>
        /// Given two forms, take all menu items that are in the "File" MenuItem of the child form, and copy them to the 
        /// sub-menu name given, nested under the "File" MenuItem for the parent form
        /// </summary>
        /// <param name="thisForm"></param>
        /// <param name="childForm"></param>
        /// <param name="subMenuName"></param>
        public static Form AddMenuItemsFromDialogToFileOption(this Form thisForm, Form childForm, string subMenuName)
        {
            const string FileString = "File";

            if (childForm != null)
            {
                var childMenuStrip = childForm.GetTopLevelControls()
                                              .OfType<MenuStrip>()
                                              .First() ?? throw new InvalidOperationException($"There are no menu strips in any child controls of the {childForm.Text} dialog.");

                var thisMenuStrip = thisForm.GetTopLevelControls()
                                            .OfType<MenuStrip>()
                                            .FirstOrDefault() ?? throw new InvalidOperationException($"There are no menu strips at the top level of the {thisForm.Text} dialog to pull out.");

                ToolStripMenuItem thisFileMenuItem = null;

                foreach (ToolStripMenuItem menuItem in thisMenuStrip.Items)
                {
                    if (menuItem.Text == FileString)
                    {
                        thisFileMenuItem = menuItem;
                    }
                }

                ToolStripMenuItem newChildMenuItems = new()
                {
                    Text = subMenuName
                };

                foreach (ToolStripMenuItem childItem in childMenuStrip.Items)
                {
                    if (childItem.Text == FileString)
                    {
                        while (childItem.DropDownItems.Count > 0)
                        {
                            newChildMenuItems.DropDownItems.Add(childItem.DropDownItems[0]);
                        }
                    }
                }

                thisFileMenuItem.DropDownItems.Add(newChildMenuItems);
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
