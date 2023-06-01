using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class TrapRepository : ModelRepository<Trap>
    {
        static TrapRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Trap>(typeof(TrapRepository));
        }

        public TrapRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.Traps.ExportOffset;
        }
    }
}
