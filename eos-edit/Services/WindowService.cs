using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Eos.ViewModels;
using Eos.ViewModels.Dialogs;
using Eos.Views;
using Eos.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Eos.Services
{
    public static class WindowService
    {
        private static Dictionary<DialogViewModel, Window> windowDict = new Dictionary<DialogViewModel, Window>();
        private static int cursorWaitCount = 0;

        private static Window NewWindow(DialogViewModel viewModel)
        {
            ViewWindow window = new ViewWindow();

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Width = viewModel.DefaultWidth;
                window.Height = viewModel.DefaultHeight;
                window.Content = viewModel;
                window.DataContext = viewModel;
                window.CanResize = viewModel.CanResize;
                window.ShowInTaskbar = false;
            }

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
                windowDict[vm] = window;

                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    if (desktop.MainWindow != null)
                    {
                        var wasFakeVisible = false;
                        if (!desktop.MainWindow.IsVisible)
                        {
                            desktop.MainWindow.IsVisible = true;
                            wasFakeVisible = true;
                        }

                        using (var source = new CancellationTokenSource())
                        {
                            window.ShowDialog(desktop.MainWindow).ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
                            Dispatcher.UIThread.MainLoop(source.Token);
                            if (wasFakeVisible) desktop.MainWindow.IsVisible = false;
                        }
                    }
                }
            }
        }

        public static MessageBoxResult ShowMessage(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            var msgBox = new MessageBoxViewModel(title, message, icon, buttons);
            WindowService.OpenDialog(msgBox);

            return msgBox.Result;
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

        public static void BeginWaitCursor()
        {
            cursorWaitCount++;
            if (cursorWaitCount == 1)
            {
                // Set Cursor
            }
        }

        public static void EndWaitCursor()
        {
            cursorWaitCount--;
            if (cursorWaitCount <= 0)
            {
                // Reset Cursor
                cursorWaitCount = 0;
            }
        }
    }
}
