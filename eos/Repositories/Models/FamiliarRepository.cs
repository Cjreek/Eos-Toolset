using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class FamiliarRepository : ModelRepository<Familiar>
    {
        static FamiliarRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Familiar>(typeof(FamiliarRepository));
        }

        public FamiliarRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.Familiars.ExportOffset;
        }
    }
}
