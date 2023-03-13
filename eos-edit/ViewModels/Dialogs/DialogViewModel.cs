using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    public class DialogViewModel : ViewModelBase
    {
        public string Title => GetWindowTitle();
        public int DefaultWidth => GetDefaultWidth();
        public int DefaultHeight => GetDefaultHeight();
        public bool CanResize => GetCanResize();
        public bool AutoSize => GetAutosize();

        protected virtual String GetWindowTitle()
        {
            return "Window Title";
        }

        protected virtual int GetDefaultWidth()
        {
            return 400;
        }

        protected virtual int GetDefaultHeight()
        {
            return 200;
        }

        protected virtual int GetMinWidth()
        {
            return 0;
        }

        protected virtual int GetMinHeight()
        {
            return 0;
        }

        protected virtual int GetMaxWidth()
        {
            return int.MaxValue;
        }

        protected virtual int GetMaxHeight()
        {
            return int.MaxValue;
        }

        protected virtual bool GetCanResize()
        {
            return false;
        }

        protected virtual bool GetAutosize()
        {
            return false;
        }
    }
}
