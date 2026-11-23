---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity projects that use OxGKit.TimeSystem, including RealTime startup time, RealTimer and DeltaTimer (timer/tick/mark), RTUpdater and DTUpdater update loops with timeScale and targetFrameRate, IntervalSetter/IntervalTimer periodic calls, and NtpTime NTP server clock synchronization.
---

# OxGKit.TimeSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.TimeSystem user. The module (UPM package `com.michaelo.oxgkit.timesystem`, UniTask-based) provides time controllers: real-time and delta-time timers, standalone update loops that run independently of MonoBehaviours, `setInterval`-style periodic tickers, and NTP clock synchronization for server-authoritative time.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.timesystem@*` or `Assets/OxGKit/TimeSystem/Scripts` when embedded). Do not invent APIs.
4. When choosing a timer/updater, state whether the use case must ignore `Time.timeScale` (real time) or follow it (game time) — that decides Real* vs Delta*/DT*.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its samples and its Editor tooling. Follow that usage exactly. Never design your own variant of it: no home-made timer class, coroutine-based interval helper, update-loop manager or server-clock wrapper that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling or shipped assets can provide (see "Editor entry points") is obtained through that entry point — the same one a developer uses — and you continue from what it produced. Do not hand-build an equivalent from memory and do not re-implement an Editor tool.
- **Stay inside the shipped shape.** Configure what the module exposes (serialized fields, public API); do not patch the package, subclass around it or add a parallel mechanism next to it.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/TimeSystem/Scripts`
- Auto dependencies: `com.cysharp.unitask`, `com.michaelo.oxgkit.loggingsystem`.
- Samples (Package Manager > OxGKit.TimeSystem > Samples): `Timer Demo`, `AI Agent Skills` (this skill).

## Editor entry points (trigger, don't hand-write)

This module ships no Editor tooling and no generator — its whole surface is the runtime API below. The entry point for an agent is the shipped sample:

| You need | What a developer does | Agent equivalent |
| --- | --- | --- |
| The reference usage of every timer/updater | Package Manager > OxGKit.TimeSystem > Samples > `Timer Demo` > Import | import the sample (see below) and read it before choosing a timer |

Importing the `Timer Demo` sample (same as the Import button of Package Manager > OxGKit.TimeSystem > Samples; Unity copies it to `Assets/Samples/OxGKit.TimeSystem/<version>/Timer Demo`):

```csharp
// Editor code
var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.michaelo.oxgkit.timesystem");
foreach (var sample in UnityEditor.PackageManager.UI.Sample.FindByPackage(package.name, package.version))
    if (sample.displayName == "Timer Demo" && !sample.isImported)
        sample.Import(UnityEditor.PackageManager.UI.Sample.ImportOptions.None);
```

Without Editor access, copy the package's `Samples~/TimerDemo` folder (with its `.meta` files) to that destination instead. It is reference material, not a framework layer to copy into the project.

## Module map (pick the right tool)

| Type | Time base | Driven by | Use for |
| --- | --- | --- | --- |
| `RealTimer` | real clock, measured from the timer's own creation | self (query-based) | cooldowns/countdowns that must ignore pause & timeScale |
| `DeltaTimer` | accumulated dt you feed | owner calls `UpdateTimer(dt)` | game-time cooldowns that pause with your feature |
| `RTUpdater` | real clock (UniTask loop) | `Start()` / `StartOnThread()` | update loops independent of Unity timeScale (e.g. network heartbeat) |
| `DTUpdater` | Unity-scaled time (UniTask loop) | `Start()` | update loops that respect `Time.timeScale` |
| `IntervalSetter` / `IntervalTimer` | ms interval | static helper | fire an `Action` every N milliseconds |
| `NtpTime` | NTP / time API server | `Synchronize()` once | anti-cheat server clock, daily reset timing |

## Core API (verified)

```csharp
using OxGKit.TimeSystem;

// --- RealTime (optional: records the app's startup time for your own use; no timer depends on it) ---
RealTime.InitStartupTime();           // call once at boot if you read timeSinceStartup
bool inited = RealTime.IsInitStartupTime();
System.DateTime startup = RealTime.timeSinceStartup;

// --- RealTimer / DeltaTimer (same surface; DeltaTimer adds UpdateTimer) ---
var timer = new RealTimer(true);       // autoPlay: true
timer.Play(); timer.Pause(); timer.Stop(); timer.Reset();
timer.IsPlaying(); timer.IsPause();
timer.GetTime();                       // current time (affected by SetTimeSpeed)
timer.SetTimeSpeed(2f); timer.GetTimeSpeed();

// Timer: one-shot countdown
timer.SetTimer(3f);
timer.TimerCountdown();                // remaining seconds (0 when reached)
timer.IsTimerTimeout();                // true after 3f seconds
timer.GetTimerCountdownRatio();        // 1 -> 0

// Tick: repeating interval (re-arms itself when IsTickTimeout() returns true)
timer.SetTick(0.5f);
if (timer.IsTickTimeout()) { /* fires every 0.5s */ }
timer.GetTick(); timer.TickCountdown(); timer.GetTickCountdownRatio();

// Mark: stopwatch anchor
timer.SetMark();
float elapsed = timer.GetElapsedMarkTime();

// DeltaTimer only — the owner drives it:
var dTimer = new DeltaTimer(true);
void Update() { dTimer.UpdateTimer(Time.deltaTime); }

// --- RTUpdater / DTUpdater (standalone update loops) ---
var updater = new RTUpdater();
updater.onUpdate      += dt => { };
updater.onFixedUpdate += fdt => { };
updater.onLateUpdate  += dt => { };
updater.timeScale = 1f;               // clamped 0..64; RTUpdater's own scale (Unity-independent)
updater.targetFrameRate = 60f;        // loop frequency
updater.Start();                      // or StartOnThread() (RTUpdater only)
updater.Stop();
updater.IsRunning();
// DTUpdater has the same members minus StartOnThread; it additionally
// multiplies by Unity Time.timeScale (pauses when Time.timeScale == 0).

// --- IntervalSetter / IntervalTimer ---
IntervalSetter.SetInterval(1001, () => Poll(), 1000);          // keyed by int id
IntervalSetter.SetInterval("poll", () => Poll(), 1000);        // keyed by string id
IntervalTimer handle = IntervalSetter.SetInterval(() => Poll(), 1000); // unkeyed, returns handle
IntervalSetter.SetIntervalOnThread(...);                        // thread-timing variants
IntervalSetter.CheckIsRunning(1001);
IntervalSetter.TryClearInterval(1001);                          // by id / string / handle
IntervalSetter.ClearAllIntervalTimers();
// Or own an IntervalTimer directly: SetInterval / SetIntervalOnThread / StopInterval / IsRunning

// --- NtpTime ---
await NtpTime.Synchronize();                     // defaults: "time.google.com", 10s timeout
bool ok = NtpTime.IsSynchronized();
System.DateTime utc   = NtpTime.GetUtcNow();     // falls back to local clock (+warning) if not synced
System.DateTime local = NtpTime.GetNow();
System.DateTime raw   = NtpTime.GetNtpDate();    // in the synchronized zone, elapsed-corrected
string zone  = NtpTime.GetTimeZone();
double off   = NtpTime.GetUtcOffset();
```

## Usage patterns

Cooldown that survives pause menus (real time):

```csharp
var cd = new RealTimer(true);          // autoPlay; counts real time from now
cd.SetTimer(10f);
if (cd.IsTimerTimeout()) { /* skill ready */ }
```

Feature-owned game-time ticker:

```csharp
var ticker = new DeltaTimer(true);
ticker.SetTick(1f);
void Update()
{
    ticker.UpdateTimer(Time.deltaTime);
    if (ticker.IsTickTimeout()) RefreshPerSecondUI();
}
```

Non-MonoBehaviour system loop (e.g., driving OxGKit.ActionSystem or a network node):

```csharp
var rt = new RTUpdater();
rt.onUpdate += dt => runner.DriveUpdate(dt);
rt.Start();
// ... on release:
rt.Stop();
```

## Rules & pitfalls

- `RealTimer` needs no initialisation: it measures real time from its own creation. `RealTime.InitStartupTime()` only records the app's startup time in `RealTime.timeSinceStartup` — call it once at boot when you read that value.
- Use v1.1.4+: in earlier versions an `IntervalTimer` / `DTUpdater` / `RTUpdater` that was stopped and started again in the same frame stayed stopped.
- Use the shipped timers and updaters for timing; do not add a coroutine-based timer or a second update-loop manager for the same job.
- `IsTickTimeout()` re-arms the tick automatically — call it exactly once per frame per consumer, or ticks will be consumed by the wrong caller.
- `DeltaTimer`/`DTUpdater` freeze when their owner stops feeding/`Time.timeScale` is 0; `RealTimer`/`RTUpdater` do not — pick deliberately.
- Always `Stop()` updaters and `TryClearInterval` interval timers when the owning feature/scene exits; they are UniTask loops and keep running otherwise.
- `NtpTime.Synchronize()` is async — gate reads on `IsSynchronized()`; unsynced getters silently return local time with only a log warning. UDP NTP may be blocked on some platforms (e.g., WebGL); the generic `Synchronize<TResponseFormat>` overload supports HTTP time-API responses instead.
- `StartOnThread`/`SetIntervalOnThread` callbacks run off the Unity main thread — do not touch UnityEngine objects from them (marshal back, e.g. with a main-thread dispatcher).

## Verify

- Enter Play mode: pause the game (`Time.timeScale = 0`) and confirm Real* keeps counting while DT*/DeltaTimer stops.
- Confirm interval callbacks stop after `TryClearInterval`/`Stop()` (no logs after leaving the scene).
