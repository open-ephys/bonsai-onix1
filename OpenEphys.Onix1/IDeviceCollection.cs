using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenEphys.Onix1
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IDeviceCollection
    {
        internal IEnumerable<IDeviceConfiguration> GetDevices();
    }
}
