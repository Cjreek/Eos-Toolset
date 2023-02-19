using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Tlk
{
    internal class TlkCollection
    {
        private Dictionary<TLKLanguage, TlkPair> tlkFiles = new Dictionary<TLKLanguage, TlkPair>();

        public void Load(String nwnBasePath)
        {
            if (!Path.EndsInDirectorySeparator(nwnBasePath))
                nwnBasePath += Path.DirectorySeparatorChar;

            var lngType = typeof(TLKLanguage);
            foreach (TLKLanguage lang in Enum.GetValues(typeof(TLKLanguage)))
            {
                var enumValueStr = Enum.GetName(lngType, lang) ?? "INVALID_ENUM_VALUE";
                var enumValueField = lngType.GetField(enumValueStr);
                if (enumValueField != null)
                {
                    var isoAttr = (ISOAttribute?)enumValueField.GetCustomAttributes(typeof(ISOAttribute), false).FirstOrDefault();
                    if (isoAttr != null)
                    {
                        var langDir = nwnBasePath + @"lang" + Path.DirectorySeparatorChar + isoAttr.ISOCode.ToLower();
                        if (Directory.Exists(langDir))
                        {
                            var tlk = new TlkPair();
                            tlk.Load(langDir + Path.DirectorySeparatorChar + "data" + Path.DirectorySeparatorChar + "dialog.tlk");
                            tlkFiles.Add(lang, tlk);
                        }
                    }
                }
            }
        }

        public String GetString(TLKLanguage language, bool female, int? stringRef)
        {
            if (!tlkFiles.ContainsKey(language))
                return "";
            return tlkFiles[language].GetString(female, stringRef);
        }
    }
}
