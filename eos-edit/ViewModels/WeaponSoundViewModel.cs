using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class WeaponSoundViewModel : DataDetailViewModel<WeaponSound>
    {
        public WeaponSoundViewModel() : base()
        {
        }

        public WeaponSoundViewModel(WeaponSound weaponSound) : base(weaponSound)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
