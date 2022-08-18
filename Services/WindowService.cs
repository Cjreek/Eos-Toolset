using Eos.ViewModels;
using Eos.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eos.Services
{
    public static class WindowService
    {
        private static Dictionary<ViewModelBase, Window> windowDict = new Dictionary<ViewModelBase, Window>();

        private static Window NewWindow(object viewModel)
        {
            ViewWindow window = new ViewWindow();
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.Content = viewModel;
            window.DataContext = viewModel;

            return window;
        }

        public static void OpenWindow<T>(T vm) where T : ViewModelBase, new()
        {
            if (windowDict.TryGetValue(vm, out Window? window))
            {
                window.BringIntoView();
            }
            else
            {
                window = NewWindow(vm);
                windowDict[vm] = window;

                window.Show();
            }
        }

        public static void OpenWindow<T>() where T : ViewModelBase, new()
        {
            OpenWindow(new T());
        }

        public static void OpenDialog<T>(T vm) where T : ViewModelBase, new()
        {
            if (windowDict.TryGetValue(vm, out Window? window))
            {
                window.BringIntoView();
            }
            else
            {
                window = NewWindow(vm);
                window.WindowStyle = WindowStyle.SingleBorderWindow;
                window.ResizeMode = ResizeMode.NoResize;
                windowDict[vm] = window;

                window.ShowDialog();
            }
        }

        public static void OpenDialog<T>() where T : ViewModelBase, new()
        {
            OpenDialog(new T());
        }

        public static void Close<T>(T vm) where T : ViewModelBase, new()
        {
            if (windowDict.TryGetValue(vm, out Window? window))
            {
                window.Close();
                windowDict.Remove(vm);
            }
        }
    }
}
