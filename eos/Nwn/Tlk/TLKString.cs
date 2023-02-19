using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Tlk
{
    public delegate void NotifyPropertyChangedDelegate();

    public class TLKString
    {
        private string _text = "";
        private string _textF = "";
        private NotifyPropertyChangedDelegate? changedCallback;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    changedCallback?.Invoke();
                }
            }
        }

        public string TextF
        {
            get { return _textF; }
            set
            {
                if (_textF != value)
                {
                    _textF = value;
                    changedCallback?.Invoke();
                }
            }
        }

        public TLKString(NotifyPropertyChangedDelegate? changedCallback)
        {
            this.changedCallback = changedCallback;
        }
    }
}
