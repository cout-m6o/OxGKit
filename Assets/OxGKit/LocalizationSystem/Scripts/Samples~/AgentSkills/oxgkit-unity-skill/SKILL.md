---
name: oxgkit-unity-skill
description: Use when developing or reviewing Unity projects that use OxGKit.LocalizationSystem, including the Localization static class, LangType languages, the required onAddSupportedLanguages / onParsingLanguageData / onChangeLanguage callbacks, ChangeLanguage flow, GetStringByCode text lookup, system-language detection, and persisting the selected language.
---

# OxGKit.LocalizationSystem Unity Skill

## Purpose

Make the agent behave like an experienced OxGKit.LocalizationSystem user. The module (UPM package `com.michaelo.oxgkit.localizationsystem`, dependency-free) is a callback-driven localization core: the project supplies the language table (from JSON, spreadsheet exports, a server, a game DB, ...) through callbacks, and `Localization` handles supported-language bookkeeping, language switching, and code→text lookup.

## First response rules

1. Reply in the user's language; keep code and identifiers in English.
2. Start with the conclusion, then implementation details.
3. Verify APIs against the installed package (`Library/PackageCache/com.michaelo.oxgkit.localizationsystem@*` or `Assets/OxGKit/LocalizationSystem/Scripts` when embedded) and inspect how the target project stores its language table before writing code.
4. Never hardcode display strings that need localization; route them through `Localization.GetStringByCode`.
5. Obey the "Working rules" below: work the way the module ships and go through its Editor entry points.

## Working rules (non-negotiable)

- **The package is the only authority.** How this module is used is defined by the installed package — its source, its samples and its Editor tooling. Follow that usage exactly. Never design your own variant of it: no second string-table format, key naming scheme, language enum, static text facade or loader that merely "fits" the module.
- **Trigger, don't imitate.** Whatever the module's Editor tooling or shipped assets can provide (see "Editor entry points") is obtained through that entry point — the same one a developer uses — and you continue from what it produced. Do not hand-build an equivalent from memory and do not re-implement an Editor tool.
- **Stay inside the shipped shape.** Configure what the module exposes (serialized fields, public API); do not patch the package, subclass around it or add a parallel mechanism next to it.
- **No gap-filling.** If the module does not cover a need, say so and ask the developer. Do not close the gap with a rule of your own.

## Install

- UPM git URL: `https://github.com/michael811125/OxGKit.git?path=Assets/OxGKit/LocalizationSystem/Scripts`
- No dependencies.
- Samples (Package Manager > OxGKit.LocalizationSystem > Samples): `Localization Demo`, `AI Agent Skills` (this skill).

## Editor entry points (trigger, don't hand-write)

This module ships no Editor tooling and no generator — its whole surface is the runtime API below, and how the language table is stored and loaded belongs to the target project. The entry point for an agent is therefore the shipped sample:

| You need | What a developer does | Agent equivalent |
| --- | --- | --- |
| The reference setup (table loading, language switch, text refresh) | Package Manager > OxGKit.LocalizationSystem > Samples > `Localization Demo` > Import | import the sample (see below) and read it before writing any localization code; follow the pattern the target project already uses |

Importing the `Localization Demo` sample (same as the Import button of Package Manager > OxGKit.LocalizationSystem > Samples; Unity copies it to `Assets/Samples/OxGKit.LocalizationSystem/<version>/Localization Demo`):

```csharp
// Editor code
var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.michaelo.oxgkit.localizationsystem");
foreach (var sample in UnityEditor.PackageManager.UI.Sample.FindByPackage(package.name, package.version))
    if (sample.displayName == "Localization Demo" && !sample.isImported)
        sample.Import(UnityEditor.PackageManager.UI.Sample.ImportOptions.None);
```

Without Editor access, copy the package's `Samples~/LocalizationDemo` folder (with its `.meta` files) to that destination instead. It is reference material, not a framework layer to copy into the project.

## Core API (verified)

```csharp
using OxGKit.LocalizationSystem;

// Required callbacks (assign BEFORE ChangeLanguage)
Localization.onAddSupportedLanguages; // Action<HashSet<LangType>>: declare supported languages
Localization.onParsingLanguageData;   // Func<LangType, Dictionary<string, string>, bool>: fill code→text for a language, return true on success
Localization.onChangeLanguage;        // Action<LangType>: notified after a successful switch (refresh UI here)

// State & queries
Localization.currentLanguage;                       // LangType (read-only)
Localization.systemLanguage;                        // OS language mapped to LangType
Localization.GetSystemLanguageToLangType();
Localization.GetSupportedLanguages();               // HashSet<LangType>
Localization.IsSupportedLanguage(langType);
Localization.GetAndCheckIsSupportedLanguage(langType); // falls back when unsupported
Localization.GetSupportedLanguagesMappingByLangType(); // Dictionary<LangType, string> (desc)
Localization.GetSupportedLanguagesMappingByLangDesc(); // Dictionary<string, LangType>

// Actions
Localization.ChangeLanguage(langType);   // parses table via onParsingLanguageData, then fires onChangeLanguage
Localization.GetStringByCode("ui.start"); // text for current language ("Unknown Text" when the code is missing)
```

`LangType` is a `byte` enum of world languages (`Unspecified`, `Arabic`, `ChineseSimplified`, `ChineseTraditional`, `Dutch`, `English`, `French`, `German`, `Italian`, `Portuguese`, `Spanish`, `Japanese`, `Korean`, `Russian`, ... — check `Languages.cs` for the full list). `LanguageMapping.GetLanguageDesc(langType)` returns a human-readable description.

## Standard setup flow

```csharp
using System.Collections.Generic;
using OxGKit.LocalizationSystem;

public static class LocalizationInitializer
{
    public static void Init()
    {
        // 1) Declare supported languages
        Localization.onAddSupportedLanguages = supported =>
        {
            supported.Add(LangType.English);
            supported.Add(LangType.ChineseTraditional);
            supported.Add(LangType.ChineseSimplified);
            supported.Add(LangType.Japanese);
        };

        // 2) Provide table parsing (from your own storage: JSON, GameDB, server, ...)
        Localization.onParsingLanguageData = (langType, langData) =>
        {
            var sheet = LoadYourLanguageSheet(); // Dictionary<code, Dictionary<langKey, text>>
            foreach (var row in sheet)
                langData.TryAdd(row.Key, row.Value.GetValueOrDefault(langType.ToString()));
            return true; // false / unassigned callback => ChangeLanguage throws
        };

        // 3) React to switches (refresh all visible localized UI)
        Localization.onChangeLanguage = langType => RefreshAllLocalizedViews();

        // 4) Restore persisted choice (fall back to system language)
        var saved = (LangType)PlayerPrefs.GetInt("language", (int)Localization.systemLanguage);
        Localization.ChangeLanguage(Localization.GetAndCheckIsSupportedLanguage(saved));
    }
}
```

UI reads text only through codes:

```csharp
this.titleText.text = Localization.GetStringByCode("ui.title");
```

Language menu switching + persistence (pair well with OxGKit.SaverSystem or your settings facade):

```csharp
Localization.ChangeLanguage(LangType.Japanese);
PlayerPrefs.SetInt("language", (int)Localization.currentLanguage);
```

## Rules & pitfalls

- Assign `onAddSupportedLanguages` and `onParsingLanguageData` **before** the first `ChangeLanguage`; an unassigned/failing parser makes `ChangeLanguage` throw.
- Load whatever data source the parser needs (game DB, downloaded tables) **before** initializing localization.
- `GetStringByCode` throws if no language table has been parsed yet, and returns `"Unknown Text"` for missing codes — treat that string in QA as "missing key".
- `ChangeLanguage` with an unsupported `LangType` only logs a warning and keeps the current language; use `GetAndCheckIsSupportedLanguage` to sanitize saved/system values first.
- `onChangeLanguage` handlers must be repeat-safe and only redraw text/data (no one-time event registration inside). A view that subscribes after the first `ChangeLanguage` already ran is not refreshed until the next change — draw it once yourself right after subscribing (the Localization Demo does this since v1.0.3).
- The module does not persist the selection — the project owns saving/restoring the chosen `LangType`.

## Verify

- Enter Play mode, switch through every supported language, and confirm all visible text refreshes and no `"Unknown Text"` shows up.
- Cold-start with a saved unsupported/legacy language value and confirm the fallback path works.
