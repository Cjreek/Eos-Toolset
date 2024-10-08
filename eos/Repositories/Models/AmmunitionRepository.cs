﻿using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class AmmunitionRepository : ModelRepository<Ammunition>
    {
        static AmmunitionRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Ammunition>(typeof(AmmunitionRepository));
        }

        public AmmunitionRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.Ammunitions.ExportOffset;
        }
    }
}
