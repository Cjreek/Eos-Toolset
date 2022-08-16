﻿using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Eos.ViewModels
{
    internal class FeatViewModel : DataDetailViewModel<Feat>
    {
        public FeatViewModel() : base()
        {
        }

        public FeatViewModel(Feat feat) : base(feat)
        {
        }

        protected override Brush GetEntityColor()
        {
            return new SolidColorBrush(Color.FromArgb(100, 91, 114, 147));
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}