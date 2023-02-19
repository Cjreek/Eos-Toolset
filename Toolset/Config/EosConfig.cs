using Eos.Nwn.Tlk;
using Eos.Types;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

using static Eos.Models.JsonUtils;

namespace Eos.Config
{
    public enum TabLayout
    {
        Simple, Multiline
    }

    public static class EosConfig
    {
        

        public static String NwnBasePath { get; private set; } = "";
        public static String LastProject { get; set; } = "";
        public static TabLayout TabLayout { get; set; } = TabLayout.Multiline;

        private static String FindNwnBasePath()
        {
            var nwnEEKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 704450", false);
            if (nwnEEKey != null)
                return (String?)nwnEEKey.GetValue("InstallLocation") ?? "";

            return NwnBasePath;
        }

        public static void Load()
        {
            if (File.Exists(Constants.ConfigPath))
            {
                var fs = new FileStream(Constants.ConfigPath, FileMode.Open, FileAccess.Read);
                try
                {
                    if (JsonNode.Parse(fs) is JsonObject configJson)
                    {
                        NwnBasePath = configJson[nameof(NwnBasePath)]?.GetValue<String>() ?? "";
                        LastProject = configJson[nameof(LastProject)]?.GetValue<String>() ?? "";
                        TabLayout = JsonToEnum<TabLayout>(configJson[nameof(TabLayout)]) ?? TabLayout.Multiline;
                    }
                }
                finally
                {
                    fs.Close();
                }
            }

            if (NwnBasePath == String.Empty)
                NwnBasePath = FindNwnBasePath();
        }

        public static void Save()
        {
            JsonObject configJson = new JsonObject();
            configJson.Add(nameof(NwnBasePath), NwnBasePath);
            configJson.Add(nameof(LastProject), LastProject);
            configJson.Add(nameof(TabLayout), EnumToJson(TabLayout));

            File.WriteAllText(Constants.ConfigPath, configJson.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.General) { WriteIndented = true }));
        }
    }
}
