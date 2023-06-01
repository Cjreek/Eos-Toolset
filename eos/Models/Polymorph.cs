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
    public class Polymorph : BaseModel
    {
        private String _name = "";
        private Appearance? _appearance;
        private Race? _racialType;
        private Portrait? _portrait;
        private Spell? _spell1;
        private Spell? _spell2;
        private Spell? _spell3;

        public String Name
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

        public Appearance? Appearance
        {
            get { return _appearance; }
            set { Set(ref _appearance, value); }
        }
        public Race? RacialType
        {
            get { return _racialType; }
            set { Set(ref _racialType, value); }
        }
        public Portrait? Portrait
        {
            get { return _portrait; }
            set { Set(ref _portrait, value); }
        }
        public String? PortraitResRef { get; set; }
        public String? CreatureWeapon1 { get; set; }
        public String? CreatureWeapon2 { get; set; }
        public String? CreatureWeapon3 { get; set; }
        public String? HideItem { get; set; }
        public String? MainHandItem { get; set; }
        public int? Strength { get; set; }
        public int? Constitution { get; set; }
        public int? Dexterity { get; set; }
        public int? NaturalACBonus { get; set; }
        public int? HPBonus { get; set; }
        //public Soundset? Soundset { get; set; } // Unused
        public Spell? Spell1
        {
            get { return _spell1; }
            set { Set(ref _spell1, value); }
        }
        public Spell? Spell2
        {
            get { return _spell2; }
            set { Set(ref _spell2, value); }
        }
        public Spell? Spell3
        {
            get { return _spell3; }
            set { Set(ref _spell3, value); }
        }
        public bool MergeWeapon { get; set; }
        public bool MergeAccessories { get; set; }
        public bool MergeArmor { get; set; }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name = "New Polymorph";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            Appearance = Resolve(Appearance, MasterRepository.Appearances);
            Portrait = Resolve(Portrait, MasterRepository.Portraits);
            RacialType = Resolve(RacialType, MasterRepository.Races);
            Spell1 = Resolve(Spell1, MasterRepository.Spells);
            Spell2 = Resolve(Spell2, MasterRepository.Spells);
            Spell3 = Resolve(Spell3, MasterRepository.Spells);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<String>() ?? "";
            this.Appearance = CreateRefFromJson<Appearance>(json["Appearance"]?.AsObject());
            this.RacialType = CreateRefFromJson<Race>(json["RacialType"]?.AsObject());
            this.Portrait = CreateRefFromJson<Portrait>(json["Portrait"]?.AsObject());
            this.PortraitResRef = json["PortraitResRef"]?.GetValue<String>() ?? "";
            this.CreatureWeapon1 = json["CreatureWeapon1"]?.GetValue<String>() ?? "";
            this.CreatureWeapon2 = json["CreatureWeapon2"]?.GetValue<String>() ?? "";
            this.CreatureWeapon3 = json["CreatureWeapon3"]?.GetValue<String>() ?? "";
            this.HideItem = json["HideItem"]?.GetValue<String>() ?? "";
            this.MainHandItem = json["MainHandItem"]?.GetValue<String>() ?? "";
            this.Strength = json["Strength"]?.GetValue<int>();
            this.Constitution = json["Constitution"]?.GetValue<int>();
            this.Dexterity = json["Dexterity"]?.GetValue<int>();
            this.NaturalACBonus = json["NaturalACBonus"]?.GetValue<int>();
            this.HPBonus = json["HPBonus"]?.GetValue<int>();
            this.Spell1 = CreateRefFromJson<Spell>(json["Spell1"]?.AsObject());
            this.Spell2 = CreateRefFromJson<Spell>(json["Spell2"]?.AsObject());
            this.Spell3 = CreateRefFromJson<Spell>(json["Spell3"]?.AsObject());
            this.MergeWeapon = json["MergeWeapon"]?.GetValue<bool>() ?? false;
            this.MergeAccessories = json["MergeAccessories"]?.GetValue<bool>() ?? false;
            this.MergeArmor = json["MergeArmor"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var polymorphJson = base.ToJson();
            polymorphJson.Add("Name", this.Name);
            polymorphJson.Add("Appearance", CreateJsonRef(this.Appearance));
            polymorphJson.Add("RacialType", CreateJsonRef(this.RacialType));
            polymorphJson.Add("Portrait", CreateJsonRef(this.Portrait));
            polymorphJson.Add("PortraitResRef", this.PortraitResRef);
            polymorphJson.Add("CreatureWeapon1", this.CreatureWeapon1);
            polymorphJson.Add("CreatureWeapon2", this.CreatureWeapon2);
            polymorphJson.Add("CreatureWeapon3", this.CreatureWeapon3);
            polymorphJson.Add("HideItem", this.HideItem);
            polymorphJson.Add("MainHandItem", this.MainHandItem);
            polymorphJson.Add("Strength", this.Strength);
            polymorphJson.Add("Constitution", this.Constitution);
            polymorphJson.Add("Dexterity", this.Dexterity);
            polymorphJson.Add("NaturalACBonus", this.NaturalACBonus);
            polymorphJson.Add("HPBonus", this.HPBonus);
            polymorphJson.Add("Spell1", CreateJsonRef(this.Spell1));
            polymorphJson.Add("Spell2", CreateJsonRef(this.Spell2));
            polymorphJson.Add("Spell3", CreateJsonRef(this.Spell3));
            polymorphJson.Add("MergeWeapon", this.MergeWeapon);
            polymorphJson.Add("MergeAccessories", this.MergeAccessories);
            polymorphJson.Add("MergeArmor", this.MergeArmor);

            return polymorphJson;
        }
    }
}
