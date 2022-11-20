using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class SoundsetRepository : ModelRepository<Soundset>
    {
        static SoundsetRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<Soundset>(typeof(SoundsetRepository));
        }

        public SoundsetRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return 5100; // Nja
        }
    }
}
