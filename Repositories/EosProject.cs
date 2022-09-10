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
    public class EosProject : RepositoryCollection, INotifyPropertyChanged
    {
        private string _projectFolder = "";
        private string _name = "";

        public EosProject() : base(false)
        {
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

            Load(projectFolder);
        }

        private void LoadProjectFile(string projectFilename)
        {
            var fs = new FileStream(projectFilename, FileMode.Open, FileAccess.Read);
            try
            {
                if (JsonNode.Parse(fs) is JsonObject projectJson)
                {
                    Name = projectJson["Name"]?.GetValue<String>() ?? "";
                    DefaultLanguage = Enum.Parse<TLKLanguage>(projectJson["DefaultLanguage"]?.GetValue<String>() ?? "");
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

        public void Load(string projectFolder)
        {
            ProjectFolder = projectFolder;
            Clear();

            LoadProjectFile(ProjectFolder + Constants.ProjectFilename);
            Races.LoadFromFile(ProjectFolder + Constants.RacesFilename);
            Classes.LoadFromFile(ProjectFolder + Constants.ClassesFilename);
            Domains.LoadFromFile(ProjectFolder + Constants.DomainsFilename);
            Spells.LoadFromFile(ProjectFolder + Constants.SpellsFilename);
            Feats.LoadFromFile(ProjectFolder + Constants.FeatsFilename);
            Skills.LoadFromFile(ProjectFolder + Constants.SkillsFilename);
            Diseases.LoadFromFile(ProjectFolder + Constants.DiseasesFilename);
            Poisons.LoadFromFile(ProjectFolder + Constants.PoisonsFilename);

            AttackBonusTables.LoadFromFile(ProjectFolder + Constants.AttackBonusTablesFilename);
            BonusFeatTables.LoadFromFile(ProjectFolder + Constants.BonusFeatTablesFilename);
            FeatTables.LoadFromFile(ProjectFolder + Constants.FeatTablesFilename);
            SavingThrowTables.LoadFromFile(ProjectFolder + Constants.SavingThrowTablesFilename);
            SkillTables.LoadFromFile(ProjectFolder + Constants.SkillTablesFilename);
            PrerequisiteTables.LoadFromFile(ProjectFolder + Constants.PrerequisiteTablesFilename);
            SpellSlotTables.LoadFromFile(ProjectFolder + Constants.SpellSlotTablesFilename);
            KnownSpellsTables.LoadFromFile(ProjectFolder + Constants.KnownSpellsTablesFilename);
        }

        public void Save()
        {
            SaveProjectFile(ProjectFolder + Constants.ProjectFilename);
            Races.SaveToFile(ProjectFolder + Constants.RacesFilename);
            Classes.SaveToFile(ProjectFolder + Constants.ClassesFilename);
            Domains.SaveToFile(ProjectFolder + Constants.DomainsFilename);
            Spells.SaveToFile(ProjectFolder + Constants.SpellsFilename);
            Feats.SaveToFile(ProjectFolder + Constants.FeatsFilename);
            Skills.SaveToFile(ProjectFolder + Constants.SkillsFilename);
            Diseases.SaveToFile(ProjectFolder + Constants.DiseasesFilename);
            Poisons.SaveToFile(ProjectFolder + Constants.PoisonsFilename);

            AttackBonusTables.SaveToFile(ProjectFolder + Constants.AttackBonusTablesFilename);
            BonusFeatTables.SaveToFile(ProjectFolder + Constants.BonusFeatTablesFilename);
            FeatTables.SaveToFile(ProjectFolder + Constants.FeatTablesFilename);
            SavingThrowTables.SaveToFile(ProjectFolder + Constants.SavingThrowTablesFilename);
            SkillTables.SaveToFile(ProjectFolder + Constants.SkillTablesFilename);
            PrerequisiteTables.SaveToFile(ProjectFolder + Constants.PrerequisiteTablesFilename);
            SpellSlotTables.SaveToFile(ProjectFolder + Constants.SpellSlotTablesFilename);
            KnownSpellsTables.SaveToFile(ProjectFolder + Constants.KnownSpellsTablesFilename);
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
}
