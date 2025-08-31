using System;
using System.Collections;

namespace Bioss.Ultrasound.Extensions
{
    public static class BitArrayExtension
    {
        public static byte ConvertToByte(this BitArray bits)
        {
            if (bits.Count != 8)
                throw new ArgumentException("bits count must be 8");

            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }
    }
}
