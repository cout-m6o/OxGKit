---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity projects that use OxGKit.Utilities, including Requester web requests (audio/texture2d/sprite/bytes/text with ARC/LRU caching), LRUCache/LRUKCache/ARCCache, UISafeAreaAdapter, EasyAnim (EasyAnimation/EasyAnimator with the AnimEnd event), TextureAnimation image-sequence playback, DontDestroy, UnityMainThread UMT dispatcher, and the RectTransformAdjuster / MissingScriptsFinder / SymlinkUtility editor tools. Also covers its Editor entry points and Unity CLI commands (unity command oxgkit.utilities).
---

# OxGKit.Utilities Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.Utilities user. The module (UPM package `com.michaelo.oxgkit.utilities`, UniTask-based) is a grab-bag of essential runtime helpers — each in its own `OxGKit.Utilities.<Sub>` namespace — plus editor tools. Pick the sub-tool that fits; they are independent.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.utilities@*` or `Assets/OxGKit/Utilities/Scripts` when embedded). Do not invent APIs; signatures below are the verified core surface.
4. On WebGL, call out download/audio limitations and avoid thread-based paths.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its samples and its Editor tooling. Follow that usage exactly. Never design your own variant of it: no second HTTP/texture/audio requester, cache class, main-thread dispatcher, safe-area script or animation-callback helper that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling or shipped assets can provide (see "Editor entry points") is obtained through that entry point — the same one a developer uses — and you continue from what it produced. Do not hand-build an equivalent from memory and do not re-implement an Editor tool.
- **Stay inside the shipped shape.** Configure what the module exposes (serialized fields, public API); do not patch the package, subclass around it or add a parallel mechanism next to it.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/Utilities/Scripts`
- Auto dependencies: `com.cysharp.unitask`, `com.domybest.lwmybox`, `com.michaelo.oxgkit.loggingsystem`.
- Samples (Package Manager > OxGKit.Utilities > Samples): `AI Agent Skills` (this skill).
- Editor tooling (assembly `OxGKit.Utilities.Editor`, auto-referenced) and Unity CLI commands (`unity command oxgkit.utilities ...`, declared in `OxGKit.Utilities.Editor.UnityCli`, compiled only when `com.unity.pipeline` 1.x is installed): see "Editor entry points".

## Editor entry points (trigger, don't hand-write)

Editor tools:

| Tool | Unity CLI command (v1.4.9+) | Menu a developer clicks | Editor method |
| --- | --- | --- | --- |
| **MissingScriptsFinder** — reports GameObjects that carry a missing script (it only reports; it removes nothing) | `unity command oxgkit.utilities find_missing_scripts [--scope LoadedScenes\|BuildScenes\|BuildScenesIncludeDisabled\|AllScenes\|AllPrefabs]` | top menu `MissingScriptsFinder >` `Search in loaded scenes` / `Search in build settings scenes` / `Search in build settings scenes (include disable)` / `Search in project all scenes` / `Search in all prefabs` | `OxGKit.Utilities.MissingFinder.Editor.MissingScriptsFinder.` `FindMissingScriptsInLoadedScenes()` / `FindMissingScriptsInBuildSettingsScenes()` / `FindMissingScriptsInBuildSettingsScenesIncludeDisable()` / `FindMissingScriptsInProjectAllScenes()` / `FindMissingScriptsInAllPrefabs()` |
| **RectTransformAdjuster** — moves the anchors of the selected RectTransforms to their current corners and zeroes the offsets, keeping the rect (undoable) | `unity command oxgkit.utilities adjust_anchors --target <handle>` | `GameObject > Adjust RectTransform Anchors (Shift+R)`, shortcut Shift+R | `OxGKit.Utilities.RectTrans.Editor.RectTransformAdjuster.AdjustAnchors()` (works on the current selection) |
| **SymlinkUtility** — links an external folder into the project | — (interactive only) | `Assets > Create > Folder (Junction)` (Windows only), `Folder (Absolute Symlink)`, `Folder (Relative Symlink)` | — |

Facts (verified on Unity 6000.3):

- MissingScriptsFinder writes its report to the Console as `Missing script: <object path> --> <scene or prefab path>` (loaded-scene objects have no path part); the CLI command returns the same hits as records `{ objectPath, assetPath }` — no record at all (empty output, `null` with `--result-only`) with exit code 0 means nothing is missing. Scenes that are not loaded are opened additively and closed again. From v1.4.9 `Search in all prefabs` skips prefab files that are not imported assets (e.g. inside `~` folders) instead of stopping with an exception.
- The menu path to execute for RectTransformAdjuster is `GameObject/Adjust RectTransform Anchors (Shift+R)` (without the shortcut suffix).
- SymlinkUtility opens an OS folder picker and, for symbolic links on Windows, an elevation prompt. It cannot be automated: ask the developer to run it, and do not create links with shell commands of your own instead.

Components:

| You need | What a developer does | Agent equivalent |
| --- | --- | --- |
| `UISafeAreaAdapter` | `Add Component > OxGKit > Utilities > Adapter > UISafeAreaAdapter` | `unity command component add --target <GameObject> --type OxGKit.Utilities.Adapter.UISafeAreaAdapter` (Unity CLI) · menu item `Component/OxGKit/Utilities/Adapter/UISafeAreaAdapter` on the selected GameObject · `Undo.AddComponent<OxGKit.Utilities.Adapter.UISafeAreaAdapter>(gameObject)` (editor code) |
| `DontDestroy` | `Add Component > OxGKit > Utilities > DontDestroy > DontDestroy` | `unity command component add --target <GameObject> --type OxGKit.Utilities.DontDestroy.DontDestroy` (Unity CLI) · menu item `Component/OxGKit/Utilities/DontDestroy/DontDestroy` on the selected GameObject · `Undo.AddComponent<OxGKit.Utilities.DontDestroy.DontDestroy>(gameObject)` (editor code) |
| `EasyAnimation` / `EasyAnimator` | `Add Component > OxGKit > Utilities > EasyAnim > EasyAnimation` | `unity command component add --target <GameObject> --type OxGKit.Utilities.EasyAnim.EasyAnimation` (Unity CLI) · menu item `Component/OxGKit/Utilities/EasyAnim/EasyAnimation` (or `.../EasyAnimator`) on the selected GameObject · `Undo.AddComponent<OxGKit.Utilities.EasyAnim.EasyAnimation>(gameObject)` (editor code) |
| `TextureAnimation` | `Add Component > OxGKit > Utilities > TextureAnim > TextureAnimation` | `unity command component add --target <GameObject> --type OxGKit.Utilities.TextureAnim.TextureAnimation` (Unity CLI) · menu item `Component/OxGKit/Utilities/TextureAnim/TextureAnimation` on the selected GameObject · `Undo.AddComponent<OxGKit.Utilities.TextureAnim.TextureAnimation>(gameObject)` (editor code) |

The `unity command ...` forms need Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor; `component add` is one of that package's own commands. `unity command oxgkit.utilities` lists this module's own commands; `--target` takes a pipeline object handle (hierarchy path such as `/Canvas/Panel`, `instanceId` or `globalId`).

## Module map

| Namespace | Types | Use for |
| --- | --- | --- |
| `OxGKit.Utilities.Requester` | `Requester` | UnityWebRequest downloads with in-memory caching |
| `OxGKit.Utilities.Cacher` | `LRUCache<K,V>`, `LRUKCache<K,V>`, `ARCCache<K,V>`, `IRemoveCacheHandler`, `UnityObjectRemoveCacheHandler` | capacity-bounded caches |
| `OxGKit.Utilities.Adapter` | `UISafeAreaAdapter` | notch-safe UI panels |
| `OxGKit.Utilities.EasyAnim` | `EasyAnim` (base), `EasyAnimation`, `EasyAnimator` | play-then-callback animation wrapper |
| `OxGKit.Utilities.TextureAnim` | `TextureAnimation` | CPU image-sequence (flipbook) playback |
| `OxGKit.Utilities.DontDestroy` | `DontDestroy` | mark a root object persistent + runtime rename |
| `OxGKit.Utilities.UnityMainThread` | `UMT` | run jobs/coroutines on the Unity main thread |

## Requester (verified)

Static async download helpers with optional per-type caching (ARC or LRU; keyed by URL):

```csharp
using OxGKit.Utilities.Requester;
using UnityEngine;

// Optional: initialize ONE cache flavor per resource type at boot (ARC or LRU, not both)
Requester.InitARCCacheCapacityForAudio(20);
Requester.InitARCCacheCapacityForTexture2d(60);
Requester.InitARCCacheCapacityForText(100);
// or Requester.InitLRUCacheCapacityFor...(...)

AudioClip clip  = await Requester.RequestAudio(url, AudioType.MPEG, successAction, errorAction, cts, cached: true, timeoutSeconds: null);
Texture2D tex   = await Requester.RequestTexture2D(url, successAction, errorAction, cts, cached: true);
Sprite sprite   = await Requester.RequestSprite(url, successAction, errorAction, position, pivot, pixelPerUnit: 100, ...);
byte[] bytes    = await Requester.RequestBytes(url, successAction, errorAction, cts);   // never cached
string text     = await Requester.RequestText(url, successAction, errorAction, cts, cached: true);

// Cache eviction
Requester.RemoveFromARCCacheForTexture2d(url);   // per-flavor removes
Requester.AutoRemoveFromCaches(url);             // try every cache
Requester.ClearARCCacheCapacityForText();        // clear + drop a cache
```

Notes: errors arrive via `errorAction(ErrorInfo)` and the task returns null; pass a `CancellationTokenSource` for lifecycle-bound requests; default request timeout guard is 180s. Instance variants (`SelfRequest*` on `new Requester()`) exist for isolated cache scopes.

## Cacher (verified)

```csharp
using OxGKit.Utilities.Cacher;

var cache = new LRUCache<string, Texture2D>(capacity: 60);            // or LRUKCache(capacity, k), ARCCache(capacity)
var cache2 = new LRUCache<string, Texture2D>(60, new MyRemoveCacheHandler());                           // your own IRemoveCacheHandler<TKey, TValue>
cache.Add(key, value);
Texture2D v = cache.Get(key);      // null/default when missing
cache.Contains(key); cache.Remove(key); cache.Clear(); cache.Count; cache.GetKeys();
```

What each cache evicts when it is full (source-verified; do not assume the textbook algorithm behind the name):

- `LRUCache` — the least recently used entry (`Get` and re-`Add` count as use).
- `ARCCache` — two lists: T1 holds new entries, T2 holds entries that were read or added again. It evicts the oldest entry of T1 while T1 has any, and only then the least recently used entry of T2. It is not the adaptive ARC algorithm.
- `LRUKCache(capacity, k)` — every entry has a counter (1 when added, +1 per `Get`/re-`Add`, capped at `k`). It evicts the entry with the smallest counter (the least recently used one among equals), then lowers the counter of every remaining entry by 1.

Every cache created without a handler uses `UnityObjectRemoveCacheHandler`, which calls `UnityEngine.Object.Destroy` on a value that is evicted, removed or cleared — a cached Unity object belongs to the cache. Pass your own `IRemoveCacheHandler<TKey, TValue>` when values need a different release.

## EasyAnim (verified)

```csharp
using OxGKit.Utilities.EasyAnim;

EasyAnim ea = go.GetComponent<EasyAnim>();  // EasyAnimation (legacy Animation) or EasyAnimator (Animator)
ea.Play("Open", () => this.OnOpened());     // EasyAnimation: clip name; EasyAnimator: TRIGGER parameter name
bool has = ea.HasAnim("Open");
```

**Required setup**: the clip must contain an Animation Event named `AnimEnd` — that event invokes the completion callback. Missing param/clip names call the callback immediately (fail-open).

## Other runtime helpers (verified)

```csharp
// UISafeAreaAdapter — put on a panel; fits `panel` RectTransform to Screen.safeArea.
// refreshAlways = true re-applies every frame (orientation changes); v1.4.8+ logs only when
// the safe area actually changed (no per-frame string allocations); RefreshViewSize() manual.

// TextureAnimation — flipbook on a Renderer/RawImage from Texture2D frames:
texAnim.SetFrameRate(12);
texAnim.SetAnimationEnd(() => Done());     // fires when a non-looping run completes
texAnim.IsAnimationComplete(); texAnim.ResetAnim(); texAnim.SetIgnoreScale(true);

// DontDestroy — component: marks the object DontDestroyOnLoad in Awake and renames it; SetRuntimeName(name).

// UMT — main-thread dispatcher (auto-creates a persistent [UMT] object):
using OxGKit.Utilities.UnityMainThread;
UMT.worker.AddJob(() => { /* runs on next Update, safe from any thread */ });
UMT.worker.RunCoroutine(routine);          // + CancelCoroutine / CancelAllCoroutines / Clear
```

## Rules & pitfalls

- Initialize Requester caches once at boot; init calls with a different capacity re-create the cache (dropping entries). Uncached or evicted Unity objects you no longer need must be destroyed by the caller — cached ones belong to the cache.
- `RequestAudio` needs the right `AudioType` for the file; WebGL streaming audio has platform limits — verify formats there.
- Cancel in-flight requests (`cts.Cancel()`) when the owner dies, or callbacks fire into released objects.
- `EasyAnim` without the `AnimEnd` event never completes-by-event — the callback simply won't fire; this is the first thing to check when "animation callback doesn't run".
- `UMT.worker` lazily creates a DontDestroy object — acceptable in gameplay, but avoid first-touching it from teardown paths.
- Editor tools live in the Editor assembly — never reference them from runtime code.
- Use the shipped helper for the job it covers (`Requester`, the caches, `UMT`, `UISafeAreaAdapter`, `EasyAnim`, ...) instead of writing a second one beside it.

## Verify

- Compile, then exercise the specific helper: a cached second `RequestTexture2D` returns instantly (no network log), safe-area panel fits on a notched device simulator, `AnimEnd`-tagged clips fire callbacks, and UMT jobs queued from a worker thread execute on the main thread.
