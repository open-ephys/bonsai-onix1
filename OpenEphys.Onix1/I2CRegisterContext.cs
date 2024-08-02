using System;
using System.Text;

namespace OpenEphys.Onix1
{
    class I2CRegisterContext
    {
        readonly DeviceContext device;
        readonly uint address;

        public I2CRegisterContext(I2CRegisterContext other, uint i2cAddress)
            : this(other.device, i2cAddress)
        {
        }

        public I2CRegisterContext(DeviceContext deviceContext, uint i2cAddress)
        {
            device = deviceContext ?? throw new ArgumentNullException(nameof(deviceContext));
            address = i2cAddress;
        }

        public void WriteByte(uint address, uint value)
        {
            uint registerAddress = (address << 7) | (this.address & 0x7F);
            device.WriteRegister(registerAddress, (byte)value);
        }

        public byte ReadByte(uint address)
        {
            uint registerAddress = (address << 7) | (this.address & 0x7F);
            return (byte)device.ReadRegister(registerAddress);
        }

        public byte[] ReadBytes(uint address, int count)
        {
            var data = new byte[count];
            for (uint i = 0; i < count; i++)
            {
                data[i] = ReadByte(address + i);
            }
            return data;
        }

        public string ReadString(uint address, int count)
        {
            var data = ReadBytes(address, count);
            count = Array.IndexOf(data, (byte)0);
            return Encoding.ASCII.GetString(data, 0, count < 0 ? data.Length : count);
        }
    }
}
