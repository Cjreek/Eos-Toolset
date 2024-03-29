﻿using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class FeatsTableItem : TableItem
    {
        private Feat? _feat;
        private int _grantedOnLevel;

        public Feat? Feat
        {
            get { return _feat; }
            set { Set(ref _feat, value); }
        }

        public FeatListType FeatList { get; set; } = FeatListType.GeneralFeat;
        public int GrantedOnLevel
        {
            get { return _grantedOnLevel; }
            set 
            {
                if (_grantedOnLevel != value)
                {
                    _grantedOnLevel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public FeatMenu Menu { get; set; } = FeatMenu.NoMenuEntry;

        public FeatsTableItem() : base()
        {
        }

        public FeatsTableItem(FeatsTable parentTable) : base(parentTable)
        {
        }

        public override void ResolveReferences()
        {
            Feat = MasterRepository.Feats.Resolve(Feat);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Feat = CreateRefFromJson<Feat>(json["Feat"]?.AsObject());
            this.FeatList = JsonToEnum<FeatListType>(json["FeatList"]) ?? FeatListType.GeneralFeat;
            this.GrantedOnLevel = json["GrantedOnLevel"]?.GetValue<int>() ?? -1;
            this.Menu = JsonToEnum<FeatMenu>(json["Menu"]) ?? FeatMenu.NoMenuEntry;
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("Feat", this.Feat?.ToJsonRef());
            json.Add("FeatList", EnumToJson(this.FeatList));
            json.Add("GrantedOnLevel", this.GrantedOnLevel);
            json.Add("Menu", EnumToJson(this.Menu));

            return json;
        }

        public override bool IsValid()
        {
            return (Feat != null);
        }
    }
}
