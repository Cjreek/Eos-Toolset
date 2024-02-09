using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn
{
    internal static class BinaryHelper
    {
        public static T? Read<T>(BinaryReader stream)
        {
            var rawBytes = stream.ReadBytes(Marshal.SizeOf<T>());
            GCHandle handle = GCHandle.Alloc(rawBytes, GCHandleType.Pinned);
            try
            {
                T? result = (T?)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                return result;
            }
            finally
            {
                handle.Free();
            }
        }

        public static void Write<T>(BinaryWriter stream, T? data)
        {
            if (data != null)
            {
                var structSize = Marshal.SizeOf<T>();
                var byteArray = new byte[structSize];

                var ptr = Marshal.AllocHGlobal(structSize);
                try
                {
                    Marshal.StructureToPtr(data, ptr, true);
                    Marshal.Copy(ptr, byteArray, 0, structSize);
                    stream.Write(byteArray, 0, structSize);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        public static void Skip(BinaryReader stream, int count)
        {
            stream.ReadBytes(count);
        }

        public static String ReadString(BinaryReader stream, int length)
        {
            return new String(stream.ReadChars(length)).Trim('\0');
        }

        public static void WriteString(BinaryWriter stream, String data, Encoding encoding, bool writeNullbyte = true)
        {
            stream.Write(encoding.GetBytes(data));
            if (writeNullbyte)
                stream.Write('\0');
        }
    }
}
