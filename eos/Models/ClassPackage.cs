using Eos.Models.Tables;
using Eos.Nwn.Tlk;
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
    public class ClassPackage : BaseModel
    {
        private CharacterClass? _forClass;
        private Domain? _domain1;
        private Domain? _domain2;
        private Companion? _associateCompanion;
        private Familiar? _associateFamiliar;
        private PackageSpellPreferencesTable? _spellPreferences;
        private PackageFeatPreferencesTable? _featPreferences;
        private PackageSkillPreferencesTable? _skillPreferences;
        private PackageEquipmentTable? _startingEquipment;

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public CharacterClass? ForClass
        {
            get { return _forClass; }
            set 
            { 
                Set(ref _forClass, value);
                NotifyPropertyChanged();
            }
        }
        public AbilityType? PreferredAbility { get; set; }
        public int Gold { get; set; }
        public SpellSchool? SpellSchool { get; set; }
        public Domain? Domain1
        {
            get { return _domain1; }
            set { Set(ref _domain1, value); }
        }
        public Domain? Domain2
        {
            get { return _domain2; }
            set { Set(ref _domain2, value); }
        }
        public Companion? AssociateCompanion
        {
            get { return _associateCompanion; }
            set { Set(ref _associateCompanion, value); }
        }
        public Familiar? AssociateFamiliar
        {
            get { return _associateFamiliar; }
            set { Set(ref _associateFamiliar, value); }
        }

        public PackageSpellPreferencesTable? SpellPreferences
        {
            get { return _spellPreferences; }
            set { Set(ref _spellPreferences, value); }
        }

        public PackageFeatPreferencesTable? FeatPreferences
        {
            get { return _featPreferences; }
            set { Set(ref _featPreferences, value); }
        }

        public PackageSkillPreferencesTable? SkillPreferences
        {
            get { return _skillPreferences; }
            set { Set(ref _skillPreferences, value); }
        }

        public PackageEquipmentTable? StartingEquipment
        {
            get { return _startingEquipment; }
            set { Set(ref _startingEquipment, value); }
        }

        public bool Playable { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override string GetTypeName()
        {
            return "Package";
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (ClassPackage?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Package";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Package";
        }

        public override String GetLabel()
        {
            return Name;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            ForClass = Resolve(ForClass, MasterRepository.Classes);
            Domain1 = Resolve(Domain1, MasterRepository.Domains);
            Domain2 = Resolve(Domain2, MasterRepository.Domains);
            AssociateCompanion = Resolve(AssociateCompanion, MasterRepository.Companions);
            AssociateFamiliar = Resolve(AssociateFamiliar, MasterRepository.Familiars);
            SpellPreferences = Resolve(SpellPreferences, MasterRepository.SpellPreferencesTables);
            FeatPreferences = Resolve(FeatPreferences, MasterRepository.FeatPreferencesTables);
            SkillPreferences = Resolve(SkillPreferences, MasterRepository.SkillPreferencesTables);
            StartingEquipment = Resolve(StartingEquipment, MasterRepository.PackageEquipmentTables);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.ForClass = CreateRefFromJson<CharacterClass>(json["ForClass"]?.AsObject());
            this.PreferredAbility = JsonToEnum<AbilityType>(json["PreferredAbility"]);
            this.Gold = json["Gold"]?.GetValue<int?>() ?? 0;
            this.SpellSchool = JsonToEnum<SpellSchool>(json["SpellSchool"]);
            this.Domain1 = CreateRefFromJson<Domain>(json["Domain1"]?.AsObject());
            this.Domain2 = CreateRefFromJson<Domain>(json["Domain2"]?.AsObject());
            this.AssociateCompanion = CreateRefFromJson<Companion>(json["AssociateCompanion"]?.AsObject());
            this.AssociateFamiliar = CreateRefFromJson<Familiar>(json["AssociateFamiliar"]?.AsObject());
            this.SpellPreferences = CreateRefFromJson<PackageSpellPreferencesTable>(json["SpellPreferences"]?.AsObject());
            this.FeatPreferences = CreateRefFromJson<PackageFeatPreferencesTable>(json["FeatPreferences"]?.AsObject());
            this.SkillPreferences = CreateRefFromJson<PackageSkillPreferencesTable>(json["SkillPreferences"]?.AsObject());
            this.StartingEquipment = CreateRefFromJson<PackageEquipmentTable>(json["StartingEquipment"]?.AsObject());
            this.Playable = json["Playable"]?.GetValue<bool>() ?? true;
        }

        public override JsonObject ToJson()
        {
            var packageJson = base.ToJson();
            packageJson.Add("Name", this.Name.ToJson());
            packageJson.Add("Description", this.Description.ToJson());
            packageJson.Add("ForClass", CreateJsonRef(this.ForClass));
            packageJson.Add("PreferredAbility", EnumToJson(this.PreferredAbility));
            packageJson.Add("Gold", this.Gold);
            packageJson.Add("SpellSchool", EnumToJson(this.SpellSchool));
            packageJson.Add("Domain1", CreateJsonRef(this.Domain1));
            packageJson.Add("Domain2", CreateJsonRef(this.Domain2));
            packageJson.Add("AssociateCompanion", CreateJsonRef(this.AssociateCompanion));
            packageJson.Add("AssociateFamiliar", CreateJsonRef(this.AssociateFamiliar));
            packageJson.Add("SpellPreferences", CreateJsonRef(this.SpellPreferences));
            packageJson.Add("FeatPreferences", CreateJsonRef(this.FeatPreferences));
            packageJson.Add("SkillPreferences", CreateJsonRef(this.SkillPreferences));
            packageJson.Add("StartingEquipment", CreateJsonRef(this.StartingEquipment));
            packageJson.Add("Playable", this.Playable);

            return packageJson;
        }
    }
}
