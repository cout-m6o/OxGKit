using System;
using System.Collections.Generic;
using OxGKit.Utilities.MissingFinder.Editor;
using OxGKit.Utilities.RectTrans.Editor;
using Unity.Pipeline.Commands;
using Unity.Pipeline.Editor.Authoring;
using Unity.Pipeline.Models;
using UnityEditor;
using UnityEngine;

namespace OxGKit.Utilities.Editor.UnityCli
{
    /// <summary>
    /// Where the missing scripts finder searches
    /// </summary>
    internal enum MissingScriptsScope
    {
        LoadedScenes,
        BuildScenes,
        BuildScenesIncludeDisabled,
        AllScenes,
        AllPrefabs
    }

    /// <summary>
    /// Unity CLI commands of OxGKit.Utilities (unity command oxgkit.utilities ...).
    /// <para>Compiled only when the Unity Pipeline package (com.unity.pipeline) is installed.</para>
    /// </summary>
    internal static class UtilitiesCliCommands
    {
        private const string Group = "oxgkit.utilities";

        private const string _MISSING_SCRIPT_LOG_PREFIX = "Missing script: ";
        private const string _MISSING_SCRIPT_LOG_SEPARATOR = " --> ";

        [CliGroupCommand(Group,
            "Search for missing scripts with the OxGKit MissingScriptsFinder and return one record per hit; same search as the menus 'MissingScriptsFinder/Search in ...'. " +
            "Scenes that are not loaded are opened additively and closed again. The hits are also logged to the Unity console.",
            Tags = new[] { "observability" },
            Effect = CommandEffect.ReadOnly)]
        internal static ExitCode FindMissingScripts(
            CommandContext cmd,
            [CliArg("scope", "LoadedScenes (default), BuildScenes (enabled scenes of the build settings), BuildScenesIncludeDisabled, AllScenes (every scene of the project) or AllPrefabs (every prefab under Assets/).")] MissingScriptsScope scope = MissingScriptsScope.LoadedScenes,
            [CliArg("timeout", "Timeout in seconds.")] double timeoutInSeconds = 600)
        {
            // The finder reports through the Unity console, collect its report lines while it runs
            var hits = new List<string>();
            Application.LogCallback collector = (condition, stackTrace, type) =>
            {
                if (type == LogType.Log && condition != null && condition.StartsWith(_MISSING_SCRIPT_LOG_PREFIX, StringComparison.Ordinal))
                    hits.Add(condition.Substring(_MISSING_SCRIPT_LOG_PREFIX.Length));
            };

            Application.logMessageReceived += collector;
            try
            {
                switch (scope)
                {
                    case MissingScriptsScope.BuildScenes:
                        MissingScriptsFinder.FindMissingScriptsInBuildSettingsScenes();
                        break;
                    case MissingScriptsScope.BuildScenesIncludeDisabled:
                        MissingScriptsFinder.FindMissingScriptsInBuildSettingsScenesIncludeDisable();
                        break;
                    case MissingScriptsScope.AllScenes:
                        MissingScriptsFinder.FindMissingScriptsInProjectAllScenes();
                        break;
                    case MissingScriptsScope.AllPrefabs:
                        MissingScriptsFinder.FindMissingScriptsInAllPrefabs();
                        break;
                    default:
                        MissingScriptsFinder.FindMissingScriptsInLoadedScenes();
                        break;
                }
            }
            finally
            {
                Application.logMessageReceived -= collector;
            }

            foreach (string hit in hits)
            {
                // "<object path> --> <scene or prefab path>", loaded scene objects have no asset part
                int separator = hit.LastIndexOf(_MISSING_SCRIPT_LOG_SEPARATOR, StringComparison.Ordinal);
                string objectPath = separator >= 0 ? hit.Substring(0, separator) : hit;
                string assetPath = separator >= 0 ? hit.Substring(separator + _MISSING_SCRIPT_LOG_SEPARATOR.Length) : null;
                cmd.Result.WriteRecord(new { objectPath, assetPath });
            }

            cmd.Log.Info($"Found {hits.Count} missing script(s) in {scope}.");
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Move the anchors of a RectTransform to its current corners and zero its offsets, keeping its rect; same result as the menu 'GameObject/Adjust RectTransform Anchors (Shift+R)' on that object. Undo-able.",
            Tags = new[] { "gameobjects" },
            Effect = CommandEffect.Idempotent)]
        internal static ExitCode AdjustAnchors(
            CommandContext cmd,
            [CliArg("target", "Handle of the GameObject with the RectTransform (globalId/instanceId/hierarchyPath).", Required = true)] ObjectRef target)
        {
            if (!ObjectResolver.TryResolve(target, out UnityEngine.Object resolved, out string error))
            {
                cmd.Log.Error(error);
                return ExitCode.NotFound;
            }

            GameObject gameObject = resolved as GameObject ?? (resolved as Component)?.gameObject;
            RectTransform rectTransform = gameObject != null ? gameObject.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                cmd.Log.Error($"'{target}' has no RectTransform.");
                return ExitCode.InvalidUsage;
            }

            if (rectTransform.parent == null)
            {
                cmd.Log.Error($"'{target}' has no parent, its anchors cannot be adjusted.");
                return ExitCode.InvalidUsage;
            }

            // The tool works on the selection, the way a developer uses it; put the selection back afterwards
            UnityEngine.Object[] previousSelection = Selection.objects;
            try
            {
                Selection.objects = new UnityEngine.Object[] { gameObject };
                RectTransformAdjuster.AdjustAnchors();
            }
            finally
            {
                Selection.objects = previousSelection;
            }

            cmd.Result.Write(new
            {
                target = ObjectResolver.Describe(gameObject),
                anchorMin = new { x = rectTransform.anchorMin.x, y = rectTransform.anchorMin.y },
                anchorMax = new { x = rectTransform.anchorMax.x, y = rectTransform.anchorMax.y },
                offsetMin = new { x = rectTransform.offsetMin.x, y = rectTransform.offsetMin.y },
                offsetMax = new { x = rectTransform.offsetMax.x, y = rectTransform.offsetMax.y },
            });
            return ExitCode.Success;
        }
    }
}
