using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Eos.Models
{
    public static class JsonUtils
    {
        public static bool EnumIsFlags<T>() where T : struct, Enum
        {
            return typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Any();
        }

        public static JsonNode? EnumToJson<T>(T value) where T : struct, Enum
        {
            return EnumToJson((T?)value);
        }

        public static JsonNode? EnumToJson<T>(T? value) where T : struct, Enum
        {
            if (EnumIsFlags<T>())
            {
                var flagArr = new JsonArray();
                if (Convert.ToInt32(value) != 0)
                {
                    foreach (var flag in Enum.GetValues<T>())
                    {
                        if ((Convert.ToInt32(value) & Convert.ToInt32(flag)) != 0)
                            flagArr.Add(Enum.GetName(flag));
                    }
                }

                return flagArr;
            }

            return Enum.GetName(value ?? default(T));
        }

        public static T? JsonToEnum<T>(JsonNode? node) where T : struct, Enum
        {
            if (node == null) return null;

            if ((node is JsonArray arr) && (EnumIsFlags<T>()))
            {
                Int32 result = 0;
                for (int i = 0; i < arr.Count; i++)
                    result = result | Convert.ToInt32(Enum.Parse<T>(arr[i]?.GetValue<String>() ?? ""));

                return (T)Enum.ToObject(typeof(T), result);
            }

            return Enum.Parse<T>(node.GetValue<String>());

        }

        public static Guid ParseGuid(String? value)
        {
            if (value == null) return Guid.Empty;
            return Guid.Parse(value);
        }

        public static T CreateRef<T>(Guid guid) where T : BaseModel, new()
        {
            T result = new T();
            result.ID = guid;
            return result;
        }

        public static T? CreateRefFromJson<T>(JsonObject? jsonRef) where T : BaseModel, new()
        {
            if (jsonRef == null) return default(T);

            return CreateRef<T>(ParseGuid(jsonRef["ID"]?.GetValue<String>()));
        }
    }
}
