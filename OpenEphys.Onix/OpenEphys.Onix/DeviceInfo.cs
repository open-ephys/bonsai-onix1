using System;

namespace OpenEphys.Onix
{
    internal class DeviceInfo
    {
        public DeviceInfo(ContextTask context, Type deviceType, uint deviceIndex)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DeviceType = deviceType ?? throw new ArgumentNullException(nameof(deviceType));
            DeviceIndex = deviceIndex;
        }

        public ContextTask Context { get; }

        public Type DeviceType { get; }

        public uint DeviceIndex { get; }

        public void AssertType(Type expectedType)
        {
            if (DeviceType != expectedType)
            {
                throw new InvalidOperationException(
                    $"Expected device with register type {expectedType}. Actual type is {DeviceType}."
                );
            }
        }

        public void Deconstruct(out ContextTask context, out uint deviceIndex)
        {
            context = Context;
            deviceIndex = DeviceIndex;
        }
    }
}
