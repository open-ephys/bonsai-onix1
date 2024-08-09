using System.Collections;
using System;

namespace OpenEphys.Onix1
{
    static class BitHelper
    {
        /// <summary>
        /// Replace a defined set of bits in unsigned integer with those from another.
        /// </summary>
        /// <param name="value">The value where bits will be replaced.</param>
        /// <param name="mask">A mask defining which bits should be replaced.</param>
        /// <param name="bits">A value containing the bits that will be assingned to the <paramref name="mask"/>
        /// positions in <paramref name="value"/>.</param>
        /// <returns></returns>
        internal static uint Replace(uint value, uint mask, uint bits)
        {
            return (value & ~mask) | (bits & mask);
        }

        /// <summary>
        /// Create a bit-reversed byte array from an bit array.
        /// </summary>
        /// <param name="bits">Bit array to convert.</param>
        /// <returns>Byte array with where the MSB to LSB order has been reversed in each byte. </returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="bits"/> array is empty.</exception>
        internal static byte[] ToBitReversedBytes(BitArray bits)
        {
            if (bits.Length == 0)
            {
                throw new ArgumentException("Shift register data is empty", nameof(bits));
            }

            var bytes = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(bytes, 0);

            for (int i = 0; i < bytes.Length; i++)
            {
                // NB: http://graphics.stanford.edu/~seander/bithacks.html
                bytes[i] = (byte)((bytes[i] * 0x0202020202ul & 0x010884422010ul) % 1023);
            }

            return bytes;
        }
    }
}
