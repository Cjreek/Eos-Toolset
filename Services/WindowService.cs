using Eos.ViewModels;
using Eos.ViewModels.Dialogs;
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
        private static Dictionary<DialogViewModel, Window> windowDict = new Dictionary<DialogViewModel, Window>();

        private static Window NewWindow(DialogViewModel viewModel)
        {
            ViewWindow window = new ViewWindow();
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Width = viewModel.DefaultWidth;
            window.Height = viewModel.DefaultHeight;
            window.Content = viewModel;
            window.DataContext = viewModel;

            return window;
        }

        public static void OpenWindow<T>(T vm) where T : DialogViewModel, new()
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

        public static void OpenWindow<T>() where T : DialogViewModel, new()
        {
            OpenWindow(new T());
        }

        public static void OpenDialog<T>(T vm) where T : DialogViewModel
        {
            if (windowDict.TryGetValue(vm, out Window? window))
            {
                window.BringIntoView();
            }
            else
            {
                window = NewWindow(vm);
                if (!vm.CanResize)
                {
                    window.WindowStyle = WindowStyle.SingleBorderWindow;
                    window.ResizeMode = ResizeMode.NoResize;
                }
                windowDict[vm] = window;

                window.ShowDialog();
            }
        }

        public static void OpenDialog<T>() where T : DialogViewModel, new()
        {
            OpenDialog(new T());
        }

        public static void Close<T>(T vm) where T : DialogViewModel
        {
            if (windowDict.TryGetValue(vm, out Window? window))
            {
                window.Close();
                windowDict.Remove(vm);
            }
        }
    }
}
