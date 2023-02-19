using Eos.Repositories;
using Eos.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models.Tables
{
    public class CustomEnum : BaseTable<CustomEnumItem>
    {
        private DataTypeDefinition dataTypeDefinition = new DataTypeDefinition(Guid.Empty, String.Empty, null);

        public DataTypeDefinition DataTypeDefinition
        {
            get
            {
                if (dataTypeDefinition.ID != ID)
                {
                    dataTypeDefinition = new DataTypeDefinition(ID, Name, this, true);
                    dataTypeDefinition.ToJson = o => ((CustomEnumItem?)o)?.Value;
                    dataTypeDefinition.To2DA = o => ((CustomEnumItem?)o)?.Value;
                    dataTypeDefinition.FromJson = json => GetByValue(json?.GetValue<String>());
                }

                return dataTypeDefinition;
            }
        }

        public CustomEnumItem? GetByValue(String? value)
        {
            foreach (var enumItem in Items)
            {
                if (enumItem?.Value.ToLower() == value?.ToLower())
                    return enumItem;
            }

            return null;
        }

        protected override void SetDefaultValues()
        {
            Name = "NewEnum";
        }
    }
}
