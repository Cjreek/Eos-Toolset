using Eos.Nwn.Ssf;
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

            // 2 = true-color, 3 = grayscale, 10/11 = their RLE variants
            var isRle = header.ImageType == 10 || header.ImageType == 11;
            var baseType = isRle ? header.ImageType - 8 : header.ImageType;
            if (baseType != 2 && baseType != 3)
                throw new NotSupportedException($"Unsupported TGA image type {header.ImageType}");
            if (header.BitsPerPixel != 8 && header.BitsPerPixel != 16 && header.BitsPerPixel != 24 && header.BitsPerPixel != 32)
                throw new NotSupportedException($"Unsupported TGA bit depth {header.BitsPerPixel}");

            Width = header.ImageWidth;
            Height = header.ImageHeight;
            BitsPerPixel = header.BitsPerPixel;

            var srcBpp = (header.BitsPerPixel + 7) / 8;
            var pixelCount = Width * Height;

            reader.ReadBytes(header.ImageIdLength);
            reader.ReadBytes(header.PaletteLength * ((header.PaletteColorDepth + 7) / 8));

            // Decode to a flat buffer of source pixels (missing data in truncated files stays zero)
            var src = new byte[pixelCount * srcBpp];
            if (isRle)
            {
                var pos = 0;
                try
                {
                    while (pos < src.Length)
                    {
                        var packet = reader.ReadByte();
                        var count = (packet & 0x7F) + 1;
                        if ((packet & 0x80) != 0)
                        {
                            var pixel = reader.ReadBytes(srcBpp);
                            for (int i = 0; i < count && pos < src.Length; i++, pos += srcBpp)
                                Array.Copy(pixel, 0, src, pos, Math.Min(pixel.Length, srcBpp));
                        }
                        else
                        {
                            var data = reader.ReadBytes(count * srcBpp);
                            var n = Math.Min(data.Length, src.Length - pos);
                            Array.Copy(data, 0, src, pos, n);
                            pos += count * srcBpp;
                        }
                    }
                }
                catch (EndOfStreamException) { }
            }
            else
            {
                var data = reader.ReadBytes(src.Length);
                Array.Copy(data, src, data.Length);
            }

            // Convert to output format
            var dstBpp = force32Bit ? 4 : srcBpp;
            byte[] dst;
            if (!force32Bit || srcBpp == 4)
                dst = src;
            else
            {
                dst = new byte[pixelCount * 4];
                for (int i = 0, s = 0, d = 0; i < pixelCount; i++, s += srcBpp, d += 4)
                {
                    switch (srcBpp)
                    {
                        case 1: // Grayscale
                            dst[d] = dst[d + 1] = dst[d + 2] = src[s];
                            dst[d + 3] = 0xFF;
                            break;
                        case 2: // A1R5G5B5
                            var v = src[s] | (src[s + 1] << 8);
                            dst[d] = (byte)((v & 0x1F) * 255 / 31);
                            dst[d + 1] = (byte)(((v >> 5) & 0x1F) * 255 / 31);
                            dst[d + 2] = (byte)(((v >> 10) & 0x1F) * 255 / 31);
                            dst[d + 3] = 0xFF;
                            break;
                        case 3: // BGR24
                            dst[d] = src[s];
                            dst[d + 1] = src[s + 1];
                            dst[d + 2] = src[s + 2];
                            dst[d + 3] = 0xFF;
                            break;
                    }
                }
                BitsPerPixel = 32;
            }

            StrideSize = dstBpp * Width;
            ImageSize = StrideSize * Height;

            // Orientation: attribute bit 5 = top-to-bottom, bit 4 = right-to-left
            var topToBottom = (header.Attributes & 0x20) != 0;
            var rightToLeft = (header.Attributes & 0x10) != 0;

            ImageData = new byte[ImageSize];
            for (int y = 0; y < Height; y++)
            {
                var srcRow = topToBottom ? y : Height - 1 - y;
                if (!rightToLeft)
                    Array.Copy(dst, srcRow * StrideSize, ImageData, y * StrideSize, StrideSize);
                else
                    for (int x = 0; x < Width; x++)
                        Array.Copy(dst, srcRow * StrideSize + (Width - 1 - x) * dstBpp, ImageData, y * StrideSize + x * dstBpp, dstBpp);
            }
        }
    }
}
