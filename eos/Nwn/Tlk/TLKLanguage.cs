using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn.Tlk
{
    internal class ISOAttribute : Attribute
    {
        public ISOAttribute(string isoCode)
        {
            ISOCode = isoCode;
        }
        public string ISOCode { get; set; }
    }

    public enum TLKLanguage : uint
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
    }
}
