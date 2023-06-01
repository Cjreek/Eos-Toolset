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
    public class MasterFeat : BaseModel
    {
        public TLKStringSet Name { get; set; } = new TLKStringSet();
        public TLKStringSet Description { get; set; } = new TLKStringSet();

        protected override void Initialize()
        {
            base.Initialize();
            Name = new TLKStringSet(() => NotifyPropertyChanged(nameof(Name)));
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (MasterFeat?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.Name ?? this.Name;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override string GetTypeName()
        {
            return "Master Feat";
        }

        protected override void SetDefaultValues()
        {
            Name[MasterRepository.Project.DefaultLanguage].Text = "New Masterfeat";
            Name[MasterRepository.Project.DefaultLanguage].TextF = "New Masterfeat";
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name.FromJson(json["Name"]?.AsObject());
            this.Description.FromJson(json["Description"]?.AsObject());
            this.Icon = json["Icon"]?.GetValue<String>();
           
        }

        public override JsonObject ToJson()
        {
            var masterFeatJson = base.ToJson();
            masterFeatJson.Add("Name", this.Name.ToJson());
            masterFeatJson.Add("Description", this.Description.ToJson());
            masterFeatJson.Add("Icon", this.Icon);

            return masterFeatJson;
        }
    }
}
