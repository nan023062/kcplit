using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nave.Network.Network
{
    public static class NetworkTools
    {
        public static void WriteToBuffer(char value, byte[] buffer, int index)
        {
            buffer[index] = (byte)value;
        }

        public static void WriteToBuffer(int value, byte[] buffer, int index)
        {
            byte[] _bytes = System.BitConverter.GetBytes(value);
            _bytes.CopyTo(buffer, index);
        }

        public static void WriteToBuffer(uint value, byte[] buffer, int index)
        {
            byte[] _bytes = System.BitConverter.GetBytes(value);
            _bytes.CopyTo(buffer, index);
        }

        public static void WriteToBuffer(short value, byte[] buffer, int index)
        {
            byte[] _bytes = System.BitConverter.GetBytes(value);
            _bytes.CopyTo(buffer, index);
        }

        public static void WriteToBuffer(ushort value, byte[] buffer, int index)
        {
            byte[] _bytes = System.BitConverter.GetBytes(value);
            _bytes.CopyTo(buffer, index);
        }

        public static void WriteToBuffer(bool value, byte[] buffer, int index)
        {
            byte[] _bytes = System.BitConverter.GetBytes(value);
            _bytes.CopyTo(buffer, index);
        }

        public static void WriteToBuffer(Int64 value, byte[] buffer, int index)
        {
            byte[] _bytes = System.BitConverter.GetBytes(value);
            _bytes.CopyTo(buffer, index);
        }

        public static void WriteToBuffer(UInt64 value, byte[] buffer, int index)
        {
            byte[] _bytes = System.BitConverter.GetBytes(value);
            _bytes.CopyTo(buffer, index);
        }

        public static void WriteToBuffer(float value, byte[] buffer, int index)
        {
            byte[] _bytes = System.BitConverter.GetBytes(value);
            _bytes.CopyTo(buffer, index);
        }

        public static void WriteToBuffer(double value, byte[] buffer, int index)
        {
            byte[] _bytes = System.BitConverter.GetBytes(value);
            _bytes.CopyTo(buffer, index);
        }

        public static int ReadInt32FromBuffer(byte[] buffer, int beginLen)
        {
            return (int)buffer[beginLen]
                    | (int)buffer[beginLen + 1] << 8
                    | (int)buffer[beginLen + 2] << 16
                    | (int)buffer[beginLen + 3] << 24;
        }

        public static uint ReadUInt32FromBuffer(byte[] buffer, int beginLen)
        {
            return (uint)buffer[beginLen]
                    | (uint)buffer[beginLen + 1] << 8
                    | (uint)buffer[beginLen + 2] << 16
                    | (uint)buffer[beginLen + 3] << 24;
        }

        public static short ReadShortFromBuffer(byte[] buffer, int beginLen)
        {
            return (short)((int)buffer[beginLen]
                           | (int)buffer[beginLen + 1] << 8);
        }

        public static UInt16 ReadUInt16FromBuffer(byte[] buffer, int beginLen)
        {
            return (UInt16)((int)buffer[beginLen]
                           | (int)buffer[beginLen + 1] << 8);
        }
    }
}
