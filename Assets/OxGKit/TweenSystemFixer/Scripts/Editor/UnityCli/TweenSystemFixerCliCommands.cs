using Unity.Pipeline.Commands;
using UnityEditor;

namespace OxGKit.TweenSystemFixer.Editor.UnityCli
{
    /// <summary>
    /// Unity CLI commands of OxGKit.TweenSystemFixer (unity command oxgkit.tween ...).
    /// <para>Compiled only when the Unity Pipeline package (com.unity.pipeline) is installed.</para>
    /// </summary>
    internal static class TweenSystemFixerCliCommands
    {
        private const string Group = "oxgkit.tween";

        [CliGroupCommand(Group,
            "Reassign the GUID of DOTween.Modules.asmdef (anywhere under Assets/) to the fixed GUID that OxGKit.TweenSystem references, and update the assets that reference the old GUID; same repair as the menu 'Assets/OxGKit TweenSystem GUID Fixer/Search And Reassign DOTween.Modules GUID', without the selection and the dialog. " +
            "Does nothing when the GUID is already the fixed one. Destructive: a change requires confirm=true (use dry-run to preview). Not undoable via Ctrl+Z.",
            Tags = new[] { "assets" },
            Effect = CommandEffect.Destructive)]
        internal static ExitCode RepairDotweenModulesGuid(
            CommandContext cmd,
            [CliArg("confirm", "Apply the GUID change. Without it the call is refused when a change is needed.")] bool confirm = false,
            [CliArg("dry-run", "Preview what would change without changing it.")] bool dryRun = false)
        {
            string assetPath = DoTweenAssemblyFixer.FindDoTweenModulesAssetPath();
            if (assetPath == null)
            {
                cmd.Log.Error("DOTween.Modules.asmdef was not found under Assets/. Import DOTween and create its assembly definitions first (Tools > Demigiant > DOTween Utility Panel > Create ASMDEF).");
                return ExitCode.NotFound;
            }

            string fixedGuid = DoTweenAssemblyFixer.doTweenModulesFixedGUID;
            string currentGuid = AssetDatabase.AssetPathToGUID(assetPath);
            bool changeNeeded = currentGuid != fixedGuid;

            if (dryRun)
            {
                cmd.Result.Write(new { status = "dry_run", assetPath, guid = currentGuid, fixedGuid, wouldChange = changeNeeded });
                return ExitCode.Success;
            }

            if (!changeNeeded)
            {
                cmd.Result.Write(new { assetPath, guid = currentGuid, fixedGuid, changed = false });
                return ExitCode.Success;
            }

            if (!confirm)
            {
                cmd.Log.Error($"Refusing to reassign the GUID of '{assetPath}' ({currentGuid} -> {fixedGuid}). Pass confirm=true to apply, or dry-run=true to preview.");
                return ExitCode.InvalidUsage;
            }

            DoTweenAssemblyFixer.RepairDoTweenModulesGUIDInProject();

            string newGuid = AssetDatabase.AssetPathToGUID(assetPath);
            if (newGuid != fixedGuid)
            {
                cmd.Log.Error($"The GUID of '{assetPath}' is still {newGuid}; see the Unity console for the reason.");
                return ExitCode.Failed;
            }

            cmd.Result.Write(new { assetPath, guid = newGuid, fixedGuid, previousGuid = currentGuid, changed = true });
            return ExitCode.Success;
        }
    }
}
