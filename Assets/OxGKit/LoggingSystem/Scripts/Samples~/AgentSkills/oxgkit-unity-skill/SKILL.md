---
name: oxgkit-unity-skill
description: Use when developing, reviewing, or debugging Unity projects that use OxGKit.LoggingSystem, including declaring loggers with [LoggerName] and the Logging base class, Logging.Print/PrintInfo/PrintWarning/PrintError/PrintException calls, the LoggingLauncher prefab and LoggerConfig.dat (StreamingAssets) configuration, LogLevel/LogColor rules, the OXGKIT_LOGGER_ON build symbol, runtime reconfiguration (ConfigureLogger/SetLoggersConfig), and HybridCLR AOT/Hotfix logger initialization. Also covers its Editor entry points and Unity CLI commands (unity command oxgkit.logging).
---

# OxGKit.LoggingSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.LoggingSystem user. LoggingSystem is a standalone logging module (UPM package `com.michaelo.oxgkit.loggingsystem`) with named loggers, per-logger and global on/off + level + color control, and an encrypted/plaintext runtime config file for tuning logs on device without rebuilding.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Before writing code, verify APIs against the installed package source (`Library/PackageCache/com.michaelo.oxgkit.loggingsystem@*` or `Assets/OxGKit/LoggingSystem/Scripts` when embedded). Do not invent APIs; if a member is not listed here, confirm it in the source first.
4. For player builds, always check whether the `OXGKIT_LOGGER_ON` scripting define symbol is set — without it no logger is created, so nothing is logged in builds (the Editor works without it).
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its samples and its Editor tooling. Follow that usage exactly. Never design your own variant of it: no home-made logger base class, static `Log` wrapper, log-level enum, config format or config writer that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling or shipped assets can provide (see "Editor entry points") is obtained through that entry point — the same one a developer uses — and you continue from what it produced. Do not hand-build an equivalent from memory and do not re-implement an Editor tool.
- **Stay inside the shipped shape.** Configure what the module exposes (serialized fields, public API); do not patch the package, subclass around it or add a parallel mechanism next to it.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/LoggingSystem/Scripts`
- Auto dependency: `com.domybest.lwmybox` (LWMyBox, inspector attributes).
- Samples (Package Manager > OxGKit.LoggingSystem > Samples): `LoggingLauncher Prefab` (bootstrap prefab), `LoggingSystem Demo`, `AI Agent Skills` (this skill).
- Editor tooling (assembly `OxGKit.LoggingSystem.Editor`, auto-referenced) and Unity CLI commands (`unity command oxgkit.logging ...`, declared in `OxGKit.LoggingSystem.Editor.UnityCli`, compiled only when `com.unity.pipeline` 1.x is installed): see "Editor entry points".

## Editor entry points (trigger, don't hand-write)

`LoggerConfig.dat` is produced and updated by the module's Editor tooling only. Never write or edit it by hand, and never generate it with code of your own.

| You need | Unity CLI command (group `oxgkit.logging`, v1.4.4+) | What a developer does |
| --- | --- | --- |
| Read the current config | `get_config` | Select the `LoggingLauncher` and read its Inspector |
| Create the config file | `create_config --file-type Json\|Bytes [--confirm] [--dry-run]` | `Assets > Create > OxGKit > Logging System > [JSON - Plaintext] Create LoggerConfig.dat (In StreamingAssets)` or `[BYTES - Cipher] Create LoggerConfig.dat (In StreamingAssets)` |
| Pick up added/removed logger classes while keeping every setting | `reload_config` | **Reload** button of the `LoggingLauncher` Inspector |
| Turn a logger on/off, set its levels or colour mode | `set_logger --name <logger> [--enabled true\|false] [--level <levels>] [--color-mode <mode>]` | Edit that logger in the `LoggingLauncher` Inspector |
| Change the master switches | `set_master [--enabled true\|false] [--level <levels>] [--color-mode <mode>]` | Master settings in the `LoggingLauncher` Inspector |
| Switch between plaintext and cipher | `convert_config --file-type Json\|Bytes` | Select the file, then `Assets > OxGKit > Logging System > Convert LoggerConfig.dat (BYTES [Cipher] <-> JSON [Plaintext])` |
| The `LoggingSettings` asset (file name, extension, cipher key) | `create_settings [--path <folder>]` | Select a folder, then `Assets > Create > OxGKit > Create Settings > Create Logging Settings in Resources` |
| The `LoggingLauncher` in the boot scene | — | Import the `LoggingLauncher Prefab` sample and drag the prefab into the scene (see below) |

Facts (verified on Unity 6000.3):

- The Create menus and `create_config` rebuild the file from the loggers found in the app domain with **default settings — every logger disabled, all levels** — and overwrite an existing file (the CLI command refuses that without `--confirm`). Create the file once; afterwards use Reload / `reload_config` (keeps every setting, adds new loggers with defaults, drops loggers that no longer exist) and `set_logger`.
- A new logger class is therefore silent until it has been added to the config (`reload_config`) **and** enabled (`set_logger --name <logger> --enabled true`, or the Inspector).
- The `LoggingLauncher` Inspector shows its own cached copy of the config, not the file. The menus, the Inspector and the CLI commands keep that copy in sync; a file edited by hand is not picked up, and the next Inspector change writes the cached copy over it.
- Levels are comma-separated flags (`--level LogWarning,LogError`): `Off`, `LogDebug`, `LogInfo`, `LogWarning`, `LogError`, `LogException`, `All`. Colour modes: `Disabled`, `Enabled`, `EditorOnly`.
- The settings menu creates `<selected folder>/Resources/LoggingSettings.asset` (`Assets/Resources/` when no folder is selected). Keep exactly one in the project; `create_settings` refuses to create a second one.

How to trigger them — take the first route you can really complete:

1. **Unity CLI.** Needs Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor. `unity command oxgkit.logging` lists the commands with their arguments.

   ```
   unity command oxgkit.logging create_config --file-type Json        # first time only
   unity command oxgkit.logging reload_config                         # after adding/removing a logger class (and a recompile)
   unity command oxgkit.logging set_logger --name App.Runtime --enabled true --level LogWarning,LogError
   ```
2. **Editor code** (a bridge that runs C# in the Editor; every supported Unity version; `OxGKit.LoggingSystem.Editor.LoggingHelper`, v1.4.4+): `LoggingHelper.CreateConfig(ConfigFileType.Json)`, `LoggingHelper.ReloadConfig()`, `LoggingHelper.ConvertConfig(ConfigFileType.Bytes)`. To change entries: `LoggingHelper.TryReadConfig(out var config, out var fileType)`, edit `config.loggerSettings` / `config.logMainActive` / `config.logMainLevel` / `config.logMainColor`, then `LoggingHelper.SaveConfig(config, fileType)` (writes the file and syncs the Inspector's copy).
3. **Menu items only** (a bridge that can only execute menu items, or `unity command editor invoke_menu --path "<menu path>"`): the two Create menus run as they are — their paths start with `Assets/Create/OxGKit/Logging System/`; the Convert menu needs the config file selected, the settings menu a selected folder.
4. **None of these**: ask the developer to use the menu or the Inspector. Do not write the file yourself.

Importing the `LoggingLauncher Prefab` sample (same as the Import button of Package Manager > OxGKit.LoggingSystem > Samples; Unity copies it to `Assets/Samples/OxGKit.LoggingSystem/<version>/LoggingLauncher Prefab`):

```csharp
// Editor code
var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.michaelo.oxgkit.loggingsystem");
foreach (var sample in UnityEditor.PackageManager.UI.Sample.FindByPackage(package.name, package.version))
    if (sample.displayName == "LoggingLauncher Prefab" && !sample.isImported)
        sample.Import(UnityEditor.PackageManager.UI.Sample.ImportOptions.None);
```

Without Editor access, copy the package's `Samples~/LoggingLauncher` folder (with its `.meta` files) to that destination instead. Use this prefab as the launcher; do not assemble your own bootstrap object for it.

## Core concepts

- A **logger** is a class inheriting `Logging`, named by `[LoggerName("Name")]` (falls back to the class name when the attribute is missing).
- **`LoggingLauncher`** is a MonoBehaviour that discovers/initializes loggers and loads their settings. Import the `LoggingLauncher Prefab` sample and drop it into the boot scene once (`initLoggersOnAwake` is on by default).
- **`LoggerConfig.dat`** in `StreamingAssets` overrides the inspector settings at runtime, so QA can toggle logs on device. It is created, updated and converted (JSON plaintext <-> BYTES cipher) only through the module's Editor tooling — see "Editor entry points". Ship releases with the cipher format. The file name, extension, and cipher key are customizable through the `LoggingSettings` asset (v1.4.0+; older versions used `loggersconfig.conf`).
- A log call is emitted only when the **global (master)** settings AND the **per-logger** settings both allow it (bitwise intersection for levels; see tables below).

## Core API (verified)

```csharp
using OxGKit.LoggingSystem;

// Declare a logger (public default constructor recommended for HybridCLR/reflection)
[LoggerName("App.Runtime")]
public class RuntimeLogger : Logging
{
    public RuntimeLogger() { }
}

// Override an existing logger's behavior by re-using its name with isOverride = true
[LoggerName("App.Runtime", true)]
public class OverrideRuntimeLogger : Logging
{
    public OverrideRuntimeLogger() { }
    public override void Log(object message) { UnityEngine.Debug.Log("[Override] " + message); }
    // Also overridable: LogInfo, LogWarning, LogError, LogException(Exception)
}

// Emit logs (context is optional, same as Unity's)
Logging.Print<RuntimeLogger>("debug message");
Logging.PrintInfo<RuntimeLogger>("info message");
Logging.PrintWarning<RuntimeLogger>("warning message");
Logging.PrintError<RuntimeLogger>("error message");
Logging.PrintException<RuntimeLogger>(exception);

// Check first when building the message is costly (v1.4.4+).
// True only when that same Print call would actually output (logger exists, master + per-logger switches and levels allow it).
if (Logging.IsLogActive<RuntimeLogger>(LogLevel.LogDebug))
    Logging.Print<RuntimeLogger>($"state: {BuildStateDump()}");
```

`LoggingLauncher` static API:

```csharp
LoggingLauncher.InitLoggers();                    // Discover and instantiate all Logging subclasses
LoggingLauncher.CreateLogger<TLogging>();         // Manually create one logger (HybridCLR flow)
LoggingLauncher.ClearLoggers();
LoggingLauncher.TryInitLoggers();                 // InitLoggers + load settings from config (coroutine wrapper)
LoggingLauncher.TryLoadLoggers();                 // (Re)load logger settings from LoggerConfig.dat
// Async (IEnumerator) variants: TryInitLoggersAsync / TryLoadLoggersAsync

// Master (global) switches
LoggingLauncher.ToggleMasterLogging(true);
LoggingLauncher.LevelMasterLogging(LogLevel.All);
LoggingLauncher.ColorMasterLogging(LogColor.EditorOnly);

// Per-logger runtime configuration (by logger name)
LoggingLauncher.ConfigureLogger("App.Runtime", true, LogLevel.LogWarning | LogLevel.LogError);
LoggingLauncher.ConfigureAllLoggers(true, LogLevel.All, LogColor.EditorOnly);

// Replace the whole config at runtime
LoggingLauncher.SetLoggersConfig(new LoggersConfig(
    new LoggerSettings("App.Runtime", true, LogLevel.All)
));
```

Enums:

```csharp
[Flags] LogLevel { Off, LogDebug, LogInfo, LogWarning, LogError, LogException, All }
LogColor { Disabled, Enabled, EditorOnly }   // EditorOnly strips rich-text color in players
ConfigFileType { Json, Bytes }               // LoggingLauncher.currentConfigFileType
```

## Level and color resolution

Levels — a level outputs only when both Global (G) and Per-logger (P) contain it (`G ∩ P`):

| Global (G) | Per-logger (P) | Effective |
| --- | --- | --- |
| Off | any | none |
| any | Off | none |
| All | All | all levels |
| All | single Px | Px |
| single Gx | All | Gx |
| set G | set P | G ∩ P (none if empty) |

Colors — effective mode is the more restrictive of the two; `EditorOnly` colors in the Editor but not in players:

| Global | Per-logger | Effective |
| --- | --- | --- |
| Disabled | any | Disabled |
| Enabled | Enabled | Enabled |
| Enabled/EditorOnly | EditorOnly (or vice versa) | EditorOnly |

## Standard setup flow

1. Import the `LoggingLauncher Prefab` sample; put the prefab in the boot scene (once).
2. Declare `[LoggerName]` logger classes near each subsystem.
3. Create `LoggerConfig.dat` once through an Editor entry point, then add new loggers with Reload / `reload_config` and enable the ones you need (a new logger is disabled by default).
4. Press Play — loggers are auto-discovered on Awake (`initLoggersOnAwake`), settings load from `StreamingAssets/LoggerConfig.dat` when present.
5. Add `OXGKIT_LOGGER_ON` to Scripting Define Symbols for any build that must log.
6. To tune logs on a device, replace the shipped `LoggerConfig.dat` with one produced by the Editor tooling (JSON while testing, BYTES for release).

## HybridCLR (AOT + Hotfix) flow

Auto discovery scans assemblies and may miss hotfix-loaded loggers, so:

1. Uncheck `Initialize On Awake` on the `LoggingLauncher` prefab.
2. In the AOT (main) startup: `LoggingLauncher.CreateLogger<TAotLogger>()` for every AOT logger.
3. In the hotfix startup: `CreateLogger<THotfixLogger>()` for every hotfix logger.
4. Call `LoggingLauncher.TryLoadLoggers()` after creating loggers (each time the logger set changes).
5. Give every logger a public default constructor (`Activator.CreateInstance` is used).

## Rules & pitfalls

- Adding or removing a logger at runtime requires `LoggingLauncher.TryLoadLoggers()` to reload settings.
- README/older docs may show `LoggerSetting`; the actual class is `LoggerSettings`.
- `Logging.Print<T>` maps to LogDebug; `PrintInfo` to LogInfo, etc. Choose levels so release builds can keep only `LogWarning | LogError | LogException`.
- Do not wrap Logging calls in your own `#if` guards for players; without `OXGKIT_LOGGER_ON` no logger exists, so the calls output nothing. The call itself still runs and its message argument is still built, so in hot paths (per-frame or per-item code) guard the call with `Logging.IsLogActive<TLogger>(level)` to skip building a message that would not be printed.
- The config file in `StreamingAssets` is loaded via web request (works on Android/WebGL); if absent, inspector/default settings apply.
- Every log goes through a `Logging` subclass and `Logging.Print*<TLogger>`. Do not add a static logging facade, a second level/category system or direct `Debug.Log` calls beside it, and do not hand-edit or hand-generate `LoggerConfig.dat`.

## Verify

- Editor: enter Play mode and confirm `[LoggingSystem] is Initialized.` appears and logger output respects the launcher settings.
- Player: build with `OXGKIT_LOGGER_ON`, then toggle levels via `LoggerConfig.dat` and re-run to confirm filtering.
