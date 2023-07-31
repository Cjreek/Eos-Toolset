using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    public class CustomObjectInstanceSearchViewModel : ModelSearchViewModel<CustomObjectInstance>
    {
        private CustomObject template;

        public CustomObjectInstanceSearchViewModel(CustomObject template, ModelRepository<CustomObjectInstance> repository) : base(new VirtualModelRepository<CustomObjectInstance>(repository))
        {
            this.template = template;
            this.IsTableSearch = true;
        }

        protected override TLKStringSet? GetModelText(CustomObjectInstance? model)
        {
            var tlk = new TLKStringSet();
            foreach (var lang in Enum.GetValues<TLKLanguage>())
                tlk[lang].Text = model?.Name ?? "";
            return tlk;
        }

        protected override string GetWindowTitle()
        {
            return "Search " + template.Name;
        }
    }
}
