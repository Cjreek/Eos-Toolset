using Eos.Models;
using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    public class ClassPackageSearchViewModel : ModelSearchViewModel<ClassPackage>
    {
        public ClassPackageSearchViewModel(VirtualModelRepository<ClassPackage> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(ClassPackage? model)
        {
            return model?.Name;
        }

        protected override string GetWindowTitle()
        {
            return "Search Class Package";
        }
    }
}
