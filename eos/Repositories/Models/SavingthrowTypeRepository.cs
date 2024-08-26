using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class SavingthrowTypeRepository : ModelRepository<SavingthrowType>
    {
        static SavingthrowTypeRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<SavingthrowType>(typeof(SavingthrowTypeRepository));
        }

        public SavingthrowTypeRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.SavingthrowTypes.ExportOffset;
        }
    }
}
