using Eos.Nwn.Tlk;
using Eos.Types;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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
        public static DateTime BaseGameDataBuildDate { get; set; }
        public static DateTime CurrentGameBuildDate { get; private set; }
        public static String LastProject { get; set; } = "";
        public static TabLayout TabLayout { get; set; } = TabLayout.Multiline;

        private static String FindSteamPathByRegistry()
        {
            // TODO: Linux/GoG/Beamdog/etc
            var nwnEEKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 704450", false);
            if (nwnEEKey != null)
                return (String?)nwnEEKey.GetValue("InstallLocation") ?? "";

            return NwnBasePath;
        }

        private static String FindGOGPathByRegistry()
        {
            // TODO: Linux/GoG/Beamdog/etc
            var nwnEEKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 704450", false);
            if (nwnEEKey != null)
                return (String?)nwnEEKey.GetValue("InstallLocation") ?? "";

            return NwnBasePath;
        }

        private static String FindNwnBasePath()
        {
            string path;
            
            path = Environment.GetEnvironmentVariable("NWN_ROOT") ?? "";
            if ((path != "") && (Directory.Exists(path))) return path;

            // Steam
            var steamPath = Path.Combine("Steam", "steamapps", "common", "Neverwinter Nights");
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                path = FindSteamPathByRegistry();
                if (path == "")
                    path = Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") ?? "", steamPath);

                if (Directory.Exists(Path.Combine(path, "data")))
                    return path;
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var home = Environment.GetEnvironmentVariable("HOME") ?? "";
                path = Path.Combine(home, ".local", "share", steamPath);

                if (Directory.Exists(Path.Combine(path, "data")))
                    return path;
            }
            else if (Environment.OSVersion.Platform == PlatformID.Other)
            {
                var home = Environment.GetEnvironmentVariable("HOME") ?? "";
                path = Path.Combine(home, "Library", "Application Support", steamPath);

                if (Directory.Exists(Path.Combine(path, "data")))
                    return path;
            }

            // Beamdog
            var beamdogSettings = Path.Combine("Beamdog Client", "settings.json");
            var settingsFile = "";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                var home = Environment.GetEnvironmentVariable("APPDATA") ?? "";
                settingsFile = Path.Combine(home, beamdogSettings);
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var home = Environment.GetEnvironmentVariable("HOME") ?? "";
                settingsFile = Path.Combine(home, ".config", beamdogSettings);
            }
            else if (Environment.OSVersion.Platform == PlatformID.Other)
            {
                var home = Environment.GetEnvironmentVariable("HOME") ?? "";
                settingsFile = Path.Combine(home, "Library", "Application Support", beamdogSettings);
            }

            if ((settingsFile != "") && (File.Exists(settingsFile)))
            {
                var fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read);
                try
                {
                    if (JsonNode.Parse(fs) is JsonObject settingsRoot)
                    {
                        if (settingsRoot.ContainsKey("folders") && settingsRoot["folders"] is JsonArray folderArray)
                        {
                            foreach (var folder in folderArray)
                            {
                                if (folder == null) continue;
                                var folderStr = folder.GetValue<String>() ?? "";

                                var release00829 = Path.Combine(folderStr, "00829");
                                if (Directory.Exists(Path.Combine(release00829, "data")))
                                    return release00829;

                                var release00785 = Path.Combine(folderStr, "00785");
                                if (Directory.Exists(Path.Combine(release00785, "data")))
                                    return release00785;
                            }
                        }
                    }
                }
                finally
                {
                    fs.Close();
                }
            }

            // GOG
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                path = FindGOGPathByRegistry();
                if (path == "")
                    path = Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") ?? "", "GOG Galaxy", "Games", "Neverwinter Nights Enhanced Edition");

                if (Directory.Exists(Path.Combine(path, "data")))
                    return path;
            }
            else
            {
                var home = Environment.GetEnvironmentVariable("HOME") ?? "";
                path = Path.Combine(home, "GOG Games", "Neverwinter Nights Enhanced Edition");

                if (Directory.Exists(Path.Combine(path, "data")))
                    return path;
            }

            return "";
        }

        private static DateTime GetGameBuildDate()
        {
            DateTime buildDate = DateTime.MinValue;

            var buildTxtContents = File.ReadAllLines(Path.Combine(NwnBasePath, "bin", "win32", "build.txt"));
            if (buildTxtContents.Length > 1)
            {
                var buildDateStr = buildTxtContents[1];

                var match = Regex.Match(buildDateStr, "((Mon|Tue|Wed|Thu|Fri|Sat|Sun) *(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) *[0-9]{1,2} *[0-9]{2}:[0-9]{2}:[0-9]{2}) *(\\w+) *([0-9]{4})");
                if (match.Success)
                {
                    buildDateStr = match.Groups[1].Value + " " + match.Groups[5].Value;
                    buildDate = DateTime.ParseExact(buildDateStr, "ddd MMM d HH:mm:ss yyyy", new CultureInfo("en-US"));
                }
            }

            return buildDate;
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
                        BaseGameDataBuildDate = configJson[nameof(BaseGameDataBuildDate)]?.GetValue<DateTime>() ?? DateTime.MinValue;
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

            CurrentGameBuildDate = GetGameBuildDate();
        }

        public static void Save()
        {
            JsonObject configJson = new JsonObject();
            configJson.Add(nameof(NwnBasePath), NwnBasePath);
            configJson.Add(nameof(BaseGameDataBuildDate), BaseGameDataBuildDate);
            configJson.Add(nameof(LastProject), LastProject);
            configJson.Add(nameof(TabLayout), EnumToJson(TabLayout));

            File.WriteAllText(Constants.ConfigPath, configJson.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.General) { WriteIndented = true }));
        }
    }
}
