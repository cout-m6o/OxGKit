---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity projects that use OxGKit.NoticeSystem (red-dot / notification badge system), including custom NoticeCondition classes, NoticeManager.RegisterCondition and Notify, the NoticeItem component and prefab, NoticeInfo registration with value/reference data, and data-driven red-dot visibility refresh. Also covers its Editor entry points and Unity CLI commands (unity command oxgkit.notice).
---

# OxGKit.NoticeSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.NoticeSystem user. The module (UPM package `com.michaelo.oxgkit.noticesystem`) is a red-dot/notification system: you define **conditions** (pure classes answering "should the badge show for this data?"), attach a **NoticeItem** component to each badge icon, register condition+data pairs on it, and call **Notify** when data changes — every NoticeItem holding a notified condition re-evaluates and toggles its own GameObject.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.noticesystem@*` or `Assets/OxGKit/NoticeSystem/Scripts` when embedded). Do not invent APIs.
4. Before adding conditions, check how the target project registers them (static register class vs `RuntimeInitializeOnLoadMethod` template) and follow that pattern.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its templates, its samples and its Editor menus. Follow that usage exactly. Never design your own variant of it: no condition-id enum or constants, no home-made condition registry, badge component, notifier wrapper or naming scheme that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling can create (see "Editor entry points") is created through that entry point — the same one a developer clicks — and you continue from what it produced. Do not hand-write an equivalent from memory.
- **Stay inside the generated shape.** After generating, fill in only what the template leaves open; keep its regions, member names and signatures.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/cout-m6o/OxGKit.git?path=Assets/OxGKit/NoticeSystem/Scripts`
- Auto dependency: `com.michaelo.oxgkit.loggingsystem` (v1.0.6+ of this module needs LoggingSystem 1.4.4+).
- Samples (Package Manager > OxGKit.NoticeSystem > Samples): `NoticeItem Prefab` (badge prefab), `NoticeSystem Demo`, `AI Agent Skills` (this skill).
- Editor tooling (assembly `OxGKit.NoticeSystem.Editor`, auto-referenced) and Unity CLI commands (`unity command oxgkit.notice ...`, declared in `OxGKit.NoticeSystem.Editor.UnityCli`, compiled only when `com.unity.pipeline` 1.x is installed): see "Editor entry points".

## Editor entry points (generate, don't hand-write)

Script templates (the Editor methods are static members of `OxGKit.NoticeSystem.Editor.NoticeSystemCreateScriptEditor`; the templates live in `Editor/TplScripts/` of the package; the `...AtPath` methods and the CLI commands exist from v1.0.6):

| You need | Unity CLI command | Menu a developer clicks | Editor methods (menu · no name field) | Template → default file |
| --- | --- | --- | --- | --- |
| A condition that a register class registers | `unity command oxgkit.notice create_condition --name <Name> [--path <folder>]` | `Assets > Create > OxGKit > Notice System > Template Notice Condition.cs (Manually)` | `CreateScriptTplNoticeCondition()` · `CreateScriptTplNoticeConditionAtPath(assetPath)` | `TplNoticeCondition.cs.txt` → `NewTplNoticeCondition.cs` |
| A condition that registers itself before the first scene loads | `unity command oxgkit.notice create_condition_before_scene_load --name <Name> [--path <folder>]` | `Assets > Create > OxGKit > Notice System > Template Notice Condition.cs (RuntimeInitializeLoadType.BeforeSceneLoad)` | `CreateScriptTplNoticeConditionWithBeforeSceneLoad()` · `CreateScriptTplNoticeConditionWithBeforeSceneLoadAtPath(assetPath)` | `TplNoticeConditionWithBeforeSceneLoad.cs.txt` → `NewTplNoticeConditionWithBeforeSceneLoad.cs` |
| The static class that registers the "Manually" conditions | `unity command oxgkit.notice create_condition_registers --name <Name> [--path <folder>]` | `Assets > Create > OxGKit > Notice System > Template Notice Condition Registers.cs (Manually)` | `CreateScriptTplNoticeConditionRegisters()` · `CreateScriptTplNoticeConditionRegistersAtPath(assetPath)` | `TplNoticeConditionRegisters.cs.txt` → `NewTplNoticeConditionRegisters.cs` |

Component and prefab:

| You need | What a developer does | Agent equivalent |
| --- | --- | --- |
| The badge component on an existing GameObject | `Add Component > OxGKit > NoticeSystem > NoticeItem` | `unity command component add --target <GameObject> --type OxGKit.NoticeSystem.NoticeItem`, or select the GameObject and execute the menu item `Component/OxGKit/NoticeSystem/NoticeItem` |
| A ready-made badge (root `NoticeItem` with the component + child `Icon` image) | Package Manager > OxGKit.NoticeSystem > Samples > `NoticeItem Prefab` > Import | see "Importing the badge prefab" below |

All routes produce the same text. The menu hands the template to Unity's own script generator, `ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, "<default file>")`, which (verified on Unity 6000.3)

- creates the script in the folder selected in the Project window and opens the inline name field — the file is written once the name is confirmed. Without a Project window (`-batchmode`) it is written immediately as `Assets/<default file>` (a number is appended when taken), whatever is selected;
- replaces every `#SCRIPTNAME#` with the file name (extension dropped, spaces removed);
- turns the `#ROOTNAMESPACEBEGIN#` / `#ROOTNAMESPACEEND#` lines into `namespace <RootNamespace>`, `{` and `}` and indents every non-blank line in between by the whitespace in front of `#ROOTNAMESPACEBEGIN#` (four spaces); when the destination has no root namespace it simply removes those two lines. The root namespace is the `rootNamespace` of the assembly definition that owns the destination folder, otherwise *Project Settings > Editor > Root Namespace*.

The Unity CLI commands and the `...AtPath` methods apply exactly these substitutions (their output is byte-identical to the generator's) and write straight to the path you give: no name field, the folder must exist, and an existing file is never overwritten.

How to trigger one — take the first route you can really complete:

1. **Unity CLI.** Needs Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor. `unity command oxgkit.notice` lists this module's commands with their arguments; they are declared in `OxGKit.NoticeSystem.Editor.UnityCli`, which compiles only when that package is installed.

   ```
   unity command oxgkit.notice create_condition --name NewMailCond --path Assets/Game/Notices
   unity status --until-ready --project-path .   # the Editor recompiles and reloads on its own - wait before the next command
   unity command script recompile_status         # "failed": false = the new script compiled
   ```

   `--name` is the class/file name (a valid C# identifier); `--path` is an existing folder, relative to the pipeline's authoring root (the `Assets/` prefix is optional; default: the root itself). The result carries `assetPath`. The Editor then imports the script, recompiles and reloads its scripts on its own (`unity command script recompile` starts a recompile explicitly); the new type exists only after that. While the Editor reloads, every `unity command` ends with exit code 12 (`EDITOR_UNREACHABLE`, "No Pipeline instance found") without having run — that is expected, not a problem of the project: wait with `unity status --until-ready` and run the command again. So wait after every script you create, before the next command.
2. **Editor code** (a bridge that runs C# in the Editor; every supported Unity version):

   ```csharp
   OxGKit.NoticeSystem.Editor.NoticeSystemCreateScriptEditor.CreateScriptTplNoticeConditionAtPath("Assets/Game/Notices/NewMailCond.cs");
   ```
3. **Menu items only** (a bridge that can only execute menu items, or `unity command editor invoke_menu --path "<menu path>"`): select the destination folder (`Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Game/Notices")`), execute `Assets/Create/OxGKit/Notice System/Template Notice Condition.cs (Manually)`, then you or the developer must confirm the name field in the Project window. Continue only after the file exists.
4. **Files only**: produce exactly what the generator produces. Read the template from the installed package — `TplScripts/` next to `NoticeSystemCreateScriptEditor.cs` (`Library/PackageCache/com.michaelo.oxgkit.noticesystem@*/Editor/TplScripts/`, or under `Assets/` when the package is embedded) — apply the two substitutions above and save the result as `<Name>.cs` in the destination folder. Never retype a template from memory or from this document.
5. **None of these**: ask the developer to run the menu and tell you which file it created.

Importing the badge prefab (same as the Import button; Unity copies it to `Assets/Samples/OxGKit.NoticeSystem/<version>/NoticeItem Prefab`):

```csharp
// Editor code
var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.michaelo.oxgkit.noticesystem");
foreach (var sample in UnityEditor.PackageManager.UI.Sample.FindByPackage(package.name, package.version))
    if (sample.displayName == "NoticeItem Prefab" && !sample.isImported)
        sample.Import(UnityEditor.PackageManager.UI.Sample.ImportOptions.None);
```

Without Editor access, copy the package's `Samples~/NoticeItem` folder (with its `.meta` files) to that destination instead. Use this prefab for badges; do not build a look-alike prefab or a second badge component.
## Core API (verified)

```csharp
using OxGKit.NoticeSystem;

// Condition: pure logic class, generated from a condition template.
// Generated: the "#region Default" block with the static id (and, in the BeforeSceneLoad variant, the Register() method).
// Yours: only the body of ShowCondition.
public class NewMailCond : NoticeCondition
{
    #region Default
    public static int id { get { return NoticeManager.GetConditionId<NewMailCond>(); } }
    #endregion

    public override bool ShowCondition(object data)
    {
        return data is MailBox box && box.unreadCount > 0;
    }
}

// Manager
NoticeManager.RegisterCondition<TNoticeCondition>(); // assigns the condition id
NoticeManager.GetConditionId<TNoticeCondition>();
NoticeManager.Notify(int conditionId);               // re-evaluate every NoticeItem holding this condition (non-alloc)
NoticeManager.Notify(params int[] conditionIds);     // multiple-conditions variant
NoticeManager.Notify(params NoticeItem[] items);     // re-evaluate specific items

// NoticeItem (MonoBehaviour on the badge icon GameObject)
noticeItem.RegisterNotice(params NoticeInfo[] infos); // register + immediately evaluates visibility
noticeItem.RenewNotice(NoticeInfo info);              // update the data snapshot (auto-registers if new)
noticeItem.DeregisterNotice(params int[] conditionIds); // no args = deregister all of its conditions
noticeItem.HasCondition(int conditionId);
noticeItem.Notify();                                  // re-notify all conditions this item holds

// NoticeInfo: condition id + the data handed to ShowCondition
new NoticeInfo(NewMailCond.id, mailBox);
```

Visibility rule: a `NoticeItem` sets its **own GameObject active** when **any** of its registered conditions returns `true`, inactive when none do. On `OnDestroy` it auto-deregisters.

## Standard flow

```csharp
// 1) Register conditions once at startup. The class is generated from the "Registers" template
//    (static constructor + Init()); only the RegisterCondition lines are yours. Call NoticeConditionRegister.Init()
//    once during boot. Conditions generated from the BeforeSceneLoad template register themselves — never register those again here.
public static class NoticeConditionRegister
{
    static NoticeConditionRegister()
    {
        NoticeManager.RegisterCondition<NewMailCond>();
        NoticeManager.RegisterCondition<CanUpgradeCond>();
    }

    /// <summary>
    /// Manually trigger static constructor
    /// </summary>
    public static void Init() { }
}

// 2) Attach the NoticeItem prefab (badge icon) under a button/tab, then register data on it
this.mailNoticeItem.RegisterNotice(new NoticeInfo(NewMailCond.id, this._mailBox));

// 3) When data changes, notify the condition — all items holding it refresh
this._mailBox.unreadCount++;
NoticeManager.Notify(NewMailCond.id);

// 4) On UI close (optional; OnDestroy also cleans up)
this.mailNoticeItem.DeregisterNotice();
```

Value-type data caveat: `NoticeInfo.data` stores a snapshot for value types (int, struct). Re-register the current value with `RenewNotice` before notifying:

```csharp
this.coinNoticeItem.RenewNotice(new NoticeInfo(CoinIsEvenCond.id, this._coin)).Notify();
```

Reference types (class instances) stay live — `Notify` alone is enough after mutating the object.

## Rules & pitfalls

- Register a condition class (`RegisterCondition<T>`) **before** using its id; `GetConditionId<T>` on an unregistered condition cannot resolve.
- `ShowCondition(object data)` must be defensive: `data` can be null or a different type when several UIs reuse a condition.
- One NoticeItem may hold several conditions (OR semantics). For AND logic, put the whole predicate inside one condition class.
- Value-type data is snapshotted — use `RenewNotice` (method-chainable) + `Notify`; forgetting this is the classic "red dot never updates" bug.
- `NoticeItem` toggles its own GameObject — make the badge a **child** icon object, not the button itself, or the button will disappear.
- Conditions are plain logic classes: no MonoBehaviour state, no scene references inside `ShowCondition`.
- Deregistration is automatic on destroy, but for pooled UI call `DeregisterNotice()` on recycle to avoid stale registrations. From v1.0.6 `DeregisterNotice` is also safe on a notice item that is already destroyed (for example when another script deregisters its items from `OnDestroy` while the scene unloads); earlier versions throw there.
- Condition ids are assigned by `RegisterCondition<T>` and read through the generated `id` property (`NewMailCond.id`). Do not hard-code ids, mirror them in an enum or cache them in your own table.
- Use one registration style per condition: either it is listed in a register class (the "Manually" template) or it carries the generated `Register()` method (the BeforeSceneLoad template). Registering the same condition twice only logs a "Repeat Register" warning, but it means two styles got mixed.

## Verify

- A freshly generated condition or register class compiles as generated; if it does not, regenerate it instead of patching the skeleton.

- Enter Play mode, mutate the driving data, call `Notify`, and confirm badges appear/disappear on every UI that holds the condition.
- Destroy/close the UI and notify again — no errors and no orphaned refreshes should occur.
