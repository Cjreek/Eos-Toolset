using Eos.Services;
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
        private List<String> strings = new List<string>();
        private Dictionary<TLKLanguage, Encoding> languageEncodings = new Dictionary<TLKLanguage, Encoding>
        {
            { TLKLanguage.English, Encoding.GetEncoding(1252) },
            { TLKLanguage.French, Encoding.GetEncoding(1252) },
            { TLKLanguage.German, Encoding.GetEncoding(1252) },
            { TLKLanguage.Italian, Encoding.GetEncoding(1252) },
            { TLKLanguage.Spanish, Encoding.GetEncoding(1252) },
            { TLKLanguage.Polish, Encoding.GetEncoding(1250) },
        };

        public void New(TLKLanguage language)
        {
            header = new TlkHeader();
            header.FileType = "TLK ".ToCharArray();
            header.FileVersion = "V3.0".ToCharArray();
            header.LanguageId = language;
            strings.Add("Bad Strref");
        }

        public void Load(String tlkFile, bool writeable = false)
        {
            using (fileStream = new FileStream(tlkFile, FileMode.Open, FileAccess.Read))
            {
                reader = new BinaryReader(fileStream, Encoding.Default);
                header = BinaryHelper.Read<TlkHeader>(reader);
            };

            fileStream = new FileStream(tlkFile, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(fileStream, languageEncodings[header.LanguageId]);
            if (writeable)
            {
                int lastNonNullIndex = -1;
                for (int i = 0; i < header.StringCount; i++)
                {
                    if (GetString(i) != "")
                        lastNonNullIndex = i;
                }

                for (int i = 0; i <= lastNonNullIndex; i++)
                {
                    strings.Add(GetString(i));
                }
            }
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

        public int AddText(String text, bool forceAdd = false)
        {
            if (forceAdd)
            {
                strings.Add(text);
                return strings.Count - 1;
            }
            else
            {
                var result = strings.IndexOf(text);
                if (result == -1)
                {
                    strings.Add(text);
                    result = strings.Count - 1;
                }

                return result;
            }
        }

        public void PadTo(int nIndex)
        {
            while (strings.Count < nIndex)
            {
                AddText("", true);
            }
        }

        public void Save(Stream stream)
        {
            var writer = new BinaryWriter(stream);

            header.StringCount = (UInt32)strings.Count;
            header.StringEntriesOffset = (UInt32)(Marshal.SizeOf<TlkHeader>() + (header.StringCount * Marshal.SizeOf<TlkStringDataElement>()));
            BinaryHelper.Write(writer, header);

            var offset = (UInt32)0;
            for (int i = 0; i < strings.Count; i++)
            {
                var strEntry = new TlkStringDataElement();
                strEntry.SoundResRef = new char[16];
                strEntry.Flags = TlkStringFlags.TEXT_PRESENT;
                strEntry.VolumeVariance = 0;
                strEntry.PitchVariance = 0;
                strEntry.OffsetToString = offset;
                strEntry.StringSize = (UInt32)languageEncodings[header.LanguageId].GetByteCount(strings[i]); //(UInt32)strings[i].Length;
                strEntry.SoundLength = 0;
                BinaryHelper.Write(writer, strEntry);

                offset += strEntry.StringSize;
            }

            for (int i = 0; i < strings.Count; i++)
            {
                BinaryHelper.WriteString(writer, strings[i], languageEncodings[header.LanguageId], false);
            }
        }

        public void Save(String filename)
        {
            var fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
            try
            {
                Save(fs);
            }
            finally
            {
                fs.Close();
            }
        }

        public void Close()
        {
            reader?.Close();
            reader = null;
            fileStream = null;
        }
    }
}
