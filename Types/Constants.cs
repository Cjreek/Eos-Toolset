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

        public static readonly String ProjectFilename;

        public static readonly String RacesFilename;
        public static readonly String ClassesFilename;
        public static readonly String DomainsFilename;
        public static readonly String SpellsFilename;
        public static readonly String FeatsFilename;
        public static readonly String SkillsFilename;
        public static readonly String DiseasesFilename;
        public static readonly String PoisonsFilename;
        public static readonly String SpellbooksFilename;

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

        public static readonly String RacesFilePath;
        public static readonly String ClassesFilePath;
        public static readonly String DomainsFilePath;
        public static readonly String SpellsFilePath;
        public static readonly String FeatsFilePath;
        public static readonly String SkillsFilePath;
        public static readonly String DiseasesFilePath;
        public static readonly String PoisonsFilePath;
        public static readonly String SpellbooksFilePath;

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

        static Constants()
        {
            var eosAppDataFolder = "Eos Toolset";
            AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + eosAppDataFolder + Path.DirectorySeparatorChar;

            var baseDataFolder = "BaseData";
            BaseDataPath = AppDataPath + baseDataFolder + Path.DirectorySeparatorChar;

            var baseResourceFolder = "Resources";
            BaseResourcePath = AppDataPath + baseResourceFolder + Path.DirectorySeparatorChar;

            ProjectFilename = "project.json";

            RacesFilename = "races.json";
            ClassesFilename = "classes.json";
            DomainsFilename = "domains.json";
            SpellsFilename = "spells.json";
            FeatsFilename = "feats.json";
            SkillsFilename = "skills.json";
            DiseasesFilename = "diseases.json";
            PoisonsFilename = "poisons.json";
            SpellbooksFilename = "spellbooks.json";

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

            RacesFilePath = BaseDataPath + RacesFilename;
            ClassesFilePath = BaseDataPath + ClassesFilename;
            DomainsFilePath = BaseDataPath + DomainsFilename;
            SpellsFilePath = BaseDataPath + SpellsFilename;
            FeatsFilePath = BaseDataPath + FeatsFilename;
            SkillsFilePath = BaseDataPath + SkillsFilename;
            DiseasesFilePath = BaseDataPath + DiseasesFilename;
            PoisonsFilePath = BaseDataPath + PoisonsFilename;
            SpellbooksFilePath = BaseDataPath + SpellbooksFilename;

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
        }
    }
}
