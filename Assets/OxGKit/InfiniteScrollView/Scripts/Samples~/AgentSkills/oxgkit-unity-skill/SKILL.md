---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity UGUI projects that use OxGKit.InfiniteScrollView, including Vertical/Horizontal/Grid infinite scroll views, InfiniteCell subclasses and their OnCreate/OnRefresh/OnRecycle/OnSnap/OnClick lifecycle, InfiniteCellData with dynamic cell sizes, pool initialization, Add/Insert/Remove/Refresh data operations, snapping, and chat-list or tab-page patterns. Also covers its Editor entry points and Unity CLI commands (unity command oxgkit.infinite_scroll_view).
---

# OxGKit.InfiniteScrollView Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.InfiniteScrollView user. The module (UPM package `com.michaelo.oxgkit.infinitescrollview`, UniTask-based, forked/improved from howtungtung's InfiniteScrollView) renders large lists with few GameObjects on native UGUI `ScrollRect`: cells are pooled, data lives in `InfiniteCellData`, and concrete layouts are `VerticalInfiniteScrollView`, `HorizontalInfiniteScrollView`, `VerticalGridInfiniteScrollView`, `HorizontalGridInfiniteScrollView`.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.infinitescrollview@*` or `Assets/OxGKit/InfiniteScrollView/Scripts` when embedded). Do not invent APIs.
4. Before coding, identify layout variant, whether cell sizes are uniform or dynamic, and the data order (`Normal`/`Reverse`) the UI needs.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its templates, its samples and its Editor menus. Follow that usage exactly. Never design your own variant of it: no home-made cell base class, cell pool, virtualized list, layout calculation or data wrapper that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling can create (see "Editor entry points") is created through that entry point — the same one a developer clicks — and you continue from what it produced. Do not hand-write an equivalent from memory.
- **Stay inside the generated shape.** After generating, fill in only what the template leaves open; keep its classes, member names and signatures.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/cout-m6o/OxGKit.git?path=Assets/OxGKit/InfiniteScrollView/Scripts`
- Auto dependencies: `com.cysharp.unitask`, `com.michaelo.oxgkit.loggingsystem`.
- Samples (Package Manager > OxGKit.InfiniteScrollView > Samples): `Infinite Vertical Demo`, `Infinite Horizontal Demo`, `Infinite Grid Demo`, `Infinite TabPage Demo`, `Infinite ChatRoom Demo` (package folders `Samples~/01_Vertical` … `05_ChatRoom`), `AI Agent Skills` (this skill).
- Editor tooling (assembly `OxGKit.InfiniteScrollView.Editor`, auto-referenced) and Unity CLI commands (`unity command oxgkit.infinite_scroll_view ...`, declared in `OxGKit.InfiniteScrollView.Editor.UnityCli`, compiled only when `com.unity.pipeline` 1.x is installed): see "Editor entry points".

## Editor entry points (generate, don't hand-write)

Script templates (the Editor methods are static members of `OxGKit.InfiniteScrollView.Editor.InfiniteScrollViewCreateScriptEditor`; the templates live in `Editor/TplScripts/` of the package; the `...AtPath` methods and the CLI commands exist from v1.7.2):

| You need | Unity CLI command | Menu a developer clicks | Editor methods (menu · no name field) | Template → default file |
| --- | --- | --- | --- | --- |
| A cell script (the `<Name>Data` class + the `<Name>` cell view) | `unity command oxgkit.infinite_scroll_view create_cell --name <Name> [--path <folder>]` | `Assets > Create > OxGKit > Infinite ScrollView > Template Infinite Cell.cs (Script)` | `CreateScriptTplInfiniteCell()` · `CreateScriptTplInfiniteCellAtPath(assetPath)` | `TplInfiniteCell.cs.txt` → `NewTplInfiniteCell.cs` |

Cell prefab (static members of `OxGKit.InfiniteScrollView.Editor.InfiniteScrollViewCreatePrefabEditor`):

| You need | Unity CLI command | Menu a developer clicks | Editor methods (menu · no name field) | Result |
| --- | --- | --- | --- | --- |
| The empty cell prefab to build the cell on | `unity command oxgkit.infinite_scroll_view create_cell_prefab --name <Name> [--path <folder>]` | `Assets > Create > OxGKit > Infinite ScrollView > Template Infinite Cell (RectTransform Prefab)` | `CreateTplRectTransformCP()` · `CreateTplRectTransformCPAtPath(assetPath)` | `<Name>.prefab` (menu default `NewTplInfiniteCell.prefab`): one GameObject with `RectTransform` + `CanvasRenderer`, anchors (0.5, 0.5), size 128 × 128, scale 1 |

All routes produce the same text. The menu hands the template to Unity's own script generator, `ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, "<default file>")`, which (verified on Unity 6000.3)

- creates the script in the folder selected in the Project window and opens the inline name field — the file is written once the name is confirmed. Without a Project window (`-batchmode`) it is written immediately as `Assets/<default file>` (a number is appended when taken), whatever is selected;
- replaces every `#SCRIPTNAME#` with the file name (extension dropped, spaces removed);
- turns the `#ROOTNAMESPACEBEGIN#` / `#ROOTNAMESPACEEND#` lines into `namespace <RootNamespace>`, `{` and `}` and indents every non-blank line in between by the whitespace in front of `#ROOTNAMESPACEBEGIN#` (four spaces); when the destination has no root namespace it simply removes those two lines. The root namespace is the `rootNamespace` of the assembly definition that owns the destination folder, otherwise *Project Settings > Editor > Root Namespace*.

The Unity CLI commands and the `...AtPath` methods apply exactly these substitutions (their output is byte-identical to the generator's) and write straight to the path you give: no name field, the folder must exist, and an existing file is never overwritten.

How to trigger it — take the first route you can really complete:

1. **Unity CLI.** Needs Unity 6+, the Unity Pipeline package (`com.unity.pipeline` 1.x — `unity pipeline install`) and the project open in a running Editor. `unity command oxgkit.infinite_scroll_view` lists this module's commands with their arguments; they are declared in `OxGKit.InfiniteScrollView.Editor.UnityCli`, which compiles only when that package is installed.

   ```
   unity command oxgkit.infinite_scroll_view create_cell --name ShopItemCell --path Assets/Game/UI/Shop
   unity status --until-ready --project-path .   # the Editor recompiles and reloads on its own - wait before the next command
   unity command script recompile_status         # "failed": false = the new script compiled
   ```

   `--name` is the class/file name (a valid C# identifier); `--path` is an existing folder, relative to the pipeline's authoring root (the `Assets/` prefix is optional; default: the root itself). The result carries `assetPath`. The Editor then imports the script, recompiles and reloads its scripts on its own (`unity command script recompile` starts a recompile explicitly); the new type exists only after that. While the Editor reloads, every `unity command` ends with exit code 12 (`EDITOR_UNREACHABLE`, "No Pipeline instance found") without having run — that is expected, not a problem of the project: wait with `unity status --until-ready` and run the command again. So wait after every script you create, before the next command.
2. **Editor code** (a bridge that runs C# in the Editor; every supported Unity version):

   ```csharp
   OxGKit.InfiniteScrollView.Editor.InfiniteScrollViewCreateScriptEditor.CreateScriptTplInfiniteCellAtPath("Assets/Game/UI/Shop/ShopItemCell.cs");
   ```
3. **Menu items only** (a bridge that can only execute menu items, or `unity command editor invoke_menu --path "<menu path>"`): select the destination folder (`Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Game/UI/Shop")`), execute `Assets/Create/OxGKit/Infinite ScrollView/Template Infinite Cell.cs (Script)`, then you or the developer must confirm the name field in the Project window. Continue only after the file exists.
4. **Files only**: produce exactly what the generator produces. Read the template from the installed package — `TplScripts/` next to `InfiniteScrollViewCreateScriptEditor.cs` (`Library/PackageCache/com.michaelo.oxgkit.infinitescrollview@*/Editor/TplScripts/`, or under `Assets/` when the package is embedded) — apply the two substitutions above and save the result as `<Name>.cs` in the destination folder. Never retype a template from memory or from this document.
5. **None of these**: ask the developer to run the menu and tell you which file it created.

The prefab follows the same routes 1–3 (`create_cell_prefab`, `CreateTplRectTransformCPAtPath("Assets/Game/UI/Shop/ShopItemCell.prefab")`, or the menu). Its menu uses the same naming flow (`ProjectWindowUtil.StartNameEditingIfProjectWindowExists`): select the destination **folder** first; with nothing selected Unity picks the location from v1.7.2 on (earlier versions threw an exception). A prefab cannot be produced by writing files — without Editor access ask the developer to run the menu.
## Building a list

1. Create a UGUI Scroll View with Unity's own `GameObject > UI` menu; add one of the scroll-view components to the `ScrollRect` object (`VerticalInfiniteScrollView`, `HorizontalInfiniteScrollView`, `VerticalGridInfiniteScrollView`, `HorizontalGridInfiniteScrollView` — they require a `ScrollRect` on the same object).
2. Generate the cell script and the cell prefab through the entry points above, add the generated cell component to the prefab root, build the cell visuals under it, and assign the prefab to `cellPrefab`.
3. Configure: `cellPoolSize`, `initializePoolOnAwake` (or call `InitializePool()` yourself), `dataOrder` (`Normal`/`Reverse`), `snapAlign` (`Start`/`Center`/`End`), `extendVisibleRange`, `padding`; grid variants add `spacing` (Vector2) plus `columnCount` (vertical grid; renamed from `columeCount` in v1.7.1, old serialized data auto-migrates) / `rowCount` (horizontal grid).
4. Fill data with `InfiniteCellData` and `Refresh()`.

## Core API (verified)

```csharp
using OxGKit.InfiniteScrollView;
using Cysharp.Threading.Tasks;
using UnityEngine;

InfiniteScrollView sv;

// Pool init — REQUIRED before any data op (or initializePoolOnAwake)
await sv.InitializePool(args: null);      // args forwarded to every cell's OnCreate
bool ready = sv.isInitialized;

// Data ops (refresh flags control immediate visual update)
sv.Add(new InfiniteCellData(new Vector2(0, 100), payload), refresh: false);
sv.Insert(index, cellData, refresh: true);
sv.Remove(index, refresh: true);
sv.Clear();
int count = sv.DataCount();

// Refreshing
sv.Refresh(refreshOnNextScroll: false, recycleActiveCells: false);
sv.RefreshVisibleCells();

// Scrolling / state
sv.ScrollToTop(); sv.ScrollToBottom(); sv.ScrollToLeft(); sv.ScrollToRight();
sv.IsAtTop(); sv.IsAtBottom(); sv.IsAtLeft(); sv.IsAtRight();
float v = sv.VerticalNormalizedPosition(); float h = sv.HorizontalNormalizedPosition();

// Snapping (duration-based tween to a cell)
sv.Snap(index, duration);
sv.SnapFirst(duration); sv.SnapMiddle(duration); sv.SnapLast(duration);

// Events
sv.onRefreshed;                      // Action
sv.onValueChanged;                   // Action<Vector2> (scroll pos)
sv.onCellSelected;                   // Action<InfiniteCell> (from cell OnClick)
sv.onRectTransformDimensionsChanged; // Action
```

Cell + data — the shape the script template generates for a file named `ItemCell.cs` (lines marked "yours" are what you add; everything else is generated and stays):

```csharp
// Payload class, stored in InfiniteCellData.data
public class ItemCellData
{
    public ItemCellData() { }

    // Set callback of parsing method — the data owner uses it to push values into the cell view
    public Action<ItemCell> parsing;

    public string title;                                  // yours: the fields this cell renders
}

// Cell view, the MonoBehaviour on the cell prefab root
public class ItemCell : InfiniteCell
{
    public Text titleText;                                // yours: references to the cell's UI

    public override async UniTask OnCreate(object args = null) { /* one-time setup; args come from InitializePool */ }

    public override void OnRefresh()
    {
        ItemCellData cellData = this.cellData.data as ItemCellData;   // generated
        cellData.parsing?.Invoke(this);                               // generated
        this.titleText.text = cellData.title;                         // yours (or do it inside the parsing callback)
    }

    public override void OnRecycle() { /* cleared from view */ }

    public override void OnSnap() { /* became the snapped cell */ }
}

var data = new InfiniteCellData(cellSize: new Vector2(0, 80), data: new ItemCellData { title = "Potion" });
// cellSize axis that matters: y for vertical lists, x for horizontal; grids use both.
```

`InfiniteCell.OnClick()` is the cell's button event: wire a `Button` on the cell to it (override it only to add behaviour, and keep calling `base.OnClick()` — that is what raises `onCellSelected`).

## Patterns from the official samples

- **Vertical/Horizontal (01/02)**: uniform cells, `Normal` vs `Reverse` order, add/remove with GUI buttons.
- **Grid (03)**: `VerticalGridInfiniteScrollView` fills `columnCount` columns per row; `HorizontalGridInfiniteScrollView` fills `rowCount` rows per column; set `spacing`.
- **TabPage (04)**: horizontal page cells + `Snap(index, duration)` for tab switching; `snapAlign` centers pages.
- **ChatRoom (05)**: dynamic cell heights — measure text (`Text.preferredHeight`) into `InfiniteCellData.cellSize`, then `Add` + `Refresh()` + `SnapLast(0.1f)` for auto-scroll-to-newest.

```csharp
// ChatRoom-style dynamic height append
this.heightInstrument.text = msg;
var cellData = new InfiniteCellData(new Vector2(0, this.heightInstrument.preferredHeight + basePad), new ChatCellData(user, msg));
this.chatScrollView.Add(cellData);
this.chatScrollView.Refresh();
this.chatScrollView.SnapLast(0.1f);
```

## Rules & pitfalls

- `InitializePool()` must complete before `Add`/`Refresh`/`Snap`; when `initializePoolOnAwake` is off, await it in your own init.
- Cells are **recycled**: `OnRefresh` must be idempotent and fully driven by `cellData` — never store per-item state in the cell (store it in the data object).
- After batch `Add`s, call `Refresh()` once (cheaper than per-add refresh flags).
- `Remove`/`Insert` shift indices — re-read `cellData.index` inside cells instead of caching indices.
- Dynamic sizes: compute `cellSize` **before** adding; changing a size later requires updating the data and a full `Refresh`.
- `extendVisibleRange` pre-instantiates off-screen rows for smoothness at the cost of more live cells; tune with the pool size (`cellPoolSize` should cover visible + extended range).
- `onCellSelected` only fires if the cell's click path calls `base.OnClick()` (button on the cell wired to `OnClick`).
- Don't fight UGUI layout groups on the content node — the scroll view positions cells itself; content needs no LayoutGroup/ContentSizeFitter.
- Every list goes through one of the four shipped scroll-view components and `InfiniteCell` subclasses generated from the template. Do not subclass the scroll views to change pooling/layout, and do not replace them with a hand-rolled recycler "for simple cases".

## Verify

- A freshly generated cell script compiles as generated; if it does not, regenerate it instead of patching the skeleton.

- Play: scroll through thousands of items with a stable cell count in the Hierarchy; add/remove/snap behave; no layout warnings.
- For dynamic lists, resize the window (or trigger `onRectTransformDimensionsChanged`) and confirm re-layout.
