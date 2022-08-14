using Eos.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Tlk
{
    [Flags]
    enum TlkStringFlags : UInt32
    {
        TEXT_PRESENT = 0x0001,
        SND_PRESENT = 0x0002,
        SNDLENGTH_PRESENT = 0x0004,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TlkHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileVersion;
        public TLKLanguage LanguageId;
        public UInt32 StringCount;
        public UInt32 StringEntriesOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TlkStringDataElement
    {
        public TlkStringFlags Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] SoundResRef;
        public UInt32 VolumeVariance;
        public UInt32 PitchVariance;
        public UInt32 OffsetToString;
        public UInt32 StringSize;
        public Single SoundLength;
    }

    internal class TlkFile
    {
        private FileStream? fileStream;
        private BinaryReader? reader;
        private TlkHeader header;

        private Dictionary<int, String> cache = new Dictionary<int, string>();

        public void Load(String tlkFile)
        {
            fileStream = new FileStream(tlkFile, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(fileStream, Encoding.Latin1);
            header = BinaryHelper.Read<TlkHeader>(reader);
        }

        public String GetString(int? stringRef)
        {
            if ((fileStream == null) || (reader == null))
                throw new Exception();

            if (stringRef >= header.StringCount)
                throw new IndexOutOfRangeException();

            if (stringRef == null)
                return "";

            int strRef = stringRef ?? -1;
            if (!cache.ContainsKey(strRef))
            {
                reader.BaseStream.Seek(Marshal.SizeOf<TlkHeader>() + (strRef * Marshal.SizeOf<TlkStringDataElement>()), SeekOrigin.Begin);
                var strDataEntry = BinaryHelper.Read<TlkStringDataElement>(reader);

                cache[strRef] = "";
                if ((strDataEntry.Flags & TlkStringFlags.TEXT_PRESENT) != 0)
                {
                    reader.BaseStream.Seek(header.StringEntriesOffset + strDataEntry.OffsetToString, SeekOrigin.Begin);
                    cache[strRef] = BinaryHelper.ReadString(reader, (int)strDataEntry.StringSize);
                }
            }

            return cache[strRef];
        }

        public void Close()
        {
            reader?.Close();
            reader = null;
            fileStream = null;
        }
    }
}
