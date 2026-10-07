using System;

namespace OpenEphys.Onix1
{
    interface IDeviceConfiguration
    {
        string DeviceName { get; set; }
        Type DeviceType { get; }
    }
    interface IDeviceConfiguration<in TContext> : IDeviceConfiguration where TContext : ContextTask
    {
        IObservable<T> Process<T>(IObservable<T> source) where T : TContext;
    }
}
