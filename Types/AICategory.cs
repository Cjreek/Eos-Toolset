using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum AICategory
    {
        HarmfulAoeDiscriminant = 1,
        HarmfulRanged = 2,
        HarmfulTouch = 3,
        FriendlyHealingAoe = 4,
        FriendlyHealingTouch = 5,
        FriendlyConditionalAoe = 6,
        FriendlyConditionalSingle = 7,
        FriendlyEnhancementAoe = 8,
        FriendlyEnhancementSingle = 9,
        FriendlyEnhancementSelf = 10,
        HarmfulAoeIndiscriminant = 11,

        TalentFriendlyProtectionSelf = 12,
        TalentFriendlyProtectionSingle = 13,
        TalentFriendlyProtectionAoe = 14,
        TalentFriendlySummon = 15,
        TalentPersistentAoe = 16,
        TalentHealingPotion = 17,
        TalentConditionalPotion = 18,
        TalentDragonBreath = 19,
        TalentProtectionPotion = 20,
        TalentEnhancementPotion = 21,
        TalentHarmfulMelee = 22,
        TalentDispel = 23
    }
}
