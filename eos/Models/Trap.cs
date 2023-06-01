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
    public class Trap : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public string TrapScript { get; set; } = "";
        public int SetDC { get; set; }
        public int DetectDC { get; set; }
        public int DisarmDC { get; set; }
        public string BlueprintResRef { get; set; } = "";

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
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Trap";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Trap";
        }

        public override String GetLabel()
        {
            return Name;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.TrapScript = json["TrapScript"]?.GetValue<string>() ?? "";
            this.SetDC = json["SetDC"]?.GetValue<int>() ?? 10;
            this.DetectDC = json["DetectDC"]?.GetValue<int>() ?? 10;
            this.DisarmDC = json["DisarmDC"]?.GetValue<int>() ?? 10;
            this.BlueprintResRef = json["BlueprintResRef"]?.GetValue<string>() ?? "";
            this.Icon = json["Icon"]?.GetValue<string>();
        }

        public override JsonObject ToJson()
        {
            var trapJson = base.ToJson();
            trapJson.Add("Name", this.Name.ToJson());
            trapJson.Add("TrapScript", this.TrapScript);
            trapJson.Add("SetDC", this.SetDC);
            trapJson.Add("DetectDC", this.DetectDC);
            trapJson.Add("DisarmDC", this.DisarmDC);
            trapJson.Add("BlueprintResRef", this.BlueprintResRef);
            trapJson.Add("Icon", this.Icon);

            return trapJson;
        }
    }
}
