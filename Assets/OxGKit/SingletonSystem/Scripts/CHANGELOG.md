## CHANGELOG

## [1.0.4] - 2026-11-23
- Updated the GitHub URLs to the new location of the repositories (https://github.com/cout-m6o): package.json (author, dependencies) and the AI Agent Skill sample (install URL, links).

## [1.0.3] - 2026-11-23
- Added SingletonCreateScriptEditor.CreateScriptTplMonoSingletonAtPath(assetPath) to create a MonoSingleton script from the template directly at a path, without the name editing of the Project window (same content as the create menu).
- Added Unity CLI support: unity command oxgkit.singleton create_mono_singleton (optional assembly OxGKit.SingletonSystem.Editor.UnityCli, compiled only when the Unity Pipeline package com.unity.pipeline 1.x is installed).
- Updated AI Agent Skill (working rules that forbid inventing a parallel convention, Editor entry points with the Unity CLI / editor code / menu / template-file routes).

## [1.0.2] - 2026-07-30
- Added AI Agent Skills sample (Samples~/AgentSkills/oxgkit-unity-skill).
- Optimized samples descriptions in package.json.
- Improved NewSingleton.GetInstance with double-checked locking (avoids duplicate creation under race conditions).
- Moved the AI Agent Skills sample entry to the first position in package.json samples.

## [1.0.1] - 2025-04-08
- Added InitInstance method.
- Modified the default value of dontDestroyOnLoad in MonoSingleton to true.

## [1.0.0] - 2025-03-19
- Added SingletonSystem.