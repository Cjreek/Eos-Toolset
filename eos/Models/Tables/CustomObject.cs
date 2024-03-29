﻿using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eos.Models.Tables
{
    public class CustomObject : BaseTable<CustomObjectProperty>
    {
        private DataTypeDefinition dataTypeDefinition = new DataTypeDefinition(Guid.Empty, String.Empty, null);

        public DataTypeDefinition DataTypeDefinition
        {
            get
            {
                if (dataTypeDefinition.ID != ID)
                {
                    dataTypeDefinition = new DataTypeDefinition(ID, Name, this, true);
                    dataTypeDefinition.ToJson = o => ((CustomObjectInstance?)o)?.ToJsonRef();
                    dataTypeDefinition.To2DA = (o, _, _) => MasterRepository.Project.CustomObjectRepositories[this].Get2DAIndex((CustomObjectInstance?)o);
                    dataTypeDefinition.FromJson = json => JsonUtils.CreateRefFromJson<CustomObjectInstance>((JsonObject?)json);
                }
                return dataTypeDefinition;
            }
        }

        public ModelRepository<CustomObjectInstance> InstanceRepository { get; set; }

        public event EventHandler? OnChanged;

        public String ResourceName { get; set; } = "";

        public CustomObject() : base()
        {
            InstanceRepository = MasterRepository.Project.CustomObjectRepositories[this];
            InstanceRepository.CollectionChanged += InstanceRepository_CollectionChanged;
        }

        ~CustomObject()
        {
            InstanceRepository.CollectionChanged -= InstanceRepository_CollectionChanged;
        }

        private void InstanceRepository_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Delay, because otherwise bindings won't update immediately
            Task.Delay(5).ContinueWith(t => NotifyPropertyChanged(nameof(InstanceRepository)));

            // Suddenly works ??
            //if ((e.Action == NotifyCollectionChangedAction.Reset) || (e.Action == NotifyCollectionChangedAction.Move))
            //    NotifyPropertyChanged(nameof(InstanceRepository));
        }

        protected override void SetDefaultValues()
        {
            Name = "NewObject";
        }

        public override JsonObject ToJson()
        {
            var json = base.ToJson();
            json.Add("ResourceName", this.ResourceName);
            return json;
        }

        public override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.ResourceName = json["ResourceName"]?.GetValue<String>() ?? "";
        }

        protected override void Changed()
        {
            OnChanged?.Invoke(this, new EventArgs());
        }
    }
}
