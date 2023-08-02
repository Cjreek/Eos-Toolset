using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models.Tables
{
    public class CustomTableInstanceItem : TableItem
    {
        private CustomTable? _template;
        private Dictionary<CustomObjectProperty, CustomValueInstance> valueDict = new Dictionary<CustomObjectProperty, CustomValueInstance>();

        public CustomTableInstanceItem() : base()
        {
        }

        public CustomTableInstanceItem(CustomTableInstance parentTable) : base(parentTable)
        {
            
        }

        public ObservableCollection<CustomValueInstance> Values { get; } = new ObservableCollection<CustomValueInstance>();

        public CustomTable? Template
        {
            get { return _template; }
            set
            {
                if (_template != value)
                {
                    if (_template != null)
                        _template.OnChanged -= _template_OnChanged;
                    Set(ref _template, value);
                    if (_template != null)
                        _template.OnChanged += _template_OnChanged;

                    InitValueDictionary();
                }
            }
        }

        private void _template_OnChanged(object? sender, EventArgs e)
        {
            InitValueDictionary();
        }

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

        public CustomValueInstance? GetPropertyValue(CustomObjectProperty property)
        {
            if (!valueDict.TryGetValue(property, out var value))
                return null;

            return value;
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

            this.Template = ((CustomTableInstance?)ParentTable)?.Template;
            foreach (var prop in valueDict.Keys)
            {
                if (!prop.DataType?.IsVisualOnly ?? false)
                    valueDict[prop].Value = prop.ValueFromJson(json[prop.Column]);
            }
        }

        public override JsonObject ToJson()
        {
            var customObjectJson = base.ToJson();

            foreach (var prop in valueDict.Keys)
            {
                if (!prop.DataType?.IsVisualOnly ?? false)
                    customObjectJson.Add(prop.Column, prop.ValueToJson(valueDict[prop].Value));
            }

            return customObjectJson;
        }
    }
}
