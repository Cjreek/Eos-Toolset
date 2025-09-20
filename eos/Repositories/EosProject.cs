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
using System.IO.Compression;
using Eos.Services;
using System.Text.Json.Serialization.Metadata;

namespace Eos.Repositories
{
    public class ProjectSettingsCustomData : INotifyPropertyChanged
    {
        private int _exportOffset;

        public bool Sorted { get; set; } = false;
        public bool FormatJson { get; set; } = true;

        public int ExportOffset
        {
            get { return _exportOffset; }
            set
            {
                if (_exportOffset !=  value)
                {
                    _exportOffset = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int DefaultExportOffset { get; }

        public ProjectSettingsCustomData(int exportOffset = -1)
        {
            DefaultExportOffset = exportOffset;
            ExportOffset = exportOffset;
        }

        public ProjectSettingsCustomData Clone()
        {
            return (ProjectSettingsCustomData)MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class ProjectSettingsExport : INotifyPropertyChanged
    {
        private string _hakFolder = "";
        private string _2daFolder = "";
        private string _ssfFolder = "";
        private string _tlkFolder = "";
        private string _incFolder = "";
        private string _erfFolder = "";
        private string _baseTlk = "";
        private string _incFilename = "";

        public bool Compress2DA { get; set; } = false;
        public int LabelMaxLength { get; set; } = -1;
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

        public string SsfFolder
        {
            get { return _ssfFolder; }
            set
            {
                if (_ssfFolder != value)
                {
                    _ssfFolder = value;
                    if (!_ssfFolder.EndsWith(Path.DirectorySeparatorChar)) _ssfFolder += Path.DirectorySeparatorChar;
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

    public class ProjectSettings : INotifyPropertyChanged
    {
        private string _backupFolder = "";
        public ObservableCollection<String> ExternalFolders { get; set; } = new ObservableCollection<String>();

        public ProjectSettingsExport Export { get; private set; } = new ProjectSettingsExport();

        public string BackupFolder
        {
            get { return _backupFolder; }
            set
            {
                if (_backupFolder != value)
                {
                    _backupFolder = value;
                    if (!_backupFolder.EndsWith(Path.DirectorySeparatorChar)) _backupFolder += Path.DirectorySeparatorChar;
                    NotifyPropertyChanged();
                }
            }
        }

        public ProjectSettingsCustomData Ammunitions { get; private set; } = new ProjectSettingsCustomData(36);
        public ProjectSettingsCustomData Appearances { get; private set; } = new ProjectSettingsCustomData(1000);
        public ProjectSettingsCustomData AppearanceSoundsets { get; private set; } = new ProjectSettingsCustomData(32);
        public ProjectSettingsCustomData AreaEffects { get; private set; } = new ProjectSettingsCustomData(47);
        public ProjectSettingsCustomData BaseItems { get; private set; } = new ProjectSettingsCustomData(113);
        public ProjectSettingsCustomData Classes { get; private set; } = new ProjectSettingsCustomData(43);
        public ProjectSettingsCustomData Companions { get; private set; } = new ProjectSettingsCustomData(9);
        public ProjectSettingsCustomData DamageTypes { get; private set; } = new ProjectSettingsCustomData(13);
        public ProjectSettingsCustomData DamageTypeGroups { get; private set; } = new ProjectSettingsCustomData(10);
        public ProjectSettingsCustomData Diseases { get; private set; } = new ProjectSettingsCustomData(17);
        public ProjectSettingsCustomData Domains { get; private set; } = new ProjectSettingsCustomData(22);
        public ProjectSettingsCustomData Familiars { get; private set; } = new ProjectSettingsCustomData(11);
        public ProjectSettingsCustomData Feats { get; private set; } = new ProjectSettingsCustomData(1116);
        public ProjectSettingsCustomData InventorySounds { get; private set; } = new ProjectSettingsCustomData(33);
        public ProjectSettingsCustomData ItemProperties { get; private set; } = new ProjectSettingsCustomData(88);
        public ProjectSettingsCustomData ItemPropertyParams { get; private set; } = new ProjectSettingsCustomData(12);
        public ProjectSettingsCustomData ItemPropertySets { get; private set; } = new ProjectSettingsCustomData(22);
        public ProjectSettingsCustomData MasterFeats { get; private set; } = new ProjectSettingsCustomData(18);
        public ProjectSettingsCustomData Packages { get; private set; } = new ProjectSettingsCustomData(132);
        public ProjectSettingsCustomData Poisons { get; private set; } = new ProjectSettingsCustomData(45);
        public ProjectSettingsCustomData Polymorphs { get; private set; } = new ProjectSettingsCustomData(107);
        public ProjectSettingsCustomData Portraits { get; private set; } = new ProjectSettingsCustomData(16001);
        public ProjectSettingsCustomData ProgrammedEffects { get; private set; } = new ProjectSettingsCustomData(1301);
        public ProjectSettingsCustomData Races { get; private set; } = new ProjectSettingsCustomData(30);
        public ProjectSettingsCustomData RangedDamageTypes { get; private set; } = new ProjectSettingsCustomData(8);
        public ProjectSettingsCustomData SavingthrowTypes { get; private set; } = new ProjectSettingsCustomData(21);
        public ProjectSettingsCustomData Skills { get; private set; } = new ProjectSettingsCustomData(28);
        public ProjectSettingsCustomData Soundsets { get; private set; } = new ProjectSettingsCustomData(5100);
        public ProjectSettingsCustomData Spellbooks { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData Spells { get; private set; } = new ProjectSettingsCustomData(840);
        public ProjectSettingsCustomData Traps { get; private set; } = new ProjectSettingsCustomData(48);
        public ProjectSettingsCustomData VisualEffects { get; private set; } = new ProjectSettingsCustomData(10100);
        public ProjectSettingsCustomData WeaponSounds { get; private set; } = new ProjectSettingsCustomData(22);

        public ProjectSettingsCustomData AttackBonusTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData BonusFeatTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData FeatTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData ItemPropertyCostTables { get; private set; } = new ProjectSettingsCustomData(31);
        public ProjectSettingsCustomData ItemPropertyTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData KnownSpellsTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData PackageEquipmentTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData PackageFeatPreferencesTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData PackageSkillPreferencesTables { get; private set; } = new ProjectSettingsCustomData();
        public ProjectSettingsCustomData PackageSpellPreferencesTables { get; private set; } = new ProjectSettingsCustomData();
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
            clone.BackupFolder = this.BackupFolder;

            clone.Export = this.Export.Clone();

            clone.Ammunitions = this.Ammunitions.Clone();
            clone.Appearances = this.Appearances.Clone();
            clone.AppearanceSoundsets = this.AppearanceSoundsets.Clone();
            clone.WeaponSounds = this.WeaponSounds.Clone();
            clone.AreaEffects = this.AreaEffects.Clone();
            clone.AttackBonusTables = this.AttackBonusTables.Clone();
            clone.BaseItems = this.BaseItems.Clone();
            clone.BonusFeatTables = this.BonusFeatTables.Clone();
            clone.Classes = this.Classes.Clone();
            clone.Companions = this.Companions.Clone();
            clone.CustomData = this.CustomData.Clone();
            clone.DamageTypes = this.DamageTypes.Clone();
            clone.DamageTypeGroups = this.DamageTypeGroups.Clone();
            clone.Diseases = this.Diseases.Clone();
            clone.Domains = this.Domains.Clone();
            clone.Familiars = this.Familiars.Clone();
            clone.Feats = this.Feats.Clone();
            clone.InventorySounds = this.InventorySounds.Clone();
            clone.ItemProperties = this.ItemProperties.Clone();
            clone.ItemPropertySets = this.ItemPropertySets.Clone();
            clone.ItemPropertyTables = this.ItemPropertyTables.Clone();
            clone.ItemPropertyCostTables = this.ItemPropertyCostTables.Clone();
            clone.ItemPropertyParams = this.ItemPropertyParams.Clone();
            clone.FeatTables = this.FeatTables.Clone();
            clone.KnownSpellsTables = this.KnownSpellsTables.Clone();
            clone.MasterFeats = this.MasterFeats.Clone();
            clone.Packages = this.Packages.Clone();
            clone.Poisons = this.Poisons.Clone();
            clone.Polymorphs = this.Polymorphs.Clone();
            clone.Portraits = this.Portraits.Clone();
            clone.ProgrammedEffects = this.ProgrammedEffects.Clone();
            clone.PrerequisiteTables = this.PrerequisiteTables.Clone();
            clone.Races = this.Races.Clone();
            clone.RangedDamageTypes = this.RangedDamageTypes.Clone();
            clone.RacialFeatsTables = this.RacialFeatsTables.Clone();
            clone.SavesTables = this.SavesTables.Clone();
            clone.SavingthrowTypes = this.SavingthrowTypes.Clone();
            clone.Skills = this.Skills.Clone();
            clone.SkillsTables = this.SkillsTables.Clone();
            clone.Soundsets = this.Soundsets.Clone();
            clone.Spellbooks = this.Spellbooks.Clone();
            clone.Spells = this.Spells.Clone();
            clone.Traps = this.Traps.Clone();
            clone.SpellSlotTables = this.SpellSlotTables.Clone();
            clone.StatGainTables = this.StatGainTables.Clone();
            clone.PackageSpellPreferencesTables = this.PackageSpellPreferencesTables.Clone();
            clone.PackageFeatPreferencesTables = this.PackageFeatPreferencesTables.Clone();
            clone.PackageSkillPreferencesTables = this.PackageSkillPreferencesTables.Clone();
            clone.PackageEquipmentTables = this.PackageEquipmentTables.Clone();
            clone.VisualEffects = this.VisualEffects.Clone();

            return clone;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class EosProject : RepositoryCollection
    {
        private string _projectFilename = "";
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

        public TlkStringTable CustomTlkStrings { get; } = new TlkStringTable();

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
        public int Version { get; set; } = 0;
        
        public void CreateNew(string projectFolder, string name, TLKLanguage defaultLanguage)
        {
            Clear();

            ProjectFolder = projectFolder;
            Name = name;
            DefaultLanguage = defaultLanguage;
            Version = ProjectUpdateService.GetHighestSupportedVersion();

            Settings.BackupFolder = Constants.BackupFolder;

            Settings = new ProjectSettings();
            Settings.Export.TwoDAFolder = Constants.Export2DAFolder;
            Settings.Export.SsfFolder = Constants.ExportSSFFolder;
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
            _projectFilename = projectFilename;
            ProjectFolder = Path.GetDirectoryName(projectFilename) ?? "";

            Settings.ExternalFolders.Clear();

            Settings.BackupFolder = Constants.BackupFolder;

            Settings.Export.HakFolder = Constants.ExportHAKFolder;
            Settings.Export.TlkFolder = Constants.ExportTLKFolder;
            Settings.Export.TwoDAFolder = Constants.Export2DAFolder;
            Settings.Export.IncludeFolder = Constants.ExportIncludeFolder;
            Settings.Export.ErfFolder = Constants.ExportERFFolder;
            Settings.Export.IncludeFilename = Constants.IncludeFilename;
            try
            {
                var fs = new FileStream(projectFilename, FileMode.Open, FileAccess.Read);
                try
                {
                    if (JsonNode.Parse(fs) is JsonObject projectJson)
                    {
                        Version = projectJson["Version"]?.GetValue<int>() ?? 0;
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

                        Settings.BackupFolder = projectJson["BackupFolder"]?.GetValue<string>() ?? Constants.BackupFolder;

                        var exportJson = projectJson["Export"];
                        Settings.Export.LowercaseFilenames = exportJson?["LowercaseFilenames"]?.GetValue<bool>() ?? false;
                        Settings.Export.HakFolder = exportJson?["HakFolder"]?.GetValue<string>() ?? Constants.ExportHAKFolder;
                        Settings.Export.ErfFolder = exportJson?["ErfFolder"]?.GetValue<string>() ?? Constants.ExportERFFolder;
                        Settings.Export.TlkFolder = exportJson?["TlkFolder"]?.GetValue<string>() ?? Constants.ExportTLKFolder;
                        Settings.Export.TwoDAFolder = exportJson?["TwoDAFolder"]?.GetValue<string>() ?? Constants.Export2DAFolder;
                        Settings.Export.SsfFolder = exportJson?["SsfFolder"]?.GetValue<string>() ?? Constants.ExportSSFFolder;
                        Settings.Export.IncludeFolder = exportJson?["IncludeFolder"]?.GetValue<string>() ?? Constants.ExportIncludeFolder;
                        Settings.Export.BaseTlkFile = exportJson?["BaseTlkFile"]?.GetValue<string>() ?? "";
                        Settings.Export.TlkOffset = exportJson?["TlkOffset"]?.GetValue<int>() ?? 0;
                        Settings.Export.IncludeFilename = exportJson?["IncludeFilename"]?.GetValue<string>() ?? Constants.IncludeFilename;
                        Settings.Export.Compress2DA = exportJson?["Compress2DA"]?.GetValue<bool>() ?? false;
                        Settings.Export.LabelMaxLength = exportJson?["LabelMaxLength"]?.GetValue<int>() ?? -1;

                        var customDataJson = projectJson["CustomData"];
                        LoadCustomDataSettings(Settings.Ammunitions, customDataJson?["Ammunitions"]);
                        LoadCustomDataSettings(Settings.Appearances, customDataJson?["Appearances"]);
                        LoadCustomDataSettings(Settings.AppearanceSoundsets, customDataJson?["AppearanceSoundsets"]);
                        LoadCustomDataSettings(Settings.AreaEffects, customDataJson?["AreaEffects"]);
                        LoadCustomDataSettings(Settings.BaseItems, customDataJson?["BaseItems"]);
                        LoadCustomDataSettings(Settings.Classes, customDataJson?["Classes"]);
                        LoadCustomDataSettings(Settings.Companions, customDataJson?["Companions"]);
                        LoadCustomDataSettings(Settings.DamageTypes, customDataJson?["DamageTypes"]);
                        LoadCustomDataSettings(Settings.DamageTypeGroups, customDataJson?["DamageTypeGroups"]);
                        LoadCustomDataSettings(Settings.Diseases, customDataJson?["Diseases"]);
                        LoadCustomDataSettings(Settings.Domains, customDataJson?["Domains"]);
                        LoadCustomDataSettings(Settings.Familiars, customDataJson?["Familiars"]);
                        LoadCustomDataSettings(Settings.Feats, customDataJson?["Feats"]);
                        LoadCustomDataSettings(Settings.InventorySounds, customDataJson?["InventorySounds"]);
                        LoadCustomDataSettings(Settings.ItemPropertySets, customDataJson?["ItemPropertySets"]);
                        LoadCustomDataSettings(Settings.ItemProperties, customDataJson?["ItemProperties"]);
                        LoadCustomDataSettings(Settings.MasterFeats, customDataJson?["MasterFeats"]);
                        LoadCustomDataSettings(Settings.Packages, customDataJson?["Packages"]);
                        LoadCustomDataSettings(Settings.Poisons, customDataJson?["Poisons"]);
                        LoadCustomDataSettings(Settings.Polymorphs, customDataJson?["Polymorphs"]);
                        LoadCustomDataSettings(Settings.Portraits, customDataJson?["Portraits"]);
                        LoadCustomDataSettings(Settings.ProgrammedEffects, customDataJson?["ProgrammedEffects"]);
                        LoadCustomDataSettings(Settings.Races, customDataJson?["Races"]);
                        LoadCustomDataSettings(Settings.RangedDamageTypes, customDataJson?["RangedDamageTypes"]);
                        LoadCustomDataSettings(Settings.SavingthrowTypes, customDataJson?["SavingThrowTypes"]);
                        LoadCustomDataSettings(Settings.Skills, customDataJson?["Skills"]);
                        LoadCustomDataSettings(Settings.Soundsets, customDataJson?["Soundsets"]);
                        LoadCustomDataSettings(Settings.Spellbooks, customDataJson?["Spellbooks"]);
                        LoadCustomDataSettings(Settings.Spells, customDataJson?["Spells"]);
                        LoadCustomDataSettings(Settings.Traps, customDataJson?["Traps"]);
                        LoadCustomDataSettings(Settings.VisualEffects, customDataJson?["VisualEffects"]);
                        LoadCustomDataSettings(Settings.WeaponSounds, customDataJson?["WeaponSounds"]);

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
                        LoadCustomDataSettings(Settings.PackageSpellPreferencesTables, customDataJson?["PackageSpellPreferencesTables"]);
                        LoadCustomDataSettings(Settings.PackageFeatPreferencesTables, customDataJson?["PackageFeatPreferencesTables"]);
                        LoadCustomDataSettings(Settings.PackageSkillPreferencesTables, customDataJson?["PackageSkillPreferencesTables"]);

                        LoadCustomDataSettings(Settings.ItemPropertyTables, customDataJson?["ItemPropertyTables"]);
                        LoadCustomDataSettings(Settings.ItemPropertyCostTables, customDataJson?["ItemPropertyCostTables"]);
                        LoadCustomDataSettings(Settings.ItemPropertyParams, customDataJson?["ItemPropertyParams"]);

                        LoadCustomDataSettings(Settings.CustomData, customDataJson?["Custom"]);
                    }
                }
                finally
                {
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }

            if (Settings.ExternalFolders.Count == 0)
                Settings.ExternalFolders.Add(Constants.ExternalFilesPath);

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
            try
            {
                JsonObject projectFile = new JsonObject();
                projectFile.Add("Version", Version);
                projectFile.Add("Name", Name);
                projectFile.Add("DefaultLanguage", Enum.GetName(DefaultLanguage));

                var externalFolders = new JsonArray();
                foreach (var folder in Settings.ExternalFolders)
                {
                    if (folder != null)
                        externalFolders.Add(folder);
                }
                projectFile.Add("ExternalFolders", externalFolders);

                projectFile.Add("BackupFolder", Settings.BackupFolder);

                var export = new JsonObject();
                export.Add("LowercaseFilenames", Settings.Export.LowercaseFilenames);
                export.Add("HakFolder", Settings.Export.HakFolder);
                export.Add("ErfFolder", Settings.Export.ErfFolder);
                export.Add("TwoDAFolder", Settings.Export.TwoDAFolder);
                export.Add("SsfFolder", Settings.Export.SsfFolder);
                export.Add("TlkFolder", Settings.Export.TlkFolder);
                export.Add("IncludeFolder", Settings.Export.IncludeFolder);
                export.Add("BaseTlkFile", Settings.Export.BaseTlkFile);
                export.Add("TlkOffset", Settings.Export.TlkOffset);
                export.Add("IncludeFilename", Settings.Export.IncludeFilename);
                export.Add("Compress2DA", Settings.Export.Compress2DA);
                export.Add("LabelMaxLength", Settings.Export.LabelMaxLength);
                projectFile.Add("Export", export);

                var customDataSettings = new JsonObject();
                SaveCustomDataSettings(Settings.Ammunitions, "Ammunitions", customDataSettings);
                SaveCustomDataSettings(Settings.Appearances, "Appearances", customDataSettings);
                SaveCustomDataSettings(Settings.AppearanceSoundsets, "AppearanceSoundsets", customDataSettings);
                SaveCustomDataSettings(Settings.AreaEffects, "AreaEffects", customDataSettings);
                SaveCustomDataSettings(Settings.BaseItems, "BaseItems", customDataSettings);
                SaveCustomDataSettings(Settings.Classes, "Classes", customDataSettings);
                SaveCustomDataSettings(Settings.Companions, "Companions", customDataSettings);
                SaveCustomDataSettings(Settings.DamageTypes, "DamageTypes", customDataSettings);
                SaveCustomDataSettings(Settings.DamageTypeGroups, "DamageTypeGroups", customDataSettings);
                SaveCustomDataSettings(Settings.Diseases, "Diseases", customDataSettings);
                SaveCustomDataSettings(Settings.Domains, "Domains", customDataSettings);
                SaveCustomDataSettings(Settings.Familiars, "Familiars", customDataSettings);
                SaveCustomDataSettings(Settings.Feats, "Feats", customDataSettings);
                SaveCustomDataSettings(Settings.InventorySounds, "InventorySounds", customDataSettings);
                SaveCustomDataSettings(Settings.ItemPropertySets, "ItemPropertySets", customDataSettings);
                SaveCustomDataSettings(Settings.ItemProperties, "ItemProperties", customDataSettings);
                SaveCustomDataSettings(Settings.MasterFeats, "MasterFeats", customDataSettings);
                SaveCustomDataSettings(Settings.Packages, "Packages", customDataSettings);
                SaveCustomDataSettings(Settings.Poisons, "Poisons", customDataSettings);
                SaveCustomDataSettings(Settings.Polymorphs, "Polymorphs", customDataSettings);
                SaveCustomDataSettings(Settings.Portraits, "Portraits", customDataSettings);
                SaveCustomDataSettings(Settings.ProgrammedEffects, "ProgrammedEffects", customDataSettings);
                SaveCustomDataSettings(Settings.Races, "Races", customDataSettings);
                SaveCustomDataSettings(Settings.RangedDamageTypes, "RangedDamageTypes", customDataSettings);
                SaveCustomDataSettings(Settings.SavingthrowTypes, "SavingThrowTypes", customDataSettings);
                SaveCustomDataSettings(Settings.Skills, "Skills", customDataSettings);
                SaveCustomDataSettings(Settings.Soundsets, "Soundsets", customDataSettings);
                SaveCustomDataSettings(Settings.Spellbooks, "Spellbooks", customDataSettings);
                SaveCustomDataSettings(Settings.Spells, "Spells", customDataSettings);
                SaveCustomDataSettings(Settings.Traps, "Traps", customDataSettings);
                SaveCustomDataSettings(Settings.VisualEffects, "VisualEffects", customDataSettings);
                SaveCustomDataSettings(Settings.WeaponSounds, "WeaponSounds", customDataSettings);

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
                SaveCustomDataSettings(Settings.PackageSpellPreferencesTables, "PackageSpellPreferencesTables", customDataSettings);
                SaveCustomDataSettings(Settings.PackageFeatPreferencesTables, "PackageFeatPreferencesTables", customDataSettings);
                SaveCustomDataSettings(Settings.PackageSkillPreferencesTables, "PackageSkillPreferencesTables", customDataSettings);

                SaveCustomDataSettings(Settings.ItemPropertyTables, "ItemPropertyTables", customDataSettings);
                SaveCustomDataSettings(Settings.ItemPropertyCostTables, "ItemPropertyCostTables", customDataSettings);
                SaveCustomDataSettings(Settings.ItemPropertyParams, "ItemPropertyParams", customDataSettings);

                SaveCustomDataSettings(Settings.CustomData, "Custom", customDataSettings);
                projectFile.Add("CustomData", customDataSettings);

                var serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.General) { WriteIndented = true };
                serializationOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                File.WriteAllText(projectFilename, projectFile.ToJsonString(serializationOptions));
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        public void Close()
        {
            IsLoaded = false;

            Version = 0;
            Name = "";
            ProjectFolder = "";

            Clear();

            EosConfig.LastProject = "";
            MasterRepository.LoadExternalResources(new string[] { });
        }

        private void AddZipEntry(ZipArchive zip, string filename, string entryName)
        {
            if (File.Exists(filename))
                zip.CreateEntryFromFile(filename, entryName);
        }

        public void CreateBackup()
        {
            try
            {
                var backupFilename = $"{Settings.BackupFolder}{Name}_v{Version}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.zip";
                Log.Info("Creating project backup: \"{0}\"", Path.GetFullPath(backupFilename));

                Directory.CreateDirectory(Path.GetDirectoryName(backupFilename) ?? "");
                using (var backupFileStream = new FileStream(ProjectFolder + backupFilename, FileMode.CreateNew))
                {
                    using (var zip = new ZipArchive(backupFileStream, ZipArchiveMode.Create))
                    {
                        AddZipEntry(zip, _projectFilename, Path.GetFileName(_projectFilename));

                        AddZipEntry(zip, ProjectFolder + Constants.RacesFilename, Constants.RacesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.ClassesFilename, Constants.ClassesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.DomainsFilename, Constants.DomainsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SpellsFilename, Constants.SpellsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.FeatsFilename, Constants.FeatsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SkillsFilename, Constants.SkillsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.DiseasesFilename, Constants.DiseasesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.PoisonsFilename, Constants.PoisonsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SpellbooksFilename, Constants.SpellbooksFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.AreaEffectsFilename, Constants.AreaEffectsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.MasterFeatsFilename, Constants.MasterFeatsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.BaseItemsFilename, Constants.BaseItemsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.ItemPropertySetsFilename, Constants.ItemPropertySetsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.ItemPropertiesFilename, Constants.ItemPropertiesFilename);

                        AddZipEntry(zip, ProjectFolder + Constants.AmmunitionsFilename, Constants.AmmunitionsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.AppearancesFilename, Constants.AppearancesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.AppearanceSoundsetsFilename, Constants.AppearanceSoundsetsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.WeaponSoundsFilename, Constants.WeaponSoundsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.InventorySoundsFilename, Constants.InventorySoundsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.PortraitsFilename, Constants.PortraitsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.VisualEffectsFilename, Constants.VisualEffectsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.ClassPackagesFilename, Constants.ClassPackagesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SoundsetsFilename, Constants.SoundsetsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.PolymorphsFilename, Constants.PolymorphsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.CompanionsFilename, Constants.CompanionsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.FamiliarsFilename, Constants.FamiliarsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.TrapsFilename, Constants.TrapsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.ProgrammedEffectsFilename, Constants.ProgrammedEffectsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.DamageTypesFilename, Constants.DamageTypesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.DamageTypeGroupsFilename, Constants.DamageTypeGroupsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.RangedDamageTypesFilename, Constants.RangedDamageTypesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SavingThrowTypesFilename, Constants.SavingThrowTypesFilename);

                        AddZipEntry(zip, ProjectFolder + Constants.AttackBonusTablesFilename, Constants.AttackBonusTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.BonusFeatTablesFilename, Constants.BonusFeatTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.FeatTablesFilename, Constants.FeatTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SavingThrowTablesFilename, Constants.SavingThrowTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SkillTablesFilename, Constants.SkillTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.PrerequisiteTablesFilename, Constants.PrerequisiteTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SpellSlotTablesFilename, Constants.SpellSlotTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.KnownSpellsTablesFilename, Constants.KnownSpellsTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.StatGainTablesFilename, Constants.StatGainTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.RacialFeatsTablesFilename, Constants.RacialFeatsTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SpellPreferencesTablesFilename, Constants.SpellPreferencesTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.FeatPreferencesTablesFilename, Constants.FeatPreferencesTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.SkillPreferencesTablesFilename, Constants.SkillPreferencesTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.PackageEquipmentTablesFilename, Constants.PackageEquipmentTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.ItemPropertyTablesFilename, Constants.ItemPropertyTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.ItemPropertyCostTablesFilename, Constants.ItemPropertyCostTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.ItemPropertyParamsFilename, Constants.ItemPropertyParamsFilename);

                        AddZipEntry(zip, ProjectFolder + Constants.CustomEnumsFilename, Constants.CustomEnumsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.CustomObjectsFilename, Constants.CustomObjectsFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.CustomTablesFilename, Constants.CustomTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.CustomDynamicTablesFilename, Constants.CustomDynamicTablesFilename);
                        AddZipEntry(zip, ProjectFolder + Constants.CustomTlkStringsFilename, Constants.CustomTlkStringsFilename);

                        foreach (var template in CustomObjects)
                        {
                            if (template != null)
                                AddZipEntry(zip, ProjectFolder + template.ResourceName + ".json", template.ResourceName + ".json");
                        }

                        foreach (var template in CustomTables)
                        {
                            if (template != null)
                                AddZipEntry(zip, ProjectFolder + template.FileName + ".json", template.FileName + ".json");
                        }

                        foreach (var template in CustomDynamicTables)
                        {
                            if (template != null)
                            {
                                AddZipEntry(zip, ProjectFolder + template.FileName + ".json", template.FileName + ".json");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error creating project backup: {0}", e.Message);
            }
        }

        private void LoadCustomTlkStrings(String filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    Log.Info("Loading \"{0}\"", filename);
                    var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    try
                    {
                        if (JsonNode.Parse(fs) is JsonObject tlkStringTable)
                        {
                            CustomTlkStrings.Clear();
                            CustomTlkStrings.FromJson(tlkStringTable);
                        }
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
        }

        public void Load(string projectFilename)
        {
            Log.Info("Loading project file: \"{0}\"", projectFilename);

            Clear();

            LoadProjectFile(projectFilename);
            Log.Info("Project version: {0}", Version);

            Log.Info("Loading project data...");
            try
            {
                CustomEnums.LoadFromFile(ProjectFolder + Constants.CustomEnumsFilename);
                CustomObjects.LoadFromFile(ProjectFolder + Constants.CustomObjectsFilename);
                CustomTables.LoadFromFile(ProjectFolder + Constants.CustomTablesFilename);
                CustomDynamicTables.LoadFromFile(ProjectFolder + Constants.CustomDynamicTablesFilename);

                CustomEnums.ResolveReferences();
                CustomObjects.ResolveReferences();
                CustomTables.ResolveReferences();
                CustomDynamicTables.ResolveReferences();

                foreach (var customObj in CustomObjects)
                {
                    if (customObj != null)
                        CustomObjectRepositories.AddRepository(customObj);
                }

                foreach (var customTable in CustomTables)
                {
                    if (customTable != null)
                        CustomTableRepositories.AddRepository(customTable);
                }

                foreach (var customDynTable in CustomDynamicTables)
                {
                    if (customDynTable != null)
                        CustomDynamicTableRepositories.AddRepository(customDynTable);
                }

                Races.LoadFromFile(ProjectFolder + Constants.RacesFilename);
                Classes.LoadFromFile(ProjectFolder + Constants.ClassesFilename);
                ClassPackages.LoadFromFile(ProjectFolder + Constants.ClassPackagesFilename);
                Domains.LoadFromFile(ProjectFolder + Constants.DomainsFilename);
                Spells.LoadFromFile(ProjectFolder + Constants.SpellsFilename);
                Feats.LoadFromFile(ProjectFolder + Constants.FeatsFilename);
                Skills.LoadFromFile(ProjectFolder + Constants.SkillsFilename);
                Diseases.LoadFromFile(ProjectFolder + Constants.DiseasesFilename);
                Poisons.LoadFromFile(ProjectFolder + Constants.PoisonsFilename);
                Spellbooks.LoadFromFile(ProjectFolder + Constants.SpellbooksFilename);
                AreaEffects.LoadFromFile(ProjectFolder + Constants.AreaEffectsFilename);
                MasterFeats.LoadFromFile(ProjectFolder + Constants.MasterFeatsFilename);
                BaseItems.LoadFromFile(ProjectFolder + Constants.BaseItemsFilename);
                ItemPropertySets.LoadFromFile(ProjectFolder + Constants.ItemPropertySetsFilename);
                ItemProperties.LoadFromFile(ProjectFolder + Constants.ItemPropertiesFilename);

                Ammunitions.LoadFromFile(ProjectFolder + Constants.AmmunitionsFilename);
                Appearances.LoadFromFile(ProjectFolder + Constants.AppearancesFilename);
                AppearanceSoundsets.LoadFromFile(ProjectFolder + Constants.AppearanceSoundsetsFilename);
                WeaponSounds.LoadFromFile(ProjectFolder + Constants.WeaponSoundsFilename);
                InventorySounds.LoadFromFile(ProjectFolder + Constants.InventorySoundsFilename);
                VisualEffects.LoadFromFile(ProjectFolder + Constants.VisualEffectsFilename);
                Soundsets.LoadFromFile(ProjectFolder + Constants.SoundsetsFilename);
                Polymorphs.LoadFromFile(ProjectFolder + Constants.PolymorphsFilename);
                Portraits.LoadFromFile(ProjectFolder + Constants.PortraitsFilename);
                Companions.LoadFromFile(ProjectFolder + Constants.CompanionsFilename);
                Familiars.LoadFromFile(ProjectFolder + Constants.FamiliarsFilename);
                Traps.LoadFromFile(ProjectFolder + Constants.TrapsFilename);
                ProgrammedEffects.LoadFromFile(ProjectFolder + Constants.ProgrammedEffectsFilename);
                DamageTypes.LoadFromFile(ProjectFolder + Constants.DamageTypesFilename);
                DamageTypeGroups.LoadFromFile(ProjectFolder + Constants.DamageTypeGroupsFilename);
                RangedDamageTypes.LoadFromFile(ProjectFolder + Constants.RangedDamageTypesFilename);
                SavingthrowTypes.LoadFromFile(ProjectFolder + Constants.SavingThrowTypesFilename);

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
                SpellPreferencesTables.LoadFromFile(ProjectFolder + Constants.SpellPreferencesTablesFilename);
                FeatPreferencesTables.LoadFromFile(ProjectFolder + Constants.FeatPreferencesTablesFilename);
                SkillPreferencesTables.LoadFromFile(ProjectFolder + Constants.SkillPreferencesTablesFilename);
                PackageEquipmentTables.LoadFromFile(ProjectFolder + Constants.PackageEquipmentTablesFilename);
                ItemPropertyTables.LoadFromFile(ProjectFolder + Constants.ItemPropertyTablesFilename);
                ItemPropertyCostTables.LoadFromFile(ProjectFolder + Constants.ItemPropertyCostTablesFilename);
                ItemPropertyParams.LoadFromFile(ProjectFolder + Constants.ItemPropertyParamsFilename);

                LoadCustomTlkStrings(ProjectFolder + Constants.CustomTlkStringsFilename);
                
                foreach (var template in CustomObjects)
                {
                    if (template != null)
                        CustomObjectRepositories[template].LoadFromFile(ProjectFolder + template.ResourceName + ".json");
                }

                foreach (var template in CustomTables)
                {
                    if (template != null)
                        CustomTableRepositories[template].LoadFromFile(ProjectFolder + template.FileName + ".json");
                }

                foreach (var template in CustomDynamicTables)
                {
                    if (template != null)
                        CustomDynamicTableRepositories[template].LoadFromFile(ProjectFolder + template.FileName + ".json");
                }

                Races.ResolveReferences();
                Classes.ResolveReferences();
                ClassPackages.ResolveReferences();
                Domains.ResolveReferences();
                Spells.ResolveReferences();
                Feats.ResolveReferences();
                Skills.ResolveReferences();
                Diseases.ResolveReferences();
                Poisons.ResolveReferences();
                Spellbooks.ResolveReferences();
                AreaEffects.ResolveReferences();
                MasterFeats.ResolveReferences();
                BaseItems.ResolveReferences();
                ItemPropertySets.ResolveReferences();
                ItemProperties.ResolveReferences();

                Ammunitions.ResolveReferences();
                Appearances.ResolveReferences();
                AppearanceSoundsets.ResolveReferences();
                WeaponSounds.ResolveReferences();
                InventorySounds.ResolveReferences();
                VisualEffects.ResolveReferences();
                Soundsets.ResolveReferences();
                Polymorphs.ResolveReferences();
                Portraits.ResolveReferences();
                Companions.ResolveReferences();
                Familiars.ResolveReferences();
                Traps.ResolveReferences();
                ProgrammedEffects.ResolveReferences();
                DamageTypes.ResolveReferences();
                DamageTypeGroups.ResolveReferences();
                RangedDamageTypes.ResolveReferences();
                SavingthrowTypes.ResolveReferences();

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
                SpellPreferencesTables.ResolveReferences();
                FeatPreferencesTables.ResolveReferences();
                SkillPreferencesTables.ResolveReferences();
                PackageEquipmentTables.ResolveReferences();
                ItemPropertyTables.ResolveReferences();
                ItemPropertyCostTables.ResolveReferences();
                ItemPropertyParams.ResolveReferences();

                foreach (var template in CustomObjects)
                {
                    if (template != null)
                        CustomObjectRepositories[template].ResolveReferences();
                }

                foreach (var template in CustomTables)
                {
                    if (template != null)
                        CustomTableRepositories[template].ResolveReferences();
                }

                foreach (var template in CustomDynamicTables)
                {
                    if (template != null)
                        CustomDynamicTableRepositories[template].ResolveReferences();
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }

            if (Settings.Ammunitions.Sorted) Ammunitions.Sort(d => d?.Label);
            if (Settings.Appearances.Sorted) Appearances.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.AppearanceSoundsets.Sorted) AppearanceSoundsets.Sort(d => d?.Name);
            if (Settings.AreaEffects.Sorted) AreaEffects.Sort(d => d?.Name);
            if (Settings.AttackBonusTables.Sorted) AttackBonusTables.Sort(d => d?.Name);
            if (Settings.BaseItems.Sorted) BaseItems.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.BonusFeatTables.Sorted) BonusFeatTables.Sort(d => d?.Name);
            if (Settings.Classes.Sorted) Classes.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.Companions.Sorted) Companions.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.DamageTypes.Sorted) DamageTypes.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.DamageTypeGroups.Sorted) DamageTypeGroups.Sort(d => d?.Name);
            if (Settings.Diseases.Sorted) Diseases.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.Domains.Sorted) Domains.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.Familiars.Sorted) Familiars.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.Feats.Sorted) Feats.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.ItemProperties.Sorted) ItemProperties.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.ItemPropertyTables.Sorted) ItemPropertyTables.Sort(d => d?.Name);
            if (Settings.ItemPropertyCostTables.Sorted) ItemPropertyCostTables.Sort(d => d?.Name);
            if (Settings.ItemPropertyParams.Sorted) ItemPropertyParams.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.MasterFeats.Sorted) MasterFeats.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.FeatTables.Sorted) FeatTables.Sort(d => d?.Name);
            if (Settings.KnownSpellsTables.Sorted) KnownSpellsTables.Sort(d => d?.Name);
            if (Settings.Packages.Sorted) ClassPackages.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.Poisons.Sorted) Poisons.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.Polymorphs.Sorted) Polymorphs.Sort(d => d?.Name);
            if (Settings.Portraits.Sorted) Portraits.Sort(d => d?.ResRef);
            if (Settings.ProgrammedEffects.Sorted) ProgrammedEffects.Sort(d => d?.Name);
            if (Settings.Races.Sorted) Races.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.RangedDamageTypes.Sorted) RangedDamageTypes.Sort(d => d?.Label);
            if (Settings.RacialFeatsTables.Sorted) RacialFeatsTables.Sort(d => d?.Name);
            if (Settings.SavesTables.Sorted) SavingThrowTables.Sort(d => d?.Name);
            if (Settings.SavingthrowTypes.Sorted) SavingthrowTypes.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.Skills.Sorted) Skills.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.SkillsTables.Sorted) SkillTables.Sort(d => d?.Name);
            if (Settings.Soundsets.Sorted) Soundsets.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.Spellbooks.Sorted) Spellbooks.Sort(d => d?.Name);
            if (Settings.Spells.Sorted) Spells.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.Traps.Sorted) Traps.Sort(d => d?.Name[DefaultLanguage].Text);
            if (Settings.SpellSlotTables.Sorted) SpellSlotTables.Sort(d => d?.Name);
            if (Settings.StatGainTables.Sorted) StatGainTables.Sort(d => d?.Name);
            if (Settings.PackageSpellPreferencesTables.Sorted) SpellPreferencesTables.Sort(d => d?.Name);
            if (Settings.PackageFeatPreferencesTables.Sorted) FeatPreferencesTables.Sort(d => d?.Name);
            if (Settings.PackageSkillPreferencesTables.Sorted) SkillPreferencesTables.Sort(d => d?.Name);
            if (Settings.PackageEquipmentTables.Sorted) PackageEquipmentTables.Sort(d => d?.Name);
            if (Settings.VisualEffects.Sorted) VisualEffects.Sort(d => d?.Name);
            if (Settings.WeaponSounds.Sorted) WeaponSounds.Sort(d => d?.Name);

            Log.Info("Loading project resources...");
            MasterRepository.LoadExternalResources(Settings.ExternalFolders);

            IsLoaded = true;
            EosConfig.LastProject = projectFilename;

            Log.Info("Project \"{0}\" loaded successfully!", Name);
        }
        
        private void SaveCustomTlkStrings(String filename)
        {
            JsonNode json = CustomTlkStrings.ToJson();
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.WriteIndented = true;
            serializeOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
            File.WriteAllText(filename, json.ToJsonString(serializeOptions));
        }

        public void Save()
        {
            Log.Info("Saving project file: \"{0}\"", ProjectFolder + Name + Constants.ProjectFileExtension);
            SaveProjectFile(ProjectFolder + Name + Constants.ProjectFileExtension);

            if (!Directory.Exists(ProjectFolder + Constants.ExternalFilesPath))
                Directory.CreateDirectory(ProjectFolder + Constants.ExternalFilesPath);

            Log.Info("Saving project data...");
            try
            {
                Races.SaveToFile(ProjectFolder + Constants.RacesFilename, Settings.Races.FormatJson);
                Classes.SaveToFile(ProjectFolder + Constants.ClassesFilename, Settings.Classes.FormatJson);
                ClassPackages.SaveToFile(ProjectFolder + Constants.ClassPackagesFilename, Settings.Packages.FormatJson);
                Domains.SaveToFile(ProjectFolder + Constants.DomainsFilename, Settings.Domains.FormatJson);
                Spells.SaveToFile(ProjectFolder + Constants.SpellsFilename, Settings.Spells.FormatJson);
                Feats.SaveToFile(ProjectFolder + Constants.FeatsFilename, Settings.Feats.FormatJson);
                Skills.SaveToFile(ProjectFolder + Constants.SkillsFilename, Settings.Skills.FormatJson);
                Diseases.SaveToFile(ProjectFolder + Constants.DiseasesFilename, Settings.Diseases.FormatJson);
                Poisons.SaveToFile(ProjectFolder + Constants.PoisonsFilename, Settings.Poisons.FormatJson);
                Spellbooks.SaveToFile(ProjectFolder + Constants.SpellbooksFilename, Settings.Spellbooks.FormatJson);
                AreaEffects.SaveToFile(ProjectFolder + Constants.AreaEffectsFilename, Settings.AreaEffects.FormatJson);
                MasterFeats.SaveToFile(ProjectFolder + Constants.MasterFeatsFilename, Settings.MasterFeats.FormatJson);
                BaseItems.SaveToFile(ProjectFolder + Constants.BaseItemsFilename, Settings.BaseItems.FormatJson);
                ItemPropertySets.SaveToFile(ProjectFolder + Constants.ItemPropertySetsFilename, Settings.ItemPropertySets.FormatJson);
                ItemProperties.SaveToFile(ProjectFolder + Constants.ItemPropertiesFilename, Settings.ItemProperties.FormatJson);

                Ammunitions.SaveToFile(ProjectFolder + Constants.AmmunitionsFilename, Settings.Ammunitions.FormatJson);
                Appearances.SaveToFile(ProjectFolder + Constants.AppearancesFilename, Settings.Appearances.FormatJson);
                AppearanceSoundsets.SaveToFile(ProjectFolder + Constants.AppearanceSoundsetsFilename, Settings.AppearanceSoundsets.FormatJson);
                WeaponSounds.SaveToFile(ProjectFolder + Constants.WeaponSoundsFilename, Settings.WeaponSounds.FormatJson);
                InventorySounds.SaveToFile(ProjectFolder + Constants.InventorySoundsFilename, Settings.InventorySounds.FormatJson);
                Portraits.SaveToFile(ProjectFolder + Constants.PortraitsFilename, Settings.Portraits.FormatJson);
                VisualEffects.SaveToFile(ProjectFolder + Constants.VisualEffectsFilename, Settings.VisualEffects.FormatJson);
                Soundsets.SaveToFile(ProjectFolder + Constants.SoundsetsFilename, Settings.Soundsets.FormatJson);
                Polymorphs.SaveToFile(ProjectFolder + Constants.PolymorphsFilename, Settings.Polymorphs.FormatJson);
                Companions.SaveToFile(ProjectFolder + Constants.CompanionsFilename, Settings.Companions.FormatJson);
                Familiars.SaveToFile(ProjectFolder + Constants.FamiliarsFilename, Settings.Familiars.FormatJson);
                Traps.SaveToFile(ProjectFolder + Constants.TrapsFilename, Settings.Traps.FormatJson);
                ProgrammedEffects.SaveToFile(ProjectFolder + Constants.ProgrammedEffectsFilename, Settings.ProgrammedEffects.FormatJson);
                DamageTypes.SaveToFile(ProjectFolder + Constants.DamageTypesFilename, Settings.DamageTypes.FormatJson);
                DamageTypeGroups.SaveToFile(ProjectFolder + Constants.DamageTypeGroupsFilename, Settings.DamageTypeGroups.FormatJson);
                RangedDamageTypes.SaveToFile(ProjectFolder + Constants.RangedDamageTypesFilename, Settings.RangedDamageTypes.FormatJson);
                SavingthrowTypes.SaveToFile(ProjectFolder + Constants.SavingThrowTypesFilename, Settings.SavingthrowTypes.FormatJson);

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
                SpellPreferencesTables.SaveToFile(ProjectFolder + Constants.SpellPreferencesTablesFilename, Settings.PackageSpellPreferencesTables.FormatJson);
                FeatPreferencesTables.SaveToFile(ProjectFolder + Constants.FeatPreferencesTablesFilename, Settings.PackageFeatPreferencesTables.FormatJson);
                SkillPreferencesTables.SaveToFile(ProjectFolder + Constants.SkillPreferencesTablesFilename, Settings.PackageSkillPreferencesTables.FormatJson);
                PackageEquipmentTables.SaveToFile(ProjectFolder + Constants.PackageEquipmentTablesFilename, Settings.PackageEquipmentTables.FormatJson);
                ItemPropertyTables.SaveToFile(ProjectFolder + Constants.ItemPropertyTablesFilename, Settings.ItemPropertyTables.FormatJson);
                ItemPropertyCostTables.SaveToFile(ProjectFolder + Constants.ItemPropertyCostTablesFilename, Settings.ItemPropertyCostTables.FormatJson);
                ItemPropertyParams.SaveToFile(ProjectFolder + Constants.ItemPropertyParamsFilename, Settings.ItemPropertyParams.FormatJson);

                CustomEnums.SaveToFile(ProjectFolder + Constants.CustomEnumsFilename, Settings.CustomData.FormatJson);
                CustomObjects.SaveToFile(ProjectFolder + Constants.CustomObjectsFilename, Settings.CustomData.FormatJson);
                CustomTables.SaveToFile(ProjectFolder + Constants.CustomTablesFilename, Settings.CustomData.FormatJson);
                CustomDynamicTables.SaveToFile(ProjectFolder + Constants.CustomDynamicTablesFilename, Settings.CustomData.FormatJson);

                SaveCustomTlkStrings(ProjectFolder + Constants.CustomTlkStringsFilename);
                
                foreach (var template in CustomObjects)
                {
                    if (template != null)
                        CustomObjectRepositories[template].SaveToFile(ProjectFolder + template.ResourceName + ".json", Settings.CustomData.FormatJson);
                }

                foreach (var template in CustomTables)
                {
                    if (template != null)
                        CustomTableRepositories[template].SaveToFile(ProjectFolder + template.FileName + ".json", Settings.CustomData.FormatJson);
                }

                foreach (var template in CustomDynamicTables)
                {
                    if (template != null)
                        CustomDynamicTableRepositories[template].SaveToFile(ProjectFolder + template.FileName + ".json", Settings.CustomData.FormatJson);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }

            Log.Info("Project \"{0}\" saved successfully!", Name);
        }
    }
}
