using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class ClassPackageViewModel : DataDetailViewModel<ClassPackage>
    {
        public ClassPackageViewModel() : base()
        {
        }

        public ClassPackageViewModel(ClassPackage package) : base(package)
        {

        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
