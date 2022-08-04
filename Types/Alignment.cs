using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    [Flags]
    internal enum Alignment
    {
        LawfulGood, NeutralGood, ChaoticGood,
        LawfulNeutral, Neutral, ChaoticNeutral,
        LawfulEvil, NeutralEvil, ChaoticEvil
    }

    internal static class Alignments
    {
        public const Alignment Good = Alignment.LawfulGood | Alignment.NeutralGood | Alignment.ChaoticGood;
        public const Alignment Neutral = Alignment.LawfulNeutral | Alignment.Neutral | Alignment.ChaoticNeutral;
        public const Alignment Evil = Alignment.LawfulEvil | Alignment.NeutralEvil | Alignment.ChaoticEvil;

        public const Alignment All = Alignments.Good | Alignments.Neutral | Alignments.Evil;
    }
}
