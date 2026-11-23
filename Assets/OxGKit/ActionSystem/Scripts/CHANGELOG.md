## CHANGELOG

## [1.0.4] - 2026-11-23
- Fixed ParallelAction stopping every remaining action once an action added before them finished (the group never completed), finished actions are now skipped instead.
- Fixed ParallelDelayAction updating actions before their staggered start (DelayAction.onEndAction fired immediately, and the group could finish early or never).
- Optimized QueueSet.Remove to remove in place (no LINQ / new Queue allocation) and QueueSet to create its internal containers on first use (an action without sub actions allocates about half as much).
- Optimized Reporter to build log messages only when the logger will print them (uses Logging.IsLogActive, requires OxGKit.LoggingSystem 1.4.4+).
- Added ActionSystemCreateScriptEditor.CreateScriptTplActionAtPath(assetPath) to create an action script from the template directly at a path, without the name editing of the Project window (same content as the create menu).
- Added Unity CLI support: unity command oxgkit.action create (optional assembly OxGKit.ActionSystem.Editor.UnityCli, compiled only when the Unity Pipeline package com.unity.pipeline 1.x is installed).
- Updated AI Agent Skill (working rules that forbid inventing a parallel convention, Editor entry points with the Unity CLI / editor code / menu / template-file routes, custom actions start from the generated template, corrected the runner update entry point to DriveUpdate).

## [1.0.3] - 2026-07-30
- Modified ActionRunner public update entry point to DriveUpdate(dt) for consistent Drive* naming across OxGKit systems (OnUpdate(dt) is now protected and driven by DriveUpdate).
- Added AI Agent Skills sample (Samples~/AgentSkills/oxgkit-unity-skill).
- Optimized samples descriptions in package.json.
- Optimized QueueSet.ToArray with a cached snapshot (eliminates per-frame array allocations in ActionRunner/actions update).
- Fixed ParallelAction/ParallelDelayAction default name assignment (was DelayAction/ParallelAction).
- Fixed ActionBase.GetTimeElapsed returned duration instead of elapsed time.
- Moved the AI Agent Skills sample entry to the first position in package.json samples.

## [1.0.2] - 2025-09-05
- Removed color from print output.

## [1.0.1] - 2024-03-15
- Modified set #ROOTNAMESPACE# symbol in script templates.

## [1.0.0] - 2024-02-02
- Stabled

## [0.0.4-preview] - 2023-09-22
- Added default constructor for Logger.

## [0.0.3-preview] - 2023-08-27
- Added Logger by LoggingSystem.

## [0.0.2-preview] - 2023-05-21
- Fixed OnDone invoke timing (If call MarkAsDone will invoke OnDone).
- Modified done flags are IsDone and IsAllDone. 

## [0.0.1-preview] - 2023-05-21
- Added ActionSystem.