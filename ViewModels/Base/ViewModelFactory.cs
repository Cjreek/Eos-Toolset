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

            if (model is Race race)
                return new RaceViewModel(race);
            if (model is CharacterClass cls)
                return new ClassViewModel(cls);
            if (model is Domain domain)
                return new DomainViewModel(domain);
            if (model is Spell spell)
                return new SpellViewModel(spell);
            if (model is Feat feat)
                return new FeatViewModel(feat);
            if (model is Skill skill)
                return new SkillViewModel(skill);
            if (model is Disease disease)
                return new DiseaseViewModel(disease);
            if (model is Poison poison)
                return new PoisonViewModel(poison);
            else
                throw new ArgumentException("No viewmodel found", "model");
        }
    }
}
