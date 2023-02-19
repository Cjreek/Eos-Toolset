using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class ClassRepository : ModelRepository<CharacterClass>
    {
        static ClassRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<CharacterClass>(typeof(ClassRepository));
        }

        public ClassRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.Classes.ExportOffset;
        }
    }
}
