using Unity.Pipeline.Commands;
using UnityEditor;

namespace OxGKit.NoticeSystem.Editor.UnityCli
{
    /// <summary>
    /// Unity CLI commands of OxGKit.NoticeSystem (unity command oxgkit.notice ...).
    /// <para>Compiled only when the Unity Pipeline package (com.unity.pipeline) is installed.</para>
    /// </summary>
    internal static class NoticeSystemCliCommands
    {
        private const string Group = "oxgkit.notice";

        [CliGroupCommand(Group,
            "Create a NoticeCondition script that a register class registers, from the OxGKit.NoticeSystem template; same content as the menu 'Assets/Create/OxGKit/Notice System/Template Notice Condition.cs (Manually)'. " +
            "NOTE: the type does not exist until scripts recompile (script recompile, then poll script recompile_status).",
            Tags = new[] { "scripts" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode CreateCondition(
            CommandContext cmd,
            [CliArg("name", "Class/file name without extension, e.g. NewMailCond. Must be a valid C# identifier.", Required = true)] string name,
            [CliArg("path", "Existing folder to write the .cs into (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            if (!CliHelper.TryResolveNewAssetPath(cmd, name, path, ".cs", true, out string assetPath, out string className, out ExitCode failure))
                return failure;

            assetPath = NoticeSystemCreateScriptEditor.CreateScriptTplNoticeConditionAtPath(assetPath);

            _WriteResult(cmd, assetPath, className);
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Create a NoticeCondition script that registers itself before the first scene loads, from the OxGKit.NoticeSystem template; same content as the menu 'Assets/Create/OxGKit/Notice System/Template Notice Condition.cs (RuntimeInitializeLoadType.BeforeSceneLoad)'. " +
            "NOTE: the type does not exist until scripts recompile (script recompile, then poll script recompile_status).",
            Tags = new[] { "scripts" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode CreateConditionBeforeSceneLoad(
            CommandContext cmd,
            [CliArg("name", "Class/file name without extension, e.g. NewMailCond. Must be a valid C# identifier.", Required = true)] string name,
            [CliArg("path", "Existing folder to write the .cs into (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            if (!CliHelper.TryResolveNewAssetPath(cmd, name, path, ".cs", true, out string assetPath, out string className, out ExitCode failure))
                return failure;

            assetPath = NoticeSystemCreateScriptEditor.CreateScriptTplNoticeConditionWithBeforeSceneLoadAtPath(assetPath);

            _WriteResult(cmd, assetPath, className);
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Create the static class that registers the manually registered notice conditions, from the OxGKit.NoticeSystem template; same content as the menu 'Assets/Create/OxGKit/Notice System/Template Notice Condition Registers.cs (Manually)'. " +
            "NOTE: the type does not exist until scripts recompile (script recompile, then poll script recompile_status).",
            Tags = new[] { "scripts" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode CreateConditionRegisters(
            CommandContext cmd,
            [CliArg("name", "Class/file name without extension, e.g. NoticeConditionRegister. Must be a valid C# identifier.", Required = true)] string name,
            [CliArg("path", "Existing folder to write the .cs into (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            if (!CliHelper.TryResolveNewAssetPath(cmd, name, path, ".cs", true, out string assetPath, out string className, out ExitCode failure))
                return failure;

            assetPath = NoticeSystemCreateScriptEditor.CreateScriptTplNoticeConditionRegistersAtPath(assetPath);

            _WriteResult(cmd, assetPath, className);
            return ExitCode.Success;
        }

        private static void _WriteResult(CommandContext cmd, string assetPath, string className)
        {
            cmd.Result.Write(new
            {
                assetPath,
                guid = AssetDatabase.AssetPathToGUID(assetPath),
                type = "MonoScript",
                className,
            });
        }
    }
}
