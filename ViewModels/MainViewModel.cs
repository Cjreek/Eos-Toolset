using Prism.Commands;
using Eos.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eos.ViewModels.Base;

namespace Eos.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private DataDetailViewModelBase? currentView;

        private ObservableCollection<DataDetailViewModelBase> detailViewList = new ObservableCollection<DataDetailViewModelBase>();
        private Dictionary<object, DataDetailViewModelBase> detailViewDict = new Dictionary<object, DataDetailViewModelBase>();

        private ModelRepository<Race> raceRepository = new ModelRepository<Race>();
        private ModelRepository<CharacterClass> classRepository = new ModelRepository<CharacterClass>();
        private ModelRepository<Domain> domainRepository = new ModelRepository<Domain>();
        private ModelRepository<Spell> spellRepository = new ModelRepository<Spell>();
        private ModelRepository<Feat> featRepository = new ModelRepository<Feat>();
        private ModelRepository<Skill> skillRepository = new ModelRepository<Skill>();

        public MainViewModel()
        {
            Race testRace = new Race();
            testRace.Name = "Orc";
            raceRepository.Add(testRace);

            testRace = new Race();
            testRace.Name = "Elf";
            raceRepository.Add(testRace);

            Skill testSkill = new Skill();
            testSkill.Name = "Lumbering";
            skillRepository.Add(testSkill);

            testSkill = new Skill();
            testSkill.Name = "Harvesting";
            skillRepository.Add(testSkill);

            CharacterClass testClass = new CharacterClass();
            testClass.Name = "Fighter";
            classRepository.Add(testClass);

            Domain testDomain = new Domain();
            testDomain.Name = "Death & Decay";
            domainRepository.Add(testDomain);

            Spell testSpell = new Spell();
            testSpell.Name = "Iceball";
            spellRepository.Add(testSpell);

            Feat testFeat = new Feat();
            testFeat.Name = "Tooth Punch";
            featRepository.Add(testFeat);

            OpenDetailCommand = new DelegateCommand<object>(detailModel =>
            {
                if (!detailViewDict.ContainsKey(detailModel))
                {
                    var detailView = ViewModelFactory.CreateViewModel(detailModel);
                    detailViewList.Add(detailView);
                    detailViewDict.Add(detailModel, detailView);
                }

                CurrentView = detailViewDict[detailModel];
            });

            CloseDetailCommand = new DelegateCommand<DataDetailViewModelBase>(vm =>
            {
                if (detailViewDict.ContainsKey(vm.GetDataObject()))
                {
                    detailViewList.Remove(vm);
                    detailViewDict.Remove(vm.GetDataObject());
                }
            });
        }

        public ObservableCollection<DataDetailViewModelBase> DetailViewList { get { return detailViewList; } }

        // Model Repositories
        public ModelRepository<Race> RaceRepository { get { return raceRepository; } }
        public ModelRepository<CharacterClass> ClassRepository { get { return classRepository; } }
        public ModelRepository<Domain> DomainRepository { get { return domainRepository; } }
        public ModelRepository<Spell> SpellRepository { get { return spellRepository; } }
        public ModelRepository<Feat> FeatRepository { get { return featRepository; } }
        public ModelRepository<Skill> SkillRepository { get { return skillRepository; } }

        // Commands
        public DelegateCommand<object> OpenDetailCommand { get; set; }
        public DelegateCommand<DataDetailViewModelBase> CloseDetailCommand { get; set; }

        public DataDetailViewModelBase? CurrentView
        {
            get { return currentView; }
            set
            {
                if (currentView != value)
                {
                    currentView = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
