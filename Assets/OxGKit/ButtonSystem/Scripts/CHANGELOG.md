## CHANGELOG

## [1.0.4] - 2026-11-23
- Updated the GitHub URLs to the new location of the repositories (https://github.com/cout-m6o): package.json (author, dependencies) and the AI Agent Skill sample (install URL, links).

## [1.0.3] - 2026-11-23
- Fixed the Button Plus create menus (GameObject/UI/Button Plus - TextMeshPro, GameObject/UI/Legacy/Button Plus) not being fully undoable: Undo removed the new Canvas / EventSystem but left the created Button Plus and its text object in the scene (they are removed now and Redo restores them).
- Added Unity CLI support: unity command oxgkit.button create (optional assembly OxGKit.ButtonSystem.Editor.UnityCli, compiled only when the Unity Pipeline package com.unity.pipeline 1.x is installed).
- Updated AI Agent Skill (working rules that forbid inventing a parallel convention, Editor entry points with the Unity CLI / editor code / menu / template-file routes).

## [1.0.2] - 2026-07-30
- Added AI Agent Skills sample (Samples~/AgentSkills/oxgkit-unity-skill).
- Optimized samples descriptions in package.json.
- Moved the AI Agent Skills sample entry to the first position in package.json samples.

## [1.0.1] - 2025-03-27
- Added InputFieldPlaceholderRemover.

## [1.0.0] - 2025-03-19
- Added ButtonSystem.