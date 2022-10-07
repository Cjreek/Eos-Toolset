using Eos.Config;
using Eos.Repositories;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
            MasterRepository.Load();
        }
    }
}
