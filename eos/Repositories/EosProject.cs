using Eos.Config;
using Eos.Models;
using Eos.Models.Tables;
using Eos.Nwn.Tlk;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ProjectSettingsCustomData Clone()
        {
            return (ProjectSettingsCustomData)MemberwiseClone();
        }
    }

    public class ProjectSettingsExport : INotifyPropertyChanged
    {
        private string _hakFolder = "";
        private string _2daFolder = "";
        private string _tlkFolder = "";
        private string _incFolder = "";
        private string _erfFolder = "";
        private string _baseTlk = "";
        private string _incFilename = "";

        public bool LowercaseFilenames { get; set; } = false;
        public string HakFolder
        {
            get { return _hakFolder; }
            set
            {
                if (_hakFolder != value)
                {
                    _hakFolder = value;
                    if (!_hakFolder.EndsWith(Path.DirectorySeparatorChar)) _hakFolder += Path.DirectorySeparatorChar;
                    NotifyPropertyChanged();
                }
            }
        }

        public string TwoDAFolder
        {
            get { return _2daFolder; }
            set
            {
                if (_2daFolder != value)
                {
                    _2daFolder = value;
                    if (!_2daFolder.EndsWith(Path.DirectorySeparatorChar)) _2daFolder += Path.DirectorySeparatorChar;
                    NotifyPropertyChanged();
                }
            }
        }

        public string TlkFolder
        {
            get { return _tlkFolder; }
            set
            {
                if (_tlkFolder != value)
                {
                    _tlkFolder = value;
                    if (!_tlkFolder.EndsWith(Path.DirectorySeparatorChar)) _tlkFolder += Path.DirectorySeparatorChar;
                    NotifyPropertyChanged();
                }
            }
        }

        public string IncludeFolder
        {
            get { return _incFolder; }
            set
            {
                if (_incFolder != value)
                {
                    _incFolder = value;
                    if (!_incFolder.EndsWith(Path.DirectorySeparatorChar)) _incFolder += Path.DirectorySeparatorChar;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ErfFolder
        {
            get { return _erfFolder; }
            set
            {
                if (_erfFolder != value)
                {
                    _erfFolder = value;
                    if (!_erfFolder.EndsWith(Path.DirectorySeparatorChar)) _erfFolder += Path.DirectorySeparatorChar;
                    NotifyPropertyChanged();
                }
            }
        }

        public string BaseTlkFile
        {
            get { return _baseTlk; }
            set
            {
                if (_baseTlk != value)
                {
                    _baseTlk = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int TlkOffset { get; set; } = 0;

        public string IncludeFilename
        {
            get { return _incFilename; }
            set
            {
                if (_incFilename != value)
                {
                    _incFilename = value;
                    if (_incFilename.EndsWith(".nss", StringComparison.OrdinalIgnoreCase))
                        _incFilename = _incFilename.Substring(0, _incFilename.Length - ".nss".Length);
                    _incFilename = _incFilename.Substring(0, Math.Min(16, _incFilename.Length));
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ProjectSettingsExport Clone()
        {
            return (ProjectSettingsExport)MemberwiseClone();
        }
    }

    public class ProjectSettings
    {
        public ObservableCollection<String> ExternalFolders { get; set; } = new ObservableCollection<String>();

        public ProjectSettingsExport Export { get; private set; } = new ProjectSettingsExport();

        public ProjectSettingsCustomData Appearances { get; private set; } = new ProjectSettingsCustomData(15100);
        public ProjectSettingsCustomData AreaEffects { get; private set; } = new ProjectSettingsCustomData(47);
        public ProjectSettingsCustomData Classes { get; private set; } = new ProjectSettingsCustomData(43);
        public ProjectSettingsCustomData Diseases { get; private set; } = new ProjectSettingsCustomData(17);
        public ProjectSettingsCustomData Domains { get; private set; } = new ProjectSettingsCustomData(22);
        public ProjectSettingsCustomData Feats { get; private set; } = new ProjectSettingsCustomData(1116);
        public ProjectSettingsCustomData MasterFeats { get; private set; } = new ProjectSettingsCustomData(18);
        public ProjectSettingsCustomData Packages { get; private set; } = new ProjectSettingsCustomData(132);
        public ProjectSettingsCustomData Poisons { get; private set; } = new ProjectSettingsCustomData(45);
        public ProjectSettingsCustomData Polymorphs { get; private set; } = new ProjectSettingsCustomData(107);
        public ProjectSettingsCustomData Portraits { get; private set; } = new ProjectSettingsCustomData(16001);
        public ProjectSettingsCustomData Races { get; private set; } = new ProjectSettingsCustomData(30);
        public ProjectSettingsCustomData Skills { get; private set; } = new ProjectSettingsCustomData(28);
        public ProjectSettingsCustomData Soundsets { get; private set; } = new ProjectSettingsCustomData(5100);
        public ProjectSettingsCustomData Spellbooks { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData Spells { get; private set; } = new ProjectSettingsCustomData(840);
        public ProjectSettingsCustomData VisualEffects { get; private set; } = new ProjectSettingsCustomData(10100);
        public ProjectSettingsCustomData AttackBonusTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData BonusFeatTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData FeatTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData KnownSpellsTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData PrerequisiteTables { get; private set; } = new ProjectSettingsCustomData(); 
        public ProjectSettingsCustomData RacialFeatsTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData SavesTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData SkillsTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData SpellSlotTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData StatGainTables { get; private set; } = new ProjectSettingsCustomData();

        public ProjectSettingsCustomData CustomData { get; private set; } = new ProjectSettingsCustomData();

        public ProjectSettings Clone()
        {
            ProjectSettings clone = (ProjectSettings)this.MemberwiseClone();
            clone.ExternalFolders = new ObservableCollection<String>(this.ExternalFolders);
            clone.Export = this.Export.Clone();

            clone.Appearances = this.Appearances.Clone();
            clone.AreaEffects = this.AreaEffects.Clone();
            clone.AttackBonusTables = this.AttackBonusTables.Clone();
            clone.BonusFeatTables = this.BonusFeatTables.Clone();
            clone.Classes = this.Classes.Clone();
            clone.CustomData = this.CustomData.Clone();
            clone.Diseases = this.Diseases.Clone();
            clone.Domains = this.Domains.Clone();
            clone.Feats = this.Feats.Clone();
            clone.FeatTables = this.FeatTables.Clone();
            clone.KnownSpellsTables = this.KnownSpellsTables.Clone();
            clone.MasterFeats = this.MasterFeats.Clone();
            clone.Packages = this.Packages.Clone();
            clone.Poisons = this.Poisons.Clone();
            clone.Polymorphs = this.Polymorphs.Clone();
            clone.Portraits = this.Portraits.Clone();
            clone.PrerequisiteTables = this.PrerequisiteTables.Clone();
            clone.Races = this.Races.Clone();
            clone.RacialFeatsTables = this.RacialFeatsTables.Clone();
            clone.SavesTables = this.SavesTables.Clone();
            clone.Skills = this.Skills.Clone();
            clone.SkillsTables = this.SkillsTables.Clone();
            clone.Soundsets = this.Soundsets.Clone();
            clone.Spellbooks = this.Spellbooks.Clone();
            clone.Spells = this.Spells.Clone();
            clone.SpellSlotTables = this.SpellSlotTables.Clone();
            clone.StatGainTables = this.StatGainTables.Clone();
            clone.VisualEffects = this.VisualEffects.Clone();

            return clone;
        }
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

        public ProjectSettings Settings { get; private set; } = new ProjectSettings();

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

            Settings = new ProjectSettings();
            Settings.Export.TwoDAFolder = Constants.Export2DAFolder;
            Settings.Export.HakFolder = Constants.ExportHAKFolder;
            Settings.Export.TlkFolder = Constants.ExportTLKFolder;
            Settings.Export.ErfFolder = Constants.ExportERFFolder;
            Settings.Export.IncludeFolder = Constants.ExportIncludeFolder;
            Settings.Export.IncludeFilename = "eos_inc";

            Settings.ExternalFolders.Add(Constants.ExternalFilesPath);

            Save();

            Load(ProjectFolder + Name + Constants.ProjectFileExtension);
        }

        private void LoadCustomDataSettings(ProjectSettingsCustomData settings, JsonNode? node)
        {
            settings.FormatJson = node?["FormatJson"]?.GetValue<bool>() ?? false;
            settings.Sorted = node?["Sorted"]?.GetValue<bool>() ?? false;
            settings.ExportOffset = node?["ExportOffset"]?.GetValue<int>() ?? settings.ExportOffset;
        }

        public void OverrideSettings(ProjectSettings settings)
        {
            this.Settings = settings.Clone();
        }

        private void LoadProjectFile(string projectFilename)
        {
            ProjectFolder = Path.GetDirectoryName(projectFilename) ?? "";

            Settings.ExternalFolders.Clear();

            Settings.Export.HakFolder = ProjectFolder + Constants.ExportHAKFolder;
            Settings.Export.TlkFolder = ProjectFolder + Constants.ExportTLKFolder;
            Settings.Export.TwoDAFolder = ProjectFolder + Constants.Export2DAFolder;
            Settings.Export.IncludeFolder = ProjectFolder + Constants.ExportIncludeFolder;
            Settings.Export.ErfFolder = ProjectFolder + Constants.ExportERFFolder;
            Settings.Export.IncludeFilename = Constants.IncludeFilename;

            var fs = new FileStream(projectFilename, FileMode.Open, FileAccess.Read);
            try
            {
                if (JsonNode.Parse(fs) is JsonObject projectJson)
                {
                    Name = projectJson["Name"]?.GetValue<String>() ?? "";
                    DefaultLanguage = Enum.Parse<TLKLanguage>(projectJson["DefaultLanguage"]?.GetValue<string>() ?? "");

                    var foldersJson = projectJson["ExternalFolders"]?.AsArray();
                    if (foldersJson != null)
                    { 
                        foreach (var folderJson in foldersJson)
                        {
                            if (folderJson == null) continue;
                            var folder = folderJson.GetValue<String>();
                            if (!folder.EndsWith(Path.DirectorySeparatorChar)) folder += Path.DirectorySeparatorChar;
                            Settings.ExternalFolders.Add(folder);
                        }
                    }

                    var exportJson = projectJson["Export"];
                    Settings.Export.LowercaseFilenames = exportJson?["LowercaseFilenames"]?.GetValue<bool>() ?? false;
                    Settings.Export.HakFolder = exportJson?["HakFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExportHAKFolder;
                    Settings.Export.ErfFolder = exportJson?["ErfFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExportERFFolder;
                    Settings.Export.TlkFolder = exportJson?["TlkFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExportTLKFolder;
                    Settings.Export.TwoDAFolder = exportJson?["TwoDAFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.Export2DAFolder;
                    Settings.Export.IncludeFolder = exportJson?["IncludeFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExportIncludeFolder;
                    Settings.Export.BaseTlkFile = exportJson?["BaseTlkFile"]?.GetValue<string>() ?? "";
                    Settings.Export.TlkOffset = exportJson?["TlkOffset"]?.GetValue<int>() ?? 0;
                    Settings.Export.IncludeFilename = exportJson?["IncludeFilename"]?.GetValue<string>() ?? Constants.IncludeFilename;

                    var customDataJson = projectJson["CustomData"];
                    LoadCustomDataSettings(Settings.Appearances, customDataJson?["AreaEffects"]);
                    LoadCustomDataSettings(Settings.AreaEffects, customDataJson?["AreaEffects"]);
                    LoadCustomDataSettings(Settings.Classes, customDataJson?["Classes"]);
                    LoadCustomDataSettings(Settings.Diseases, customDataJson?["Diseases"]);
                    LoadCustomDataSettings(Settings.Domains, customDataJson?["Domains"]);
                    LoadCustomDataSettings(Settings.Feats, customDataJson?["Feats"]);
                    LoadCustomDataSettings(Settings.MasterFeats, customDataJson?["MasterFeats"]);
                    LoadCustomDataSettings(Settings.Packages, customDataJson?["Packages"]);
                    LoadCustomDataSettings(Settings.Poisons, customDataJson?["Poisons"]);
                    LoadCustomDataSettings(Settings.Polymorphs, customDataJson?["Polymorphs"]);
                    LoadCustomDataSettings(Settings.Portraits, customDataJson?["Portraits"]);
                    LoadCustomDataSettings(Settings.Races, customDataJson?["Races"]);
                    LoadCustomDataSettings(Settings.Skills, customDataJson?["Skills"]);
                    LoadCustomDataSettings(Settings.Soundsets, customDataJson?["Soundsets"]);
                    LoadCustomDataSettings(Settings.Spellbooks, customDataJson?["Spellbooks"]);
                    LoadCustomDataSettings(Settings.Spells, customDataJson?["Spells"]);
                    LoadCustomDataSettings(Settings.VisualEffects, customDataJson?["VisualEffects"]);

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

            if (Settings.ExternalFolders.Count == 0)
                Settings.ExternalFolders.Add(ProjectFolder + Constants.ExternalFilesPath);

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

            var externalFolders = new JsonArray();
            foreach (var folder in Settings.ExternalFolders)
            {
                if (folder != null)
                    externalFolders.Add(folder);
            }
            projectFile.Add("ExternalFolders", externalFolders);

            var export = new JsonObject();
            export.Add("LowercaseFilenames", Settings.Export.LowercaseFilenames);
            export.Add("HakFolder", Settings.Export.HakFolder);
            export.Add("ErfFolder", Settings.Export.ErfFolder);
            export.Add("TwoDAFolder", Settings.Export.TwoDAFolder);
            export.Add("TlkFolder", Settings.Export.TlkFolder);
            export.Add("IncludeFolder", Settings.Export.IncludeFolder);
            export.Add("BaseTlkFile", Settings.Export.BaseTlkFile);
            export.Add("TlkOffset", Settings.Export.TlkOffset);
            export.Add("IncludeFilename", Settings.Export.IncludeFilename);
            projectFile.Add("Export", export);

            var customDataSettings = new JsonObject();
            SaveCustomDataSettings(Settings.Appearances, "Appearances", customDataSettings);
            SaveCustomDataSettings(Settings.AreaEffects, "AreaEffects", customDataSettings);
            SaveCustomDataSettings(Settings.Classes, "Classes", customDataSettings);
            SaveCustomDataSettings(Settings.Diseases, "Diseases", customDataSettings);
            SaveCustomDataSettings(Settings.Domains, "Domains", customDataSettings);
            SaveCustomDataSettings(Settings.Feats, "Feats", customDataSettings);
            SaveCustomDataSettings(Settings.MasterFeats, "MasterFeats", customDataSettings);
            SaveCustomDataSettings(Settings.Packages, "Packages", customDataSettings);
            SaveCustomDataSettings(Settings.Poisons, "Poisons", customDataSettings);
            SaveCustomDataSettings(Settings.Polymorphs, "Polymorphs", customDataSettings);
            SaveCustomDataSettings(Settings.Portraits, "Portraits", customDataSettings);
            SaveCustomDataSettings(Settings.Races, "Races", customDataSettings);
            SaveCustomDataSettings(Settings.Skills, "Skills", customDataSettings);
            SaveCustomDataSettings(Settings.Soundsets, "Soundsets", customDataSettings);
            SaveCustomDataSettings(Settings.Spellbooks, "Spellbooks", customDataSettings);
            SaveCustomDataSettings(Settings.Spells, "Spells", customDataSettings);
            SaveCustomDataSettings(Settings.VisualEffects, "VisualEffects", customDataSettings);

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

        public void Close()
        {
            IsLoaded = false;

            Name = "";
            ProjectFolder = "";

            Clear();

            EosConfig.LastProject = "";
            MasterRepository.LoadExternalResources(new string[] { });
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
            MasterFeats.LoadFromFile(ProjectFolder + Constants.MasterFeatsFilename);

            Appearances.LoadFromFile(ProjectFolder + Constants.AppearancesFilename);
            VisualEffects.LoadFromFile(ProjectFolder + Constants.VisualEffectsFilename);
            ClassPackages.LoadFromFile(ProjectFolder + Constants.ClassPackagesFilename);
            Soundsets.LoadFromFile(ProjectFolder + Constants.SoundsetsFilename);
            Polymorphs.LoadFromFile(ProjectFolder + Constants.PolymorphsFilename);
            Portraits.LoadFromFile(ProjectFolder + Constants.PortraitsFilename);

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
            MasterFeats.ResolveReferences();

            Appearances.ResolveReferences();
            VisualEffects.ResolveReferences();
            ClassPackages.ResolveReferences();
            Soundsets.ResolveReferences();
            Polymorphs.ResolveReferences();
            Portraits.ResolveReferences();

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

            if (Settings.Appearances.Sorted) Appearances.Sort(d => d?.Name);
            if (Settings.AreaEffects.Sorted) AreaEffects.Sort(d => d?.Name);
            if (Settings.AttackBonusTables.Sorted) AttackBonusTables.Sort(d => d?.Name);
            if (Settings.BonusFeatTables.Sorted) BonusFeatTables.Sort(d => d?.Name);
            if (Settings.Classes.Sorted) Classes.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Diseases.Sorted) Diseases.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Domains.Sorted) Domains.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Feats.Sorted) Feats.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.MasterFeats.Sorted) MasterFeats.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.FeatTables.Sorted) FeatTables.Sort(d => d?.Name);
            if (Settings.KnownSpellsTables.Sorted) KnownSpellsTables.Sort(d => d?.Name);
            if (Settings.Packages.Sorted) ClassPackages.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Poisons.Sorted) Poisons.Sort(d => d?.Name[TLKLanguage.English].Text);
            if (Settings.Polymorphs.Sorted) Polymorphs.Sort(d => d?.Name);
            if (Settings.Portraits.Sorted) Portraits.Sort(d => d?.ResRef);
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
            if (Settings.VisualEffects.Sorted) VisualEffects.Sort(d => d?.Name);

            IsLoaded = true;
            EosConfig.LastProject = projectFilename;
            MasterRepository.LoadExternalResources(Settings.ExternalFolders);
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
            MasterFeats.SaveToFile(ProjectFolder + Constants.MasterFeatsFilename, Settings.MasterFeats.FormatJson);

            Appearances.SaveToFile(ProjectFolder + Constants.AppearancesFilename, Settings.Appearances.FormatJson);
            Portraits.SaveToFile(ProjectFolder + Constants.PortraitsFilename, Settings.Portraits.FormatJson);
            VisualEffects.SaveToFile(ProjectFolder + Constants.VisualEffectsFilename, Settings.VisualEffects.FormatJson);
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
