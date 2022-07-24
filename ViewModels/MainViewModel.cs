using Prism.Commands;
using Eos.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels
{
    internal class MainViewModel
    {
        private ObservableCollection<DataDetailViewModelBase> detailViewList = new ObservableCollection<DataDetailViewModelBase>();
        private Dictionary<object, DataDetailViewModelBase> detailViewDict = new Dictionary<object, DataDetailViewModelBase>();

        private ObservableCollection<Race> raceList = new ObservableCollection<Race>();

        public MainViewModel()
        {
            Race testRace = new Race();
            testRace.Name = "Orc";
            raceList.Add(testRace);

            // Factory die anhand von Model -> ViewModel gibt
            OpenRaceCommand = new DelegateCommand<Race>(race =>
            {
                if (!detailViewDict.ContainsKey(race))
                {
                    var raceDetail = new RaceViewModel(race);
                    detailViewList.Add(raceDetail);
                    detailViewDict.Add(race, raceDetail);
                }
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
        public DelegateCommand<Race> OpenRaceCommand { get; set; }
        public DelegateCommand<DataDetailViewModelBase> CloseDetailCommand { get; set; }

        public string Name { get; set; } = "Hallo Welt";
    }
}
