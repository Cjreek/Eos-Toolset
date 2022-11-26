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
    public class Spell : BaseModel
    {
        private SpellSchool _spellSchool = SpellSchool.G;
        private SpellType _spellType = SpellType.Spell;
        private Spell? _subSpell1;
        private Spell? _subSpell2;
        private Spell? _subSpell3;
        private Spell? _subSpell4;
        private Spell? _subSpell5;
        private Spell? _subSpell6;
        private Spell? _subSpell7;
        private Spell? _subSpell8;
        private Spell? _parentSpell;
        private Spell? _counterSpell1;
        private Spell? _counterSpell2;

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public TLKStringSet AlternativeCastMessage { get; set; } = new TLKStringSet();
        public SpellSchool School
        {
            get { return _spellSchool; }
            set
            {
                if (_spellSchool != value)
                {
                    _spellSchool = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public SpellRange Range { get; set; } = SpellRange.P;
        public SpellComponent Components { get; set; } = SpellComponent.V | SpellComponent.S;
        public MetaMagicType AvailableMetaMagic { get; set; } = (MetaMagicType)0;
        public SpellTarget TargetTypes { get; set; } = SpellTarget.Self;
        public String? ImpactScript { get; set; }
        public int InnateLevel { get; set; }
        public int ConjurationTime { get; set; } = 1500;
        public SpellConjureAnimation? ConjuringAnimation { get; set; } = SpellConjureAnimation.Hand;
        public String? ConjurationHeadEffect { get; set; }
        public String? ConjurationHandEffect { get; set; }
        public String? ConjurationGroundEffect { get; set; }
        public String? ConjurationSound { get; set; }
        public String? ConjurationMaleSound { get; set; }
        public String? ConjurationFemaleSound { get; set; }
        public SpellCastAnimation? CastingAnimation { get; set; } = SpellCastAnimation.Out;
        public int CastTime { get; set; } = 1000;
        public String? CastingHeadEffect { get; set; }
        public String? CastingHandEffect { get; set; }
        public String? CastingGroundEffect { get; set; }
        public String? CastingSound { get; set; }
        public bool HasProjectile { get; set; } = false;
        public bool HasProjectileVisuals { get; set; } = false;
        public String? ProjectileModel { get; set; }
        public ProjectileType? ProjectileType { get; set; } = Types.ProjectileType.Homing;
        public ProjectileSource? ProjectileSpawnPoint { get; set; } = ProjectileSource.Hand;
        public String? ProjectileSound { get; set; }
        public ProjectileOrientation? ProjectileOrientation { get; set; } = Types.ProjectileOrientation.Path;
        public Spell? SubSpell1
        {
            get { return _subSpell1; }
            set { Set(ref _subSpell1, value); }
        }
        public Spell? SubSpell2
        {
            get { return _subSpell2; }
            set { Set(ref _subSpell2, value); }
        }
        public Spell? SubSpell3
        {
            get { return _subSpell3; }
            set { Set(ref _subSpell3, value); }
        }
        public Spell? SubSpell4
        {
            get { return _subSpell4; }
            set { Set(ref _subSpell4, value); }
        }
        public Spell? SubSpell5
        {
            get { return _subSpell5; }
            set { Set(ref _subSpell5, value); }
        }
        public Spell? SubSpell6
        {
            get { return _subSpell6; }
            set { Set(ref _subSpell6, value); }
        }
        public Spell? SubSpell7
        {
            get { return _subSpell7; }
            set { Set(ref _subSpell7, value); }
        }
        public Spell? SubSpell8
        {
            get { return _subSpell8; }
            set { Set(ref _subSpell8, value); }
        }

        public AICategory? Category { get; set; }
        public Spell? ParentSpell
        {
            get { return _parentSpell; }
            set { Set(ref _parentSpell, value); }
        }
        public SpellType Type
        {
            get { return _spellType; }
            set
            {
                if (_spellType != value)
                {
                    _spellType = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool UseConcentration { get; set; } = true;
        public bool IsCastSpontaneously { get; set; } = false;
        public bool IsHostile { get; set; }
        public Spell? CounterSpell1
        {
            get { return _counterSpell1; }
            set { Set(ref _counterSpell1, value); }
        }
        public Spell? CounterSpell2
        {
            get { return _counterSpell2; }
            set { Set(ref _counterSpell2, value); }
        }

        public TargetShape? TargetShape { get; set; }
        public double? TargetSizeX { get; set; }
        public double? TargetSizeY { get; set; }
        public TargetFlag? TargetingFlags { get; set; }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (Spell?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        protected override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Spell";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Spell";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            SubSpell1 = Resolve(SubSpell1, MasterRepository.Spells);
            SubSpell2 = Resolve(SubSpell2, MasterRepository.Spells);
            SubSpell3 = Resolve(SubSpell3, MasterRepository.Spells);
            SubSpell4 = Resolve(SubSpell4, MasterRepository.Spells);
            SubSpell5 = Resolve(SubSpell5, MasterRepository.Spells);
            SubSpell6 = Resolve(SubSpell6, MasterRepository.Spells);
            SubSpell7 = Resolve(SubSpell7, MasterRepository.Spells);
            SubSpell8 = Resolve(SubSpell8, MasterRepository.Spells);
            ParentSpell = Resolve(ParentSpell, MasterRepository.Spells);
            CounterSpell1 = Resolve(CounterSpell1, MasterRepository.Spells);
            CounterSpell2 = Resolve(CounterSpell2, MasterRepository.Spells);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.AlternativeCastMessage.FromJson(json["AlternativeCastMessage"]?.AsObject());
            this.Icon = json["Icon"]?.GetValue<String>();
            this.School = JsonToEnum<SpellSchool>(json["School"]) ?? SpellSchool.G;
            this.Range = JsonToEnum<SpellRange>(json["Range"]) ?? SpellRange.P;
            this.Components = JsonToEnum<SpellComponent>(json["Components"]) ?? (SpellComponent)0;
            this.AvailableMetaMagic = JsonToEnum<MetaMagicType>(json["AvailableMetaMagic"]) ?? (MetaMagicType)0;
            this.TargetTypes = JsonToEnum<SpellTarget>(json["TargetTypes"]) ?? (SpellTarget)0;
            this.ImpactScript = json["ImpactScript"]?.GetValue<String>();
            this.InnateLevel = json["InnateLevel"]?.GetValue<int>() ?? 0;
            this.ConjurationTime = json["ConjurationTime"]?.GetValue<int>() ?? 1500;
            this.ConjuringAnimation = JsonToEnum<SpellConjureAnimation>(json["ConjuringAnimation"]) ?? SpellConjureAnimation.Hand;
            this.ConjurationHeadEffect = json["ConjurationHeadEffect"]?.GetValue<String>();
            this.ConjurationHandEffect = json["ConjurationHandEffect"]?.GetValue<String>();
            this.ConjurationGroundEffect = json["ConjurationGroundEffect"]?.GetValue<String>();
            this.ConjurationSound = json["ConjurationSound"]?.GetValue<String>();
            this.ConjurationMaleSound = json["ConjurationMaleSound"]?.GetValue<String>();
            this.ConjurationFemaleSound = json["ConjurationFemaleSound"]?.GetValue<String>();
            this.CastingAnimation = JsonToEnum<SpellCastAnimation>(json["CastingAnimation"]) ?? SpellCastAnimation.Out;
            this.CastTime = json["CastTime"]?.GetValue<int>() ?? 1000;
            this.CastingHeadEffect = json["CastingHeadEffect"]?.GetValue<String>();
            this.CastingHandEffect = json["CastingHandEffect"]?.GetValue<String>();
            this.CastingGroundEffect = json["CastingGroundEffect"]?.GetValue<String>();
            this.CastingSound = json["CastingSound"]?.GetValue<String>();
            this.HasProjectile = json["HasProjectile"]?.GetValue<bool>() ?? false;
            this.ProjectileModel = json["ProjectileModel"]?.GetValue<String>();
            this.ProjectileType = JsonToEnum<ProjectileType>(json["ProjectileType"]);
            this.ProjectileSpawnPoint = JsonToEnum<ProjectileSource>(json["ProjectileSpawnPoint"]);
            this.ProjectileSound = json["ProjectileSound"]?.GetValue<String>();
            this.ProjectileOrientation = JsonToEnum<ProjectileOrientation>(json["ProjectileOrientation"]);
            this.SubSpell1 = CreateRefFromJson<Spell>(json["SubSpell1"]?.AsObject());
            this.SubSpell2 = CreateRefFromJson<Spell>(json["SubSpell2"]?.AsObject());
            this.SubSpell3 = CreateRefFromJson<Spell>(json["SubSpell3"]?.AsObject());
            this.SubSpell4 = CreateRefFromJson<Spell>(json["SubSpell4"]?.AsObject());
            this.SubSpell5 = CreateRefFromJson<Spell>(json["SubSpell5"]?.AsObject());
            this.SubSpell6 = CreateRefFromJson<Spell>(json["SubSpell6"]?.AsObject());
            this.SubSpell7 = CreateRefFromJson<Spell>(json["SubSpell7"]?.AsObject());
            this.SubSpell8 = CreateRefFromJson<Spell>(json["SubSpell8"]?.AsObject());
            this.Category = JsonToEnum<AICategory>(json["Category"]);
            this.ParentSpell = CreateRefFromJson<Spell>(json["ParentSpell"]?.AsObject());
            this.Type = JsonToEnum<SpellType>(json["Type"]) ?? SpellType.Spell;
            this.UseConcentration = json["UseConcentration"]?.GetValue<bool>() ?? true;
            this.IsCastSpontaneously = json["IsCastSpontaneously"]?.GetValue<bool>() ?? false;
            this.IsHostile = json["IsHostile"]?.GetValue<bool>() ?? true;
            this.CounterSpell1 = CreateRefFromJson<Spell>(json["CounterSpell1"]?.AsObject());
            this.CounterSpell2 = CreateRefFromJson<Spell>(json["CounterSpell2"]?.AsObject());
            this.HasProjectileVisuals = json["HasProjectileVisuals"]?.GetValue<bool>() ?? false;
            this.TargetShape = JsonToEnum<TargetShape>(json["TargetShape"]);
            this.TargetSizeX = json["TargetSizeX"]?.GetValue<double>();
            this.TargetSizeY = json["TargetSizeY"]?.GetValue<double>();
            this.TargetingFlags = JsonToEnum<TargetFlag>(json["TargetingFlags"]);
        }

        public override JsonObject ToJson()
        {
            var spellJson = base.ToJson();
            spellJson.Add("Name", this.Name.ToJson());
            spellJson.Add("Description", this.Description.ToJson());
            spellJson.Add("AlternativeCastMessage", this.AlternativeCastMessage.ToJson());
            spellJson.Add("Icon", this.Icon);
            spellJson.Add("School", EnumToJson(this.School));
            spellJson.Add("Range", EnumToJson(this.Range));
            spellJson.Add("Components", EnumToJson(this.Components));
            spellJson.Add("AvailableMetaMagic", EnumToJson(this.AvailableMetaMagic));
            spellJson.Add("TargetTypes", EnumToJson(this.TargetTypes));
            spellJson.Add("ImpactScript", this.ImpactScript);
            spellJson.Add("InnateLevel", this.InnateLevel);
            spellJson.Add("ConjurationTime", this.ConjurationTime);
            spellJson.Add("ConjuringAnimation", EnumToJson(this.ConjuringAnimation));
            spellJson.Add("ConjurationHeadEffect", this.ConjurationHeadEffect);
            spellJson.Add("ConjurationHandEffect", this.ConjurationHandEffect);
            spellJson.Add("ConjurationGroundEffect", this.ConjurationGroundEffect);
            spellJson.Add("ConjurationSound", this.ConjurationSound);
            spellJson.Add("ConjurationMaleSound", this.ConjurationMaleSound);
            spellJson.Add("ConjurationFemaleSound", this.ConjurationFemaleSound);
            spellJson.Add("CastingAnimation", EnumToJson(this.CastingAnimation));
            spellJson.Add("CastTime", this.CastTime);
            spellJson.Add("CastingHeadEffect", this.CastingHeadEffect);
            spellJson.Add("CastingHandEffect", this.CastingHandEffect);
            spellJson.Add("CastingGroundEffect", this.CastingGroundEffect);
            spellJson.Add("CastingSound", this.CastingSound);
            spellJson.Add("HasProjectile", this.HasProjectile);
            spellJson.Add("ProjectileModel", this.ProjectileModel);
            spellJson.Add("ProjectileType", EnumToJson(this.ProjectileType));
            spellJson.Add("ProjectileSpawnPoint", EnumToJson(this.ProjectileSpawnPoint));
            spellJson.Add("ProjectileSound", this.ProjectileSound);
            spellJson.Add("ProjectileOrientation", EnumToJson(this.ProjectileOrientation));
            spellJson.Add("SubSpell1", CreateJsonRef(this.SubSpell1));
            spellJson.Add("SubSpell2", CreateJsonRef(this.SubSpell2));
            spellJson.Add("SubSpell3", CreateJsonRef(this.SubSpell3));
            spellJson.Add("SubSpell4", CreateJsonRef(this.SubSpell4));
            spellJson.Add("SubSpell5", CreateJsonRef(this.SubSpell5));
            spellJson.Add("SubSpell6", CreateJsonRef(this.SubSpell6));
            spellJson.Add("SubSpell7", CreateJsonRef(this.SubSpell7));
            spellJson.Add("SubSpell8", CreateJsonRef(this.SubSpell8));
            spellJson.Add("Category", EnumToJson(this.Category));
            spellJson.Add("ParentSpell", CreateJsonRef(this.ParentSpell));
            spellJson.Add("Type", EnumToJson(this.Type));
            spellJson.Add("UseConcentration", this.UseConcentration);
            spellJson.Add("IsCastSpontaneously", this.IsCastSpontaneously);
            spellJson.Add("IsHostile", this.IsHostile);
            spellJson.Add("CounterSpell1", CreateJsonRef(this.CounterSpell1));
            spellJson.Add("CounterSpell2", CreateJsonRef(this.CounterSpell2));
            spellJson.Add("HasProjectileVisuals", this.HasProjectileVisuals);
            spellJson.Add("TargetShape", EnumToJson(this.TargetShape));
            spellJson.Add("TargetSizeX", this.TargetSizeX);
            spellJson.Add("TargetSizeY", this.TargetSizeY);
            spellJson.Add("TargetingFlags", EnumToJson(this.TargetingFlags));

            return spellJson;
        }
    }
}
