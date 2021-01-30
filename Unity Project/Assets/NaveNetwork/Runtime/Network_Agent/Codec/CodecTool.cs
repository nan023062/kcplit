using System;
using UnityEngine;

namespace Nave.Network
{
    public static class CodecTool
    {
        public const int prec = 1000;
        public const float deprec = 1.0f / prec;
        public const float MAX = 0x7FFFFFFF * deprec;

        public static int ToInt(float value)
        {
            if (value >= MAX)
            {
                Debug.LogErrorFormat("浮点数{0}采用低精度处理时，发现超过了设计的最大值{1}！数据会被截断，请核实。", value, MAX);
                value = MAX;
            }
            return Mathf.RoundToInt(value * prec);
        }

        public static float ToFloat(int value)
        {
            return value * deprec;
        }

        public static int ToVarintBytes(ref byte[] buffer, uint ui)
        {
            int pos = 0;
            while (true)
            {
                byte b = (byte)(ui & 0x7F);
                ui = ui >> 7;
                if (ui == 0)
                {
                    buffer[pos++] = b;
                    break;
                }
                else
                {
                    b |= 0x80;
                    buffer[pos++] = b;
                }
            }
            return pos;
        }

        public static void OutVarintBytes(byte[] buffer, out uint ui)
        {
            int pos = 0;
            int i = 0;
            int n = 0;
            while (true)
            {
                byte b = buffer[pos++];
                i |= (b & 0x7F) << (n++ * 7);
                if ((b & 0x80) == 0) break;
                else if (n >= 5) throw new Exception("解码uint的字节数溢出了！！");
            }
            ui = (uint)i;
        }

        public static int ToVarintBytes(ref byte[] buffer, int i)
        {
            if (i < 0) i = (-i << 1) - 1;
            else i = i << 1;
            return ToVarintBytes(ref buffer, (uint)i);
        }

        public static void OutVarintBytes(byte[] buffer, out int i)
        {
            uint ui = 0;
            OutVarintBytes(buffer, out ui);
            if ((ui & 1) == 1) i = -(int)((ui + 1) >> 1);
            else i = (int)(ui >> 1);
        }

        public static int ToVarintBytes(ref byte[] buffer, float f)
        {
            return ToVarintBytes(ref buffer, ToInt(f));
        }

        public static void OutVarintBytes(byte[] buffer, out float f)
        {
            int i = 0;
            OutVarintBytes(buffer, out i);
            f = ToFloat(i);
        }

        public static int ToVarintBytes(ref byte[] buffer, UInt64 ui64)
        {
            int pos = 0;
            while (true)
            {
                byte b = (byte)(ui64 & 0x7F);
                ui64 = ui64 >> 7;
                if (ui64 == 0)
                {
                    buffer[pos++] = b;
                    break;
                }
                else
                {
                    b |= 0x80;
                    buffer[pos++] = b;
                }
            }
            return pos;
        }

        public static void OutVarintBytes(byte[] buffer, out UInt64 ui64)
        {
            int pos = 0;
            int i = 0;
            int n = 0;
            while (true)
            {
                byte b = buffer[pos++];
                i |= (b & 0x7F) << (n++ * 7);
                if ((b & 0x80) == 0) break;
                else if (n >= 10) throw new Exception("解码uint的字节数溢出了！！");
            }
            ui64 = (uint)i;
        }
    }
}
