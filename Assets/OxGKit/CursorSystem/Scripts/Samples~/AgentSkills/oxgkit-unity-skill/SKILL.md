---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity projects that use OxGKit.CursorSystem, including the Cursors static facade, the CursorManager prefab and its CursorState list, static/dynamic (frame-animated) cursor textures, cursor visibility, CursorLockMode handling, hotspot/scale settings, and runtime cursor state switching.
---

# OxGKit.CursorSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.CursorSystem user. The module (UPM package `com.michaelo.oxgkit.cursorsystem`, dependency-free) manages mouse-cursor rendering as named **cursor states** — each state is a static texture or a frame-animated (dynamic) texture sequence with hotspot/scale settings — plus visibility and lock-state control. Typical use: simulation/management games where the cursor changes per interaction mode.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.cursorsystem@*` or `Assets/OxGKit/CursorSystem/Scripts` when embedded) and inspect the scene's `CursorManager` state list before writing code; state names are data, not constants — do not invent them.
4. For WebGL targets, flag browser cursor restrictions (custom cursor size limits, lock-state needing a user gesture) when relevant.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its samples and its Editor tooling. Follow that usage exactly. Never design your own variant of it: no second cursor manager, cursor-state enum or constants table, state machine or `Cursor.SetCursor` call that bypasses the manager.
- **Trigger, don't imitate.** Whatever the module's Editor tooling or shipped assets can provide (see "Editor entry points") is obtained through that entry point — the same one a developer uses — and you continue from what it produced. Do not hand-build an equivalent from memory and do not re-implement an Editor tool.
- **Stay inside the shipped shape.** Configure what the module exposes (serialized fields, public API); do not patch the package, subclass around it or add a parallel mechanism next to it.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/CursorSystem/Scripts`
- No dependencies.
- Samples (Package Manager > OxGKit.CursorSystem > Samples): `CursorManager Prefab` (pre-wired manager), `CursorManager Demo`, `AI Agent Skills` (this skill).

## Editor entry points (trigger, don't hand-write)

This module ships no generator: the cursor setup is the `CursorManager` component, taken from the shipped prefab and configured through its serialized state list.

| You need | What a developer does | Agent equivalent |
| --- | --- | --- |
| The manager in the boot scene | Package Manager > OxGKit.CursorSystem > Samples > `CursorManager Prefab` > Import, then drag `CursorManager.prefab` into the scene | import the sample (see below), then instantiate the prefab — `unity command prefab instantiate` (Unity CLI) or `PrefabUtility.InstantiatePrefab` (editor code) |
| A working reference scene | Package Manager > OxGKit.CursorSystem > Samples > `CursorManager Demo` > Import | read it before authoring states |

Importing the `CursorManager Prefab` sample (same as the Import button of Package Manager > OxGKit.CursorSystem > Samples; Unity copies it to `Assets/Samples/OxGKit.CursorSystem/<version>/CursorManager Prefab`):

```csharp
// Editor code
var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.michaelo.oxgkit.cursorsystem");
foreach (var sample in UnityEditor.PackageManager.UI.Sample.FindByPackage(package.name, package.version))
    if (sample.displayName == "CursorManager Prefab" && !sample.isImported)
        sample.Import(UnityEditor.PackageManager.UI.Sample.ImportOptions.None);
```

Without Editor access, copy the package's `Samples~/CursorManagerPrefab` folder (with its `.meta` files) to that destination instead. Use this prefab as the manager; do not build a look-alike object for it.

Cursor states are data authored on that component (Inspector, or `unity command component set_properties` / `SerializedObject`); read the existing list before adding to it and never mirror the state names in an enum or constants class of your own.

## Setup

1. Import the `CursorManager Prefab` sample and drop the prefab into the boot scene (or let `Cursors.InitInstance()` create/find the manager).
2. Author `CursorState` entries in the inspector: `stateName`, `RenderType` (`Static` / `Dynamic`), `cursorMode`, optional `scalingEnabled` + `scale`, `hotspot`, and either `staticCursorTexture` or `dynamicCursorTextures` (+ `isLoop`, `PlayMode`, `frameRate`).
3. The **first** state in the list is the default state used by `ResetCursorState()`.

## Core API (verified)

```csharp
using OxGKit.CursorSystem;
using UnityEngine;

Cursors.InitInstance();                       // initialize the CursorManager instance

// Visibility & lock
Cursors.IsCursorVisible();
Cursors.SetCursorVisible(bool visible);
Cursors.GetCurrentCursorLockState();          // CursorLockMode
Cursors.SetCursorLockState(CursorLockMode.Locked / .Confined / .None);

// States
Cursors.GetAllCursorStates();                 // CursorManager.CursorState[]
Cursors.GetCursorState(string stateName);
Cursors.GetCurrentCursorState();
Cursors.SetCursorState(string stateName);     // bool: switch by name
Cursors.ResetCursorState();                   // back to the default (first) state
Cursors.SetIgnoreScale(bool ignore);          // dynamic playback ignores Time.timeScale
Cursors.SetScaleToAllCursors(Vector2 scale);
Cursors.ResetRender();
Cursors.RemoveCursorRender();                 // hide custom render; ResetCursorState() restores
```

`CursorManager.CursorState` members (for runtime authoring, e.g. textures loaded from bundles):

```csharp
var state = Cursors.GetCursorState("Default");
state.SetStaticCursor(texture2d);             // also switches RenderType usage
state.SetDynamicCursor(texture2dArray);       // frame sequence
state.SetCursorOffset(hotspot);               // hotspot position
state.SetCursorScale(scale);
state.SetCursorMode(CursorMode.Auto);
state.SetFrameRate(12);
state.SetLoop(true);
state.ResetRender();
```

## Usage patterns

Runtime switching:

```csharp
Cursors.InitInstance();
Cursors.SetCursorState("Attack");    // e.g. hovering an enemy
Cursors.ResetCursorState();          // back to default on exit
```

FPS-style capture / release:

```csharp
Cursors.SetCursorVisible(false);
Cursors.SetCursorLockState(CursorLockMode.Locked);
// on menu open:
Cursors.SetCursorLockState(CursorLockMode.None);
Cursors.SetCursorVisible(true);
```

Assigning downloaded/bundled textures at runtime:

```csharp
Texture2D tex = LoadCursorTextureSomehow();
Cursors.GetCursorState("Default").SetStaticCursor(tex);
Cursors.SetCursorVisible(true);
```

## Rules & pitfalls

- State names must match the inspector data exactly; `SetCursorState` returns `false` for unknown names — check it during development.
- Dynamic cursors animate on the manager's update; use `SetIgnoreScale(true)` if cursor animation must continue while the game is paused (`Time.timeScale = 0`).
- `RemoveCursorRender()` removes the custom cursor rendering; call `ResetCursorState()` (not just `SetCursorVisible`) to restore it.
- Cursor textures should be imported with `Cursor` texture type for crisp rendering; on WebGL, prefer the browser default cursor or small textures — large custom cursors may be rejected by browsers.
- Keep one `CursorManager` (boot scene / DontDestroy); do not scatter direct `UnityEngine.Cursor` calls alongside it — route everything through `Cursors` to keep state consistent.

## Verify

- Enter Play mode: switch each named state, toggle visibility, and cycle lock modes; confirm hotspot alignment by clicking precise UI targets.
- For dynamic states, confirm frame rate/loop settings and pause behavior (`SetIgnoreScale`).
