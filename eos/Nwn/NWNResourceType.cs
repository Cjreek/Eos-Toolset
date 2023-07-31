﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Nwn
{
    public enum NWNResourceType : ushort
    {
        RES = 0, BMP = 1, MVE = 2, TGA = 3, WAV = 4, WFX = 5, PLT = 6, INI = 7, MP3 = 8, MPG = 9, TXT = 10,
        PLH = 2000, TEX = 2001, MDL = 2002, THG = 2003, FNT = 2005, LUA = 2007, SLT = 2008, NSS = 2009, 
        NCS = 2010, MOD = 2011, ARE = 2012, SET = 2013, IFO = 2014, BIC = 2015, WOK = 2016, TWODA = 2017, 
        TLK = 2018, TXI = 2022, GIT = 2023, BTI = 2024, UTI = 2025, BTC = 2026, UTC = 2027, DLG = 2029, 
        ITP = 2030, BTT = 2031, UTT = 2032, DDS = 2033, BTS = 2034, UTS = 2035, LTR = 2036, GFF = 2037, 
        FAC = 2038, BTE = 2039, UTE = 2040, BTD = 2041, UTD = 2042, BTP = 2043, UTP = 2044, DFT = 2045, 
        GIC = 2046, GUI = 2047, CSS = 2048, CCS = 2049, BTM = 2050, UTM = 2051, DWK = 2052, PWK = 2053, 
        BTG = 2054, UTG = 2055, JRL = 2056, SAV = 2057, UTW = 2058, FOURPC = 2059, SSF = 2060, HAK = 2061, 
        NWM = 2062, BIK = 2063, NDB = 2064, PTM = 2065, PTT = 2066, BAK = 2067, DAT = 2068, SHD = 2069, 
        XBC = 2070, WBM = 2071, MTR = 2072, KTX = 2073, TTF = 2074, SQL = 2075, TML = 2076, SQ3 = 2077, 
        LOD = 2078, GIF = 2079, PNG = 2080, JPG = 2081, CAF = 2082, JUI = 2083,
        IDS = 9996, ERF = 9997, BIF = 9998, KEY = 9999,
        INVALID_RESOURCE_TYPE = 0xFFFF
    }
}
