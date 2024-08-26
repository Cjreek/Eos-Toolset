using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
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
    public class RangedDamageType : BaseModel
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

        protected override void SetDefaultValues()
        {
            Name = "New Ranged Damagetype";
            Name = "New Ranged Damagetype";
        }

        public override String GetLabel()
        {
            return Name;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            Name = json["Name"]?.GetValue<String>() ?? "";
        }

        public override JsonObject ToJson()
        {
            var rangedDamageTypeJson = base.ToJson();
            rangedDamageTypeJson.Add("Name", this.Name);

            return rangedDamageTypeJson;
        }
    }
}
