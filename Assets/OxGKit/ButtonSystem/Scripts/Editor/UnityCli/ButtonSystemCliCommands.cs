using Unity.Pipeline.Commands;
using Unity.Pipeline.Editor.Authoring;
using Unity.Pipeline.Models;
using UnityEditor;
using UnityEngine;

namespace OxGKit.ButtonSystem.Editor.UnityCli
{
    /// <summary>
    /// Label type of the ButtonPlus created by the Unity CLI command
    /// </summary>
    internal enum ButtonPlusLabel
    {
        TextMeshPro,
        Legacy
    }

    /// <summary>
    /// Unity CLI commands of OxGKit.ButtonSystem (unity command oxgkit.button ...).
    /// <para>Compiled only when the Unity Pipeline package (com.unity.pipeline) is installed.</para>
    /// </summary>
    internal static class ButtonSystemCliCommands
    {
        private const string Group = "oxgkit.button";

        [CliGroupCommand(Group,
            "Create a ButtonPlus UI object (Image + ButtonPlus with a child label, 160 x 30); same result as the menus 'GameObject/UI/Button Plus - TextMeshPro' and 'GameObject/UI/Legacy/Button Plus'. " +
            "Without a parent it goes under a Canvas of the current stage (the selected one first); a Canvas and an EventSystem are created when none exists. Undo-able.",
            Tags = new[] { "gameobjects" },
            Effect = CommandEffect.Additive)]
        internal static ExitCode Create(
            CommandContext cmd,
            [CliArg("label", "Text component of the label: TextMeshPro (default) or Legacy.")] ButtonPlusLabel label = ButtonPlusLabel.TextMeshPro,
            [CliArg("parent", "Optional parent handle (globalId/instanceId/hierarchyPath). The button becomes a child of it.")] ObjectRef parent = null)
        {
            // Resolve before creating, so a bad handle creates nothing
            GameObject parentObject = null;
            if (parent != null)
            {
                if (!ObjectResolver.TryResolve(parent, out Object resolved, out string error))
                {
                    cmd.Log.Error(error);
                    return ExitCode.NotFound;
                }

                parentObject = resolved as GameObject ?? (resolved as Component)?.gameObject;
                if (parentObject == null)
                {
                    cmd.Log.Error($"'{parent}' is not a GameObject.");
                    return ExitCode.InvalidUsage;
                }
            }

            // Same entry points as the menu items (a MenuCommand without context behaves like the main menu)
            var menuCommand = new MenuCommand(parentObject);
            if (label == ButtonPlusLabel.Legacy)
                ButtonPlusGeneratorMenu.AddButtonLegacy(menuCommand);
            else
                ButtonPlusGeneratorMenu.AddButtonTMP(menuCommand);

            // The generator selects the object it created
            GameObject created = Selection.activeGameObject;
            if (created == null || created.GetComponent<ButtonPlus>() == null)
            {
                cmd.Log.Error("The ButtonPlus object was not created.");
                return ExitCode.Failed;
            }

            cmd.Result.Write(ObjectResolver.Describe(created));
            return ExitCode.Success;
        }
    }
}
