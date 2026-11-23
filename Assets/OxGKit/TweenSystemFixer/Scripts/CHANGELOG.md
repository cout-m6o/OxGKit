## CHANGELOG

## [1.0.3] - 2026-11-23
- Added DoTweenAssemblyFixer.RepairDoTweenModulesGUIDInProject() (same repair as the menu, searches the whole Assets folder without a selection and shows no dialog), FindDoTweenModulesAssetPath() and doTweenModulesFixedGUID.
- Added Unity CLI support: unity command oxgkit.tween repair_dotween_modules_guid (optional assembly OxGKit.TweenSystemFixer.Editor.UnityCli, compiled only when the Unity Pipeline package com.unity.pipeline 1.x is installed).
- Updated AI Agent Skill (working rules, repair routes: Unity CLI / editor code / menu, the menu only searches the current selection and ends with a dialog).

## [1.0.2] - 2026-07-30
- Added AI Agent Skills sample (Samples~/AgentSkills/oxgkit-unity-skill).
- Added samples section to package.json.

## [1.0.1] - 2025-09-05
- Removed color from print output.

## [1.0.0] - 2023-07-09
- Added TweenSystemFixer.