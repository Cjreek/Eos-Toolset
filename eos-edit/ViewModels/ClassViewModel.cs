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
    public class ClassViewModel : DataDetailViewModel<CharacterClass>
    {
        public ClassViewModel() : base()
        {
        }
        public ClassViewModel(CharacterClass cls) : base(cls)
        {
        }

        protected override ISolidColorBrush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 218, 165, 32));
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
