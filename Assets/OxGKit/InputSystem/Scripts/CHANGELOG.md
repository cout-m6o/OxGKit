## CHANGELOG

## [1.0.6] - 2026-11-23
- Updated the GitHub URLs to the new location of the repositories (https://github.com/cout-m6o): package.json (author, dependencies) and the AI Agent Skill sample (install URL, links).

## [1.0.5] - 2026-11-23
- Fixed Inputs.CM.IsActive always returning true (SetActive modified a copy of the ControlMap struct).
- Optimized control map / input action lookups (TryGetValue) and added double-checked locking for the InputCenter instance.
- Added InputSystemCreateScriptEditor.CreateScriptTplInputActionAtPath(assetPath) and CreateScriptTplInputBindingCompositeAtPath(assetPath) to create the scripts from the templates directly at a path, without the name editing of the Project window (same content as the create menus).
- Added Unity CLI support: unity command oxgkit.input create_action / create_binding_composite (optional assembly OxGKit.InputSystem.Editor.UnityCli, compiled only when the Unity Pipeline package com.unity.pipeline 1.x is installed).
- Updated AI Agent Skill (working rules that forbid inventing a parallel convention, Editor entry points with the Unity CLI / editor code / menu / template-file routes, corrected the Binding Composite template description).

## [1.0.4] - 2026-07-30
- Added AI Agent Skills sample (Samples~/AgentSkills/oxgkit-unity-skill).
- Optimized samples descriptions in package.json.
- Moved the AI Agent Skills sample entry to the first position in package.json samples.

## [1.0.3] - 2025-09-05
- Defined Inputs.IA.DriveUpdate method name (obsolete Inputs.IA.UpdateInputActions).
- Removed color from print output.

## [1.0.2] - 2025-07-10
- Organized print level.

## [1.0.1] - 2024-03-15
- Modified set #ROOTNAMESPACE# symbol in script templates.

## [1.0.0] - 2024-02-02
- Stabled

## [0.0.4-preview] - 2023-10-30
- Renamed OnInit to OnCreate (interface IInputAction).
```C#
    void OnCreate();
```

## [0.0.3-preview] - 2023-09-22
- Added default constructor for Logger.

## [0.0.2-preview] - 2023-08-27
- Added Logger by LoggingSystem.
- Renamed Inputs.IA.OnUpdateInputActions to Inputs.IA.UpdateInputActions.

## [0.0.1-preview] - 2023-05-21
- Added InputSystem.