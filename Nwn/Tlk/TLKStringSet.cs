﻿using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Nwn.Tlk
{
    public class TLKStringSet
    {
        private Dictionary<TLKLanguage, TLKString> strings;
        private NotifyPropertyChangedDelegate? changedCallback;

        public TLKStringSet(NotifyPropertyChangedDelegate? changedCallback = null)
        {
            strings = new Dictionary<TLKLanguage, TLKString>();
            strings.Add(TLKLanguage.English, new TLKString(changedCallback));
            strings.Add(TLKLanguage.French, new TLKString(changedCallback));
            strings.Add(TLKLanguage.German, new TLKString(changedCallback));
            strings.Add(TLKLanguage.Italian, new TLKString(changedCallback));
            strings.Add(TLKLanguage.Spanish, new TLKString(changedCallback));
            strings.Add(TLKLanguage.Polish, new TLKString(changedCallback));
            strings.Add(TLKLanguage.Korean, new TLKString(changedCallback));
            strings.Add(TLKLanguage.ChineseTraditional, new TLKString(changedCallback));
            strings.Add(TLKLanguage.ChineseSimplified, new TLKString(changedCallback));
            strings.Add(TLKLanguage.Japanese, new TLKString(changedCallback));

            this.changedCallback = changedCallback;
        }

        public TLKString this[TLKLanguage index]
        {
            get { return strings[index]; }
        }

        public int? OriginalIndex { get; set; }

        public static implicit operator string(TLKStringSet tlkSet) => tlkSet[MasterRepository.Project.DefaultLanguage].Text;

        private string GetIsoName(TLKLanguage lang)
        {
            var name = Enum.GetName(lang);
            if (name == null) throw new MissingFieldException();
            return ((ISOAttribute?)(typeof(TLKLanguage).GetField(name)?.GetCustomAttributes(typeof(ISOAttribute), false).FirstOrDefault()))?.ISOCode ?? "";
        }

        public JsonObject? ToJson()
        {
            var tlkJson = new JsonObject();
            tlkJson.Add("Index", OriginalIndex);
            foreach (var lang in Enum.GetValues<TLKLanguage>())
            {
                if (this[lang].Text != "")
                {
                    var lngObj = new JsonObject();
                    lngObj.Add("M", this[lang].Text);
                    if ((this[lang].Text != this[lang].TextF) && (this[lang].TextF != ""))
                        lngObj.Add("F", this[lang].TextF);

                    tlkJson.Add(GetIsoName(lang), lngObj);
                }
            }

            return tlkJson;
        }

        public void FromJson(JsonObject? tlkJson)
        {
            if (tlkJson == null) tlkJson = new JsonObject();

            OriginalIndex = tlkJson["Index"]?.GetValue<int?>();
            foreach (var lang in Enum.GetValues<TLKLanguage>())
            {
                var iso = GetIsoName(lang);
                if (tlkJson.ContainsKey(iso))
                {
                    this[lang].Text = tlkJson[iso]?.AsObject()?["M"]?.GetValue<string>() ?? "";
                    this[lang].TextF = tlkJson[iso]?.AsObject()?["F"]?.GetValue<string>() ?? this[lang].Text;
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
