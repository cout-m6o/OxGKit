using Unity.Pipeline.Commands;
using UnityEditor;

namespace OxGKit.InputSystem.Editor.UnityCli
{
    /// <summary>
    /// Unity CLI commands of OxGKit.InputSystem (unity command oxgkit.input ...).
    /// <para>Compiled only when the Unity Pipeline package (com.unity.pipeline) is installed.</para>
    /// </summary>
    internal static class InputSystemCliCommands
    {
        private const string Group = "oxgkit.input";

        [CliGroupCommand(Group,
            "Create an IInputAction script (game-level input signal dispatcher) from the OxGKit.InputSystem template; same content as the menu 'Assets/Create/OxGKit/Input System/Template Input Action.cs (Input Interface For Any)'. " +
            "NOTE: the type does not exist until scripts recompile (script recompile, then poll script recompile_status).",
            Tags = new[] { "scripts" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode CreateAction(
            CommandContext cmd,
            [CliArg("name", "Class/file name without extension, e.g. PlayerAction. Must be a valid C# identifier.", Required = true)] string name,
            [CliArg("path", "Existing folder to write the .cs into (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            if (!CliHelper.TryResolveNewAssetPath(cmd, name, path, ".cs", true, out string assetPath, out string className, out ExitCode failure))
                return failure;

            assetPath = InputSystemCreateScriptEditor.CreateScriptTplInputActionAtPath(assetPath);

            cmd.Result.Write(new
            {
                assetPath,
                guid = AssetDatabase.AssetPathToGUID(assetPath),
                type = "MonoScript",
                className,
            });
            return ExitCode.Success;
        }

        [CliGroupCommand(Group,
            "Create a Binding Composite script for Unity New Input System from the OxGKit.InputSystem template; same content as the menu 'Assets/Create/OxGKit/Input System/New Input System (Extension)/Template Input Binding Composite.cs (For Unity New Input System)'. " +
            "The name is the value struct: <name>.cs declares the class <name>Composite and its nested struct <name>. " +
            "NOTE: the types do not exist until scripts recompile (script recompile, then poll script recompile_status).",
            Tags = new[] { "scripts" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode CreateBindingComposite(
            CommandContext cmd,
            [CliArg("name", "File name without extension and name of the value struct, e.g. MoveInput (the class becomes MoveInputComposite). Must be a valid C# identifier.", Required = true)] string name,
            [CliArg("path", "Existing folder to write the .cs into (relative to the authoring root; the Assets/ prefix is optional). Defaults to the authoring root.")] string path = null)
        {
            if (!CliHelper.TryResolveNewAssetPath(cmd, name, path, ".cs", true, out string assetPath, out string valueName, out ExitCode failure))
                return failure;

            assetPath = InputSystemCreateScriptEditor.CreateScriptTplInputBindingCompositeAtPath(assetPath);

            cmd.Result.Write(new
            {
                assetPath,
                guid = AssetDatabase.AssetPathToGUID(assetPath),
                type = "MonoScript",
                className = valueName + "Composite",
                valueStructName = valueName,
            });
            return ExitCode.Success;
        }
    }
}
