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

        public static readonly String RacesFile;
        public static readonly String ClassesFile;
        public static readonly String DomainsFile;
        public static readonly String SpellsFile;
        public static readonly String FeatsFile;
        public static readonly String SkillsFile;
        public static readonly String DiseasesFile;
        public static readonly String PoisonsFile;

        static Constants()
        {
            var eosAppDataFolder = "Eos Toolset";
            AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + eosAppDataFolder + Path.DirectorySeparatorChar;

            var baseDataFolder = "BaseData";
            BaseDataPath = AppDataPath + baseDataFolder + Path.DirectorySeparatorChar;

            var baseResourceFolder = "Resources";
            BaseResourcePath = AppDataPath + baseResourceFolder + Path.DirectorySeparatorChar;

            RacesFile = BaseDataPath + "races.json";
            ClassesFile = BaseDataPath + "classes.json";
            DomainsFile = BaseDataPath + "domains.json";
            SpellsFile = BaseDataPath + "spells.json";
            FeatsFile = BaseDataPath + "feats.json";
            SkillsFile = BaseDataPath + "skills.json";
            DiseasesFile = BaseDataPath + "diseases.json";
            PoisonsFile = BaseDataPath + "poisons.json";
        }
    }
}
