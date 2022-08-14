using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    internal class ISOAttribute : Attribute
    {
        public ISOAttribute(string isoCode)
        {
            this.ISOCode = isoCode;
        }
        public String ISOCode { get; set; }
    }

    public enum TLKLanguage : UInt32
    {
        [ISO("EN")]
        English = 0,
        [ISO("FR")]
        French = 1,
        [ISO("DE")]
        German = 2,
        [ISO("IT")]
        Italian = 3,
        [ISO("ES")]
        Spanish = 4,
        [ISO("PL")]
        Polish = 5,
        [ISO("KO")]
        Korean = 128,
        [ISO("ZHT")]
        ChineseTraditional = 129,
        [ISO("ZHS")]
        ChineseSimplified = 130,
        [ISO("JA")]
        Japanese = 131,
    }
}
