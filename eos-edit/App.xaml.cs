using Nwn.Tga;
using Eos.Config;
using Eos.Nwn;
using Eos.Repositories;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Eos
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            EosConfig.Save();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            EosConfig.Load();

            MasterRepository.Initialize(EosConfig.NwnBasePath);
            MasterRepository.Resources.RegisterResourceLoader(NWNResourceType.TGA, TargaResourceLoader);
            MasterRepository.Resources.RegisterResourceLoader(NWNResourceType.NSS, ScriptSourceLoader);

            MasterRepository.Load();
        }

        private object? TargaResourceLoader(Stream stream)
        {
            if (stream.Length > 0)
            {
                TargaImage tga = new TargaImage(stream);

                PixelFormat pf = PixelFormats.Default;
                switch (tga.BitsPerPixel)
                {
                    case 8: pf = PixelFormats.Gray8; break;
                    case 16: pf = PixelFormats.Bgr555; break;
                    case 24: pf = PixelFormats.Bgr24; break;
                    case 32: pf = PixelFormats.Bgra32; break; // bgr32
                }

                var bmp = BitmapSource.Create(tga.Width, tga.Height, 96, 96, pf, null, tga.ImageData, tga.StrideSize);
                bmp.Freeze();
                //tga.Image?.Freeze();
                //return tga.Image;
                return bmp;
            }
            else
                return null;
        }

        private object? ScriptSourceLoader(Stream stream)
        {
            if (stream.Length > 0)
            {
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            else
                return null;
        }
    }
}
