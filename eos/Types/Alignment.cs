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

    public struct AlignmentTuple
    {
        public int Flags;
        public int Axis;
        public bool Inverted;

        public AlignmentTuple(int flags, int axis, bool inverted)
        {
            this.Flags = flags;
            this.Axis = axis;
            this.Inverted = inverted;
        }
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

        private const int AXIS_LC = 0x01;
        private const int AXIS_GE = 0x02;

        private static Dictionary<Alignment, AlignmentTuple> alignmentLookup = new Dictionary<Alignment, AlignmentTuple>();

        static Alignments()
        {
            alignmentLookup.Clear();
            for (int flag=0; flag <= 0x1F; flag++)
            {
                for (int axis=0x00; axis <= 0x03; axis++)
                {
                    Alignment alignNormal = Create(flag, axis, false);
                    Alignment alignInverted = Create(flag, axis, true);
                    
                    // Add if alignment doesn't exist or overwrite if existing alignment tuple is inverted (Prioritize non-inverted)
                    if (alignmentLookup.TryGetValue(alignNormal, out AlignmentTuple existingAlign))
                    {
                        if (existingAlign.Inverted) 
                            alignmentLookup[alignNormal] = new AlignmentTuple(flag, axis, false);
                    }
                    else
                        alignmentLookup[alignNormal] = new AlignmentTuple(flag, axis, false);
                    
                    // Only add inverted alignment if combination doesn't exist yet
                    if (!alignmentLookup.ContainsKey(alignInverted))
                        alignmentLookup[alignInverted] = new AlignmentTuple(flag, axis, true);
                }
            }
        }

        public static bool IsValid(Alignment alignment)
        {
            return alignmentLookup.ContainsKey(alignment);
        }

        public static AlignmentTuple? Get2daAlignment(Alignment alignment)
        {
            if (alignmentLookup.ContainsKey(alignment))
                return alignmentLookup[alignment];
            return null;
        }

        public static Alignment Create(int flags, int axis, bool invert)
        {
            bool done;
            int nAlignRestrict = flags;
            int nAlignRestrictType = axis;

            Alignment result = (Alignment)0;
            foreach (var alignment in Enum.GetValues<Alignment>())
            {
                done = false;
                if ((nAlignRestrictType & AXIS_LC) != 0)
                {
                    if ((Alignments.Lawful.HasFlag(alignment) && ((nAlignRestrict & ALIGN_LAWFUL) != 0)) ||
                        (Alignments.NeutralLC.HasFlag(alignment) && ((nAlignRestrict & ALIGN_NEUTRAL) != 0)) ||
                        (Alignments.Chaotic.HasFlag(alignment) && ((nAlignRestrict & ALIGN_CHAOTIC) != 0)))
                    {
                        if (invert)
                            result |= alignment;
                        done = true;
                    }
                }

                if (((nAlignRestrictType & AXIS_GE) != 0) && (!done))
                {
                    if ((Alignments.Good.HasFlag(alignment) && ((nAlignRestrict & ALIGN_GOOD) != 0)) ||
                        (Alignments.NeutralGE.HasFlag(alignment) && ((nAlignRestrict & ALIGN_NEUTRAL) != 0)) ||
                        (Alignments.Evil.HasFlag(alignment) && ((nAlignRestrict & ALIGN_EVIL) != 0)))
                    {
                        if (invert)
                            result |= alignment;
                        done = true;
                    }
                }

                if (!done && !invert)
                    result |= alignment;
            }

            return result;
        }
    }
}
