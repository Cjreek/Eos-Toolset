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
    public class BaseItem : BaseModel
    {
        private int? _criticalThreatRange;
        private BaseItem? _ammunitionBaseItem;
        private InventorySound? _inventorySound;
        private ItemPropertySet? _itemPropertySet;
        private Feat? _requiredFeat1;
        private Feat? _requiredFeat2;
        private Feat? _requiredFeat3;
        private Feat? _requiredFeat4;
        private Feat? _requiredFeat5;
        private WeaponSound? _weaponSound;
        private Feat? _weaponFocusFeat;
        private Feat? _epicWeaponFocusFeat;
        private Feat? _weaponSpecializationFeat;
        private Feat? _epicWeaponSpecializationFeat;
        private Feat? _improvedCriticalFeat;
        private Feat? _overwhelmingCriticalFeat;
        private Feat? _devastatingCriticalFeat;
        private Feat? _weaponOfChoiceFeat;

        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public TLKStringSet StatsText { get; set; } = new TLKStringSet();
        public int InventorySlotWidth { get; set; }
        public int InventorySlotHeight { get; set; }
        public InventorySlots EquipableSlots { get; set; } = (InventorySlots)0;
        public bool CanRotateIcon { get; set; }
        public ItemModelType ModelType { get; set; } = ItemModelType.Simple;
        public string ItemModel { get; set; } = "";
        public bool GenderSpecific { get; set; }
        public AlphaChannelUsageType? Part1Alpha { get; set; }
        public AlphaChannelUsageType? Part2Alpha { get; set; }
        public AlphaChannelUsageType? Part3Alpha { get; set; }
        public string DefaultModel { get; set; } = "";
        public bool IsContainer { get; set; } = false;
        public WeaponWieldType WeaponWieldType { get; set; }
        public WeaponDamageType? WeaponDamageType { get; set; }
        public WeaponSize? WeaponSize { get; set; }
        public BaseItem? AmmunitionBaseItem
        {
            get { return _ammunitionBaseItem; }
            set { Set(ref _ammunitionBaseItem, value); }
        }
        public double? PreferredAttackDistance { get; set; }
        public int MinimumModelCount { get; set; }
        public int MaximumModelCount { get; set; }
        public int? DamageDiceCount { get; set; }
        public int? DamageDice { get; set; }
        public int? CriticalThreatRange
        {
            get { return _criticalThreatRange; }
            set
            {
                if (_criticalThreatRange != value)
                {
                    _criticalThreatRange = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int? CriticalMultiplier { get; set; }
        public ItemCategory Category { get; set; } = ItemCategory.None;
        public double BaseCost { get; set; }
        public int MaxStackSize { get; set; }
        public double ItemCostMultiplier { get; set; }
        public InventorySound? InventorySound
        {
            get { return _inventorySound; }
            set { Set(ref _inventorySound, value); }
        }
        public int MinSpellProperties { get; set; }
        public int MaxSpellProperties { get; set; }
        public ItemPropertySet? ItemPropertySet
        {
            get { return _itemPropertySet; }
            set { Set(ref _itemPropertySet, value); }
        }
        public StorePanelType? StorePanel { get; set; }
        public Feat? RequiredFeat1
        {
            get { return _requiredFeat1; }
            set { Set(ref _requiredFeat1, value); }
        }
        public Feat? RequiredFeat2
        {
            get { return _requiredFeat2; }
            set { Set(ref _requiredFeat2, value); }
        }
        public Feat? RequiredFeat3
        {
            get { return _requiredFeat3; }
            set { Set(ref _requiredFeat3, value); }
        }
        public Feat? RequiredFeat4
        {
            get { return _requiredFeat4; }
            set { Set(ref _requiredFeat4, value); }
        }
        public Feat? RequiredFeat5
        {
            get { return _requiredFeat5; }
            set { Set(ref _requiredFeat5, value); }
        }
        public ArmorClassType? ArmorClassType { get; set; }
        public int BaseShieldAC { get; set; }
        public int ArmorCheckPenalty { get; set; }
        public int DefaultChargeCount { get; set; }
        public ItemModelRotation GroundModelRotation { get; set; }
        public double Weight { get; set; }
        public WeaponSound? WeaponSound
        {
            get { return _weaponSound; }
            set { Set(ref _weaponSound, value); }
        }
        public AmmunitionType? AmmunitionType { get; set; }
        public QuickbarBehaviour QuickbarBehaviour { get; set; }
        public int? ArcaneSpellFailure { get; set; }
        public int? LeftSlashAnimationPercent { get; set; }
        public int? RightSlashAnimationPercent { get; set; }
        public int? StraightSlashAnimationPercent { get; set; }
        public int? StorePanelOrder { get; set; }
        public int ItemLevelRestrictionStackSize { get; set; }
        public Feat? WeaponFocusFeat
        {
            get { return _weaponFocusFeat; }
            set { Set(ref _weaponFocusFeat, value); }
        }
        public Feat? EpicWeaponFocusFeat
        {
            get { return _epicWeaponFocusFeat; }
            set { Set(ref _epicWeaponFocusFeat, value); }
        }
        public Feat? WeaponSpecializationFeat
        {
            get { return _weaponSpecializationFeat; }
            set { Set(ref _weaponSpecializationFeat, value); }
        }
        public Feat? EpicWeaponSpecializationFeat
        {
            get { return _epicWeaponSpecializationFeat; }
            set { Set(ref _epicWeaponSpecializationFeat, value); }
        }
        public Feat? ImprovedCriticalFeat
        {
            get { return _improvedCriticalFeat; }
            set { Set(ref _improvedCriticalFeat, value); }
        }
        public Feat? OverwhelmingCriticalFeat
        {
            get { return _overwhelmingCriticalFeat; }
            set { Set(ref _overwhelmingCriticalFeat, value); }
        }
        public Feat? DevastatingCriticalFeat
        {
            get { return _devastatingCriticalFeat; }
            set { Set(ref _devastatingCriticalFeat, value); }
        }
        public Feat? WeaponOfChoiceFeat
        {
            get { return _weaponOfChoiceFeat; }
            set { Set(ref _weaponOfChoiceFeat, value); }
        }
        public bool IsMonkWeapon { get; set; }
        public SizeCategory? WeaponFinesseMinimumCreatureSize { get; set; }

        protected override String GetTypeName()
        {
            return "Base Item";
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (CharacterClass?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New base Item";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New base Item";
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            AmmunitionBaseItem = Resolve(AmmunitionBaseItem, MasterRepository.BaseItems);
            InventorySound = Resolve(InventorySound, MasterRepository.InventorySounds);
            ItemPropertySet = Resolve(ItemPropertySet, MasterRepository.ItemPropertySets);
            RequiredFeat1 = Resolve(RequiredFeat1, MasterRepository.Feats);
            RequiredFeat2 = Resolve(RequiredFeat2, MasterRepository.Feats);
            RequiredFeat3 = Resolve(RequiredFeat3, MasterRepository.Feats);
            RequiredFeat4 = Resolve(RequiredFeat4, MasterRepository.Feats);
            RequiredFeat5 = Resolve(RequiredFeat5, MasterRepository.Feats);
            WeaponSound = Resolve(WeaponSound, MasterRepository.WeaponSounds);
            WeaponFocusFeat = Resolve(WeaponFocusFeat, MasterRepository.Feats);
            EpicWeaponFocusFeat = Resolve(EpicWeaponFocusFeat, MasterRepository.Feats);
            WeaponSpecializationFeat = Resolve(WeaponSpecializationFeat, MasterRepository.Feats);
            EpicWeaponSpecializationFeat = Resolve(EpicWeaponSpecializationFeat, MasterRepository.Feats);
            ImprovedCriticalFeat = Resolve(ImprovedCriticalFeat, MasterRepository.Feats);
            OverwhelmingCriticalFeat = Resolve(OverwhelmingCriticalFeat, MasterRepository.Feats);
            DevastatingCriticalFeat = Resolve(DevastatingCriticalFeat, MasterRepository.Feats);
            WeaponOfChoiceFeat = Resolve(WeaponOfChoiceFeat, MasterRepository.Feats);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.StatsText.FromJson(json["StatsText"]?.AsObject());
            this.Icon = json["Icon"]?.GetValue<string>();
            this.InventorySlotWidth = json["InventorySlotWidth"]?.GetValue<int>() ?? 1;
            this.InventorySlotHeight = json["InventorySlotHeight"]?.GetValue<int>() ?? 1;
            this.EquipableSlots = JsonToEnum<InventorySlots>(json["EquipableSlots"]) ?? (InventorySlots)0;
            this.CanRotateIcon = json["CanRotateIcon"]?.GetValue<bool>() ?? false;
            this.ModelType = JsonToEnum<ItemModelType>(json["ModelType"]) ?? ItemModelType.Simple;
            this.ItemModel = json["ItemModel"]?.GetValue<string>() ?? "";
            this.GenderSpecific = json["GenderSpecific"]?.GetValue<bool>() ?? false;
            this.Part1Alpha = JsonToEnum<AlphaChannelUsageType>(json["Part1Alpha"]);
            this.Part2Alpha = JsonToEnum<AlphaChannelUsageType>(json["Part2Alpha"]);
            this.Part3Alpha = JsonToEnum<AlphaChannelUsageType>(json["Part3Alpha"]);
            this.DefaultModel = json["DefaultModel"]?.GetValue<string>() ?? "";
            this.IsContainer = json["IsContainer"]?.GetValue<bool>() ?? false;
            this.WeaponWieldType = JsonToEnum<WeaponWieldType>(json["WeaponWieldType"]) ?? WeaponWieldType.Standard;
            this.WeaponDamageType = JsonToEnum<WeaponDamageType>(json["WeaponDamageType"]);
            this.WeaponSize = JsonToEnum<WeaponSize>(json["WeaponSize"]);
            this.AmmunitionBaseItem = CreateRefFromJson<BaseItem>(json["AmmunitionBaseItem"]?.AsObject());
            this.PreferredAttackDistance = json["PreferredAttackDistance"]?.GetValue<double>();
            this.MinimumModelCount = json["MinimumModelCount"]?.GetValue<int>() ?? 10;
            this.MaximumModelCount = json["MaximumModelCount"]?.GetValue<int>() ?? 100;
            this.DamageDiceCount = json["DamageDiceCount"]?.GetValue<int>();
            this.DamageDice = json["DamageDice"]?.GetValue<int>();
            this.CriticalThreatRange = json["CriticalThreatRange"]?.GetValue<int>();
            this.CriticalMultiplier = json["CriticalMultiplier"]?.GetValue<int>();
            this.Category = JsonToEnum<ItemCategory>(json["Category"]) ?? ItemCategory.None;
            this.BaseCost = json["BaseCost"]?.GetValue<double>() ?? 0.0;
            this.MaxStackSize = json["MaxStackSize"]?.GetValue<int>() ?? 1;
            this.ItemCostMultiplier = json["ItemCostMultiplier"]?.GetValue<double>() ?? 1.0;
            this.InventorySound = CreateRefFromJson<InventorySound>(json["InventorySound"]?.AsObject());
            this.MinSpellProperties = json["MinSpellProperties"]?.GetValue<int>() ?? 0;
            this.MaxSpellProperties = json["MaxSpellProperties"]?.GetValue<int>() ?? 8;
            this.ItemPropertySet = CreateRefFromJson<ItemPropertySet>(json["ItemPropertySet"]?.AsObject());
            this.StorePanel = JsonToEnum<StorePanelType>(json["StorePanel"]);
            this.RequiredFeat1 = CreateRefFromJson<Feat>(json["RequiredFeat1"]?.AsObject());
            this.RequiredFeat2 = CreateRefFromJson<Feat>(json["RequiredFeat2"]?.AsObject());
            this.RequiredFeat3 = CreateRefFromJson<Feat>(json["RequiredFeat3"]?.AsObject());
            this.RequiredFeat4 = CreateRefFromJson<Feat>(json["RequiredFeat4"]?.AsObject());
            this.RequiredFeat5 = CreateRefFromJson<Feat>(json["RequiredFeat5"]?.AsObject());
            this.ArmorClassType = JsonToEnum<ArmorClassType>(json["ArmorClassType"]);
            this.BaseShieldAC = json["BaseShieldAC"]?.GetValue<int>() ?? 0;
            this.ArmorCheckPenalty = json["ArmorCheckPenalty"]?.GetValue<int>() ?? 0;
            this.DefaultChargeCount = json["DefaultChargeCount"]?.GetValue<int>() ?? 0;
            this.GroundModelRotation = JsonToEnum<ItemModelRotation>(json["GroundModelRotation"]) ?? ItemModelRotation.None;
            this.Weight = json["Weight"]?.GetValue<double>() ?? 0;
            this.WeaponSound = CreateRefFromJson<WeaponSound>(json["WeaponSound"]?.AsObject());
            this.AmmunitionType = JsonToEnum<AmmunitionType>(json["AmmunitionType"]);
            this.QuickbarBehaviour = JsonToEnum<QuickbarBehaviour>(json["QuickbarBehaviour"]) ?? QuickbarBehaviour.Default;
            this.ArcaneSpellFailure = json["ArcaneSpellFailure"]?.GetValue<int>();
            this.LeftSlashAnimationPercent = json["LeftSlashAnimationPercent"]?.GetValue<int>();
            this.RightSlashAnimationPercent = json["RightSlashAnimationPercent"]?.GetValue<int>();
            this.StraightSlashAnimationPercent = json["StraightSlashAnimationPercent"]?.GetValue<int>();
            this.StorePanelOrder = json["StorePanelOrder"]?.GetValue<int>();
            this.ItemLevelRestrictionStackSize = json["ItemLevelRestrictionStackSize"]?.GetValue<int>() ?? 1;
            this.WeaponFocusFeat = CreateRefFromJson<Feat>(json["WeaponFocusFeat"]?.AsObject());
            this.EpicWeaponFocusFeat = CreateRefFromJson<Feat>(json["EpicWeaponFocusFeat"]?.AsObject());
            this.WeaponSpecializationFeat = CreateRefFromJson<Feat>(json["WeaponSpecializationFeat"]?.AsObject());
            this.EpicWeaponSpecializationFeat = CreateRefFromJson<Feat>(json["EpicWeaponSpecializationFeat"]?.AsObject());
            this.ImprovedCriticalFeat = CreateRefFromJson<Feat>(json["ImprovedCriticalFeat"]?.AsObject());
            this.OverwhelmingCriticalFeat = CreateRefFromJson<Feat>(json["OverwhelmingCriticalFeat"]?.AsObject());
            this.DevastatingCriticalFeat = CreateRefFromJson<Feat>(json["DevastatingCriticalFeat"]?.AsObject());
            this.WeaponOfChoiceFeat = CreateRefFromJson<Feat>(json["WeaponOfChoiceFeat"]?.AsObject());
            this.IsMonkWeapon = json["IsMonkWeapon"]?.GetValue<bool>() ?? false;
            this.WeaponFinesseMinimumCreatureSize = JsonToEnum<SizeCategory>(json["WeaponFinesseMinimumCreatureSize"]);
        }

        public override JsonObject ToJson()
        {
            var baseItemJson = base.ToJson();
            baseItemJson.Add("Name", this.Name.ToJson());
            baseItemJson.Add("Description", this.Description.ToJson());
            baseItemJson.Add("StatsText", this.StatsText.ToJson());
            baseItemJson.Add("Icon", this.Icon);
            baseItemJson.Add("InventorySlotWidth", this.InventorySlotWidth);
            baseItemJson.Add("InventorySlotHeight", this.InventorySlotHeight);
            baseItemJson.Add("EquipableSlots", EnumToJson(this.EquipableSlots));
            baseItemJson.Add("CanRotateIcon", this.CanRotateIcon);
            baseItemJson.Add("ModelType", EnumToJson(this.ModelType));
            baseItemJson.Add("ItemModel", this.ItemModel);
            baseItemJson.Add("GenderSpecific", this.GenderSpecific);
            baseItemJson.Add("Part1Alpha", EnumToJson(this.Part1Alpha));
            baseItemJson.Add("Part2Alpha", EnumToJson(this.Part2Alpha));
            baseItemJson.Add("Part3Alpha", EnumToJson(this.Part3Alpha));
            baseItemJson.Add("DefaultModel", this.DefaultModel);
            baseItemJson.Add("IsContainer", this.IsContainer);
            baseItemJson.Add("WeaponWieldType", EnumToJson(this.WeaponWieldType));
            baseItemJson.Add("WeaponDamageType", EnumToJson(this.WeaponDamageType));
            baseItemJson.Add("WeaponSize", EnumToJson(this.WeaponSize));
            baseItemJson.Add("AmmunitionBaseItem", CreateJsonRef(this.AmmunitionBaseItem));
            baseItemJson.Add("PreferredAttackDistance", this.PreferredAttackDistance);
            baseItemJson.Add("MinimumModelCount", this.MinimumModelCount);
            baseItemJson.Add("MaximumModelCount", this.MaximumModelCount);
            baseItemJson.Add("DamageDiceCount", this.DamageDiceCount);
            baseItemJson.Add("DamageDice", this.DamageDice);
            baseItemJson.Add("CriticalThreatRange", this.CriticalThreatRange);
            baseItemJson.Add("CriticalMultiplier", this.CriticalMultiplier);
            baseItemJson.Add("Category", EnumToJson(this.Category));
            baseItemJson.Add("BaseCost", this.BaseCost);
            baseItemJson.Add("MaxStackSize", this.MaxStackSize);
            baseItemJson.Add("ItemCostMultiplier", this.ItemCostMultiplier);
            baseItemJson.Add("InventorySound", CreateJsonRef(this.InventorySound));
            baseItemJson.Add("MinSpellProperties", this.MinSpellProperties);
            baseItemJson.Add("MaxSpellProperties", this.MaxSpellProperties);
            baseItemJson.Add("ItemPropertySet", CreateJsonRef(this.ItemPropertySet));
            baseItemJson.Add("StorePanel", EnumToJson(this.StorePanel));
            baseItemJson.Add("RequiredFeat1", CreateJsonRef(this.RequiredFeat1));
            baseItemJson.Add("RequiredFeat2", CreateJsonRef(this.RequiredFeat2));
            baseItemJson.Add("RequiredFeat3", CreateJsonRef(this.RequiredFeat3));
            baseItemJson.Add("RequiredFeat4", CreateJsonRef(this.RequiredFeat4));
            baseItemJson.Add("RequiredFeat5", CreateJsonRef(this.RequiredFeat5));
            baseItemJson.Add("ArmorClassType", EnumToJson(this.ArmorClassType));
            baseItemJson.Add("BaseShieldAC", this.BaseShieldAC);
            baseItemJson.Add("ArmorCheckPenalty", this.ArmorCheckPenalty);
            baseItemJson.Add("DefaultChargeCount", this.DefaultChargeCount);
            baseItemJson.Add("GroundModelRotation", EnumToJson(this.GroundModelRotation));
            baseItemJson.Add("Weight", this.Weight);
            baseItemJson.Add("WeaponSound", CreateJsonRef(this.WeaponSound));
            baseItemJson.Add("AmmunitionType", EnumToJson(this.AmmunitionType));
            baseItemJson.Add("QuickbarBehaviour", EnumToJson(this.QuickbarBehaviour));
            baseItemJson.Add("ArcaneSpellFailure", this.ArcaneSpellFailure);
            baseItemJson.Add("LeftSlashAnimationPercent", this.LeftSlashAnimationPercent);
            baseItemJson.Add("RightSlashAnimationPercent", this.RightSlashAnimationPercent);
            baseItemJson.Add("StraightSlashAnimationPercent", this.StraightSlashAnimationPercent);
            baseItemJson.Add("StorePanelOrder", this.StorePanelOrder);
            baseItemJson.Add("ItemLevelRestrictionStackSize", this.ItemLevelRestrictionStackSize);
            baseItemJson.Add("WeaponFocusFeat", CreateJsonRef(this.WeaponFocusFeat));
            baseItemJson.Add("EpicWeaponFocusFeat", CreateJsonRef(this.EpicWeaponFocusFeat));
            baseItemJson.Add("WeaponSpecializationFeat", CreateJsonRef(this.WeaponSpecializationFeat));
            baseItemJson.Add("EpicWeaponSpecializationFeat", CreateJsonRef(this.EpicWeaponSpecializationFeat));
            baseItemJson.Add("ImprovedCriticalFeat", CreateJsonRef(this.ImprovedCriticalFeat));
            baseItemJson.Add("OverwhelmingCriticalFeat", CreateJsonRef(this.OverwhelmingCriticalFeat));
            baseItemJson.Add("DevastatingCriticalFeat", CreateJsonRef(this.DevastatingCriticalFeat));
            baseItemJson.Add("WeaponOfChoiceFeat", CreateJsonRef(this.WeaponOfChoiceFeat));
            baseItemJson.Add("IsMonkWeapon", this.IsMonkWeapon);
            baseItemJson.Add("WeaponFinesseMinimumCreatureSize", EnumToJson(this.WeaponFinesseMinimumCreatureSize));

            return baseItemJson;
        }
    }
}
