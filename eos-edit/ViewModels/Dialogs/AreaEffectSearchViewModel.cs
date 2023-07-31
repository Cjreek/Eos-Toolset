﻿using Eos.Models;
using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Dialogs
{
    public class AreaEffectSearchViewModel : ModelSearchViewModel<AreaEffect>
    {
        public AreaEffectSearchViewModel(VirtualModelRepository<AreaEffect> repository) : base(repository)
        {
        }

        protected override TLKStringSet? GetModelText(AreaEffect? model)
        {
            var tlk = new TLKStringSet();
            foreach (var lang in Enum.GetValues<TLKLanguage>())
                tlk[lang].Text = model?.Name ?? "";
            return tlk;
        }

        protected override string GetWindowTitle()
        {
            return "Search Area Effect";
        }
    }
}
