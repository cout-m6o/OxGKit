using Unity.Pipeline.Commands;
using UnityEditor;

namespace OxGKit.SingletonSystem.Editor.UnityCli
{
    /// <summary>
    /// Unity CLI commands of OxGKit.SingletonSystem (unity command oxgkit.singleton ...).
    /// <para>Compiled only when the Unity Pipeline package (com.unity.pipeline) is installed.</para>
    /// </summary>
    internal static class SingletonSystemCliCommands
    {
        private const string Group = "oxgkit.singleton";

        [CliGroupCommand(Group,
            "Create a MonoSingleton<T> script from the OxGKit.SingletonSystem template; same content as the menu 'Assets/Create/OxGKit/SingletonSystem/Template Mono Singleton.cs'. " +
            "NOTE: the type does not exist until scripts recompile (script recompile, then poll script recompile_status).",
            Tags = new[] { "scripts" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode CreateMonoSingleton(
            CommandContext cmd,
            [CliArg("name", "Class/file name without extension, e.g. GameSystem. Must be a valid C# identifier.", Required = true)] string name,
            [CliArg("path", "Existing folder to write the .cs into (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            if (!CliHelper.TryResolveNewAssetPath(cmd, name, path, ".cs", true, out string assetPath, out string className, out ExitCode failure))
                return failure;

            assetPath = SingletonCreateScriptEditor.CreateScriptTplMonoSingletonAtPath(assetPath);

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
