﻿using Eos.Models.Tables;
using Eos.Nwn;
using Eos.Nwn.Tlk;
using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using static Eos.Models.JsonUtils;

namespace Eos.Models
{
    public class BaseModelReference
    {
        public BaseModel? ReferenceObject { get; set; }
        public String? ReferenceProperty { get; set; }
        public String? ReferenceName { get; set; }
    }

    public class BaseModel : INotifyPropertyChanged
    {
        private String _hint = "";
        private String? _icon;
        private bool _clearingReferences = false;
        private ModelExtension? _extensions;
        private Dictionary<(BaseModel refObject, String refProperty), BaseModelReference> referenceDict = new Dictionary<(BaseModel refObject, String refProperty), BaseModelReference>();

        private Dictionary<CustomObjectProperty, CustomValueInstance> extensionValueDict = new Dictionary<CustomObjectProperty, CustomValueInstance>();
        public ObservableCollection<CustomValueInstance> ExtensionValues { get; } = new ObservableCollection<CustomValueInstance>();

        public ModelExtension? Extensions
        {
            get { return _extensions; }
            set
            {
                if (_extensions != value)
                {
                    _extensions = value;
                    InitExtensionValues();
                }
            }
        }

        public Guid ID { get; set; }
        public bool Disabled { get; set; }
        public bool IsReadonly { get; set; } = false;
        public Guid? Overrides { get; set; } = null;
        public int? Index { get; set; }
        public int? CalculatedIndex
        {
            get
            {
                var index = MasterRepository.Project.GetBase2DAIndex(this);
                return index;
            }
        }

        public String TypeName => GetTypeName();

        public String? Icon
        {
            get { return _icon; }
            set
            {
                if (value != _icon)
                {
                    _icon = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public String Hint
        {
            get { return _hint; }
            set
            {
                if (value != _hint)
                {
                    _hint = value;
                    NotifyPropertyChanged();
                }
            }
        }

        protected virtual TLKStringSet? GetTlkDisplayName()
        {
            return null;
        }

        public TLKStringSet? TlkDisplayName => GetTlkDisplayName();

        public String DisplayHint
        {
            get
            {
                var selfOverride = MasterRepository.Project.GetOverride(this);
                return selfOverride?.Hint ?? this.Hint;
            }
        }

        protected virtual String GetTypeName()
        {
            return GetType().Name;
        }

        private void InitExtensionValues()
        {
            ExtensionValues.Clear();
            extensionValueDict.Clear();
            if (_extensions != null)
            {
                foreach (var prop in _extensions.Items)
                {
                    if (prop != null)
                    {
                        var valueInstance = new CustomValueInstance(prop);
                        extensionValueDict[prop] = valueInstance;
                        ExtensionValues.Add(valueInstance);
                    }
                }
            }
        }

        public IEnumerable<BaseModelReference> References => referenceDict.Values;
        public int ReferenceCount => referenceDict.Count;

        protected void Set<T>(ref T? reference, T? value, [CallerMemberName] String refProperty = "") where T: BaseModel
        {
            if (reference != value)
            {
                reference?.RemoveReference(this, refProperty);
                reference = value;
                reference?.AddReference(this, refProperty);
            }
        }

        public void AddReference(BaseModel refObject, [CallerMemberName] String refProperty = "")
        {
            referenceDict[(refObject, refProperty)] = new BaseModelReference()
            {
                ReferenceObject = refObject,
                ReferenceProperty = refProperty,
                ReferenceName = refObject.GetLabel(),
            };
        }

        public void RemoveReference(BaseModel refObject, [CallerMemberName] String refProperty = "")
        {
            referenceDict.Remove((refObject, refProperty));
        }

        public BaseModel()
        {
            SetDefaultValues();
        }

        protected virtual void SetDefaultValues()
        {

        }

        public virtual String GetLabel()
        {
            return "";
        }

        protected JsonObject? CreateJsonRef(BaseModel? model)
        {
            if (model == null) return null;

            var result = new JsonObject();
            result.Add("ID", model.ID.ToString());
            result.Add("Label", model.GetLabel());
            return result;
        }

        public JsonObject? ToJsonRef()
        {
            return CreateJsonRef(this);
        }

        public static T CreateFromJson<T>(JsonObject json, ModelExtension extensions) where T : BaseModel, new()
        {
            T result = new T();
            result.Extensions = extensions;
            result.FromJson(json);
            return result;
        }

        public virtual JsonObject ToJson()
        {
            var result = new JsonObject();
            result.Add("ID", this.ID.ToString());
            result.Add("Index", this.Index);
            result.Add("Overrides", this.Overrides != null ? this.Overrides.ToString() : null);
            result.Add("Disabled", this.Disabled);
            result.Add("Hint", this.Hint);

            var extensionValues = new JsonObject();
            foreach (var prop in extensionValueDict.Keys)
            {
                if (!prop.DataType?.IsVisualOnly ?? false)
                    extensionValues.Add(prop.Column, prop.ValueToJson(extensionValueDict[prop].Value));
            }
            result.Add("ExtensionValues", extensionValues);

            return result;
        }

        public virtual void FromJson(JsonObject json)
        {
            this.ID = ParseGuid(json["ID"]?.GetValue<String>());
            this.Index = json["Index"]?.GetValue<int?>();
            this.Overrides = ParseNullableGuid(json["Overrides"]?.GetValue<String>());
            this.Disabled = json["Disabled"]?.GetValue<bool?>() ?? false;
            this.Hint = json["Hint"]?.GetValue<String>() ?? "";

            var extensionValues = json["ExtensionValues"]?.AsObject();
            if (extensionValues != null)
            {
                foreach (var prop in extensionValueDict.Keys)
                {
                    if (!prop.DataType?.IsVisualOnly ?? false)
                        extensionValueDict[prop].Value = prop.ValueFromJson(extensionValues[prop.Column]);
                }
            }
        }

        public M? Resolve<M>(M? modelRef, VirtualModelRepository<M> repository) where M : BaseModel, new()
        {
            if (_clearingReferences) return null;
            return repository.Resolve(modelRef);
        }

        protected BaseModel? ResolveByType(Type modelType, Guid id)
        {
            return MasterRepository.Standard.GetByID(modelType, id) ?? MasterRepository.Project.GetByID(modelType, id);
        }

        public virtual void ResolveReferences()
        {
            foreach (var prop in extensionValueDict.Keys)
            {
                if ((extensionValueDict[prop].Value is VariantValue varValue) && (varValue.Value is BaseModel varModel))
                {
                    if ((varModel is CustomObjectInstance coInstance) && (varValue.DataType?.CustomType is CustomObject template))
                        varValue.Value = MasterRepository.Project.CustomObjectRepositories[template].GetByID(coInstance.ID);
                    else
                        varValue.Value = ResolveByType(varModel.GetType(), varModel.ID);
                }
                else if (extensionValueDict[prop].Value is BaseModel model)
                {
                    if ((model is CustomObjectInstance coInstance) && (prop.DataType?.CustomType is CustomObject template))
                        extensionValueDict[prop].Value = MasterRepository.Project.CustomObjectRepositories[template].GetByID(coInstance.ID);
                    else
                        extensionValueDict[prop].Value = ResolveByType(model.GetType(), model.ID);
                }
            }
        }

        public void ClearReferences()
        {
            _clearingReferences = true;
            try
            {
                ResolveReferences();
            }
            finally
            {
                _clearingReferences = false;
            }
        }

        public virtual BaseModel? Copy(bool addCopyHint = true)
        {
            var result = (BaseModel?)this.GetType()?.GetConstructor(new Type[] { })?.Invoke(new object[] { });
            if (result != null)
            {
                result.Extensions = Extensions;
                result.FromJson(this.ToJson());
                result.ResolveReferences();
                result.ID = Guid.Empty;
                result.Index = null;
                if (addCopyHint)
                {
                    result.Hint = (result.Hint + " Copy").Trim();
                }
                return result;
            }

            return null;
        }

        public virtual BaseModel? Override()
        {
            var result = Copy(false);
            if (result != null)
            {
                result.Index = -1;
                result.Overrides = this.ID;
            }

            return result;
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
}
