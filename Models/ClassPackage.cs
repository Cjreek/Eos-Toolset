﻿using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class ClassPackage : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public CharacterClass? ForClass { get; set; }
        public AbilityType? PreferredAbility { get; set; }
        public int Gold { get; set; }
        public SpellSchool? SpellSchool { get; set; }
        public Domain? Domain1 { get; set; }
        public Domain? Domain2 { get; set; }
        public IntPtr Associate { get; set; }
        public IntPtr SpellPreferences { get; set; }
        public IntPtr FeatPreferences { get; set; }
        public IntPtr SkillPreferences { get; set; }
        public IntPtr StartingEquipment { get; set; }
        public IntPtr Soundset { get; set; }
        public bool Playable { get; set; }

        public override void ResolveReferences()
        {
            ForClass = Resolve(ForClass, MasterRepository.Classes);
            Domain1 = Resolve(Domain1, MasterRepository.Domains);
            Domain2 = Resolve(Domain2, MasterRepository.Domains);
            //Soundset = Resolve(Soundset, MasterRepository.Soundsets);
        }

        public override void FromJson(JsonObject json)
        {
            this.ID = ParseGuid(json["ID"]?.GetValue<String>());
            this.Index = json["Index"]?.GetValue<int?>();
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.ForClass = CreateRefFromJson<CharacterClass>(json["ForClass"]?.AsObject());
            this.PreferredAbility = JsonToEnum<AbilityType>(json["PreferredAbility"]);
            this.Gold = json["Gold"]?.GetValue<int?>() ?? 0;
            this.SpellSchool = JsonToEnum<SpellSchool>(json["SpellSchool"]);
            this.Domain1 = CreateRefFromJson<Domain>(json["Domain1"]?.AsObject());
            this.Domain2 = CreateRefFromJson<Domain>(json["Domain2"]?.AsObject());
            this.Associate = IntPtr.Zero; // !
            this.SpellPreferences = IntPtr.Zero; // !
            this.FeatPreferences = IntPtr.Zero; // !
            this.SkillPreferences = IntPtr.Zero; // !
            this.StartingEquipment = IntPtr.Zero; // !
            this.Soundset = IntPtr.Zero; // !
            this.Playable = json["Playable"]?.GetValue<bool>() ?? true;
        }

        public override JsonObject ToJson()
        {
            var packageJson = new JsonObject();
            packageJson.Add("ID", this.ID.ToString());
            packageJson.Add("Index", this.Index);
            packageJson.Add("Name", this.Name.ToJson());
            packageJson.Add("Description", this.Description.ToJson());
            packageJson.Add("ForClass", CreateJsonRef(this.ForClass));
            packageJson.Add("PreferredAbility", EnumToJson(this.PreferredAbility));
            packageJson.Add("Gold", this.Gold);
            packageJson.Add("SpellSchool", EnumToJson(this.SpellSchool));
            packageJson.Add("Domain1", CreateJsonRef(this.Domain1));
            packageJson.Add("Domain2", CreateJsonRef(this.Domain2));
            packageJson.Add("Associate", null); // !
            packageJson.Add("SpellPreferences", null); // !
            packageJson.Add("FeatPreferences", null); // !
            packageJson.Add("SkillPreferences", null); // !
            packageJson.Add("StartingEquipment", null); // !
            packageJson.Add("Soundset", null); // ! CreateJsonRef(this.Soundset)
            packageJson.Add("Playable", this.Playable);

            return packageJson;
        }
    }
}
