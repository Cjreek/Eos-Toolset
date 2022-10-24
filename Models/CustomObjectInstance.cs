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
            Value = value;
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
        private String _label = "";
        private Dictionary<CustomObjectProperty, CustomValueInstance> valueDict = new Dictionary<CustomObjectProperty, CustomValueInstance>();

        public CustomObject? Template
        {
            get { return _template; }
            set 
            { 
                Set(ref _template, value);
                InitValueDictionary();
            }
        }

        public String Label
        {
            get { return _label; }
            set
            {
                if (_label != value)
                {
                    _label = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<CustomValueInstance> Values { get; } = new ObservableCollection<CustomValueInstance>();

        private void InitValueDictionary()
        {
            Values.Clear();
            if (_template != null)
            {
                foreach (var prop in _template.Items)
                {
                    if (prop != null)
                    {
                        var valueInstance = new CustomValueInstance(prop);
                        valueDict[prop] = valueInstance;
                        Values.Add(valueInstance);
                    }
                }
            }
        }

        protected override void SetDefaultValues()
        {
            if (Template != null)
                Label = "New " + Template.Name;
            else
                Label = "New Object";
        }

        protected override String GetLabel()
        {
            return Label;
        }

        private BaseModel? ResolveByType(Type modelType, Guid id)
        {
            return MasterRepository.Standard.GetByID(modelType, id) ?? MasterRepository.Project.GetByID(modelType, id);
        }

        public override void ResolveReferences()
        {
            foreach (var prop in valueDict.Keys)
            {
                if ((valueDict[prop].Value is VariantValue varValue) && (varValue.Value is BaseModel varModel))
                {
                    if ((varModel is CustomObjectInstance coInstance) && (varValue.DataType?.CustomType is CustomObject template))
                        varValue.Value = MasterRepository.Project.CustomObjectRepositories[template].GetByID(coInstance.ID);
                    else
                        varValue.Value = ResolveByType(varModel.GetType(), varModel.ID);
                }
                else if (valueDict[prop].Value is BaseModel model)
                {
                    if ((model is CustomObjectInstance coInstance) && (prop.DataType?.CustomType is CustomObject template))
                        valueDict[prop].Value = MasterRepository.Project.CustomObjectRepositories[template].GetByID(coInstance.ID);
                    else
                        valueDict[prop].Value = ResolveByType(model.GetType(), model.ID);
                }
            }
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.Label = json["Label"]?.GetValue<String>() ?? "";
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
            customObjectJson.Add("Label", this.Label);
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
