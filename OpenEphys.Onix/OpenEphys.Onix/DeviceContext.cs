using System;

namespace OpenEphys.Onix
{
    internal class DeviceContext
    {
        readonly ContextTask _context;
        readonly oni.Device _device;

        public DeviceContext(ContextTask context, oni.Device device)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _device = device;
        }

        public ContextTask Context => _context;

        public uint Address => _device.Address;

        public oni.Device DeviceMetadata => _device;

        public oni.Hub Hub => _context.GetHub(_device.Address);

        public uint ReadRegister(uint registerAddress)
        {
            return _context.ReadRegister(_device.Address, registerAddress);
        }

        public void WriteRegister(uint registerAddress, uint value)
        {
            _context.WriteRegister(_device.Address, registerAddress, value);
        }

        public void Write<T>(T data) where T : unmanaged
        {
            _context.Write(_device.Address, data);
        }

        public void Write<T>(T[] data) where T : unmanaged
        {
            _context.Write(_device.Address, data);
        }

        public void Write(IntPtr data, int dataSize)
        {
            _context.Write(_device.Address, data, dataSize);
        }
    }
}
