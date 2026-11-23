## CHANGELOG

## [1.1.5] - 2026-11-23
- Updated the GitHub URLs to the new location of the repositories (https://github.com/cout-m6o): package.json (author, dependencies) and the AI Agent Skill sample (install URL, links).

## [1.1.4] - 2026-11-23
- Fixed IntervalTimer/DTUpdater/RTUpdater staying stopped when stopped and started again in the same frame (or restarted from their own callback), the cancelled loop used to stop the new run one frame later.
- Optimized IntervalSetter with typed tables (int ids are no longer boxed on every lookup, no 32768-entry table reserved up front, the capacity limit is unchanged).
- Optimized DeltaTimer/RealTimer by removing their empty finalizers.
- Modified RealTimer/DTUpdater/RTUpdater to measure elapsed time with DateTime.UtcNow (no time zone conversion, unaffected by DST or time zone changes).
- Updated AI Agent Skill (working rules that forbid inventing a parallel convention, sample entry point, corrected RealTimer: it measures from its own creation and does not need RealTime.InitStartupTime).

## [1.1.3] - 2026-07-30
- Added AI Agent Skills sample (Samples~/AgentSkills/oxgkit-unity-skill).
- Optimized samples descriptions in package.json.
- Renamed private field _isRuning to _isRunning (IntervalTimer, RTUpdater, DTUpdater).
- Normalized Chinese comment punctuation to full-width (comments only, no code changes).
- Moved the AI Agent Skills sample entry to the first position in package.json samples.

## [1.1.2] - 2025-09-05
- Removed color from print output.

## [1.1.1] - 2025-07-10
- Organized print level.

## [1.1.0] - 2025-05-15
- Added NtpTime support for HTTP protocol on WebGL (using timeapi.io)

## [1.0.0] - 2025-03-19
- Added TimeSystem.