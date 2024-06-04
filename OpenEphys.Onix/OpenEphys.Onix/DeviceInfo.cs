using System;

namespace OpenEphys.Onix
{
    internal class DeviceInfo
    {
        public DeviceInfo(DeviceContext device, Type deviceType)
            : this(device.Context, deviceType, device.Address)
        {
        }

        public DeviceInfo(ContextTask context, Type deviceType, uint deviceAddress)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DeviceType = deviceType ?? throw new ArgumentNullException(nameof(deviceType));
            DeviceAddress = deviceAddress;
        }

        public ContextTask Context { get; }

        public Type DeviceType { get; }

        public uint DeviceAddress { get; }

        public void AssertType(Type expectedType)
        {
            if (DeviceType != expectedType)
            {
                throw new InvalidOperationException(
                    $"Expected device with register type {expectedType}. Actual type is {DeviceType}."
                );
            }
        }

        public void Deconstruct(out ContextTask context, out uint deviceAddress)
        {
            context = Context;
            deviceAddress = DeviceAddress;
        }
    }
}
