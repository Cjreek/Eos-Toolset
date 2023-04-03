using Eos.Nwn.Bif;
using Eos.Nwn.Ssf;
using Eos.Nwn.Tlk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Eos.Nwn.Erf
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ErfHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FileVersion;
        public UInt32 LanguageCount;
        public UInt32 LocalizedStringSize;
        public UInt32 EntryCount;
        public UInt32 OffsetToLocalizedString;
        public UInt32 OffsetToKeyList;
        public UInt32 OffsetToResourceList;
        public UInt32 BuildYear;
        public UInt32 BuildDay;
        public UInt32 DescriptionStrRef;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 116)]
        public byte[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ErfKeyElement
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] ResRef;
        public UInt32 ResID;
        public NWNResourceType ResType;
        public UInt16 Unused;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ErfStringElement
    {
        public UInt32 LanguageID;
        public UInt32 StringSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ErfResourceElement
    {
        public UInt32 OffsetToResource;
        public UInt32 ResourceSize;
    }

    public class ErfResource
    {
        public String ResRef { get; set; } = "";
        public NWNResourceType ResourceType { get; set; }
    }

    public enum ErfType
    {
        ERF, HAK, MOD, SAV
    }

    public class ErfFile
    {
        private static Dictionary<ErfType, String> ErfFileTypeStrings = new Dictionary<ErfType, string>
        {
            { ErfType.ERF, "ERF " },
            { ErfType.HAK, "HAK " },
            { ErfType.MOD, "MOD " },
            { ErfType.SAV, "SAV " },
        };

        private class ErfResourceKey
        {
            public UInt32 ID { get; set; }
            public String ResRef { get; set; } = "";
            public NWNResourceType ResourceType { get; set; }
            public UInt32? OffsetToResource { get; set; }
            public UInt32? ResourceSize { get; set; }
            public String? LocalFileName { get; set; }
        }

        private FileStream? fileStream = null;
        private BinaryReader? reader = null;
        private ErfHeader header;

        private Dictionary<(string, NWNResourceType), ErfResourceKey> resourceKeys = new Dictionary<(string, NWNResourceType), ErfResourceKey>();
        private List<ErfResource> resources = new List<ErfResource>();

        private Dictionary<TLKLanguage, Encoding> languageEncodings = new Dictionary<TLKLanguage, Encoding>
        {
            { TLKLanguage.English, Encoding.GetEncoding(1252) },
            { TLKLanguage.French, Encoding.GetEncoding(1252) },
            { TLKLanguage.German, Encoding.GetEncoding(1252) },
            { TLKLanguage.Italian, Encoding.GetEncoding(1252) },
            { TLKLanguage.Spanish, Encoding.GetEncoding(1252) },
            { TLKLanguage.Polish, Encoding.GetEncoding(1250) },
            { TLKLanguage.Korean, Encoding.GetEncoding(949) },
            { TLKLanguage.ChineseTraditional, Encoding.GetEncoding(950) },
            { TLKLanguage.ChineseSimplified, Encoding.GetEncoding(936) },
            { TLKLanguage.Japanese, Encoding.GetEncoding(932) },
        };

        public int Count => resources.Count;
        public ErfResource this[int index] => resources[index];

        public TLKStringSet Description { get; private set; } = new TLKStringSet();

        ~ErfFile()
        {
            Close();
        }

        public void Load(String filename)
        {
            fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            reader = new BinaryReader(fileStream);
            header = BinaryHelper.Read<ErfHeader>(reader);
            LoadKeys();
            LoadDescription();
        }

        public Stream Read(String resRef, NWNResourceType resType)
        {
            if (!resourceKeys.TryGetValue((resRef, resType), out ErfResourceKey? resKey))
                throw new KeyNotFoundException();

            if ((resKey.LocalFileName ?? "") != "")
            {
                return new MemoryStream(File.ReadAllBytes(resKey.LocalFileName ?? ""));
            }
            else
            {
                if ((fileStream == null) || (reader == null))
                    throw new Exception();

                fileStream.Seek(resKey.OffsetToResource ?? 0, SeekOrigin.Begin);
                return new MemoryStream(reader.ReadBytes((int)(resKey.ResourceSize ?? 0)));
            }
        }

        public void AddResource(String resRef, NWNResourceType resType, String sourceFilename)
        {
            if (resourceKeys.ContainsKey((resRef, resType)))
                throw new InvalidDataException();

            var newRes = new ErfResource();
            newRes.ResRef = resRef;
            newRes.ResourceType = resType;
            resources.Add(newRes);

            var newKey = new ErfResourceKey();
            newKey.ResRef = resRef;
            newKey.ResourceType = resType;
            newKey.LocalFileName = sourceFilename;
            resourceKeys.Add((resRef, resType), newKey);
        }

        public void Save(Stream stream, ErfType type = ErfType.HAK)
        {
            var writer = new BinaryWriter(stream);

            var header = new ErfHeader();
            header.FileType = ErfFileTypeStrings[type].ToCharArray();
            header.FileVersion = "V1.0".ToCharArray();
            header.LanguageCount = 0;
            header.LocalizedStringSize = 0;
            header.EntryCount = (UInt32)resources.Count;
            header.OffsetToLocalizedString = 0;
            header.OffsetToKeyList = 0;
            header.OffsetToResourceList = 0;
            header.BuildYear = (UInt32)(DateTime.Now.Year - 1900);
            header.BuildDay = (UInt32)DateTime.Now.DayOfYear;
            header.DescriptionStrRef = 0xFFFFFFFF;
            header.Reserved = new byte[116];
            BinaryHelper.Write(writer, header);

            header.OffsetToLocalizedString = (UInt32)stream.Position;

            UInt32 langCount = 0;
            UInt32 strSegmentSize = 0;
            foreach (var lang in Enum.GetValues<TLKLanguage>())
            {
                if (Description[lang].Text != "")
                {
                    var strElement = new ErfStringElement();
                    strElement.LanguageID = ((UInt32)lang) * 2;
                    strElement.StringSize = (UInt32)Description[lang].Text.Length + 1;
                    BinaryHelper.Write(writer, strElement);
                    BinaryHelper.WriteString(writer, Description[lang].Text, languageEncodings[lang]);

                    strSegmentSize += (UInt32)(Marshal.SizeOf<ErfStringElement>() + strElement.StringSize);
                    langCount++;
                }

                if (Description[lang].TextF != "")
                {
                    var strElement = new ErfStringElement();
                    strElement.LanguageID = ((UInt32)lang) * 2 + 1;
                    strElement.StringSize = (UInt32)Description[lang].TextF.Length + 1;
                    BinaryHelper.Write(writer, strElement);
                    BinaryHelper.WriteString(writer, Description[lang].TextF, languageEncodings[lang]);

                    strSegmentSize += (UInt32)(Marshal.SizeOf<ErfStringElement>() + strElement.StringSize);
                    langCount++;
                }
            }
            header.LanguageCount = langCount;
            header.LocalizedStringSize = strSegmentSize;
            header.OffsetToKeyList = (UInt32)stream.Position;

            for (int i=0; i < resources.Count; i++)
            {
                var res = resources[i];

                var resKey = new ErfKeyElement();
                resKey.ResID = (UInt32)i;
                resKey.ResRef = new char[16];
                res.ResRef.CopyTo(0, resKey.ResRef, 0, res.ResRef.Length);
                resKey.ResType = res.ResourceType;
                resKey.Unused = 0;

                BinaryHelper.Write(writer, resKey);
            }
            header.OffsetToResourceList = (UInt32)stream.Position;

            for (int i = 0; i < resources.Count; i++)
            {
                var res = resources[i];

                var resElement = new ErfResourceElement();
                resElement.OffsetToResource = 0;
                resElement.ResourceSize = 0;
                BinaryHelper.Write(writer, resElement);
            }

            var offsetSizeList = new List<(long offset, long size)>();
            var dataOffset = stream.Position;

            // Write Resource data
            for (int i = 0; i < resources.Count; i++)
            {
                var res = resources[i];
                var dataStream = Read(resources[i].ResRef, resources[i].ResourceType);
                dataStream.CopyTo(stream);

                offsetSizeList.Add((dataOffset, dataStream.Length));
                dataOffset += dataStream.Length;
            }

            // Correct resource element entries
            stream.Seek(header.OffsetToResourceList, SeekOrigin.Begin);
            for (int i = 0; i < resources.Count; i++)
            {
                var res = resources[i];

                var resElement = new ErfResourceElement();
                resElement.OffsetToResource = (UInt32)offsetSizeList[i].offset;
                resElement.ResourceSize = (UInt32)offsetSizeList[i].size;
                BinaryHelper.Write(writer, resElement);
            }

            // Correct header
            stream.Seek(0, SeekOrigin.Begin);
            BinaryHelper.Write(writer, header);
        }

        public void Save(String filename, ErfType type = ErfType.HAK)
        {
            var fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
            try
            {
                Save(fs, type);
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

        private void LoadDescription()
        {
            if (fileStream != null && reader != null)
            {
                Description = new TLKStringSet();

                fileStream.Seek(header.OffsetToLocalizedString, SeekOrigin.Begin);
                while ((fileStream.Position - header.OffsetToLocalizedString) < header.LocalizedStringSize)
                {
                    var str = BinaryHelper.Read<ErfStringElement>(reader);
                    var strData = BinaryHelper.ReadString(reader, (int)str.StringSize);

                    var isFemale = (str.LanguageID % 2) != 0;
                    var actualLanguage = (TLKLanguage)((str.LanguageID & ~0x1) / 2);

                    if (isFemale)
                        Description[actualLanguage].TextF = strData;
                    else
                        Description[actualLanguage].Text = strData;
                }
            }
        }

        private void LoadKeys()
        {
            if (fileStream != null && reader != null)
            {
                resources.Clear();
                resourceKeys.Clear();
                for (int i = 0; i < header.EntryCount; i++)
                {
                    ErfResource tmpResource = new ErfResource();
                    ErfResourceKey erfResourceKey = new ErfResourceKey();

                    fileStream.Seek(header.OffsetToKeyList + Marshal.SizeOf<ErfKeyElement>() * i, SeekOrigin.Begin);
                    var key = BinaryHelper.Read<ErfKeyElement>(reader);

                    fileStream.Seek(header.OffsetToResourceList + Marshal.SizeOf<ErfResourceElement>() * i, SeekOrigin.Begin);
                    var resourceElement = BinaryHelper.Read<ErfResourceElement>(reader);

                    tmpResource.ResRef = new String(key.ResRef).Trim('\0');
                    tmpResource.ResourceType = key.ResType;
                    resources.Add(tmpResource);

                    erfResourceKey.ID = key.ResID;
                    erfResourceKey.ResRef = tmpResource.ResRef;
                    erfResourceKey.ResourceType = key.ResType;
                    erfResourceKey.OffsetToResource = resourceElement.OffsetToResource;
                    erfResourceKey.ResourceSize = resourceElement.ResourceSize;
                    resourceKeys.Add((erfResourceKey.ResRef, erfResourceKey.ResourceType), erfResourceKey);
                }
            }
        }
    }
}
