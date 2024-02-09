using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Bif
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct KeyHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileVersion;
        public UInt32 BIFCount;
        public UInt32 KeyCount;
        public UInt32 OffsetToFileTable;
        public UInt32 OffsetToKeyTable;
        public UInt32 BuildYear;
        public UInt32 BuildDay;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct KeyFileEntry
    {
        public UInt32 FileSize;
        public UInt32 FilenameOffset;
        public UInt16 FilenameSize;
        public UInt16 Drives;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct KeyKeyEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] ResRef;
        public NWNResourceType ResourceType;
        public UInt32 ResID;
    }

    internal class BifResourceKey
    {
        public String SourceBif { get; set; } = "";
        public String ResRef { get; set; } = "";
        public NWNResourceType ResourceType { get; set; }
        public UInt32 BifIndex { get; set; }
    }

    internal class KeyFile : IEnumerable<(String? resRef, NWNResourceType type)>
    {
        private Dictionary<(String? resRef, NWNResourceType type), BifResourceKey> _resourceKeys = new Dictionary<(String? resRef, NWNResourceType type), BifResourceKey>();

        public void Load(String filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
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
            BinaryReader sr = new BinaryReader(stream);

            KeyHeader header = BinaryHelper.Read<KeyHeader>(sr);

            sr.BaseStream.Seek(header.OffsetToFileTable, SeekOrigin.Begin);
            var fileEntryList = new List<KeyFileEntry>();
            for (int i = 0; i < header.BIFCount; i++)
            {
                var fileEntry = BinaryHelper.Read<KeyFileEntry>(sr);
                fileEntryList.Add(fileEntry);
            }

            var filenames = new List<String>();
            for (int i = 0; i < fileEntryList.Count; i++)
            {
                sr.BaseStream.Seek(fileEntryList[i].FilenameOffset, SeekOrigin.Begin);
                var filename = BinaryHelper.ReadString(sr, fileEntryList[i].FilenameSize).Replace('\\', Path.DirectorySeparatorChar);
                filenames.Add(filename);
            }

            _resourceKeys.Clear();
            sr.BaseStream.Seek(header.OffsetToKeyTable, SeekOrigin.Begin);
            for (int i = 0; i < header.KeyCount; i++)
            {
                var keyEntry = BinaryHelper.Read<KeyKeyEntry>(sr);

                BifResourceKey resKey = new BifResourceKey();
                resKey.ResRef = new String(keyEntry.ResRef).Trim('\x00');
                resKey.ResourceType = keyEntry.ResourceType;
                resKey.BifIndex = keyEntry.ResID & 0xFFFFF;
                resKey.SourceBif = filenames[(int)(keyEntry.ResID >> 20)];

                if (!_resourceKeys.ContainsKey((resKey.ResRef, resKey.ResourceType)))
                    _resourceKeys.Add((resKey.ResRef, resKey.ResourceType), resKey);
            }
        }
        public BifResourceKey this[String? resRef, NWNResourceType type]
        {
            get
            {
                resRef = resRef?.ToLower();
                return _resourceKeys[(resRef, type)];
            }
        }

        public bool Contains(String? resRef, NWNResourceType type)
        {
            resRef = resRef?.ToLower();
            return _resourceKeys.ContainsKey((resRef, type));
        }

        public IEnumerator<(string? resRef, NWNResourceType type)> GetEnumerator()
        {
            return _resourceKeys.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _resourceKeys.Keys.GetEnumerator();
        }
    }
}
