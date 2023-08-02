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
    public class CustomTableInstanceSearchViewModel : ModelSearchViewModel<CustomTableInstance>
    {
        private CustomTable template;

        public CustomTableInstanceSearchViewModel(CustomTable template, ModelRepository<CustomTableInstance> repository) : base(new VirtualModelRepository<CustomTableInstance>(repository))
        {
            this.template = template;
            this.IsTableSearch = true;
        }

        protected override TLKStringSet? GetModelText(CustomTableInstance? model)
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
