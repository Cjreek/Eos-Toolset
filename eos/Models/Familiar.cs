using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class Familiar : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();
        public string Template { get; set; } = "";

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
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Familiar";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Familiar";
        }

        public override String GetLabel()
        {
            return Name;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.Template = json["Template"]?.GetValue<string>() ?? "";
        }

        public override JsonObject ToJson()
        {
            var familiarJson = base.ToJson();
            familiarJson.Add("Name", this.Name.ToJson());
            familiarJson.Add("Description", this.Description.ToJson());
            familiarJson.Add("Template", this.Template);

            return familiarJson;
        }
    }
}
