using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum ImmunityType
    {
        //None = 0,
        [DisplayName("Ability Decrease")]
        AbilityDecrease = 19,
        [DisplayName("AC Decrease")]
        ACDecrease = 23,
        [DisplayName("Attack Decrease")]
        AttackDecrease = 20,
        Blindness = 7,
        Charm = 14,
        Confused = 16,
        [DisplayName("Critical Hit")]
        CriticalHit = 31,
        Cursed = 17,
        [DisplayName("Damage Decrease")]
        DamageDecrease = 21,
        [DisplayName("Damage Immunity Decrease")]
        DamageImmunityDecrease = 22,
        Dazed = 18,
        Deafness = 8,
        Death = 32,
        Disease = 3,
        Dominate = 15,
        Entangle = 10,
        Fear = 4,
        Knockdown = 28,
        [DisplayName("Mind Spells")]
        MindSpells = 1,
        [DisplayName("Movement Speed Decrease")]
        MovementSpeedDecrease = 24,
        [DisplayName("Negative Level")]
        NegativeLevel = 29,
        Paralysis = 6,
        Poison = 2,
        [DisplayName("Saving Throw Decrease")]
        SavingThrowDecrease = 25,
        Silence = 11,
        [DisplayName("Skill Decrease")]
        SkillDecrease = 27,
        Sleep = 13,
        Slow = 9,
        [DisplayName("Sneak Attack")]
        SneakAttack = 30,
        [DisplayName("Spell Resistance Decrease")]
        SpellResistanceDecrease = 26,
        Stun = 12,
        Trap = 5,
    }
}
