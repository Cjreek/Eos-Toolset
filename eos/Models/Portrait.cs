using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class Portrait : BaseModel
    {
        private Race? _race;
        private string _resRef = "";

        public string ResRef
        {
            get { return _resRef; }
            set
            {
                if (_resRef != value)
                {
                    _resRef = value;
                    Icon =  $"po_{_resRef}s";
                    NotifyPropertyChanged();
                }
            }
        }

        public string? LowGoreResRef { get; set; }
        public PortraitGender Gender { get; set; }
        public Race? Race
        {
            get { return _race; }
            set { Set(ref _race, value); }
        }

        public string Name => ResRef;

        public PlaceableType? PlaceableType { get; set; }
        public bool IsPlayerPortrait { get; set; }

        public override String GetLabel()
        {
            return ResRef;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            Race = Resolve(Race, MasterRepository.Races);
        }

        protected override void SetDefaultValues()
        {
            ResRef = "new_portrait";
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.ResRef = json["ResRef"]?.GetValue<String>() ?? "";
            this.LowGoreResRef = json["LowGoreResRef"]?.GetValue<String>();
            this.Gender = JsonToEnum<PortraitGender>(json["Gender"]) ?? PortraitGender.Male;
            this.PlaceableType = JsonToEnum<PlaceableType>(json["PlaceableType"]);
            this.Race = CreateRefFromJson<Race>(json["Race"]?.AsObject());
            this.IsPlayerPortrait = json["IsPlayerPortrait"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var portraitJson = base.ToJson();
            portraitJson.Add("ResRef", this.ResRef);
            portraitJson.Add("LowGoreResRef", this.LowGoreResRef);
            portraitJson.Add("Gender", EnumToJson(this.Gender));
            portraitJson.Add("PlaceableType", EnumToJson(this.PlaceableType));
            portraitJson.Add("Race", CreateJsonRef(this.Race));
            portraitJson.Add("IsPlayerPortrait", this.IsPlayerPortrait);

            return portraitJson;
        }
    }
}
