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

        public static readonly String ExportFolder;
        public static readonly String ExportTLKFolder;
        public static readonly String ExportHAKFolder;

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

        public static readonly String AppearancesFilename;
        public static readonly String ClassPackagesFilename;
        public static readonly String SoundsetsFilename;

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

        public static readonly String CustomEnumsFilename;
        public static readonly String CustomObjectsFilename;

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

        public static readonly String AppearancesFilePath;
        public static readonly String ClassPackagesFilePath;
        public static readonly String SoundsetsFilePath;

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

            ExportFolder = @"output\";
            ExportHAKFolder = ExportFolder + @"hak\";
            ExportTLKFolder = ExportFolder + @"tlk\";

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

            AppearancesFilename = "appearances.json";
            ClassPackagesFilename = "packages.json";
            SoundsetsFilename = "soundsets.json";

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

            CustomEnumsFilename = "customenums.json";
            CustomObjectsFilename = "customobjects.json";

            IconResourcesFolder = @"icons\";
            ExternalFilesPath = @"external\";

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

            AppearancesFilePath = BaseDataPath + AppearancesFilename;
            ClassPackagesFilePath = BaseDataPath + ClassPackagesFilename;
            SoundsetsFilePath = BaseDataPath + SoundsetsFilename;

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

            IconResourcesFilePath = BaseDataPath + IconResourcesFolder;
        }
    }
}
