﻿using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class RaceViewModel : DataDetailViewModel<Race>
    {
        public RaceViewModel() : base()
        {
        }

        public RaceViewModel(Race race) : base(race)
        {
        }

        protected override string GetHeader()
        {
            return Data.Name;
        }
    }
}
