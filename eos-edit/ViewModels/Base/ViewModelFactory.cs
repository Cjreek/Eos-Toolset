using Eos.Models;
using Eos.Models.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Base
{
    internal static class ViewModelFactory
    {
        public static DataDetailViewModelBase CreateViewModel(object model)
        {
            if (model == null) throw new ArgumentNullException("model");

            if (model is Race race)
                return new RaceViewModel(race);
            if (model is CharacterClass cls)
                return new ClassViewModel(cls);
            if (model is Domain domain)
                return new DomainViewModel(domain);
            if (model is Spell spell)
                return new SpellViewModel(spell);
            if (model is Feat feat)
                return new FeatViewModel(feat);
            if (model is Skill skill)
                return new SkillViewModel(skill);
            if (model is Disease disease)
                return new DiseaseViewModel(disease);
            if (model is Poison poison)
                return new PoisonViewModel(poison);
            if (model is Spellbook spellbook)
                return new SpellbookViewModel(spellbook);
            if (model is AreaEffect aoe)
                return new AreaEffectViewModel(aoe);
            if (model is MasterFeat masterFeat)
                return new MasterFeatViewModel(masterFeat);
            if (model is BaseItem baseItem)
                return new BaseItemViewModel(baseItem);

            if (model is ClassPackage classPackage)
                return new ClassPackageViewModel(classPackage);
            if (model is Soundset soundset)
                return new SoundsetViewModel(soundset);
            if (model is Appearance appearance)
                return new AppearanceViewModel(appearance);
            if (model is AppearanceSoundset appearanceSoundset)
                return new AppearanceSoundsetViewModel(appearanceSoundset);
            if (model is WeaponSound weaponSound)
                return new WeaponSoundViewModel(weaponSound);
            if (model is InventorySound inventorySound)
                return new InventorySoundViewModel(inventorySound);
            if (model is Polymorph polymorph)
                return new PolymorphViewModel(polymorph);
            if (model is Companion companion)
                return new CompanionViewModel(companion);
            if (model is Familiar familiar)
                return new FamiliarViewModel(familiar);
            if (model is Trap trap)
                return new TrapViewModel(trap);
            if (model is Portrait portrait)
                return new PortraitViewModel(portrait);
            if (model is ItemProperty itemProperty)
                return new ItemPropertyViewModel(itemProperty);
            if (model is ItemPropertyTable itemPropertyTable)
                return new ItemPropertyTableViewModel(itemPropertyTable);
            if (model is ItemPropertyCostTable itemPropertyCostTable)
                return new ItemPropertyCostTableViewModel(itemPropertyCostTable);
            if (model is ItemPropertyParam itemPropertyParam)
                return new ItemPropertyParamViewModel(itemPropertyParam);
            if (model is ItemPropertySet itemPropertySet)
                return new ItemPropertySetViewModel(itemPropertySet);
            if (model is VisualEffect vfx)
                return new VisualEffectViewModel(vfx);
            if (model is ProgrammedEffect progFX)
                return new ProgrammedEffectViewModel(progFX);
            if (model is DamageType damageType)
                return new DamageTypeViewModel(damageType);
            if (model is DamageTypeGroup damageTypeGroup)
                return new DamageTypeGroupViewModel(damageTypeGroup);
            if (model is RangedDamageType rangedDamageType)
                return new RangedDamageTypeViewModel(rangedDamageType);
            if (model is SavingthrowType savingthrowType)
                return new SavingthrowTypeViewModel(savingthrowType);
            if (model is Ammunition ammunition)
                return new AmmunitionViewModel(ammunition);

            if (model is FeatsTable featsTable)
                return new FeatsTableViewModel(featsTable);
            if (model is AttackBonusTable abTable)
                return new AttackBonusTableViewModel(abTable);
            if (model is BonusFeatsTable bonusFeatsTable)
                return new BonusFeatTableViewModel(bonusFeatsTable);
            if (model is SkillsTable skillTable)
                return new SkillTableViewModel(skillTable);
            if (model is SavingThrowTable savesTable)
                return new SavingThrowTableViewModel(savesTable);
            if (model is PrerequisiteTable requTable)
                return new PrerequisiteTableViewModel(requTable);
            if (model is StatGainTable statGainTable)
                return new StatGainTableViewModel(statGainTable);
            if (model is RacialFeatsTable racialFeatsTable)
                return new RacialFeatsTableViewModel(racialFeatsTable);
            if (model is SpellSlotTable spellSlotsTable)
                return new SpellSlotTableViewModel(spellSlotsTable);
            if (model is KnownSpellsTable knownSpellsTable)
                return new KnownSpellsTableViewModel(knownSpellsTable);
            if (model is PackageSpellPreferencesTable spellPrefsTable)
                return new PackageSpellPreferencesTableViewModel(spellPrefsTable);
            if (model is PackageFeatPreferencesTable featPrefsTable)
                return new PackageFeatPreferencesTableViewModel(featPrefsTable);
            if (model is PackageSkillPreferencesTable skillPrefsTable)
                return new PackageSkillPreferencesTableViewModel(skillPrefsTable);
            if (model is PackageEquipmentTable equipmentTable)
                return new PackageEquipmentTableViewModel(equipmentTable);

            if (model is CustomEnum customEnum)
                return new CustomEnumViewModel(customEnum);
            if (model is CustomObject customObject)
                return new CustomObjectViewModel(customObject);
            if (model is CustomTable customTable)
                return new CustomTableViewModel(customTable);
            if (model is CustomDynamicTable customDynamicTable)
                return new CustomDynamicTableViewModel(customDynamicTable);
            if (model is CustomObjectInstance customObjectInstance)
                return new CustomObjectInstanceViewModel(customObjectInstance);
            if (model is CustomTableInstance customTableInstance)
                return new CustomTableInstanceViewModel(customTableInstance);
            if (model is CustomDynamicTableInstance customDynamicTableInstance)
                return new CustomDynamicTableInstanceViewModel(customDynamicTableInstance);

            if (model is ModelExtension extension)
                return new ModelExtensionViewModel(extension);
            
            if (model is TlkStringTable tlkStringTable)
                return new TlkStringTableViewModel(tlkStringTable);

            else
                throw new ArgumentException("No viewmodel found", "model");
        }
    }
}
