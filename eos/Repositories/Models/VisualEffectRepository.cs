using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class VisualEffectRepository : ModelRepository<VisualEffect>
    {
        static VisualEffectRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<VisualEffect>(typeof(VisualEffectRepository));
        }

        public VisualEffectRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return MasterRepository.Project.Settings.VisualEffects.ExportOffset;
        }
    }
}
