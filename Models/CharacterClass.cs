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
    public class CharacterClass : BaseModel
    {
        private bool _isSpellcaster = false;
        private AttackBonusTable? _attackBonusTable;
        private FeatsTable? _feats;
        private SavingThrowTable? _savingThrows;
        private SkillsTable? _skills;
        private BonusFeatsTable? _bonusFeats;
        private SpellSlotTable? _spellSlots;
        private KnownSpellsTable? _knownSpells;
        private PrerequisiteTable? _requirements;
        private ClassPackage? _defaultPackage;
        private StatGainTable? _statGainTable;
        private Spellbook? _spellbook;

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet NameLower { get; set; } = new TLKStringSet();
        public TLKStringSet NamePlural { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public int HitDie { get; set; } = 8;
        public int SkillPointsPerLevel { get; set; } = 4;

        public AttackBonusTable? AttackBonusTable
        {
            get { return _attackBonusTable; }
            set { Set(ref _attackBonusTable, value); }
        }

        public FeatsTable? Feats
        {
            get { return _feats; }
            set { Set(ref _feats, value); }
        }

        public SavingThrowTable? SavingThrows
        {
            get { return _savingThrows; }
            set { Set(ref _savingThrows, value); }
        }

        public SkillsTable? Skills
        {
            get { return _skills; }
            set { Set(ref _skills, value); }
        }

        public BonusFeatsTable? BonusFeats
        {
            get { return _bonusFeats; }
            set { Set(ref _bonusFeats, value); }
        }

        public SpellSlotTable? SpellSlots
        {
            get { return _spellSlots; }
            set { Set(ref _spellSlots, value); }
        }

        public KnownSpellsTable? KnownSpells
        {
            get { return _knownSpells; }
            set { Set(ref _knownSpells, value); }
        }

        public bool Playable { get; set; } = true;

        public bool IsSpellCaster
        {
            get { return _isSpellcaster; }
            set
            {
                if (_isSpellcaster != value)
                {
                    _isSpellcaster = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int RecommendedStr { get; set; } = 10;
        public int RecommendedDex { get; set; } = 10;
        public int RecommendedCon { get; set; } = 10;
        public int RecommendedWis { get; set; } = 10;
        public int RecommendedInt { get; set; } = 10;
        public int RecommendedCha { get; set; } = 10;
        public AbilityType PrimaryAbility { get; set; } = AbilityType.STR;
        public Alignment AllowedAlignments { get; set; } = Alignments.All;
        public PrerequisiteTable? Requirements
        {
            get { return _requirements; }
            set { Set(ref _requirements, value); }
        }
        public int MaxLevel { get; set; } = 0;
        public bool MulticlassXPPenalty { get; set; } = true;
        public int ArcaneCasterLevelMod { get; set; } = 0;
        public int DivineCasterLevelMod { get; set; } = 0;
        public int PreEpicMaxLevel { get; set; } = -1;
        public ClassPackage? DefaultPackage
        {
            get { return _defaultPackage; }
            set { Set(ref _defaultPackage, value); }
        }
        public StatGainTable? StatGainTable
        {
            get { return _statGainTable; }
            set { Set(ref _statGainTable, value); }
        }
        public bool MemorizesSpells { get; set; } = true;
        public bool SpellbookRestricted { get; set; } = true;
        public bool PicksDomain { get; set; } = false;
        public bool PicksSchool { get; set; } = false;
        public bool CanLearnFromScrolls { get; set; } = false;
        public bool IsArcaneCaster { get; set; } = true;
        public bool HasSpellFailure { get; set; } = true;
        public AbilityType SpellcastingAbility { get; set; } = AbilityType.INT;
        public Spellbook? Spellbook
        {
            get { return _spellbook; }
            set { Set(ref _spellbook, value); }
        }
        public double CasterLevelMultiplier { get; set; } = 1.0;
        public int MinCastingLevel { get; set; }
        public int MinAssociateLevel { get; set; }
        public bool CanCastSpontaneously { get; set; } = false;
        public bool SkipSpellSelection { get; set; } = false;

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (CharacterClass?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        protected override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Class";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Class";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            AttackBonusTable = Resolve(AttackBonusTable, MasterRepository.AttackBonusTables);
            Feats = Resolve(Feats, MasterRepository.FeatTables);
            SavingThrows = Resolve(SavingThrows, MasterRepository.SavingThrowTables);
            Skills = Resolve(Skills, MasterRepository.SkillTables);
            BonusFeats = Resolve(BonusFeats, MasterRepository.BonusFeatTables);
            SpellSlots = Resolve(SpellSlots, MasterRepository.SpellSlotTables);
            KnownSpells = Resolve(KnownSpells, MasterRepository.KnownSpellsTables);
            Requirements = Resolve(Requirements, MasterRepository.PrerequisiteTables);
            StatGainTable = Resolve(StatGainTable, MasterRepository.StatGainTables);
            DefaultPackage = Resolve(DefaultPackage, MasterRepository.ClassPackages);
            Spellbook = Resolve(Spellbook, MasterRepository.Spellbooks);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.NamePlural.FromJson(json["NamePlural"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.Icon = json["Icon"]?.GetValue<String>();
            this.HitDie = json["HitDie"]?.GetValue<int>() ?? 1;
            this.SkillPointsPerLevel = json["SkillPointsPerLevel"]?.GetValue<int>() ?? 4;
            this.AttackBonusTable = CreateRefFromJson<AttackBonusTable>(json["AttackBonusTable"]?.AsObject());
            this.Feats = CreateRefFromJson<FeatsTable>(json["Feats"]?.AsObject());
            this.SavingThrows = CreateRefFromJson<SavingThrowTable>(json["SavingThrows"]?.AsObject());
            this.Skills = CreateRefFromJson<SkillsTable>(json["Skills"]?.AsObject());
            this.BonusFeats = CreateRefFromJson<BonusFeatsTable>(json["BonusFeats"]?.AsObject());
            this.SpellSlots = CreateRefFromJson<SpellSlotTable>(json["SpellSlots"]?.AsObject());
            this.KnownSpells = CreateRefFromJson<KnownSpellsTable>(json["KnownSpells"]?.AsObject());
            this.Playable = json["Playable"]?.GetValue<bool>() ?? false;
            this.IsSpellCaster = json["IsSpellCaster"]?.GetValue<bool>() ?? false;
            this.RecommendedStr = json["RecommendedStr"]?.GetValue<int>() ?? 10;
            this.RecommendedDex = json["RecommendedDex"]?.GetValue<int>() ?? 10;
            this.RecommendedCon = json["RecommendedCon"]?.GetValue<int>() ?? 10;
            this.RecommendedWis = json["RecommendedWis"]?.GetValue<int>() ?? 10;
            this.RecommendedInt = json["RecommendedInt"]?.GetValue<int>() ?? 10;
            this.RecommendedCha = json["RecommendedCha"]?.GetValue<int>() ?? 10;
            this.PrimaryAbility = JsonToEnum<AbilityType>(json["PrimaryAbility"]) ?? AbilityType.STR;
            this.AllowedAlignments = JsonToEnum<Alignment>(json["AllowedAlignments"]) ?? (Alignment)0;
            this.Requirements = CreateRefFromJson<PrerequisiteTable>(json["Requirements"]?.AsObject());
            this.MaxLevel = json["MaxLevel"]?.GetValue<int>() ?? 0;
            this.MulticlassXPPenalty = json["MulticlassXPPenalty"]?.GetValue<bool>() ?? false;
            this.ArcaneCasterLevelMod = json["ArcaneCasterLevelMod"]?.GetValue<int>() ?? 0;
            this.DivineCasterLevelMod = json["DivineCasterLevelMod"]?.GetValue<int>() ?? 0;
            this.PreEpicMaxLevel = json["PreEpicMaxLevel"]?.GetValue<int>() ?? -1;
            this.DefaultPackage = CreateRefFromJson<ClassPackage>(json["DefaultPackage"]?.AsObject());
            this.StatGainTable = CreateRefFromJson<StatGainTable>(json["StatGainTable"]?.AsObject());
            this.MemorizesSpells = json["MemorizesSpells"]?.GetValue<bool>() ?? false;
            this.SpellbookRestricted = json["SpellbookRestricted"]?.GetValue<bool>() ?? false;
            this.PicksDomain = json["PicksDomain"]?.GetValue<bool>() ?? false;
            this.PicksSchool = json["PicksSchool"]?.GetValue<bool>() ?? false;
            this.CanLearnFromScrolls = json["CanLearnFromScrolls"]?.GetValue<bool>() ?? false;
            this.IsArcaneCaster = json["IsArcaneCaster"]?.GetValue<bool>() ?? false;
            this.HasSpellFailure = json["HasSpellFailure"]?.GetValue<bool>() ?? false;
            this.SpellcastingAbility = JsonToEnum<AbilityType>(json["SpellcastingAbility"]) ?? AbilityType.INT;
            this.Spellbook = CreateRefFromJson<Spellbook>(json["Spellbook"]?.AsObject());
            this.CasterLevelMultiplier = json["CasterLevelMultiplier"]?.GetValue<double>() ?? 1.0;
            this.MinCastingLevel = json["MinCastingLevel"]?.GetValue<int>() ?? 1;
            this.MinAssociateLevel = json["MinAssociateLevel"]?.GetValue<int>() ?? 255;
            this.CanCastSpontaneously = json["CanCastSpontaneously"]?.GetValue<bool>() ?? false;
            this.SkipSpellSelection = json["SkipSpellSelection"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var classJson = base.ToJson();
            classJson.Add("Name", this.Name.ToJson());
            classJson.Add("NamePlural", this.NamePlural.ToJson());
            classJson.Add("Description", this.Description.ToJson());
            classJson.Add("Icon", this.Icon);
            classJson.Add("HitDie", this.HitDie);
            classJson.Add("SkillPointsPerLevel", this.SkillPointsPerLevel);
            classJson.Add("AttackBonusTable", CreateJsonRef(this.AttackBonusTable));
            classJson.Add("Feats", CreateJsonRef(this.Feats));
            classJson.Add("SavingThrows", CreateJsonRef(this.SavingThrows));
            classJson.Add("Skills", CreateJsonRef(this.Skills));
            classJson.Add("BonusFeats", CreateJsonRef(this.BonusFeats));
            classJson.Add("SpellSlots", CreateJsonRef(this.SpellSlots));
            classJson.Add("KnownSpells", CreateJsonRef(this.KnownSpells));
            classJson.Add("Playable", this.Playable);
            classJson.Add("IsSpellCaster", this.IsSpellCaster);
            classJson.Add("RecommendedStr", this.RecommendedStr);
            classJson.Add("RecommendedDex", this.RecommendedDex);
            classJson.Add("RecommendedCon", this.RecommendedCon);
            classJson.Add("RecommendedWis", this.RecommendedWis);
            classJson.Add("RecommendedInt", this.RecommendedInt);
            classJson.Add("RecommendedCha", this.RecommendedCha);
            classJson.Add("PrimaryAbility", EnumToJson(this.PrimaryAbility));
            classJson.Add("AllowedAlignments", EnumToJson(this.AllowedAlignments));
            classJson.Add("Requirements", CreateJsonRef(this.Requirements));
            classJson.Add("MaxLevel", this.MaxLevel);
            classJson.Add("MulticlassXPPenalty", this.MulticlassXPPenalty);
            classJson.Add("ArcaneCasterLevelMod", this.ArcaneCasterLevelMod);
            classJson.Add("DivineCasterLevelMod", this.DivineCasterLevelMod);
            classJson.Add("PreEpicMaxLevel", this.PreEpicMaxLevel);
            classJson.Add("DefaultPackage", CreateJsonRef(this.DefaultPackage));
            classJson.Add("StatGainTable", CreateJsonRef(this.StatGainTable));
            classJson.Add("MemorizesSpells", this.MemorizesSpells);
            classJson.Add("SpellbookRestricted", this.SpellbookRestricted);
            classJson.Add("PicksDomain", this.PicksDomain);
            classJson.Add("PicksSchool", this.PicksSchool);
            classJson.Add("CanLearnFromScrolls", this.CanLearnFromScrolls);
            classJson.Add("IsArcaneCaster", this.IsArcaneCaster);
            classJson.Add("HasSpellFailure", this.HasSpellFailure);
            classJson.Add("SpellcastingAbility", EnumToJson(this.SpellcastingAbility));
            classJson.Add("Spellbook", CreateJsonRef(this.Spellbook));
            classJson.Add("CasterLevelMultiplier", this.CasterLevelMultiplier);
            classJson.Add("MinCastingLevel", this.MinCastingLevel);
            classJson.Add("MinAssociateLevel", this.MinAssociateLevel);
            classJson.Add("CanCastSpontaneously", this.CanCastSpontaneously);
            classJson.Add("SkipSpellSelection", this.SkipSpellSelection);

            return classJson;
        }
    }
}
