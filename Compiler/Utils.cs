using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Compiler
{
    public static class Utils
    {
        public static NameDefStatement FindNameDefsList(List<NameDefStatement> list, NameToken value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name.Value == value.Value)
                {
                    return list[i];
                }
            }
            return null;
        }

        public static void Write(List<byte> list, uint value, int size)
        {
            while (size > 0)
            {
                list.Add((byte)(value & 0xff));
                value >>= 8;
                size--;
            }
        }

        public static void Write(List<byte> list, string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                list.Add((byte)str[i]);
            }
            list.Add(0);
        }

        public static void Write(List<byte> list, string str, int size)
        {
            int i;
            for (i = 0; i < str.Length; i++)
            {
                list.Add((byte)str[i]);
            }
            while (i < size)
            {
                list.Add(0);
                i++;
            }
        }

        public static void Rewrite(List<byte> list, uint value, int size, int offset)
        {
            while (size > 0)
            {
                list[offset] = (byte)(value & 0xff);
                value >>= 8;
                size--;
                offset++;
            }
        }
    }
}