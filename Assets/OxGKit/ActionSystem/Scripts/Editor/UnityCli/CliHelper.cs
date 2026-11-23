using System;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Pipeline.Commands;
using Unity.Pipeline.Editor.Authoring;
using UnityEditor;

namespace OxGKit.ActionSystem.Editor.UnityCli
{
    /// <summary>
    /// Shared helpers of the Unity CLI commands in this assembly
    /// </summary>
    internal static class CliHelper
    {
        /// <summary>
        /// Log the reason and return the exit code
        /// </summary>
        internal static ExitCode Fail(CommandContext cmd, ExitCode code, string error)
        {
            cmd.Log.Error(error);
            return code;
        }

        /// <summary>
        /// Resolve the "name" and "path" (folder) arguments into the asset path of a new file.
        /// The folder goes through the path sandbox of the Unity Pipeline package (authoring root), it must exist and the file must not exist yet.
        /// </summary>
        /// <param name="cmd">Command context</param>
        /// <param name="name">File name, the extension is optional</param>
        /// <param name="path">Folder, null or empty for the authoring root</param>
        /// <param name="extension">Extension including the dot, e.g. ".cs"</param>
        /// <param name="identifierRequired">Whether the file name must be a valid C# identifier</param>
        /// <param name="assetPath">The resolved asset path</param>
        /// <param name="fileName">The file name without extension</param>
        /// <param name="failure">The exit code when the method returns false</param>
        internal static bool TryResolveNewAssetPath(CommandContext cmd, string name, string path, string extension, bool identifierRequired, out string assetPath, out string fileName, out ExitCode failure)
        {
            assetPath = null;
            fileName = null;
            failure = ExitCode.Success;

            if (string.IsNullOrWhiteSpace(name))
            {
                failure = Fail(cmd, ExitCode.InvalidUsage, "'name' is required.");
                return false;
            }

            fileName = name.Trim();
            if (fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                fileName = fileName.Substring(0, fileName.Length - extension.Length);

            if (identifierRequired)
            {
                if (!Regex.IsMatch(fileName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
                {
                    failure = Fail(cmd, ExitCode.InvalidUsage, $"'{fileName}' is not a valid C# class identifier.");
                    return false;
                }
            }
            else if (fileName.Length == 0 || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                failure = Fail(cmd, ExitCode.InvalidUsage, $"'{fileName}' is not a valid file name.");
                return false;
            }

            // A null/empty path means the authoring root itself (already a confined, valid path)
            string folder;
            if (string.IsNullOrWhiteSpace(path))
            {
                folder = ProjectPaths.AuthoringRoot;
            }
            else
            {
                folder = ProjectPaths.Resolve(path, out string error);
                if (folder == null)
                {
                    failure = Fail(cmd, ExitCode.InvalidUsage, error);
                    return false;
                }
            }

            if (!AssetDatabase.IsValidFolder(folder))
            {
                failure = Fail(cmd, ExitCode.NotFound, $"Destination folder '{folder}' does not exist. Create it first with asset create_folder.");
                return false;
            }

            assetPath = $"{folder}/{fileName}{extension}";
            if (File.Exists(Path.Combine(ProjectPaths.ProjectRoot, assetPath)))
            {
                failure = Fail(cmd, ExitCode.Failed, $"A file already exists at '{assetPath}'. Existing files are never overwritten.");
                return false;
            }

            return true;
        }
    }
}
