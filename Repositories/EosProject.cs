using Eos.Config;
using Eos.Models.Tables;
using Eos.Nwn.Tlk;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eos.Repositories
{
    public class EosProject : RepositoryCollection
    {
        private string _projectFolder = "";
        private string _name = "";
        private bool _isLoaded = false;
        private bool _useNWNX = true;

        public EosProject() : base(false)
        {
        }

        public bool UseNWNX
        {
            get { return _useNWNX; }
            set { _useNWNX = value; }
        }

        public bool IsLoaded
        {
            get { return _isLoaded; }
            private set
            {
                if (value != _isLoaded)
                {
                    _isLoaded = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ProjectFolder
        {
            get { return _projectFolder; }
            set
            {
                _projectFolder = value;
                if (!Path.EndsInDirectorySeparator(_projectFolder))
                    _projectFolder = _projectFolder + Path.DirectorySeparatorChar;
            }
        }
        public TLKLanguage DefaultLanguage { get; set; } = TLKLanguage.English;

        public void CreateNew(string projectFolder, string name, TLKLanguage defaultLanguage)
        {
            Clear();

            ProjectFolder = projectFolder;
            Name = name;
            DefaultLanguage = defaultLanguage;
            Save();

            Load(ProjectFolder + Name + Constants.ProjectFileExtension);
        }

        private void LoadProjectFile(string projectFilename)
        {
            ProjectFolder = Path.GetDirectoryName(projectFilename) ?? "";

            var fs = new FileStream(projectFilename, FileMode.Open, FileAccess.Read);
            try
            {
                if (JsonNode.Parse(fs) is JsonObject projectJson)
                {
                    Name = projectJson["Name"]?.GetValue<String>() ?? "";
                    DefaultLanguage = Enum.Parse<TLKLanguage>(projectJson["DefaultLanguage"]?.GetValue<string>() ?? "");
                }
            }
            finally
            {
                fs.Close();
            }
        }

        private void SaveProjectFile(string projectFilename)
        {
            JsonObject projectFile = new JsonObject();
            projectFile.Add("Name", Name);
            projectFile.Add("DefaultLanguage", Enum.GetName(DefaultLanguage));
            File.WriteAllText(projectFilename, projectFile.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.General) { WriteIndented = true }));
        }

        public void Load(string projectFilename)
        {
            Clear();

            LoadProjectFile(projectFilename);

            CustomEnums.LoadFromFile(ProjectFolder + Constants.CustomEnumsFilename);
            CustomObjects.LoadFromFile(ProjectFolder + Constants.CustomObjectsFilename);

            CustomEnums.ResolveReferences();
            CustomObjects.ResolveReferences();

            foreach (var customObj in CustomObjects)
            {
                if (customObj != null)
                    CustomObjectRepositories.AddRepository(customObj);
            }

            Races.LoadFromFile(ProjectFolder + Constants.RacesFilename);
            Classes.LoadFromFile(ProjectFolder + Constants.ClassesFilename);
            Domains.LoadFromFile(ProjectFolder + Constants.DomainsFilename);
            Spells.LoadFromFile(ProjectFolder + Constants.SpellsFilename);
            Feats.LoadFromFile(ProjectFolder + Constants.FeatsFilename);
            Skills.LoadFromFile(ProjectFolder + Constants.SkillsFilename);
            Diseases.LoadFromFile(ProjectFolder + Constants.DiseasesFilename);
            Poisons.LoadFromFile(ProjectFolder + Constants.PoisonsFilename);
            Spellbooks.LoadFromFile(ProjectFolder + Constants.SpellbooksFilename);
            AreaEffects.LoadFromFile(ProjectFolder + Constants.AreaEffectsFilename);

            ClassPackages.LoadFromFile(ProjectFolder + Constants.ClassPackagesFilename);
            Soundsets.LoadFromFile(ProjectFolder + Constants.SoundsetsFilename);

            AttackBonusTables.LoadFromFile(ProjectFolder + Constants.AttackBonusTablesFilename);
            BonusFeatTables.LoadFromFile(ProjectFolder + Constants.BonusFeatTablesFilename);
            FeatTables.LoadFromFile(ProjectFolder + Constants.FeatTablesFilename);
            SavingThrowTables.LoadFromFile(ProjectFolder + Constants.SavingThrowTablesFilename);
            SkillTables.LoadFromFile(ProjectFolder + Constants.SkillTablesFilename);
            PrerequisiteTables.LoadFromFile(ProjectFolder + Constants.PrerequisiteTablesFilename);
            SpellSlotTables.LoadFromFile(ProjectFolder + Constants.SpellSlotTablesFilename);
            KnownSpellsTables.LoadFromFile(ProjectFolder + Constants.KnownSpellsTablesFilename);
            StatGainTables.LoadFromFile(ProjectFolder + Constants.StatGainTablesFilename);
            RacialFeatsTables.LoadFromFile(ProjectFolder + Constants.RacialFeatsTablesFilename);

            foreach (var template in CustomObjects)
            {
                if (template != null)
                    CustomObjectRepositories[template].LoadFromFile(ProjectFolder + template.ResourceName + ".json");
            }

            Races.ResolveReferences();
            Classes.ResolveReferences();
            Domains.ResolveReferences();
            Spells.ResolveReferences();
            Feats.ResolveReferences();
            Skills.ResolveReferences();
            Diseases.ResolveReferences();
            Poisons.ResolveReferences();
            Spellbooks.ResolveReferences();
            AreaEffects.ResolveReferences();

            Appearances.ResolveReferences();
            ClassPackages.ResolveReferences();
            Soundsets.ResolveReferences();

            AttackBonusTables.ResolveReferences();
            BonusFeatTables.ResolveReferences();
            FeatTables.ResolveReferences();
            SavingThrowTables.ResolveReferences();
            SkillTables.ResolveReferences();
            PrerequisiteTables.ResolveReferences();
            SpellSlotTables.ResolveReferences();
            KnownSpellsTables.ResolveReferences();
            StatGainTables.ResolveReferences();
            RacialFeatsTables.ResolveReferences();

            foreach (var template in CustomObjects)
            {
                if (template != null)
                    CustomObjectRepositories[template].ResolveReferences();
            }

            IsLoaded = true;
            EosConfig.LastProject = projectFilename;
            MasterRepository.LoadExternalResources(ProjectFolder + Constants.ExternalFilesPath);
        }

        public void Save()
        {
            SaveProjectFile(ProjectFolder + Name + Constants.ProjectFileExtension);

            Races.SaveToFile(ProjectFolder + Constants.RacesFilename);
            Classes.SaveToFile(ProjectFolder + Constants.ClassesFilename);
            Domains.SaveToFile(ProjectFolder + Constants.DomainsFilename);
            Spells.SaveToFile(ProjectFolder + Constants.SpellsFilename);
            Feats.SaveToFile(ProjectFolder + Constants.FeatsFilename);
            Skills.SaveToFile(ProjectFolder + Constants.SkillsFilename);
            Diseases.SaveToFile(ProjectFolder + Constants.DiseasesFilename);
            Poisons.SaveToFile(ProjectFolder + Constants.PoisonsFilename);
            Spellbooks.SaveToFile(ProjectFolder + Constants.SpellbooksFilename);
            AreaEffects.SaveToFile(ProjectFolder + Constants.AreaEffectsFilename);

            ClassPackages.SaveToFile(ProjectFolder + Constants.ClassPackagesFilename);
            Soundsets.SaveToFile(ProjectFolder + Constants.SoundsetsFilename);

            AttackBonusTables.SaveToFile(ProjectFolder + Constants.AttackBonusTablesFilename);
            BonusFeatTables.SaveToFile(ProjectFolder + Constants.BonusFeatTablesFilename);
            FeatTables.SaveToFile(ProjectFolder + Constants.FeatTablesFilename);
            SavingThrowTables.SaveToFile(ProjectFolder + Constants.SavingThrowTablesFilename);
            SkillTables.SaveToFile(ProjectFolder + Constants.SkillTablesFilename);
            PrerequisiteTables.SaveToFile(ProjectFolder + Constants.PrerequisiteTablesFilename);
            SpellSlotTables.SaveToFile(ProjectFolder + Constants.SpellSlotTablesFilename);
            KnownSpellsTables.SaveToFile(ProjectFolder + Constants.KnownSpellsTablesFilename);
            StatGainTables.SaveToFile(ProjectFolder + Constants.StatGainTablesFilename);
            RacialFeatsTables.SaveToFile(ProjectFolder + Constants.RacialFeatsTablesFilename);

            CustomEnums.SaveToFile(ProjectFolder + Constants.CustomEnumsFilename);
            CustomObjects.SaveToFile(ProjectFolder + Constants.CustomObjectsFilename);

            foreach (var template in CustomObjects)
            {
                if (template != null)
                    CustomObjectRepositories[template].SaveToFile(ProjectFolder + template.ResourceName + ".json");
            }
        }
    }
}
