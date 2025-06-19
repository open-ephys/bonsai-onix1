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
        public void WriteByte(uint address, uint value, bool sixteenBitAddress = false)
        {
            WriteWord(address, value, 1, sixteenBitAddress);
        }

        public void WriteWord(uint address, uint value, uint numBytes, bool sixteenBitAddress = false)
        {
            if (numBytes < 1 || numBytes > 4) throw new ArgumentOutOfRangeException(nameof(numBytes)); 
            uint registerAddress = (address << 7) | (this.address & 0x7F);
            registerAddress |= sixteenBitAddress ? 0x80000000 : 0;
            registerAddress |= (numBytes - 1) << 28;
            device.WriteRegister(registerAddress, value);
        }

        public byte ReadByte(uint address, bool sixteenBitAddress = false)
        {
            return (byte)ReadWord(address, 1, sixteenBitAddress);
        }
        public uint ReadWord(uint address, uint numBytes, bool sixteenBitAddress = false)
        {
            if (numBytes < 1 || numBytes > 4) throw new ArgumentOutOfRangeException(nameof(numBytes));
            uint registerAddress = (address << 7) | (this.address & 0x7F);
            registerAddress |= sixteenBitAddress ? 0x80000000 : 0;
            registerAddress |= (numBytes - 1) << 28;
            return device.ReadRegister(registerAddress);
        }

        public void ReadWord(uint address, uint numBytes, byte[] arr, int offset, bool sixteenBitAddress = false)
        {
            uint data = ReadWord(address, numBytes, sixteenBitAddress);
            for (int i = 0; i < numBytes; i++)
            {
                arr[offset + i] = (byte)(data >> (8 * i));
            }
        }
        public byte[] ReadBytes(uint address, int count, bool sixteenBitAddress = false)
        {
            var data = new byte[count];
            for (uint i = 0; i < count; i++)
            {
                data[i] = ReadByte(address + i, sixteenBitAddress);
            }
            return data;
        }

        public string ReadString(uint address, int count, bool sixteenBitAddress = false)
        {
            var data = ReadBytes(address, count, sixteenBitAddress);
            count = Array.IndexOf(data, (byte)0);
            return Encoding.ASCII.GetString(data, 0, count < 0 ? data.Length : count);
        }
    }
}
