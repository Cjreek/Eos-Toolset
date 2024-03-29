﻿using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class FeatsTable : BaseTable<FeatsTableItem>
    {
        protected override string GetTypeName()
        {
            return "Feats Table";
        }

        protected override void SetDefaultValues()
        {
            Name = "CLS_FEAT_NEW";
        }

        protected override int GetMaximumItems()
        {
            return UInt16.MaxValue;
        }

        public IEnumerable<FeatsTableItem?> GrantedFeats => Items.Where(item => item?.FeatList == Types.FeatListType.AutomaticallyGranted && item?.GrantedOnLevel < 99)
                                                                 .OrderBy(item => item?.GrantedOnLevel)
                                                                 .ThenBy(item => item?.Feat?.Name[MasterRepository.Project.DefaultLanguage].Text);
        public IEnumerable<FeatsTableItem?> GeneralFeats => Items.Where(item => item?.FeatList == Types.FeatListType.GeneralFeat && item?.GrantedOnLevel < 99)
                                                                 .OrderBy(item => item?.GrantedOnLevel)
                                                                 .ThenBy(item => item?.Feat?.Name[MasterRepository.Project.DefaultLanguage].Text);
        public IEnumerable<FeatsTableItem?> GeneralOrBonusFeats => Items.Where(item => item?.FeatList == Types.FeatListType.GeneralFeatOrBonusFeat && item?.GrantedOnLevel < 99)
                                                                        .OrderBy(item => item?.GrantedOnLevel)
                                                                        .ThenBy(item => item?.Feat?.Name[MasterRepository.Project.DefaultLanguage].Text);
        public IEnumerable<FeatsTableItem?> BonusFeats => Items.Where(item => item?.FeatList == Types.FeatListType.BonusFeat && item?.GrantedOnLevel < 99)
                                                               .OrderBy(item => item?.GrantedOnLevel)
                                                               .ThenBy(item => item?.Feat?.Name[MasterRepository.Project.DefaultLanguage].Text);

        public IEnumerable<FeatsTableItem?> GainableFeats => Items.Where(item => item?.GrantedOnLevel == 99)
                                                                  .OrderBy(item => item?.Feat?.Name[MasterRepository.Project.DefaultLanguage].Text);

        protected override void Changed()
        {
            NotifyPropertyChanged(nameof(GrantedFeats));
            NotifyPropertyChanged(nameof(GeneralFeats));
            NotifyPropertyChanged(nameof(GeneralOrBonusFeats));
            NotifyPropertyChanged(nameof(BonusFeats));
            NotifyPropertyChanged(nameof(GainableFeats));
        }
    }
}
