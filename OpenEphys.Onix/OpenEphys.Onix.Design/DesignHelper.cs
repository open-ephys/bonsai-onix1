using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace OpenEphys.Onix.Design
{
    public static class DesignHelper
    {
        public static T DeserializeString<T>(string channelLayout)
        {
            return JsonConvert.DeserializeObject<T>(channelLayout);
        }

        public static void SerializeObject(object _object, string filepath)
        {
            var stringJson = JsonConvert.SerializeObject(_object);

            File.WriteAllText(filepath, stringJson);
        }

        public static IEnumerable<Control> GetAllChildren(this Control root)
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

        /// <summary>
        /// Given two forms, take all menu items that are in the "File" MenuItem of the child form, and copy them directly to the 
        /// "File" MenuItem for the parent form
        /// </summary>
        /// <param name="thisForm"></param>
        /// <param name="form"></param>
        public static void AddMenuItemsFromDialogToFileOption(this Form thisForm, Form form)
        {
            const string FileString = "File";

            if (form != null)
            {
                var menuStrips = form.GetAllChildren()
                                     .OfType<MenuStrip>()
                                     .ToList();

                var thisMenuStrip = thisForm.GetAllChildren()
                                            .OfType<MenuStrip>()
                                            .FirstOrDefault();

                ToolStripMenuItem existingMenuItem = null;

                foreach (ToolStripMenuItem menuItem in thisMenuStrip.Items)
                {
                    if (menuItem.Text == FileString)
                    {
                        existingMenuItem = menuItem;
                    }
                }

                if (menuStrips != null && menuStrips.Count > 0)
                {
                    foreach (var menuStrip in menuStrips)
                    {
                        foreach (ToolStripMenuItem menuItem in menuStrip.Items)
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
            }
        }

        /// <summary>
        /// Given two forms, take all menu items that are in the "File" MenuItem of the child form, and copy them to the 
        /// sub-menu name given, nested under the "File" MenuItem for the parent form
        /// </summary>
        /// <param name="thisForm"></param>
        /// <param name="form"></param>
        /// <param name="subMenuName"></param>
        public static void AddMenuItemsFromDialogToFileOption(this Form thisForm, Form form, string subMenuName)
        {
            const string FileString = "File";

            if (form != null)
            {
                var menuStrips = form.GetAllChildren()
                                     .OfType<MenuStrip>()
                                     .ToList();

                var thisMenuStrip = thisForm.GetAllChildren()
                                            .OfType<MenuStrip>()
                                            .FirstOrDefault();

                ToolStripMenuItem existingMenuItem = null;

                foreach (ToolStripMenuItem menuItem in thisMenuStrip.Items)
                {
                    if (menuItem.Text == FileString)
                    {
                        existingMenuItem = menuItem;
                    }
                }

                ToolStripMenuItem newItems = new()
                {
                    Text = subMenuName
                };

                if (menuStrips != null && menuStrips.Count > 0)
                {
                    foreach (var menuStrip in menuStrips)
                    {
                        foreach (ToolStripMenuItem menuItem in menuStrip.Items)
                        {
                            if (menuItem.Text == FileString)
                            {
                                while (menuItem.DropDownItems.Count > 0)
                                {
                                    newItems.DropDownItems.Add(menuItem.DropDownItems[0]);
                                }
                            }
                        }
                    }

                    existingMenuItem.DropDownItems.Add(newItems);
                }
            }
        }

        /// <summary>
        /// Convert a ProbeInterface object to a list of electrodes, which includes all possible electrodes
        /// </summary>
        /// <param name="channelConfiguration">A <see cref="NeuropixelsV1eProbeGroup"/> object</param>
        /// <returns>List of <see cref="NeuropixelsV1eElectrode"/> electrodes</returns>
        public static List<NeuropixelsV1eElectrode> ToElectrodes(NeuropixelsV1eProbeGroup channelConfiguration)
        {
            List<NeuropixelsV1eElectrode> electrodes = new();

            foreach (var c in channelConfiguration.GetContacts())
            {
                electrodes.Add(new NeuropixelsV1eElectrode(c));
            }

            return electrodes;
        }

        public static void UpdateElectrodes(List<NeuropixelsV1eElectrode> electrodes, NeuropixelsV1eProbeGroup channelConfiguration)
        {
            if (electrodes.Count != channelConfiguration.NumberOfContacts)
            {
                throw new InvalidOperationException($"Different number of electrodes found in {nameof(electrodes)} versus {nameof(channelConfiguration)}");
            }

            int index = 0;

            foreach (var c in channelConfiguration.GetContacts())
            {
                electrodes[index++] = new NeuropixelsV1eElectrode(c);
            }
        }

        /// <summary>
        /// Convert a ProbeInterface object to a list of electrodes, which only includes currently enabled electrodes
        /// </summary>
        /// <param name="channelConfiguration">A <see cref="NeuropixelsV1eProbeGroup"/> object</param>
        /// <returns>List of <see cref="NeuropixelsV1eElectrode"/> electrodes that are enabled</returns>
        public static List<NeuropixelsV1eElectrode> ToChannelMap(NeuropixelsV1eProbeGroup channelConfiguration)
        {
            List<NeuropixelsV1eElectrode> channelMap = new();

            foreach (var c in channelConfiguration.GetContacts().Where(c => c.DeviceId != -1))
            {
                channelMap.Add(new NeuropixelsV1eElectrode(c));
            }

            return channelMap.OrderBy(e => e.Channel).ToList();
        }

        public static void UpdateChannelMap(List<NeuropixelsV1eElectrode> channelMap, NeuropixelsV1eProbeGroup channelConfiguration)
        {
            var enabledElectrodes = channelConfiguration.GetContacts()
                                                        .Where(c => c.DeviceId != -1);

            if (channelMap.Count != enabledElectrodes.Count())
            {
                throw new InvalidOperationException($"Different number of enabled electrodes found in {nameof(channelMap)} versus {nameof(channelConfiguration)}");
            }

            int index = 0;

            foreach (var c in enabledElectrodes)
            {
                channelMap[index++] = new NeuropixelsV1eElectrode(c);
            }
        }

        /// <summary>
        /// Update the currently enabled contacts in the probe group, based on the currently selected contacts in 
        /// the given channel map. The only operation that occurs is an update of the DeviceChannelIndices field,
        /// where -1 indicates the contact is no longer enabled
        /// </summary>
        /// <param name="channelMap">List of <see cref="NeuropixelsV1eElectrode"/> objects, which contain the index of the selected contact</param>
        /// <param name="probeGroup"><see cref="NeuropixelsV1eProbeGroup"/></param>
        public static void UpdateProbeGroup(List<NeuropixelsV1eElectrode> channelMap, NeuropixelsV1eProbeGroup probeGroup)
        {
            int[] deviceChannelIndices = new int[probeGroup.NumberOfContacts];

            deviceChannelIndices = deviceChannelIndices.Select(i => i = -1).ToArray();

            foreach (var e in channelMap)
            {
                deviceChannelIndices[e.ElectrodeNumber] = e.Channel;
            }

            probeGroup.UpdateDeviceChannelIndices(0, deviceChannelIndices);
        }

        public static List<NeuropixelsV1eElectrode> SelectElectrodes(this List<NeuropixelsV1eElectrode> channelMap, List<NeuropixelsV1eElectrode> electrodes)
        {
            foreach (var e in electrodes)
            {
                channelMap[e.Channel] = e;
            }

            return channelMap;
        }
    }
}
