using Eos.Nwn.Tlk;
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
    public class SavingthrowType : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public ImmunityType? Immunity { get; set; }
        public bool ImmunityOnlyForSpells { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (Familiar?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Savingthrow Type";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Savingthrow Type";
        }

        public override String GetLabel()
        {
            return Name;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Immunity = JsonToEnum<ImmunityType>(json["Immunity"]);
            this.ImmunityOnlyForSpells = json["ImmunityOnlyForSpells"]?.GetValue<bool>() ?? false;
        }

        public override JsonObject ToJson()
        {
            var savingthrowTypeJson = base.ToJson();
            savingthrowTypeJson.Add("Name", this.Name.ToJson());
            savingthrowTypeJson.Add("Immunity", EnumToJson(this.Immunity));
            savingthrowTypeJson.Add("ImmunityOnlyForSpells", this.ImmunityOnlyForSpells);

            return savingthrowTypeJson;
        }
    }
}
