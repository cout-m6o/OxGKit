---
name: oxgkit-unity-skill
description: Use when fixing OxGKit.TweenSystem assembly reference errors caused by the DOTween.Modules asmdef GUID, including the OxGKit.TweenSystemFixer editor tool, the fixed GUID fdf3e181e62e9d243a7fee5ce890ab71, and manual DOTween.Modules.asmdef.meta repair after importing or upgrading DOTween Pro. Also covers its Editor entry points and Unity CLI commands (unity command oxgkit.tween).
---

# OxGKit.TweenSystemFixer Unity Skill

## Purpose

Make the agent able to diagnose and repair the one problem this module solves. `OxGKit.TweenSystem` references the `DOTween.Modules` assembly by GUID, but DOTween Pro generates `DOTween.Modules.asmdef` with a **random GUID** on each machine/setup — so the reference breaks (missing-assembly compile errors, `MenuItem`s not appearing). `OxGKit.TweenSystemFixer` (UPM package `com.michaelo.oxgkit.tweensystemfixer`, editor-only) reassigns the fixed GUID that `OxGKit.TweenSystem` expects.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Confirm the actual symptom first (missing `DOTween.Modules` reference in `OxGKit.TweenSystem`, GUID warnings, compile failure right after importing/upgrading DOTween) before prescribing the fix.
4. Warn about GUID-reassignment side effects: any other asmdef that referenced the old `DOTween.Modules` GUID must be updated to the fixed GUID too.
5. Obey the "Working rules" below: repair with the shipped tool, never with a GUID or a script of your own.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its samples and its Editor tooling. Follow that usage exactly. Never design your own variant of it: no other GUID, no edits to the references inside `OxGKit.TweenSystem`, and no home-made GUID-rewriting script.
- **Trigger, don't imitate.** Whatever the module's Editor tooling or shipped assets can provide (see "Fix option A") is obtained through that entry point — the same one a developer uses — and you continue from what it produced. Do not hand-build an equivalent from memory and do not re-implement an Editor tool.
- **Stay inside the shipped shape.** Configure what the module exposes (serialized fields, public API); do not patch the package, subclass around it or add a parallel mechanism next to it.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/TweenSystemFixer/Scripts`
- Editor-only; no dependencies. Only needed alongside `OxGKit.TweenSystem` + DOTween Pro.
- Editor tooling (assembly `OxGKit.TweenSystemFixer.Editor`, auto-referenced) and Unity CLI command (`unity command oxgkit.tween repair_dotween_modules_guid`, declared in `OxGKit.TweenSystemFixer.Editor.UnityCli`, compiled only when `com.unity.pipeline` 1.x is installed): see "Fix option A".

## The fixed GUID

```
fdf3e181e62e9d243a7fee5ce890ab71
```

`OxGKit.TweenSystem`'s asmdef references `DOTween.Modules` by this GUID.

## Prerequisites

1. DOTween Pro imported.
2. DOTween assemblies created: `Tools > Demigiant > DOTween Utility Panel > Create ASMDEF` (this generates `Plugins/Demigiant/DOTween/Modules/DOTween.Modules.asmdef`).

## Fix option A — tool (this package)

Always use the tool; take the first route you can really complete:

| Route | How | Searches | Dialog |
| --- | --- | --- | --- |
| Unity CLI (v1.0.3+) | `unity command oxgkit.tween repair_dotween_modules_guid --dry-run` to preview, then `--confirm` to apply | the whole `Assets/` folder | none |
| Editor code (v1.0.3+; every supported Unity version) | `OxGKit.TweenSystemFixer.Editor.DoTweenAssemblyFixer.RepairDoTweenModulesGUIDInProject()` — returns the asset path, `null` when the asmdef cannot be found | the whole `Assets/` folder | none |
| Menu (what a developer clicks) | select the folder that contains the DOTween modules (or `Assets`, or the asmdef itself), then `Assets > OxGKit TweenSystem GUID Fixer > Search And Reassign DOTween.Modules GUID` (`DoTweenAssemblyFixer.RepairDoTweenModulesGUID()`) | **only the current Project-window selection** (folders recursively) | a modal "Done" dialog that must be dismissed — do not trigger this route from automation |

- Needs Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor. The CLI command changes nothing when the GUID is already the fixed one, and refuses a change without `--confirm`.
- Every route reassigns the GUID in `DOTween.Modules.asmdef.meta`, rewrites the assets under `Assets/` that referenced the old GUID, then saves and refreshes the AssetDatabase (scripts recompile afterwards).
- `Cannot found DOTween.Modules.asmdef, repair failed` in the Console (CLI: exit code 13): the ASMDEF was never created (see Prerequisites), it lives outside `Assets/`, or — menu route — it is not inside the selection.

## Fix option B — manual

1. Close Unity (or ensure no import in progress).
2. Open `Assets/Plugins/Demigiant/DOTween/Modules/DOTween.Modules.asmdef.meta` in a text editor.
3. Replace the `guid:` value with `fdf3e181e62e9d243a7fee5ce890ab71`.
4. If other asmdefs in the project referenced the old GUID, update those references to the fixed GUID as well.
5. Reopen Unity / reimport.

## Rules & pitfalls

- This must be redone whenever DOTween is re-imported/upgraded in a way that regenerates the asmdef with a new GUID.
- Reassigning a GUID rewrites which asset existing references resolve to — apply it only to `DOTween.Modules.asmdef`, never as a general "fix GUIDs" habit.
- The tool searches `Assets/` only; UPM-packaged DOTween locations are out of scope for it (use the manual fix).
- Install order matters: install this fixer package and run the repair **before** `OxGKit.TweenSystem` is added (or while the project still compiles) — Editor tooling, menus and CLI commands are only available when the project's scripts compile.
- Source-control note: commit the changed `.meta` file, or teammates hit the same breakage.

## Verify

- After the fix, `OxGKit.TweenSystem` compiles, its `Add Component > OxGKit > TweenSystem > ...` entries appear, and Console shows no missing-GUID/assembly warnings.
