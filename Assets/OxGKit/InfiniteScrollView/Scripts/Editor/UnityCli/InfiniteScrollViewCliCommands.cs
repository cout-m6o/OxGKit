using Unity.Pipeline.Commands;
using UnityEditor;

namespace OxGKit.InfiniteScrollView.Editor.UnityCli
{
    /// <summary>
    /// Unity CLI commands of OxGKit.InfiniteScrollView (unity command oxgkit.infinite_scroll_view ...).
    /// <para>Compiled only when the Unity Pipeline package (com.unity.pipeline) is installed.</para>
    /// </summary>
    internal static class InfiniteScrollViewCliCommands
    {
        private const string Group = "oxgkit.infinite_scroll_view";

        [CliGroupCommand(Group,
            "Create an infinite cell script (the <name>Data class and the <name> InfiniteCell view) from the OxGKit.InfiniteScrollView template; same content as the menu 'Assets/Create/OxGKit/Infinite ScrollView/Template Infinite Cell.cs (Script)'. " +
            "NOTE: the types do not exist until scripts recompile (script recompile, then poll script recompile_status).",
            Tags = new[] { "scripts" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode CreateCell(
            CommandContext cmd,
            [CliArg("name", "Class/file name without extension, e.g. ShopItemCell (the data class becomes ShopItemCellData). Must be a valid C# identifier.", Required = true)] string name,
            [CliArg("path", "Existing folder to write the .cs into (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            if (!CliHelper.TryResolveNewAssetPath(cmd, name, path, ".cs", true, out string assetPath, out string className, out ExitCode failure))
                return failure;

            assetPath = InfiniteScrollViewCreateScriptEditor.CreateScriptTplInfiniteCellAtPath(assetPath);

            cmd.Result.Write(new
            {
                assetPath,
                guid = AssetDatabase.AssetPathToGUID(assetPath),
                type = "MonoScript",
                className,
                dataClassName = className + "Data",
            });
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Create the empty infinite cell prefab (one GameObject with RectTransform + CanvasRenderer, anchors 0.5, size 128 x 128); same result as the menu 'Assets/Create/OxGKit/Infinite ScrollView/Template Infinite Cell (RectTransform Prefab)'. " +
            "Asset creation is not undoable via Ctrl+Z.",
            Tags = new[] { "prefabs" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode CreateCellPrefab(
            CommandContext cmd,
            [CliArg("name", "Prefab file name without extension, e.g. ShopItemCell.", Required = true)] string name,
            [CliArg("path", "Existing folder to write the .prefab into (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            if (!CliHelper.TryResolveNewAssetPath(cmd, name, path, ".prefab", false, out string assetPath, out string _, out ExitCode failure))
                return failure;

            assetPath = InfiniteScrollViewCreatePrefabEditor.CreateTplRectTransformCPAtPath(assetPath);

            cmd.Result.Write(new
            {
                assetPath,
                guid = AssetDatabase.AssetPathToGUID(assetPath),
                type = "GameObject",
            });
            return ExitCode.Success;
        }
    }
}
