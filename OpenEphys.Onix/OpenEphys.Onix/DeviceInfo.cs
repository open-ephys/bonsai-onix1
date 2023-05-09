using System;

namespace OpenEphys.Onix
{
    internal class DeviceInfo
    {
        public DeviceInfo(ContextTask context, uint deviceIndex)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DeviceIndex = deviceIndex;
        }

        public ContextTask Context { get; }

        public uint DeviceIndex { get; }

        public void Deconstruct(out ContextTask context, out uint deviceIndex)
        {
            context = Context;
            deviceIndex = DeviceIndex;
        }
    }
}
