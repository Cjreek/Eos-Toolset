﻿using Avalonia.Media;
using Eos.Models;
using Eos.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    public class AmmunitionViewModel : DataDetailViewModel<Ammunition>
    {
        public AmmunitionViewModel() : base()
        {
        }

        public AmmunitionViewModel(Ammunition ammunition) : base(ammunition)
        {
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return base.GetEntityColor();
        }

        protected override string GetHeader()
        {
            return Data.Label;
        }
    }
}
