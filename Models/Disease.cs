using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models
{
    internal class Disease : BaseModel
    {
        public string Name { get; set; } = "";
        public int FirstSaveDC { get; set; } = 15;
        public int SecondSaveDC { get; set; } = 10;
        public int IncubationHours { get; set; } = 1;
        public int AbilityDamage1DiceCount { get; set; } = 1;
        public int AbilityDamage1Dice { get; set; } = 4;
        public AbilityType? AbilityDamage1Type { get; set; } = AbilityType.CON;
        public int AbilityDamage2DiceCount { get; set; } = 0;
        public int AbilityDamage2Dice { get; set; } = 0;
        public AbilityType? AbilityDamage2Type { get; set; } = null;
        public int AbilityDamage3DiceCount { get; set; } = 0;
        public int AbilityDamage3Dice { get; set; } = 0;
        public AbilityType? AbilityDamage3Type { get; set; } = null;
        public IntPtr IncubationEndScript { get; set; }
        public IntPtr DailyEffectScript { get; set; }
    }
}
