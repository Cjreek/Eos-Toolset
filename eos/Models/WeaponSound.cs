using Eos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models
{
    public class WeaponSound : BaseModel
    {
        private string _name = "";

        public string Name
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
        public string? Leather0 { get; set; }
        public string? Leather1 { get; set; }
        public string? Chain0 { get; set; }
        public string? Chain1 { get; set; }
        public string? Plate0 { get; set; }
        public string? Plate1 { get; set; }
        public string? Stone0 { get; set; }
        public string? Stone1 { get; set; }
        public string? Wood0 { get; set; }
        public string? Wood1 { get; set; }
        public string? Chitin0 { get; set; }
        public string? Chitin1 { get; set; }
        public string? Scale0 { get; set; }
        public string? Scale1 { get; set; }
        public string? Ethereal0 { get; set; }
        public string? Ethereal1 { get; set; }
        public string? Crystal0 { get; set; }
        public string? Crystal1 { get; set; }
        public string? Miss0 { get; set; }
        public string? Miss1 { get; set; }
        public string? Parry { get; set; }
        public string? Critical { get; set; }

        public override String GetLabel()
        {
            return Name;
        }

        protected override string GetTypeName()
        {
            return "Weapon Sound";
        }

        protected override void SetDefaultValues()
        {
            Name = "New Weapon Sound";
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Name"]?.GetValue<string>() ?? "";
            this.Leather0 = json["Leather0"]?.GetValue<string>();
            this.Leather1 = json["Leather1"]?.GetValue<string>();
            this.Chain0 = json["Chain0"]?.GetValue<string>();
            this.Chain1 = json["Chain1"]?.GetValue<string>();
            this.Plate0 = json["Plate0"]?.GetValue<string>();
            this.Plate1 = json["Plate1"]?.GetValue<string>();
            this.Stone0 = json["Stone0"]?.GetValue<string>();
            this.Stone1 = json["Stone1"]?.GetValue<string>();
            this.Wood0 = json["Wood0"]?.GetValue<string>();
            this.Wood1 = json["Wood1"]?.GetValue<string>();
            this.Chitin0 = json["Chitin0"]?.GetValue<string>();
            this.Chitin1 = json["Chitin1"]?.GetValue<string>();
            this.Scale0 = json["Scale0"]?.GetValue<string>();
            this.Scale1 = json["Scale1"]?.GetValue<string>();
            this.Ethereal0 = json["Ethereal0"]?.GetValue<string>();
            this.Ethereal1 = json["Ethereal1"]?.GetValue<string>();
            this.Crystal0 = json["Crystal0"]?.GetValue<string>();
            this.Crystal1 = json["Crystal1"]?.GetValue<string>();
            this.Miss0 = json["Miss0"]?.GetValue<string>();
            this.Miss1 = json["Miss1"]?.GetValue<string>();
            this.Parry = json["Parry"]?.GetValue<string>();
            this.Critical = json["Critical"]?.GetValue<string>();
        }

        public override JsonObject ToJson()
        {
            var weaponSoundsJson = base.ToJson();
            weaponSoundsJson.Add("Name", this.Name);
            weaponSoundsJson.Add("Leather0", this.Leather0);
            weaponSoundsJson.Add("Leather1", this.Leather1);
            weaponSoundsJson.Add("Chain0", this.Chain0);
            weaponSoundsJson.Add("Chain1", this.Chain1);
            weaponSoundsJson.Add("Plate0", this.Plate0);
            weaponSoundsJson.Add("Plate1", this.Plate1);
            weaponSoundsJson.Add("Stone0", this.Stone0);
            weaponSoundsJson.Add("Stone1", this.Stone1);
            weaponSoundsJson.Add("Wood0", this.Wood0);
            weaponSoundsJson.Add("Wood1", this.Wood1);
            weaponSoundsJson.Add("Chitin0", this.Chitin0);
            weaponSoundsJson.Add("Chitin1", this.Chitin1);
            weaponSoundsJson.Add("Scale0", this.Scale0);
            weaponSoundsJson.Add("Scale1", this.Scale1);
            weaponSoundsJson.Add("Ethereal0", this.Ethereal0);
            weaponSoundsJson.Add("Ethereal1", this.Ethereal1);
            weaponSoundsJson.Add("Crystal0", this.Crystal0);
            weaponSoundsJson.Add("Crystal1", this.Crystal1);
            weaponSoundsJson.Add("Miss0", this.Miss0);
            weaponSoundsJson.Add("Miss1", this.Miss1);
            weaponSoundsJson.Add("Parry", this.Parry);
            weaponSoundsJson.Add("Critical", this.Critical);

            return weaponSoundsJson;
        }
    }
}
