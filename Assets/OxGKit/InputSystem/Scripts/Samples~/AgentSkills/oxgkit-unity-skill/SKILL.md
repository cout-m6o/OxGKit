---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity projects that use OxGKit.InputSystem, including the Inputs.CM control-map registry (Unity New Input System IInputActionCollection), the Inputs.IA input-action registry (IInputAction signal dispatchers), Inputs.IA.DriveUpdate loops, custom Binding Composites, and decoupling gameplay/UI from device input via event subscription. Also covers its Editor entry points and Unity CLI commands (unity command oxgkit.input).
---

# OxGKit.InputSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.InputSystem user. The module (UPM package `com.michaelo.oxgkit.inputsystem`) is a thin input-dispatch layer: **Control Maps** wrap Unity New Input System generated classes, and **Input Actions** (`IInputAction`) re-dispatch input as C# events, so gameplay/UI code subscribes to game-level signals and never touches devices or platforms directly. Input Actions can also wrap any other input plugin — they are plain signal dispatchers.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.inputsystem@*` or `Assets/OxGKit/InputSystem/Scripts` when embedded) and inspect the project's generated control-map classes before writing code. Do not invent APIs or asset names.
4. Check the project actually has Unity New Input System enabled (Player Settings > Active Input Handling) when Control Maps are involved.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its templates, its sample and its Editor menus. Follow that usage exactly. Never design your own variant of it: no second input registry, input-manager singleton, event bus, action base class or naming scheme that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling can create (see "Editor entry points") is created through that entry point — the same one a developer clicks — and you continue from what it produced. Do not hand-write an equivalent from memory.
- **Stay inside the generated shape.** After generating, fill in only what the template leaves open; keep its member names and signatures.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/cout-m6o/OxGKit.git?path=Assets/OxGKit/InputSystem/Scripts`
- Auto dependencies: `com.unity.inputsystem`, `com.michaelo.oxgkit.loggingsystem`.
- Samples (Package Manager > OxGKit.InputSystem > Samples): `InputSystem Demo`, `AI Agent Skills` (this skill).
- Editor tooling (assembly `OxGKit.InputSystem.Editor`, auto-referenced) and Unity CLI commands (`unity command oxgkit.input ...`, declared in `OxGKit.InputSystem.Editor.UnityCli`, compiled only when `com.unity.pipeline` 1.x is installed): see "Editor entry points".

## Editor entry points (generate, don't hand-write)

Script templates (the Editor methods are static members of `OxGKit.InputSystem.Editor.InputSystemCreateScriptEditor`; the templates live in `Editor/TplScripts/` of the package; the `...AtPath` methods and the CLI commands exist from v1.0.5):

| You need | Unity CLI command | Menu a developer clicks | Editor methods (menu · no name field) | Template → default file |
| --- | --- | --- | --- | --- |
| An `IInputAction` (game-level signal dispatcher, any input source) | `unity command oxgkit.input create_action --name <Name> [--path <folder>]` | `Assets > Create > OxGKit > Input System > Template Input Action.cs (Input Interface For Any)` | `CreateScriptTplInputAction()` · `CreateScriptTplInputActionAtPath(assetPath)` | `TplInputAction.cs.txt` → `NewTplInputAction.cs` |
| A Binding Composite for Unity New Input System | `unity command oxgkit.input create_binding_composite --name <Name> [--path <folder>]` | `Assets > Create > OxGKit > Input System > New Input System (Extension) > Template Input Binding Composite.cs (For Unity New Input System)` | `CreateScriptTplInputBindingComposite()` · `CreateScriptTplInputBindingCompositeAtPath(assetPath)` | `TplInputBindingComposite.cs.txt` → `NewTplInputBindingComposite.cs` |

All routes produce the same text. The menu hands the template to Unity's own script generator, `ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, "<default file>")`, which (verified on Unity 6000.3)

- creates the script in the folder selected in the Project window and opens the inline name field — the file is written once the name is confirmed. Without a Project window (`-batchmode`) it is written immediately as `Assets/<default file>` (a number is appended when taken), whatever is selected;
- replaces every `#SCRIPTNAME#` with the file name (extension dropped, spaces removed);
- turns the `#ROOTNAMESPACEBEGIN#` / `#ROOTNAMESPACEEND#` lines into `namespace <RootNamespace>`, `{` and `}` and indents every non-blank line in between by the whitespace in front of `#ROOTNAMESPACEBEGIN#` (four spaces); when the destination has no root namespace it simply removes those two lines. The root namespace is the `rootNamespace` of the assembly definition that owns the destination folder, otherwise *Project Settings > Editor > Root Namespace*.

The Unity CLI commands and the `...AtPath` methods apply exactly these substitutions (their output is byte-identical to the generator's) and write straight to the path you give: no name field, the folder must exist, and an existing file is never overwritten.

How to trigger one — take the first route you can really complete:

1. **Unity CLI.** Needs Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor. `unity command oxgkit.input` lists this module's commands with their arguments; they are declared in `OxGKit.InputSystem.Editor.UnityCli`, which compiles only when that package is installed.

   ```
   unity command oxgkit.input create_action --name PlayerAction --path Assets/Game/Inputs
   unity status --until-ready --project-path .   # the Editor recompiles and reloads on its own - wait before the next command
   unity command script recompile_status         # "failed": false = the new script compiled
   ```

   `--name` is the class/file name (a valid C# identifier); `--path` is an existing folder, relative to the pipeline's authoring root (the `Assets/` prefix is optional; default: the root itself). The result carries `assetPath`. The Editor then imports the script, recompiles and reloads its scripts on its own (`unity command script recompile` starts a recompile explicitly); the new type exists only after that. While the Editor reloads, every `unity command` ends with exit code 12 (`EDITOR_UNREACHABLE`, "No Pipeline instance found") without having run — that is expected, not a problem of the project: wait with `unity status --until-ready` and run the command again. So wait after every script you create, before the next command.
2. **Editor code** (a bridge that runs C# in the Editor; every supported Unity version):

   ```csharp
   OxGKit.InputSystem.Editor.InputSystemCreateScriptEditor.CreateScriptTplInputActionAtPath("Assets/Game/Inputs/PlayerAction.cs");
   ```
3. **Menu items only** (a bridge that can only execute menu items, or `unity command editor invoke_menu --path "<menu path>"`): select the destination folder (`Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Game/Inputs")`), execute `Assets/Create/OxGKit/Input System/Template Input Action.cs (Input Interface For Any)`, then you or the developer must confirm the name field in the Project window. Continue only after the file exists.
4. **Files only**: produce exactly what the generator produces. Read the template from the installed package — `TplScripts/` next to `InputSystemCreateScriptEditor.cs` (`Library/PackageCache/com.michaelo.oxgkit.inputsystem@*/Editor/TplScripts/`, or under `Assets/` when the package is embedded) — apply the two substitutions above and save the result as `<Name>.cs` in the destination folder. Never retype a template from memory or from this document.
5. **None of these**: ask the developer to run the menu and tell you which file it created.

Naming: the Input Action template names the class after the file. The Binding Composite template turns the name into **two** types — `<Name>.cs` gives `public class <Name>Composite : InputBindingComposite<<Name>Composite.<Name>>` with the nested value struct `<Name>` — so name it after the value (the sample's `MoveInput.cs` holds `MoveInputComposite` and `MoveInputComposite.MoveInput`).
## Core API (verified)

```csharp
using OxGKit.InputSystem;

// Control Maps registry (for Unity New Input System generated classes)
Inputs.CM.RegisterControlMap<TIInputActionCollection>();   // new() + Enable()
Inputs.CM.GetControlMap<TIInputActionCollection>();        // null if not registered
Inputs.CM.SetActive<TIInputActionCollection>(bool active); // Enable()/Disable() the collection
Inputs.CM.IsActive<TIInputActionCollection>();

// Input Actions registry (game-level signal dispatchers)
Inputs.IA.RegisterInputAction<TInputAction>();             // new() + OnCreate()
Inputs.IA.GetInputAction<TInputAction>();
Inputs.IA.DriveUpdate(float dt);                           // pumps every IInputAction.OnUpdate(dt)

public interface IInputAction
{
    void OnCreate();             // called on register: bind to control maps / plugins here
    void OnUpdate(float dt);     // polled by Inputs.IA.DriveUpdate(dt)
    void RemoveAllListeners();   // clear subscribed events
}
```

## Usage pattern

Register order matters — Control Maps first, then Input Actions (their `OnCreate` usually fetches a control map):

```csharp
using OxGKit.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

// 1) An IInputAction that converts device input into game events.
//    Generated from "Template Input Action.cs": the class, OnCreate, OnUpdate and RemoveAllListeners already exist —
//    you add the events, the bindings inside OnCreate and the handlers.
public class PlayerAction : IInputAction
{
    public event System.Action<Vector2> onMove;
    public event System.Action onAttack;

    public void OnCreate()
    {
        var controls = Inputs.CM.GetControlMap<PlayerControls>();
        if (controls == null) return;
        controls.Player.Move.performed += this._OnMove;
        controls.Player.Move.canceled  += this._OnMove;
        controls.Player.Attack.performed += _ => this.onAttack?.Invoke();
    }

    public void OnUpdate(float dt) { /* optional polling */ }

    public void RemoveAllListeners()
    {
        this.onMove = null;
        this.onAttack = null;
    }

    private void _OnMove(InputAction.CallbackContext ctx) => this.onMove?.Invoke(ctx.ReadValue<Vector2>());
}

// 2) Bootstrap (once)
Inputs.CM.RegisterControlMap<PlayerControls>();
Inputs.IA.RegisterInputAction<PlayerAction>();

// 3) One owner drives the update loop
private void Update() => Inputs.IA.DriveUpdate(Time.deltaTime);

// 4) Gameplay/UI subscribes and unsubscribes
Inputs.IA.GetInputAction<PlayerAction>().onMove += this.OnMove;
Inputs.IA.GetInputAction<PlayerAction>().onMove -= this.OnMove;
```

Enable/disable input during UI modes or cutscenes:

```csharp
Inputs.CM.SetActive<PlayerControls>(false);                    // whole collection
Inputs.CM.GetControlMap<PlayerControls>()?.Player.Disable();   // single action map
Inputs.CM.GetControlMap<PlayerControls>()?.Player.Enable();
```

## Binding Composites (Unity New Input System)

For composite bindings (e.g., a 2D vector from four keys with custom processing), generate one with the Binding Composite entry point — never write the registration plumbing by hand. The generated `<Name>Composite : InputBindingComposite<<Name>Composite.<Name>>` already contains: the value struct `<Name>` with example fields, `[InputControl(layout = ...)]` part fields (`Button`, `Integer`, `Vector2`, `Axis` examples), `ReadValue(ref InputBindingCompositeContext context)`, a static constructor calling `InputSystem.RegisterBindingComposite<<Name>Composite>()`, `[InitializeOnLoad]` under `#if UNITY_EDITOR` and an empty `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Init()` that triggers that constructor at runtime. Your work is limited to the struct fields, the part fields and the body of `ReadValue`; leave the registration members untouched. The demo's `MoveInput` composite plus `PlayerControls.inputactions` shows the full setup — read them (`Samples~/InputDemo`) before authoring composites.

## Rules & pitfalls

- Do not register an `IInputAction` before the Control Map it reads is registered — `GetControlMap` returns null and bindings are silently skipped.
- Exactly one runtime owner should call `Inputs.IA.DriveUpdate(dt)`; forgetting it disables every `OnUpdate`-based action, calling it from several places double-ticks them.
- Gameplay/feature code subscribes to `IInputAction` events only; do not scatter raw `InputAction` reads through the project.
- Unsubscribe events when a feature exits; call `RemoveAllListeners()` (or re-register) on scene/domain transitions to avoid stale delegates.
- Registries are static: registering the same type twice is a no-op (first registration wins) — restart flows should account for that.
- `Inputs.CM.IsActive<T>()` follows `SetActive<T>()` only from v1.0.5 on; earlier versions kept returning the state from registration time.
- `Inputs.CM` / `Inputs.IA` are the registries. Do not add a parallel registry, a wrapper singleton or an event bus in front of them, and do not create `IInputAction` classes by hand — generate them.

## Verify

- A freshly generated Input Action or Binding Composite compiles as generated; if it does not, regenerate it instead of patching the skeleton.

- Compile, enter Play mode, confirm subscribed events fire for keyboard/gamepad, and that `SetActive<T>(false)` mutes input.
- If an `OnUpdate`-based behavior is dead, first check `DriveUpdate` is being called each frame.
