---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity projects that use OxGKit.PoolSystem, including the NodePool GameObject pool component, across-frames (load-balanced) initialization, Get/Put recycling, autoPut growth, maxSize limiting, and pool lifecycle/cleanup.
---

# OxGKit.PoolSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.PoolSystem user. The module (UPM package `com.michaelo.oxgkit.poolsystem`, UniTask-based) provides **`NodePool`** — a simple component-based GameObject pool with smoothed (across-frames) instantiation for load balancing, optional auto-growth, and a max-size cap.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.poolsystem@*` or `Assets/OxGKit/PoolSystem/Scripts` when embedded). Do not invent APIs.
4. Identify the pool owner (which scene object/system hosts the `NodePool`) and its destroy path before wiring pooled objects.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its samples and its Editor tooling. Follow that usage exactly. Never design your own variant of it: no home-made pool class, generic object-pool wrapper, pool manager singleton or instantiate/destroy helper that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling or shipped assets can provide (see "Editor entry points") is obtained through that entry point — the same one a developer uses — and you continue from what it produced. Do not hand-build an equivalent from memory and do not re-implement an Editor tool.
- **Stay inside the shipped shape.** Configure what the module exposes (serialized fields, public API); do not patch the package, subclass around it or add a parallel mechanism next to it.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/cout-m6o/OxGKit.git?path=Assets/OxGKit/PoolSystem/Scripts`
- Auto dependencies: `com.cysharp.unitask`, `com.domybest.lwmybox`, `com.michaelo.oxgkit.loggingsystem`.
- Samples (Package Manager > OxGKit.PoolSystem > Samples): `NodePool Demo`, `AI Agent Skills` (this skill).

## Editor entry points (trigger, don't hand-write)

This module ships no generator: a pool is the `NodePool` component, added the way a developer adds it and configured through its serialized fields.

| You need | What a developer does | Agent equivalent |
| --- | --- | --- |
| A pool on a stable owner GameObject | `Add Component > OxGKit > PoolSystem > NodePool` | `unity command component add --target <GameObject> --type OxGKit.PoolSystem.NodePool` (Unity CLI) · menu item `Component/OxGKit/PoolSystem/NodePool` on the selected GameObject · `Undo.AddComponent<OxGKit.PoolSystem.NodePool>(gameObject)` (editor code) |
| A working reference scene | Package Manager > OxGKit.PoolSystem > Samples > `NodePool Demo` > Import | read it before wiring a pool; do not copy its scripts into the project as a framework layer |

The `unity command ...` forms need Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor; `component add` is one of that package's own commands. After adding the component set `go` (the source prefab) and the other fields listed below — with `unity command component set_properties`, the Inspector or `SerializedObject` in editor code.

## Inspector fields (verified)

| Field | Meaning |
| --- | --- |
| `go` | source GameObject to instantiate (required) |
| `initializeOnStart` | auto-`Initialize()` on `Start` (default true) |
| `initSize` | initial pooled count (default 5) |
| `initLoadAcrossFrames` | spread initial instantiation across frames (default true) |
| `initDelayFrameAfterSpawnCount` | spawn N objects, then wait (default 1) |
| `initDelayFrame` | frames to wait between batches (default 1) |
| `autoPut` | when `Get()` finds the pool empty, auto-create more (default false) |
| `autoPutSize` | how many to create per auto-grow (default 1) |
| `autoPutLoadAcrossFrames` (+ batch/delay fields) | across-frames settings for auto-grow |
| `maxSize` | `0` / `-1` = unlimited; `> 0` caps total pooled count (init + autoPut; excess `Put` destroys) |

## Core API (verified)

```csharp
using OxGKit.PoolSystem;
using UnityEngine;

NodePool pool;

pool.Initialize();          // Clear() then (re)create initSize objects (async when across-frames)
pool.IsLoadFinished();      // init/auto-put instantiation finished?
pool.Count();               // current pooled count

GameObject a = pool.Get();                                 // null if empty and no autoPut
GameObject b = pool.Get(parent);
GameObject c = pool.Get(parent, localPosition);
GameObject d = pool.Get(parent, localPosition, rotation);  // rotation is world-space

pool.Put(a);                // deactivate + re-parent under the pool node
pool.Clear();               // cancel pending creation, destroy pooled objects (also runs OnDestroy)
```

Behavior notes:

- `Get()` activates the object and detaches it (parentless overload sets parent null); `Put()` deactivates and re-parents it under the pool's transform.
- With `autoPut` on and the pool empty, `Get()` synchronously creates at least the first object of the growth batch (the rest may fill across later frames), so it still returns an object immediately.
- `Put()` beyond `maxSize` destroys the object instead of pooling it (a log line notes it).

## Usage pattern

```csharp
public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private NodePool _pool;   // NodePool on a stable owner object; go = bullet prefab

    private void Fire(Transform muzzle)
    {
        GameObject bullet = this._pool.Get(null, Vector3.zero);
        if (bullet == null) return;                    // pool empty, no autoPut
        bullet.transform.position = muzzle.position;
        bullet.GetComponent<Bullet>().Launch(b => this._pool.Put(b.gameObject)); // recycle on death
    }
}
```

Reset pooled-object state on reuse — either in an `OnEnable` on the pooled object (activation happens on `Get`) or explicitly right after `Get()`.

## Rules & pitfalls

- One `NodePool` pools exactly one source prefab; use one pool per prefab type.
- Pooled objects must be **stateless on reuse**: reset velocity/trail/coroutines on `Get`/`OnEnable`; never assume freshly-instantiated state.
- Do not `Destroy` a pooled object yourself — always `Put()` it back; destroyed objects silently shrink the pool.
- Gate heavy startup on `IsLoadFinished()` when `initLoadAcrossFrames` is on and you need the full pool warm (e.g., before a wave spawns).
- The pool destroys its contents in `OnDestroy` — pooled objects must not be needed after the owner dies; conversely, `Put()` after the owner died would re-parent onto a dead transform (guard feature shutdown order).
- `maxSize` counts pooled (idle) objects; in-flight objects returning to a full pool get destroyed — size the cap to peak concurrency, not average.
- From v1.0.3 the auto-grow batch honours its own "after spawn count" setting (`autoDelayFrameAfterSpawnCount`); earlier versions used the init setting for it.
- One `NodePool` component per prefab is the whole pooling mechanism — do not wrap it in a generic pool manager or keep a second list of pooled objects beside it.

## Verify

- Enter Play mode: watch the pool node fill across frames, `Get`/`Put` round-trips keep `Count()` stable, and no `Instantiate` spikes appear in the Profiler during steady state.
- With `maxSize` set, confirm excess `Put` logs and destroys rather than leaking.
