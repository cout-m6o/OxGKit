---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity UGUI projects that use OxGKit.ButtonSystem, including the ButtonPlus extended Button (long-click Once/Continuous/PressedAndReleased modes, triggerTime/intervalTime, scale transition, onLongClickPressed/onLongClickReleased) and the InputFieldPlaceholderRemover helper. Also covers its Editor entry points and Unity CLI commands (unity command oxgkit.button).
---

# OxGKit.ButtonSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.ButtonSystem user. The module (UPM package `com.michaelo.oxgkit.buttonsystem`, dependency-free) extends UGUI: **`ButtonPlus`** inherits `UnityEngine.UI.Button` and adds long-click behaviors and a scale press-transition, and **`InputFieldPlaceholderRemover`** hides an InputField/TMP_InputField placeholder while it is focused.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.buttonsystem@*` or `Assets/OxGKit/ButtonSystem/Scripts` when embedded). Do not invent APIs.
4. Respect the target project's UI event-wiring conventions (helper extensions, bind-once lifecycles) before adding listeners.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its samples and its Editor tooling. Follow that usage exactly. Never design your own variant of it: no home-made long-press component, button base class, click-throttling wrapper or "ButtonPlus-like" prefab that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling or shipped assets can provide (see "Editor entry points") is obtained through that entry point — the same one a developer uses — and you continue from what it produced. Do not hand-build an equivalent from memory and do not re-implement an Editor tool.
- **Stay inside the shipped shape.** Configure what the module exposes (serialized fields, public API); do not patch the package, subclass around it or add a parallel mechanism next to it.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/ButtonSystem/Scripts`
- No dependencies.
- Samples (Package Manager > OxGKit.ButtonSystem > Samples): `ButtonPlus Demo`, `AI Agent Skills` (this skill).
- Editor tooling (assembly `OxGKit.ButtonSystem.Editor`, auto-referenced) and Unity CLI command (`unity command oxgkit.button ...`, declared in `OxGKit.ButtonSystem.Editor.UnityCli`, compiled only when `com.unity.pipeline` 1.x is installed): see "Editor entry points".

## Editor entry points (trigger, don't hand-write)

| You need | Unity CLI command | Menu a developer clicks | Editor method |
| --- | --- | --- | --- |
| A new ButtonPlus with a TextMeshPro label | `unity command oxgkit.button create [--parent <handle>]` (v1.0.3+) | `GameObject > UI > Button Plus - TextMeshPro` | `OxGKit.ButtonSystem.Editor.ButtonPlusGeneratorMenu.AddButtonTMP(MenuCommand)` |
| A new ButtonPlus with a legacy `Text` label | `unity command oxgkit.button create --label Legacy [--parent <handle>]` (v1.0.3+) | `GameObject > UI > Legacy > Button Plus` | `OxGKit.ButtonSystem.Editor.ButtonPlusGeneratorMenu.AddButtonLegacy(MenuCommand)` |
| `ButtonPlus` on an existing GameObject | `unity command component add --target <GameObject> --type OxGKit.ButtonSystem.ButtonPlus` | `Add Component > OxGKit > ButtonSystem > Button > ButtonPlus` | menu item `Component/OxGKit/ButtonSystem/Button/ButtonPlus` on the selected GameObject |

What the two create entry points produce (verified on Unity 6000.3): a GameObject `Button Plus` (160 × 30) with `Image` + `ButtonPlus` and one child label reading "Button Plus" — `Text (TMP)` (`TextMeshProUGUI`, font size 24) or `Text (Legacy)` (`Text`, font size 14). It is placed under the given parent; without one, under the Canvas of the current selection or any Canvas of the open stage, and when there is none a `Canvas` (+ `CanvasScaler`, `GraphicRaycaster`) and an `EventSystem` (+ `StandaloneInputModule`) are created first. The new object ends up selected and the operation is undoable (from v1.0.3 Undo removes everything that was created; earlier versions left the `Button Plus` object and its label in the scene). On Unity 6 the built-in UI entries live under `GameObject > UI (Canvas)`, while the Button Plus entries stay under `GameObject > UI`.

How to trigger them — take the first route you can really complete:

1. **Unity CLI.** Needs Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor. `unity command oxgkit.button` lists the command with its arguments.

   ```
   unity command oxgkit.button create --parent /Canvas/Panel
   ```

   `--parent` takes a pipeline object handle (a hierarchy path such as `/Canvas/Panel`, an `instanceId` or a `globalId`); the result is the new object's identity (`hierarchyPath`, `instanceId`, `globalId`) for follow-up commands such as `component set_properties`.
2. **Editor code** (a bridge that runs C# in the Editor; every supported Unity version): `ButtonPlusGeneratorMenu.AddButtonTMP(new MenuCommand(parentGameObject));` — `new MenuCommand(null)` behaves like the main menu.
3. **Menu items only** (a bridge that can only execute menu items, or `unity command editor invoke_menu --path "<menu path>"`): select the parent if you need one, then execute `GameObject/UI/Button Plus - TextMeshPro` or `GameObject/UI/Legacy/Button Plus`.
4. **None of these**: ask the developer to click the menu.

Then configure the created `ButtonPlus` through its serialized fields (see "Core API"). Do not assemble a look-alike by hand (GameObject + Image + ButtonPlus + label) and do not subclass `ButtonPlus` to add behaviour it already has.

## Core API (verified)

`ButtonPlus : UnityEngine.UI.Button` — everything from `Button` (`onClick`, `interactable`, transitions, ...) plus:

```csharp
using OxGKit.ButtonSystem;

ButtonPlus btn;

// Extended press transition
btn.extdTransition;        // ExtdTransition.None | Scale
btn.transScale.size;       // pressed scale factor (default 0.95)

// Long click
btn.extdLongClick;         // ExtdLongClick.None | Once | Continuous | PressedAndReleased
btn.ignoreTimeScale;       // long-click timing unaffected by Time.timeScale (default true)
btn.triggerTime;           // seconds held before long click triggers (default 1)
btn.intervalTime;          // repeat interval for Continuous (default 0.1)
btn.onLongClickPressed;    // ButtonClickedEvent
btn.onLongClickReleased;   // ButtonClickedEvent
```

Long-click modes:

| Mode | Behavior |
| --- | --- |
| `Once` | after holding `triggerTime`, fires `onLongClickPressed` once |
| `Continuous` | after `triggerTime`, fires `onLongClickPressed` every `intervalTime` while held |
| `PressedAndReleased` | fires `onLongClickPressed` at `triggerTime`; fires `onLongClickReleased` on pointer up/exit |

Click interplay: when a long click has triggered, the subsequent pointer-up does **not** invoke `onClick` (the component temporarily swaps the event out) — a hold is either a long click or a click, never both. Pressed-state visuals (including the Scale transition) reset on click/exit.

## Usage patterns

```csharp
// Normal click (inherited)
btn.onClick.AddListener(this.OnConfirm);

// Hold-to-repeat (e.g. +1 per 0.1s while held)
btn.extdLongClick = ButtonPlus.ExtdLongClick.Continuous;
btn.triggerTime = 0.5f;
btn.intervalTime = 0.1f;
btn.onLongClickPressed.AddListener(this.OnIncrease);

// Hold-to-preview (press/release pair)
btn.extdLongClick = ButtonPlus.ExtdLongClick.PressedAndReleased;
btn.onLongClickPressed.AddListener(this.OnShowPreview);
btn.onLongClickReleased.AddListener(this.OnHidePreview);

// Cleanup on release/destroy
btn.onClick.RemoveAllListeners();
btn.onLongClickPressed.RemoveAllListeners();
btn.onLongClickReleased.RemoveAllListeners();
```

`InputFieldPlaceholderRemover`: add the component onto the same GameObject as a `UnityEngine.UI.InputField` or `TMP_InputField`; the placeholder hides on select/focus and restores on deselect. No API calls needed.

## Rules & pitfalls

- Register listeners once (bind phase), not in repeated show/refresh callbacks — `ButtonClickedEvent` accumulates duplicates.
- `extdTransition = Scale` multiplies the transform scale on press; it composes with (does not replace) the inherited UGUI `transition` — designers may use both. Do not overwrite designer-authored `Button.colors`/transition settings from runtime refresh code.
- `PressedAndReleased` fires the released event on pointer **exit** as well; handlers must tolerate release-without-move-back.
- `ignoreTimeScale` defaults to true — long clicks still work in pause menus; set it false only when hold timing should freeze with gameplay.
- In pooled/persistent UI, remove listeners on release, or route wiring through the project's existing button helper if one exists.
- ButtonPlus works with any UGUI setup (Legacy Text or TMP); the create-menu items just scaffold the matching label type.

## Verify

- Enter Play mode: short-press fires `onClick` only; hold past `triggerTime` fires the long-click events and suppresses `onClick`; visual scale returns to normal after release.
- With `Time.timeScale = 0`, confirm long-click behavior matches the `ignoreTimeScale` setting.
