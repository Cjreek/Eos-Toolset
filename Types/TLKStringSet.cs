using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public class TLKStringSet
    {
        private Dictionary<TLKLanguage, TLKString> strings;
        public TLKStringSet()
        {
            this.strings = new Dictionary<TLKLanguage, TLKString>();
            this.strings.Add(TLKLanguage.English, new TLKString());
            this.strings.Add(TLKLanguage.French, new TLKString());
            this.strings.Add(TLKLanguage.German, new TLKString());
            this.strings.Add(TLKLanguage.Italian, new TLKString());
            this.strings.Add(TLKLanguage.Spanish, new TLKString());
            this.strings.Add(TLKLanguage.Polish, new TLKString());
            this.strings.Add(TLKLanguage.Korean, new TLKString());
            this.strings.Add(TLKLanguage.ChineseTraditional, new TLKString());
            this.strings.Add(TLKLanguage.ChineseSimplified, new TLKString());
            this.strings.Add(TLKLanguage.Japanese, new TLKString());
        }

        public TLKString this[TLKLanguage index]
        {
            get { return strings[index]; }
        }

        public void SetDefault(String value, String? valueFemale = null)
        {
            strings[TLKLanguage.English].Text = value;
            strings[TLKLanguage.English].TextF = valueFemale ?? value;
        }

        public void SetAll(String value, String? valueFemale = null)
        {
            foreach (TLKLanguage lng in Enum.GetValues(typeof(TLKLanguage)))
            {
                strings[lng].Text = value;
                strings[lng].TextF = valueFemale ?? value;
            }
        }

        public static implicit operator string(TLKStringSet tlkSet) => tlkSet[TLKLanguage.English].Text;
    }
}
