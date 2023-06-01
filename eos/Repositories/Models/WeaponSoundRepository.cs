using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class WeaponSoundRepository : ModelRepository<WeaponSound>
    {
        static WeaponSoundRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<WeaponSound>(typeof(WeaponSoundRepository));
        }

        public WeaponSoundRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.WeaponSounds.ExportOffset;
        }
    }
}
