using Eos.Nwn.Tlk;
using Eos.Services;
using Eos.Types;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

    public class RuntimeConfig : INotifyPropertyChanged
    {
        private TLKLanguage currentLanguage;
        private bool currentGender;

        public TLKLanguage CurrentLanguage
        {
            get { return currentLanguage; }
            set
            {
                if (currentLanguage != value)
                {
                    currentLanguage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CurrentGender
        {
            get { return currentGender; }
            set
            {
                if (currentGender != value)
                {
                    currentGender = value;
                    NotifyPropertyChanged();
                }
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

    public static class EosConfig
    {
        public static String NwnBasePath { get; private set; } = "";
        public static DateTime BaseGameDataBuildDate { get; set; }
        public static DateTime CurrentGameBuildDate { get; private set; }
        public static String LastProject { get; set; } = "";
        public static TabLayout TabLayout { get; set; } = TabLayout.Multiline;

        public static RuntimeConfig RuntimeConfig { get; } = new RuntimeConfig();

        public static void OverrideNwnBasePath(string overridePath)
        {
            NwnBasePath = overridePath;
        }

        private static String FindSteamPathByRegistry()
        {
            // TODO: Linux/GoG/Beamdog/etc
            var nwnEEKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 704450", false);
            if (nwnEEKey != null)
                return (String?)nwnEEKey.GetValue("InstallLocation") ?? "";

            return "";
        }

        private static String FindGOGPathByRegistry()
        {
            var gogKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\GOG.com\Games\1097893768", false);
            if (gogKey != null)
                return (String?)gogKey.GetValue("path") ?? "";

            return "";
        }

        private static String FindNwnBasePath()
        {
            string path;
            
            path = Environment.GetEnvironmentVariable("NWN_ROOT") ?? "";
            if ((path != "") && (Directory.Exists(path)))
            {
                Log.Info("Found installation via NWN_ROOT at: {0}", path);
                return path;
            }

            // Steam
            var steamPath = Path.Combine("Steam", "steamapps", "common", "Neverwinter Nights");
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                path = FindSteamPathByRegistry();
                if (path == "")
                    path = Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") ?? "", steamPath);

                if (Directory.Exists(Path.Combine(path, "data")))
                {
                    Log.Info("Found Steam installation at: {0}", path);
                    return path;
                }
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var home = Environment.GetEnvironmentVariable("HOME") ?? "";
                path = Path.Combine(home, ".local", "share", steamPath);

                if (Directory.Exists(Path.Combine(path, "data")))
                {
                    Log.Info("Found Steam installation at: {0}", path);
                    return path;
                }
            }
            else if (Environment.OSVersion.Platform == PlatformID.Other)
            {
                var home = Environment.GetEnvironmentVariable("HOME") ?? "";
                path = Path.Combine(home, "Library", "Application Support", steamPath);

                if (Directory.Exists(Path.Combine(path, "data")))
                {
                    Log.Info("Found Steam installation at: {0}", path);
                    return path;
                }
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
                Log.Info("Found Beamdog settings.json at: {0}", path);

                var fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.Read);
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
                                {
                                    Log.Info("Found Beamdog installation at: {0}", release00829);
                                    return release00829;
                                }

                                var release00785 = Path.Combine(folderStr, "00785");
                                if (Directory.Exists(Path.Combine(release00785, "data")))
                                {
                                    Log.Info("Found Beamdog installation at: {0}", release00785);
                                    return release00785;
                                }
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
                {
                    Log.Info("Found GOG installation at: {0}", path);
                    return path;
                }
            }
            else
            {
                var home = Environment.GetEnvironmentVariable("HOME") ?? "";
                path = Path.Combine(home, "GOG Games", "Neverwinter Nights Enhanced Edition");

                if (Directory.Exists(Path.Combine(path, "data")))
                {
                    Log.Info("Found GOG installation at: {0}", path);
                    return path;
                }
            }

            return "";
        }

        public static DateTime GetGameBuildDate(string nwnBasePath)
        {
            Log.Info("Detecting game build date...");

            DateTime buildDate = DateTime.MinValue;
            try
            {
                var buildTxtContents = File.ReadAllLines(Path.Combine(nwnBasePath, "bin", "win32", "build.txt"));
                if (buildTxtContents.Length > 1)
                {
                    var buildDateStr = Regex.Replace(buildTxtContents[1], " {2,}", " ");

                    var match = Regex.Match(buildDateStr, "((Mon|Tue|Wed|Thu|Fri|Sat|Sun) *(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) *[0-9]{1,2} *[0-9]{2}:[0-9]{2}:[0-9]{2}) *(\\w+) *([0-9]{4})");
                    if (match.Success)
                    {
                        buildDateStr = match.Groups[1].Value + " " + match.Groups[5].Value;
                        buildDate = DateTime.ParseExact(buildDateStr, "ddd MMM d HH:mm:ss yyyy", new CultureInfo("en-US"));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            if (buildDate == DateTime.MinValue)
                Log.Warning("No valid build date found! This might create problems when updating projects.");
            else
                Log.Info("Build date found: {0}", buildDate);

            return buildDate;
        }

        public static void Load()
        {
            Log.Info("Loading config from \"{0}\"", Constants.ConfigPath);
            if (File.Exists(Constants.ConfigPath))
            {
                try
                {
                    var fs = new FileStream(Constants.ConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
                catch (Exception e)
                {
                    Log.Error(e.Message);
                    throw;
                }
            }

            if (NwnBasePath == String.Empty)
            {
                Log.Info("NwnBasePath is empty -> Searching for NWN installation...");
                NwnBasePath = FindNwnBasePath();
            }

            CurrentGameBuildDate = GetGameBuildDate(NwnBasePath);

            Log.Info("Config loaded successfully!");
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
