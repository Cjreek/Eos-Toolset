using Eos.Config;
using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn.Tlk;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Eos.Repositories
{
    public class ProjectSettingsCustomData
    {
        public bool Sorted { get; set; } = false;
        public bool FormatJson { get; set; } = false;
        public int ExportOffset { get; set; }

        public ProjectSettingsCustomData(int exportOffset = -1)
        {
            ExportOffset = exportOffset;
        }
    }

    public class ProjectSettingsExport
    {
        public bool LowercaseFilenames { get; set; } = false;
        public string HakFolder { get; set; } = "";
        public string TwoDAFolder { get; set; } = "";
        public string TlkFolder { get; set; } = "";
        public string IncludeFolder { get; set; } = "";
        public string BaseTlkFile { get; set; } = "";
        public int TlkOffset { get; set; } = 0;
    }

    public class ProjectSettings
    {
        public string ExternalFolder { get; set; } = "";

        public ProjectSettingsExport Export { get; } = new ProjectSettingsExport();

        public ProjectSettingsCustomData AreaEffects { get; } = new ProjectSettingsCustomData(47);
        public ProjectSettingsCustomData Classes { get; } = new ProjectSettingsCustomData(43);
        public ProjectSettingsCustomData Diseases { get; } = new ProjectSettingsCustomData(17);
        public ProjectSettingsCustomData Domains { get; } = new ProjectSettingsCustomData(22);
        public ProjectSettingsCustomData Feats { get; } = new ProjectSettingsCustomData(1116);
        public ProjectSettingsCustomData Packages { get; } = new ProjectSettingsCustomData(132);
        public ProjectSettingsCustomData Poisons { get; } = new ProjectSettingsCustomData(45);
        public ProjectSettingsCustomData Polymorphs { get; } = new ProjectSettingsCustomData(107);
        public ProjectSettingsCustomData Races { get; } = new ProjectSettingsCustomData(30);
        public ProjectSettingsCustomData Skills { get; } = new ProjectSettingsCustomData(28);
        public ProjectSettingsCustomData Soundsets { get; } = new ProjectSettingsCustomData(5100);
        public ProjectSettingsCustomData Spellbooks { get; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData Spells { get; } = new ProjectSettingsCustomData(840);
        public ProjectSettingsCustomData AttackBonusTables { get; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData BonusFeatTables { get; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData FeatTables { get; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData KnownSpellsTables { get; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData PrerequisiteTables { get; } = new ProjectSettingsCustomData(); 
        public ProjectSettingsCustomData RacialFeatsTables { get; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData SavesTables { get; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData SkillsTables { get; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData SpellSlotTables { get; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData StatGainTables { get; } = new ProjectSettingsCustomData();

        public ProjectSettingsCustomData CustomData { get; } = new ProjectSettingsCustomData();
    }

    public class EosProject : RepositoryCollection
    {
        private string _projectFolder = "";
        private string _name = "";
        private bool _isLoaded = false;
        private bool _useNWNX = true;

        public EosProject() : base(false)
        {
        }

        public ProjectSettings Settings { get; } = new ProjectSettings();

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

        private void LoadCustomDataSettings(ProjectSettingsCustomData settings, JsonNode? node)
        {
            settings.FormatJson = node?["FormatJson"]?.GetValue<bool>() ?? false;
            settings.Sorted = node?["Sorted"]?.GetValue<bool>() ?? false;
            settings.ExportOffset = node?["ExportOffset"]?.GetValue<int>() ?? settings.ExportOffset;
        }

        private void LoadProjectFile(string projectFilename)
        {
            ProjectFolder = Path.GetDirectoryName(projectFilename) ?? "";

            Settings.ExternalFolder = ProjectFolder + Constants.ExternalFilesPath;
        
            Settings.Export.HakFolder = ProjectFolder + Constants.ExportHAKFolder;
            Settings.Export.TlkFolder = ProjectFolder + Constants.ExportTLKFolder;
            Settings.Export.TwoDAFolder = ProjectFolder + Constants.Export2DAFolder;
            Settings.Export.IncludeFolder = ProjectFolder + Constants.ExportIncludeFolder;

            var fs = new FileStream(projectFilename, FileMode.Open, FileAccess.Read);
            try
            {
                if (JsonNode.Parse(fs) is JsonObject projectJson)
                {
                    Name = projectJson["Name"]?.GetValue<String>() ?? "";
                    DefaultLanguage = Enum.Parse<TLKLanguage>(projectJson["DefaultLanguage"]?.GetValue<string>() ?? "");

                    Settings.ExternalFolder = projectJson["ExternalFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExternalFilesPath;

                    var exportJson = projectJson["Export"];
                    Settings.Export.LowercaseFilenames = exportJson?["LowercaseFilenames"]?.GetValue<bool>() ?? false;
                    Settings.Export.HakFolder = exportJson?["HakFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExportHAKFolder;
                    Settings.Export.TlkFolder = exportJson?["TlkFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExportTLKFolder;
                    Settings.Export.TwoDAFolder = exportJson?["TwoDAFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.Export2DAFolder;
                    Settings.Export.IncludeFolder = exportJson?["IncludeFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExportIncludeFolder;
                    Settings.Export.BaseTlkFile = exportJson?["BaseTlkFile"]?.GetValue<string>() ?? "";
                    Settings.Export.TlkOffset = exportJson?["TlkOffset"]?.GetValue<int>() ?? 0;

                    var customDataJson = projectJson["CustomData"];
                    LoadCustomDataSettings(Settings.AreaEffects, customDataJson?["AreaEffects"]);
                    LoadCustomDataSettings(Settings.Classes, customDataJson?["Classes"]);
                    LoadCustomDataSettings(Settings.Diseases, customDataJson?["Diseases"]);
                    LoadCustomDataSettings(Settings.Domains, customDataJson?["Domains"]);
                    LoadCustomDataSettings(Settings.Feats, customDataJson?["Feats"]);
                    LoadCustomDataSettings(Settings.Packages, customDataJson?["Packages"]);
                    LoadCustomDataSettings(Settings.Poisons, customDataJson?["Poisons"]);
                    LoadCustomDataSettings(Settings.Polymorphs, customDataJson?["Polymorphs"]);
                    LoadCustomDataSettings(Settings.Races, customDataJson?["Races"]);
                    LoadCustomDataSettings(Settings.Skills, customDataJson?["Skills"]);
                    LoadCustomDataSettings(Settings.Soundsets, customDataJson?["Soundsets"]);
                    LoadCustomDataSettings(Settings.Spellbooks, customDataJson?["Spellbooks"]);
                    LoadCustomDataSettings(Settings.Spells, customDataJson?["Spells"]);

                    LoadCustomDataSettings(Settings.AttackBonusTables, customDataJson?["AttackBonusTables"]);
                    LoadCustomDataSettings(Settings.BonusFeatTables, customDataJson?["BonusFeatTables"]);
                    LoadCustomDataSettings(Settings.FeatTables, customDataJson?["FeatTables"]);
                    LoadCustomDataSettings(Settings.KnownSpellsTables, customDataJson?["KnownSpellsTables"]);
                    LoadCustomDataSettings(Settings.PrerequisiteTables, customDataJson?["PrerequisiteTables"]);
                    LoadCustomDataSettings(Settings.RacialFeatsTables, customDataJson?["RacialFeatsTables"]);
                    LoadCustomDataSettings(Settings.SavesTables, customDataJson?["SavesTables"]);
                    LoadCustomDataSettings(Settings.SkillsTables, customDataJson?["SkillsTables"]);
                    LoadCustomDataSettings(Settings.SpellSlotTables, customDataJson?["SpellSlotTables"]);
                    LoadCustomDataSettings(Settings.StatGainTables, customDataJson?["StatGainTables"]);

                    LoadCustomDataSettings(Settings.CustomData, customDataJson?["Custom"]);
                }
            }
            finally
            {
                fs.Close();
            }

            if (!Settings.ExternalFolder.EndsWith(Path.DirectorySeparatorChar)) Settings.ExternalFolder += Path.DirectorySeparatorChar;
            if (!Settings.Export.HakFolder.EndsWith(Path.DirectorySeparatorChar)) Settings.Export.HakFolder += Path.DirectorySeparatorChar;
            if (!Settings.Export.TlkFolder.EndsWith(Path.DirectorySeparatorChar)) Settings.Export.TlkFolder += Path.DirectorySeparatorChar;
            if (!Settings.Export.TwoDAFolder.EndsWith(Path.DirectorySeparatorChar)) Settings.Export.TwoDAFolder += Path.DirectorySeparatorChar;
            if (!Settings.Export.IncludeFolder.EndsWith(Path.DirectorySeparatorChar)) Settings.Export.IncludeFolder += Path.DirectorySeparatorChar;

            Directory.SetCurrentDirectory(ProjectFolder);
        }

        private void SaveCustomDataSettings(ProjectSettingsCustomData settings, string nodeName, JsonObject rootNode)
        {
            var settingsNode = new JsonObject();
            settingsNode.Add("FormatJson", settings.FormatJson);
            settingsNode.Add("Sorted", settings.Sorted);
            settingsNode.Add("ExportOffset", settings.ExportOffset);
            rootNode.Add(nodeName, settingsNode);
        }

        private void SaveProjectFile(string projectFilename)
        {
            JsonObject projectFile = new JsonObject();
            projectFile.Add("FileFormatVersion", 1);
            projectFile.Add("Name", Name);
            projectFile.Add("DefaultLanguage", Enum.GetName(DefaultLanguage));
            projectFile.Add("ExternalFolder", Settings.ExternalFolder);

            var export = new JsonObject();
            export.Add("LowercaseFilenames", Settings.Export.LowercaseFilenames);
            export.Add("HakFolder", Settings.Export.HakFolder);
            export.Add("TwoDAFolder", Settings.Export.TwoDAFolder);
            export.Add("TlkFolder", Settings.Export.TlkFolder);
            export.Add("IncludeFolder", Settings.Export.IncludeFolder);
            export.Add("BaseTlkFile", Settings.Export.BaseTlkFile);
            export.Add("TlkOffset", Settings.Export.TlkOffset);
            projectFile.Add("Export", export);

            var customDataSettings = new JsonObject();
            SaveCustomDataSettings(Settings.AreaEffects, "AreaEffects", customDataSettings);
            SaveCustomDataSettings(Settings.Classes, "Classes", customDataSettings);
            SaveCustomDataSettings(Settings.Diseases, "Diseases", customDataSettings);
            SaveCustomDataSettings(Settings.Domains, "Domains", customDataSettings);
            SaveCustomDataSettings(Settings.Feats, "Feats", customDataSettings);
            SaveCustomDataSettings(Settings.Packages, "Packages", customDataSettings);
            SaveCustomDataSettings(Settings.Poisons, "Poisons", customDataSettings);
            SaveCustomDataSettings(Settings.Polymorphs, "Polymorphs", customDataSettings);
            SaveCustomDataSettings(Settings.Races, "Races", customDataSettings);
            SaveCustomDataSettings(Settings.Skills, "Skills", customDataSettings);
            SaveCustomDataSettings(Settings.Soundsets, "Soundsets", customDataSettings);
            SaveCustomDataSettings(Settings.Spellbooks, "Spellbooks", customDataSettings);
            SaveCustomDataSettings(Settings.Spells, "Spells", customDataSettings);

            SaveCustomDataSettings(Settings.BonusFeatTables, "BonusFeatTables", customDataSettings);
            SaveCustomDataSettings(Settings.AttackBonusTables, "AttackBonusTables", customDataSettings);
            SaveCustomDataSettings(Settings.FeatTables, "FeatTables", customDataSettings);
            SaveCustomDataSettings(Settings.KnownSpellsTables, "KnownSpellsTables", customDataSettings);
            SaveCustomDataSettings(Settings.PrerequisiteTables, "PrerequisiteTables", customDataSettings);
            SaveCustomDataSettings(Settings.RacialFeatsTables, "RacialFeatsTables", customDataSettings);
            SaveCustomDataSettings(Settings.SavesTables, "SavesTables", customDataSettings);
            SaveCustomDataSettings(Settings.SkillsTables, "SkillsTables", customDataSettings);
            SaveCustomDataSettings(Settings.SpellSlotTables, "SpellSlotTables", customDataSettings);
            SaveCustomDataSettings(Settings.StatGainTables, "StatGainTables", customDataSettings);

            SaveCustomDataSettings(Settings.CustomData, "Custom", customDataSettings);
            projectFile.Add("CustomData", customDataSettings);

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
            Polymorphs.LoadFromFile(ProjectFolder + Constants.PolymorphsFilename);

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
            Polymorphs.ResolveReferences();

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

            if (Settings.AreaEffects.Sorted) AreaEffects.Sort(d => d?.Name);
            if (Settings.AttackBonusTables.Sorted) AttackBonusTables.Sort(d => d?.Name);
            if (Settings.BonusFeatTables.Sorted) BonusFeatTables.Sort(d => d?.Name);
            if (Settings.Classes.Sorted) Classes.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Diseases.Sorted) Diseases.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Domains.Sorted) Domains.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Feats.Sorted) Feats.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.FeatTables.Sorted) FeatTables.Sort(d => d?.Name);
            if (Settings.KnownSpellsTables.Sorted) KnownSpellsTables.Sort(d => d?.Name);
            if (Settings.Packages.Sorted) ClassPackages.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Poisons.Sorted) Poisons.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Polymorphs.Sorted) Polymorphs.Sort(d => d?.Name);
            if (Settings.Races.Sorted) Races.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.RacialFeatsTables.Sorted) RacialFeatsTables.Sort(d => d?.Name);
            if (Settings.SavesTables.Sorted) SavingThrowTables.Sort(d => d?.Name);
            if (Settings.Skills.Sorted) Skills.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.SkillsTables.Sorted) SkillTables.Sort(d => d?.Name);
            if (Settings.Soundsets.Sorted) Soundsets.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Spellbooks.Sorted) Spellbooks.Sort(d => d?.Name);
            if (Settings.Spells.Sorted) Spells.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.SpellSlotTables.Sorted) SpellSlotTables.Sort(d => d?.Name);
            if (Settings.StatGainTables.Sorted) StatGainTables.Sort(d => d?.Name);            

            IsLoaded = true;
            EosConfig.LastProject = projectFilename;
            MasterRepository.LoadExternalResources(Settings.ExternalFolder);
        }

        public void Save()
        {
            SaveProjectFile(ProjectFolder + Name + Constants.ProjectFileExtension);

            if (!Directory.Exists(ProjectFolder + Constants.ExternalFilesPath))
                Directory.CreateDirectory(ProjectFolder + Constants.ExternalFilesPath);

            Races.SaveToFile(ProjectFolder + Constants.RacesFilename, Settings.Races.FormatJson);
            Classes.SaveToFile(ProjectFolder + Constants.ClassesFilename, Settings.Classes.FormatJson);
            Domains.SaveToFile(ProjectFolder + Constants.DomainsFilename, Settings.Domains.FormatJson);
            Spells.SaveToFile(ProjectFolder + Constants.SpellsFilename, Settings.Spells.FormatJson);
            Feats.SaveToFile(ProjectFolder + Constants.FeatsFilename, Settings.Feats.FormatJson);
            Skills.SaveToFile(ProjectFolder + Constants.SkillsFilename, Settings.Skills.FormatJson);
            Diseases.SaveToFile(ProjectFolder + Constants.DiseasesFilename, Settings.Diseases.FormatJson);
            Poisons.SaveToFile(ProjectFolder + Constants.PoisonsFilename, Settings.Poisons.FormatJson);
            Spellbooks.SaveToFile(ProjectFolder + Constants.SpellbooksFilename, Settings.Spellbooks.FormatJson);
            AreaEffects.SaveToFile(ProjectFolder + Constants.AreaEffectsFilename, Settings.AreaEffects.FormatJson);

            ClassPackages.SaveToFile(ProjectFolder + Constants.ClassPackagesFilename, Settings.Packages.FormatJson);
            Soundsets.SaveToFile(ProjectFolder + Constants.SoundsetsFilename, Settings.Soundsets.FormatJson);
            Polymorphs.SaveToFile(ProjectFolder + Constants.PolymorphsFilename, Settings.Polymorphs.FormatJson);

            AttackBonusTables.SaveToFile(ProjectFolder + Constants.AttackBonusTablesFilename, Settings.AttackBonusTables.FormatJson);
            BonusFeatTables.SaveToFile(ProjectFolder + Constants.BonusFeatTablesFilename, Settings.BonusFeatTables.FormatJson);
            FeatTables.SaveToFile(ProjectFolder + Constants.FeatTablesFilename, Settings.FeatTables.FormatJson);
            SavingThrowTables.SaveToFile(ProjectFolder + Constants.SavingThrowTablesFilename, Settings.SavesTables.FormatJson);
            SkillTables.SaveToFile(ProjectFolder + Constants.SkillTablesFilename, Settings.SkillsTables.FormatJson);
            PrerequisiteTables.SaveToFile(ProjectFolder + Constants.PrerequisiteTablesFilename, Settings.PrerequisiteTables.FormatJson);
            SpellSlotTables.SaveToFile(ProjectFolder + Constants.SpellSlotTablesFilename, Settings.SpellSlotTables.FormatJson);
            KnownSpellsTables.SaveToFile(ProjectFolder + Constants.KnownSpellsTablesFilename, Settings.KnownSpellsTables.FormatJson);
            StatGainTables.SaveToFile(ProjectFolder + Constants.StatGainTablesFilename, Settings.StatGainTables.FormatJson);
            RacialFeatsTables.SaveToFile(ProjectFolder + Constants.RacialFeatsTablesFilename, Settings.RacialFeatsTables.FormatJson);

            CustomEnums.SaveToFile(ProjectFolder + Constants.CustomEnumsFilename, Settings.CustomData.FormatJson);
            CustomObjects.SaveToFile(ProjectFolder + Constants.CustomObjectsFilename, Settings.CustomData.FormatJson);

            foreach (var template in CustomObjects)
            {
                if (template != null)
                    CustomObjectRepositories[template].SaveToFile(ProjectFolder + template.ResourceName + ".json", Settings.CustomData.FormatJson);
            }
        }
    }
}
