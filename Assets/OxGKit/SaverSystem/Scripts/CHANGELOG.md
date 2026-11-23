## CHANGELOG

## [1.0.4] - 2026-11-23
- Updated the GitHub URLs to the new location of the repositories (https://github.com/cout-m6o): package.json (author, dependencies) and the AI Agent Skill sample (install URL, links).

## [1.0.3] - 2026-11-23
- Fixed SaveData/DeleteData corrupting other keys whose name ends with the same text (e.g. "volume" and "bgm_volume"), the data line of a key is now located line by line instead of replacing substrings in the whole text.
- Fixed DeleteData not removing a line that ends with CRLF or a last line without a newline (it now removes every line of the key, including duplicated lines), and SaveData appending onto the last line when the stored text does not end with a newline.
- Optimized SaveData/DeleteData (no full parse per call) and ParsingDataMap (scans the text directly, same result with fewer allocations).
- Added regression tests for data lines (SaverTests, in-memory saver).
- Fixed the Editor test assembly (OxGKit.SaverSystem.Editor.Tests) being compiled in every project that installs the package: it now has the UNITY_INCLUDE_TESTS define constraint, so a project without the Test Framework package no longer gets compile errors (NUnit not found) and the tests no longer show up in the Test Runner of consumer projects.
- Updated AI Agent Skill (working rules that forbid inventing a parallel convention, EditorPrefsSaver entry point, data line fix note).

## [1.0.2] - 2026-07-30
- Added AI Agent Skills sample (Samples~/AgentSkills/oxgkit-unity-skill).
- Added samples section to package.json.
- Optimized ParsingDataMap to iterate the split array directly (removes a redundant List copy).

## [1.0.1] - 2025-07-10
- Added Dispose method to Saver.

## [1.0.0] - 2025-04-10
- Added SaverSystem.