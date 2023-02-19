using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn
{
    public enum NWNResourceType : ushort
    {
        BMP = 1, TGA = 3, WAV = 4, PLT = 6, INI = 7, TXT = 10,
        MDL = 2002, NSS = 2009, NCS = 2010, ARE = 2012, SET = 2013, IFO = 2014, BIC = 2015, WOK = 2016,
        TWODA = 2017, TXI = 2022, GIT = 2023, UTI = 2025, UTC = 2027, DLG = 2029, ITP = 2030, UTT = 2032,
        DDS = 2033, UTS = 2035, LTR = 2036, GFF = 2037, FAC = 2038, UTE = 2040, UTD = 2042, UTP = 2044,
        DFT = 2045, GIC = 2046, GUI = 2047, UTM = 2051, DWK = 2052, PWK = 2053, JRL = 2056, UTW = 2058,
        SSF = 2060, NDB = 2064, PTM = 2065, PTT = 2066,
        INVALID_RESOURCE_TYPE = 0xFFFF
    }
}
