using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eos.ViewModels.Dialogs
{
    public class DialogViewModel : ViewModelBase
    {
        public string Title => GetWindowTitle();
        public int DefaultWidth => GetDefaultWidth();
        public int DefaultHeight => GetDefaultHeight();
        public bool CanResize => GetCanResize();

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

        protected virtual bool GetCanResize()
        {
            return false;
        }
    }
}
