## CHANGELOG

## [1.0.7] - 2026-11-23
- Updated the GitHub URLs to the new location of the repositories (https://github.com/cout-m6o): package.json (author, dependencies) and the AI Agent Skill sample (install URL, links).

## [1.0.6] - 2026-11-23
- Optimized NoticeManager/NoticeItem/ListSet lookups (single dictionary/set lookup instead of Contains + indexer).
- Optimized notice logs to build their messages only when the logger will print them (uses Logging.IsLogActive, requires OxGKit.LoggingSystem 1.4.4+).
- Fixed NoticeItem.DeregisterNotice throwing (NullReferenceException or MissingReferenceException) when it is called for a notice item that is already destroyed, e.g. from another script's OnDestroy while a scene unloads or play mode stops (the NoticeDemo sample does this).
- Added NoticeSystemCreateScriptEditor.CreateScriptTplNoticeConditionAtPath(assetPath), CreateScriptTplNoticeConditionWithBeforeSceneLoadAtPath(assetPath) and CreateScriptTplNoticeConditionRegistersAtPath(assetPath) to create the scripts from the templates directly at a path, without the name editing of the Project window (same content as the create menus).
- Added Unity CLI support: unity command oxgkit.notice create_condition / create_condition_before_scene_load / create_condition_registers (optional assembly OxGKit.NoticeSystem.Editor.UnityCli, compiled only when the Unity Pipeline package com.unity.pipeline 1.x is installed).
- Updated AI Agent Skill (working rules that forbid inventing a parallel convention, Editor entry points with the Unity CLI / editor code / menu / template-file routes, condition and register examples follow the generated templates).

## [1.0.5] - 2026-07-30
- Added AI Agent Skills sample (Samples~/AgentSkills/oxgkit-unity-skill).
- Optimized samples descriptions in package.json.
- Added non-alloc NoticeManager.Notify(int conditionId) overload (params overload and internal notify paths route through it).
- Optimized NoticeItem.GetNoticeInfos to avoid LINQ ToArray allocation.
- Fixed comment and script template typos (Default region, Cond Id, notice items).
- Normalized Chinese comment punctuation to full-width (comments only, no code changes).
- Moved the AI Agent Skills sample entry to the first position in package.json samples.

## [1.0.4] - 2025-09-05
- Removed color from print output.

## [1.0.3] - 2025-07-10
- Organized print level.

## [1.0.2] - 2024-10-31
- Modified when noticeItem is detected as destroyed, it will be removed from condition.

## [1.0.1] - 2024-03-15
- Modified set #ROOTNAMESPACE# symbol in script templates.

## [1.0.0] - 2024-02-02
- Stabled.

## [0.0.5-preview] - 2023-09-22
- Added default constructor for Logger.

## [0.0.4-preview] - 2023-08-27
- Added Logger by LoggingSystem.

## [0.0.3-preview] - 2023-05-26
- Added NoticeItem can deregister specific condition id.
- Modified NoticeItem RenewNotice method params use NoticeInfo instead, also supports method chaining.
- Extended Notify with NoticeItem (NoticeManager).
- Optimized NoticeItem cache.

## [0.0.2-preview] - 2023-05-22
- Added ListSet.
- Added Notify for NoticeItem (can notify by notice item).
- Added RenewNotice for NoticeItem (If use value type data must renew).
- Modified RegisterNotice supports assign to renew NoticeItem.
- Optimized cache operation use ListSet instead.

## [0.0.1-preview] - 2023-05-21
- Added NoticeSystem.