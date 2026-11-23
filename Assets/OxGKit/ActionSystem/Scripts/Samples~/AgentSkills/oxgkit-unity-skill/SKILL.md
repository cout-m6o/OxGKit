---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity projects that use OxGKit.ActionSystem, including ActionRunner update loops, composing SequenceAction / ParallelAction / ParallelDelayAction / DelayAction / DelegateAction, authoring custom ActionBase subclasses with SetDuration and MarkAsDone, and building timed/chained gameplay or animation flows. Also covers its Editor entry points and Unity CLI commands (unity command oxgkit.action).
---

# OxGKit.ActionSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.ActionSystem user. The module (UPM package `com.michaelo.oxgkit.actionsystem`, UniTask-based) is a lightweight action-sequence framework: compose actions (delays, callbacks, sequences, parallels, custom actions) into trees and pump them from any update loop via an `ActionRunner`. Good for scripted flows, cutscene-ish sequencing, and stitching timed logic without coroutines.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.actionsystem@*` or `Assets/OxGKit/ActionSystem/Scripts` when embedded). Do not invent APIs.
4. Always identify who owns the runner's `DriveUpdate(dt)` call and where `Release()` happens before writing action code.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its template, its sample and its Editor menu. Follow that usage exactly. Never design your own variant of it: no alternative action base class, naming scheme, runner wrapper or folder convention that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling can create (see "Editor entry points") is created through that entry point — the same one a developer clicks — and you continue from what it produced. Do not hand-write an equivalent from memory.
- **Stay inside the generated shape.** After generating, fill in only what the template leaves open; keep its regions, member names and signatures.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/cout-m6o/OxGKit.git?path=Assets/OxGKit/ActionSystem/Scripts`
- Auto dependencies: `com.cysharp.unitask`, `com.michaelo.oxgkit.loggingsystem` (v1.0.4+ of this module needs LoggingSystem 1.4.4+).
- Samples (Package Manager > OxGKit.ActionSystem > Samples): `ActionSystem Demo`, `AI Agent Skills` (this skill).
- Editor tooling (assembly `OxGKit.ActionSystem.Editor`, auto-referenced) and Unity CLI commands (`unity command oxgkit.action ...`, declared in `OxGKit.ActionSystem.Editor.UnityCli`, compiled only when `com.unity.pipeline` 1.x is installed): see "Editor entry points".

## Editor entry points (generate, don't hand-write)

Script templates (the Editor methods are static members of `OxGKit.ActionSystem.Editor.ActionSystemCreateScriptEditor`; the templates live in `Editor/TplScripts/` of the package; the `...AtPath` methods and the CLI commands exist from v1.0.4):

| You need | Unity CLI command | Menu a developer clicks | Editor methods (menu · no name field) | Template → default file |
| --- | --- | --- | --- | --- |
| A custom action class | `unity command oxgkit.action create --name <Name> [--path <folder>]` | `Assets > Create > OxGKit > Action System > Template Action.cs` | `CreateScriptTplAction()` · `CreateScriptTplActionAtPath(assetPath)` | `TplAction.cs.txt` → `NewTplAction.cs` |

All routes produce the same text. The menu hands the template to Unity's own script generator, `ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, "<default file>")`, which (verified on Unity 6000.3)

- creates the script in the folder selected in the Project window and opens the inline name field — the file is written once the name is confirmed. Without a Project window (`-batchmode`) it is written immediately as `Assets/<default file>` (a number is appended when taken), whatever is selected;
- replaces every `#SCRIPTNAME#` with the file name (extension dropped, spaces removed);
- turns the `#ROOTNAMESPACEBEGIN#` / `#ROOTNAMESPACEEND#` lines into `namespace <RootNamespace>`, `{` and `}` and indents every non-blank line in between by the whitespace in front of `#ROOTNAMESPACEBEGIN#` (four spaces); when the destination has no root namespace it simply removes those two lines. The root namespace is the `rootNamespace` of the assembly definition that owns the destination folder, otherwise *Project Settings > Editor > Root Namespace*.

The Unity CLI commands and the `...AtPath` methods apply exactly these substitutions (their output is byte-identical to the generator's) and write straight to the path you give: no name field, the folder must exist, and an existing file is never overwritten.

How to trigger it — take the first route you can really complete:

1. **Unity CLI.** Needs Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor. `unity command oxgkit.action` lists this module's commands with their arguments; they are declared in `OxGKit.ActionSystem.Editor.UnityCli`, which compiles only when that package is installed.

   ```
   unity command oxgkit.action create --name MoveAction --path Assets/Game/Actions
   unity status --until-ready --project-path .   # the Editor recompiles and reloads on its own - wait before the next command
   unity command script recompile_status         # "failed": false = the new script compiled
   ```

   `--name` is the class/file name (a valid C# identifier); `--path` is an existing folder, relative to the pipeline's authoring root (the `Assets/` prefix is optional; default: the root itself). The result carries `assetPath`. The Editor then imports the script, recompiles and reloads its scripts on its own (`unity command script recompile` starts a recompile explicitly); the new type exists only after that. While the Editor reloads, every `unity command` ends with exit code 12 (`EDITOR_UNREACHABLE`, "No Pipeline instance found") without having run — that is expected, not a problem of the project: wait with `unity status --until-ready` and run the command again. So wait after every script you create, before the next command.
2. **Editor code** (a bridge that runs C# in the Editor; every supported Unity version):

   ```csharp
   OxGKit.ActionSystem.Editor.ActionSystemCreateScriptEditor.CreateScriptTplActionAtPath("Assets/Game/Actions/MoveAction.cs");
   ```
3. **Menu items only** (a bridge that can only execute menu items, or `unity command editor invoke_menu --path "<menu path>"`): select the destination folder (`Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Game/Actions")`), execute `Assets/Create/OxGKit/Action System/Template Action.cs`, then you or the developer must confirm the name field in the Project window. Continue only after the file exists.
4. **Files only**: produce exactly what the generator produces. Read the template from the installed package — `TplScripts/` next to `ActionSystemCreateScriptEditor.cs` (`Library/PackageCache/com.michaelo.oxgkit.actionsystem@*/Editor/TplScripts/`, or under `Assets/` when the package is embedded) — apply the two substitutions above and save the result as `<Name>.cs` in the destination folder. Never retype a template from memory or from this document.
5. **None of these**: ask the developer to run the menu and tell you which file it created.

Never start a custom action from an empty file or by copying another class.
## Core API (verified)

```csharp
using OxGKit.ActionSystem;

// Runner — owns and updates running actions
var runner = new ActionRunner("FeatureActions");
runner.RunAction(action);      // resets the runner, then starts this action
runner.QueueAction(action);    // chainable; queued actions start on the next DriveUpdate
runner.RemoveAction(uid);      // remove by ActionBase.uid (marks it all-done)
runner.DriveUpdate(dt);        // pump from Update()/RTUpdater/etc. (drives the internal OnUpdate)
runner.Release();              // clear everything (call on feature exit)

// ActionBase — public surface
action.uid;  action.name;
action.IsStarted(); action.IsDone(); action.IsAllDone();
action.MarkAsDone();           // finish this action (fires OnDone)
action.MarkAllDone();          // finish including sub actions
action.SetDuration(seconds);   // -1 = runs until MarkAsDone() is called manually
action.GetTimeElapsed();       // elapsed seconds since start (fixed in v1.0.3; previously returned the duration)
action.GetTimeElapsedRatio();  // 0..1 based on duration
```

Default actions:

```csharp
// Delay then optional callback
DelayAction.CreateDelayAction(1.5f, onEndAction: null);

// Invoke a callback (optionally after a delay — returns a SequenceAction in that case)
DelegateAction.CreateDelegateAction(() => DoSomething(), delayTime: 0f);

// Sequence — runs children one by one
var seq = new SequenceAction("OpenFlow");
seq.AddAction(DelegateAction.CreateDelegateAction(this.OpenUI));
seq.AddAction(DelayAction.CreateDelayAction(1f));
seq.AddAction(DelegateAction.CreateDelegateAction(this.PlayDone));

// Parallel — runs children simultaneously, done when all are done
var par = new ParallelAction("Burst");
par.AddAction(a1).AddAction(a2);

// ParallelDelay — starts children in parallel with a stagger between starts
var stagger = new ParallelDelayAction(0.2f);
stagger.AddAction(a1).AddAction(a2);

runner.RunAction(seq);
```

## Driving the runner

```csharp
public class FeatureController : MonoBehaviour
{
    private readonly ActionRunner _runner = new ActionRunner("Feature");

    private void Update() => this._runner.DriveUpdate(Time.deltaTime);

    private void OnDestroy() => this._runner.Release();
}
```

Any loop works (MonoBehaviour `Update`, OxGKit `RTUpdater`, a stage system tick) — actions only advance while `DriveUpdate(dt)` is called. (`Drive*` is the OxGKit naming convention for developer-facing pump methods — same as `Inputs.IA.DriveUpdate`; the runner's internal `OnUpdate` is `protected` since v1.0.3.)

## Custom actions (always start from the generated template)

Generate the script through the entry point above, then fill it in. The generated class derives from `ActionBase` and already contains, in this order — keep all of it:

1. `#region Default Constructor` — four constructors: `()`, `(int uid)`, `(string name)`, `(string name, int uid)`. They set `name` (defaults to the class name) and `uid`; every built-in action has the same region.
2. A "Declare your params" comment — the action's fields go here.
3. `public static <Name> Create<Name>(/* Define params */)` — the factory. Give it parameters and assign them to the new instance; callers create the action through it, like the built-in `DelayAction.CreateDelayAction(...)` / `DelegateAction.CreateDelegateAction(...)`.
4. `protected override void OnStart()` containing `this.SetDuration(-1);` — the start logic goes here.

Filling in a generated `WaitForServerAction` (only these members change; the constructor region stays as generated):

```csharp
    // 2. params
    private string _api;

    // 3. factory
    public static WaitForServerAction CreateWaitForServerAction(string api)
    {
        var action = new WaitForServerAction();
        action._api = api;
        return action;
    }

    // 4. start logic
    protected override void OnStart()
    {
        this.SetDuration(-1);                                // -1: finished only by MarkAsDone()
        Server.Request(this._api, ok => this.MarkAsDone());  // every path must reach MarkAsDone()
    }
```

Optional overrides you may add beside `OnStart`: `protected override void OnUpdate(float dt)` (called on every `DriveUpdate` until the action is done) and `protected override void OnDone()` (called once by `MarkAsDone`). Nothing else in `ActionBase` is virtual.

Timed custom action: call `SetDuration(seconds)` in `OnStart()`; the base class auto-marks done when elapsed. Use `GetTimeElapsedRatio()` for progress-driven effects (e.g., lerping).

## Rules & pitfalls

- `RunAction` **resets the runner** (clears running/queued actions) before starting the new action — use `QueueAction` to append instead of replace.
- A runner that nobody pumps does nothing; a runner pumped twice per frame runs double speed. One owner per runner.
- `SetDuration(-1)` actions never finish unless `MarkAsDone()` / `MarkAllDone()` is called — guarantee the callback path fires (including error paths).
- Actions are stateful one-shots; reuse only via re-running (`RunStart` resets internal state) and never share one action instance across two runners simultaneously.
- Call `runner.Release()` when the owning stage/UI/feature exits, or pending callbacks may fire into released objects.
- `DelegateAction.CreateDelegateAction(action, delayTime > 0)` returns a `SequenceAction` wrapper — its static type is `ActionBase`, don't cast it to `DelegateAction`.
- Use v1.0.4+: in earlier versions a `ParallelAction` / `ParallelDelayAction` stopped updating its later children as soon as a child added before them had finished, so the group never completed. `ParallelDelayAction` now also leaves a child alone until its staggered start has happened.
- Express a flow with the shipped actions (`SequenceAction`, `ParallelAction`, `ParallelDelayAction`, `DelayAction`, `DelegateAction`) plus template-generated custom actions — do not build a second sequencing mechanism (coroutine chains, ad-hoc timers, a home-made action queue) beside `ActionRunner` for the same flow.

## Verify

- A freshly generated action compiles as generated; if it does not, regenerate it instead of patching the skeleton.
- Compile, run the flow, and confirm the sequence order/timing in logs (the runner reports done actions through OxGKit LoggingSystem: enable its logger to trace).
- Exit the feature mid-sequence and confirm no callbacks fire afterwards (Release path).
