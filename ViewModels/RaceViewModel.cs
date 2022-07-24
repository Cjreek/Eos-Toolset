using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class RaceViewModel : DataDetailViewModel<Race>
    {
        public Race RaceModel { get; set; }

        public RaceViewModel()
        {
            RaceModel = new Race();
        }

        public RaceViewModel(Race race)
        {
            this.RaceModel = race;
        }

        protected override string GetHeader()
        {
            return RaceModel.Name ?? "No Name";
        }

        protected override Race GetData()
        {
            return RaceModel;
        }
    }
}
