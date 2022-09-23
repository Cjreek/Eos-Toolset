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
        public Spell? Spell { get; set; }
    }

    public class Spellbook : BaseModel
    {
        public string Name { get; set; } = "";
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

        protected override String GetLabel()
        {
            return Name;
        }

        public void AddSpell(int level, Spell spell)
        {
            AddSpell(level, new SpellbookEntry { Spell = spell });
        }

        public void AddSpell(int level, SpellbookEntry entry)
        {
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

        private void ReadLevel(JsonArray? jsonArray, ObservableCollection<SpellbookEntry> targetList)
        {
            if (jsonArray == null) return;
            foreach (var item in jsonArray)
            {
                if (item is JsonObject spellObj)
                {
                    var spell = CreateRefFromJson<Spell>(spellObj);
                    if (spell != null)
                        targetList.Add(new SpellbookEntry { Spell = spell });
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
            this.ID = ParseGuid(json["ID"]?.GetValue<String>());
            this.Name = json["Name"]?.GetValue<String>() ?? "";

            ReadLevel(json["Level0"]?.AsArray(), Level0);
            ReadLevel(json["Level1"]?.AsArray(), Level1);
            ReadLevel(json["Level2"]?.AsArray(), Level2);
            ReadLevel(json["Level3"]?.AsArray(), Level3);
            ReadLevel(json["Level4"]?.AsArray(), Level4);
            ReadLevel(json["Level5"]?.AsArray(), Level5);
            ReadLevel(json["Level6"]?.AsArray(), Level6);
            ReadLevel(json["Level7"]?.AsArray(), Level7);
            ReadLevel(json["Level8"]?.AsArray(), Level8);
            ReadLevel(json["Level9"]?.AsArray(), Level9);
        }

        public override JsonObject ToJson()
        {
            var spellbookJson = new JsonObject();
            spellbookJson.Add("ID", this.ID.ToString());
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
