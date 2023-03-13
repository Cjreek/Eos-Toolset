using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eos.Config;
using Eos.Nwn;
using Eos.Repositories;
using Eos.ViewModels;
using Eos.Views;
using Nwn.Tga;
using System.IO;
using System.Runtime.InteropServices;

namespace Eos
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Startup += Desktop_Startup;
                desktop.Exit += Desktop_Exit;

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_Startup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            EosConfig.Load();

            MasterRepository.Initialize(EosConfig.NwnBasePath);
            MasterRepository.Resources.RegisterResourceLoader(NWNResourceType.TGA, TargaResourceLoader);
            MasterRepository.Resources.RegisterResourceLoader(NWNResourceType.NSS, ScriptSourceLoader);

            MasterRepository.Load();
        }

        private object? TargaResourceLoader(Stream stream)
        {
            Bitmap? result = null;
            if (stream.Length > 0)
            {
                TargaImage tga = new TargaImage(stream, true);

                PixelFormat pf = PixelFormats.Gray8;
                switch (tga.BitsPerPixel)
                {
                    case 8: pf = PixelFormats.Gray8; break; // Wrong
                    case 16: pf = PixelFormats.Gray8; break; // Wrong
                    case 24: pf = PixelFormats.Bgra8888; break; // Wrong
                    case 32: pf = PixelFormats.Bgra8888; break; // bgr32
                }

                var imageDataPtr = GCHandle.Alloc(tga.ImageData, GCHandleType.Pinned);
                try
                {
                    result = new Bitmap(pf, AlphaFormat.Unpremul, imageDataPtr.AddrOfPinnedObject(), PixelSize.FromSize(new Size(tga.Width, tga.Height), 1.0), new Vector(96, 96), tga.StrideSize);
                }
                finally 
                { 
                    imageDataPtr.Free(); 
                }
            }
         
            return result;
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

        private void Desktop_Exit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            EosConfig.Save();
            MasterRepository.Cleanup();
        }
    }
}
