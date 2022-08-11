﻿using Prism.Commands;
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
using System.Windows;
using Eos.Models.Base;
using Eos.Types;

namespace Eos.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private ObservableCollection<DataDetailViewModelBase> detailViewList = new ObservableCollection<DataDetailViewModelBase>();
        private Dictionary<object, DataDetailViewModelBase> detailViewDict = new Dictionary<object, DataDetailViewModelBase>();

        private DataDetailViewModelBase? currentView;
        private TLKLanguage currentLanguage;
        private bool currentGender;

        public ObservableCollection<DataDetailViewModelBase> DetailViewList { get { return detailViewList; } }

        public TLKLanguage CurrentLanguage
        {
            get { return currentLanguage; }
            set
            {
                if (currentLanguage != value)
                {
                    currentLanguage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CurrentGender
        {
            get { return currentGender; }
            set
            {
                if (currentGender != value)
                {
                    currentGender = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        private void GenerateDebugData()
        {
            Race testRace = new Race();
            testRace.Name.SetDefault("Orc");
            MasterRepository.Standard.Races.Add(testRace);

            testRace = new Race();
            testRace.Name.SetDefault("Elf");
            MasterRepository.Standard.Races.Add(testRace);

            Skill testSkill = new Skill();
            testSkill.Name.SetDefault("Lumbering");
            MasterRepository.Custom.Skills.Add(testSkill);

            testSkill = new Skill();
            testSkill.Name.SetDefault("Harvesting");
            MasterRepository.Standard.Skills.Add(testSkill);

            CharacterClass testClass = new CharacterClass();
            testClass.Name.SetDefault("Fighter");
            MasterRepository.Standard.Classes.Add(testClass);

            Domain testDomain = new Domain();
            testDomain.Name.SetDefault("Death & Decay");
            MasterRepository.Standard.Domains.Add(testDomain);

            Spell testSpell = new Spell();
            testSpell.Name.SetDefault("Iceball");
            MasterRepository.Standard.Spells.Add(testSpell);

            Feat testFeat = new Feat();
            testFeat.Name.SetDefault("Tooth Punch");
            MasterRepository.Standard.Feats.Add(testFeat);

            Disease testDisease = new Disease();
            testDisease.Name.SetDefault("Gehung");
            MasterRepository.Standard.Diseases.Add(testDisease);

            Poison testPoison = new Poison();
            testPoison.Name.SetDefault("Poison of Death");
            MasterRepository.Standard.Poisons.Add(testPoison);
        }

        public MainViewModel()
        {
            GenerateDebugData();

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
    }
}
