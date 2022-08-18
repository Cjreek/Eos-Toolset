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

        public static readonly String RacesFilePath;
        public static readonly String ClassesFilePath;
        public static readonly String DomainsFilePath;
        public static readonly String SpellsFilePath;
        public static readonly String FeatsFilePath;
        public static readonly String SkillsFilePath;
        public static readonly String DiseasesFilePath;
        public static readonly String PoisonsFilePath;

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

            RacesFilePath = BaseDataPath + RacesFilename;
            ClassesFilePath = BaseDataPath + ClassesFilename;
            DomainsFilePath = BaseDataPath + DomainsFilename;
            SpellsFilePath = BaseDataPath + SpellsFilename;
            FeatsFilePath = BaseDataPath + FeatsFilename;
            SkillsFilePath = BaseDataPath + SkillsFilename;
            DiseasesFilePath = BaseDataPath + DiseasesFilename;
            PoisonsFilePath = BaseDataPath + PoisonsFilename;
        }
    }
}
