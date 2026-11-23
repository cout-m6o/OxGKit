---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity projects that use OxGKit.SingletonSystem, including MonoSingleton<T> (MonoBehaviour singleton with OnCreate/OnStart/OnRelease lifecycle, GetInstance/InitInstance, dontDestroyOnLoad, DestroyInstance) and NewSingleton<T> (plain-class singleton). Also covers its Editor entry points and Unity CLI commands (unity command oxgkit.singleton).
---

# OxGKit.SingletonSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.SingletonSystem user. The module (UPM package `com.michaelo.oxgkit.singletonsystem`, dependency-free) provides two singleton bases: **`MonoSingleton<T>`** for MonoBehaviour systems (find-or-create, optional DontDestroyOnLoad, guarded lifecycle hooks) and **`NewSingleton<T>`** for plain C# classes.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.singletonsystem@*` or `Assets/OxGKit/SingletonSystem/Scripts` when embedded). Do not invent APIs.
4. Challenge singleton usage that doesn't fit: short-lived UI, pooled cells, and per-stage temporary state should not be singletons.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry point.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its template, its sample and its Editor menu. Follow that usage exactly. Never design your own variant of it: no second singleton base class, static `Instance` property pattern, service locator or lifecycle convention that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling can create (see "Editor entry points") is created through that entry point — the same one a developer clicks — and you continue from what it produced. Do not hand-write an equivalent from memory.
- **Stay inside the generated shape.** After generating, fill in only what the template leaves open; keep its member names and signatures.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/SingletonSystem/Scripts`
- No dependencies.
- Samples (Package Manager > OxGKit.SingletonSystem > Samples): `MonoSingleton Demo` (two scenes showing cross-scene persistence), `AI Agent Skills` (this skill).
- Editor tooling (assembly `OxGKit.SingletonSystem.Editor`, auto-referenced) and Unity CLI commands (`unity command oxgkit.singleton ...`, declared in `OxGKit.SingletonSystem.Editor.UnityCli`, compiled only when `com.unity.pipeline` 1.x is installed): see "Editor entry points".

## Editor entry points (generate, don't hand-write)

Script templates (the Editor methods are static members of `OxGKit.SingletonSystem.Editor.SingletonCreateScriptEditor`; the templates live in `Editor/TplScripts/` of the package; the `...AtPath` methods and the CLI commands exist from v1.0.3):

| You need | Unity CLI command | Menu a developer clicks | Editor methods (menu · no name field) | Template → default file |
| --- | --- | --- | --- | --- |
| A `MonoSingleton<T>` class | `unity command oxgkit.singleton create_mono_singleton --name <Name> [--path <folder>]` | `Assets > Create > OxGKit > SingletonSystem > Template Mono Singleton.cs` | `CreateScriptTplMonoSingleton()` · `CreateScriptTplMonoSingletonAtPath(assetPath)` | `TplMonoSingleton.cs.txt` → `NewTplMonoSingleton.cs` |

All routes produce the same text. The menu hands the template to Unity's own script generator, `ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, "<default file>")`, which (verified on Unity 6000.3)

- creates the script in the folder selected in the Project window and opens the inline name field — the file is written once the name is confirmed. Without a Project window (`-batchmode`) it is written immediately as `Assets/<default file>` (a number is appended when taken), whatever is selected;
- replaces every `#SCRIPTNAME#` with the file name (extension dropped, spaces removed);
- turns the `#ROOTNAMESPACEBEGIN#` / `#ROOTNAMESPACEEND#` lines into `namespace <RootNamespace>`, `{` and `}` and indents every non-blank line in between by the whitespace in front of `#ROOTNAMESPACEBEGIN#` (four spaces); when the destination has no root namespace it simply removes those two lines. The root namespace is the `rootNamespace` of the assembly definition that owns the destination folder, otherwise *Project Settings > Editor > Root Namespace*.

The Unity CLI commands and the `...AtPath` methods apply exactly these substitutions (their output is byte-identical to the generator's) and write straight to the path you give: no name field, the folder must exist, and an existing file is never overwritten.

How to trigger it — take the first route you can really complete:

1. **Unity CLI.** Needs Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor. `unity command oxgkit.singleton` lists this module's commands with their arguments; they are declared in `OxGKit.SingletonSystem.Editor.UnityCli`, which compiles only when that package is installed.

   ```
   unity command oxgkit.singleton create_mono_singleton --name GameSystem --path Assets/Game/Systems
   unity status --until-ready --project-path .   # the Editor recompiles and reloads on its own - wait before the next command
   unity command script recompile_status         # "failed": false = the new script compiled
   ```

   `--name` is the class/file name (a valid C# identifier); `--path` is an existing folder, relative to the pipeline's authoring root (the `Assets/` prefix is optional; default: the root itself). The result carries `assetPath`. The Editor then imports the script, recompiles and reloads its scripts on its own (`unity command script recompile` starts a recompile explicitly); the new type exists only after that. While the Editor reloads, every `unity command` ends with exit code 12 (`EDITOR_UNREACHABLE`, "No Pipeline instance found") without having run — that is expected, not a problem of the project: wait with `unity status --until-ready` and run the command again. So wait after every script you create, before the next command.
2. **Editor code** (a bridge that runs C# in the Editor; every supported Unity version):

   ```csharp
   OxGKit.SingletonSystem.Editor.SingletonCreateScriptEditor.CreateScriptTplMonoSingletonAtPath("Assets/Game/Systems/GameSystem.cs");
   ```
3. **Menu items only** (a bridge that can only execute menu items, or `unity command editor invoke_menu --path "<menu path>"`): select the destination folder (`Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Game/Systems")`), execute `Assets/Create/OxGKit/SingletonSystem/Template Mono Singleton.cs`, then you or the developer must confirm the name field in the Project window. Continue only after the file exists.
4. **Files only**: produce exactly what the generator produces. Read the template from the installed package — `TplScripts/` next to `SingletonCreateScriptEditor.cs` (`Library/PackageCache/com.michaelo.oxgkit.singletonsystem@*/Editor/TplScripts/`, or under `Assets/` when the package is embedded) — apply the two substitutions above and save the result as `<Name>.cs` in the destination folder. Never retype a template from memory or from this document.
5. **None of these**: ask the developer to run the menu and tell you which file it created.

`NewSingleton<T>` has no generator: any plain class with a public parameterless constructor is used through `NewSingleton<T>` as shown below.
## MonoSingleton&lt;T&gt; (verified)

```csharp
using OxGKit.SingletonSystem;

// Generated from "Template Mono Singleton.cs": the class and the three overrides already exist — you fill the bodies.
public class GameSystem : MonoSingleton<GameSystem>
{
    protected override void OnCreate()  { /* Unity Awake */ }
    protected override void OnStart()   { /* Unity Start */ }
    protected override void OnRelease() { /* Unity OnDestroy */ }
}

// Access / lifecycle
GameSystem.GetInstance();                    // find-or-create; param dontDestroyOnLoad = true by default
GameSystem.InitInstance();                   // alias for warm-up
GameSystem.CheckInstanceExists();
GameSystem.DestroyInstance(gameObjectIncluded: true);

// Static state flags
GameSystem.isCreated; GameSystem.isStarted; GameSystem.isReleased;

// Instance field
instance.dontDestroyOnLoad;                  // also settable on a scene-placed instance in the inspector
```

Behavior:

- `GetInstance(dontDestroyOnLoad = true)` (play mode only): finds an existing scene instance or creates a `new GameObject(typeof(T).Name)` with the component; applies `DontDestroyOnLoad` when requested.
- A scene-placed instance registers itself in `Awake` and honors its serialized `dontDestroyOnLoad` field.
- Lifecycle hooks are one-shot-guarded: `OnCreate()` from `Awake`, `OnStart()` from `Start`, `OnRelease()` from `OnDestroy` (flags reset so a new instance can be created later).
- **Do not override `Awake()`, `Start()`, or `OnDestroy()`** — they are private in the base; every other Unity message (`Update`, `OnEnable`, ...) is yours to implement normally.

## NewSingleton&lt;T&gt; (verified)

```csharp
using OxGKit.SingletonSystem;

public class ScoreService
{
    // Access via the generic holder:
}
ScoreService svc = NewSingleton<ScoreService>.GetInstance();  // lazy new T()
NewSingleton<ScoreService>.InitInstance();
NewSingleton<ScoreService>.CheckInstanceExists();
NewSingleton<ScoreService>.DestroyInstance();                 // just clears the static ref
```

`T` needs a public parameterless constructor (`where T : class, new()`).

## Usage patterns

Boot warm-up (explicit creation order beats lazy surprises):

```csharp
private void Awake()
{
    GameSystem.InitInstance();
    AudioSystem.InitInstance();
}
```

Scene-scoped singleton (dies with the scene):

```csharp
StageSystem.GetInstance(dontDestroyOnLoad: false);
```

Safe access from teardown paths:

```csharp
if (GameSystem.CheckInstanceExists())
    GameSystem.GetInstance().Save();
```

## Rules & pitfalls

- The `dontDestroyOnLoad` decision is applied on **first creation** — decide it at the first `GetInstance`/`InitInstance` call site (or on the scene-placed instance) and keep it consistent.
- `GetInstance()` from `OnApplicationQuit`/`OnDestroy` teardown can re-create objects mid-quit — guard with `CheckInstanceExists()` (or check `isReleased`).
- Registering event subscriptions in `OnCreate`/`OnStart` requires unsubscribing in `OnRelease`; singletons outlive scenes and accumulate stale delegates otherwise.
- Two scene-placed instances of the same `T` are not auto-deduplicated — the first found wins as `_instance`; avoid placing duplicates across additively-loaded scenes.
- `MonoSingleton` is for stable core systems (audio, save, input hub). UI panels, pooled items, per-stage helpers: plain components instead.
- `NewSingleton<T>.DestroyInstance()` only nulls the holder — the old object survives if referenced elsewhere; it is not a disposal mechanism.
- A singleton in a project that uses this module is a `MonoSingleton<T>` generated from the template or a class accessed through `NewSingleton<T>`. Do not add a hand-written `static Instance` field, a `[RuntimeInitializeOnLoadMethod]` bootstrapper or another base class to achieve the same thing.

## Verify

- A freshly generated singleton compiles as generated; if it does not, regenerate it instead of patching the skeleton.

- Play the two demo scenes: the singleton persists across scene loads when `dontDestroyOnLoad` is true, `OnCreate/OnStart` fire once, and `OnRelease` fires on destroy.
- Domain-reload check: with Enter Play Mode Options (no domain reload), statics persist — confirm `DestroyInstance`/flags behave across repeated plays.
