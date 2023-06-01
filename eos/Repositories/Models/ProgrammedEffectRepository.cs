using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class ProgrammedEffectRepository : ModelRepository<ProgrammedEffect>
    {
        static ProgrammedEffectRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<ProgrammedEffect>(typeof(ProgrammedEffectRepository));
        }

        public ProgrammedEffectRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.ProgrammedEffects.ExportOffset;
        }
    }
}
