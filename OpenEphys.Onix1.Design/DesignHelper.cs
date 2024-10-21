using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace OpenEphys.Onix1.Design
{
    static class DesignHelper
    {
        /// <summary>
        /// Given a string with a valid JSON structure, deserialize the string to the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        #nullable enable
        public static T? DeserializeString<T>(string jsonString)
        {
            var errors = new List<string>();

            var serializerSettings = new JsonSerializerSettings()
            {
                Error = delegate(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                {
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            };

            var obj = JsonConvert.DeserializeObject<T>(jsonString, serializerSettings);

            if (errors.Count > 0)
            {
                MessageBox.Show($"There were errors encountered while parsing a JSON string. Check the console " +
                    $"for an error log.", "JSON Parse Error");

                foreach (var e in errors)
                {
                    Console.Error.WriteLine(e);
                }

                return default;
            }

            return obj;
        }
        #nullable disable

        public static void SerializeObject(object _object, string filepath)
        {
            var stringJson = JsonConvert.SerializeObject(_object, Formatting.Indented);

            File.WriteAllText(filepath, stringJson);
        }

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
        public static void AddMenuItemsFromDialogToFileOption(this Form thisForm, Form childForm)
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
        }

        /// <summary>
        /// Given two forms, take all menu items that are in the "File" MenuItem of the child form, and copy them to the 
        /// sub-menu name given, nested under the "File" MenuItem for the parent form
        /// </summary>
        /// <param name="thisForm"></param>
        /// <param name="childForm"></param>
        /// <param name="subMenuName"></param>
        public static void AddMenuItemsFromDialogToFileOption(this Form thisForm, Form childForm, string subMenuName)
        {
            const string FileString = "File";

            if (childForm != null)
            {
                var childMenuStrip = childForm.GetAllControls()
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
        }
    }
}
