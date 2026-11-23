using System.IO;
using System.Linq;
using Unity.Pipeline.Commands;
using Unity.Pipeline.Editor.Authoring;
using UnityEditor;

namespace OxGKit.LoggingSystem.Editor.UnityCli
{
    /// <summary>
    /// Unity CLI commands of OxGKit.LoggingSystem (unity command oxgkit.logging ...).
    /// <para>Compiled only when the Unity Pipeline package (com.unity.pipeline) is installed.</para>
    /// </summary>
    internal static class LoggingSystemCliCommands
    {
        private const string Group = "oxgkit.logging";

        private const string _SETTINGS_MENU = "Assets/Create/OxGKit/Create Settings/Create Logging Settings in Resources";

        [CliGroupCommand(Group,
            "Read the logger config file in StreamingAssets (master switches and every logger entry).",
            Tags = new[] { "observability" },
            Effect = CommandEffect.ReadOnly)]
        internal static ExitCode GetConfig(CommandContext cmd)
        {
            if (!_TryReadConfig(cmd, out LoggersConfig config, out ConfigFileType fileType, out ExitCode failure))
                return failure;

            _WriteConfigResult(cmd, config, fileType);
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Create the logger config file in StreamingAssets from every logger found in the app domain, each with its default settings (disabled, all levels); same result as the menus 'Assets/Create/OxGKit/Logging System/[JSON - Plaintext] / [BYTES - Cipher] Create LoggerConfig.dat (In StreamingAssets)'. " +
            "An existing config file is rebuilt from scratch, so its settings are lost: that requires confirm=true (use dry-run to preview). To keep existing settings use reload_config instead. Not undoable via Ctrl+Z.",
            Tags = new[] { "observability" },
            Effect = CommandEffect.Destructive)]
        internal static ExitCode CreateConfig(
            CommandContext cmd,
            [CliArg("file-type", "Json (plaintext, easy to inspect) or Bytes (cipher, for releases).", Required = true)] ConfigFileType fileType,
            [CliArg("confirm", "Rebuild an existing config file. Without it the call is refused when the file exists.")] bool confirm = false,
            [CliArg("dry-run", "Preview what would happen without writing anything.")] bool dryRun = false)
        {
            string path = LoggingHelper.GetConfigFilePath();
            bool exists = File.Exists(path);

            if (dryRun)
            {
                cmd.Result.Write(new { status = "dry_run", path = _Normalize(path), fileType = fileType.ToString(), wouldOverwrite = exists });
                return ExitCode.Success;
            }

            if (exists && !confirm)
                return CliHelper.Fail(cmd, ExitCode.InvalidUsage, $"'{_Normalize(path)}' already exists and would be rebuilt with default settings. Pass confirm=true to apply, dry-run=true to preview, or use reload_config to keep the existing settings.");

            LoggersConfig config = LoggingHelper.CreateConfig(fileType);

            _WriteConfigResult(cmd, config, fileType);
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Update the logger config file while keeping its settings: adds the loggers that are new in the app domain (with default settings) and removes the entries of loggers that no longer exist; same result as the Reload button of the LoggingLauncher inspector. Run it after adding or removing a logger class. Not undoable via Ctrl+Z.",
            Tags = new[] { "observability" },
            Effect = CommandEffect.Idempotent)]
        internal static ExitCode ReloadConfig(CommandContext cmd)
        {
            if (!_TryReadConfig(cmd, out LoggersConfig _, out ConfigFileType fileType, out ExitCode failure))
                return failure;

            LoggersConfig config = LoggingHelper.ReloadConfig();
            if (config == null)
                return CliHelper.Fail(cmd, ExitCode.Failed, "The logger config file could not be reloaded.");

            _WriteConfigResult(cmd, config, fileType);
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Convert the logger config file between Json (plaintext) and Bytes (cipher) without changing its content; same result as the menu 'Assets/OxGKit/Logging System/Convert LoggerConfig.dat (BYTES [Cipher] <-> JSON [Plaintext])'. Not undoable via Ctrl+Z.",
            Tags = new[] { "observability" },
            Effect = CommandEffect.Idempotent)]
        internal static ExitCode ConvertConfig(
            CommandContext cmd,
            [CliArg("file-type", "The type to convert the file to: Json (plaintext) or Bytes (cipher).", Required = true)] ConfigFileType fileType)
        {
            if (!_TryReadConfig(cmd, out LoggersConfig config, out ConfigFileType currentFileType, out ExitCode failure))
                return failure;

            if (!LoggingHelper.ConvertConfig(fileType))
                return CliHelper.Fail(cmd, ExitCode.Failed, "The logger config file could not be converted.");

            cmd.Result.Write(new { path = _Normalize(LoggingHelper.GetConfigFilePath()), fileType = fileType.ToString(), previousFileType = currentFileType.ToString(), loggerCount = config.loggerSettings.Count });
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Change one logger entry of the logger config file (only the given values change); same result as editing that logger in the LoggingLauncher inspector. Not undoable via Ctrl+Z.",
            Tags = new[] { "observability" },
            Effect = CommandEffect.Idempotent)]
        internal static ExitCode SetLogger(
            CommandContext cmd,
            [CliArg("name", "Logger name as listed by get_config (the [LoggerName] value, or the class name).", Required = true)] string name,
            [CliArg("enabled", "Turn the logger on or off.")] bool? enabled = null,
            [CliArg("level", "Log levels, comma-separated: Off, LogDebug, LogInfo, LogWarning, LogError, LogException, All.")] LogLevel? level = null,
            [CliArg("color-mode", "Disabled, Enabled or EditorOnly.")] LogColor? color = null)
        {
            if (enabled == null && level == null && color == null)
                return CliHelper.Fail(cmd, ExitCode.InvalidUsage, "Nothing to change: pass at least one of enabled, level or color-mode.");

            if (!_TryReadConfig(cmd, out LoggersConfig config, out ConfigFileType fileType, out ExitCode failure))
                return failure;

            LoggerSettings settings = config.loggerSettings.FirstOrDefault(s => s.loggerName == name);
            if (settings == null)
                return CliHelper.Fail(cmd, ExitCode.NotFound, $"Logger '{name}' is not in the config file. Run oxgkit.logging get_config to list the loggers, or oxgkit.logging reload_config after adding a logger class.");

            if (enabled != null) settings.logActive = enabled.Value;
            if (level != null) settings.logLevel = level.Value;
            if (color != null) settings.logColor = color.Value;

            LoggingHelper.SaveConfig(config, fileType);

            cmd.Result.Write(_Describe(settings));
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Change the master switches of the logger config file (only the given values change); same result as editing the master settings in the LoggingLauncher inspector. Not undoable via Ctrl+Z.",
            Tags = new[] { "observability" },
            Effect = CommandEffect.Idempotent)]
        internal static ExitCode SetMaster(
            CommandContext cmd,
            [CliArg("enabled", "Turn all logging on or off.")] bool? enabled = null,
            [CliArg("level", "Log levels, comma-separated: Off, LogDebug, LogInfo, LogWarning, LogError, LogException, All.")] LogLevel? level = null,
            [CliArg("color-mode", "Disabled, Enabled or EditorOnly.")] LogColor? color = null)
        {
            if (enabled == null && level == null && color == null)
                return CliHelper.Fail(cmd, ExitCode.InvalidUsage, "Nothing to change: pass at least one of enabled, level or color-mode.");

            if (!_TryReadConfig(cmd, out LoggersConfig config, out ConfigFileType fileType, out ExitCode failure))
                return failure;

            if (enabled != null) config.logMainActive = enabled.Value;
            if (level != null) config.logMainLevel = level.Value;
            if (color != null) config.logMainColor = color.Value;

            LoggingHelper.SaveConfig(config, fileType);

            cmd.Result.Write(new { enabled = config.logMainActive, level = config.logMainLevel.ToString(), colorMode = config.logMainColor.ToString() });
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Create the LoggingSettings asset (config file name, extension and cipher key) at <path>/Resources/LoggingSettings.asset by running the menu 'Assets/Create/OxGKit/Create Settings/Create Logging Settings in Resources' on that folder. A project needs at most one. Not undoable via Ctrl+Z.",
            Tags = new[] { "observability" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode CreateSettings(
            CommandContext cmd,
            [CliArg("path", "Existing folder that gets the Resources folder (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            string folder;
            if (string.IsNullOrWhiteSpace(path))
            {
                folder = ProjectPaths.AuthoringRoot;
            }
            else
            {
                folder = ProjectPaths.Resolve(path, out string error);
                if (folder == null)
                    return CliHelper.Fail(cmd, ExitCode.InvalidUsage, error);
            }

            if (!AssetDatabase.IsValidFolder(folder))
                return CliHelper.Fail(cmd, ExitCode.NotFound, $"Folder '{folder}' does not exist. Create it first with asset create_folder.");

            string[] existing = AssetDatabase.FindAssets($"t:{nameof(LoggingSettings)}").Select(AssetDatabase.GUIDToAssetPath).ToArray();
            if (existing.Length > 0)
                return CliHelper.Fail(cmd, ExitCode.Failed, $"A LoggingSettings asset already exists at '{existing[0]}'. A project needs only one.");

            // Run the real menu item on the folder, the way a developer does
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder);
            if (!EditorApplication.ExecuteMenuItem(_SETTINGS_MENU))
                return CliHelper.Fail(cmd, ExitCode.Failed, $"The menu item '{_SETTINGS_MENU}' could not be executed.");

            string assetPath = $"{folder}/Resources/{nameof(LoggingSettings)}.asset";
            if (AssetDatabase.LoadAssetAtPath<LoggingSettings>(assetPath) == null)
                return CliHelper.Fail(cmd, ExitCode.Failed, $"The LoggingSettings asset was not created at '{assetPath}'.");

            cmd.Result.Write(new { assetPath, guid = AssetDatabase.AssetPathToGUID(assetPath), type = nameof(LoggingSettings) });
            return ExitCode.Success;
        }

        private static bool _TryReadConfig(CommandContext cmd, out LoggersConfig config, out ConfigFileType fileType, out ExitCode failure)
        {
            failure = ExitCode.Success;
            string path = LoggingHelper.GetConfigFilePath();

            if (!File.Exists(path))
            {
                config = null;
                fileType = ConfigFileType.Bytes;
                failure = CliHelper.Fail(cmd, ExitCode.NotFound, $"No logger config file at '{_Normalize(path)}'. Create it first with oxgkit.logging create_config.");
                return false;
            }

            if (!LoggingHelper.TryReadConfig(out config, out fileType) || config.loggerSettings == null)
            {
                failure = CliHelper.Fail(cmd, ExitCode.Failed, $"The logger config file at '{_Normalize(path)}' could not be parsed.");
                return false;
            }

            return true;
        }

        private static void _WriteConfigResult(CommandContext cmd, LoggersConfig config, ConfigFileType fileType)
        {
            cmd.Result.Write(new
            {
                path = _Normalize(LoggingHelper.GetConfigFilePath()),
                fileType = fileType.ToString(),
                master = new { enabled = config.logMainActive, level = config.logMainLevel.ToString(), colorMode = config.logMainColor.ToString() },
                loggers = config.loggerSettings.Select(_Describe).ToArray(),
            });
        }

        private static object _Describe(LoggerSettings settings)
        {
            return new { name = settings.loggerName, enabled = settings.logActive, level = settings.logLevel.ToString(), colorMode = settings.logColor.ToString() };
        }

        /// <summary>
        /// Project-relative path with forward slashes
        /// </summary>
        private static string _Normalize(string path)
        {
            path = path.Replace('\\', '/');
            string dataPath = UnityEngine.Application.dataPath;
            if (path.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase))
                path = "Assets" + path.Substring(dataPath.Length);
            return path;
        }
    }
}
