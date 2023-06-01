using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class AppearanceSoundset : BaseModel
    {
        private string _name = "";
        private WeaponSound? _leftAttack;
        private WeaponSound? _rightAttack;
        private WeaponSound? _straightAttack;
        private WeaponSound? _lowCloseAttack;
        private WeaponSound? _highCloseAttack;
        private WeaponSound? _reachAttack;
        private WeaponSound? _miss;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected override void SetDefaultValues()
        {
            Name = "New Appearance Soundset";
        }

        public ArmorType? ArmorType { get; set; }
        public WeaponSound? LeftAttack
        {
            get { return _leftAttack; }
            set { Set(ref _leftAttack, value); }
        }
        public WeaponSound? RightAttack
        {
            get { return _rightAttack; }
            set { Set(ref _rightAttack, value); }
        }
        public WeaponSound? StraightAttack
        {
            get { return _straightAttack; }
            set { Set(ref _straightAttack, value); }
        }
        public WeaponSound? LowCloseAttack
        {
            get { return _lowCloseAttack; }
            set { Set(ref _lowCloseAttack, value); }
        }
        public WeaponSound? HighCloseAttack
        {
            get { return _highCloseAttack; }
            set { Set(ref _highCloseAttack, value); }
        }
        public WeaponSound? ReachAttack
        {
            get { return _reachAttack; }
            set { Set(ref _reachAttack, value); }
        }
        public WeaponSound? Miss
        {
            get { return _miss; }
            set { Set(ref _miss, value); }
        }
        public string? Looping { get; set; }
        public string? FallForward { get; set; }
        public string? FallBackward { get; set; }

        public override String GetLabel()
        {
            return Name;
        }

        protected override string GetTypeName()
        {
            return "Appearance Soundset";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            LeftAttack = Resolve(LeftAttack, MasterRepository.WeaponSounds);
            RightAttack = Resolve(RightAttack, MasterRepository.WeaponSounds);
            StraightAttack = Resolve(StraightAttack, MasterRepository.WeaponSounds);
            LowCloseAttack = Resolve(LowCloseAttack, MasterRepository.WeaponSounds);
            HighCloseAttack = Resolve(HighCloseAttack, MasterRepository.WeaponSounds);
            ReachAttack = Resolve(ReachAttack, MasterRepository.WeaponSounds);
            Miss = Resolve(Miss, MasterRepository.WeaponSounds);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<string>() ?? "";
            this.ArmorType = JsonToEnum<ArmorType>(json["ArmorType"]);
            this.LeftAttack = CreateRefFromJson<WeaponSound>(json["LeftAttack"]?.AsObject());
            this.RightAttack = CreateRefFromJson<WeaponSound>(json["RightAttack"]?.AsObject());
            this.StraightAttack = CreateRefFromJson<WeaponSound>(json["StraightAttack"]?.AsObject());
            this.LowCloseAttack = CreateRefFromJson<WeaponSound>(json["LowCloseAttack"]?.AsObject());
            this.HighCloseAttack = CreateRefFromJson<WeaponSound>(json["HighCloseAttack"]?.AsObject());
            this.ReachAttack = CreateRefFromJson<WeaponSound>(json["ReachAttack"]?.AsObject());
            this.Miss = CreateRefFromJson<WeaponSound>(json["Miss"]?.AsObject());
            this.Looping = json["Looping"]?.GetValue<string>();
            this.FallForward = json["FallForward"]?.GetValue<string>();
            this.FallBackward = json["FallBackward"]?.GetValue<string>();
        }

        public override JsonObject ToJson()
        {
            var appearanceSoundsetJson = base.ToJson();
            appearanceSoundsetJson.Add("Name", this.Name);
            appearanceSoundsetJson.Add("ArmorType", EnumToJson(this.ArmorType));
            appearanceSoundsetJson.Add("LeftAttack", CreateJsonRef(this.LeftAttack));
            appearanceSoundsetJson.Add("RightAttack", CreateJsonRef(this.RightAttack));
            appearanceSoundsetJson.Add("StraightAttack", CreateJsonRef(this.StraightAttack));
            appearanceSoundsetJson.Add("LowCloseAttack", CreateJsonRef(this.LowCloseAttack));
            appearanceSoundsetJson.Add("HighCloseAttack", CreateJsonRef(this.HighCloseAttack));
            appearanceSoundsetJson.Add("ReachAttack", CreateJsonRef(this.ReachAttack));
            appearanceSoundsetJson.Add("Miss", CreateJsonRef(this.Miss));
            appearanceSoundsetJson.Add("Looping", this.Looping);
            appearanceSoundsetJson.Add("FallForward", this.FallForward);
            appearanceSoundsetJson.Add("FallBackward", this.FallBackward);

            return appearanceSoundsetJson;
        }
    }
}
