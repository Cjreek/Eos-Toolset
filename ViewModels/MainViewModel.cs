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

namespace Eos.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private DataDetailViewModelBase? currentView;

        private ObservableCollection<DataDetailViewModelBase> detailViewList = new ObservableCollection<DataDetailViewModelBase>();
        private Dictionary<object, DataDetailViewModelBase> detailViewDict = new Dictionary<object, DataDetailViewModelBase>();

        private ObservableCollection<Race> raceList = new ObservableCollection<Race>();
        private ObservableCollection<Skill> skillList = new ObservableCollection<Skill>();

        public MainViewModel()
        {
            Race testRace = new Race();
            testRace.Name = "Orc";
            raceList.Add(testRace);

            testRace = new Race();
            testRace.Name = "Elf";
            raceList.Add(testRace);

            Skill testSkill = new Skill();
            testSkill.Name = "Lumbering";
            skillList.Add(testSkill);

            testSkill = new Skill();
            testSkill.Name = "Harvesting";
            skillList.Add(testSkill);

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

        public ObservableCollection<Race> RaceList { get { return raceList; } }
        public ObservableCollection<Skill> SkillList { get { return skillList; } }

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

        // Commands
        public DelegateCommand<object> OpenDetailCommand { get; set; }
        public DelegateCommand<DataDetailViewModelBase> CloseDetailCommand { get; set; }
    }
}
