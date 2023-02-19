using Eos.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public enum SoundsetEntryType
    {
        [DisplayName("Attack")]
        ATTACK = 0,
        [DisplayName("Battlecry 1")]
        BATTLECRY1 = 1,
        [DisplayName("Battlecry 2")]
        BATTLECRY2 = 2,
        [DisplayName("Battlecry 3")]
        BATTLECRY3 = 3,
        [DisplayName("Heal request")]
        HEALME = 4,
        [DisplayName("Help request")]
        HELP = 5,
        [DisplayName("Enemies close")]
        ENEMIES = 6,
        [DisplayName("Call to flee")]
        FLEE = 7,
        [DisplayName("Taunt")]
        TAUNT = 8,
        [DisplayName("Guard request")]
        GUARDME = 9,
        [DisplayName("Hold position")]
        HOLD = 10,
        [DisplayName("Attack sound 1")]
        GATTACK1 = 11,
        [DisplayName("Attack sound 2")]
        GATTACK2 = 12,
        [DisplayName("Attack sound 3")]
        GATTACK3 = 13,
        [DisplayName("Pain 1")]
        PAIN1 = 14,
        [DisplayName("Pain 2")]
        PAIN2 = 15,
        [DisplayName("Pain 3")]
        PAIN3 = 16,
        [DisplayName("Close to death")]
        NEARDEATH = 17,
        [DisplayName("Death")]
        DEATH = 18,
        [DisplayName("Poisoned")]
        POISONED = 19,
        [DisplayName("Spell failed")]
        SPELLFAILED = 20,
        [DisplayName("Weapon ineffective")]
        WEAPONSUCKS = 21,
        [DisplayName("Follow request")]
        FOLLOWME = 22,
        [DisplayName("Found something")]
        LOOKHERE = 23,
        [DisplayName("Group request")]
        GROUP = 24,
        [DisplayName("Move over")]
        MOVEOVER = 25,
        [DisplayName("Lock picking")]
        PICKLOCK = 26,
        [DisplayName("Searching the area")]
        SEARCH = 27,
        [DisplayName("Hide request")]
        HIDE = 28,
        [DisplayName("Affirmation")]
        CANDO = 29,
        [DisplayName("Failure")]
        CANTDO = 30,
        [DisplayName("Task complete")]
        TASKCOMPLETE = 31,
        [DisplayName("Encumbered")]
        ENCUMBERED = 32,
        [DisplayName("Greeting")]
        SELECTED = 33,
        [DisplayName("Conversation start")]
        HELLO = 34,
        [DisplayName("Yes")]
        YES = 35,
        [DisplayName("No")]
        NO = 36,
        [DisplayName("Stop/Hold")]
        STOP = 37,
        [DisplayName("Rest request")]
        REST = 38,
        [DisplayName("Bored")]
        BORED = 39,
        [DisplayName("Goodbye")]
        GOODBYE = 40,
        [DisplayName("Thanks")]
        THANKS = 41,
        [DisplayName("Laugh")]
        LAUGH = 42,
        [DisplayName("Cuss")]
        CUSS = 43,
        [DisplayName("Cheer")]
        CHEER = 44,
        [DisplayName("Conversation request")]
        TALKTOME = 45,
        [DisplayName("Agreement")]
        GOODIDEA = 46,
        [DisplayName("Skeptical")]
        BADIDEA = 47,
        [DisplayName("Threaten")]
        THREATEN = 48,
    }
}
