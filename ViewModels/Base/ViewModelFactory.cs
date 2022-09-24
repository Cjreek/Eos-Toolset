using Eos.Models;
using Eos.Models.Tables;
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
            if (model is Spellbook spellbook)
                return new SpellbookViewModel(spellbook);

            if (model is ClassPackage classPackage)
                return new ClassPackageViewModel(classPackage);
            if (model is Soundset soundset)
                return new SoundsetViewModel(soundset);

            if (model is FeatsTable featsTable)
                return new FeatsTableViewModel(featsTable);
            if (model is AttackBonusTable abTable)
                return new AttackBonusTableViewModel(abTable);
            if (model is BonusFeatsTable bonusFeatsTable)
                return new BonusFeatTableViewModel(bonusFeatsTable);
            if (model is SkillsTable skillTable)
                return new SkillTableViewModel(skillTable);
            if (model is SavingThrowTable savesTable)
                return new SavingThrowTableViewModel(savesTable);
            if (model is PrerequisiteTable requTable)
                return new PrerequisiteTableViewModel(requTable);
            if (model is StatGainTable statGainTable)
                return new StatGainTableViewModel(statGainTable);
            if (model is RacialFeatsTable racialFeatsTable)
                return new RacialFeatsTableViewModel(racialFeatsTable);

            else
                throw new ArgumentException("No viewmodel found", "model");
        }
    }
}
