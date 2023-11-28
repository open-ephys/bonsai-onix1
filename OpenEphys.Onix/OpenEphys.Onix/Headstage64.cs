﻿using System.Collections.Generic;
using System.ComponentModel;
using Bonsai;

namespace OpenEphys.Onix
{
    public class Headstage64 : HubDeviceFactory, INamedElement
    {
        PortName port;
        readonly ConfigureFmcLinkController LinkController = new();

        public Headstage64()
        {
            Port = PortName.PortA;
            LinkController.Passthrough = false;
            LinkController.MinVoltage = 5.0;
            LinkController.MaxVoltage = 7.0;
        }

        public string Name { get; set; }

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureRhd2164 Rhd2164 { get; set; } = new();

        [Category(ConfigurationCategory)]
        [TypeConverter(typeof(HubDeviceConverter))]
        public ConfigureBno055 Bno055 { get; set; } = new();

        public PortName Port
        {
            get { return port; }
            set
            {
                port = value;
                LinkController.DeviceAddress = (uint)port;
                Rhd2164.DeviceName = !string.IsNullOrEmpty(Name) ? $"{Name}.Rhd2164" : null;
                Bno055.DeviceName = !string.IsNullOrEmpty(Name) ? $"{Name}.Bno055" : null;
                Rhd2164.DeviceAddress = ((uint)port << 8) + 0;
                Bno055.DeviceAddress = ((uint)port << 8) + 1;
            }
        }

        internal override IEnumerable<IDeviceConfiguration> GetDevices()
        {
            yield return LinkController;
            yield return Rhd2164;
            yield return Bno055;
        }
    }

    public enum PortName
    {
        PortA = 1,
        PortB = 2
    }
}