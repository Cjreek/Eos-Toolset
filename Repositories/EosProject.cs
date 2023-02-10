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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

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

        public bool ExportLowercaseFilenames { get; set; } = false;
        public string ExternalFolder { get; set; } = "";
        public string ExportHakFolder { get; set; } = "";
        public string Export2daFolder { get; set; } = "";
        public string ExportTlkFolder { get; set; } = "";
        public string ExportIncludeFolder { get; set; } = "";
        public string BaseTlkFile { get; set; } = "";
        public int ExportTlkOffset { get; set; } = 0;
        public bool FormatJson { get; set; } = false;
        public bool SortCustomData { get; set; } = false;

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

            ExternalFolder = ProjectFolder + Constants.ExternalFilesPath;
            ExportHakFolder = ProjectFolder + Constants.ExportHAKFolder;
            ExportTlkFolder = ProjectFolder + Constants.ExportTLKFolder;
            Export2daFolder = ProjectFolder + Constants.Export2DAFolder;
            ExportIncludeFolder = ProjectFolder + Constants.Export2DAFolder;
            BaseTlkFile = "";
            ExportTlkOffset = 0;
            FormatJson = false;
            SortCustomData = false;

            var fs = new FileStream(projectFilename, FileMode.Open, FileAccess.Read);
            try
            {
                if (JsonNode.Parse(fs) is JsonObject projectJson)
                {
                    Name = projectJson["Name"]?.GetValue<String>() ?? "";
                    DefaultLanguage = Enum.Parse<TLKLanguage>(projectJson["DefaultLanguage"]?.GetValue<string>() ?? "");
                    ExportLowercaseFilenames = projectJson["ExportLowercaseFilenames"]?.GetValue<bool>() ?? false;
                    ExternalFolder = projectJson["ExternalFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExternalFilesPath;
                    ExportHakFolder = projectJson["ExportHakFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExportHAKFolder;
                    ExportTlkFolder = projectJson["ExportTlkFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.ExportTLKFolder;
                    Export2daFolder = projectJson["Export2daFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.Export2DAFolder;
                    ExportIncludeFolder = projectJson["ExportIncludeFolder"]?.GetValue<string>() ?? ProjectFolder + Constants.Export2DAFolder;
                    BaseTlkFile = projectJson["BaseTlkFile"]?.GetValue<string>() ?? "";
                    ExportTlkOffset = projectJson["ExportTlkOffset"]?.GetValue<int>() ?? 0;
                    FormatJson = projectJson["FormatJson"]?.GetValue<bool>() ?? false;
                    SortCustomData = projectJson["SortCustomData"]?.GetValue<bool>() ?? false;
                }
            }
            finally
            {
                fs.Close();
            }

            if (!ExternalFolder.EndsWith(Path.DirectorySeparatorChar)) ExternalFolder += Path.DirectorySeparatorChar;
            if (!ExportHakFolder.EndsWith(Path.DirectorySeparatorChar)) ExportHakFolder += Path.DirectorySeparatorChar;
            if (!ExportTlkFolder.EndsWith(Path.DirectorySeparatorChar)) ExportTlkFolder += Path.DirectorySeparatorChar;
            if (!Export2daFolder.EndsWith(Path.DirectorySeparatorChar)) Export2daFolder += Path.DirectorySeparatorChar;
            if (!ExportIncludeFolder.EndsWith(Path.DirectorySeparatorChar)) ExportIncludeFolder += Path.DirectorySeparatorChar;

            Directory.SetCurrentDirectory(ProjectFolder);
        }

        private void SaveProjectFile(string projectFilename)
        {
            JsonObject projectFile = new JsonObject();
            projectFile.Add("Name", Name);
            projectFile.Add("DefaultLanguage", Enum.GetName(DefaultLanguage));
            projectFile.Add("ExportLowercaseFilenames", ExportLowercaseFilenames);
            projectFile.Add("ExternalFolder", ExternalFolder);
            projectFile.Add("ExportHakFolder", ExportHakFolder);
            projectFile.Add("Export2daFolder", Export2daFolder);
            projectFile.Add("ExportTlkFolder", ExportTlkFolder);
            projectFile.Add("ExportIncludeFolder", ExportIncludeFolder);
            projectFile.Add("BaseTlkFile", BaseTlkFile);
            projectFile.Add("ExportTlkOffset", ExportTlkOffset);
            projectFile.Add("FormatJson", FormatJson);
            projectFile.Add("SortCustomData", SortCustomData);

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

            if (SortCustomData)
            {
                Races.Sort(d => d?.Name[TLKLanguage.English].Text);
                Classes.Sort(d => d?.Name[TLKLanguage.English].Text);
                Domains.Sort(d => d?.Name[TLKLanguage.English].Text);
                Skills.Sort(s => s?.Name[TLKLanguage.English].Text);
                Feats.Sort(f => f?.Name[TLKLanguage.English].Text);
                Spells.Sort(s => s?.Name[TLKLanguage.English].Text);
                Diseases.Sort(d => d?.Name[TLKLanguage.English].Text);
                Poisons.Sort(p => p?.Name[TLKLanguage.English].Text);
                Spellbooks.Sort(p => p?.Name);

                Polymorphs.Sort(p => p?.Name);
                AreaEffects.Sort(p => p?.Name);
                Appearances.Sort(p => p?.Name[TLKLanguage.English].Text);
                ClassPackages.Sort(p => p?.Name[TLKLanguage.English].Text);
                Soundsets.Sort(p => p?.Name[TLKLanguage.English].Text);
            }

            IsLoaded = true;
            EosConfig.LastProject = projectFilename;
            MasterRepository.LoadExternalResources(ExternalFolder);
        }

        public void Save()
        {
            SaveProjectFile(ProjectFolder + Name + Constants.ProjectFileExtension);

            if (!Directory.Exists(ProjectFolder + Constants.ExternalFilesPath))
                Directory.CreateDirectory(ProjectFolder + Constants.ExternalFilesPath);

            Races.SaveToFile(ProjectFolder + Constants.RacesFilename, FormatJson);
            Classes.SaveToFile(ProjectFolder + Constants.ClassesFilename, FormatJson);
            Domains.SaveToFile(ProjectFolder + Constants.DomainsFilename, FormatJson);
            Spells.SaveToFile(ProjectFolder + Constants.SpellsFilename, FormatJson);
            Feats.SaveToFile(ProjectFolder + Constants.FeatsFilename, FormatJson);
            Skills.SaveToFile(ProjectFolder + Constants.SkillsFilename, FormatJson);
            Diseases.SaveToFile(ProjectFolder + Constants.DiseasesFilename, FormatJson);
            Poisons.SaveToFile(ProjectFolder + Constants.PoisonsFilename, FormatJson);
            Spellbooks.SaveToFile(ProjectFolder + Constants.SpellbooksFilename, FormatJson);
            AreaEffects.SaveToFile(ProjectFolder + Constants.AreaEffectsFilename, FormatJson);

            ClassPackages.SaveToFile(ProjectFolder + Constants.ClassPackagesFilename, FormatJson);
            Soundsets.SaveToFile(ProjectFolder + Constants.SoundsetsFilename, FormatJson);
            Polymorphs.SaveToFile(ProjectFolder + Constants.PolymorphsFilename, FormatJson);

            AttackBonusTables.SaveToFile(ProjectFolder + Constants.AttackBonusTablesFilename, FormatJson);
            BonusFeatTables.SaveToFile(ProjectFolder + Constants.BonusFeatTablesFilename, FormatJson);
            FeatTables.SaveToFile(ProjectFolder + Constants.FeatTablesFilename, FormatJson);
            SavingThrowTables.SaveToFile(ProjectFolder + Constants.SavingThrowTablesFilename, FormatJson);
            SkillTables.SaveToFile(ProjectFolder + Constants.SkillTablesFilename, FormatJson);
            PrerequisiteTables.SaveToFile(ProjectFolder + Constants.PrerequisiteTablesFilename, FormatJson);
            SpellSlotTables.SaveToFile(ProjectFolder + Constants.SpellSlotTablesFilename, FormatJson);
            KnownSpellsTables.SaveToFile(ProjectFolder + Constants.KnownSpellsTablesFilename, FormatJson);
            StatGainTables.SaveToFile(ProjectFolder + Constants.StatGainTablesFilename, FormatJson);
            RacialFeatsTables.SaveToFile(ProjectFolder + Constants.RacialFeatsTablesFilename, FormatJson);

            CustomEnums.SaveToFile(ProjectFolder + Constants.CustomEnumsFilename, FormatJson);
            CustomObjects.SaveToFile(ProjectFolder + Constants.CustomObjectsFilename, FormatJson);

            foreach (var template in CustomObjects)
            {
                if (template != null)
                    CustomObjectRepositories[template].SaveToFile(ProjectFolder + template.ResourceName + ".json", FormatJson);
            }
        }
    }
}
