using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Bif
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BifHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileVersion;
        public UInt32 VariableResourceCount;
        public UInt32 FixedResourceCount;
        public UInt32 VariableTableOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct VariableResourceEntry
    {
        public UInt32 ID;
        public UInt32 Offset;
        public UInt32 FileSize;
        public UInt32 ResourceType;
    }

    internal class BifFile
    {
        private FileStream? fileStream = null;
        private BinaryReader? reader = null;
        private BifHeader header;

        ~BifFile()
        {
            Close();
        }

        public void Load(String bifFile)
        {
            fileStream = new FileStream(bifFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            reader = new BinaryReader(fileStream);
            header = BinaryHelper.Read<BifHeader>(reader);
        }

        public Stream Read(int resourceIndex)
        {
            if ((fileStream == null) || (reader == null))
                throw new Exception();

            if (resourceIndex >= header.VariableResourceCount)
                throw new Exception();

            reader.BaseStream.Seek(header.VariableTableOffset + (resourceIndex * Marshal.SizeOf<VariableResourceEntry>()), SeekOrigin.Begin);
            var resourceEntry = BinaryHelper.Read<VariableResourceEntry>(reader);

            reader.BaseStream.Seek(resourceEntry.Offset, SeekOrigin.Begin);
            return new MemoryStream(reader.ReadBytes((int)resourceEntry.FileSize));
        }

        public void Close()
        {
            reader?.Close();
            reader = null;
            fileStream = null;
        }
    }
}
