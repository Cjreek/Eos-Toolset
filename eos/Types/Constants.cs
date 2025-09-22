using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.Types
{
    public static class Constants
    {
        public static readonly String AppDataPath;
        public static readonly String BaseDataPath;
        public static readonly String BaseResourcePath;
        public static readonly String ConfigPath;

        public static readonly String BackupFolder;

        public static readonly String ExportFolder;
        public static readonly String Export2DAFolder;
        public static readonly String ExportSSFFolder;
        public static readonly String ExportTLKFolder;
        public static readonly String ExportHAKFolder;
        public static readonly String ExportIncludeFolder;
        public static readonly String ExportERFFolder;

        public static readonly String IncludeFilename;

        public static readonly String ProjectFileExtension;

        public static readonly String RacesFilename;
        public static readonly String ClassesFilename;
        public static readonly String DomainsFilename;
        public static readonly String SpellsFilename;
        public static readonly String FeatsFilename;
        public static readonly String SkillsFilename;
        public static readonly String DiseasesFilename;
        public static readonly String PoisonsFilename;
        public static readonly String SpellbooksFilename;
        public static readonly String AreaEffectsFilename;
        public static readonly String MasterFeatsFilename;
        public static readonly String BaseItemsFilename;
        public static readonly String ItemPropertySetsFilename;
        public static readonly String ItemPropertiesFilename;

        public static readonly String AmmunitionsFilename;
        public static readonly String AppearancesFilename;
        public static readonly String AppearanceSoundsetsFilename;
        public static readonly String WeaponSoundsFilename;
        public static readonly String InventorySoundsFilename;
        public static readonly String PortraitsFilename;
        public static readonly String VisualEffectsFilename;
        public static readonly String ClassPackagesFilename;
        public static readonly String SoundsetsFilename;
        public static readonly String PolymorphsFilename;
        public static readonly String CompanionsFilename;
        public static readonly String FamiliarsFilename;
        public static readonly String TrapsFilename;
        public static readonly String ProgrammedEffectsFilename;
        public static readonly String DamageTypesFilename;
        public static readonly String DamageTypeGroupsFilename;
        public static readonly String RangedDamageTypesFilename;
        public static readonly String SavingThrowTypesFilename;

        public static readonly String AttackBonusTablesFilename;
        public static readonly String BonusFeatTablesFilename;
        public static readonly String FeatTablesFilename;
        public static readonly String SavingThrowTablesFilename;
        public static readonly String PrerequisiteTablesFilename;
        public static readonly String SkillTablesFilename;
        public static readonly String SpellSlotTablesFilename;
        public static readonly String KnownSpellsTablesFilename;
        public static readonly String StatGainTablesFilename;
        public static readonly String RacialFeatsTablesFilename;
        public static readonly String SpellPreferencesTablesFilename;
        public static readonly String FeatPreferencesTablesFilename;
        public static readonly String SkillPreferencesTablesFilename;
        public static readonly String PackageEquipmentTablesFilename;
        public static readonly String ItemPropertyTablesFilename;
        public static readonly String ItemPropertyCostTablesFilename;
        public static readonly String ItemPropertyParamsFilename;

        public static readonly String CustomEnumsFilename;
        public static readonly String CustomObjectsFilename;
        public static readonly String CustomTablesFilename;
        public static readonly String CustomDynamicTablesFilename;
        public static readonly String CustomTlkStringsFilename;

        public static readonly String IconResourcesFolder;
        public static readonly String ExternalFilesPath;

        public static readonly String RacesFilePath;
        public static readonly String ClassesFilePath;
        public static readonly String DomainsFilePath;
        public static readonly String SpellsFilePath;
        public static readonly String FeatsFilePath;
        public static readonly String SkillsFilePath;
        public static readonly String DiseasesFilePath;
        public static readonly String PoisonsFilePath;
        public static readonly String SpellbooksFilePath;
        public static readonly String AreaEffectsFilePath;
        public static readonly String MasterFeatsFilePath;
        public static readonly String BaseItemsFilePath;
        public static readonly String ItemPropertySetsFilePath;
        public static readonly String ItemPropertiesFilePath;

        public static readonly String AmmunitionsFilePath;
        public static readonly String AppearancesFilePath;
        public static readonly String AppearanceSoundsetsFilePath;
        public static readonly String WeaponSoundsFilePath;
        public static readonly String InventorySoundsFilePath;
        public static readonly String PortraitsFilePath;
        public static readonly String VisualEffectsFilePath; 
        public static readonly String ClassPackagesFilePath;
        public static readonly String SoundsetsFilePath;
        public static readonly String PolymorphsFilePath;
        public static readonly String CompanionsFilePath;
        public static readonly String FamiliarsFilePath;
        public static readonly String TrapsFilePath;
        public static readonly String ProgrammedEffectsFilePath;
        public static readonly String DamageTypesFilePath;
        public static readonly String DamageTypeGroupsFilePath;
        public static readonly String RangedDamageTypesFilePath;
        public static readonly String SavingThrowTypesFilePath;

        public static readonly String AttackBonusTablesFilePath;
        public static readonly String BonusFeatTablesFilePath;
        public static readonly String FeatTablesFilePath;
        public static readonly String SavingThrowTablesFilePath;
        public static readonly String PrerequisiteTablesFilePath;
        public static readonly String SkillTablesFilePath;
        public static readonly String SpellSlotTablesFilePath;
        public static readonly String KnownSpellsTablesFilePath;
        public static readonly String StatGainTablesFilePath;
        public static readonly String RacialFeatsTablesFilePath;
        public static readonly String SpellPreferencesTablesFilePath;
        public static readonly String FeatPreferencesTablesFilePath;
        public static readonly String SkillPreferencesTablesFilePath;
        public static readonly String PackageEquipmentTablesFilePath;
        public static readonly String ItemPropertyTablesFilePath;
        public static readonly String ItemPropertyCostTablesFilePath;
        public static readonly String ItemPropertyParamsFilePath;

        public static readonly String IconResourcesFilePath;

        static Constants()
        {
            var eosAppDataFolder = "Eos Toolset";
            AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + eosAppDataFolder + Path.DirectorySeparatorChar;
            ConfigPath = AppDataPath + "config.json";

            var baseDataFolder = "BaseData";
            BaseDataPath = AppDataPath + baseDataFolder + Path.DirectorySeparatorChar;

            var baseResourceFolder = "Resources";
            BaseResourcePath = AppDataPath + baseResourceFolder + Path.DirectorySeparatorChar;

            BackupFolder = "backup" + Path.DirectorySeparatorChar;

            ExportFolder = "output" + Path.DirectorySeparatorChar;
            Export2DAFolder = ExportFolder;
            ExportSSFFolder = ExportFolder;
            ExportHAKFolder = ExportFolder + "hak" + Path.DirectorySeparatorChar;
            ExportTLKFolder = ExportFolder + "tlk" + Path.DirectorySeparatorChar;
            ExportIncludeFolder = ExportFolder + "include" + Path.DirectorySeparatorChar;
            ExportERFFolder = ExportFolder + "erf" + Path.DirectorySeparatorChar;

            IncludeFilename = "inc_eos";

            ProjectFileExtension = ".eosproj";

            RacesFilename = "races.json";
            ClassesFilename = "classes.json";
            DomainsFilename = "domains.json";
            SpellsFilename = "spells.json";
            FeatsFilename = "feats.json";
            SkillsFilename = "skills.json";
            DiseasesFilename = "diseases.json";
            PoisonsFilename = "poisons.json";
            SpellbooksFilename = "spellbooks.json";
            AreaEffectsFilename = "areaeffects.json";
            MasterFeatsFilename = "masterfeats.json";
            BaseItemsFilename = "baseitems.json";
            ItemPropertySetsFilename = "itempropsets.json";
            ItemPropertiesFilename = "itemproperties.json";

            AmmunitionsFilename = "ammunitiontypes.json";
            AppearancesFilename = "appearances.json";
            AppearanceSoundsetsFilename = "appearancesoundsets.json";
            WeaponSoundsFilename = "weaponsounds.json";
            InventorySoundsFilename = "inventorysounds.json";
            PortraitsFilename = "portraits.json";
            VisualEffectsFilename = "vfx.json";
            ClassPackagesFilename = "packages.json";
            SoundsetsFilename = "soundsets.json";
            PolymorphsFilename = "polymorphs.json";
            CompanionsFilename = "companions.json";
            FamiliarsFilename = "familiars.json";
            TrapsFilename = "traps.json";
            ProgrammedEffectsFilename = "progfx.json";
            DamageTypesFilename = "damagetypes.json";
            DamageTypeGroupsFilename = "damagetypegroups.json";
            RangedDamageTypesFilename = "rangeddamagetypes.json";
            SavingThrowTypesFilename = "savingthrowtypes.json";

            AttackBonusTablesFilename = "tables_bab.json";
            BonusFeatTablesFilename = "tables_bonusfeats.json";
            FeatTablesFilename = "tables_feats.json";
            SavingThrowTablesFilename = "tables_saves.json";
            PrerequisiteTablesFilename = "tables_prerequisites.json";
            SkillTablesFilename = "tables_skills.json";
            SpellSlotTablesFilename = "tables_spellslots.json";
            KnownSpellsTablesFilename = "tables_knownspells.json";
            StatGainTablesFilename = "tables_statgain.json";
            RacialFeatsTablesFilename = "tables_racialfeats.json";
            SpellPreferencesTablesFilename = "package_spells.json";
            FeatPreferencesTablesFilename = "package_feats.json";
            SkillPreferencesTablesFilename = "package_skills.json";
            PackageEquipmentTablesFilename = "package_equipment.json";
            ItemPropertyTablesFilename = "iprp_tables.json";
            ItemPropertyCostTablesFilename = "iprp_costtables.json";
            ItemPropertyParamsFilename = "iprp_params.json";

            CustomEnumsFilename = "customenums.json";
            CustomObjectsFilename = "customobjects.json";
            CustomTablesFilename = "customtables.json";
            CustomDynamicTablesFilename = "customdyntables.json";
            CustomTlkStringsFilename = "tlk.json";

            IconResourcesFolder = @"icons" + Path.DirectorySeparatorChar;
            ExternalFilesPath = @"external" + Path.DirectorySeparatorChar;

            RacesFilePath = BaseDataPath + RacesFilename;
            ClassesFilePath = BaseDataPath + ClassesFilename;
            DomainsFilePath = BaseDataPath + DomainsFilename;
            SpellsFilePath = BaseDataPath + SpellsFilename;
            FeatsFilePath = BaseDataPath + FeatsFilename;
            SkillsFilePath = BaseDataPath + SkillsFilename;
            DiseasesFilePath = BaseDataPath + DiseasesFilename;
            PoisonsFilePath = BaseDataPath + PoisonsFilename;
            SpellbooksFilePath = BaseDataPath + SpellbooksFilename;
            AreaEffectsFilePath = BaseDataPath + AreaEffectsFilename;
            MasterFeatsFilePath = BaseDataPath + MasterFeatsFilename;
            BaseItemsFilePath= BaseDataPath + BaseItemsFilename;
            ItemPropertySetsFilePath = BaseDataPath + ItemPropertySetsFilename;
            ItemPropertiesFilePath = BaseDataPath + ItemPropertiesFilename;

            AmmunitionsFilePath = BaseDataPath + AmmunitionsFilename;
            AppearancesFilePath = BaseDataPath + AppearancesFilename;
            AppearanceSoundsetsFilePath = BaseDataPath + AppearanceSoundsetsFilename;
            WeaponSoundsFilePath = BaseDataPath + WeaponSoundsFilename;
            InventorySoundsFilePath = BaseDataPath + InventorySoundsFilename;
            PortraitsFilePath = BaseDataPath + PortraitsFilename;
            VisualEffectsFilePath = BaseDataPath + VisualEffectsFilename;
            ClassPackagesFilePath = BaseDataPath + ClassPackagesFilename;
            SoundsetsFilePath = BaseDataPath + SoundsetsFilename;
            PolymorphsFilePath = BaseDataPath + PolymorphsFilename;
            CompanionsFilePath = BaseDataPath + CompanionsFilename;
            FamiliarsFilePath = BaseDataPath + FamiliarsFilename;
            TrapsFilePath = BaseDataPath + TrapsFilename;
            ProgrammedEffectsFilePath = BaseDataPath + ProgrammedEffectsFilename;
            DamageTypesFilePath = BaseDataPath + DamageTypesFilename;
            DamageTypeGroupsFilePath = BaseDataPath + DamageTypeGroupsFilename;
            RangedDamageTypesFilePath = BaseDataPath + RangedDamageTypesFilename;
            SavingThrowTypesFilePath = BaseDataPath + SavingThrowTypesFilename;

            AttackBonusTablesFilePath = BaseDataPath + AttackBonusTablesFilename;
            BonusFeatTablesFilePath = BaseDataPath + BonusFeatTablesFilename;
            FeatTablesFilePath = BaseDataPath + FeatTablesFilename;
            SavingThrowTablesFilePath = BaseDataPath + SavingThrowTablesFilename;
            PrerequisiteTablesFilePath = BaseDataPath + PrerequisiteTablesFilename;
            SkillTablesFilePath = BaseDataPath + SkillTablesFilename;
            SpellSlotTablesFilePath = BaseDataPath + SpellSlotTablesFilename;
            KnownSpellsTablesFilePath = BaseDataPath + KnownSpellsTablesFilename;
            StatGainTablesFilePath = BaseDataPath + StatGainTablesFilename;
            RacialFeatsTablesFilePath = BaseDataPath + RacialFeatsTablesFilename;
            SpellPreferencesTablesFilePath = BaseDataPath + SpellPreferencesTablesFilename;
            FeatPreferencesTablesFilePath = BaseDataPath + FeatPreferencesTablesFilename;
            SkillPreferencesTablesFilePath = BaseDataPath + SkillPreferencesTablesFilename;
            PackageEquipmentTablesFilePath = BaseDataPath + PackageEquipmentTablesFilename;
            ItemPropertyTablesFilePath = BaseDataPath + ItemPropertyTablesFilename;
            ItemPropertyCostTablesFilePath = BaseDataPath + ItemPropertyCostTablesFilename;
            ItemPropertyParamsFilePath = BaseDataPath + ItemPropertyParamsFilename;

            IconResourcesFilePath = BaseDataPath + IconResourcesFolder;
        }
    }
}
