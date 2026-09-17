using System;

namespace OpenEphys.Onix1
{
    internal interface IDeviceConfiguration
    {
        string DeviceName { get; set; }

        Type DeviceType { get; }

        IObservable<TContext> Process<TContext>(IObservable<TContext> source) where TContext : ContextTask;
    }
}
