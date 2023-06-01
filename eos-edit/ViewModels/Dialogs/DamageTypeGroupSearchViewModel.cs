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
    internal class DamageTypeGroupSearchViewModel : ModelSearchViewModel<DamageTypeGroup>
    {
        public DamageTypeGroupSearchViewModel(VirtualModelRepository<DamageTypeGroup> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(DamageTypeGroup? model)
        {
            return model?.FeedbackText;
        }

        protected override string GetWindowTitle()
        {
            return "Search Damage Type Group";
        }
    }
}
