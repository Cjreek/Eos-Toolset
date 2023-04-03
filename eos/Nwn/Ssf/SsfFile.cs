using Eos.Nwn.Tlk;
using Eos.Nwn.TwoDimensionalArray;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Ssf
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SsfHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileVersion;
        public UInt32 EntryCount;
        public UInt32 TableOffset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public byte[] Padding;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SsfDataEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] ResRef;
        public UInt32 StringRef;
    }

    public class SsfFile
    {
        public List<SsfDataEntry> dataList = new List<SsfDataEntry>();

        public SsfFile() { }
        public SsfFile(Stream stream)
        {
            Load(stream);
        }

        public SsfFile(String filename)
        {
            Load(filename);
        }

        public List<SsfDataEntry> Data { get { return dataList; } }

        public void Load(String filename)
        {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            try
            {
                Load(fs);
            }
            finally
            {
                fs.Close();
            }
        }

        public void Load(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.GetEncoding(1252));
            var header = BinaryHelper.Read<SsfHeader>(reader);

            var offsetArray = new int[header.EntryCount];

            stream.Seek(header.TableOffset, SeekOrigin.Begin);
            var offsetArrayRaw = reader.ReadBytes((int)header.EntryCount * sizeof(UInt32));
            Buffer.BlockCopy(offsetArrayRaw, 0, offsetArray, 0, offsetArrayRaw.Length);

            foreach (var offset in offsetArray)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                var entry = BinaryHelper.Read<SsfDataEntry>(reader);
                Data.Add(entry);
            }
        }
    }
}
