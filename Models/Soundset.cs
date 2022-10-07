using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
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
    public class SoundsetEntry
    {
        public SoundsetEntryType Type { get; set; }
        public string SoundFile { get; set; } = "";
        public TLKStringSet Text { get; set; } = new TLKStringSet();
    }

    public class SoundsetEntryList : ObservableCollection<SoundsetEntry>
    {
        private Dictionary<SoundsetEntryType, SoundsetEntry> entriesByType = new Dictionary<SoundsetEntryType, SoundsetEntry>();

        public SoundsetEntryList()
        {
            foreach (var soundsetEntryType in Enum.GetValues<SoundsetEntryType>())
            {
                var entry = new SoundsetEntry();
                entry.Type = soundsetEntryType;

                entriesByType[soundsetEntryType] = entry;
                Add(entry);
            }
        }

        public SoundsetEntry GetByType(SoundsetEntryType type)
        {
            return entriesByType[type];
        }
    }

    public class Soundset : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public Gender Gender { get; set; }
        public SoundsetType Type { get; set; }
        public String SoundsetResource { get; set; } = "";
        public SoundsetEntryList Entries { get; set; } = new SoundsetEntryList();

        protected override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Soundset";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Soundset";
        }

        public override void ResolveReferences()
        {
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Gender = JsonToEnum<Gender>(json["Gender"]) ?? Gender.Male;
            this.Type = JsonToEnum<SoundsetType>(json["Type"]) ?? SoundsetType.Player;
            this.SoundsetResource = json["SoundsetResource"]?.GetValue<String>() ?? "";

            JsonArray entryArray = json["Entries"]?.AsArray() ?? new JsonArray();
            for (int i=0; i < entryArray.Count; i++)
            {
                var entryType = JsonToEnum<SoundsetEntryType>(entryArray[i]?["Type"]);
                if (entryType != null)
                {
                    var entry = Entries.GetByType(entryType ?? SoundsetEntryType.ATTACK);
                    entry.Text.FromJson(entryArray[i]?["Text"]?.AsObject());
                    entry.SoundFile = entryArray[i]?["SoundFile"]?.GetValue<String?>() ?? "";
                }
            }
        }

        public override JsonObject ToJson()
        {
            var soundsetJson = base.ToJson();
            soundsetJson.Add("Name", this.Name.ToJson());
            soundsetJson.Add("Gender", EnumToJson(this.Gender));
            soundsetJson.Add("Type", EnumToJson(this.Type));
            soundsetJson.Add("SoundsetResource", this.SoundsetResource);

            var entryArray = new JsonArray();
            foreach (var entry in this.Entries)
            {
                if (entry.SoundFile.Trim() != String.Empty)
                {
                    var entryJson = new JsonObject();
                    entryJson.Add("Type", EnumToJson(entry.Type));
                    if (!(entry.Text[MasterRepository.Project.DefaultLanguage].Text.Trim() == String.Empty))
                        entryJson.Add("Text", entry.Text.ToJson());
                    entryJson.Add("SoundFile", entry.SoundFile);
                    entryArray.Add(entryJson);
                }
            }
            soundsetJson.Add("Entries", entryArray);

            return soundsetJson;
        }
    }
}
