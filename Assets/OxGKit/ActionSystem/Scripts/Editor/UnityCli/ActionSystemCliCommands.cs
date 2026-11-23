using Unity.Pipeline.Commands;
using UnityEditor;

namespace OxGKit.ActionSystem.Editor.UnityCli
{
    /// <summary>
    /// Unity CLI commands of OxGKit.ActionSystem (unity command oxgkit.action ...).
    /// <para>Compiled only when the Unity Pipeline package (com.unity.pipeline) is installed.</para>
    /// </summary>
    internal static class ActionSystemCliCommands
    {
        private const string Group = "oxgkit.action";

        [CliGroupCommand(Group,
            "Create a custom action script (ActionBase subclass) from the OxGKit.ActionSystem template; same content as the menu 'Assets/Create/OxGKit/Action System/Template Action.cs'. " +
            "NOTE: the type does not exist until scripts recompile (script recompile, then poll script recompile_status).",
            Tags = new[] { "scripts" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode Create(
            CommandContext cmd,
            [CliArg("name", "Class/file name without extension, e.g. MoveAction. Must be a valid C# identifier.", Required = true)] string name,
            [CliArg("path", "Existing folder to write the .cs into (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            if (!CliHelper.TryResolveNewAssetPath(cmd, name, path, ".cs", true, out string assetPath, out string className, out ExitCode failure))
                return failure;

            assetPath = ActionSystemCreateScriptEditor.CreateScriptTplActionAtPath(assetPath);

            cmd.Result.Write(new
            {
                assetPath,
                guid = AssetDatabase.AssetPathToGUID(assetPath),
                type = "MonoScript",
                className,
            });
            return ExitCode.Success;
        }
    }
}
