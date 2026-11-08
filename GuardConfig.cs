using System;
using System.Collections.Generic;
using System.IO;

namespace NAVI
{
    /// <summary>Allow = warn and let the user choose. Block = warn and always cancel the import.</summary>
    public enum GuardMode { Allow, Block }

    public enum ThemeMode { Auto, Light, Dark }

    /// <summary>
    ///  Config file rules of the NAVI guided CAD import blocker.
    ///  One file for all years of Revit:
    ///  C:\ProgramData\Autodesk\Revit\Addins\NAVI\CadImportGuard.ini
    /// </summary>
    public sealed class GuardConfig
    {
        public static readonly string ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Autodesk", "Revit", "Addins", "NAVI");
        public static readonly string ConfigPath = Path.Combine(ConfigFolder, "CadImportGuard.ini");

        public bool Enabled { get; private set; } = true;
        public GuardMode Mode { get; private set; } = GuardMode.Allow;
        public bool ApplyInProjects { get; private set; } = true;
        public bool ApplyInFamilies { get; private set; } = false;
        public bool ExemptAdtMembers { get; private set; } = true;
        public bool IsAdtMember { get; private set; } = false;
        public ThemeMode Theme { get; private set; } = ThemeMode.Auto;

        /// <summary>True when the guard should hook the Import CAD command at all.</summary>
        public bool ShouldHook =>
            Enabled
            && (ApplyInProjects || ApplyInFamilies)
            && !(ExemptAdtMembers && IsAdtMember);

        public bool AppliesTo(bool isFamilyDocument) =>
            isFamilyDocument ? ApplyInFamilies : ApplyInProjects;

        public static GuardConfig Load()
        {
            var config = new GuardConfig();
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    TryWriteDefault();
                    return config;
                }

                var values = Parse(File.ReadAllLines(ConfigPath));
                config.Enabled = GetBool(values, "Enabled", config.Enabled);
                config.Mode = GetEnum(values, "Mode", config.Mode);
                // AllowImport = False is the plain switch for Block; it wins over Mode when both are present.
                if (values.ContainsKey("AllowImport"))
                    config.Mode = GetBool(values, "AllowImport", true) ? GuardMode.Allow : GuardMode.Block;
                config.ApplyInProjects = GetBool(values, "ApplyInProjects", config.ApplyInProjects);
                config.ApplyInFamilies = GetBool(values, "ApplyInFamilies", config.ApplyInFamilies);
                config.ExemptAdtMembers = GetBool(values, "ExemptAdtMembers", config.ExemptAdtMembers);
                // "ADT Memeber" is the spelling the old BergmannCEDRevitToolsConfig.txt used.
                config.IsAdtMember = GetBool(values, "ADT Member", GetBool(values, "ADT Memeber", config.IsAdtMember));
                config.Theme = GetEnum(values, "Theme", config.Theme);
            }
            catch
            {
                // Unreadable config: fall back to defaults rather than failing Revit startup.
            }
            return config;
        }

        static Dictionary<string, string> Parse(IEnumerable<string> lines)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == '#' || line[0] == ';' || line[0] == '[')
                    continue;

                int split = line.IndexOf('=');
                if (split <= 0)
                    continue;

                string value = line.Substring(split + 1);
                int comment = value.IndexOfAny(new[] { '#', ';' });
                if (comment >= 0)
                    value = value.Substring(0, comment);

                values[line.Substring(0, split).Trim()] = value.Trim();
            }
            return values;
        }

        static bool GetBool(Dictionary<string, string> values, string key, bool fallback)
        {
            if (!values.TryGetValue(key, out string value))
                return fallback;

            switch (value.ToLowerInvariant())
            {
                case "true": case "yes": case "on": case "1": return true;
                case "false": case "no": case "off": case "0": return false;
                default: return fallback;
            }
        }

        static T GetEnum<T>(Dictionary<string, string> values, string key, T fallback) where T : struct =>
            values.TryGetValue(key, out string value) && Enum.TryParse(value, true, out T parsed)
                ? parsed
                : fallback;

        static void TryWriteDefault()
        {
            try
            {
                Directory.CreateDirectory(ConfigFolder);
                File.WriteAllText(ConfigPath, DefaultFileText);
            }
            catch
            {
                // Users without write access to ProgramData just run on defaults.
            }
        }

        const string DefaultFileText =
@"# CAD Import Guard - shared by Revit 2020-2027. Restart Revit after editing.

# Master switch. False = the Import CAD command is not hooked at all.
Enabled = True

# True  = warn and let the user choose to import anyway.
# False = warn and always cancel the import (user is offered Link CAD instead).
AllowImport = True

# Where the guard runs.
ApplyInProjects = True
ApplyInFamilies = False

# ADT members skip the guard entirely when ExemptAdtMembers = True.
ExemptAdtMembers = True
ADT Member = False

# Auto follows Revit's UI theme (2024+). Light or Dark forces it (useful on 2020-2023).
Theme = Auto
";
    }
}
