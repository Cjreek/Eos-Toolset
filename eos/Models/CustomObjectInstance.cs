using Eos.Models.Tables;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class CustomValueInstance : INotifyPropertyChanged
    {
        private object? _value;

        public CustomObjectProperty Property { get; set; }
        public object? Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    if (_value is VariantValue oldVarValue)
                        oldVarValue.PropertyChanged -= VarValue_PropertyChanged;
                    _value = value;
                    if (_value is VariantValue newVarValue)
                        newVarValue.PropertyChanged += VarValue_PropertyChanged;

                    NotifyPropertyChanged();
                }
            }
        }

        private void VarValue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Value));
        }

        public CustomValueInstance(CustomObjectProperty property, object? value = null)
        {
            Property = property;
            Value = value ?? property.DataType?.GetDefaultValue();

            Property.PropertyChanged += Property_PropertyChanged;
        }

        private void Property_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Property.DataType))
            {
                Value = null ?? Property.DataType?.GetDefaultValue();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class CustomObjectInstance : BaseModel
    {
        private CustomObject? _template;
        private String _name = "";
        private Dictionary<CustomObjectProperty, CustomValueInstance> valueDict = new Dictionary<CustomObjectProperty, CustomValueInstance>();

        public CustomObject? Template
        {
            get { return _template; }
            set 
            { 
                if (_template != null)
                    _template.OnChanged -= _template_OnChanged;
                Set(ref _template, value);
                if (_template != null)
                    _template.OnChanged += _template_OnChanged;

                InitValueDictionary();
            }
        }

        private void _template_OnChanged(object? sender, EventArgs e)
        {
            InitValueDictionary();
        }

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

        public ObservableCollection<CustomValueInstance> Values { get; } = new ObservableCollection<CustomValueInstance>();

        private void InitValueDictionary()
        {
            if (_template == null)
            {
                Values.Clear();
            }
            else
            {
                // Remove missing values
                foreach (var prop in valueDict.Keys)
                {
                    if ((prop != null) && (!_template.Contains(prop)))
                    {
                        Values.Remove(valueDict[prop]);
                        valueDict.Remove(prop);
                    }
                }

                // Add new values
                foreach (var prop in _template.Items)
                {
                    if (prop != null)
                    {
                        if (!valueDict.ContainsKey(prop))
                        {
                            var valueInstance = new CustomValueInstance(prop);
                            valueDict[prop] = valueInstance;
                            Values.Add(valueInstance);
                        }
                    }
                }

                // Reorder
                for (int i = 0; i < _template.Count; i++)
                {
                    var prop = _template[i];
                    if (prop != null)
                    {
                        var custValue = Values.First(val => val.Property == prop);
                        var index = Values.IndexOf(custValue);
                        Values.Move(index, i);
                    }
                }
            }
        }

        protected override void SetDefaultValues()
        {
            if (Template != null)
                Name = "New " + Template.Name;
            else
                Name = "New Object";
        }

        public override String GetLabel()
        {
            return Name;
        }

        public override void ResolveReferences()
        {
            foreach (var prop in valueDict.Keys)
            {
                if ((valueDict[prop].Value is VariantValue varValue) && (varValue.Value is BaseModel varModel))
                {
                    if ((varModel is CustomObjectInstance coInstance) && (varValue.DataType?.CustomType is CustomObject template))
                        varValue.Value = MasterRepository.Project.CustomObjectRepositories[template].GetByID(coInstance.ID);
                    else if ((varModel is CustomTableInstance ctInstance) && (varValue.DataType?.CustomType is CustomTable tableTemplate))
                        varValue.Value = MasterRepository.Project.CustomTableRepositories[tableTemplate].GetByID(ctInstance.ID);
                    else
                        varValue.Value = ResolveByType(varModel.GetType(), varModel.ID);
                }
                else if (valueDict[prop].Value is BaseModel model)
                {
                    if ((model is CustomObjectInstance coInstance) && (prop.DataType?.CustomType is CustomObject template))
                        valueDict[prop].Value = MasterRepository.Project.CustomObjectRepositories[template].GetByID(coInstance.ID);
                    else if ((model is CustomTableInstance ctInstance) && (prop.DataType?.CustomType is CustomTable tableTemplate))
                        valueDict[prop].Value = MasterRepository.Project.CustomTableRepositories[tableTemplate].GetByID(ctInstance.ID);
                    else
                        valueDict[prop].Value = ResolveByType(model.GetType(), model.ID);
                }
            }
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Name = json["Label"]?.GetValue<String>() ?? "";
            this.Template = MasterRepository.CustomObjects.GetByID(ParseGuid(json["Template"]?["ID"]?.GetValue<String>()));
            foreach (var prop in valueDict.Keys)
            {
                if (!prop.DataType?.IsVisualOnly ?? false)
                    valueDict[prop].Value = prop.ValueFromJson(json[prop.Column]);
            }
        }

        public override JsonObject ToJson()
        {
            var customObjectJson = base.ToJson();
            customObjectJson.Add("Label", this.Name);
            customObjectJson.Add("Template", CreateJsonRef(this.Template));
            foreach (var prop in valueDict.Keys)
            {
                if (!prop.DataType?.IsVisualOnly ?? false)
                    customObjectJson.Add(prop.Column, prop.ValueToJson(valueDict[prop].Value));
            }

            return customObjectJson;
        }
    }
}
