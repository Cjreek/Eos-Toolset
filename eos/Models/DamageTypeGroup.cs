using Eos.Nwn.Tlk;
using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eos.Models
{
    public class DamageTypeGroup : BaseModel
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

        public TLKStringSet FeedbackText { get; set; } = new TLKStringSet();
        public uint? Color { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
            FeedbackText = new TLKStringSet(() => NotifyPropertyChanged(nameof(FeedbackText)));
        }

        protected override TLKStringSet? GetTlkDisplayName()
        {
            var modelOverride = (DamageTypeGroup?)MasterRepository.Project.GetOverride(this);
            return modelOverride?.FeedbackText ?? this.FeedbackText;
        }

        public override String GetLabel()
        {
            return Name;
        }

        protected override void SetDefaultValues()
        {
            Name = "New Damage Type Group";
            FeedbackText[MasterRepository.Project.DefaultLanguage].Text = "<CUSTOM0> New Damage Type Group";
            FeedbackText[MasterRepository.Project.DefaultLanguage].TextF = "<CUSTOM0> New Damage Type Group";
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<string>() ?? "";
            this.FeedbackText.FromJson(json["FeedbackText"]?.AsObject());
            this.Color = json["Color"]?.GetValue<uint>() ?? 0;
        }

        public override JsonObject ToJson()
        {
            var damageTypeJson = base.ToJson();
            damageTypeJson.Add("Name", this.Name);
            damageTypeJson.Add("FeedbackText", this.FeedbackText.ToJson());
            damageTypeJson.Add("Color", this.Color);

            return damageTypeJson;
        }
    }
}
