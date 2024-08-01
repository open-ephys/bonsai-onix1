using System;

namespace OpenEphys.Onix1
{
    internal interface IDeviceConfiguration
    {
        string DeviceName { get; set; }

        Type DeviceType { get; }

        IObservable<ContextTask> Process(IObservable<ContextTask> source);
    }
}
