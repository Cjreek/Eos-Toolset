using Eos.Models;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Repositories.Models
{
    public class ClassPackageRepository : ModelRepository<ClassPackage>
    {
        static ClassPackageRepository()
        {
            RepositoryFactory.RegisterRepositoryClass<ClassPackage>(typeof(ClassPackageRepository));
        }

        public ClassPackageRepository(bool isReadonly) : base(isReadonly)
        {
        }

        public override int GetCustomDataStartIndex()
        {
            return 132;
        }
    }
}
