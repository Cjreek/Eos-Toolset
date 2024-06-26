﻿using Eos.Nwn.Ssf;
using Eos.Nwn;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Nwn.Tga
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TargaHeader
    {
        public byte ImageIdLength;
        public byte UsesPalette;
        public byte ImageType;
        public ushort PaletteStart;
        public ushort PaletteLength;
        public byte PaletteColorDepth;
        public ushort OriginX;
        public ushort OriginY;
        public ushort ImageWidth;
        public ushort ImageHeight;
        public byte BitsPerPixel;
        public byte Attributes;
    }

    public class TargaImage
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int BitsPerPixel { get; private set; }
        public int StrideSize { get; private set; }
        public int ImageSize { get; private set; }
        public byte[] ImageData { get; private set; } = new byte[0];

        public TargaImage() { }
        public TargaImage(Stream stream, bool force32Bit = false)
        {
            Load(stream, force32Bit);
        }

        public TargaImage(String filename, bool force32Bit = false)
        {
            Load(filename, force32Bit);
        }

        public void Load(String filename, bool force32Bit = false)
        {
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                Load(fs, force32Bit);
            }
            finally
            {
                fs.Close();
            }
        }

        public void Load(Stream stream, bool force32Bit)
        {
            var reader = new BinaryReader(stream);
            var header = BinaryHelper.Read<TargaHeader>(reader);

            Width = header.ImageWidth;
            Height = header.ImageHeight;
            BitsPerPixel = header.BitsPerPixel;

            var bytesPerPixel = (header.BitsPerPixel + 7) / 8;
            StrideSize = bytesPerPixel * header.ImageWidth;
            ImageSize = StrideSize * header.ImageHeight;

            reader.ReadBytes(header.ImageIdLength);
            reader.ReadBytes(header.PaletteLength * (header.PaletteColorDepth / 8)); // Calc palette bytes per pixel

            var rows = new List<byte[]>();
            var row = new List<byte>();
            if (header.ImageType == 10) // RGB24 RLE
            {
                var bytesRead = 0;
                var rowBytesRead = 0;
                while (bytesRead < ImageSize)
                {
                    var rlePacket = reader.ReadByte();
                    var rleType = (rlePacket & 0x80) >> 7;
                    var rleCount = (rlePacket & 0x7F) + 1;

                    if (rleType == 0) // Raw
                    {
                        for (int i = 0; i < rleCount; i++)
                        {
                            row.AddRange(reader.ReadBytes(bytesPerPixel));
                            if ((bytesPerPixel == 3) && (force32Bit))
                                row.Add(0xFF);

                            bytesRead += bytesPerPixel;
                            rowBytesRead += bytesPerPixel;
                            if (rowBytesRead >= StrideSize)
                            {
                                rows.Add(row.ToArray());
                                row = new List<byte>();
                                rowBytesRead = 0;
                            }
                        }
                    }
                    else
                    {
                        var rlePixel = reader.ReadBytes(bytesPerPixel);
                        for (int i = 0; i < rleCount; i++)
                        {
                            row.AddRange(rlePixel);
                            if ((bytesPerPixel == 3) && (force32Bit))
                                row.Add(0xFF);

                            bytesRead += bytesPerPixel;
                            rowBytesRead += bytesPerPixel;
                            if (rowBytesRead >= StrideSize)
                            {
                                rows.Add(row.ToArray());
                                row = new List<byte>();
                                rowBytesRead = 0;
                            }
                        }
                    }
                }
            }
            else if (header.ImageType == 2) // RGB24/32
            {
                if ((BitsPerPixel == 32) || (!force32Bit))
                {
                    for (int y = 0; y < Height; y++)
                        rows.Add(reader.ReadBytes(StrideSize));
                }
                else // Convert 24-Bit Data to 32 bits
                {
                    var buffer = reader.ReadBytes(ImageSize);
                    var rowData = new byte[header.ImageWidth * 4];
                    var rowDataIndex = 0;
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        if (rowDataIndex == header.ImageWidth * 4)
                        {
                            rows.Add(rowData);
                            rowData = new byte[header.ImageWidth * 4];
                            rowDataIndex = 0;
                        }

                        rowData[rowDataIndex] = buffer[i];
                        if (i % 3 == 2)
                        {
                            rowDataIndex++;
                            rowData[rowDataIndex] = 0xFF;
                        }

                        rowDataIndex++;
                    }

                    rows.Add(rowData);
                }
            }

            if (force32Bit)
            {
                BitsPerPixel = 32;
                bytesPerPixel = 4;
                StrideSize = bytesPerPixel * header.ImageWidth;
                ImageSize = StrideSize * header.ImageHeight;
            }

            if (header.OriginY == 0) rows.Reverse();
            if (header.OriginX == 1)
            {
                for (int y = 0; y < rows.Count; y++)
                {
                    var list = new List<byte>();
                    for (int x = 0; x < header.ImageWidth; x++)
                    {
                        var pixel = rows[y].Take(new Range(x * bytesPerPixel, x * bytesPerPixel + bytesPerPixel));
                        list.InsertRange(0, pixel);
                    }

                    rows[y] = list.ToArray();
                }
            }

            ImageData = new byte[ImageSize];
            for (int y = 0; y < Height; y++)
                Array.Copy(rows[y], 0, ImageData, StrideSize * y, StrideSize);
        }
    }
}
