using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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

        public int? OriginalIndex { get; set; }

        public static implicit operator string(TLKStringSet tlkSet) => tlkSet[TLKLanguage.English].Text;

        private String GetIsoName(TLKLanguage lang)
        {
            var name = Enum.GetName(lang);
            if (name == null) throw new MissingFieldException();
            return ((ISOAttribute?)(typeof(TLKLanguage).GetField(name)?.GetCustomAttributes(typeof(ISOAttribute), false).FirstOrDefault()))?.ISOCode ?? "";
        }

        public JsonObject? ToJson()
        {
            var tlkJson = new JsonObject();
            tlkJson.Add("Index", this.OriginalIndex);
            foreach (var lang in Enum.GetValues<TLKLanguage>())
            {
                if (this[lang].Text != "")
                {
                    var lngObj = new JsonObject();
                    lngObj.Add("M", this[lang].Text);
                    if (this[lang].Text != this[lang].TextF)
                        lngObj.Add("F", this[lang].TextF);

                    tlkJson.Add(GetIsoName(lang), lngObj);
                }
            }

            return tlkJson;
        }

        public void FromJson(JsonObject? tlkJson)
        {
            if (tlkJson == null) tlkJson = new JsonObject();

            this.OriginalIndex = tlkJson["Index"]?.GetValue<int?>();
            foreach (var lang in Enum.GetValues<TLKLanguage>())
            {
                var iso = GetIsoName(lang);
                if (tlkJson.ContainsKey(iso))
                {
                    this[lang].Text = tlkJson[iso]?.AsObject()?["M"]?.GetValue<String>() ?? "";
                    this[lang].TextF = tlkJson[iso]?.AsObject()?["F"]?.GetValue<String>() ?? this[lang].Text;
                }
                else
                {
                    this[lang].Text = "";
                    this[lang].TextF = "";
                }
            }
        }
    }
}
