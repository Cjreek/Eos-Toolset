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

        public static String ReadString(BinaryReader stream, int length)
        {
            return new String(stream.ReadChars(length));
        }
    }
}
