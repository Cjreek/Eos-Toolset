using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    [Flags]
    public enum Alignment
    {
        LawfulGood = 0x01, NeutralGood = 0x02, ChaoticGood = 0x04,
        LawfulNeutral = 0x08, Neutral = 0x10, ChaoticNeutral = 0x20,
        LawfulEvil = 0x40, NeutralEvil = 0x80, ChaoticEvil = 0x100
    }

    public static class Alignments
    {
        public const Alignment Good = Alignment.LawfulGood | Alignment.NeutralGood | Alignment.ChaoticGood;
        public const Alignment Neutral = Alignment.LawfulNeutral | Alignment.Neutral | Alignment.ChaoticNeutral;
        public const Alignment Evil = Alignment.LawfulEvil | Alignment.NeutralEvil | Alignment.ChaoticEvil;

        public const Alignment NeutralLC = Alignment.NeutralGood | Alignment.Neutral | Alignment.NeutralEvil;
        public const Alignment NeutralGE = Alignment.LawfulNeutral | Alignment.Neutral | Alignment.ChaoticNeutral;

        public const Alignment Lawful = Alignment.LawfulGood | Alignment.LawfulNeutral | Alignment.LawfulEvil;
        public const Alignment Chaotic = Alignment.ChaoticGood | Alignment.ChaoticNeutral | Alignment.ChaoticEvil;

        public const Alignment All = Alignments.Good | Alignments.Neutral | Alignments.Evil;

        private const int ALIGN_NEUTRAL = 0x01;
        private const int ALIGN_LAWFUL = 0x02;
        private const int ALIGN_CHAOTIC = 0x04;
        private const int ALIGN_GOOD = 0x08;
        private const int ALIGN_EVIL = 0x10;

        private const int AXIS_NONE = 0x00;
        private const int AXIS_LC = 0x01;
        private const int AXIS_GE = 0x02;
        private const int AXIS_BOTH = 0x03;

        public static Alignment Create(int flags, int axis, bool invert)
        {
            Alignment result = (Alignment)0;
            if ((flags & ALIGN_NEUTRAL) != 0)
            {
                switch (axis)
                {
                    case AXIS_NONE:
                        result |= Alignment.Neutral;
                        break;
                    case AXIS_LC:
                        result |= Alignments.NeutralLC;
                        break;
                    case AXIS_GE:
                        result |= Alignments.NeutralGE;
                        break;
                    case AXIS_BOTH:
                        result |= Alignments.NeutralGE | Alignments.NeutralLC;
                        break;
                }
            }

            if ((flags & ALIGN_LAWFUL) != 0)
                result |= Alignments.Lawful;

            if ((flags & ALIGN_CHAOTIC) != 0)
                result |= Alignments.Chaotic;

            if ((flags & ALIGN_GOOD) != 0)
                result |= Alignments.Good;

            if ((flags & ALIGN_EVIL) != 0)
                result |= Alignments.Evil;

            if (invert)
                result = Alignments.All & ~result;

            return result;
        }
    }
}
