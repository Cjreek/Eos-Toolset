using Eos.Nwn.Tlk;
using Eos.Nwn.TwoDimensionalArray;
using Eos.Types;
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

        public SsfFile() 
        {
            for (int i = 0; i <= (int)SoundsetEntryType.THREATEN; i++)
                dataList.Add(new SsfDataEntry() { ResRef = new char[16] });
        }

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

            Data.Clear();
            foreach (var offset in offsetArray)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                var entry = BinaryHelper.Read<SsfDataEntry>(reader);
                Data.Add(entry);
            }
        }

        public void Save(Stream stream)
        {
            var writer = new BinaryWriter(stream, Encoding.GetEncoding(1252));

            var header = new SsfHeader();
            header.FileType = "SSF ".ToCharArray();
            header.FileVersion = "V1.0".ToCharArray();
            header.EntryCount = (uint)dataList.Count;
            header.TableOffset = (uint)Marshal.SizeOf<SsfHeader>();
            header.Padding = new byte[24];

            BinaryHelper.Write(writer, header);

            var dataOffset = header.TableOffset + (header.EntryCount * sizeof(UInt32));
            for (int i = 0; i < header.EntryCount; i++)
                writer.Write((uint)(dataOffset + i * Marshal.SizeOf<SsfDataEntry>()));

            for (int i = 0; i < header.EntryCount; i++)
                BinaryHelper.Write(writer, dataList[i]);
        }

        public void Save(string filename)
        {
            var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            try
            {
                Save(fs);
            }
            finally
            {
                fs.Close();
            }
        }

        public void Set(SoundsetEntryType type, int? strRef, string soundResRef = "")
        {
            var soundResRefArray = soundResRef.ToCharArray(0, Math.Min(soundResRef.Length, 16));
            var resRefArray = new char[16];
            for (int i = 0; (i < soundResRefArray.Length) && (i < resRefArray.Length); i++)
                resRefArray[i] = soundResRefArray[i];

            dataList[(int)type] = new SsfDataEntry() { ResRef = resRefArray, StringRef = (uint?)strRef ?? 0xFFFFFFFF };
        }
    }
}
