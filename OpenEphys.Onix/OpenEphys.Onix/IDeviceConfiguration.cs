using System;

namespace OpenEphys.Onix
{
    internal interface IDeviceConfiguration
    {
        string Name { get; }

        Type Type { get; }
    }
}
