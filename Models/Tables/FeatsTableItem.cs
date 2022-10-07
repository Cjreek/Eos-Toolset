using Eos.Repositories;
using Eos.Types;
using Eos.ViewModels.Base;
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
            set
            {
                if (_feat != value)
                {
                    _feat = value;
                    NotifyPropertyChanged();
                }
            }
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

        public override void ResolveReferences()
        {
            Feat = BaseModel.Resolve(Feat, MasterRepository.Feats);
        }

        public override void FromJson(JsonObject json)
        {
            this.Feat = CreateRefFromJson<Feat>(json["Feat"]?.AsObject());
            this.FeatList = JsonToEnum<FeatListType>(json["FeatList"]) ?? FeatListType.GeneralFeat;
            this.GrantedOnLevel = json["GrantedOnLevel"]?.GetValue<int>() ?? -1;
            this.Menu = JsonToEnum<FeatMenu>(json["Menu"]) ?? FeatMenu.NoMenuEntry;
        }

        public override JsonObject ToJson()
        {
            var json = new JsonObject();
            json.Add("Feat", this.Feat?.ToJsonRef());
            json.Add("FeatList", EnumToJson(this.FeatList));
            json.Add("GrantedOnLevel", this.GrantedOnLevel);
            json.Add("Menu", EnumToJson(this.Menu));

            return json;
        }
    }
}
