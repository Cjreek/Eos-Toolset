using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class RaceRepository : ModelRepository<Race>
    {
        static RaceRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Race>(typeof(RaceRepository));
        }

        public RaceRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return 30;
        }
    }
}
