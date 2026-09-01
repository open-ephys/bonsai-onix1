using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// Defines a device with an address inside an ONI system
    /// </summary>
    internal interface IAddressableDevice
    {
        uint DeviceAddress { get; set; }
    }
}
