using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum AICategory
    {
        [DisplayName("Harmful AoE (discriminant)")]
        HarmfulAoeDiscriminant = 1,
        [DisplayName("Harmful Ranged")]
        HarmfulRanged = 2,
        [DisplayName("Harmful Touch")]
        HarmfulTouch = 3,
        [DisplayName("Friendly AoE Healing")]
        FriendlyHealingAoe = 4,
        [DisplayName("Friendly Touch Healing")]
        FriendlyHealingTouch = 5,
        [DisplayName("Friendly Conditional AoE")]
        FriendlyConditionalAoe = 6,
        [DisplayName("Friendly Conditional Single")]
        FriendlyConditionalSingle = 7,
        [DisplayName("Friendly Aoe Enhancement")]
        FriendlyEnhancementAoe = 8,
        [DisplayName("Friendly Single Enhancement")]
        FriendlyEnhancementSingle = 9,
        [DisplayName("Friendly Self Enhancement")]
        FriendlyEnhancementSelf = 10,
        [DisplayName("Harmful AoE (indiscriminant)")]
        HarmfulAoeIndiscriminant = 11,

        [DisplayName("Protection (Self)")]
        TalentFriendlyProtectionSelf = 12,
        [DisplayName("Protection (Single)")]
        TalentFriendlyProtectionSingle = 13,
        [DisplayName("Protection (AoE)")]
        TalentFriendlyProtectionAoe = 14,
        [DisplayName("Summon")]
        TalentFriendlySummon = 15,
        [DisplayName("Persistent AoE")]
        TalentPersistentAoe = 16,
        [DisplayName("Healing Potion")]
        TalentHealingPotion = 17,
        [DisplayName("Conditional Potion")]
        TalentConditionalPotion = 18,
        [DisplayName("Dragon Breath")]
        TalentDragonBreath = 19,
        [DisplayName("Protection (Potion)")]
        TalentProtectionPotion = 20,
        [DisplayName("Enhancement (Potion)")]
        TalentEnhancementPotion = 21,
        [DisplayName("Harmful Melee")]
        TalentHarmfulMelee = 22,
        [DisplayName("Dispel")]
        TalentDispel = 23
    }
}
