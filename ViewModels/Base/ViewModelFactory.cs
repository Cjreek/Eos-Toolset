using Eos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal static class ViewModelFactory
    {
        public static DataDetailViewModelBase CreateViewModel(object model)
        {
            if (model == null) throw new ArgumentNullException("model");

            if (model is Race)
                return new RaceViewModel((Race)model);
            if (model is Skill)
                return new SkillViewModel((Skill)model);
            else
                throw new ArgumentException("No viewmodel found", "model");
        }
    }
}
