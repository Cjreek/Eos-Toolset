using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class SpellbookEntry
    {
        private Spell? _spell;
        private Spellbook _spellbook;

        public int Level { get; set; }
        public Spell? Spell
        {
            get { return _spell; }
            set
            {
                if (_spell != value)
                {
                    _spell?.AddReference(_spellbook, "Level" + Level.ToString());
                    _spell = value;
                    _spell?.RemoveReference(_spellbook, "Level" + Level.ToString());
                }
            }
        }

        public SpellbookEntry(Spellbook spellbook, Spell? spell = null, int level = 0)
        {
            this._spellbook = spellbook;
            this._spell = spell;
            this.Level = level;
        }
    }

    public class Spellbook : BaseModel
    {
        private String _name = "";

        public String Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<SpellbookEntry> Level0 { get; } = new ObservableCollection<SpellbookEntry>();
        public ObservableCollection<SpellbookEntry> Level1 { get; } = new ObservableCollection<SpellbookEntry>();
        public ObservableCollection<SpellbookEntry> Level2 { get; } = new ObservableCollection<SpellbookEntry>();
        public ObservableCollection<SpellbookEntry> Level3 { get; } = new ObservableCollection<SpellbookEntry>();
        public ObservableCollection<SpellbookEntry> Level4 { get; } = new ObservableCollection<SpellbookEntry>();
        public ObservableCollection<SpellbookEntry> Level5 { get; } = new ObservableCollection<SpellbookEntry>();
        public ObservableCollection<SpellbookEntry> Level6 { get; } = new ObservableCollection<SpellbookEntry>();
        public ObservableCollection<SpellbookEntry> Level7 { get; } = new ObservableCollection<SpellbookEntry>();
        public ObservableCollection<SpellbookEntry> Level8 { get; } = new ObservableCollection<SpellbookEntry>();
        public ObservableCollection<SpellbookEntry> Level9 { get; } = new ObservableCollection<SpellbookEntry>();

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name = "NewSpellbook";
        }

        public void AddSpell(int level, Spell spell)
        {
            AddSpell(level, new SpellbookEntry(this, spell, level));
        }

        public void AddSpell(int level, SpellbookEntry entry)
        {
            entry.Spell?.AddReference(this, "Level" + level.ToString());

            entry.Level = level;
            switch (level)
            {
                case 0: Level0.Add(entry); break;
                case 1: Level1.Add(entry); break;
                case 2: Level2.Add(entry); break;
                case 3: Level3.Add(entry); break;
                case 4: Level4.Add(entry); break;
                case 5: Level5.Add(entry); break;
                case 6: Level6.Add(entry); break;
                case 7: Level7.Add(entry); break;
                case 8: Level8.Add(entry); break;
                case 9: Level9.Add(entry); break;
            }
        }

        public void RemoveSpell(SpellbookEntry? entry)
        {
            if (entry == null) return;
            if (Level0.Remove(entry)) entry.Spell?.RemoveReference(this, "Level0");
            if (Level1.Remove(entry)) entry.Spell?.RemoveReference(this, "Level1");
            if (Level2.Remove(entry)) entry.Spell?.RemoveReference(this, "Level2");
            if (Level3.Remove(entry)) entry.Spell?.RemoveReference(this, "Level3");
            if (Level4.Remove(entry)) entry.Spell?.RemoveReference(this, "Level4");
            if (Level5.Remove(entry)) entry.Spell?.RemoveReference(this, "Level5");
            if (Level6.Remove(entry)) entry.Spell?.RemoveReference(this, "Level6");
            if (Level7.Remove(entry)) entry.Spell?.RemoveReference(this, "Level7");
            if (Level8.Remove(entry)) entry.Spell?.RemoveReference(this, "Level8");
            if (Level9.Remove(entry)) entry.Spell?.RemoveReference(this, "Level9");
        }

        private SpellbookEntry? FindEntry(ObservableCollection<SpellbookEntry> spellLevel, int spellId)
        {
            foreach (var spellEntry in spellLevel)
            {
                if (spellEntry.Spell?.Index == spellId) 
                    return spellEntry;
            }

            return null;
        }

        public void RemoveSpellById(int level, int spellId)
        {
            switch (level)
            {
                case 0: RemoveSpell(FindEntry(Level0, spellId)); break;
                case 1: RemoveSpell(FindEntry(Level1, spellId)); break;
                case 2: RemoveSpell(FindEntry(Level2, spellId)); break;
                case 3: RemoveSpell(FindEntry(Level3, spellId)); break;
                case 4: RemoveSpell(FindEntry(Level4, spellId)); break;
                case 5: RemoveSpell(FindEntry(Level5, spellId)); break;
                case 6: RemoveSpell(FindEntry(Level6, spellId)); break;
                case 7: RemoveSpell(FindEntry(Level7, spellId)); break;
                case 8: RemoveSpell(FindEntry(Level8, spellId)); break;
                case 9: RemoveSpell(FindEntry(Level9, spellId)); break;
            }
        }

        public int? GetSpellLevel(int spellId)
        {
            var entry = FindEntry(Level0, spellId) ?? 
                        FindEntry(Level1, spellId) ??
                        FindEntry(Level2, spellId) ??
                        FindEntry(Level3, spellId) ??
                        FindEntry(Level4, spellId) ??
                        FindEntry(Level5, spellId) ??
                        FindEntry(Level6, spellId) ??
                        FindEntry(Level7, spellId) ??
                        FindEntry(Level8, spellId) ??
                        FindEntry(Level9, spellId);

            return entry?.Level;
        }

        private void ResolveSpellList(ObservableCollection<SpellbookEntry> spellList)
        {
            for (int i=spellList.Count-1; i>=0; i--)
            {
                SpellbookEntry entry = spellList[i];
                entry.Spell = Resolve(entry.Spell, MasterRepository.Spells);
                if (entry.Spell != null)
                    spellList[i] = entry;
                else
                    spellList.RemoveAt(i);
            }
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            ResolveSpellList(Level0);
            ResolveSpellList(Level1);
            ResolveSpellList(Level2);
            ResolveSpellList(Level3);
            ResolveSpellList(Level4);
            ResolveSpellList(Level5);
            ResolveSpellList(Level6);
            ResolveSpellList(Level7);
            ResolveSpellList(Level8);
            ResolveSpellList(Level9);
        }

        private void ReadLevel(JsonArray? jsonArray, ObservableCollection<SpellbookEntry> targetList, int level)
        {
            if (jsonArray == null) return;
            foreach (var item in jsonArray)
            {
                if (item is JsonObject spellObj)
                {
                    var spell = CreateRefFromJson<Spell>(spellObj);
                    if (spell != null)
                        targetList.Add(new SpellbookEntry(this, spell, level));
                }
            }
        }

        private void WriteLevel(JsonObject parentJson, String levelName, ObservableCollection<SpellbookEntry> sourceList)
        {
            var jsonArray = new JsonArray();
            for (int i = 0; i < sourceList.Count; i++)
                jsonArray.Add(CreateJsonRef(sourceList[i].Spell));
            parentJson.Add(levelName, jsonArray);
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<String>() ?? "";

            ReadLevel(json["Level0"]?.AsArray(), Level0, 0);
            ReadLevel(json["Level1"]?.AsArray(), Level1, 1);
            ReadLevel(json["Level2"]?.AsArray(), Level2, 2);
            ReadLevel(json["Level3"]?.AsArray(), Level3, 3);
            ReadLevel(json["Level4"]?.AsArray(), Level4, 4);
            ReadLevel(json["Level5"]?.AsArray(), Level5, 5);
            ReadLevel(json["Level6"]?.AsArray(), Level6, 6);
            ReadLevel(json["Level7"]?.AsArray(), Level7, 7);
            ReadLevel(json["Level8"]?.AsArray(), Level8, 8);
            ReadLevel(json["Level9"]?.AsArray(), Level9, 9);
        }

        public override JsonObject ToJson()
        {
            var spellbookJson = base.ToJson();
            spellbookJson.Add("Name", this.Name);

            WriteLevel(spellbookJson, "Level0", Level0);
            WriteLevel(spellbookJson, "Level1", Level1);
            WriteLevel(spellbookJson, "Level2", Level2);
            WriteLevel(spellbookJson, "Level3", Level3);
            WriteLevel(spellbookJson, "Level4", Level4);
            WriteLevel(spellbookJson, "Level5", Level5);
            WriteLevel(spellbookJson, "Level6", Level6);
            WriteLevel(spellbookJson, "Level7", Level7);
            WriteLevel(spellbookJson, "Level8", Level8);
            WriteLevel(spellbookJson, "Level9", Level9);

            return spellbookJson;
        }
    }
}
