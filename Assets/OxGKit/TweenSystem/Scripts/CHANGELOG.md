## CHANGELOG

## [1.0.6] - 2026-11-23
- Updated the GitHub URLs to the new location of the repositories (https://github.com/cout-m6o): package.json (author, dependencies) and the AI Agent Skill sample (install URL, links).

## [1.0.5] - 2026-11-23
- Optimized DoTweenAnim by removing the duplicated interval timer creation in every tween constructor, and GetMaxDurationTween no longer allocates (same result).
- Updated AI Agent Skill (working rules that forbid inventing a parallel convention, entry points for the DoTweenAnim / DoTweenAnimEvent components and the DOTween.Modules GUID repair).

## [1.0.4] - 2026-07-30
- Added AI Agent Skills sample (Samples~/AgentSkills/oxgkit-unity-skill).
- Optimized samples descriptions in package.json.
- Fixed region comment typo (tRoatation -> tRotation).
- Moved the AI Agent Skills sample entry to the first position in package.json samples.

## [1.0.3] - 2025-11-20
- Fixed an issue where DoTweenAnimeEvent triggered the endCallback even when the animation had not finished playing.

## [1.0.2] - 2025-03-19
- Modified to use OxGKit.TimeSystem plugin.

## [1.0.1] - 2025-01-12
- Added methods with an endCallback parameter to DoTweenAnimEvent.
- Modified DoTweenAnim to allow editing in Prefab Isolation mode during Play Mode.
- Fixed issue with DoTweenAnimEvent's parallel mode not correctly calling the endCallback.

## [1.0.0] - 2024-02-02
- Stabled.

## [0.0.4-preivew] - 2023-07-23
- Added preview feature for DoTweenAnime.

## [0.0.3-preview] - 2023-07-09
- Added methods for DoTweenAnimeEvent, can dynamic add and remove DoTweenAnime.
- Removed DoTweenAssemblyFixer use and install TweenSystemFixer instead.
  - Extra install Add https://github.com/cout-m6o/OxGKit.git?path=Assets/OxGKit/TweenSystemFixer/Scripts to Package Manager

## [0.0.2-preview] - 2023-07-09
- Added DoTweenAssemblyFixer to fix error GUID.

## [0.0.1-preview] - 2023-07-08
- Added TweenSystem.